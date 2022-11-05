using System;
using System.Web.Mvc;

namespace RSToolKit.WebUI.Infrastructure.Attributes
{
    /// <summary>
    /// Sets the <code>ViewBag.Title</code> property to be used in the view.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited = true, AllowMultiple = true)]
    public class HtmlTitleAttribute
        : ActionFilterAttribute
    {
        /// <summary>
        /// The title to use for the <code>ViewBag.Title</code> property.
        /// </summary>
        public string Title { get; set; }

        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            filterContext.Controller.ViewBag.Title = Title;
        }
    }
}