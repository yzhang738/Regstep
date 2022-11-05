using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using RSToolKit.Domain.Exceptions;

namespace RSToolKit.WebUI.Infrastructure
{
    /// <summary>
    /// An exception that represents an invalid id. This is thrown if the id is not of either type long or Guid.
    /// </summary>
    public class InvalidIdException
        : RegStepException
    {
        /// <summary>
        /// Initializes with default message.
        /// </summary>
        public InvalidIdException()
            : base("The id must be either a global unique identifier or 64 bit integer.", 500, 21)
        { }

        /// <summary>
        /// Initializes with a custom message.
        /// </summary>
        /// <param name="message">The message to portray.</param>
        public InvalidIdException(string message)
            : base(message, 500, 21)
        { }
    }
}