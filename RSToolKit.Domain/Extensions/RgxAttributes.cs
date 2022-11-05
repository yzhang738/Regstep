using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RSToolKit.Domain
{
    public class RgxErrorValueAttribute : Attribute
    {
        private string _value;

        public RgxErrorValueAttribute(string value)
        {
            _value = value;
        }

        public string RgxErrorValue
        {
            get
            {
                return _value;
            }
        }
    }

    public class RgxPatternAttribute : Attribute
    {
        private string _value;

        public RgxPatternAttribute(string value)
        {
            _value = value;
        }

        public string RgxPatternValue
        {
            get
            {
                return _value;
            }
        }
    }

    public class RgxValueAttribute : Attribute
    {
        private string _value;

        public RgxValueAttribute(string value)
        {
            _value = value;
        }

        public string RgxValue
        {
            get
            {
                return _value;
            }
        }
    }

}
