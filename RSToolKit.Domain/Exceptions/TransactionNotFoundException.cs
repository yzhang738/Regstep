using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RSToolKit.Domain.Exceptions
{
    /// <summary>
    /// An exception that represents an the inability to locate the desired transaction record.
    /// </summary>
    public class TransactionNotFoundException
        : RegStepException
    {
        /// <summary>
        /// Initializes with default message.
        /// </summary>
        public TransactionNotFoundException()
            : base("The transaction was unable to be located in the databsae.", 500, 82)
        { }

        /// <summary>
        /// Initializes with a custom message.
        /// </summary>
        /// <param name="message">The message to portray.</param>
        public TransactionNotFoundException(string message)
            : base(message, 500, 82)
        { }
    }
}