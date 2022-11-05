using System;
using System.Web.Mvc;
using System.Web;

namespace RSToolKit.WebUI.Infrastructure
{
    /// <summary>
    /// Checks to see if the request if for a minimal page. If it is, it will change the layout to the specified layout for a minimal view.
    /// If no layout is specified <code>_Layout_Minimal</code> is used.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
    public class MinimalViewAttribute
        : ActionFilterAttribute
    {
        protected string _layout;
        protected bool _useMinimum;

        /// <summary>
        /// Initializes the attribute with the specified layout.
        /// </summary>
        /// <param name="key"></param>
        public MinimalViewAttribute(string layout)
            : this()
        {
            this._layout = layout;
        }

        /// <summary>
        /// Initializes the attribute with the default layout.
        /// </summary>
        /// <param name="key"></param>
        public MinimalViewAttribute()
            : base()
        {
            this._layout = "~/Views/Shared/_Layout-Minimal.cshtml";
            this._useMinimum = false;
        }

        /// <summary>
        /// If we are rendering a minimum page, we change the layout.
        /// </summary>
        /// <param name="filterContext">The current filter context.</param>
        public override void OnResultExecuted(ResultExecutedContext filterContext)
        {
            if (!this._useMinimum || this._IsAjax(filterContext.HttpContext))
                return;
            var view = filterContext.Result as ViewResult;
            view.MasterName = _layout;
        }

        /// <summary>
        /// Checks to see if we are rendering a minimum page.
        /// </summary>
        /// <param name="filterContext">The current filter context.</param>
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var minimal = filterContext.HttpContext.Request.Params["minimal"] ?? "false";
            if (minimal != null && minimal.ToLower() == "true")
                this._useMinimum = true;
        }

        /// <summary>
        /// Checks to see if this is an ajax request.
        /// </summary>
        /// <param name="filterContext">The context of the filter.</param>
        /// <returns>True if ajax call, false otherwise.</returns>
        protected bool _IsAjax(HttpContextBase httpContext)
        {
            return httpContext.Request.Headers["X-Requested-With"] == "XMLHttpRequest";
        }

    }
}