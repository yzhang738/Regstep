using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RSToolKit.Domain.Data;

namespace RSToolKit.Domain.Entities.MerchantAccount
{
    public class AuthorizeDotNetProccessInfo : IMerchantAccountProccessInfo
    {

        public decimal Ammount
        {
            get;
            set;
        }
        public int Status
        {
            get;
            set;
        }
        public bool Success
        {
            get;
            set;
        }
        public ServiceMode Mode { get; set; }
        public Dictionary<string, string> Parameters { get; set; }
        public string CardNumber { get; set; }
        public string ExpMonthAndYear { get; set; }
        public string Description { get; set; }
        public TransactionType TransactionType { get; set; }

        // For validation.
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Address { get; set; }
        public string State { get; set; }
        public string Zip { get; set; }
        public string Id { get; set; }
        public Guid Invoice { get; set; }

        // For the response
        public string AuthCode { get; set; }
        public string TransactionId { get; set; }

        public AuthorizeDotNetProccessInfo()
        {
            Parameters = new Dictionary<string, string>();
            Mode = ServiceMode.Test;
            Success = false;
            Ammount = 0.00m;
            CardNumber = "";
            ExpMonthAndYear = "";
            Description = "";
            FirstName = LastName = Address = State = Zip = "";
            Invoice = Guid.Empty;
            AuthCode = "";
            TransactionId = "0";
        }

    }
    public enum TransactionType
    {
        [StringValue("Authorize and Capture")]
        AuthorizeCapture = 0,
        [StringValue("Credit")]
        Credit = 1,
        [StringValue("Authorize")]
        Authorize = 2,
        [StringValue("Capture")]
        Capture = 3,
        [StringValue("Void")]
        Void = 4,
    }
}
