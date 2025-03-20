using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QRCoder;
using QRFileTrackingapi.Data;
using QRFileTrackingapi.Models;
using QRFileTrackingapi.Models.Entities;
using System;
using System.Threading.Tasks;

namespace QRFileTrackingapi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RcodeFilesController : ControllerBase
    {
        private readonly ApplicationDbContext dbContext;

        public RcodeFilesController(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        // Get all files
        [HttpGet]
        public async Task<IActionResult> GetAllFiles()
        {
            var allFiles = await dbContext.RcodeFiles.ToListAsync();
            return Ok(allFiles);
        }

        // Get file by Rcode
        [HttpGet("{rCode}")]
        public async Task<IActionResult> GetFileById(string rCode)
        {
            var file = await dbContext.RcodeFiles.SingleOrDefaultAsync(f => f.Rcode == rCode);
            if (file == null)
            {
                return NotFound(new { message = "File not found!" });
            }
            return Ok(file);
        }

        [Authorize]
        [HttpGet("my-files")]
        public async Task<IActionResult> GetFilesForLoggedInUser()
        {
            // Get the EPF No of the logged-in user from JWT claims
            var epfNo = User.Claims.FirstOrDefault(c => c.Type == "EpfNo")?.Value;

            if (string.IsNullOrEmpty(epfNo))
            {
                return Unauthorized(new { message = "Invalid user or missing EPF No in token." });
            }

            // Fetch files that belong to the logged-in user
            var userFiles = await dbContext.RcodeFiles.Where(f => f.EpfNo == epfNo).ToListAsync();

            return Ok(userFiles);
        }


        // Generate QR Code as a PNG Image for a file
        [HttpGet("{rCode}/qr")]
        public async Task<IActionResult> GetQRCode(string rCode)
        {
            var file = await dbContext.RcodeFiles.SingleOrDefaultAsync(f => f.Rcode == rCode);
            if (file == null)
            {
                return NotFound(new { message = "File not found!" });
            }

            try
            {
                QRCodeGenerator qrGenerator = new QRCodeGenerator();
                QRCodeData qrCodeData = qrGenerator.CreateQrCode(file.Rcode, QRCodeGenerator.ECCLevel.Q);
                PngByteQRCode qrCode = new PngByteQRCode(qrCodeData);

                byte[] qrCodeBytes = qrCode.GetGraphic(20);
                return File(qrCodeBytes, "image/png"); // Return as PNG image
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error generating QR Code", error = ex.Message });
            }
        }

        // Add file and generate QR code dynamically
        [HttpPost]
        public async Task<IActionResult> AddFile([FromBody] AddRcodeFileDto addFileDto)
        {
            // Check if the Rcode already exists in the database
            if (await dbContext.RcodeFiles.AnyAsync(f => f.Rcode == addFileDto.Rcode))
            {
                return BadRequest(new { message = "File with this Rcode already exists!" });
            }

            try
            {
                // Generate the QR code dynamically
                QRCodeGenerator qrGenerator = new QRCodeGenerator();
                QRCodeData qrCodeData = qrGenerator.CreateQrCode(addFileDto.Rcode, QRCodeGenerator.ECCLevel.Q);
                PngByteQRCode qrCode = new PngByteQRCode(qrCodeData);

                // Get the QR code as a byte array
                byte[] qrCodeBytes = qrCode.GetGraphic(20);

                // Create the file entity (without storing the QR code image)
                var fileEntity = new RcodeFile()
                {
                    Rcode = addFileDto.Rcode,
                    EName = addFileDto.EName,
                    EpfNo = addFileDto.EpfNo,
                    ContactNo = addFileDto.ContactNo,
                    Status = addFileDto.Status,
                    GetDate = addFileDto.GetDate
                };

                // Add the new file to the database
                await dbContext.RcodeFiles.AddAsync(fileEntity);
                await dbContext.SaveChangesAsync();

                // Return the file details along with the dynamically generated QR code as a PNG
                return CreatedAtAction(nameof(GetFileById), new { rCode = fileEntity.Rcode }, new
                {
                    file = fileEntity,
                    qrCodeImage = File(qrCodeBytes, "image/png") // Return the QR code as PNG image in the response
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while generating the QR code.", error = ex.Message });
            }
        }

        // Update file details (Remove Role update here)
        [HttpPut("{rCode}")]
        public async Task<IActionResult> UpdateFile(string rCode, [FromBody] UpdateRcodeFileDto updateFileDto)
        {
            var file = await dbContext.RcodeFiles.SingleOrDefaultAsync(f => f.Rcode == rCode);
            if (file == null)
            {
                return NotFound(new { message = "File not found!" });
            }

            file.EName = updateFileDto.EName;
            file.EpfNo = updateFileDto.EpfNo;
            file.ContactNo = updateFileDto.ContactNo;
            file.Status = updateFileDto.Status;
            file.GetDate = updateFileDto.GetDate;

            await dbContext.SaveChangesAsync();
            return Ok(file);
        }

        // Delete file
        [HttpDelete("{rCode}")]
        public async Task<IActionResult> DeleteFile(string rCode)
        {
            var file = await dbContext.RcodeFiles.SingleOrDefaultAsync(f => f.Rcode == rCode);
            if (file == null)
            {
                return NotFound(new { message = "File not found!" });
            }

            dbContext.RcodeFiles.Remove(file);
            await dbContext.SaveChangesAsync();
            return Ok(new { message = "File deleted successfully!" });
        }

        // Generate QR Code as Base64 string (this is kept as a helper if needed in future)
        private string GenerateQRCodeBase64(string qrText)
        {
            using (QRCodeGenerator qrGenerator = new QRCodeGenerator())
            {
                QRCodeData qrCodeData = qrGenerator.CreateQrCode(qrText, QRCodeGenerator.ECCLevel.Q);
                PngByteQRCode qrCode = new PngByteQRCode(qrCodeData);
                byte[] qrBytes = qrCode.GetGraphic(20);
                return Convert.ToBase64String(qrBytes); // Return as Base64 string
            }
        }

        [HttpPost("generate-multi-qr")]
        public async Task<IActionResult> GenerateMultiQRCode([FromBody] List<string> rCodes)
        {
            if (rCodes == null || !rCodes.Any())
            {
                return BadRequest(new { message = "No files selected!" });
            }

            string qrContent = string.Join(",", rCodes); // Combine Rcodes as CSV

            // Generate QR Code
            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(qrContent, QRCodeGenerator.ECCLevel.Q);
            PngByteQRCode qrCode = new PngByteQRCode(qrCodeData);

            byte[] qrCodeBytes = qrCode.GetGraphic(20);
            return File(qrCodeBytes, "image/png");
        }

        [HttpPut("update-multi-files")]
        public async Task<IActionResult> UpdateFiles([FromBody] UpdateMultipleFilesDto updateDto)
        {
            var rcodes = updateDto.Rcodes;
            var newEpfNo = updateDto.NewEpfNo;
            var newEName = updateDto.NewEName;

            // Find files matching the Rcodes
            var filesToUpdate = await dbContext.RcodeFiles
                .Where(f => rcodes.Contains(f.Rcode))  // Match by Rcode
                .ToListAsync();

            if (filesToUpdate.Count == 0)
            {
                return NotFound(new { message = "No files found with the provided Rcodes." });
            }

            // Update files with the new employee details
            foreach (var file in filesToUpdate)
            {
                file.EpfNo = newEpfNo;
                file.EName = newEName;
            }

            await dbContext.SaveChangesAsync();

            return Ok(new { message = $"{filesToUpdate.Count} file(s) updated successfully." });
        }


    }
}
