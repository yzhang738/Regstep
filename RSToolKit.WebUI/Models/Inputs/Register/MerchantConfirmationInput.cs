using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RSToolKit.WebUI.Models.Inputs.Register
{
    /// <summary>
    /// Holds input information for the charge confirmation form.
    /// </summary>
    public class MerchantConfirmationInput
    {
        /// <summary>
        /// The email to copy the charge too.
        /// </summary>
        public string Email { get; set; }
        /// <summary>
        /// Paying agent's name.
        /// </summary>
        public string PayingAgentName { get; set; }
        /// <summary>
        /// Paying agent's phone number.
        /// </summary>
        public string PayingAgentNumber { get; set; }
        /// <summary>
        /// The registrants key.
        /// </summary>
        public Guid RegistrantKey { get; set; }
    }
}