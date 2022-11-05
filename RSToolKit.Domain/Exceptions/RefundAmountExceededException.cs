using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RSToolKit.Domain.Exceptions
{
    /// <summary>
    /// An exception that represents an the inability to locate the desired transaction record.
    /// </summary>
    public class RefundAmountExceededException
        : RegStepException
    {
        /// <summary>
        /// Initializes with default message.
        /// </summary>
        public RefundAmountExceededException()
            : base("The refund was unable to be processed because the amount to refund was more than the amount captured.", 500, 42)
        { }

        /// <summary>
        /// Initializes with a custom message.
        /// </summary>
        /// <param name="message">The message to portray.</param>
        public RefundAmountExceededException(string message)
            : base(message, 500, 42)
        { }
    }
}