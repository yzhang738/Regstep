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
    public class TransactionRequest
        : IMerchantAccountProccessInfo, IFinanceAmmount, IRegistrantItem, IRequirePermissions
    {
        [Key]
        public Guid UId { get; set; }

        [NotMapped]
        public string Creator { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long SortingId { get; set; }

        [ForeignKey("RegistrantKey")]
        public virtual Registrant Registrant { get; set; }
        public Guid? RegistrantKey { get; set; }

        [ForeignKey("MerchantAccountKey")]
        public virtual MerchantAccountInfo MerchantAccount { get; set; }
        public Guid? MerchantAccountKey { get; set; }

        [ForeignKey("CreditCardKey")]
        public virtual CreditCard CreditCard { get; set; }
        public Guid? CreditCardKey { get; set; }

        public virtual List<TransactionDetail> Details { get; set; }

        public string Cart { get; set; }

        public virtual Form Form { get; set; }
        public Guid? FormKey { get; set; }

        public decimal Ammount { get; set; }
        public string Confirmation { get; set; }
        [ForeignKey("CompanyKey")]
        public virtual Company Company { get; set; }
        public Guid? CompanyKey { get; set; }
        public TransactionType TransactionType { get; set; }
        public int Status { get; set; }
        public bool Success { get; set; }
        public ServiceMode Mode { get; set; }
        public Currency Currency { get; set; }
        [NotMapped]
        public Dictionary<string, string> Parameters { get; set; }
        public string LastFour { get; set; }
        [NotMapped]
        public string CardNumber { get; set; }
        [NotMapped]
        public string ExpMonthAndYear { get; set; }
        public string Description { get; set; }
        [NotMapped]
        public string CVV { get; set; }
        public string NameOnCard { get; set; }
        [NotMapped]
        public string Phone { get; set; }

        [NotMapped]
        public string Name
        {
            get
            {
                return "Credit Card";
            }
            set
            {
                return;
            }
        }

        [NotMapped]
        public DateTimeOffset Date { get { return DateCreated; } }
        [NotMapped]
        public string CardType { get; set; }

        // For validation.
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Address { get; set; }
        public string Address2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Zip { get; set; }
        public string Country { get; set; }
        public string Id { get; set; }

        // For the response
        public string AuthCode { get; set; }
        public string TransactionId { get; set; }

        public string Permission { get; set; }

        public DateTimeOffset DateCreated { get; set; }
        public DateTimeOffset DateModified { get; set; }

        public Form GetForm()
        {
            return Form;
        }

        public decimal? TotalAmount()
        {
            return TotalAmount(true);
        }

        public decimal? TotalAmount(bool approved = true)
        {
            var ammount = 0.00m;
            var details = Details.Where(d => d.CaptureKey == null).ToList();
            if (approved)
                if (Details.Where(d => d.CaptureKey == null && d.Approved).Count() == 0)
                    return null;
            foreach (var detail in Details.Where(d => d.CaptureKey == null && d.Approved).ToList())
            {
                if (detail.TransactionType == Entities.MerchantAccount.TransactionType.Capture || detail.TransactionType == Entities.MerchantAccount.TransactionType.AuthorizeCapture)
                    ammount += detail.AmmountRecieved();
            }
            return ammount;
        }


        public TransactionRequest()
        {
            Creator = "";
            CardType = null;
            SortingId = 0;
            UId = Guid.NewGuid();
            Address = City = State = Country = Confirmation = "";
            Ammount = 0m;
            TransactionType = TransactionType.AuthorizeCapture;
            Details = new List<TransactionDetail>();
            Permission = "770";
            DateCreated = DateModified = DateTimeOffset.UtcNow;
            CVV = "";
            Parameters = new Dictionary<string,string>();
            NameOnCard = CardNumber = ExpMonthAndYear = Description = CVV = Confirmation = FirstName = LastName = Address = Address2 = City = State = Zip = Country = Id = AuthCode = TransactionId = LastFour = "";
        }

        public INode GetNode()
        {
            if (Registrant != null)
                return Registrant.Form as INode;
            if (Company != null)
                return Company;
            return null;
        }

        public IPermissionHolder GetPermissionHolder()
        {
            return Registrant.Form;
        }
    }
}
