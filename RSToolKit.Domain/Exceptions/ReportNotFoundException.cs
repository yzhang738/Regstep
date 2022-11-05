using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RSToolKit.Domain.Exceptions
{
    /// <summary>
    /// An exception that represents an the inability to locate the desired report record.
    /// </summary>
    public class ReportNotFoundException
        : RegStepException
    {
        /// <summary>
        /// Initializes with default message.
        /// </summary>
        public ReportNotFoundException()
            : base("The report was unable to be located in the databsae.", 500, 101)
        { }

        /// <summary>
        /// Initializes with a custom message.
        /// </summary>
        /// <param name="message">The message to portray.</param>
        public ReportNotFoundException(string message)
            : base(message, 500, 101)
        { }
    }
}
