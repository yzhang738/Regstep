using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace RSToolKit.WebUI.Models.Inputs.Register
{
    public class ChargeInput
    {
        /// <summary>
        /// The key of the registrant.
        /// </summary>
        [Required]
        public Guid RegistrantKey { get; set; }
        /// <summary>
        /// The name as it is on the card.
        /// </summary>
        [Required]
        public string NameOnCard { get; set; }
        /// <summary>
        /// The number on the card.
        /// </summary>
        [Required]
        public string CardNumber { get; set; }
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
        [Required]
        public string ExpMonth { get; set; }
        /// <summary>
        /// Card expiration year.
        /// </summary>
        [Required]
        public string ExpYear { get; set; }
        /// <summary>
        /// Card code.
        /// </summary>
        [Required]
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
        public ChargeInput()
        { }
    }
}