using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using RSToolKit.Domain.Data;
using RSToolKit.Domain.Entities.Email;

namespace RSToolKit.WebUI.Infrastructure
{
    /// <summary>
    /// Holds extensions for RS emails and it's subsidaries.
    /// </summary>
    public static class RSEmailExtensions
    {
        #region RSEmail
        /// <summary>
        /// Removes the item from the context.
        /// IMPORTANT: This does not save the changes to the database.
        /// </summary>
        /// <param name="item">The item to remove.</param>
        /// <param name="context">The context the item is attached to.</param>
        /// <returns>The amount of items removed from the context.</returns>
        public static int Delete(this RSEmail item, EFDbContext context)
        {
            var count = 1;
            foreach (var emailArea in item.EmailAreas.ToList())
                count += emailArea.Delete(context);
            foreach (var emailVariable in item.Variables.ToList())
                count += emailVariable.Delete(context);
            foreach (var logic in item.Logics.ToList())
                count += logic.Delete(context);
            context.EmailSends.Where(e => e.EmailKey == item.UId).ToList().ForEach(e => e.EmailKey = null);
            context.RSEmails.Remove(item);
            return count;           
        }
        #endregion

        #region RSEmail Areas
        /// <summary>
        /// Removes the item from the context.
        /// IMPORTANT: This does not save the changes to the database.
        /// </summary>
        /// <param name="item">The item to remove.</param>
        /// <param name="context">The context the item is attached to.</param>
        /// <returns>The amount of items removed from the context.</returns>
        public static int Delete(this EmailArea item, EFDbContext context)
        {
            var count = 1;
            foreach (var variable in item.Variables.ToList())
                count += variable.Delete(context);
            context.EmailAreas.Remove(item);
            return count;
        }
        #endregion

        #region RSEmail Variable
        /// <summary>
        /// Removes the item from the context.
        /// IMPORTANT: This does not save the changes to the database.
        /// </summary>
        /// <param name="item">The item to remove.</param>
        /// <param name="context">The context the item is attached to.</param>
        /// <returns>The amount of items removed from the context.</returns>
        public static int Delete(this EmailVariable item, EFDbContext context)
        {
            var count = 1;
            context.EmailVariables.Remove(item);
            return count;
        }
        #endregion

        #region RSEmail Area Variable
        /// <summary>
        /// Removes the item from the context.
        /// IMPORTANT: This does not save the changes to the database.
        /// </summary>
        /// <param name="item">The item to remove.</param>
        /// <param name="context">The context the item is attached to.</param>
        /// <returns>The amount of items removed from the context.</returns>
        public static int Delete(this EmailAreaVariable item, EFDbContext context)
        {
            context.EmailAreaVariables.Remove(item);
            return 1;
        }
        #endregion
    }
}