using System.Web;
using System.Web.Optimization;
using RSToolKit.WebUI.Infrastructure;

namespace RSToolKit.WebUI
{
    public class BundleConfig
    {
        // For more information on Bundling, visit http://go.microsoft.com/fwlink/?LinkId=254725
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/scripts/jquery")
                .Include("~/Scripts/jquery-2.1.4.js")
            );
            bundles.Add(new ScriptBundle("~/bundles/scripts/bootstrap")
                .Include("~/Scripts/bootstrap.js")
                .Include("~/Scripts/Bootstrap/Plugins/fileInput.js")
                .Include("~/Scripts/Bootstrap/Plugins/datetimepicker/bootstrap-datetimepicker.js")
            );
            bundles.Add(new ScriptBundle("~/bundles/scripts/extensions")
                .Include("~/Scripts/toolkit/extensions.js")
            );
            bundles.Add(new ScriptBundle("~/bundles/scripts/toolkit")
                .Include("~/Scripts/toolkit/toolkit.js")
                .Include("~/Scripts/toolkit/versioning.js")
                .Include("~/Scripts/toolkit/jQueryExtensions.js")
                .Include("~/Scripts/toolkit/trail.js")
                .Include("~/Scripts/toolkit/browserGap.js")
                .Include("~/Scripts/toolkit/prettyProcessing.js")
                .Include("~/Scripts/toolkit/restful.js")
                .Include("~/Scripts/Moment/moment.js")
            );
        }
    }
}