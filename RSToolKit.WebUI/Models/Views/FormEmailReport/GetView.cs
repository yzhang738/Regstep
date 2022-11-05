using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using RSToolKit.WebUI.Models.Views;

namespace RSToolKit.WebUI.Models.Views.FormEmailReport
{
    /// <summary>
    /// Holds information for the get view.
    /// </summary>
    public class GetView
        : ViewBase
    {
        /// <summary>
        /// The form key.
        /// </summary>
        public Guid FormKey { get; set; }
        /// <summary>
        /// The form name.
        /// </summary>
        public string FormName { get; set; }

        /// <summary>
        /// Initializes the class.
        /// </summary>
        public GetView()
            : base()
        {
            Title = "Form Email Report";
            FormName = "Email Report";
        }
    }
}