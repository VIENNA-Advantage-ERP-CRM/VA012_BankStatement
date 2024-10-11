using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using VA012.Models;
using VAdvantage.Utility;

namespace VA012.Controllers
{
    public class VA012_WidgetController : Controller
    {
        // GET: VA012/VA012_Widget
        public ActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public JsonResult SaveFileinTemp(HttpPostedFileBase file, string fileName, string folderKey)
        {
            try
            {
                if (!Directory.Exists(Path.Combine(System.Web.Hosting.HostingEnvironment.ApplicationPhysicalPath, "TempDownload")))
                {
                    Directory.CreateDirectory(Path.Combine(System.Web.Hosting.HostingEnvironment.ApplicationPhysicalPath, "TempDownload"));
                }
                //if(File)
                HttpPostedFileBase hpf = file as HttpPostedFileBase;
                string fName = fileName;
                string savedFileName = Path.Combine(System.Web.Hosting.HostingEnvironment.ApplicationPhysicalPath, "TempDownload", Path.GetFileName(fileName));
                MemoryStream ms = new MemoryStream();
                hpf.InputStream.CopyTo(ms);
                byte[] byteArray = ms.ToArray();

                if (System.IO.File.Exists(savedFileName))//Append Content In File
                {
                    using (FileStream fs = new FileStream(savedFileName, FileMode.Append, System.IO.FileAccess.Write))
                    {
                        fs.Write(byteArray, 0, byteArray.Length);
                        ms.Close();
                    }
                }
                else // create new file
                {
                    using (FileStream fs = new FileStream(savedFileName, FileMode.Create, System.IO.FileAccess.Write))
                    {
                        fs.Write(byteArray, 0, byteArray.Length);
                        ms.Close();
                    }
                }

                //using (var rasterizer = new GhostscriptRasterizer())
                //{
                //    // Open the PDF file
                //    rasterizer.Open(savedFileName);

                //    // Render the specified page as an image
                //    Image pdfPageImage = rasterizer.GetPage(300, 1); // 300 DPI resolution

                //    // Save the image as a PNG file
                //    string outputFile = Path.Combine(Path.Combine(System.Web.Hosting.HostingEnvironment.ApplicationPhysicalPath, "TempDownload"), $"Page_{1}.png");
                //    pdfPageImage.Save(outputFile, ImageFormat.Png);

                //    //return outputFile;
                //}

                return Json(folderKey, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json("ERROR:" + ex.Message, JsonRequestBehavior.AllowGet);
            }

        }
    }
}