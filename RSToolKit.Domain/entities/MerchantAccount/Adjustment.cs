using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using RSToolKit.Domain.Data;
using RSToolKit.Domain.Entities.Clients;

namespace RSToolKit.Domain.Entities.MerchantAccount
{
    public class Adjustment : IRSData, IFinanceAmmount, IRegistrantItem
    {
        [ForeignKey("Registrant")]
        public Guid UId { get; set; }

        [Key]
        public long SortingId { get; set; }

        public virtual Registrant Registrant { get; set; }

        public DateTimeOffset DateCreated { get; set; }
        public DateTimeOffset DateModified { get; set; }

        public Guid ModifiedBy { get; set; }
        public Guid ModificationToken { get; set; }

        public string Name { get; set; }

        public string AdjustmentType { get; set; }
        public decimal Amount { get; set; }
        public string TransactionId { get; set; }

        public bool Voided { get; set; }

        public DateTimeOffset? TransactionDate { get; set; }
        public string VoidedBy { get; set; }

        [NotMapped]
        public DateTimeOffset Date
        {
            get
            {
                if (TransactionDate.HasValue)
                    return TransactionDate.Value;
                return DateCreated;
            }
        }

        [NotMapped]
        public string Creator { get; set; }

        public decimal? TotalAmount()
        {
            return Amount;
        }

        public Adjustment()
        {
            Amount = 0.00m;
            ModifiedBy = Guid.Empty;
            ModificationToken = Guid.Empty;
            Name = AdjustmentType =  "Adjustment";
            DateModified = DateCreated = DateTimeOffset.UtcNow;
            TransactionId = "";
            TransactionDate = null;
            Voided = false;
            VoidedBy = "";
        }

        public static Adjustment New(FormsRepository repository, Registrant registrant, User user, string type, string name, decimal amount, DateTimeOffset? date = null, Guid? owner = null, Guid? group = null, string permission = "770")
        {
            var node = new Adjustment()
            {
                Registrant = registrant,
                Amount = amount,
                Name = name,
                AdjustmentType = type
            };
            if (date.HasValue)
            {
                node.TransactionDate = date;
                node.DateCreated = node.DateModified = date.Value;
            }
            registrant.Adjustments.Add(node);
            repository.Commit();
            return node;
        }

        public INode GetNode()
        {
            return Registrant.Form as INode;
        }

        public Form GetForm()
        {
            return Registrant.Form;
        }
    }
}
