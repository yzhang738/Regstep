using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using RSToolKit.WebUI.Models.Views;

namespace RSToolKit.WebUI.Models.Views.Unsubscribe
{
    public class UnsubscribeView
        : ViewBase
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public string EmailSend { get; set; }
    }
}