using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RSToolKit.Domain.Data;
using System.Linq.Expressions;

namespace RSToolKit.Domain.Security
{
    /// <summary>
    /// Holds methods for manipulating security logs.
    /// </summary>
    public static class SecurityLog
    {
        /// <summary>
        /// Adds a security log entry into the database.
        /// </summary>
        /// <param name="item">The item being accessed.</param>
        /// <param name="user">The secure user accessig the item.</param>
        /// <param name="action">The action being used.</param>
        public static void LogSecurityEntry(this IProtected item, SecureUser user, SecurityAccessType action)
        {
            using (var context = new EFDbContext())
            {
                var log = new SecurityLogEntry()
                {
                    ItemKey = item.UId,
                    UserKey = user.AppUser.Id,
                    AccessTimeStamp = DateTimeOffset.Now,
                    Action = action
                };
                context.SecurityLogEntries.Add(log);
                context.SaveChanges();
            }
        }

        /// <summary>
        /// Gets a list of log entries.
        /// </summary>
        /// <param name="item">The item to get the log entries of.</param>
        /// <param name="search">The search parameters. You can leave out the uid of the <paramref name="item"/>. This will already be applied.</param>
        /// <param name="take">[optional]How many records to take. default (all or -1)</param>
        /// <param name="skip">[optional]How many records to skip. default (none or -1)</param>
        /// <returns>The list of security log entries.</returns>
        public static List<SecurityLogEntry> SearchEntries(this IProtected item, Expression<Func<SecurityLogEntry, bool>> search, int take = -1, int skip = -1)
        {
            using (var context = new EFDbContext())
            {
                var items = context.SecurityLogEntries.Where(i => i.ItemKey == item.UId).Where(search);
                if (skip > 0)
                    items.Skip(skip);
                if (take > 0)
                    items.Take(take);
                var list = items.ToList();
                foreach (var t_item in list)
                {
                    var user = context.Users.Find(t_item.UserKey);
                    if (user == null)
                        t_item.UserName = "User Deleted";
                    else
                        t_item.UserName = user.UserName;
                }
                return list;
            }
        }

    }
}
