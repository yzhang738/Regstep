using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RSToolKit.Domain.Entities;
using System.Reflection;
using System.Globalization;
using System.Net.Mail;
using System.Collections;

namespace RSToolKit.Domain
{
    public static class Extensions
    {
        #region DateTimeOffset Extensions

        public static double ToEpoch(this DateTimeOffset dto)
        {
            return (dto.UtcDateTime - new DateTime(1970, 1, 1)).TotalSeconds;
        }

        public static double ToEpoch(this DateTimeOffset? dto)
        {
            return ToEpoch(dto.HasValue ? dto.Value : DateTimeOffset.UtcNow);
        }

        #endregion

        #region Generic Extensions

        /// <summary>
        /// Checks to see if the object is one of a list of values.
        /// </summary>
        /// <typeparam name="T">The type being tested.</typeparam>
        /// <param name="obj">The obj being tested.</param>
        /// <param name="args">The objects to check the obj against</param>
        /// <returns>Returns true if it equals one of the args.</returns>
        public static bool In<T>(this T obj, params T[] args)
        {
            return args.Contains(obj);
        }

        /// <summary>
        /// Checks to see if the object is one of a list of values.
        /// </summary>
        /// <typeparam name="T">The type being tested.</typeparam>
        /// <param name="obj">The obj being tested.</param>
        /// <param name="args">The objects to check the obj against</param>
        /// <returns>Returns true if it equals one of the args.</returns>
        public static bool In<T>(this T obj, IEnumerable<T> args)
        {
            return args.Contains(obj);
        }

        /// <summary>
        /// Moves the specified item to the front of the list.
        /// </summary>
        /// <typeparam name="T">The items in the list.</typeparam>
        /// <param name="list">The list.</param>
        /// <param name="item">The item.</param>
        /// <returns>Returns true if the item was found and moved, false otherwise.</returns>
        public static bool MoveToFront<T>(this List<T> list, T item)
        {
            if (!list.Contains(item))
                return false;
            list.Remove(item);
            list.Insert(0, item);
            return true;
        }

        /// <summary>
        /// Checks to see if the string is a valid Guid.
        /// </summary>
        /// <param name="value">The string to check.</param>
        /// <returns>True if yes and false otherwise.</returns>
        public static bool IsGuid(this string value)
        {
            Guid id;
            return Guid.TryParse(value, out id);
        }

        public static string RemovePound(this string value)
        {
            if (value.StartsWith("#"))
            {
                value = value.Remove(0, 1);
            }
            return value;
        }

        public static bool IsEmail(this string value)
        {
            try
            {
                MailAddress m = new MailAddress(value);
                return true;
            }
            catch (FormatException)
            {
                return false;
            }
        }

        public static string GetLast(this string source, int tail_length)
        {
            if (tail_length >= source.Length)
                return source;
            return source.Substring(source.Length - tail_length);
        }

        public static string GetElipse(this string source, int head_length)
        {
            if (String.IsNullOrEmpty(source))
                return "";
            if (head_length >= source.Length)
                return source;
            return source.Substring(0, head_length) + "...";
        }


        public static string First20(this string source)
        {
            if (20 >= source.Length)
                return source;
            return source.Substring(20);
        }

        #endregion

        #region Enum Extensions

        /// <summary>
        /// Gets the StringValueAttribute of the enumeration if it exists.
        /// </summary>
        /// <param name="value">The enum you are getting the string value for.</param>
        /// <returns>Returns the string if it has one or 'NONE' if the property is not set.</returns>
        public static string GetStringValue(this Enum value)
        {
            Type type = value.GetType();
            FieldInfo fieldInfo = type.GetField(value.ToString());
            StringValueAttribute[] attribs = fieldInfo.GetCustomAttributes(typeof(StringValueAttribute), false) as StringValueAttribute[];
            return attribs.Length > 0 ? attribs[0].StringValue : "NONE";
        }

        /// <summary>
        /// Gets the JTableTypeAttribute of the enumeration if it exists.
        /// </summary>
        /// <param name="value">The enum you are getting the jtable type for.</param>
        /// <returns>Returns the string if it has one or 'text' if the property is not set.</returns>
        public static string GetJTableType(this Enum value)
        {
            Type type = value.GetType();
            FieldInfo fieldInfo = type.GetField(value.ToString());
            JTableTypeAttribute[] attribs = fieldInfo.GetCustomAttributes(typeof(JTableTypeAttribute), false) as JTableTypeAttribute[];
            return attribs.Length > 0 ? attribs[0].JTableType : "text";
        }

        /// <summary>
        /// Gets the TestValueAttribute of the enumeration if it exists.
        /// </summary>
        /// <param name="value">The enum you are getting the string value for.</param>
        /// <returns>Returns the string if it has one or '==' if the property is not set.</returns>
        public static string GetTestValue(this Enum value)
        {
            Type type = value.GetType();
            FieldInfo fieldInfo = type.GetField(value.ToString());
            TestValueAttribute[] attribs = fieldInfo.GetCustomAttributes(typeof(TestValueAttribute), false) as TestValueAttribute[];
            return attribs.Length > 0 ? attribs[0].TestValue : "==";
        }


        /// <summary>
        /// Gets the FloatValueAttribute of the enumeration if it exists.
        /// </summary>
        /// <param name="value">The enum you are getting the string value for.</param>
        /// <returns>Returns the float if it has one or 0.5 if the property is not set.</returns>
        public static float GetFloatValue(this Enum value)
        {
            Type type = value.GetType();
            FieldInfo fieldInfo = type.GetField(value.ToString());
            FloatValueAttribute[] attribs = fieldInfo.GetCustomAttributes(typeof(FloatValueAttribute), false) as FloatValueAttribute[];
            return attribs.Length > 0 ? attribs[0].FloatValue : 0.5f;
        }


        /// <summary>
        /// Gets the PayPalValueAttribute of the enumeration if it exists.
        /// </summary>
        /// <param name="value">The enum you are getting the paypal value for.</param>
        /// <returns>Returns the string if it has one or null if the property is not set.</returns>
        public static string GetPayPalValue(this Enum value)
        {
            Type type = value.GetType();
            FieldInfo fieldInfo = type.GetField(value.ToString());
            PayPalValueAttribute[] attribs = fieldInfo.GetCustomAttributes(typeof(PayPalValueAttribute), false) as PayPalValueAttribute[];
            return attribs.Length > 0 ? attribs[0].PayPalValue : "USD";
        }

        /// <summary>
        /// Gets the iPayValueAttribute of the enumeration if it exists.
        /// </summary>
        /// <param name="value">The enum you are getting the paypal value for.</param>
        /// <returns>Returns the string if it has one or null if the property is not set.</returns>
        public static string GetiPayValue(this Enum value)
        {
            Type type = value.GetType();
            FieldInfo fieldInfo = type.GetField(value.ToString());
            iPayValueAttribute[] attribs = fieldInfo.GetCustomAttributes(typeof(iPayValueAttribute), false) as iPayValueAttribute[];
            return attribs.Length > 0 ? attribs[0].iPayValue : "840";
        }



        /// <summary>
        /// Either returns the value of the string or a supplied value if null.
        /// </summary>
        /// <param name="obj">The string extension object.</param>
        /// <param name="value">The value to return if null. Returns empty string if ommited.</param>
        /// <returns>The string if not null or the supplied value or empty string if ommited.</returns>
        public static string IfNull(this String obj, string value = "")
        {
            if (String.IsNullOrEmpty(obj))
                return value;
            return obj;
        }

        public static bool IsForForm(this Enum value)
        {
            Type type = value.GetType();
            FieldInfo fieldInfo = type.GetField(value.ToString());
            FormAttribute[] attribs = fieldInfo.GetCustomAttributes(typeof(FormAttribute), false) as FormAttribute[];
            return attribs.Length > 0 ? attribs[0].BoolValue : false;
        }


        public static string GetTagValue(this Enum value)
        {
            Type type = value.GetType();
            FieldInfo fieldInfo = type.GetField(value.ToString());
            TagValueAttribute[] attribs = fieldInfo.GetCustomAttributes(typeof(TagValueAttribute), false) as TagValueAttribute[];
            return attribs.Length > 0 ? attribs[0].StringValue : "";
        }

        public static string GetCurrencyFormat(this Enum value)
        {
            Type type = value.GetType();
            FieldInfo fieldInfo = type.GetField(value.ToString());
            CurrencyFormat[] attribs = fieldInfo.GetCustomAttributes(typeof(CurrencyFormat), false) as CurrencyFormat[];
            return attribs.Length > 0 ? attribs[0].CurrencyValue : "";
        }
        
        public static string GetCurrencySymbol(this Enum value)
        {
            Type type = value.GetType();
            FieldInfo fieldInfo = type.GetField(value.ToString());
            CurrencySymbolValueAttribute[] attribs = fieldInfo.GetCustomAttributes(typeof(CurrencySymbolValueAttribute), false) as CurrencySymbolValueAttribute[];
            return attribs.Length > 0 ? attribs[0].CurrencySymbolValue : null;

        }

        public static CultureInfo GetCulture(this Enum value)
        {
            Type type = value.GetType();
            FieldInfo fieldInfo = type.GetField(value.ToString());
            CultureAttribute[] attribs = fieldInfo.GetCustomAttributes(typeof(CultureAttribute), false) as CultureAttribute[];
            return attribs.Length > 0 ? attribs[0].CultureValue : CultureInfo.CurrentCulture;
        }

        public static string GetRgxValue(this Enum value)
        {
            Type type = value.GetType();
            FieldInfo fieldInfo = type.GetField(value.ToString());
            RgxValueAttribute[] attribs = fieldInfo.GetCustomAttributes(typeof(RgxValueAttribute), false) as RgxValueAttribute[];
            return attribs.Length > 0 ? attribs[0].RgxValue : "";
        }

        public static string GetRgxPatternValue(this Enum value)
        {
            Type type = value.GetType();
            FieldInfo fieldInfo = type.GetField(value.ToString());
            RgxPatternAttribute[] attribs = fieldInfo.GetCustomAttributes(typeof(RgxPatternAttribute), false) as RgxPatternAttribute[];
            return attribs.Length > 0 ? attribs[0].RgxPatternValue : "";
        }

        public static string GetRgxErrorValue(this Enum value)
        {
            Type type = value.GetType();
            FieldInfo fieldInfo = type.GetField(value.ToString());
            RgxErrorValueAttribute[] attribs = fieldInfo.GetCustomAttributes(typeof(RgxErrorValueAttribute), false) as RgxErrorValueAttribute[];
            return attribs.Length > 0 ? attribs[0].RgxErrorValue : "";
        }
        

        #endregion

    }
}
