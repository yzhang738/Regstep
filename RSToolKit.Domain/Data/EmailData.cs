using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Mail;
using RSToolKit.Domain.Entities.Email;

namespace RSToolKit.Domain.Data
{
    public class EmailData
    {
        public List<MailAddress> To { get; set; }
        public MailAddress From { get; set; }
        public List<MailAddress> CC { get; set; }
        public List<MailAddress> BCC { get; set; }
        public List<MailAddress> ReplyToList { get; set; }
        public string Html { get; set; }
        public string Text { get; set; }
        public string Subject { get; set; }
        public Guid SendKey { get; set; }
        public SendGridHeader Header { get; set; }
        public Guid EmailId { get; set; }
        public string Type { get; set; }

        public EmailData()
        {
            To = new List<MailAddress>();
            CC = new List<MailAddress>();
            BCC = new List<MailAddress>();
            ReplyToList = new List<MailAddress>();
            SendKey = Guid.NewGuid();
            EmailId = Guid.Empty;
            Header = null;
            Type = "Unknown";
        }
    }
}
