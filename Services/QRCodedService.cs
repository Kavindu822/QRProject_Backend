using QRCoder;
using System;

namespace QRFileTrackingapi.Services
{
    public class QRCodeService
    {
        public byte[] GenerateQRCode(string text)
        {
            using (QRCodeGenerator qrGenerator = new QRCodeGenerator())
            {
                QRCodeData qrCodeData = qrGenerator.CreateQrCode(text, QRCodeGenerator.ECCLevel.Q);
                PngByteQRCode qrCode = new PngByteQRCode(qrCodeData);
                return qrCode.GetGraphic(20);
            }
        }
    }
}
