using ZXing;
using ZXing.QrCode;
using System.Drawing;
using System.IO;

namespace ETicketApp.Services
{
     public class QrCodeService
     {
          public string GenerateQrCodeBase64(string text)
          {
               var barcodeWriter = new BarcodeWriter
               {
                    Format = BarcodeFormat.QR_CODE,
                    Options = new QrCodeEncodingOptions
                    {
                         Height = 250,
                         Width = 250
                    },
                    Renderer = new ZXing.Rendering.BitmapRenderer() // Add Renderer to fix InvalidOperationException
               };

               using (var qrImage = barcodeWriter.Write(text))
               using (var stream = new MemoryStream())
               {
                    qrImage.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                    return $"data:image/png;base64,{Convert.ToBase64String(stream.ToArray())}";
               }
          }
     }
}
