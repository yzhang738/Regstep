using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using RSToolKit.Domain.Entities.Clients;
using RSToolKit.Domain.Data;

namespace RSToolKit.Domain.Logging
{
    public class Log : IRSData
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long SortingId { get; set; }
        [Required]
        public DateTimeOffset DateCreated { get; set; }
        public DateTimeOffset DateModified { get; set; }
        [Required]
        [MaxLength(255)]
        public string Thread { get; set; }
        [Required]
        [MaxLength(50)]
        public string Level { get; set; }
        [Required]
        [MaxLength(255)]
        public string Logger { get; set; }
        [Required]
        [MaxLength(4000)]
        public string Message { get; set; }
        [Required]
        [MaxLength(4000)]
        public string Exception { get; set; }
        [ForeignKey("UserKey")]
        public virtual User User { get; set; }
        public string UserKey { get; set; }
        [ForeignKey("CompanyKey")]
        public virtual Company Company { get; set; }
        public Guid? CompanyKey { get; set; }
        public virtual List<LogNote> LogNotes { get; set; }
        public Guid ModificationToken { get; set; }
        public Guid ModifiedBy { get; set; }
        [NotMapped]
        public Guid UId
        {
            get { return Guid.Empty; }
            set { return; }
        }
        [NotMapped]
        public string Name
        {
            get
            {
                return Message;
            }
            set
            {
                return;
            }
        }

        public Log()
        {
            LogNotes = new List<LogNote>();
            Exception = "No Call Stack";
        }
    }

    public class LogNote
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
        [ForeignKey("LogKey")]
        public virtual Log Log { get; set; }
        public long LogKey { get; set; }
        [Required]
        [MaxLength(4000)]
        public string Note { get; set; }
        public LogStatus Status { get; set; }
        [ForeignKey("UserKey")]
        public virtual User User { get; set; }
        public string UserKey { get; set; }
        public DateTimeOffset Date { get; set; }

        public LogNote()
        {
            Status = LogStatus.Unresolved;
        }

        public static LogNote New(Log log, string note, LogStatus status, User user)
        {
            var t_note = new LogNote()
            {
                Log = log,
                Note = note,
                Status = status,
                User = user,
                Date = DateTimeOffset.Now
            };
            return t_note;
        }
    }

    public enum LogStatus
    {
        [StringValue("Unresolved")]
        Unresolved = 0,
        [StringValue("Committed")]
        Committed = 1,
        [StringValue("Resolved")]
        Resolved = 2
    }

    public class LogThreadComparer : IEqualityComparer<Log>
    {

        public bool Equals(Log x, Log y)
        {
            return x.Thread.Equals(y.Thread);
        }

        public int GetHashCode(Log obj)
        {
            return obj.Thread.GetHashCode();
        }
    }

    public class LogLoggerComparer : IEqualityComparer<Log>
    {

        public bool Equals(Log x, Log y)
        {
            return x.Logger.Equals(y.Logger);
        }

        public int GetHashCode(Log obj)
        {
            return obj.Logger.GetHashCode();
        }
    }

    public class LogUserKeyComparer : IEqualityComparer<Log>
    {

        public bool Equals(Log x, Log y)
        {
            if (x.UserKey == null && y.UserKey != null)
                return false;
            if (y.UserKey == null && x.UserKey != null)
                return false;
            if (x.UserKey == null && y.UserKey == null)
                return true;
            return x.UserKey.Equals(y.UserKey);
        }

        public int GetHashCode(Log obj)
        {
            if (obj.UserKey != null)
                return obj.UserKey.GetHashCode();
            return -1;
        }
    }
}
