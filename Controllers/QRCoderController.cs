using Microsoft.AspNetCore.Mvc;
using QRCoder;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace ETicketApp.Controllers
{
     public class QRCoderController : Controller
     {
          [HttpGet]
          public IActionResult Index()
          {
               return View();
          }

          [HttpPost]
          public IActionResult GenerateQRCode(string qrText)
          {
               if (string.IsNullOrWhiteSpace(qrText))
               {
                    return View("Index");
               }

               using var qrGenerator = new QRCodeGenerator();
               var qrCodeData = qrGenerator.CreateQrCode(qrText, QRCodeGenerator.ECCLevel.Q);
               using var qrCode = new QRCode(qrCodeData);
               using var bitmap = qrCode.GetGraphic(20);

               using var memoryStream = new MemoryStream();
               bitmap.Save(memoryStream, ImageFormat.Png);
               ViewBag.QRCodeImage = Convert.ToBase64String(memoryStream.ToArray());

               return View("Index");
          }
     }
}
