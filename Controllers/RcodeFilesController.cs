using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QRCoder;
using QRFileTrackingapi.Data;
using QRFileTrackingapi.Models;
using QRFileTrackingapi.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
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
        //[Authorize(Roles = "Admin")]
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

        // Get history of a file
        [HttpGet("{rCode}/history")]
        public async Task<IActionResult> GetFileHistory(string rCode)
        {
            var history = await dbContext.RcodeFilesHistories
                .Where(h => h.Rcode == rCode)
                .OrderByDescending(h => h.TransferDate)
                .ToListAsync();

            if (!history.Any())
            {
                return NotFound(new { message = "No history found for this file!" });
            }

            return Ok(history);
        }

        [HttpGet("history")]
        public async Task<IActionResult> GetAllFilesHistory()
        {
            var history = await dbContext.RcodeFilesHistories
                .OrderByDescending(h => h.TransferDate)
                .ToListAsync();

            if (!history.Any())
            {
                return NotFound(new { message = "No file history found!" });
            }

            return Ok(history);
        }

        [Authorize(Roles = "Employee")]
        [HttpGet("my-history")]
        public async Task<IActionResult> GetMyHistory()
        {
            // Get the EPF No of the logged-in user from JWT claims
            var epfNo = User.Claims.FirstOrDefault(c => c.Type == "EpfNo")?.Value;

            if (string.IsNullOrEmpty(epfNo))
            {
                return Unauthorized(new { message = "Invalid user or missing EPF No in token." });
            }

            // Fetch the file history for the logged-in user where NewEpfNo matches the EPF No
            var userHistory = await dbContext.RcodeFilesHistories
                .Where(h => h.NewEpfNo == epfNo)  // Match NewEpfNo with EPF No from the JWT token
                .ToListAsync();

            if (userHistory == null || !userHistory.Any())
            {
                return NotFound(new { message = "No history found for this user." });
            }

            return Ok(userHistory);
        }




        [Authorize(Roles = "Employee")]
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


        // Add file
        [HttpPost]
        public async Task<IActionResult> AddFile([FromBody] AddRcodeFileDto addFileDto)
        {
            // Check if the file with the same Rcode already exists
            if (await dbContext.RcodeFiles.AnyAsync(f => f.Rcode == addFileDto.Rcode))
            {
                return BadRequest(new { message = "File with this Rcode already exists!" });
            }

            try
            {
                // Set status based on Employee Name
                string status = addFileDto.EName == "Library" ? "In" : "Out";

                // If EName is 'Library', allow EPF No to be null or any value.
                // If EName is not 'Library', EPF No is required.
                if (addFileDto.EName != "Library" && string.IsNullOrEmpty(addFileDto.EpfNo))
                {
                    return BadRequest(new { message = "EPF No is required when Employee Name is not 'Library'." });
                }

                // Create the RcodeFile entity
                var fileEntity = new RcodeFile()
                {
                    Rcode = addFileDto.Rcode,
                    EName = addFileDto.EName,
                    EpfNo = addFileDto.EName == "Library" ? addFileDto.EpfNo : addFileDto.EpfNo, // EPF No can be null or any value for Library
                    ContactNo = addFileDto.ContactNo,
                    Status = status,
                    GetDate = addFileDto.GetDate,
                    UpdateDate = addFileDto.GetDate,
                    Department = addFileDto.Department
                };

                // Add the file to the database
                await dbContext.RcodeFiles.AddAsync(fileEntity);
                await dbContext.SaveChangesAsync();

                // Return success response
                return Ok(new { file = fileEntity });
            }
            catch (Exception ex)
            {
                // Log the detailed exception for debugging
                return StatusCode(500, new { message = "Error adding file.", error = ex.Message });
            }
        }



        // Update file details and log history
        [HttpPut("{rCode}")]
        public async Task<IActionResult> UpdateFile(string rCode, [FromBody] UpdateRcodeFileDto updateFileDto)
        {
            var file = await dbContext.RcodeFiles.SingleOrDefaultAsync(f => f.Rcode == rCode);
            if (file == null)
            {
                return NotFound(new { message = "File not found!" });
            }

            // Log history
            var history = new RcodeFileHistory()
            {
                Rcode = file.Rcode,
                PreviousEpfNo = file.EpfNo,
                NewEpfNo = updateFileDto.EpfNo,
                PreviousEName = file.EName,
                NewEName = updateFileDto.EName,
                PreviousContactNo = file.ContactNo,
                NewContactNo = updateFileDto.ContactNo,
                TransferDate = DateTime.UtcNow
            };

            dbContext.RcodeFilesHistories.Add(history);

            // Update file details
            file.EName = updateFileDto.EName;
            file.EpfNo = updateFileDto.EpfNo;
            file.ContactNo = updateFileDto.ContactNo;
            file.Status = updateFileDto.Status;
            file.GetDate = updateFileDto.GetDate;
            file.UpdateDate = DateTime.UtcNow;
            file.Department = updateFileDto.Department; // Update department

            await dbContext.SaveChangesAsync();
            return Ok(new { message = "File updated successfully!", file });
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

        //[Authorize]
        [HttpPut("update-multi-files")]
        public async Task<IActionResult> UpdateMultipleFiles([FromBody] UpdateMultipleFilesDto updateDto)
        {
            // Validate inputs
            if (updateDto.Rcodes == null || updateDto.Rcodes.Count == 0)
            {
                return BadRequest(new { message = "Rcodes are required." });
            }

            if (string.IsNullOrEmpty(updateDto.NewEpfNo) || string.IsNullOrEmpty(updateDto.NewEName))
            {
                return BadRequest(new { message = "New employee details are required." });
            }

            var rcodes = updateDto.Rcodes;
            var newEpfNo = updateDto.NewEpfNo;
            var newEName = updateDto.NewEName;
            var newContactNo = updateDto.NewContactNo;

            // Convert UTC to Sri Lanka time
            TimeZoneInfo lankaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Sri Lanka Standard Time");
            var nowLankaTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, lankaTimeZone);

            // Find files matching the Rcodes
            var filesToUpdate = await dbContext.RcodeFiles
                .Where(f => rcodes.Contains(f.Rcode))
                .ToListAsync();

            if (filesToUpdate.Count == 0)
            {
                return NotFound(new { message = "No files found with the provided Rcodes." });
            }

            // Create history entries
            var historyEntries = filesToUpdate.Select(file => new RcodeFileHistory
            {
                Rcode = file.Rcode,
                PreviousEpfNo = file.EpfNo,
                PreviousEName = file.EName,
                PreviousContactNo = file.ContactNo,
                NewEpfNo = newEpfNo,
                NewEName = newEName,
                NewContactNo = newContactNo,
                TransferDate = nowLankaTime
            }).ToList();

            await dbContext.RcodeFilesHistories.AddRangeAsync(historyEntries);

            foreach (var file in filesToUpdate)
            {
                file.EpfNo = newEpfNo;
                file.EName = newEName;
                file.ContactNo = newContactNo;
                file.UpdateDate = nowLankaTime;

                if (file.EpfNo.Equals("Library", StringComparison.OrdinalIgnoreCase) ||
                    file.EName.Equals("Library", StringComparison.OrdinalIgnoreCase))
                {
                    file.Status = "Out";
                }
            }

            await dbContext.SaveChangesAsync();

            return Ok(new
            {
                message = $"{filesToUpdate.Count} file(s) updated successfully.",
                updatedFiles = filesToUpdate
            });
        }


        [Authorize]
        [HttpPut("transfer-files-to-employee-or-via-qr")]
        public async Task<IActionResult> TransferFilesToEmployeeOrViaQR([FromBody] UpdateMultipleFilesDto updateDto)
        {
            if (updateDto.Rcodes == null || updateDto.Rcodes.Count == 0)
            {
                return BadRequest(new { message = "Rcodes are required." });
            }

            var cleanedRcodes = updateDto.Rcodes
                .SelectMany(r => r.Split(','))
                .Select(r => r.Replace("[", "").Replace("]", "").Replace("\"", "").Trim())
                .Where(r => !string.IsNullOrWhiteSpace(r))
                .ToList();

            if (cleanedRcodes.Count == 0)
            {
                return BadRequest(new { message = "Invalid Rcodes format." });
            }

            if (string.IsNullOrEmpty(updateDto.NewEpfNo) || string.IsNullOrEmpty(updateDto.NewEName))
            {
                return BadRequest(new { message = "New employee details are required." });
            }

            var newEpfNo = updateDto.NewEpfNo;
            var newEName = updateDto.NewEName;
            var newContactNo = updateDto.NewContactNo;

            TimeZoneInfo lankaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Sri Lanka Standard Time");
            var nowLankaTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, lankaTimeZone);

            var filesToUpdate = await dbContext.RcodeFiles
                .Where(f => cleanedRcodes.Contains(f.Rcode))
                .ToListAsync();

            if (filesToUpdate.Count == 0)
            {
                return NotFound(new { message = "No files found with the provided Rcodes." });
            }

            var historyEntries = filesToUpdate.Select(file => new RcodeFileHistory
            {
                Rcode = file.Rcode,
                PreviousEpfNo = file.EpfNo,
                PreviousEName = file.EName,
                PreviousContactNo = file.ContactNo,
                NewEpfNo = newEpfNo,
                NewEName = newEName,
                NewContactNo = newContactNo,
                TransferDate = nowLankaTime
            }).ToList();

            await dbContext.RcodeFilesHistories.AddRangeAsync(historyEntries);

            foreach (var file in filesToUpdate)
            {
                file.EpfNo = newEpfNo;
                file.EName = newEName;
                file.ContactNo = newContactNo;
                file.UpdateDate = nowLankaTime;

                if (newEpfNo.Equals("Library", StringComparison.OrdinalIgnoreCase) ||
                    newEName.Equals("Library", StringComparison.OrdinalIgnoreCase))
                {
                    file.Status = "Out";
                }
            }

            await dbContext.SaveChangesAsync();

            return Ok(new
            {
                message = $"{filesToUpdate.Count} file(s) transferred successfully.",
                updatedFiles = filesToUpdate
            });
        }




        // Transfer all files owned by the logged-in user
        [Authorize]
        [HttpPut("transfer-my-files")]
        public async Task<IActionResult> TransferMyFiles([FromBody] TransferAllFilesDto transferAllDto)
        {
            if (string.IsNullOrEmpty(transferAllDto.NewEpfNo) || string.IsNullOrEmpty(transferAllDto.NewEName))
            {
                return BadRequest(new { message = "New owner details are required!" });
            }

            var userEpfNo = User.FindFirst("EpfNo")?.Value;
            if (string.IsNullOrEmpty(userEpfNo))
            {
                return Unauthorized(new { message = "Invalid token. Cannot find EPF No." });
            }

            var myFiles = await dbContext.RcodeFiles
                .Where(f => f.EpfNo == userEpfNo)
                .ToListAsync();

            if (myFiles.Count == 0)
            {
                return NotFound(new { message = "You do not own any files to transfer!" });
            }

            var historyEntries = myFiles.Select(file => new RcodeFileHistory
            {
                Rcode = file.Rcode,
                PreviousEpfNo = file.EpfNo,
                PreviousEName = file.EName,
                PreviousContactNo = file.ContactNo,
                NewEpfNo = transferAllDto.NewEpfNo,
                NewEName = transferAllDto.NewEName,
                NewContactNo = transferAllDto.NewContactNo,
                TransferDate = DateTime.UtcNow
            }).ToList();

            await dbContext.RcodeFilesHistories.AddRangeAsync(historyEntries);

            foreach (var file in myFiles)
            {
                file.EpfNo = transferAllDto.NewEpfNo;
                file.EName = transferAllDto.NewEName;
                file.ContactNo = transferAllDto.NewContactNo;
                file.UpdateDate = DateTime.UtcNow;
            }

            await dbContext.SaveChangesAsync();

            return Ok(new
            {
                message = $"{myFiles.Count} file(s) transferred successfully.",
                previousOwner = new { EpfNo = userEpfNo },
                newOwner = new { EpfNo = transferAllDto.NewEpfNo, EName = transferAllDto.NewEName, ContactNo = transferAllDto.NewContactNo }
            });
        }

        // Delete multiple files by Rcodes
        [HttpDelete]
        public async Task<IActionResult> DeleteFiles([FromBody] List<string> rCodes)
        {
            if (rCodes == null || rCodes.Count == 0)
            {
                return BadRequest(new { message = "No files selected for deletion." });
            }

            try
            {
                var filesToDelete = await dbContext.RcodeFiles
                    .Where(f => rCodes.Contains(f.Rcode))
                    .ToListAsync();

                if (filesToDelete.Count == 0)
                {
                    return NotFound(new { message = "No files found with the provided Rcodes." });
                }

                dbContext.RcodeFiles.RemoveRange(filesToDelete);
                await dbContext.SaveChangesAsync();

                return Ok(new { message = $"{filesToDelete.Count} file(s) deleted successfully!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error deleting files.", error = ex.Message });
            }
        }



    }
}
