using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RSToolKit.Domain.Reflection
{
    public static class ReflectionExtensions
    {
        /// <summary>
        /// Gets the value of the either the member or property of the type.
        /// </summary>
        /// <param name="type">The type to get the property or field from.</param>
        /// <param name="name">The name of the field or property.</param>
        /// <param name="src">The object to get the value from.</param>
        /// <param name="index">An optional index parameter for indexed fields or properties.</param>
        /// <returns>Returns the value as an <code>object</code>.</returns>
        public static object GetValue(this Type type, string name, object src, object[] index = null)
        {
            // First we check for a property.
            var propInfo = type.GetProperty(name);
            if (propInfo != null)
                return propInfo.GetValue(src, index);
            var fieldInfo = type.GetField(name);
            if (fieldInfo != null)
                return fieldInfo.GetValue(src);
            return null;
        }
    }
}
