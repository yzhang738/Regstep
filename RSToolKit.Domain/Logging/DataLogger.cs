using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RSToolKit.Domain.Entities.Clients;
using RSToolKit.Domain.Data;
using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.Data.Entity.Validation;

namespace RSToolKit.Domain.Logging
{
    /// <summary>
    /// Logs various information into the database.
    /// </summary>
    public class DataLogger
        : ILogger
    {
        protected string _userId;
        /// <summary>
        /// The user that is involved in the logging.
        /// </summary>
        public User User { get; set; }
        /// <summary>
        /// The method that was used when logging.
        /// </summary>
        public string LoggingMethod { get; set; }
        /// <summary>
        /// The thread that was used when logging.
        /// </summary>
        public string Thread { get; set; }
        public string UserId
        {
            get
            {
                if (User != null)
                    return User.Id;
                return _userId;
            }
            set
            {
                _userId = value;
            }
        }

        /// <summary>
        /// Initializes a data logger.
        /// </summary>
        public DataLogger()
        {
            User = null;
            LoggingMethod = "Default";
            Thread = "Main";
            _userId = null;
        }

        /// <summary>
        /// Logs an error into the database.
        /// </summary>
        /// <param name="ex">The exception to log.</param>
        /// <param name="company">The company that the log applies to.</param>
        /// <returns></returns>
        public Log Error(Exception ex, Company company = null)
        {
            var message = "";
            var exception = "";
            var e = ex;
            var e_count = 1;
            while (e != null)
            {
                if (e_count > 1)
                {
                    message += Environment.NewLine;
                    exception += Environment.NewLine;
                }
                message += e_count + ": " + (String.IsNullOrWhiteSpace(e.Message) ? e.ToString() : e.Message);
                exception += "Stack Trace " + e_count + ": " + Environment.NewLine + e.StackTrace;
                e_count++;
                e = e.InnerException;
            }
            if (exception.Length > 4000)
                exception = exception.Substring(0, 3900) + "...";
            var log = new Log()
            {
                Message = message,
                Level = "5",
                Thread = Thread,
                Logger = LoggingMethod,
                Exception = exception,
                DateCreated = DateTimeOffset.Now
            };
            log.UserKey = UserId;
            if (User != null)
                log.CompanyKey = User.WorkingCompanyKey;
            if (company != null)
                log.CompanyKey = company.UId;
            using (var context = new EFDbContext())
            {
                context.Logs.Add(log);
                context.SaveChanges();
            }
            return log;
        }

        /// <summary>
        /// Logs a warning into the database.
        /// </summary>
        /// <param name="message">The message to log.</param>
        /// <param name="callStack">Wether to include a call stack in the entry.</param>
        /// <param name="company">The company that the log applies to.</param>
        /// <returns></returns>
        public Log Warning(string message, bool callStack = false, Company company = null)
        {
            var log = new Log()
            {
                Message = message,
                Level = "4",
                Thread = Thread,
                Logger = LoggingMethod,
                DateCreated = DateTimeOffset.Now
            };
            if (callStack)
                log.Exception = GetCallStack();
            log.UserKey = UserId;
            if (User != null)
                log.CompanyKey = User.WorkingCompanyKey;
            if (company != null)
                log.CompanyKey = company.UId;
            using (var context = new EFDbContext())
            {
                context.Logs.Add(log);
                context.SaveChanges();
            }
            return log;
        }

        /// <summary>
        /// Logs a message into the database.
        /// </summary>
        /// <param name="ex">The exception to log</param>
        /// <param name="callStack">Wether to include a call stack in the entry.</param>
        /// <param name="company">The company that the log applies to.</param>
        /// <returns></returns>
        public Log Info(string message, bool callStack = true, Company company = null)
        {
            var log = new Log()
            {
                Message = message,
                User = User,
                Level = "3",
                Thread = Thread,
                Logger = LoggingMethod,
                DateCreated = DateTimeOffset.Now
            };
            if (callStack)
                log.Exception = GetCallStack();
            log.UserKey = UserId;
            if (User != null)
                log.CompanyKey = User.WorkingCompanyKey;
            if (company != null)
                log.CompanyKey = company.UId;
            using (var context = new EFDbContext())
            {
                context.Logs.Add(log);
                context.SaveChanges();
            }
            return log;
        }

        /// <summary>
        /// Logs a debug message into the database.
        /// </summary>
        /// <param name="ex">The exception to log</param>
        /// <param name="callStack">Wether to include a call stack in the entry.</param>
        /// <returns></returns>
        public Log Debug(string message, bool callStack = false, Company company = null)
        {
            var log = new Log()
            {
                Message = message,
                User = User,
                Level = "2",
                Thread = Thread,
                Logger = LoggingMethod,
                DateCreated = DateTimeOffset.Now
            };
            if (callStack)
                log.Exception = GetCallStack();
            log.UserKey = UserId;
            if (User != null)
                log.CompanyKey = User.WorkingCompanyKey;
            if (company != null)
                log.CompanyKey = company.UId;
            using (var context = new EFDbContext())
            {
                context.Logs.Add(log);
                context.SaveChanges();
            }
            return log;
        }

        /// <summary>
        /// Logs a custom entry into the logging database.
        /// </summary>
        /// <param name="message">The message to log.</param>
        /// <param name="user">The user to log it as.</param>
        /// <param name="level">The level to log.</param>
        /// <param name="thread">The thread to log.</param>
        /// <param name="logger">The logger to log.</param>
        /// <param name="callStack">The callStack to log.</param>
        /// <param name="company">The company that the log applies to.</param>
        /// <returns>A log entry.</returns>
        public Log Log(string message, User user = null, string level = null, string thread = null, string logger = null, string callStack = "", Company company = null)
        {
            var log = new Log()
            {
                Message = message,
                User = user ?? User,
                Level = level ?? "1",
                Thread = thread ?? Thread,
                Logger = logger ?? LoggingMethod,
                Exception = callStack ?? "",
                DateCreated = DateTimeOffset.Now
            };
            log.UserKey = UserId;
            if (User != null)
                log.CompanyKey = User.WorkingCompanyKey;
            if (company != null)
                log.CompanyKey = company.UId;
            using (var context = new EFDbContext())
            {
                context.Logs.Add(log);
                context.SaveChanges();
            }
            return log;
        }

        /// <summary>
        /// Disposes of the class to include the repository.
        /// </summary>
        public void Dispose()
        {
        }

        /// <summary>
        /// Gets the callstack from the exceptions
        /// </summary>
        /// <returns></returns>
        protected string GetCallStack()
        {
            var stackTrace = new StackTrace();
            var r_string = "";
            for (int i = 0; i < stackTrace.FrameCount; i++)
            {
                StackFrame sf = stackTrace.GetFrame(i);
                r_string += "Method: " + sf.GetMethod();
                r_string += " File: " + sf.GetFileName();
                r_string += " Line Number: " + sf.GetFileLineNumber();
                r_string += Environment.NewLine;
            }
            return r_string;
        }
    }
}
