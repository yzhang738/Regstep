using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using RSToolKit.Domain.Data;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using RSToolKit.Domain.Security;

namespace RSToolKit.Domain.Entities.Reports
{
    public class GlobalReport
        : IDataItem, IProtected, IRegScript
    {
        /// <summary>
        /// The id of the data item.
        /// </summary>
        /// <remarks>In an ideal situation, this would be the primary key in the databse.</remarks>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
        /// <summary>
        /// The unique identifier of the item.
        /// </summary>
        /// <remarks>This is a global identifier in providing a unique way to point to any item in the database regardless of the table it is in.</remarks>
        [Index(IsClustered = false, IsUnique = true)]
        public Guid UId { get; set; }
        /// <summary>
        /// The key of the company.
        /// </summary>
        public Guid CompanyKey { get; set; }
        /// <summary>
        /// This is for concurrency checks.
        /// </summary>
        /// <remarks>
        /// This should be decorated with the EF code first attribute <code>[TimeStamp]</code>.
        /// </remarks>
        [Timestamp]
        public byte[] RowVersion { get; set; }
        /// <summary>
        /// The script that will be run.
        /// </summary>
        [MaxLength]
        public string Script { get; set; }
        /// <summary>
        /// A flag for if te item has been deleted.
        /// </summary>
        public bool Deleted { get; set; }
        /// <summary>
        /// The name of the report.
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// If the report is a favorite or not.
        /// </summary>
        public bool Favorite { get; set; }
        /// <summary>
        /// Raw list of forms.
        /// </summary>
        public string RawForms
        {
            get
            {
                return JsonConvert.SerializeObject(Forms);
            }
            set
            {
                Forms = JsonConvert.DeserializeObject<List<long>>(String.IsNullOrEmpty(value) ? "[]" : value);
            }
        }
        /// <summary>
        /// The forms the global report pertains too.
        /// </summary>
        [NotMapped]
        public List<long> Forms { get; set; }
        /// <summary>
        /// Initializes the item.
        /// </summary>
        public GlobalReport()
        {
            UId = Guid.Empty;
            CompanyKey = Guid.Empty;
            Script = String.Empty;
            Favorite = false;
            Forms = new List<long>();
        }
    }
}
