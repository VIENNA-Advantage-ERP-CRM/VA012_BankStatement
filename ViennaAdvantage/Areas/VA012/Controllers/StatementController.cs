using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CoreLibrary.DataBase;
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

        /// <summary>
        /// Get the PaymentBase Type
        /// </summary>
        /// <param name="fields">fields</param>
        /// <returns>PaymentBase Type in JSON format</returns>
        public JsonResult GetPaymentRule(String fields)
        {
            string retJSON = "";
            if (Session["ctx"] != null)
            {
                Ctx ctx = Session["ctx"] as Ctx;
                StatementModel statement = new StatementModel();
                retJSON = JsonConvert.SerializeObject(statement.GetPaymentRule(ctx, fields));
            }
            return Json(retJSON, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// Get Invoice/Order PaymentMethodId
        /// Author:Rakesh(VA228)
        /// </summary>
        /// <param name="fields">fields</param>
        /// <returns>PaymentMethodId</returns>
        public JsonResult GetPaymentMethod(String fields)
        {
            string retJSON = "";
            if (Session["ctx"] != null)
            {
                Ctx ctx = Session["ctx"] as Ctx;
                StatementModel statement = new StatementModel();
                retJSON = JsonConvert.SerializeObject(statement.GetPaymentMethod(ctx, fields));
            }
            return Json(retJSON, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// GetCharge Type for contra
        /// Author:Meghraj(VA323)
        /// </summary>
        /// <param name="fields">fields</param>
        /// <returns>ChargeType</returns>
        public JsonResult GetChargeType(String fields)
        {
            string retJSON = "";
            if (Session["ctx"] != null)
            {
                Ctx ctx = Session["ctx"] as Ctx;
                StatementModel statement = new StatementModel();
                retJSON = JsonConvert.SerializeObject(statement.GetChargeMethod(ctx, fields));
            }
            return Json(retJSON, JsonRequestBehavior.AllowGet);
        }
    }
}