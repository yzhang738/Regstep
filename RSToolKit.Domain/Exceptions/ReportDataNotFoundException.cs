using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RSToolKit.Domain.Exceptions
{
    /// <summary>
    /// An exception that represents an the inability to locate the desired report data record.
    /// </summary>
    public class ReportDataNotFoundException
        : RegStepException
    {
        /// <summary>
        /// Initializes with default message.
        /// </summary>
        public ReportDataNotFoundException()
            : base("The report data was unable to be located in the databsae.", 500, 31)
        { }

        /// <summary>
        /// Initializes with a custom message.
        /// </summary>
        /// <param name="message">The message to portray.</param>
        public ReportDataNotFoundException(string message)
            : base(message, 500, 31)
        { }
    }
}