using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RSToolKit.WebUI.Infrastructure.Register
{
    /// <summary>
    /// Holds information about page numbers.
    /// </summary>
    public static class PageNumber
    {
        public static int Review = -1;
        public static int Promotions = -2;
        public static int CheckOut = -3;
        public static int Confirmation = -4;
        public static int NoPage = 0;
        public static int RSVP = 1;
        public static int Audience = 2;
    }
}