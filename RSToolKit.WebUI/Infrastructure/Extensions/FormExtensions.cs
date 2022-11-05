using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using RSToolKit.Domain.Entities;
using RSToolKit.Domain.Data;

namespace RSToolKit.WebUI.Infrastructure
{
    /// <summary>
    /// Holds extensions for forms.
    /// </summary>
    public static class FormExtensions
    {
        /// <summary>
        /// Removes the form from the context.
        /// IMPORTANT: This does not save the changes to the database.
        /// </summary>
        /// <param name="form">The form to remove.</param>
        /// <param name="context">The context the item is attached to.</param>
        /// <returns>The amount of items removed from the context.</returns>
        public static int Delete(this Form form, EFDbContext context)
        {
            var count = 1;
            foreach (var registrant in form.Registrants.ToList())
                count += registrant.Delete(context);
            if (form.TineyUrl != null)
            {
                count++;
                context.TinyUrls.Remove(form.TineyUrl);
            }
            foreach (var email in form.Emails.ToList())
                count += email.Delete(context);
            foreach (var logicBlock in form.LogicBlocks.ToList())
                count += logicBlock.Delete(context);
            foreach (var page in form.Pages.ToList())
                count += page.Delete(context);
            foreach (var audience in form.Audiences.ToList())
                count += audience.Delete(context);
            foreach (var seating in form.Seatings.ToList())
                count += seating.Delete(context);
            foreach (var customText in form.CustomTexts.ToList())
                count += customText.Delete(context);
            foreach (var formStyle in form.FormStyles.ToList())
                count += formStyle.Delete(context);
            foreach (var defaultComponentOrder in form.DefaultComponentOrders.ToList())
                count += defaultComponentOrder.Delete(context);
            foreach (var promotionalCode in form.PromotionalCodes.ToList())
                count += promotionalCode.Delete(context);
            foreach (var email in form.HtmlEmails.ToList())
                count += email.Delete(context);
            foreach (var customReport in form.CustomReports.ToList())
                count += customReport.Delete(context);
            foreach (var advancedInventoryReport in form.InventoryReports.ToList())
                count += advancedInventoryReport.Delete(context);
            context.EmailSends.Where(e => e.FormKey == form.UId).ToList().ForEach(e =>
            {
                e.FormKey = null; e.Form = null;
            });
            context.Pointers.RemoveRange(context.Pointers.Where(p => p.Target == form.UId).ToList());
            context.Forms.Remove(form);
            return count;
        }

        /// <summary>
        /// Renumbers the pages of the form.
        /// </summary>
        /// <param name="form">The form working on.</param>
        public static void RenumberPages(this Form form)
        {
            var o_pages = form.Pages.Where(p => p.Type == PageType.UserDefined).OrderBy(p => p.PageNumber);
            var start = 3;
            if (form.Survey)
                start = 1;
            foreach (var page in o_pages)
                page.PageNumber = start++;
        }

        #region Form styles
        /// <summary>
        /// Removes the item from the context.
        /// IMPORTANT: This does not save the changes to the database.
        /// </summary>
        /// <param name="item">The item to remove.</param>
        /// <param name="context">The context the item is attached to.</param>
        /// <returns>The amount of items removed from the context.</returns>
        public static int Delete(this FormStyle item, EFDbContext context)
        {
            context.FormStyles.Remove(item);
            return 1;
        }
        #endregion

        #region Default component order
        /// <summary>
        /// Removes the item from the context.
        /// IMPORTANT: This does not save the changes to the database.
        /// </summary>
        /// <param name="item">The item to remove.</param>
        /// <param name="context">The context the item is attached to.</param>
        /// <returns>The amount of items removed from the context.</returns>
        public static int Delete(this Domain.Entities.Components.DefaultComponentOrder item, EFDbContext context)
        {
            context.DefaultComponentOrders.Remove(item);
            return 1;
        }
        #endregion
    }
}
