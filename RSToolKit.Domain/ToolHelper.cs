using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RSToolKit.Domain.Entities.Clients;
using System.Globalization;

namespace RSToolKit.Domain
{
    public static class ToolHelper
    {
        public static string ConvertDate_String(DateTimeOffset date, User user)
        {
            return date.ToOffset(user.TimeZone.BaseUtcOffset).DateTime.ToString("ddMMMyyyy HHmm").ToUpper();
        }

        public static string ConvertDate_String(DateTimeOffset date, User user, string format)
        {
            return date.ToOffset(user.TimeZone.BaseUtcOffset).DateTime.ToString(format);
        }

        public static string ConvertDate_String(DateTimeOffset date, User user, CultureInfo culture)
        {
            return date.ToOffset(user.TimeZone.BaseUtcOffset).DateTime.ToString(culture);
        }

        public static string ConvertDate_String(DateTimeOffset date, User user, string format, CultureInfo culture)
        {
            return date.ToOffset(user.TimeZone.BaseUtcOffset).DateTime.ToString(format, culture);
        }
    }
}
