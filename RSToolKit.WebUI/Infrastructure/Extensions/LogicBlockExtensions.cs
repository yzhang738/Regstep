using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using RSToolKit.Domain.Data;
using RSToolKit.Domain.Entities;

namespace RSToolKit.WebUI.Infrastructure
{
    /// <summary>
    /// Holds extensions for logic blocks.
    /// </summary>
    public static class LogicBlockExtensions
    {
        /// <summary>
        /// Removes the item from the context.
        /// IMPORTANT: This does not save the changes to the database.
        /// </summary>
        /// <param name="item">The item to remove.</param>
        /// <param name="context">The context the item is attached to.</param>
        /// <returns>The amount of items removed from the context.</returns>
        public static int Delete(this LogicBlock item, EFDbContext context)
        {
            var count = 1;
            foreach (var logic in item.Logics.ToList())
                count += logic.Delete(context);
            context.LogicBlocks.Remove(item);
            return count;
        }
    }
}