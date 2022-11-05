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

namespace RSToolKit.WebUI.Infrastructure
{
    /// <summary>
    /// An attribute that can enumerate through a collection containing <code>IKeepAlive</code> items and remove the ones that have been dead for too long.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = true)]
    public class KeepAliveAttribute
        : ActionFilterAttribute
    {
        /// <summary>
        /// The name of the property or member that is inherits <code>IDictionary</code> or <code>IList</code>.
        /// </summary>
        public string Collection { get; set; }
        /// <summary>
        /// The interval required to elapse before the <code>IKeepAlive</code> item is killed.
        /// </summary>
        public TimeSpan KillInterval { get; set; }

        /// <summary>
        /// Initializes the attribute with no <code>Collection</code> and a <code>KillInterval</code> of 90 minutes.
        /// </summary>
        public KeepAliveAttribute()
            : base()
        {
            KillInterval = TimeSpan.FromMinutes(90);
        }

        /// <summary>
        /// Initializes the class with the specified collection name and a <code>KillInterval</code> o f90 minutes.
        /// </summary>
        /// <param name="collection">The data to fill <code>Collection</code> property with.</param>
        public KeepAliveAttribute(string collection)
            : this()
        {
            Collection = collection;
        }

        /// <summary>
        /// Initializes the class with the specified values.
        /// </summary>
        /// <param name="collection">The data to fill <code>Collection</code> property with.</param>
        /// <param name="killInterval">The data to fill <code>KillInterval</code> property with in minutes.</param>
        public KeepAliveAttribute(string collection, double killInterval)
            : this()
        {
            Collection = collection;
            KillInterval = TimeSpan.FromMinutes(killInterval);
        }

        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            base.OnActionExecuted(filterContext);
            if (Collection == null)
                throw new ArgumentNullException("Collection", "The Collection must be set in order for the attribute to work.");
            var controller = filterContext.Controller;
            var type_controller = controller.GetType();
            var ut_collection = type_controller.GetValue(Collection, controller);
            var collection = (IKeepAliveCollection)ut_collection;
            if (collection == null)
                throw new InvalidOperationException("The property specified does not inherit IKeepAliveCollection<IKeepAlive>.");
            var checkDate = DateTime.Now;
            // We get the dead items.
            lock (collection.SyncRoot)
            {
                var keepAliveItems = collection.Where(i => checkDate.Subtract(i.LastActivity) > KillInterval).ToList();
                // We have the collection and the items that need killed. Now we run through the dead items and remove them.
                keepAliveItems.ForEach(i => collection.Remove(i));
            }
        }
    }
}