using System;
using System.Web.Mvc;
using System.Web;
using System.Linq;
using System.Collections;
using System.Collections.Concurrent;
using System.Reflection;
using RSToolKit.Domain.Data;
using RSToolKit.Domain.Reflection;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RSToolKit.WebUI.Infrastructure
{
    /// <summary>
    /// An attribute that can enumerate through a collection containing <code>IKeepAlive</code> items and remove the ones that have been dead for too long.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = true)]
    public class UpdateAttribute
        : ActionFilterAttribute
    {
        /// <summary>
        /// The name of the property or member that is inherits <code>IDictionary</code> or <code>IList</code>.
        /// </summary>
        public string Collection { get; set; }
        /// <summary>
        /// The interval required to elapse before the <code>IKeepAlive</code> item is killed.
        /// </summary>
        public TimeSpan UpdateInterval { get; set; }
        /// <summary>
        /// The method to use to update the collection.
        /// </summary>
        public string UpdateMethod { get; set; }

        /// <summary>
        /// Initializes the attribute with no <code>Collection</code> and a <code>KillInterval</code> of 90 minutes.
        /// </summary>
        public UpdateAttribute()
            : base()
        {
            UpdateInterval = TimeSpan.FromMinutes(90);
        }

        /// <summary>
        /// Initializes the class with the specified collection name and a <code>KillInterval</code> o f90 minutes.
        /// </summary>
        /// <param name="collection">The data to fill <code>Collection</code> property with.</param>
        /// <param name="updateInterval">The interval in minutes to check for updates.</param>
        /// <param name="updateMethod">The method on the controller to use to update. It must accept an <code>IUpdatable</code> for first parameter.</param>
        public UpdateAttribute(string collection, string updateMethod, double updateInterval = 90)
            : this()
        {
            Collection = collection;
            UpdateMethod = updateMethod;
            UpdateInterval = TimeSpan.FromMinutes(updateInterval);
        }

        /// <summary>
        /// Initializes the class with the specified values.
        /// </summary>
        /// <param name="collection">The data to fill <code>Collection</code> property with.</param>
        /// <param name="killInterval">The data to fill <code>KillInterval</code> property with in minutes.</param>
        public UpdateAttribute(string collection, double updateInterval)
            : this()
        {
            Collection = collection;
            UpdateInterval = TimeSpan.FromMinutes(updateInterval);
        }

        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            base.OnActionExecuted(filterContext);
            if (String.IsNullOrWhiteSpace(Collection))
                throw new InvalidOperationException("The Collection cannot be null or whitespaces..");
            if (String.IsNullOrWhiteSpace(UpdateMethod))
                throw new InvalidOperationException("The UpdateMethod cannot be null or whitespaces.");
            var controller = filterContext.Controller as RegStepController;
            if (controller == null)
                throw new InvalidOperationException("The controller must be derived from RegStepController.");
            var type_controller = controller.GetType();
            var collection = type_controller.GetValue(Collection, controller) as IUpdatableCollection;
            if (collection == null)
                throw new InvalidOperationException("The property specified does not inherit IKeepAliveCollection<IKeepAlive>.");
            var mi_update = type_controller.GetMethod(UpdateMethod);
            if (mi_update == null)
                throw new InvalidOperationException("The UpdateMethod does not exist.");
            var checkDate = DateTime.Now;
            // We get the items that need updated and update them items.
            foreach (var item in collection.Where(i => checkDate.Subtract(i.LastUpdate) > UpdateInterval))
                mi_update.Invoke(controller, new object[] { item });
        }
    }
}