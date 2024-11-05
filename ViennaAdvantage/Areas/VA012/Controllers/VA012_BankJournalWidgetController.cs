using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using VA012.Models;
using VAdvantage.Utility;

namespace VA012.Controllers
{
    public class VA012_BankJournalWidgetController : Controller
    {
        // GET: VA012/VA012_BankJournalWidget
        public ActionResult Index()
        {
            return View();
        }
        /// <summary>
        /// Gets users created and share folder of login user. Runs process to delete documents from recycle bin.
        /// </summary>
        /// <param name="WinNo">Window number for creating form frame.</param>
        /// <param name="UserID">Logged in user id.</param>
        /// <param name="roleID">Logged in role id.</param>
        /// <param name="clientID">Logged in tenant id.</param>
        /// <param name="orgID">Logged in user id.</param>
        /// <param name="orderBy">Order by for selected folder.</param>
        /// <returns>Returns list of FolderProperties class type.Contains folders information.</returns>
        public JsonResult GetFolders( int UserID, int roleID, int clientID, int orgID, string orderBy)
        {
            Ctx ctx = Session["ctx"] as Ctx;
           // List<VA012_FolderProperties> lstFolder = null;
            if (UserID == 0)
            {
                UserID = ctx.GetAD_User_ID();
            }
            VA012_BankingJournalWidgetModel objFolder = new VA012_BankingJournalWidgetModel();
            object lstFolder = "";
            lstFolder = objFolder.GetFolders(ctx, UserID, roleID, clientID, orgID);
            var jsonResult = Json(JsonConvert.SerializeObject(lstFolder), JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }
        /// <summary>
        /// Checks the users access on folders. 
        /// </summary>
        /// <param name="folderID">Id of folder selected.</param>
        /// <returns>Returns the access id as json string</returns>
        public JsonResult CheckFolderAccess(int folderID)
        {
            int result = 0;
            StringBuilder strMsg = new StringBuilder();
            try
            {
                Ctx ctx = Session["ctx"] as Ctx;
                VA012_BankingJournalWidgetModel objFolder = new VA012_BankingJournalWidgetModel();
                result = objFolder.CheckFolderAccess(folderID, ctx);
            }
            catch (Exception ex)
            {
                strMsg.Append("Error:" + ex.Message);
            }
            return Json(JsonConvert.SerializeObject(result), JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// Get Document when user click on folder using string array parameter.
        /// </summary>
        /// <param name="parameter">Array of string for getting the documents.This contains parameter values.</param>
        /// <returns>Returns list of DocumentProperties class type that contains all properties of document.</returns>
      
        public JsonResult GetDocument(string[] parameter)
        {
            VA012_BankingJournalWidgetModel objDocument = new VA012_BankingJournalWidgetModel();
            object FillGrid = "";
            Ctx ctx = Session["ctx"] as Ctx;
           /// List<DocumentProperties>
                FillGrid = objDocument.LoadDocument(Convert.ToInt32(parameter[1]), Convert.ToString(parameter[0]), Convert.ToInt32(parameter[3]), 
                Convert.ToInt32(parameter[4]),Convert.ToBoolean(parameter[5]), Convert.ToBoolean(parameter[6]), Convert.ToBoolean(parameter[7]),
                Convert.ToString(parameter[8]), ctx, Convert.ToInt32(parameter[9]), Convert.ToInt32(parameter[10]), Convert.ToString(parameter[11]));
            var jsonResult = Json(JsonConvert.SerializeObject(FillGrid), JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }
        public JsonResult GetMetaData(int documentID)
        {
            VA012_BankingJournalWidgetModel objDocument = new VA012_BankingJournalWidgetModel();
            object metaData = "";
            Ctx ctx = Session["ctx"] as Ctx;
            metaData = objDocument.GetMetaData(documentID,ctx);
            var jsonResult = Json(JsonConvert.SerializeObject(metaData), JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }
    }

}