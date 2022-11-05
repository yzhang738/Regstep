using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RSToolKit.Domain.Exceptions
{
    public class RegStepException
        : Exception
    {
        /// <summary>
        /// The major excpetion number.
        /// Ex: 500
        /// </summary>
        public int MajorError { get { return _majorError; } }
        /// <summary>
        /// The minor exeption number.
        /// </summary>
        public int MinorError { get { return _minorError; } }
        /// <summary>
        /// The message that describes the current exception.
        /// </summary>
        public override string Message
        {
            get
            {
                if (String.IsNullOrEmpty(_message))
                    return MajorError + (MinorError != 0 ? "." + MinorError : "");
                return MajorError + (MinorError != 0 ? "." + MinorError + ": " : ": ") + _message;
            }
        }
        /// <summary>
        /// Represents the id of the log in the database.
        /// </summary>
        public long LoggedId { get; set; }
        /// <summary>
        /// The link to get more information about this error.
        /// </summary>
        public override string HelpLink
        {
            get
            {
                if (MinorError != 0)
                    return "http://developer.regstep.com/errors/" + MajorError + "/" + MinorError;
                return "http://developer.regstep.com/errors/" + MajorError;
            }
            set
            {
                return;
            }
        }

        protected int _majorError = 500;
        protected int _minorError = 0;
        protected string _message = null;

        /// <summary>
        /// Initializes the excpetion.
        /// </summary>
        public RegStepException()
            : base()
        {
            LoggedId = -1;
        }

        /// <summary>
        /// Initializes the excpetion with a descriptive message.
        /// </summary>
        /// <param name="message">The descriptive message.</param>
        public RegStepException(string message)
            : this()
        {
            _message = message;
        }

        /// <summary>
        /// Initializes the excpetion with a descriptive message and error numbers.
        /// </summary>
        /// <param name="message">The descriptive message.</param>
        /// <param name="major">The major error.</param>
        /// <param name="minor">The minor error</param>
        public RegStepException(string message, int major, int minor)
            : this(message)
        {
            _majorError = major;
            _minorError = minor;
        }

        /// <summary>
        /// Initializes the exception with error numbers.
        /// </summary>
        /// <param name="major">The major error.</param>
        /// <param name="minor">The minor error</param>
        public RegStepException(int major, int minor)
            : this(null, major, minor)
        { }

    }
}