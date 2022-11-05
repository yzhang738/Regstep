using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using RSToolKit.Domain.Exceptions;

namespace RSToolKit.WebUI.Infrastructure
{
    /// <summary>
    /// An exception that represents an non existent token.
    /// </summary>
    public class TokenNotFoundException
        : RegStepException
    {
        /// <summary>
        /// Initializes with default message.
        /// </summary>
        public TokenNotFoundException()
            : base("The token was not found in memory.", 500, 22)
        { }

        /// <summary>
        /// Initializes with a custom message.
        /// </summary>
        /// <param name="message">The message to portray.</param>
        public TokenNotFoundException(string message)
            : base(message, 500, 22)
        { }
    }
}