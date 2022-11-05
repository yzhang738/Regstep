using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using RSToolKit.Domain.Data;
using RSToolKit.Domain.Entities;

namespace RSToolKit.WebUI.Infrastructure
{
    /// <summary>
    /// Holds extensions for seatings.
    /// </summary>
    public static class SeatingExtensions
    {
        /// <summary>
        /// Removes the item from the context.
        /// IMPORTANT: This does not save the changes to the database.
        /// </summary>
        /// <param name="item">The item to remove.</param>
        /// <param name="context">The context the item is attached to.</param>
        /// <returns>The amount of items removed from the context.</returns>
        public static int Delete(this Seating item, EFDbContext context)
        {
            var count = 1;
            foreach (var seater in item.Seaters.ToList())
                count += seater.Delete(context);
            foreach (var style in item.Styles.ToList())
                count += style.Delete(context);
            context.Seatings.Remove(item);
            return count;
        }

        #region Seating style
        /// <summary>
        /// Removes the item from the context.
        /// IMPORTANT: This does not save the changes to the database.
        /// </summary>
        /// <param name="item">The item to remove.</param>
        /// <param name="context">The context the item is attached to.</param>
        /// <returns>The amount of items removed from the context.</returns>
        public static int Delete(this SeatingStyle item, EFDbContext context)
        {
            context.SeatingStyles.Remove(item);
            return 1;
        }
        #endregion
    }
}