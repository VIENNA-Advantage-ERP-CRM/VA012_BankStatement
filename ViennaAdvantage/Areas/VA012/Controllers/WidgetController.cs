using CoreLibrary.DataBase;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using VAdvantage.Utility;

namespace VA012.Controllers
{
    public class WidgetController : Controller
    {
        public JsonResult GetReference()
        {
            Dictionary<string, object> info = new Dictionary<string, object>();
            if (Session["ctx"] != null)
            {
                int ref_ID = Util.GetValueOfInt(DB.ExecuteScalar("SELECT AD_Reference_ID FROM AD_Reference WHERE Name = 'VA012_BankConnectVia'"));
                info["BankConnectVia_Reference_ID"] = ref_ID;
            }
            return Json(JsonConvert.SerializeObject(info), JsonRequestBehavior.AllowGet);
        }
    }
}