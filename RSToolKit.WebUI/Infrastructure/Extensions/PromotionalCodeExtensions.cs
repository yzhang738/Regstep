using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using RSToolKit.Domain.Data;
using RSToolKit.Domain.Entities;

namespace RSToolKit.WebUI.Infrastructure
{
    /// <summary>
    /// Holds extensions for Promotional codes and it's subsidaries.
    /// </summary>
    public static class PromotionalCodeExtensions
    {
        /// <summary>
        /// Removes the item from the context.
        /// IMPORTANT: This does not save the changes to the database.
        /// </summary>
        /// <param name="item">The item to remove.</param>
        /// <param name="context">The context the item is attached to.</param>
        /// <returns>The amount of items removed from the context.</returns>
        public static int Delete(this PromotionCode item, EFDbContext context)
        {
            var count = 1;
            foreach (var entry in item.Entries.ToList())
                count += entry.Delete(context);
            context.PromotionCodes.Remove(item);
            return count;
        }

        #region Promotion code entries
        /// <summary>
        /// Removes the item from the context.
        /// IMPORTANT: This does not save the changes to the database.
        /// </summary>
        /// <param name="item">The item to remove.</param>
        /// <param name="context">The context the item is attached to.</param>
        /// <returns>The amount of items removed from the context.</returns>
        public static int Delete(this PromotionCodeEntry item, EFDbContext context)
        {
            context.PromotionCodeEntries.Remove(item);
            return 1;
        }
        #endregion
    }
}