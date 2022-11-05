using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using RSToolKit.Domain.Data;
using RSToolKit.Domain.Entities;

namespace RSToolKit.WebUI.Infrastructure
{
    /// <summary>
    /// Holds extensions for single form reports.
    /// </summary>
    public static class SingleFormReportExtensions
    {
        /// <summary>
        /// Removes the item from the context.
        /// IMPORTANT: This does not save the changes to the database.
        /// </summary>
        /// <param name="item">The item to remove.</param>
        /// <param name="context">The context the item is attached to.</param>
        /// <returns>The amount of items removed from the context.</returns>
        public static int Delete(this SingleFormReport item, EFDbContext context)
        {
            var count = 1;
            foreach (var filter in item.Filters.ToList())
                count += filter.Delete(context);
            context.SingleFormReports.Remove(item);
            return count;
        }

        #region Query filters
        /// <summary>
        /// Removes the item from the context.
        /// IMPORTANT: This does not save the changes to the database.
        /// </summary>
        /// <param name="item">The item to remove.</param>
        /// <param name="context">The context the item is attached to.</param>
        /// <returns>The amount of items removed from the context.</returns>
        public static int Delete(this QueryFilter item, EFDbContext context)
        {
            context.QueryFilters.Remove(item);
            return 1;
        }
        #endregion
    }
}