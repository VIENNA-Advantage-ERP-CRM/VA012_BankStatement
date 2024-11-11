using System.Web.Mvc;
using System.Web.Optimization;

namespace VA012
{
    public class VA012AreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "VA012";
            }
        }
        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "VA012_default",
                "VA012/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
                , new[] { "VA012.Controllers" }
            );
            StyleBundle style = new StyleBundle("~/Areas/VA012/Contents/VA012Style");
            ScriptBundle script = new ScriptBundle("~/Areas/VA012/Scripts/VA012Js");

            style.Include("~/Areas/VA012/Contents/bankstatement.css");
            style.Include("~/Areas/VA012/Contents/BankConnectWidget.css");
            style.Include("~/Areas/VA012/Contents/VA012_BankingJournal.css");
            script.Include("~/Areas/VA012/Scripts/apps/forms/bankstatement.js");
            script.Include("~/Areas/VA012/Scripts/apps/forms/VA012_BankConnectWidget.js");
            script.Include("~/Areas/VA012/Scripts/apps/forms/VA012_BankingJournal.js");
            script.Include("~/Areas/VA012/Scripts/uploadexcel.js");
            script.Include("~/Areas/VA012/Scripts/model/callouts.js");
            style.Include("~/Areas/VA012/Contents/VA012_BankChargeSummary.css");
            script.Include("~/Areas/VA012/Scripts/apps/forms/VA012_BankChargeSummary.js");

            //style.Include("~/Areas/VA012/Contents/VA012.all.min.css");
            //script.Include("~/Areas/VA012/Scripts/VA012.all.min.js");

            VAdvantage.ModuleBundles.RegisterScriptBundle(script, "VA012", 10);
            VAdvantage.ModuleBundles.RegisterStyleBundle(style, "VA012", 10);
        }
    }
}