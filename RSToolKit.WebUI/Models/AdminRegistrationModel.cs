using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RSToolKit.WebUI.Models
{
    public class AdminRegStart
    {
        public string Email { get; set; }
        public Guid? FormKey { get; set; }

        public AdminRegStart()
        {
            Email = "";
            FormKey = null;
        }
    }

    public class AdminRegNext
    {
        public Guid? RegistrantKey { get; set; }
        public int PageNumber { get; set; }

        public AdminRegNext()
        {
            RegistrantKey = null;
            PageNumber = 0;
        }
    }

    public class AdminRegRSVP
    {
        //bool RSVP, Guid FormKey, Guid RegistrantKey, string Confirmation, int PageNumber, bool Live
        public bool RSVP { get; set; }
        public Guid? RegistrantKey { get; set; }

        public AdminRegRSVP()
        {
            RSVP = true;
            RegistrantKey = null;
        }
    }

    public class AdminRegAudience
    {
        public Guid? Audience { get; set; }
        public Guid? RegistrantKey { get; set; }

        public AdminRegAudience()
        {
            Audience = null;
            RegistrantKey = null;
        }
    }

    public class AdminRegCreditCard
    {
        public Guid RegistrantKey { get; set; }
        public string CardNumber { get; set; }
        public string NameOnCard { get; set; }
        public string ExpMonth { get; set; }
        public string ExpYear { get; set; }
        public string ZipCode { get; set; }
        public string CardCode { get; set; }
        public Dictionary<string, string> Errors { get; set; }

        public string Line1 { get; set; }
        public string Line2 { get; set; }
        public string Country { get; set; }
        public string State { get; set; }
        public string City { get; set; }
        public string Phone { get; set; }

        public string Desc { get; set; }

        public AdminRegCreditCard()
        {
            CardNumber = NameOnCard = ExpMonth = ExpYear = ZipCode = CardCode = Line1 = Line2 = Country = State = City = Phone = "";
            Desc = "";
            Errors = new Dictionary<string, string>();
        }
    }

    public class AdminRegChargeConfirmation
    {
        public Guid RegistrantKey { get; set; }
        public Guid TrId { get; set; }
        public string Email { get; set; }

        public AdminRegChargeConfirmation()
        {
            Email = "";
        }
    }

    public class AdminRegBillMe
    {
        //Guid id, string email, string payingAgentNumber, string payingAgentName
        public Guid RegistrantKey { get; set; }
        public string Email { get; set; }
        public string PayingAgentNumber { get; set; }
        public string PayingAgentName { get; set; }

        public AdminRegBillMe()
        {
            Email = PayingAgentName = PayingAgentNumber = "";
        }
    }

}