using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using RSToolKit.Domain.Data;
using RSToolKit.Domain.Entities.Components;

namespace RSToolKit.WebUI.Infrastructure
{
    /// <summary>
    /// Holds extensions for components.
    /// </summary>
    public static class ComponentExtensions
    {
        /// <summary>
        /// Removes the component from the context.
        /// IMPORTANT: This does not save the changes to the database.
        /// </summary>
        /// <param name="component">The component to remove.</param>
        /// <param name="context">The context the item is attached to.</param>
        /// <returns>The amount of items removed from the context.</returns>
        public static int Delete(this Component component, EFDbContext context)
        {
            var count = 1;
            component.Logics.ForEach(l => count += l.Delete(context));
            if (component.Variable != null)
            {
                count++;
                context.Variables.Remove(component.Variable);
            }
            count += component.PriceGroup.Delete(context);
            count += component.Styles.Count;
            context.ComponentStyles.RemoveRange(component.Styles);
            component.Seaters.ForEach(s => count += s.Delete(context));
            if (component is IComponentItemParent)
            {
                foreach (var item in ((IComponentItemParent)component).Children.ToList())
                    count += (item as Component).Delete(context);
            }
            foreach (var style in component.Styles.ToList())
                count += style.Delete(context);
            if (!(component is IComponentItem))
            {
                component.Panel.Components.Remove(component);
                component.Panel.RenumberComponents();
            }
            context.Components.Remove(component);
            return count;
        }

        #region Styles
        /// <summary>
        /// Removes the item from the context.
        /// IMPORTANT: This does not save the changes to the database.
        /// </summary>
        /// <param name="item">The item to remove.</param>
        /// <param name="context">The context the item is attached to.</param>
        /// <returns>The amount of items removed from the context.</returns>
        public static int Delete(this Domain.Entities.ComponentStyle item, EFDbContext context)
        {
            context.ComponentStyles.Remove(item);
            return 1;
        }
        #endregion
    }
}