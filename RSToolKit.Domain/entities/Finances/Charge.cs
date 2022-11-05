using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using RSToolKit.Domain.Data;
using RSToolKit.Domain.Entities.Clients;
using RSToolKit.Domain.Security;
using Newtonsoft.Json;

namespace RSToolKit.Domain.Entities.Finances
{
    /// <summary>
    /// A charge that was placed.
    /// </summary>
    public class Charge
        : ICharge, IGloballyProtected, IDataItem
    {
        /// <summary>
        /// The id of the data item.
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
        /// <summary>
        /// The unique identifier of the item.
        /// </summary>
        [Index(IsClustered = false)]
        public Guid UId { get; set; }
        /// <summary>
        /// The date the item was created.
        /// </summary>
        public DateTimeOffset DateCreated { get; set; }
        /// <summary>
        /// The date of the last modification.
        /// </summary>
        public DateTimeOffset DateModified { get; set; }
        /// <summary>
        /// The type of charge.
        /// </summary>
        public ChargeType Type { get; set; }
        /// <summary>
        /// The amount of the transaction.
        /// </summary>
        public decimal Amount { get; set; }
        /// <summary>
        /// The description of the transaction.
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// The date the transaction was initiated.
        /// </summary>
        public DateTimeOffset Date { get; set; }
        /// <summary>
        /// The id of the form the charge applies to.
        /// </summary>
        [Index(IsClustered = false)]
        public long FormKey { get; set; }
        /// <summary>
        /// The key of the company that the charge belongs to.
        /// </summary>
        [ForeignKey("Company")]
        public Guid CompanyKey { get; set; }
        /// <summary>
        /// The company that the charge is applied to.
        /// </summary>
        [JsonIgnore]
        public virtual Company Company {get;set;}
        public bool Deleted { get; set; }
        public string Name { get; set; }

        /// <summary>
        /// This is for concurrency checks.
        /// </summary>
        [Timestamp]
        [JsonIgnore]
        public byte[] RowVersion { get; set; }

        // Properties that are not mapped.
        /// <summary>
        /// The date the transaction was settled.
        /// </summary>
        [NotMapped]
        public DateTimeOffset SettledDate { get { return Date; } }

        /// <summary>
        /// Initializes the class.
        /// </summary>
        public Charge()
        {
            this._Initialize();
        }

        /// <summary>
        /// Initializes properties and fields.
        /// </summary>
        protected void _Initialize()
        {
        }

        /// <summary>
        /// Operations that run just after the class is loaded from the database.
        /// </summary>
        public void PostLoadOperations(EFDbContext context)
        { }

        /// <summary>
        /// Operations that run just before saving.
        /// </summary>
        public void PreSaveOperations(EFDbContext context)
        { }

        #region Explicit interface declaration
        #region IFinanceAmount
        /// <summary>
        /// If the finance is a gain or loss.
        /// </summary>
        FinanceType IFinanceAction.Type { get; }
        #endregion
        #endregion
    }
}
