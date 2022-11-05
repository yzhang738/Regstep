using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RSToolKit.Domain.Errors
{
    public class UserException : Exception
    {

        public UserException(string s)
            : base(s)
        {
        }
    }
}