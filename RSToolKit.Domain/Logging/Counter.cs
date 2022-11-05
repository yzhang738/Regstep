using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace RSToolKit.Domain.Logging
{
    /// <summary>
    /// Holds information about a counter in the log.
    /// </summary>
    public class Counter
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
        public Guid Key { get; set; }
        public CountType Type { get; set; }
        public long Count { get; set; }

        /// <summary>
        /// Intializes the class.
        /// </summary>
        public Counter()
        {
            Key = Guid.Empty;
            Type = CountType.Unknown;
            Count = 0;
        }

        /// <summary>
        /// Initializes the class with the specified values.
        /// </summary>
        /// <param name="key">The key that is being counted.</param>
        /// <param name="type">The type the key belongs too.</param>
        public Counter(Guid key, CountType type)
        {
            Key = key;
            Type = type;
        }
    }

    /// <summary>
    /// The type of counting being done.
    /// </summary>
    public enum CountType
    {
        /// <summary>
        /// Unknown.
        /// </summary>
        Unknown = 0,
        /// <summary>
        /// Counting form reports.
        /// </summary>
        FormReport = 1
    }
}
