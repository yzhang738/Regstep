using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RSToolKit.WebUI.Models.Views.Register
{
    /// <summary>
    /// Holds information for the audience view.
    /// </summary>
    public class AudiencesView
        : BaseRegisterView
    {
        /// <summary>
        /// The audiences html.
        /// </summary>
        public HtmlString Audiences { get; set; }
        /// <summary>
        /// The notice text to display on the page.
        /// </summary>
        public HtmlString Notice { get; set; }

        /// <summary>
        /// Initializes the class.
        /// </summary>
        public AudiencesView()
            : base()
        { }
    }
}