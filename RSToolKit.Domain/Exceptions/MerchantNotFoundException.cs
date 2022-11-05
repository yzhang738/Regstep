using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RSToolKit.Domain.Exceptions
{
    /// <summary>
    /// An exception that represents an the inability to locate the desired transaction record.
    /// </summary>
    public class MerchantNotFoundException
        : RegStepException
    {
        /// <summary>
        /// Initializes with default message.
        /// </summary>
        public MerchantNotFoundException()
            : base("The merchant was unable to be located in the databsae.", 500, 81)
        { }

        /// <summary>
        /// Initializes with a custom message.
        /// </summary>
        /// <param name="message">The message to portray.</param>
        public MerchantNotFoundException(string message)
            : base(message, 500, 81)
        { }
    }
}