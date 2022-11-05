using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using RSToolKit.WebUI.Models.Views;

namespace RSToolKit.WebUI.Models.Views.Contact
{
    /// <summary>
    /// Holds information about the contact list view.
    /// </summary>
    public class ListView
        : ViewBase
    {
        /// <summary>
        /// The name to use for the title line.
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// The key to send to request a token.
        /// </summary>
        public Guid TokenRequestKey { get; set; }

        /// <summary>
        /// Initializes the class.
        /// </summary>
        public ListView()
        {
            Title = "Contact List";
        }
    }
}