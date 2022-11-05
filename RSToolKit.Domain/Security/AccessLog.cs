using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using RSToolKit.Domain.Entities.Clients;
using RSToolKit.Domain.Data;
using System.Security.Cryptography;
using System.IO;
using System.Text.RegularExpressions;
using RSToolKit.Domain.Errors;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;

namespace RSToolKit.Domain.Security
{
    [Table("AccessLog")]
    public class AccessLog
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long SortingId { get; set; }
        public Guid AccessedItem { get; set; }
        public string AccessKey { get; set; }
        public DateTimeOffset AccessTime { get; set; }
        public SecureAccessType AccessType { get; set; }
        public string Note { get; set; }
        [ForeignKey("CompanyKey")]
        public virtual Company Company { get; set; }
        public Guid? CompanyKey { get; set; }
        [ForeignKey("AccessKey")]
        public virtual User AccessUser { get; set; }
        public string Type { get; set; }

        public AccessLog()
        {
            AccessTime = DateTimeOffset.UtcNow;
            AccessType = SecureAccessType.Unknown;
            Note = "";
            Type = "ISecure";
        }

        public static AccessLog New<Titem>(User user, Titem item, SecureAccessType type, Company company, string note = "")
            where Titem : class, ISecure
        {
            var log = new AccessLog()
            {
                AccessedItem = item.UId,
                AccessKey = user != null ? user.Id : null,
                AccessType = type,
                Note = note,
                CompanyKey = company != null ? (Guid?)company.UId : null,
                Type = ObjectContext.GetObjectType(item.GetType()).BaseType.Name
            };
            using (var context = new EFDbContext())
            {
                context.AccessLog.Add(log);
                context.SaveChanges();
            }
            return log;
        }

        /// <summary>
        /// Combines logs for the same items into a list.
        /// </summary>
        /// <param name="list">The list to manipulate.</param>
        /// <returns>A list of list of access logs.</returns>
        public static List<List<AccessLog>> Sort(IEnumerable<AccessLog> list)
        {
            var logs = new List<List<AccessLog>>();
            var distinctIds = list.Select(al => al.AccessedItem).Distinct();
            foreach (var id in distinctIds)
            {
                var listSet = list.Where(al => al.AccessedItem == id).OrderBy(al => al.AccessTime).ToList();
                if (listSet.Count > 0)
                    logs.Add(listSet);
            }
            return logs;
        }

        /// <summary>
        /// Sorts similar access items and places them in a nested dictionary with the securegroup and secure holder and then a list of a list of the accesslog for an item.
        /// </summary>
        /// <param name="list">The list to combine and sort.</param>
        /// <returns>The combined and sorted complex dictionary.</returns>
        public static Dictionary<ISecureGroup, Dictionary<ISecureHolder, List<List<AccessLog>>>> SortAndCombine(IEnumerable<AccessLog> list)
        {
            var dic = new Dictionary<ISecureGroup, Dictionary<ISecureHolder, List<List<AccessLog>>>>();
            using (var rep = new FormsRepository())
            {
                var sorted = Sort(list);
                sorted.ForEach(s =>
                    {
                        var f = s.First();
                        var stringId = f.AccessedItem.ToString();
                        var data = rep.Search<ISecureMap>(m => m.Value == stringId).FirstOrDefault();
                        if (data == null)
                            return;
                        if (!dic.ContainsKey(data.Parent.Holder))
                            dic.Add(data.Parent.Holder, new Dictionary<ISecureHolder, List<List<AccessLog>>>());
                        if (!dic[data.Parent.Holder].ContainsKey((ISecureHolder)data.Parent))
                            dic[data.Parent.Holder].Add((ISecureHolder)data.Parent, new List<List<AccessLog>>());
                        dic[data.Parent.Holder][(ISecureHolder)data.Parent].Add(s);                       
                    });
            }
            return dic;
        }
    }
}
