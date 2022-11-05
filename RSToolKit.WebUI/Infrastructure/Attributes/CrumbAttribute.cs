using System;
using System.Web.Mvc;
using System.Web;

namespace RSToolKit.WebUI.Infrastructure
{
    /// <summary>
    /// Holds the label for the <code>Crumb</code> to be used in place of the action name.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
    public class CrumbLabelAttribute
        : ActionFilterAttribute
    {
        /// <summary>
        /// The private label for the crumb.
        /// </summary>
        private string _label;

        /// <summary>
        /// Initializes the <code>CrumbLabelAttribute</code> with the desired label.
        /// </summary>
        /// <param name="label">The label to use for the <code>Crumb</code></param>
        public CrumbLabelAttribute(string label)
            : this()
        {
            this._label = label;
        }

        /// <summary>
        /// Initializes the <code>CrumbLabelAttribute</code> with the label of _unknown.
        /// </summary>
        public CrumbLabelAttribute()
        {
            this._label = "unknown";
        }

        /// <summary>
        /// Sets the label of the crumb if it exists.
        /// </summary>
        /// <param name="filterContext">The filter context.</param>
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            RSToolKit.Domain.Entities.Clients.Crumb crumb = null;
            if (filterContext.Controller is RSController)
                crumb = ((RSController)filterContext.Controller).Crumb;
            if (crumb == null)
                return;
            crumb.Label = this._label;
        }
    }
}
