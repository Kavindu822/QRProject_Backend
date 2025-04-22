using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using QRFileTrackingapi.Models.DTOs;
using QRFileTrackingapi.Models.Entities;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace QRFileTrackingapi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserAccountsController : ControllerBase
    {
        private readonly UserManager<UserAccount> userManager;
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly ILogger<UserAccountsController> logger;
        private readonly IConfiguration config;

        public UserAccountsController(
            UserManager<UserAccount> userManager,
            RoleManager<IdentityRole> roleManager,
            ILogger<UserAccountsController> logger,
            IConfiguration config)
        {
            this.userManager = userManager;
            this.roleManager = roleManager;
            this.logger = logger;
            this.config = config;
        }

        // ✅ REGISTER USER
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            logger.LogInformation("User registration attempt - EPF No: {EpfNo}, Role: {Role}", registerDto.EpfNo, registerDto.Role);

            if (registerDto.Role != "Admin" && registerDto.Role != "Employee")
                return BadRequest(new { message = "Invalid role. Must be 'Admin' or 'Employee'." });

            var existingUser = await userManager.FindByNameAsync(registerDto.EpfNo);
            if (existingUser != null)
                return BadRequest(new { message = "User already registered." });

            var user = new UserAccount
            {
                UserName = registerDto.EpfNo,
                EpfNo = registerDto.EpfNo,
                EName = registerDto.EName,
                ContactNo = registerDto.ContactNo,
                Department = registerDto.Department,
                SeatNo = registerDto.SeatNo,
                Role = registerDto.Role,
                IsApproved = false // Needs admin approval
            };

            var result = await userManager.CreateAsync(user, registerDto.Password);
            if (!result.Succeeded)
                return BadRequest(new { message = string.Join(", ", result.Errors.Select(e => e.Description)) });

            await userManager.AddToRoleAsync(user, registerDto.Role);
            logger.LogInformation("User registered successfully - EPF No: {EpfNo}", registerDto.EpfNo);
            return Ok(new { message = "Registered successfully. Awaiting admin approval." });
        }


        // ✅ LOGIN
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            var user = await userManager.FindByNameAsync(loginDto.EpfNo);
            if (user == null || !await userManager.CheckPasswordAsync(user, loginDto.Password))
                return Unauthorized(new { message = "Invalid credentials." });

            if (!user.IsApproved)
                return Unauthorized(new { message = "Your account is not approved yet." });

            var token = GenerateJwtToken(user);
            return Ok(new { Token = token, UserAccount = user });
        }

        private string GenerateJwtToken(UserAccount user)
        {
            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.Name, user.UserName),
        new Claim("EpfNo", user.EpfNo),
        new Claim("IsApproved", user.IsApproved.ToString()),
        new Claim(ClaimTypes.Role, user.Role),
        new Claim("ContactNo", user.ContactNo),  // Add contactNo as a claim
        new Claim("EName", user.EName)  // Add eName as a claim
    };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: config["Jwt:Issuer"],
                audience: config["Jwt:Audience"],
                claims: claims,  // Include all the claims
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }



        // ✅ APPROVE USER (Admin Only)
        [Authorize(Roles = "Admin")]
        [HttpPut("approve/{epfNo}")]
        public async Task<IActionResult> ApproveUser(string epfNo)
        {
            var user = await userManager.FindByNameAsync(epfNo);
            if (user == null) return NotFound(new { message = $"User with EPF No '{epfNo}' not found." });

            user.IsApproved = true;
            await userManager.UpdateAsync(user);
            return Ok(new { message = "User approved successfully." });
        }

        // ✅ DELETE USER (Admin Only)
        [Authorize(Roles = "Admin")]
        [HttpDelete("delete/{epfNo}")]
        public async Task<IActionResult> DeleteUser(string epfNo)
        {
            var user = await userManager.FindByNameAsync(epfNo);
            if (user == null) return NotFound(new { message = "User not found." });

            await userManager.DeleteAsync(user);
            return Ok(new { message = "User deleted successfully." });
        }

        // ✅ GET APPROVED EMPLOYEES BY DEPARTMENT (Admin Only)
        //[Authorize(Roles = "Admin")]
        [HttpGet("employees-by-department/{department}")]
        public IActionResult GetEmployeesByDepartment(string department)
        {
            var employees = userManager.Users
                .Where(u => u.Role == "Employee" && u.Department == department && u.IsApproved == true)
                .Select(u => new
                {
                    u.EpfNo,
                    u.EName,
                    u.ContactNo,
                    u.Department,
                    u.SeatNo,
                    u.IsApproved
                })
                .ToList();

            if (!employees.Any())
                return NotFound(new { message = $"No approved employees found in department '{department}'." });

            return Ok(employees);
        }

        //[Authorize(Roles = "Admin")]
        [HttpGet("approved-employees")]
        public IActionResult GetAllApprovedEmployees()
        {
            var employees = userManager.Users
                .Where(u => u.Role == "Employee" && u.IsApproved == true)
                .Select(u => new
                {
                    u.EpfNo,
                    u.EName,
                    u.ContactNo,
                    u.Department,
                    u.SeatNo,
                    u.IsApproved
                })
                .ToList();

            if (!employees.Any())
                return NotFound(new { message = "No approved employees found." });

            return Ok(employees);
        }




        // ✅ GET APPROVED ADMINS ONLY (Admin Only)
        [Authorize(Roles = "Admin")]
        [HttpGet("admins")]
        public IActionResult GetApprovedAdmins()
        {
            var admins = userManager.Users
                .Where(u => u.Role == "Admin" && u.IsApproved == true)  // Compare with true
                .Select(u => new
                {
                    u.EpfNo,
                    u.EName,
                    u.ContactNo,
                    u.Department,
                    u.SeatNo,
                    u.IsApproved
                })
                .ToList();

            if (!admins.Any())
                return NotFound(new { message = "No approved admins found." });

            return Ok(admins);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("not-approved-users")]
        public IActionResult GetNotApprovedUsers()
        {
            var notApprovedUsers = userManager.Users
                .Where(u => u.IsApproved == false)
                .Select(u => new
                {
                    u.EName,
                    u.EpfNo,
                    u.Role,
                    u.ContactNo,
                    u.Department
                })
                .ToList();

            if (!notApprovedUsers.Any())
                return NotFound(new { message = "No unapproved users found." });

            return Ok(notApprovedUsers);
        }

        //[Authorize(Roles = "Admin")]
        [HttpPost("admin-reset-password")]
        public async Task<IActionResult> AdminResetPassword([FromBody] AdminResetPasswordDto dto)
        {
            var user = await userManager.FindByNameAsync(dto.EpfNo);
            if (user == null)
                return NotFound(new { message = "User not found." });

            var token = await userManager.GeneratePasswordResetTokenAsync(user);
            var result = await userManager.ResetPasswordAsync(user, token, dto.TemporaryPassword);
            if (!result.Succeeded)
                return BadRequest(new { message = "Failed to reset password.", errors = result.Errors });

            return Ok(new { message = "Temporary password set. Tell the employee to log in with it and reset." });
        }

        public class AdminResetPasswordDto
        {
            public string EpfNo { get; set; }
            public string TemporaryPassword { get; set; }
        }

        [HttpPost("set-new-password-after-temp")]
        public async Task<IActionResult> SetNewPassword([FromBody] ResetPasswordWithTempDto dto)
        {
            var user = await userManager.FindByNameAsync(dto.EpfNo);
            if (user == null)
                return NotFound(new { message = "User not found." });

            // First, validate the temporary password
            var isTempPasswordValid = await userManager.CheckPasswordAsync(user, dto.TemporaryPassword);
            if (!isTempPasswordValid)
                return Unauthorized(new { message = "Temporary password is incorrect." });

            if (dto.NewPassword != dto.ConfirmPassword)
                return BadRequest(new { message = "New password and confirm password do not match." });

            // Reset the password properly
            var token = await userManager.GeneratePasswordResetTokenAsync(user);
            var result = await userManager.ResetPasswordAsync(user, token, dto.NewPassword);

            if (!result.Succeeded)
                return BadRequest(new { message = "Failed to reset password.", errors = result.Errors });

            return Ok(new { message = "Password changed successfully. You can now log in with your new password." });
        }

        public class ResetPasswordWithTempDto
        {
            public string EpfNo { get; set; }
            public string TemporaryPassword { get; set; }
            public string NewPassword { get; set; }
            public string ConfirmPassword { get; set; }
        }



        [Authorize]
        [HttpGet("eligible-seat-transfer-users")]
        public async Task<IActionResult> GetEligibleSeatTransferUsers()
        {
            var currentUserEpfNo = User.FindFirstValue("EpfNo");
            if (string.IsNullOrEmpty(currentUserEpfNo))
                return Unauthorized(new { message = "Unable to identify the logged-in user." });

            var currentUser = await userManager.FindByNameAsync(currentUserEpfNo);
            if (currentUser == null || !currentUser.IsApproved)
                return Unauthorized(new { message = "Your account is not approved or does not exist." });

            var eligibleUsers = userManager.Users
                .Where(u =>
                    u.IsApproved == true &&
                    u.Role == "Employee" &&
                    u.EpfNo != currentUser.EpfNo &&
                    u.Department == currentUser.Department &&
                    u.SeatNo == currentUser.SeatNo)
                .Select(u => new
                {
                    u.EpfNo,
                    u.EName,
                    u.ContactNo,
                })
                .ToList();

            //// Add the "Library" manually
            //eligibleUsers.Add(new
            //{
            //    EpfNo = "Library",
            //    EName = "Library",
            //    ContactNo = "N/A"
            //});

            return Ok(eligibleUsers);
        }




    }
}