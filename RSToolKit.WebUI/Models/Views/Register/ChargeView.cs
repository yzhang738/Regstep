using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RSToolKit.WebUI.Models.Views.Register
{
    /// <summary>
    /// Holds information for the Charge view.
    /// </summary>
    public class ChargeView
        : BaseRegisterView
    {
        /// <summary>
        /// The name as it is on the card.
        /// </summary>
        public string NameOnCard { get; set; }
        /// <summary>
        /// The number on the card.
        /// </summary>
        public string CardNumber { get; set; }
        /// <summary>
        /// Flag to show all possible inputs.
        /// </summary>
        public bool FullInputs { get; set; }
        /// <summary>
        /// The type of card. Should be one of the following.
        /// visa
        /// mastercard
        /// discover
        /// amex
        /// </summary>
        public string CardType { get; set; }
        /// <summary>
        /// Card expiration month.
        /// </summary>
        public string ExpMonth { get; set; }
        /// <summary>
        /// Card expiration year.
        /// </summary>
        public string ExpYear { get; set; }
        /// <summary>
        /// Card code.
        /// </summary>
        public string CardCode { get; set; }
        /// <summary>
        /// Line 1 of address.
        /// </summary>
        public string Line1 { get; set; }
        /// <summary>
        /// Line 2 of address.
        /// </summary>
        public string Line2 { get; set; }
        /// <summary>
        /// The city.
        /// </summary>
        public string City { get; set; }
        /// <summary>
        /// The state or province.
        /// </summary>
        public string State { get; set; }
        /// <summary>
        /// The postal code.
        /// </summary>
        public string ZipCode { get; set; }
        /// <summary>
        /// The country.
        /// </summary>
        public string Country { get; set; }
        /// <summary>
        /// Phone
        /// </summary>
        public string Phone { get; set; }

        /// <summary>
        /// Initializes the class.
        /// </summary>
        public ChargeView()
        {
            FullInputs = false;
        }
    }
}