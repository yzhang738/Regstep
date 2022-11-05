using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RSToolKit.Domain.Exceptions
{
    /// <summary>
    /// An exception that represents an the inability to locate the desired form record.
    /// </summary>
    public class FormNotFoundException
        : RegStepException
    {
        /// <summary>
        /// Initializes with default message.
        /// </summary>
        public FormNotFoundException()
            : base("The form was unable to be located in the databsae.", 500, 71)
        { }

        /// <summary>
        /// Initializes with a custom message.
        /// </summary>
        /// <param name="message">The message to portray.</param>
        public FormNotFoundException(string message)
            : base(message, 500, 71)
        { }
    }
}