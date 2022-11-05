using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using RSToolKit.Domain.Data;
using RSToolKit.Domain.Entities.Email;

namespace RSToolKit.WebUI.Infrastructure
{
    /// <summary>
    /// Holds extensions for html emails.
    /// </summary>
    public static class HtmlEmailExtensions
    {
        /// <summary>
        /// Removes the item from the context.
        /// IMPORTANT: This does not save the changes to the database.
        /// </summary>
        /// <param name="item">The item to remove.</param>
        /// <param name="context">The context the item is attached to.</param>
        /// <returns>The amount of items removed from the context.</returns>
        public static int Delete(this RSHtmlEmail item, EFDbContext context)
        {
            context.RSHtmlEmails.Remove(item);
            context.EmailSends.Where(e => e.EmailKey == item.UId).ToList().ForEach(e => e.EmailKey = null);
            return 1;
        }
    }
}