using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RSToolKit.Domain.Exceptions
{
    /// <summary>
    /// An exception that represents insufficient permissions.
    /// </summary>
    public class InsufficientPermissionsException
        : RegStepException
    {
        /// <summary>
        /// Initializes with default message and minor error of 60 (generic permissions).
        /// </summary>
        public InsufficientPermissionsException(int minor = 200)
            : base("Insufficient permissions to handle data.", 500, 200)
        {
            switch (MinorError)
            {
                case 201:
                    _message = "Insufficient permissions to read data.";
                    break;
                case 202:
                    _message = "Insufficient permissions to modify data.";
                    break;
                case 203:
                    _message = "Insufficient permissions to create data.";
                    break;
                case 204:
                    _message = "Insufficient permissions to delete data.";
                    break;
            }
        }

        public InsufficientPermissionsException(Security.SecurityAccessType accessType)
            : base("Insufficient permissions to handle data.", 500, 200)
        {
            switch (accessType)
            {
                case Security.SecurityAccessType.Read:
                    _minorError = 201;
                    _message = "Insufficient permissions to read data.";
                    break;
                case Security.SecurityAccessType.Write:
                    _message = "Insufficient permissions to modify data.";
                    _minorError = 202;
                    break;
            }
        }

        /// <summary>
        /// Initializes with a custom message.
        /// </summary>
        /// <param name="message">The message to portray.</param>
        /// <param name="minor">The minor error number.</param>
        public InsufficientPermissionsException(string message, int minor = 200)
            : base(message, 500, minor)
        { }
    }
}