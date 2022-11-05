using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using RSToolKit.WebUI.Models.Views;

namespace RSToolKit.WebUI.Models.Views.Register
{
    /// <summary>
    /// Holds information for the rsvp view.
    /// </summary>
    public class RSVPView
        : BaseRegisterView
    {
        /// <summary>
        /// True if the first time registering.
        /// </summary>
        public bool First { get; set; }
        /// <summary>
        /// The rsvp html.
        /// </summary>
        public HtmlString RSVP { get; set; }
        /// <summary>
        /// The notice text to display on the page.
        /// </summary>
        public HtmlString Notice { get; set; }

        /// <summary>
        /// Initializes the class.
        /// </summary>
        public RSVPView()
            : base()
        { }
    }
}