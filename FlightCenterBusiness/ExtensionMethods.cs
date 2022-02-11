using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FlightCenterBusiness
{
    [Serializable]
    public static class ExtensionMethods
    {
        public static string ConvertDateTimeToString(this object subject, string propertyName)
        {
            // Get the property value of the object
            object propertyValue = subject.GetType().GetProperty(propertyName).GetValue(subject, null);

            if (propertyValue == null)
                return "";

            DateTime datetime = (DateTime)propertyValue;

            return datetime.ToString("yyyy/MM/dd");
        }

        public static object GetReflectedPropertyValue(this object subject, string propertyName)
        {
            // Get the property value of the object
            object reflectedValue = subject.GetType().GetProperty(propertyName).GetValue(subject, null);

            Type type = subject.GetType().GetProperty(propertyName).PropertyType;
            if (type == typeof(int))
            {
                return Convert.ToInt32(reflectedValue);
            }
            //else if (type == typeof(bool))
            //{
            //    return Convert.ToBoolean(reflectedValue);
            //}
            else
            {
                return reflectedValue != null ? reflectedValue.ToString().ToLower() : "";
            }
        }

        public static object GetReflectedPropertyValue(this object subject, string propertyName, bool ignoreCase)
        {
            // Get the property value of the object
            object reflectedValue = subject.GetType().GetProperty(propertyName).GetValue(subject, null);

            Type type = subject.GetType().GetProperty(propertyName).PropertyType;
            if (type == typeof(int))
            {
                return Convert.ToInt32(reflectedValue);
            }
            //else if (type == typeof(bool))
            //{
            //    return Convert.ToBoolean(reflectedValue);
            //}
            else
            {
                if (ignoreCase)
                {
                    return reflectedValue != null ? reflectedValue.ToString().ToLower() : "";
                }
                else
                {
                    return reflectedValue != null ? reflectedValue.ToString() : "";
                }
            }
        }
    }
}
