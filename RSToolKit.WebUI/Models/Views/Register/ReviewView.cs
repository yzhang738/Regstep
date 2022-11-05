using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RSToolKit.WebUI.Models.Views.Register
{
    public class ReviewView
        : BaseRegisterView
    {
        /// <summary>
        /// The summary of the registration.
        /// </summary>
        public HtmlString Summary { get; set; }
    }
}