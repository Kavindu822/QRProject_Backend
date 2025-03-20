//using QRCoder;
//using System;
//using System.Drawing;
//using System.IO;

//namespace QRFileTrackingapi.Helpers
//{
//    public static class QRCodeHelper
//    {
//        public static string GenerateQRCode(string text)
//        {
//            try
//            {
//                using (QRCodeGenerator qrGenerator = new QRCodeGenerator())
//                {
//                    using (QRCodeData qrCodeData = qrGenerator.CreateQrCode(text, QRCodeGenerator.ECCLevel.Q))
//                    {
//                        using (QRCode qrCode = new QRCode(qrCodeData))
//                        {
//                            using (Bitmap qrBitmap = qrCode.GetGraphic(20)) // You can change '20' to adjust image size
//                            {
//                                using (MemoryStream ms = new MemoryStream())
//                                {
//                                    qrBitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
//                                    return Convert.ToBase64String(ms.ToArray()); // Store as Base64
//                                }
//                            }
//                        }
//                    }
//                }
//            }
//            catch (Exception ex)
//            {
//                // Log the exception (you can use a logging framework here)
//                throw new InvalidOperationException("Error generating QR code", ex);
//            }
//        }
//    }
//}
