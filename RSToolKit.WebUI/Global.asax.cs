using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using RSToolKit.Domain.Data;
using System.Data.Entity;
using System;
using System.Web;
using RSToolKit.Domain.Logging;
using RSToolKit.WebUI.Infrastructure;
using Newtonsoft.Json;

namespace RSToolKit.WebUI
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            WebApiConfig.Register(GlobalConfiguration.Configuration);
            AreaRegistration.RegisterAllAreas();
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            Database.SetInitializer<EFDbContext>(null);
            GlobalConfiguration.Configuration.MessageHandlers.Add(new MissingContentTypeDH());
        }

        protected void Application_BeginRequest(Object sender, EventArgs e)
        {
#if !DEBUG
            if (HttpContext.Current.Request.IsSecureConnection.Equals(false) && HttpContext.Current.Request.IsLocal.Equals(false))
            {
                Response.Redirect("https://" + Request.ServerVariables["HTTP_HOST"] + HttpContext.Current.Request.RawUrl);
            }
#endif
        }

        protected void Application_Error(object sender, EventArgs e)
        {
            Exception exception = Server.GetLastError();
            var iLog = new DataLogger()
            {
                Thread = "Main",
                LoggingMethod = "Global"
            };

            int status = 500;
            var message = exception.Message;
            Server.ClearError();
            var httpException = exception as HttpException;
            if (httpException != null)
                status = httpException.GetHttpCode();
            if (status != 404)
                return;
            Server.ClearError();
            bool isAjax = String.Equals("XMLHttpRequest", Context.Request.Headers["x-requested-with"], StringComparison.OrdinalIgnoreCase);
            if (isAjax)
            {

                Context.Response.ContentType = "application/json";
                Context.Response.StatusCode = status;
                Context.Response.Write(JsonConvert.SerializeObject(new { message = message }));
            }
            else
            {
                Response.Clear();
                Response.Redirect("~/Error/Error404");
            }
        }
    }
}
