using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RSToolKit.Domain.Exceptions
{
    /// <summary>
    /// An exception that is thrown when the company could not be found.
    /// </summary>
    public class CompanyNotFoundException
        : RegStepException
    {
        /// <summary>
        /// Initializes with default message.
        /// </summary>
        public CompanyNotFoundException()
            : base("The form was unable to be located in the databsae.", 500, 111)
        { }

        /// <summary>
        /// Initializes with a custom message.
        /// </summary>
        /// <param name="message">The message to portray.</param>
        public CompanyNotFoundException(string message)
            : base(message, 500, 111)
        { }
    }
}
