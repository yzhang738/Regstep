using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RSToolKit.WebUI.Models.Views.Register
{
    public class CheckOutView
        : BaseRegisterView
    {
        /// <summary>
        /// Registrant's invoice.
        /// </summary>
        public HtmlString Invoice { get; set; }
        /// <summary>
        /// Flag as to whether payments are being accepted.
        /// </summary>
        public bool Payments { get; set; }
    }
}