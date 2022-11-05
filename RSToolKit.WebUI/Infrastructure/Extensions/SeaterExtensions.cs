using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using RSToolKit.Domain.Data;
using RSToolKit.Domain.Entities;

namespace RSToolKit.WebUI.Infrastructure
{
    /// <summary>
    /// Holds extensions for Seaters.
    /// </summary>
    public static class SeaterExtensions
    {
        /// <summary>
        /// Removes the item from the context.
        /// IMPORTANT: This does not save the changes to the database.
        /// </summary>
        /// <param name="item">The item to remove.</param>
        /// <param name="context">The context the item is attached to.</param>
        /// <returns>The amount of items removed from the context.</returns>
        public static int Delete(this Seater item, EFDbContext context)
        {
            context.Seaters.Remove(item);
            return 1;
        }
    }
}