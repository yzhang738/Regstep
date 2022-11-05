using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RSToolKit.Domain
{
    public class StringValueAttribute : Attribute
    {
        private string _value;

        public StringValueAttribute(string value)
        {
            _value = value;
        }

        public string StringValue
        {
            get
            {
                return _value;
            }
        }
    }

    public class JTableTypeAttribute : Attribute
    {
        private string _value;

        public JTableTypeAttribute(string value)
        {
            _value = value;
        }

        public string JTableType
        {
            get
            {
                return _value;
            }
        }
    }


    public class TestValueAttribute : Attribute
    {
        private string _value;

        public TestValueAttribute(string value)
        {
            _value = value;
        }

        public string TestValue
        {
            get
            {
                return _value;
            }
        }
    }

    public class FloatValueAttribute : Attribute
    {
        private float _value;

        public FloatValueAttribute(float value)
        {
            _value = value;
        }

        public float FloatValue
        {
            get
            {
                return _value;
            }
        }
    }


    public class PayPalValueAttribute : Attribute
    {
        private string _value;
        public PayPalValueAttribute(string value) { _value = value; }
        public string PayPalValue { get { return _value; } }
    }
    public class iPayValueAttribute : Attribute
    {
        private string _value;
        public iPayValueAttribute(string value) { _value = value; }
        public string iPayValue { get { return _value; } }
    }


    public class FormAttribute : Attribute
    {
        private bool _value;

        public FormAttribute(bool value = true)
        {
            _value = value;
        }

        public bool BoolValue
        {
            get
            {
                return _value;
            }
        }
    }


    public class CurrencyFormat : Attribute
    {
        private string _value;
        public CurrencyFormat(string value)
        {
            _value = value;
        }
        public string CurrencyValue { get { return _value; } }
    }

    public class TagValueAttribute : Attribute
    {
        private string _value;

        public TagValueAttribute(string value)
        {
            _value = value;
        }

        public string StringValue
        {
            get
            {
                return _value;
            }
        }
    }

}