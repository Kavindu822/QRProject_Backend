using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
                Name = registerDto.Name,
                Phone = registerDto.Phone,
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
            // Creating the claims for the JWT token
            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.Name, user.UserName),
        new Claim("EpfNo", user.EpfNo),
        new Claim("IsApproved", user.IsApproved.ToString()),
        new Claim("role", user.Role)
        //new Claim(ClaimTypes.Role, user.Role)
    };

            // Getting the secret key from the configuration (appsettings.json or environment variables)
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]));

            // Creating the credentials using the secret key
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Creating the JWT token
            var token = new JwtSecurityToken(
                config["Jwt:Issuer"],   // JWT Issuer
                config["Jwt:Audience"], // JWT Audience
                claims,                 // Claims
                expires: DateTime.UtcNow.AddHours(1), // Token expiry
                signingCredentials: creds // Signing credentials
            );

            // Returning the JWT token as a string
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

        [HttpGet("all/{role?}")] // Role is optional
        public async Task<IActionResult> GetAllUsers(string? role = null)
        {
            var users = userManager.Users.AsQueryable();

            if (!string.IsNullOrEmpty(role))
            {
                users = users.Where(u => u.Role == role);
            }

            return Ok(await users.ToListAsync());
        }



        //[Authorize]
        [HttpGet("{epfNo}")]
        public async Task<IActionResult> GetUserByEpfNo(string epfNo)
        {
            var user = await userManager.FindByNameAsync(epfNo);
            if (user == null)
                return NotFound(new { message = $"User with EPF No '{epfNo}' not found." });

            return Ok(user);
        }


    }
}
