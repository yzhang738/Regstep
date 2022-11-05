using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RSToolKit.Domain.Exceptions
{
    /// <summary>
    /// An exception that represents an the inability to locate the desired report data record.
    /// </summary>
    public class ReportDataNotAttachedException
        : RegStepException
    {
        /// <summary>
        /// Initializes with default message.
        /// </summary>
        public ReportDataNotAttachedException()
            : base("The report data is not attached to a JsonTable object.", 500, 32)
        { }

        /// <summary>
        /// Initializes with a custom message.
        /// </summary>
        /// <param name="message">The message to portray.</param>
        public ReportDataNotAttachedException(string message)
            : base(message, 500, 31)
        { }
    }
}