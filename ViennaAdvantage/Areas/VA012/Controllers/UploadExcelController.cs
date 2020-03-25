using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
//using VIS.Filters;
using System.Web.Mvc;
using System.IO;
using VA012.Models;
using Newtonsoft.Json;

namespace VA012.Controllers
{
    public class UploadExcelController : Controller
    {
        //
        // GET: /VA012/UploadExcel/
        public ActionResult Index()
        {
            return View();
        }
       // [AjaxAuthorizeAttribute]
       // [AjaxSessionFilterAttribute]
        [HttpPost]
        public JsonResult SaveFileinTemp(HttpPostedFileBase file, string fileName, string folderKey, string orgFileName)
        {

            StatementResponse _obj = new StatementResponse();
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
                

                
                _obj._path = savedFileName;
                _obj._filename=fName;
                _obj._orgfilename = orgFileName;
                //_obj._filename=f_name;
                return Json(_obj, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _obj._error = "ERROR:" + ex.Message;
                return Json(_obj, JsonRequestBehavior.AllowGet);
            }

        }
	}
}