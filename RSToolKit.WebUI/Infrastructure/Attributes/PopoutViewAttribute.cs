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
    public class PopoutViewAttribute
        : ActionFilterAttribute
    {
        protected string _layout;
        protected bool _usePopout;

        /// <summary>
        /// Initializes the attribute with the specified layout.
        /// </summary>
        /// <param name="key"></param>
        public PopoutViewAttribute(string layout)
            : this()
        {
            this._layout = layout;
        }

        /// <summary>
        /// Initializes the attribute with the default layout.
        /// </summary>
        /// <param name="key"></param>
        public PopoutViewAttribute()
            : base()
        {
            this._layout = "~/Views/Shared/_L_Popout.cshtml";
            this._usePopout = false;
        }

        /// <summary>
        /// If we are rendering a minimum page, we change the layout.
        /// </summary>
        /// <param name="filterContext">The current filter context.</param>
        public override void OnResultExecuting(ResultExecutingContext filterContext)
        {
            if (!this._usePopout || this._IsAjax(filterContext.HttpContext))
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
            var popout = filterContext.HttpContext.Request.Params["popout"] ?? "false";
            if (popout != null && popout.ToLower() == "true")
            {
                this._usePopout = true;
                if (filterContext.Controller is RegStepController)
                    (filterContext.Controller as RegStepController).IsPopout = true;
            }
            else
            {
                this._usePopout = false;
                if (filterContext.Controller is RegStepController)
                    (filterContext.Controller as RegStepController).IsPopout = false;
            }
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