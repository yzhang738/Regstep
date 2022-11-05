using Microsoft.AspNet.Identity;
using RSToolKit.Domain.Exceptions;
using RSToolKit.Domain.Logging;
using System;
using System.Linq;
using System.Web.Mvc;

namespace RSToolKit.WebUI.Infrastructure
{
    /// <summary>
    /// Handles RegStepExceptions of the designated type.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited = true, AllowMultiple = true)]
    public class RegStepHandleErrorAttribute
        : FilterAttribute, IExceptionFilter
    {

        protected Type _exceptionType;
        protected string _view;
       
        public RegStepHandleErrorAttribute(string view = null)
            : base()
        {
            _exceptionType = typeof(RegStepException);
            _view = String.IsNullOrWhiteSpace(view) ? "RegStepException" : view;
        }

        public RegStepHandleErrorAttribute(Type exceptionType, string view = null)
            : this()
        {
            if (exceptionType.IsAssignableFrom(typeof(RegStepException)))
                _exceptionType = exceptionType;
            if (!String.IsNullOrWhiteSpace(view))
                _view = view;
        }

        private bool IsAjax(ExceptionContext filterContext)
        {
            return filterContext.HttpContext.Request.IsAjaxRequest();
        }

        public void OnException(ExceptionContext filterContext)
        {
            var excType = filterContext.Exception.GetType();
            if (excType != _exceptionType)
                return;
            if (filterContext.ExceptionHandled)
                return;
            var exc = (RegStepException)filterContext.Exception;
            var logger = new DataLogger();
            if (filterContext.HttpContext.User.Identity.IsAuthenticated)
                logger.UserId = filterContext.HttpContext.User.Identity.GetUserId();
            var log = logger.Error(exc);
            exc.LoggedId = log.SortingId;
            // if the request is AJAX return JSON else view.
            if (IsAjax(filterContext))
            {
                //Because its a exception raised after ajax invocation
                //Lets return a Json.Net object
                filterContext.HttpContext.Response.Clear();
                filterContext.Result = new JsonNetResult()
                {
                    Data = filterContext.Exception.Message,
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                };
                filterContext.HttpContext.Response.StatusCode = 500;
                filterContext.ExceptionHandled = true;
            }
            else
            {
                var result = CreateActionResult(filterContext);
                filterContext.Result = result;

                // Prepare the response code.
                filterContext.ExceptionHandled = true;
                filterContext.HttpContext.Response.Clear();
                filterContext.HttpContext.Response.StatusCode = 500;
                filterContext.HttpContext.Response.TrySkipIisCustomErrors = true;
            }
        }

        protected virtual ActionResult CreateActionResult(ExceptionContext filterContext)
        {
            var ctx = new ControllerContext(filterContext.RequestContext, filterContext.Controller);
            var viewName = SelectFirstView(ctx,
                                           "~/Views/Error/" + _view + ".cshtml",
                                           "~/Views/Error/RegStepException.cshtml");

            var controllerName = (string)filterContext.RouteData.Values["controller"];
            var actionName = (string)filterContext.RouteData.Values["action"];
            var model = new HandleErrorInfo((RegStepException)filterContext.Exception, controllerName, actionName);
            var result = new ViewResult
            {
                ViewName = viewName,
                ViewData = new ViewDataDictionary<HandleErrorInfo>(model),
            };
            return result;
        }

        protected string SelectFirstView(ControllerContext ctx, params string[] viewNames)
        {
            return viewNames.First(view => ViewExists(ctx, view));
        }

        protected bool ViewExists(ControllerContext ctx, string name)
        {
            var result = ViewEngines.Engines.FindView(ctx, name, null);
            return result.View != null;
        }
    }
}