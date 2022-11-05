using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RSToolKit.WebUI.Models.Views.Register
{
    /// <summary>
    /// Holds the basic information for a register view.
    /// </summary>
    public class BaseRegisterView
        : ViewBase
    {
        /// <summary>
        /// The html header.
        /// </summary>
        public HtmlString Header { get; set; }
        /// <summary>
        /// The html footer.
        /// </summary>
        public HtmlString Footer { get; set; }
        /// <summary>
        /// The html shopping cart.
        /// </summary>
        public HtmlString ShoppingCart { get; set; }
        /// <summary>
        /// The html page number.
        /// </summary>
        public HtmlString PageNumber { get; set; }
        /// <summary>
        /// The raw styles to add to the view.
        /// </summary>
        public HtmlString Styles { get; set; }
        /// <summary>
        /// The hidden inputs.
        /// </summary>
        public HtmlString HiddenInputs { get; set; }
        /// <summary>
        /// The registrants id.
        /// </summary>
        public long RegistrantId { get; set; }
        /// <summary>
        /// The page number for the previous page.
        /// </summary>
        public int PreviousPage { get; set; }

        /// <summary>
        /// Initializes with the title "Registration";
        /// </summary>
        public BaseRegisterView()
            : base()
        {
            Title = "Registration";
        }
    }
}