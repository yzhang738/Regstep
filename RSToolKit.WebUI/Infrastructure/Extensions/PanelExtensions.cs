using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using RSToolKit.Domain.Data;
using RSToolKit.Domain.Entities;

namespace RSToolKit.WebUI.Infrastructure
{
    /// <summary>
    /// Holds extensions for panels.
    /// </summary>
    public static class PanelExtensions
    {
        /// <summary>
        /// Removes the panel from the context.
        /// IMPORTANT: This does not save the changes to the database.
        /// </summary>
        /// <param name="panel">The panel to remove.</param>
        /// <param name="context">The context the item is attached to.</param>
        /// <returns>The amount of items removed from the context.</returns>
        public static int Delete(this Panel panel, EFDbContext context)
        {
            var count = 1;
            foreach (var component in panel.Components.ToList())
                count += component.Delete(context);
            foreach (var logic in panel.Logics.ToList())
                count += logic.Delete(context);
            panel.Page.Panels.Remove(panel);
            panel.Page.RenumberPanels();
            context.Panels.Remove(panel);
            return count;
        }

        /// <summary>
        /// Renumbers the components in the panel.
        /// </summary>
        /// <param name="panel">The panel to work on.</param>
        public static void RenumberComponents(this Panel panel)
        {
            var o_components = panel.Components.OrderBy(c => c.Row).ThenBy(c => c.Order);
            var row = 1;
            var order = 1;
            foreach (var comp in o_components)
            {
                if (comp.Row == row || comp.Row == 0)
                {
                    comp.Order = order++;
                }
                else
                {
                    order = 1;
                    row++;
                    comp.Row = row;
                    comp.Order = order++;
                }
            }
        }
    }
}