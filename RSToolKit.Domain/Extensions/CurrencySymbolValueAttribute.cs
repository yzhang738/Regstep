using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;

namespace RSToolKit.Domain
{
    public class CurrencySymbolValueAttribute : Attribute
    {
        private string _value;

        public CurrencySymbolValueAttribute(string value)
        {
            _value = value;
        }

        public string CurrencySymbolValue
        {
            get
            {
                return _value;
            }
        }
    }
    public class CultureAttribute : Attribute
    {
        private CultureInfo _value;

        public CultureAttribute(CultureInfo value)
        {
            _value = value;
        }

        public CultureInfo CultureValue
        {
            get
            {
                return _value;
            }
        }
    }
}
