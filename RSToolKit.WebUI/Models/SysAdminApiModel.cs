using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RSToolKit.WebUI.Models
{
    public class ApiMessage
    {
        public bool Succes { get; set; }
        public string Message { get; set; }
        public DateTimeOffset Date { get; set; }
        public Dictionary<string, string> Params { get; set; }
        public Guid Id { get; set; }
    }
}