using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace RSToolKit.WebUI.Models
{

    public class FormRegError
    {
        public bool hide { get; set; }
        public string message { get; set; }
        public FormRegError()
        {
            hide = true;
            message = "";
        }
    }

    public class RegistrationModel
    {
        public int PageNumber { get; set; }
        public Guid RegistrantKey { get; set; }
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public Dictionary<Guid, string> Components { get; set; }
        public Dictionary<Guid, bool> Waitlistings { get; set; }

        public RegistrationModel()
        {
            Components = new Dictionary<Guid, string>();
            Waitlistings = new Dictionary<Guid, bool>();
        }
    }

    public class CreditCardModel
    {
        public Guid FormKey { get; set; }
        public Guid RegistrantKey { get; set; }
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        [Required]
        public string CardNumber { get; set; }
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        [Required]
        public string NameOnCard { get; set; }
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        [Required]
        public string ExpMonth { get; set; }
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        [Required]
        public string ExpYear { get; set; }
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        [Required]
        public string ZipCode { get; set; }
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        [Required]
        public string CardCode { get; set; }
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string CardType { get; set; }

        public Dictionary<string, string> Errors { get; set; }

        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string Line1 { get; set; }
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string Line2 { get; set; }
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string Country { get; set; }
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string State { get; set; }
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string City { get; set; }
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string Phone { get; set; }

        public string Desc { get; set; }

        public CreditCardModel()
        {
            CardNumber = NameOnCard = ExpMonth = ExpYear = ZipCode = CardCode = Line1 = Line2 = Country = State = City = Phone = "";
            Desc = "";
            Errors = new Dictionary<string, string>();
        }
    }

    public class ChargeConfirmationModel
    {
        public Guid Id { get; set; }
        public Guid TrId { get; set; }
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string Email { get; set; }

        public ChargeConfirmationModel()
        {
            Email = "";
        }
    }
}