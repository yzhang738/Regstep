using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RSToolKit.WebUI.Models.Views.Register
{
    /// <summary>
    /// Holds information for the confirmation view on a registration.
    /// </summary>
    public class ConfirmationView
        : BaseRegisterView
    {
        /// <summary>
        /// The name of the form.
        /// </summary>
        public string FormName { get; set; }
        /// <summary>
        /// Flag to show shopping cart.
        /// </summary>
        public bool ShowShoppingCart { get; set; }
        /// <summary>
        /// The notice for the confirmation page.
        /// </summary>
        public HtmlString Notice { get; set; }
        /// <summary>
        /// The invoice html.
        /// </summary>
        public HtmlString Invoice { get; set; }
        /// <summary>
        /// Registrant's confirmation.
        /// </summary>
        public string RegistrantConfirmation { get; set; }
        /// <summary>
        /// Registrant's email.
        /// </summary>
        public string RegistrantEmail { get; set; }
        /// <summary>
        /// Holds the html for the summary of the registration.
        /// </summary>
        public HtmlString Summary { get; set; }
        /// <summary>
        /// The form badge.
        /// </summary>
        public HtmlString FormBadge { get; set; }
        /// <summary>
        /// The status of the registration.
        /// </summary>
        public RSToolKit.Domain.Entities.RegistrationStatus RegistrantStatus { get; set; }
        /// <summary>
        /// The id of the form.
        /// </summary>
        public long FormId { get; set; }

        /// <summary>
        /// Initializes the class
        /// </summary>
        public ConfirmationView()
        {
            Title = "Confirmation";
        }
    }
}