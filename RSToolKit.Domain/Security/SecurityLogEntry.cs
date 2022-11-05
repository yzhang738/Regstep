using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using RSToolKit.Domain.Entities.Clients;

namespace RSToolKit.Domain.Security
{
    /// <summary>
    /// An entry in the security log.
    /// </summary>
    [Table("SecurityLogEntries")]
    public class SecurityLogEntry
    {
        /// <summary>
        /// The key of the item.
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
        /// <summary>
        /// The item that was accessed.
        /// </summary>
        public Guid ItemKey { get; set; }
        /// <summary>
        /// The key of the user doing the modification.
        /// </summary>
        public string UserKey { get; set; }
        /// <summary>
        /// The type of access that was done.
        /// </summary>
        public SecurityAccessType Action { get; set; }
        /// <summary>
        /// The time the item was accessed.
        /// </summary>
        public DateTimeOffset AccessTimeStamp { get; set; }

        /// <summary>
        /// The name of the user who accessed the item.
        /// </summary>
        [NotMapped]
        public string UserName { get; set; }
    }
}
