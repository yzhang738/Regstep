using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RSToolKit.Domain.Entities.Clients;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using AuthorizeNet;
using RSToolKit.Domain.Data;

namespace RSToolKit.Domain.Entities.MerchantAccount
{
    public class TransactionDetail : IFinanceAmmount, IRegistrantItem
    {
        [ForeignKey("CreditCardKey")]
        public virtual CreditCard CreditCard { get; set; }
        public Guid? CreditCardKey { get; set; }
        [ForeignKey("TransactionRequestKey")]
        public virtual TransactionRequest TransactionRequest { get; set; }
        public Guid TransactionRequestKey { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long SortingId { get; set; }
        [Key]
        public Guid UId { get; set; }
        public Guid? FormKey { get; set; }
        public decimal Ammount { get; set; }
        public decimal ConversionRate { get; set; }
        public bool Approved { get; set; }
        public string AuthorizationCode { get; set; }
        [NotMapped]
        public string CardNumber { get; set; }
        public string InvoiceNumber { get; set; }
        public string Message { get; set; }
        public string ResponseCode { get; set; }
        public string TransactionID { get; set; }
        public string CVVResponse { get; set; }
        public string Response { get; set; }
        public TransactionType TransactionType { get; set; }
        public DateTimeOffset DateCreated { get; set; }
        [NotMapped]
        public string Creator { get; set; }
        [NotMapped]
        public string Name { get; set; }
        [NotMapped]
        public Registrant Registrant
        {
            get
            {
                return TransactionRequest.Registrant;
            }
            set
            {
                return;
            }
        }

        public virtual List<TransactionDetail> Credits { get; set; }

        [ForeignKey("CaptureKey")]
        public virtual TransactionDetail Capture { get; set; }
        public Guid? CaptureKey { get; set; }

        public decimal? TotalAmount()
        {
            return Ammount;
        }

        [NotMapped]
        public bool voided { get; set; }

        public decimal AmmountRecieved()
        {
            var recieved = Ammount;
            foreach (var credit in Credits.Where(c => c.Approved).ToList())
            {
                recieved -= credit.Ammount;
            }
            return recieved;
        }

        public TransactionDetail()
        {
            ConversionRate = 1;
            Credits = new List<TransactionDetail>();
            voided = false;
            SortingId = 0;
            UId = Guid.NewGuid();
            Ammount = 0m;
            Approved = false;
            AuthorizationCode = CardNumber = InvoiceNumber = Message = ResponseCode = TransactionID = "";
            TransactionType = TransactionType.AuthorizeCapture;
            DateCreated = DateTimeOffset.UtcNow;
            Response = "";
        }

        public INode GetNode()
        {
            if (TransactionRequest.Registrant != null)
                return TransactionRequest.Registrant.Form;
            if (TransactionRequest.CompanyKey.HasValue)
                return TransactionRequest.Company;
            return null;
        }

        public Form GetForm()
        {
            return TransactionRequest.Form;
        }

    }
}
