using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using RSToolKit.WebUI.Models.Views;

namespace RSToolKit.WebUI.Models.Views.Register
{
    /// <summary>
    /// Holds view data for the page.
    /// </summary>
    public class PageView
        : BaseRegisterView
    {
        /// <summary>
        /// The list of html panel strings in the propper order.
        /// </summary>
        public List<HtmlString> PanelHtml { get; set; }

        /// <summary>
        /// Initializes the class.
        /// </summary>
        public PageView()
            : base()
        {
            PanelHtml = new List<HtmlString>();
        }
    }
}