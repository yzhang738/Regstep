using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using RSToolKit.Domain.Data;
using RSToolKit.Domain.Entities;

namespace RSToolKit.WebUI.Infrastructure
{
    /// <summary>
    /// Holds extensions for a price group and its subsidaries.
    /// </summary>
    public static class PriceGroupExtensions
    {

        #region PriceGroup
        /// <summary>
        /// Removes the item from the context.
        /// IMPORTANT: This does not save the changes to the database.
        /// </summary>
        /// <param name="item">The item to remove.</param>
        /// <param name="context">The context the item is attached to.</param>
        /// <returns>The amount of items removed from the context.</returns>
        public static int Delete(this PriceGroup item, EFDbContext context)
        {
            if (item == null)
                return 0;
            var count = 1;
            foreach (var price in item.Prices.ToList())
                count += price.Delete(context);
            foreach (var style in item.Styles.ToList())
                count += style.Delete(context);
            context.PriceGroups.Remove(item);
            return count;
        }
        #endregion

        #region Prices
        /// <summary>
        /// Removes the item from the context.
        /// IMPORTANT: This does not save the changes to the database.
        /// </summary>
        /// <param name="item">The item to remove.</param>
        /// <param name="context">The context the item is attached to.</param>
        /// <returns>The amount of items removed from the context.</returns>
        public static int Delete(this Prices item, EFDbContext context)
        {
            if (item == null)
                return 0;
            var count = 1;
            foreach (var price in item.Price.ToList())
                count += price.Delete(context);
            context.Prices.Remove(item);
            return count;
        }
        #endregion

        #region Price
        /// <summary>
        /// Removes the item from the context.
        /// IMPORTANT: This does not save the changes to the database.
        /// </summary>
        /// <param name="item">The item to remove.</param>
        /// <param name="context">The context the item is attached to.</param>
        /// <returns>The amount of items removed from the context.</returns>
        public static int Delete(this Price item, EFDbContext context)
        {
            if (item == null)
                return 0;
            context.Price.Remove(item);
            return 1;
        }
        #endregion

        #region Price Styles
        /// <summary>
        /// Removes the item from the context.
        /// IMPORTANT: This does not save the changes to the database.
        /// </summary>
        /// <param name="item">The item to remove.</param>
        /// <param name="context">The context the item is attached to.</param>
        /// <returns>The amount of items removed from the context.</returns>
        public static int Delete(this PriceStyle item, EFDbContext context)
        {
            if (item == null)
                return 0;
            context.PriceStyles.Remove(item);
            return 1;
        }
        #endregion
    }
}