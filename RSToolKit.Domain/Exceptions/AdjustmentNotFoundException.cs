using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RSToolKit.Domain.Exceptions
{
    /// <summary>
    /// An exception that represents an the inability to locate the desired adjustment data record.
    /// </summary>
    public class AdjustmentNotFoundException
        : RegStepException
    {
        /// <summary>
        /// Initializes with default message.
        /// </summary>
        public AdjustmentNotFoundException()
            : base("The adjustment was unable to be located in the databsae.", 500, 91)
        { }

        /// <summary>
        /// Initializes with a custom message.
        /// </summary>
        /// <param name="message">The message to portray.</param>
        public AdjustmentNotFoundException(string message)
            : base(message, 500, 91)
        { }
    }
}