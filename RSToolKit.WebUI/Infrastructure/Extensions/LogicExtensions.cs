using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using RSToolKit.Domain.Data;
using RSToolKit.Domain.Entities;

namespace RSToolKit.WebUI.Infrastructure
{
    /// <summary>
    /// Holds extensions for Logics.
    /// </summary>
    public static class LogicExtensions
    {
        /// <summary>
        /// Removes the item from the context.
        /// IMPORTANT: This does not save the changes to the database.
        /// </summary>
        /// <param name="item">The item to remove.</param>
        /// <param name="context">The context the item is attached to.</param>
        /// <returns>The amount of items removed from the context.</returns>
        public static int Delete(this Logic logic, EFDbContext context)
        {
            var count = 1 + logic.Commands.Count + logic.LogicGroups.Count;
            context.LogicCommands.RemoveRange(logic.Commands);
            foreach (var logicGroup in logic.LogicGroups.ToList())
            {
                count += logicGroup.LogicStatements.Count;
                context.LogicStatements.RemoveRange(logicGroup.LogicStatements);
            }
            context.LogicGroups.RemoveRange(logic.LogicGroups);
            context.Logics.Remove(logic);
            return count;
        }
    }
}