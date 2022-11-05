﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RSToolKit.WebUI.Infrastructure.Extensions
{
    public static class DateTimeExtensions
    {
        /// <summary>
        /// The regstep date formats.
        /// </summary>
        public static Dictionary<string, string> RegStepDateFormats = new Dictionary<string, string>()
        {
            { "rs_s", "yyyy-M-d h:mm:ss tt" }
        };

        /// <summary>
        /// Gets the datetime based on the user time zone.
        /// </summary>
        /// <param name="datetimeoffset">The datetime offset.</param>
        /// <param name="user">The user that has the time zone.</param>
        /// <param name="stringFormat">The format to use.</param>
        /// <returns>A string representation of the date time offset.</returns>
        public static string UserString(this DateTimeOffset datetimeoffset, Domain.Entities.Clients.User user, string stringFormat = "rs_s")
        {
            var format = "yyyy-M-d h:mm:ss tt";
            if (stringFormat.StartsWith("rs_"))
            {
                if (!RegStepDateFormats.TryGetValue(stringFormat, out format))
                    format = "yyyy-M-d h:mm:ss tt";
            }
            if (user == null)
                datetimeoffset.LocalDateTime.ToString(format);
            var userDate = TimeZoneInfo.ConvertTime(datetimeoffset, user.TimeZone);
            return userDate.ToString(format);
        }
    }
}