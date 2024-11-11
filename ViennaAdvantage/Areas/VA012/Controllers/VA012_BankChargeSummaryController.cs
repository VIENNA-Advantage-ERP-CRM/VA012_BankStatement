/*******************************************************
       * Module Name    : VAS
       * Purpose        : Bank charge Summary-Month wise widget Controller
       * Chronological  : Development
       * Created Date   : 7 Nov, 2024
       * Created by     : VIS103
******************************************************/
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
        /// Get Finacial Year data(Financial Year StartDate and End Date and Year)
        /// </summary>
        /// <returns>return year start, end date and Year</returns>
        public JsonResult GetFinancialYearDetail()
        {
            string retJSON = "";
            if (Session["ctx"] != null)
            {
                Ctx ctx = Session["ctx"] as Ctx;
                VA012_BankChargeSummaryModel obj = new VA012_BankChargeSummaryModel();
                retJSON = JsonConvert.SerializeObject(obj.GetFinancialYearDetail(ctx));
            }
            return Json(retJSON, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// Get Bank Statement line data against charge
        /// </summary>
        /// <param name="C_BankAccount_ID">Selected Bank Account</param>
        /// <param name="C_Charge_ID">Selected Charge Parameter if any</param>
        /// <param name="yrStartDate">Year Start Date</param>
        /// <param name="yrEndDate">Year End Date</param>
        /// <param name="Year_ID">Year ID</param>
        /// <returns>return list of labels and bank statement charge data</returns>
        public JsonResult GetBankChargeData(int C_BankAccount_ID, int C_Charge_ID, DateTime yrStartDate, DateTime yrEndDate, int Year_ID)
        {
            string retJSON = "";
            if (Session["ctx"] != null)
            {
                Ctx ctx = Session["ctx"] as Ctx;
                VA012_BankChargeSummaryModel obj = new VA012_BankChargeSummaryModel();
                retJSON = JsonConvert.SerializeObject(obj.GetBankChargeData(ctx, C_BankAccount_ID, C_Charge_ID, yrStartDate, yrEndDate, Year_ID));
            }
            return Json(retJSON, JsonRequestBehavior.AllowGet);
        }
    }
}