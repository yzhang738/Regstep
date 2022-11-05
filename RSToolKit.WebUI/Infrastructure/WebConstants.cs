using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Drawing;
using RSToolKit.Domain.Data.Info;
using System.Collections.Concurrent;

namespace RSToolKit.WebUI.Infrastructure
{
    public static class WebConstants
    {
        public static Color Excel_HeaderColor;
        public static Color Excel_StripeColor;
        public static ConcurrentDictionary<string, Progress> ProgressStati;

        static WebConstants()
        {
            Excel_HeaderColor = Color.FromArgb(51, 51, 51);
            Excel_StripeColor = Color.FromArgb(221, 221, 221);
            ProgressStati = new ConcurrentDictionary<string, Progress>();
        }
    }
}