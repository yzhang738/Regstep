using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RSToolKit.WebUI.Infrastructure.Json
{
    /// <summary>
    /// Holds information about a form error sent back as a json.
    /// </summary>
    public class JsonError
    {
        /// <summary>
        /// The key of the error, this is the name attribute of the input.
        /// </summary>
        public string Key { get; set; }
        /// <summary>
        /// The error message.
        /// </summary>
        public IEnumerable<string> Errors { get; set; }
    }
}