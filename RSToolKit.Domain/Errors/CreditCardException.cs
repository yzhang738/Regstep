using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RSToolKit.Domain.Errors
{
    public class CreditCardException : Exception
    {
        public CreditCardException(string message) : base(message) { }
    }
}
