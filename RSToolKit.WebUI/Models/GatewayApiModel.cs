using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using RSToolKit.Domain.Entities.MerchantAccount;
using RSToolKit.Domain.Entities;

namespace RSToolKit.WebUI.Models
{
    public class TransactionModel
    {
        public decimal Amount { get; set; }
        public Guid RegistrantKey { get; set; }
        public Guid FormKey { get; set; }
        public Guid CompanyKey { get; set; }
        public string CardNumber { get; set; }
        public string NameOnCard { get; set; }
        public string ZipCode { get; set; }
        public string CardCode { get; set; }
        public string ExpMonth { get; set; }
        public string ExpYear { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Phone { get; set; }
        public string Country { get; set; }
        public string Zip { get; set; }
        public TransactionType TransactionType { get; set; }
        public bool Live { get; set; }
        public string CardType { get; set; }

        public TransactionModel()
        {            
        }
    }
}