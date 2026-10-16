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

            // Built by webpack (npm run build) from Scripts/src/VA012js.js and Contents/src/VA012css.css
            style.Include("~/Areas/VA012/Contents/VA012.all.min.css");
            script.Include("~/Areas/VA012/Scripts/dist/VA012.all.min.js");

            // already minified by webpack: keep the content as-is (no re-minify, no parse)
            script.Transforms.Clear();
            script.Transforms.Add(new NoTransform());

            VAdvantage.ModuleBundles.RegisterScriptBundle(script, "VA012", 10);
            VAdvantage.ModuleBundles.RegisterStyleBundle(style, "VA012", 10);
        }
    }

    public sealed class NoTransform : IBundleTransform
    {
        public void Process(BundleContext context, BundleResponse response)
        {
            // Do nothing: keep content as-is (no minify, no parse)
        }
    }
}
