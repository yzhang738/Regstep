using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RSToolKit.WebUI.Models.Views.Register
{
    public class PromotionsView
        : BaseRegisterView
    {
        /// <summary>
        /// The html of the current codes.
        /// </summary>
        public HtmlString Codes { get; set; }
        /// <summary>
        /// Flag to skip payments.
        /// </summary>
        public bool SkipPayment { get; set; }
        /// <summary>
        /// The billing options for the form.
        /// </summary>
        public RSToolKit.Domain.Entities.BillingOptions BillingOption { get; set; }
    }
}