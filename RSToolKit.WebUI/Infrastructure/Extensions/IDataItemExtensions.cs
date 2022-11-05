using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using RSToolKit.Domain.Data;
using RSToolKit.Domain.Entities.Manipulations;
using RSToolKit.Domain.Security;

namespace RSToolKit.WebUI.Infrastructure.Extensions
{
    /// <summary>
    /// Holds extension methods for IDataItem.
    /// </summary>
    public static class IDataItemExtensions
    {
        /// <summary>
        /// Gets the access items of a data item based on the dates supplied.
        /// </summary>
        /// <param name="item">The item data in question.</param>
        /// <param name="minDate">The minimum data.</param>
        /// <param name="maxDate">The maximum data.</param>
        /// <param name="minInclusive">Flag for minimum date to be inclusive.</param>
        /// <param name="maxInclusive">Flag for maximum date to be inclusive.</param>
        /// <returns>The list of items.</returns>
        public static IEnumerable<ItemAccess> GetAccess(this IDataItem item, DateTimeOffset? minDate = null, bool minInclusive = true, DateTimeOffset? maxDate = null, bool maxInclusive = true, ItemAccessTypes? accessType = null, string userId = "")
        {
            IEnumerable<ItemAccess> items;
            using (var context = new EFDbContext())
            {
                var t_items = context.ItemAccess.Where(i => i.Target == item.UId);

                if (minDate.HasValue)
                {
                    if (minInclusive)
                        t_items = t_items.Where(i => i.Date >= minDate.Value);
                    else
                        t_items = t_items.Where(i => i.Date > minDate.Value);
                }

                if (maxDate.HasValue)
                {
                    if (maxInclusive)
                        t_items = t_items.Where(i => i.Date <= maxDate.Value);
                    else
                        t_items = t_items.Where(i => i.Date < maxDate.Value);
                }

                if (accessType.HasValue)
                    t_items = t_items.Where(i => i.AccessType == accessType.Value);

                if (userId != null)
                    t_items = t_items.Where(i => i.Subject == userId);

                items = t_items.ToList();
            }
            return items;
        }

        /// <summary>
        /// Creates a new access item and stores it in the database.
        /// </summary>
        /// <param name="item">The item being accessed.</param>
        /// <param name="accessType">The type of access.</param>
        /// <param name="userId">The user id of the person accessing it.</param>
        /// <returns>The ItemAccess saved in the databse.</returns>
        public static ItemAccess CreateAccess(this IDataItem item, ItemAccessTypes accessType, string userId)
        {
            var accessItem = new ItemAccess()
            {
                Target = item.UId,
                Subject = userId,
                Date = DateTimeOffset.Now,
                AccessType = accessType
            };
            using (var context = new EFDbContext())
            {
                context.ItemAccess.Add(accessItem);
                context.SaveChanges();
            }
            return accessItem;
        }
    }
}