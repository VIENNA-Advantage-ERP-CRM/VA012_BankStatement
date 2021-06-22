using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;
using VA012.Models;
using VAdvantage.Utility;

namespace VA012.Controllers
{
    public class StatementController : Controller
    {
        /// <summary>
        /// /Get Account Date
        /// </summary>
        /// <param name="fields">Tab Fields</param>
        /// <returns>Account Date</returns>
        public JsonResult GetCashDetails(String fields)
        {
            string retJSON = "";
            if (Session["ctx"] != null)
            {
                Ctx ctx = Session["ctx"] as Ctx;
                StatementModel statement = new StatementModel();
                retJSON = JsonConvert.SerializeObject(statement.GetAcctDate(ctx, fields));
            }
            return Json(retJSON, JsonRequestBehavior.AllowGet);
        }
    }
}