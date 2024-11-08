using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using VA012.Models;
using VAdvantage.Utility;

namespace VA012.Controllers
{
    public class VA012_BankChargeSummaryController : Controller
    {
        // GET: VA012/VA012_BankChargeSummary
        public ActionResult Index()
        {
            return View();
        }
        /// <summary>
        /// Get Finacial Year data(Financial Year StartDate and End Date and Periods)
        /// </summary>
        /// <returns></returns>
        public JsonResult GetFinancialYearDetail()
        {
            string retJSON = "";
            if (Session["ctx"] != null)
            {
                Ctx ctx = Session["ctx"] as Ctx;
                string errorMsg = "";
                VA012_BankChargeSummaryModel obj = new VA012_BankChargeSummaryModel();
                retJSON = JsonConvert.SerializeObject(obj.GetFinancialYearDetail(ctx, out errorMsg));
            }
            return Json(retJSON, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// Get Bank Satement line data against charge
        /// </summary>
        /// <returns></returns>
        public JsonResult GetBankChargeData(int C_BankAccount_ID,int C_Charge_ID,DateTime yrStartDate,DateTime yrEndDate,int Year_ID)
        {
            string retJSON = "";
            if (Session["ctx"] != null)
            {
                Ctx ctx = Session["ctx"] as Ctx;
                string errorMessage = "";
                VA012_BankChargeSummaryModel obj = new VA012_BankChargeSummaryModel();
                retJSON = JsonConvert.SerializeObject(obj.GetBankChargeData(ctx,C_BankAccount_ID, C_Charge_ID, yrStartDate, yrEndDate, Year_ID,out errorMessage));
            }
            return Json(retJSON, JsonRequestBehavior.AllowGet);
        }
    }
}