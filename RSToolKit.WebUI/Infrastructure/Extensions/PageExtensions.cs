using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using RSToolKit.Domain.Data;
using RSToolKit.Domain.Entities;

namespace RSToolKit.WebUI.Infrastructure
{
    /// <summary>
    /// Holds extensions for pages.
    /// </summary>
    public static class PageExtensions
    {
        /// <summary>
        /// Removes the page from the context.
        /// IMPORTANT: This does not save the changes to the database.
        /// </summary>
        /// <param name="page">The page to remove.</param>
        /// <param name="context">The context the item is attached to.</param>
        /// <returns>The amount of items removed from the context.</returns>
        public static int Delete(this Page page, EFDbContext context)
        {
            var count = 1;
            foreach (var panel in page.Panels.ToList())
                count += panel.Delete(context);
            foreach (var logic in page.Logics.ToList())
                count += logic.Delete(context);
            page.Form.Pages.Remove(page);
            page.Form.RenumberPages();
            context.Pages.Remove(page);

            return count;
        }

        /// <summary>
        /// Renumbers the panels in the page.
        /// </summary>
        /// <param name="page">The page to work on.</param>
        public static void RenumberPanels(this Page page)
        {
            var o_panels = page.Panels.OrderBy(p => p.Order);
            var start = 1;
            foreach (var panel in o_panels)
                panel.Order = start++;
        }
    }
}