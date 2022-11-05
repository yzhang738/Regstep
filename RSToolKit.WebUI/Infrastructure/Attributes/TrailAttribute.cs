using System;
using System.Web.Mvc;
using System.Web;
using RSToolKit.Domain.Entities.Navigation;

namespace RSToolKit.WebUI.Infrastructure
{
    /// <summary>
    /// Holds the label for the <code>Pheromone</code> to be used in place of the action name.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
    public class TrailAttribute
        : ActionFilterAttribute
    {
        /// <summary>
        /// The private label for the <code>Pheromone</code>.
        /// </summary>
        private string _label;

        /// <summary>
        /// Initializes the <code>TrailAttribute</code> with the desired label.
        /// </summary>
        /// <param name="label">The label to use for the <code>Pheromone</code></param>
        public TrailAttribute(string label)
            : this()
        {
            this._label = label;
        }

        /// <summary>
        /// Initializes the <code>TrailAttribute</code> with the label of unknown.
        /// </summary>
        public TrailAttribute()
        {
            this._label = "unknown";
        }

        /// <summary>
        /// Sets the label of the <code>Pheromone</code> if it exists.
        /// </summary>
        /// <param name="filterContext">The filter context.</param>
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            Pheromone pheromone = null;
            if (filterContext.Controller is RegStepController)
                pheromone = ((RegStepController)filterContext.Controller).Pheromone;
            if (pheromone == null)
                return;
            pheromone.Label = this._label;
            ((RegStepController)filterContext.Controller).UpdateTrail();
        }
    }
}
