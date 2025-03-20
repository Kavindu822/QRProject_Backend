using Microsoft.AspNetCore.Mvc;
using QRCoder;
using SkiaSharp;
using System;

namespace QRFileTrackingapi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QRCodeController : ControllerBase
    {
        // Generate QR code and return as a PNG image
        [HttpGet("GenerateQRCode")]
        public ActionResult GenerateQRCode(string QRCodeText)
        {
            if (string.IsNullOrEmpty(QRCodeText))
            {
                return BadRequest("QRCodeText is required");
            }

            try
            {
                // Generate the QR code data
                QRCodeGenerator qrGenerator = new QRCodeGenerator();
                QRCodeData qrCodeData = qrGenerator.CreateQrCode(QRCodeText, QRCodeGenerator.ECCLevel.Q);
                PngByteQRCode qrCode = new PngByteQRCode(qrCodeData);

                // Get the QR code as a byte array
                byte[] qrCodeBytes = qrCode.GetGraphic(20);

                // Return the QR code as a PNG image
                return File(qrCodeBytes, "image/png");
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while generating the QR code.", error = ex.Message });
            }
        }
    }
}
