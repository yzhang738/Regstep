using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RSToolKit.WebUI.Models.Views.Register
{
    /// <summary>
    /// Holds the information for the start view on registration.
    /// </summary>
    public class StartView
        : BaseRegisterView
    {
        /// <summary>
        /// The start notice html.
        /// </summary>
        public HtmlString Notice { get; set; }
        /// <summary>
        /// True if form is open, false otherwise.
        /// </summary>
        public bool OpenForm { get; set; }
        /// <summary>
        /// The name of the form.
        /// </summary>
        public string FormName { get; set; }
        /// <summary>
        /// The state of the form.
        /// </summary>
        public string FormState { get; set; }
        /// <summary>
        /// The email to be used.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Initializes the start view with the title "Admin Registration - Start".
        /// </summary>
        public StartView()
            : base()
        {
            Notice = new HtmlString("Please enter the email of the <b>registrant</b> below.<br /><br />Only one registration record is allowed per email. If you are registering on behalf of someone else, please input <b>their email</b> below.<br /><br />When you click <b>Start Registration</b>, you will receive a secure link via email to continue this registration in case you do not complete it now.<br /><br />If you complete this registration, you will receive a confirmation email with a link to your confirmation page.");
            Title = "Admin Registration - Start";
            OpenForm = true;
            FormName = "undefined";
            FormState = "undefined";
        }
    }
}