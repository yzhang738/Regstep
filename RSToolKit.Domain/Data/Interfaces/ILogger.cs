using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RSToolKit.Domain.Logging;
using RSToolKit.Domain.Entities.Clients;

namespace RSToolKit.Domain.Data
{
    public interface ILogger
        : IDisposable
    {
        User User { get; set; }
        string Thread { get; set; }
        string LoggingMethod { get; set; }
        Log Error(Exception ex, Company company = null);
        Log Warning(string message, bool callStack = false, Company company = null);
        Log Info(string message, bool callStack = true, Company company = null);
        Log Debug(string message, bool callStack = false, Company company = null);
        Log Log(string message, User user = null, string level = null, string thread = null, string logger = null, string callStack = null, Company company = null);
        string UserId { get; set; }
    }
}
