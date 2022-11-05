using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RSToolKit.Domain.Exceptions
{
    /// <summary>
    /// An exception that represents an the inability to locate the desired registration record.
    /// </summary>
    public class RegistrantNotFoundException
        : RegStepException
    {
        /// <summary>
        /// Initializes with default message.
        /// </summary>
        public RegistrantNotFoundException()
            : base("The registrant was unable to be located in the databsae.", 500, 51)
        { }

        /// <summary>
        /// Initializes with a custom message.
        /// </summary>
        /// <param name="message">The message to portray.</param>
        public RegistrantNotFoundException(string message)
            : base(message, 500, 51)
        { }
    }
}