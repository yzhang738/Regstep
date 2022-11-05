using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RSToolKit.Domain.Exceptions;

namespace RSToolKit.Domain.Data.Repositories
{
    /// <summary>
    /// An exception that represents an the inability to locate the desired record in the database.
    /// </summary>
    public class RecordNotFoundException
        : RegStepException
    {
        /// <summary>
        /// Initializes with default message.
        /// </summary>
        public RecordNotFoundException()
            : base("The record was unable to be located in the databsae.", 500, 101)
        { }

        /// <summary>
        /// Initializes with a custom message.
        /// </summary>
        /// <param name="message">The message to portray.</param>
        public RecordNotFoundException(string message)
            : base(message, 500, 101)
        { }
    }
}
