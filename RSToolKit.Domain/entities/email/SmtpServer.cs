using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RSToolKit.Domain.Entities;
using System.Security.Cryptography;
using System.IO;
using System.Data.SqlClient;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Net.Mail;
using System.Net;
using SendGrid;
using RSToolKit.Domain.Data;
using RSToolKit.Domain.Entities.Clients;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using RSToolKit.Domain.Entities;
using HtmlAgilityPack;
using RSToolKit.Domain.Security;

// Complete
namespace RSToolKit.Domain.Entities.Email
{
    public class SmtpServer : INodeItem, IRSData, IProtected
    {

        public static readonly Regex FooterRegex = new Regex(@"<!--footer-->", RegexOptions.IgnoreCase);

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long SortingId { get; set; }

        [Key]
        public Guid UId { get; set; }

        [MaxLength(250)]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string Name { get; set; }

        public DateTimeOffset DateCreated { get; set; }

        public DateTimeOffset DateModified { get; set; }

        public Guid ModificationToken { get; set; }

        public Guid ModifiedBy { get; set; }

        [ForeignKey("CompanyKey")]
        public virtual Company Company { get; set; }
        public Guid CompanyKey { get; set; }

        private const string initVector = "tu89geji340t89u2";
        private string passphrase = "asdfaser adfaa adf9 a0fd83u4 -97 09*&)(*A&SD0[iq-0 9ur";
        private const int keysize = 256;

        public string Description { get; set; }

        [MaxLength(250)]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string Username { get; set; }
        [MaxLength(1000)]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string PasswordHash { get; set; }
        public bool Primary { get; set; }
        [MaxLength(500)]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string Address { get; set; }
        public bool SSL { get; set; }
        public int Port { get; set; }
        public bool SendGrid { get; set; }

        public SmtpServer() : base()
        {
            Username = Description = "";
            PasswordHash = "";
            Address = "";
            SSL = false;
            Port = 45;
            Name = "New SmtpServer";
            Primary = false;
            SendGrid = true;
        }

        public static SmtpServer New(FormsRepository repository, Company company, User user, string address, int port, string username, string key, bool ssl = false, Guid? owner = null, Guid? group = null, string permission = "770", string name = null)
        {
            var node = new SmtpServer()
            {
                UId = Guid.NewGuid(),
                Company = company,
                CompanyKey = company.UId,
                Username = username,
                Address = address,
                Port = port,
                SSL = ssl
            };
            node.EncryptPassword(key);
            repository.Add(node);
            repository.Commit();
            return node;
        }

        public string GetPassword()
        {
            byte[] plainTextBytes;
            int decryptedByteCount;
            var initVectorBytes = Encoding.UTF8.GetBytes(initVector);
            var cipherTextBytes = Convert.FromBase64String(PasswordHash);
            var password = new PasswordDeriveBytes(passphrase, null);
            var keyBytes = password.GetBytes(keysize / 8);
            var symmKey = new RijndaelManaged();
            symmKey.Mode = CipherMode.CBC;
            var decryptor = symmKey.CreateDecryptor(keyBytes, initVectorBytes);
            using (var ms = new MemoryStream(cipherTextBytes))
            using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
            {
                plainTextBytes = new byte[cipherTextBytes.Length];
                decryptedByteCount = cs.Read(plainTextBytes, 0, plainTextBytes.Length);
            }
            return Encoding.UTF8.GetString(plainTextBytes, 0, decryptedByteCount);
        }

        public void EncryptPassword(string value)
        {
            byte[] cipherTextBytes;
            byte[] initVectorBytes = Encoding.UTF8.GetBytes(initVector);
            byte[] plainTextBytes = Encoding.UTF8.GetBytes(value);
            PasswordDeriveBytes password = new PasswordDeriveBytes(passphrase, null);
            byte[] keyBytes = password.GetBytes(keysize / 8);
            RijndaelManaged symmetricKey = new RijndaelManaged();
            symmetricKey.Mode = CipherMode.CBC;
            ICryptoTransform encryptor = symmetricKey.CreateEncryptor(keyBytes, initVectorBytes);
            using (MemoryStream memoryStream = new MemoryStream())
            using (CryptoStream cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
            {
                cryptoStream.Write(plainTextBytes, 0, plainTextBytes.Length);
                cryptoStream.FlushFinalBlock();
                cipherTextBytes = memoryStream.ToArray();
                memoryStream.Close();
                cryptoStream.Close();
            }
            PasswordHash = Convert.ToBase64String(cipherTextBytes);
        }

        public SmtpClient CreateServer()
        {
            var client = new SmtpClient(Address, Port)
            {
                EnableSsl = SSL,
                Credentials = new NetworkCredential(Username, GetPassword())
            };
            return client;
        }

        #region Methods

        public static bool CanSend(IEmail email, IPerson person, FormsRepository repository)
        {
            // We need to find the last email.
            var emailSends = person.EmailSends.Where(es => es.EmailKey == email.UId).OrderBy(es => es.DateSent).ToList();
            EmailSend emailSend = null;
            var count = 0;
            foreach (var send in emailSends)
            {
                if (send.EmailEvents.Where(s => s.Event.ToLower() == "delivered").Count() == 0)
                    continue;
                count++;
                emailSend = send;
            }
            if (emailSend == null)
                return true;
            var lastSend = DateTimeOffset.Now - emailSend.DateSent;
            if (email.SendInterval.HasValue && email.SendInterval > lastSend)
                return false;
            if (count >= email.MaxSends && email.MaxSends > 0)
                return false;
            return true;
        }

        public static bool CanSend(IEmail email, IPerson person)
        {
            // We need to find the last email.
            var emailSends = person.EmailSends.Where(es => es.EmailKey == email.UId).OrderBy(es => es.DateSent).ToList();
            EmailSend emailSend = null;
            var count = 0;
            foreach (var send in emailSends)
            {
                if (send.EmailEvents.Where(s => s.Event.ToLower() == "delivered").Count() == 0)
                    continue;
                count++;
                emailSend = send;
            }
            if (emailSend == null)
                return true;
            var lastSend = DateTimeOffset.Now - emailSend.DateSent;
            if (email.SendInterval.HasValue && email.SendInterval > lastSend)
                return false;
            if (count >= email.MaxSends && email.MaxSends > 0)
                return false;
            return true;
        }

        public INode GetNode()
        {
            return Company;
        }

        public EmailSendResponse SendEmail(EmailData message, EFDbContext context, Company company, INamedNode node, IPerson person = null, IEnumerable<MailAddress> extraEmails = null, Contact addContact = null)
        {
            var t_extraEmails = extraEmails ?? new List<MailAddress>();
            MailAddress clientEmail = null;
            foreach (var e_mail in t_extraEmails)
                message.CC.Add(e_mail);
            string email = null;
            if (person != null)
                email = person.Email;
            else if (message.To.Count > 0)
                email = message.To.First().Address;
            if (email == null)
                return new EmailSendResponse() { Success = false, Response = "No email supplied." };
            Guid? companyKey = company == null ? null : (Guid?)company.UId;
            Guid? key = node != null ? (Guid?)node.UId : null;
            var unsubcomp = false;
            var unsubnode = false;
            string unsType = null;
            if (company != null && context.Unsubscribes.Count(u => u.Email.ToLower() == email.ToLower() && u.UnsubscribeFrom == company.UId) > 0)
            {
                unsubcomp = true;
                unsType = "Company";
            }
            if (node != null && context.Unsubscribes.Count(u => u.Email.ToLower() == email.ToLower() && u.UnsubscribeFrom == node.UId) > 0)
            {
                unsubnode = true;
                unsType = "Node";
            }
            if (unsubcomp || unsubnode)
            {
                if (person != null)
                {
                    var emailSendUns = new EmailSend()
                    {
                        DateSent = DateTimeOffset.UtcNow,
                        Recipient = email,
                        EmailDescription = message.Type,
                        EmailKey = message.EmailId,
                        UId = message.SendKey
                    };
                    emailSendUns.EmailEvents.Add(new EmailEvent()
                    {
                        Date = emailSendUns.DateSent,
                        Event = "Dropped",
                        Details = "Email delivery was not attempted.",
                        Notes = "Recipient has previously unsubscriped from " + (unsubcomp ? "all emails from your account." : "emails pertaining to " + node.Name + ".")
                    });
                    person.EmailSends.Add(emailSendUns);
                    context.SaveChanges();
                }
                return new EmailSendResponse() { Success = false, Response = "Person on unsubscribe list: " + unsType + "." };
            }
            message.Html = Regex.Replace(message.Html, @"\[email=>email\]", email);
            if (message.Text != null)
                message.Text = Regex.Replace(message.Text, @"\[email=>email\]", email);
            // Create the email event
            EmailSend emailSend = null;
            if (person != null)
            {
                emailSend = new EmailSend()
                {
                    DateSent = DateTimeOffset.UtcNow,
                    Recipient = email,
                    EmailDescription = message.Type,
                    EmailKey = message.EmailId,
                    UId = message.SendKey
                };
                emailSend.IgnoreEventsFrom.AddRange(message.CC.Select(c => c.Address.ToLower()));
                if (person is Registrant)
                {
                    emailSend.Registrant = person as Registrant;
                    emailSend.FormKey = ((Registrant)person).FormKey;
                    emailSend.Contact = ((Registrant)person).Contact ?? addContact;
                }
                else if (person is Contact)
                {
                    emailSend.Contact = person as Contact;
                }
                emailSend.EmailKey = message.EmailId;
                context.EmailSends.Add(emailSend);
                emailSend.EmailEvents.Add(new EmailEvent()
                {
                    Date = emailSend.DateSent,
                    Event = "Attempting to Send"
                });
                context.SaveChanges();
                try
                {
                    clientEmail = new MailAddress(person.Email);
                }
                catch (Exception)
                {
                    return new EmailSendResponse() { Response = "Invalid Email", Success = false };
                }
            }
            if (message.Header != null)
            {
                message.Header.unique_args["email-id"] = emailSend.SortingId.ToString();
                message.Header.unique_args["smtp-id"] = SortingId.ToString();
                message.Html = Regex.Replace(message.Html, @"\[email=>emailsend\]", emailSend.SortingId.ToString());
                if (message.Text != null)
                    message.Text = Regex.Replace(message.Text, @"\[email=>emailsend\]", emailSend.SortingId.ToString());
            }

            // Now we need to set up click tracking
            // Before building live, make sure it points to toolkit and not devtoolkit
            var agile_htmlBody = new HtmlDocument();
            agile_htmlBody.LoadHtml(message.Html);
            var nodes = agile_htmlBody.DocumentNode.SelectNodes("//a[@href]");
            if (nodes != null && emailSend != null)
            {
                foreach (HtmlNode link in nodes)
                {
                    var att = link.Attributes.FirstOrDefault(a => a.Name == "href");
                    var data = link.Attributes.FirstOrDefault(a => a.Name == "data-a-id");
                    if (data == null)
                        att.Value = "https://toolkit.regstep.com/emailclick/" + emailSend.SortingId.ToString() + "/none/?url=" + att.Value;
                    else
                        att.Value = "https://toolkit.regstep.com/emailclick/" + emailSend.SortingId.ToString() + "/" + data.Value + "?url=" + att.Value;
                }
                var t_nodes = agile_htmlBody.DocumentNode.SelectNodes("//tr[@id='footer_email_regstep']");
                if (t_nodes != null)
                {
                    var footer = t_nodes.FirstOrDefault();
                    if (footer != null)
                    {
                        footer.AppendChild(HtmlNode.CreateNode("<img src=\"https://toolkit.regstep.com/emailopen/" + emailSend.SortingId.ToString() + "?v=" + Guid.NewGuid() + "\" width=\"1\" height=\"1\" border=\"0\" style=\"height:1px !important;width:1px !important;border-width:0 !important;padding-top:0 !important;padding-bottom:0 !important;padding-right:0 !important;padding-left:0 !important;\" />"));
                    }
                }
            }
            message.Html = agile_htmlBody.DocumentNode.InnerHtml;

            var mailMessage = new MailMessage();
            if (message.Text != null)
            {
                mailMessage.Body = message.Text;
                mailMessage.IsBodyHtml = false;
                var htmlMime = new System.Net.Mime.ContentType("text/html");
                var alternate = AlternateView.CreateAlternateViewFromString(message.Html, htmlMime);
                mailMessage.AlternateViews.Add(alternate);
            }
            else
            {
                mailMessage.Body = message.Html;
                mailMessage.IsBodyHtml = true;
            }
            mailMessage.Subject = message.Subject;
            mailMessage.To.Clear();
            mailMessage.CC.Clear();
            mailMessage.Bcc.Clear();
            mailMessage.ReplyToList.Clear();
            message.To.ForEach(m => mailMessage.To.Add(m));
            if (clientEmail != null)
                mailMessage.To.Add(clientEmail);
            message.CC.ForEach(m => mailMessage.CC.Add(m));
            message.BCC.ForEach(m => mailMessage.Bcc.Add(m));
            message.ReplyToList.ForEach(m => mailMessage.ReplyToList.Add(m));
            if (message.ReplyToList.Count == 0)
                message.ReplyToList.Add(mailMessage.From);
            var displayName = "";
            if (message.ReplyToList.Count > 0 && message.ReplyToList[0] != null)
                displayName = message.ReplyToList[0].DisplayName;
            if (String.IsNullOrWhiteSpace(displayName))
                displayName = node.Name;
            mailMessage.From = new MailAddress("no_reply@regstep.com", displayName);
            mailMessage.Headers.Remove("X-SMTPAPI");
            if (message.Header != null)
                mailMessage.Headers.Add("X-SMTPAPI", message.Header.ToString());
            var server = CreateServer();
            server.DeliveryFormat = SmtpDeliveryFormat.International;
            try
            {
                server.Send(mailMessage);
            }
            catch (Exception e)
            {
                emailSend.EmailEvents.Add(new EmailEvent() { Event = "Unsupported Email Format" });
                return new EmailSendResponse() { Response = "Invalid Email", Success = false };
            }
            return new EmailSendResponse() { Success = true, Response = "Attemtping to Send" };
        }

        public EmailSendResponse SendEmail(EmailData message, FormsRepository repository, Company company, INamedNode node, IPerson person = null, SendGridHeader header = null, string type = "unknown", Guid? emailKey = null, IEnumerable<MailAddress> extraEmails = null, Contact addContact = null)
        {
            var t_extraEmails = extraEmails ?? new List<MailAddress>();
            MailAddress clientEmail = null;
            foreach (var e_mail in t_extraEmails)
                message.CC.Add(e_mail);
            string email = null;
            if (person != null)
                email = person.Email;
            else if (message.To.Count > 0)
                email = message.To.First().Address;
            if (email == null)
                return new EmailSendResponse() { Success = false, Response = "No email supplied." };
            Guid? companyKey = company == null ? null : (Guid?)company.UId;
            Guid? key = node != null ? (Guid?)node.UId : null;
            var unsubcomp = false;
            var unsubnode = false;
            string unsType = null;
            if (company != null && repository.Search<Unsubscribe>(u => u.Email.ToLower() == email.ToLower() && u.UnsubscribeFrom == company.UId).Count() > 0)
            {
                unsubcomp = true;
                unsType = "Company";
            }
            if (node != null && repository.Search<Unsubscribe>(u => u.Email.ToLower() == email.ToLower() && u.UnsubscribeFrom == node.UId).Count() > 0)
            {
                unsubnode = true;
                unsType = "Node";
            }
            if (unsubcomp || unsubnode)
            {
                if (person != null)
                {
                    var emailSendUns = new EmailSend()
                    {
                        DateSent = DateTimeOffset.UtcNow,
                        Recipient = email,
                        EmailDescription = type,
                        EmailKey = emailKey,
                        UId = message.SendKey
                    };
                    emailSendUns.EmailEvents.Add(new EmailEvent()
                    {
                        Date = emailSendUns.DateSent,
                        Event = "Dropped",
                        Details = "Email delivery was not attempted.",
                        Notes = "Recipient has previously unsubscriped from " + (unsubcomp ? "all emails from your account." : "emails pertaining to " + node.Name + ".")
                    });
                    person.EmailSends.Add(emailSendUns);
                    repository.Commit();
                }
                return new EmailSendResponse() { Success = false, Response = "Person on unsubscribe list: " + unsType + "."};
            }
            message.Html = Regex.Replace(message.Html, @"\[email=>email\]", email);
            if (message.Text != null)
                message.Text = Regex.Replace(message.Text, @"\[email=>email\]", email);
            // Create the email event
            EmailSend emailSend = null;
            if (person != null)
            {
                emailSend = new EmailSend()
                {
                    DateSent = DateTimeOffset.UtcNow,
                    Recipient = email,
                    EmailDescription = type,
                    EmailKey = emailKey,
                    UId = message.SendKey
                };
                emailSend.IgnoreEventsFrom.AddRange(message.CC.Select(c => c.Address.ToLower()));
                if (person is Registrant)
                {
                    emailSend.Registrant = person as Registrant;
                    emailSend.FormKey = ((Registrant)person).FormKey;
                    emailSend.Contact = addContact;
                }
                else if (person is Contact)
                {
                    emailSend.Contact = person as Contact;
                }
                emailSend.EmailKey = emailKey;
                repository.Add(emailSend);
                emailSend.EmailEvents.Add(new EmailEvent()
                {
                    Date = emailSend.DateSent,
                    Event = "Attempting to Send"
                });
                repository.Commit();
                try
                {
                    clientEmail = new MailAddress(person.Email);
                }
                catch (Exception)
                {
                    return new EmailSendResponse() { Response = "Invalid Email", Success = false };
                }
            }
            if (emailSend != null)
            {
                header.unique_args["email-id"] = emailSend.SortingId.ToString();
                header.unique_args["smtp-id"] = SortingId.ToString();
                message.Html = Regex.Replace(message.Html, @"\[email=>emailsend\]", emailSend.SortingId.ToString());
                if (message.Text != null)
                    message.Text = Regex.Replace(message.Text, @"\[email=>emailsend\]", emailSend.SortingId.ToString());
            }
          
            // Now we need to set up click tracking
            // Before building live, make sure it points to toolkit and not devtoolkit
            var agile_htmlBody = new HtmlDocument();
            agile_htmlBody.LoadHtml(message.Html);
            var nodes = agile_htmlBody.DocumentNode.SelectNodes("//a[@href]");
            if (nodes != null && emailSend != null)
            {
                foreach (HtmlNode link in nodes)
                {
                    var att = link.Attributes.FirstOrDefault(a => a.Name == "href");
                    var data = link.Attributes.FirstOrDefault(a => a.Name == "data-a-id");
                    var linkType = link.Attributes.FirstOrDefault(a => a.Name == "data-footer-link");
                    if (linkType != null && linkType.Value.ToLower() == "yes")
                        continue;
                    if (data == null)
                        att.Value = "https://toolkit.regstep.com/emailclick/" + emailSend.SortingId.ToString() + "/none/?url=" + att.Value;
                    else
                        att.Value = "https://toolkit.regstep.com/emailclick/" + emailSend.SortingId.ToString() + "/" + data.Value + "?url=" + att.Value;
                }
                var t_nodes = agile_htmlBody.DocumentNode.SelectNodes("//tr[@id='footer_email_regstep']");
                if (t_nodes != null)
                {
                    var footer = t_nodes.FirstOrDefault();
                    if (footer != null)
                    {
                        footer.AppendChild(HtmlNode.CreateNode("<img src=\"https://toolkit.regstep.com/emailopen/" + emailSend.SortingId.ToString() + "?v=" + Guid.NewGuid() + "\" width=\"1\" height=\"1\" border=\"0\" style=\"height:1px !important;width:1px !important;border-width:0 !important;padding-top:0 !important;padding-bottom:0 !important;padding-right:0 !important;padding-left:0 !important;\" />"));
                    }
                }
            }
            message.Html = agile_htmlBody.DocumentNode.InnerHtml; 

            var mailMessage = new MailMessage();
            if (message.Text != null)
            {
                mailMessage.Body = message.Text;
                mailMessage.IsBodyHtml = false;
                var htmlMime = new System.Net.Mime.ContentType("text/html");
                var alternate = AlternateView.CreateAlternateViewFromString(message.Html, htmlMime);
                mailMessage.AlternateViews.Add(alternate);
            }
            else
            {
                mailMessage.Body = message.Html;
                mailMessage.IsBodyHtml = true;
            }
            mailMessage.Subject = message.Subject;
            mailMessage.To.Clear();
            mailMessage.CC.Clear();
            mailMessage.Bcc.Clear();
            mailMessage.ReplyToList.Clear();
            message.To.ForEach(m => mailMessage.To.Add(m));
            if (clientEmail != null)
                mailMessage.To.Add(clientEmail);
            message.CC.ForEach(m => mailMessage.CC.Add(m));
            message.BCC.ForEach(m => mailMessage.Bcc.Add(m));
            message.ReplyToList.ForEach(m => mailMessage.ReplyToList.Add(m));
            if (message.ReplyToList.Count == 0)
                message.ReplyToList.Add(mailMessage.From);
            var displayName = "";
            if (message.ReplyToList.Count > 0 && message.ReplyToList[0] != null)
                displayName = message.ReplyToList[0].DisplayName;
            if (String.IsNullOrWhiteSpace(displayName))
                displayName = node.Name;
            mailMessage.From = new MailAddress("no_reply@regstep.com", displayName);
            mailMessage.Headers.Remove("X-SMTPAPI");
            if (header != null)
                mailMessage.Headers.Add("X-SMTPAPI", header.ToString());
            var server = CreateServer();
            server.DeliveryFormat = SmtpDeliveryFormat.International;
            try
            {
                server.Send(mailMessage);
            }
            catch (Exception e)
            {
                emailSend.EmailEvents.Add(new EmailEvent() { Event = "Unsupported Email Format" });
                return new EmailSendResponse() { Response = "Invalid Email", Success = false };
            }
            return new EmailSendResponse() { Success = true, Response = "Attemtping to Send" };
        }

        public EmailSendResponse SendEmail(EmailData message, FormsRepository repository, Company company, INode node, string recipient, SendGridHeader header = null, string type = "unknown", IEnumerable<MailAddress> extraEmails = null)
        {
            var t_extraEmails = extraEmails ?? new List<MailAddress>();
            foreach (var e_mail in t_extraEmails)
                message.CC.Add(e_mail);
            string email = recipient;
            if (email == null)
                return new EmailSendResponse() { Success = false, Response = "No email supplied." };
            Guid? companyKey = company == null ? null : (Guid?)company.UId;
            Guid? key = node != null ? (Guid?)node.UId : null;
            var unsubcomp = false;
            var unsubnode = false;
            string unsType = null;
            if (company != null && repository.Search<Unsubscribe>(u => u.Email.ToLower() == email.ToLower() && u.UnsubscribeFrom == company.UId).Count() > 0)
            {
                unsubcomp = true;
                unsType = "Company";
            }
            if (node != null && repository.Search<Unsubscribe>(u => u.Email.ToLower() == email.ToLower() && u.UnsubscribeFrom == node.UId).Count() > 0)
            {
                unsubnode = true;
                unsType = "Node";
            }
            if (unsubcomp || unsubnode)
            {
                return new EmailSendResponse() { Success = false, Response = "Person on unsubscribe list: " + unsType + "." };
            }
            message.Html = Regex.Replace(message.Html, @"\[email\s*=(?:>|(?:&gt;))\s*email\]", email);
            message.Text = Regex.Replace(message.Text, @"\[email\s*=(?:>|(?:&gt;))\s*email\]", email);
            try
            {
                message.To.Add(new MailAddress(email));
            }
            catch (Exception)
            {
                return new EmailSendResponse() { Response = "Invalid Email", Success = false };
            }
            // Send the email.
            // Not a sendgrid server so we use the built in .NET library.
            var mailMessage = new MailMessage();
            if (message.Text != null)
            {
                mailMessage.Body = message.Text;
                mailMessage.IsBodyHtml = false;
                var htmlMime = new System.Net.Mime.ContentType("text/html");
                var alternate = AlternateView.CreateAlternateViewFromString(message.Html, htmlMime);
                mailMessage.AlternateViews.Add(alternate);
            }
            else
            {
                mailMessage.Body = message.Html;
                mailMessage.IsBodyHtml = true;
            }
            mailMessage.Subject = message.Subject;
            mailMessage.To.Clear();
            mailMessage.CC.Clear();
            mailMessage.Bcc.Clear();
            mailMessage.ReplyToList.Clear();
            message.To.ForEach(m => mailMessage.To.Add(m));
            message.CC.ForEach(m => mailMessage.CC.Add(m));
            message.BCC.ForEach(m => mailMessage.Bcc.Add(m));
            message.ReplyToList.ForEach(m => mailMessage.ReplyToList.Add(m));
            mailMessage.From = message.From;
            mailMessage.Headers.Remove("X-SMTPAPI");
            if (header != null)
                mailMessage.Headers.Add("X-SMTPAPI", header.ToString());
            var server = CreateServer();
            try
            {
                server.Send(mailMessage);
            }
            catch (Exception e)
            {
                return new EmailSendResponse() { Response = "Invalid Email", Success = false };
            }
            return new EmailSendResponse() { Success = true, Response = "Attempting to Send." };
        }

        public static EmailData GenerateFooter(EmailData data, IEmail email)
        {
            IEmailHolder node = email.Form;
            if (node == null)
                node = email.EmailCampaign;
            var t_footer = "";
            if (node != null && email.Company != null)
            {
                t_footer = @"
			    <tr id=""footer_email_regstep"">
                    <td width=""100%"" valign=""top"" align=""left"" bgcolor=""@render_var_background-bgcolor"" style=""margin: 0 0 0 0; padding: 30px 2px 0 2px; border: none; font-family: @render_var_Email-Font; color: @render_var_Footer-Font-Color; font-size: 12px; line-height: 18px;"">
                        RegStep - Event Management and Marketing Software
                        <br />
                        <a href=""http://www.regstep.com"" style=""color: @render_var_Footer-Font-Color; text-decoration: underline;"" target=""blank"">www.regstep.com</a>
                        <br />
                        <br />
                        Not interested in " + node.Name + @": <a data-footer-link=""yes"" href=""https://toolkit.regstep.com/unsubscribe/" + (node is Form ? "form" : "campaign") + "/" + node.SortingId + @"/[email=>emailsend]"" style=""color: @render_var_Footer-Font-Color; text-decoration: underline;"" target=""blank"">Unsubscribe</a>*
                        <br />
                        <br />
                        Stop receiving all emails from " + email.Company.Name + @": <a data-footer-link=""yes"" href=""https://toolkit.regstep.com/unsubscribe/company/" + email.Company.SortingId + @"/" + @"[email=>emailsend]"" style=""color: @render_var_Footer-Font-Color; text-decoration: underline;"" target=""blank"">Unsubscribe</a>*
                        <br />
                        <br />
                        <span style=""font-size: 10px;"">* Unsubscribes apply to email sent using the RS Toolkit</span>
                    </td>
                </tr>";
                data.Text += @"RegStep - Event Management and Marketing Software" + Environment.NewLine;
                data.Text += @"www.regstep.com" + Environment.NewLine;
                data.Text += @"Not interested in " + node.Name + @": Unsubscribe*" + Environment.NewLine;
                data.Text += "https://toolkit.regstep.com/unsubscribe/" + (node is Form ? "form" : "campaign") + "/" + node.UId + @"/[email=>emailsend]" + Environment.NewLine;
                data.Text += @"Stop receiving all emails from " + email.Company.Name + @": Unsubscribe*" + Environment.NewLine;
                data.Text += "https://toolkit.regstep.com/unsubscribe/company/" + email.Company.SortingId + @"/[email=>emailsend]" + Environment.NewLine;
                data.Text += @"* Unsubscribes apply to email sent using the RS Toolkit";
            }
            data.Html = FooterRegex.Replace(data.Html, t_footer);
            EmailVariable backgroundColor, fontColor, font;
            if (email is RSHtmlEmail)
            {
                backgroundColor = new EmailVariable() { Variable = "background-bgcolor", Value = "inherit" };
                fontColor = new EmailVariable() { Variable = "Footer-Font-Color", Value = "#000000" };
                font = new EmailVariable() { Variable = "Email-Font", Value = "arial" };
            }
            else
            {
                backgroundColor = (email as RSEmail).Variables.FirstOrDefault(v => v.Variable == "background-bgcolor") ?? new EmailVariable() { Variable = "background-bgcolor", Value = "inherit", Name = "Background Color", Type = "color" };
                fontColor = (email as RSEmail).Variables.FirstOrDefault(v => v.Variable == "Footer-Font-Color") ?? new EmailVariable() { Variable = "Footer-Font-Color", Value = "#000000", Name = "Footer Font Color", Type = "color" };
                font = (email as RSEmail).Variables.FirstOrDefault(v => v.Variable == "Email-Font") ?? new EmailVariable() { Variable = "Email-Font", Value = "arial", Name = "Email Font Color", Type = "font" };
            }
            data.Html = Regex.Replace(data.Html, @"@render_var_" + backgroundColor.Variable, String.IsNullOrWhiteSpace(backgroundColor.Value) ? "" : backgroundColor.Value, RegexOptions.IgnoreCase);
            data.Html = Regex.Replace(data.Html, @"@render_var_" + fontColor.Variable, String.IsNullOrWhiteSpace(fontColor.Value) ? "" : fontColor.Value, RegexOptions.IgnoreCase);
            data.Html = Regex.Replace(data.Html, @"@render_var_" + font.Variable, String.IsNullOrWhiteSpace(font.Value) ? "" : font.Value, RegexOptions.IgnoreCase);
            return data;
        }

        //Depriciated
        public void SendAdminEmail(MailMessage message, IEnumerable<string> recipients, IDataContext context)
        {
            var server = CreateServer();
            foreach (var r in recipients)
            {
                message.To.Clear();
                message.To.Add(new MailAddress(r));
                server.Send(message);
            }
        }

        #endregion

        public string EncodeEmail(string email)
        {
            StringBuilder sb = new StringBuilder();
            foreach (char c in email)
            {
                if (c > 127)
                {
                    // This character is too big for ASCII
                    string encodedValue = @"\u" + ((int)c).ToString("x4");
                    sb.Append(encodedValue);
                }
                else
                {
                    sb.Append(c);
                }
            }
            return sb.ToString();
        }
    }

    public class SendGridHeader
    {
        public List<string> category { get; set; }
        public Dictionary<string, string> unique_args { get; set; }
        public SendGridHeader()
        {
            category = new List<string>();
            unique_args = new Dictionary<string, string>();
        }
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }

    public class EmailSendResponse
    {
        public bool Success { get; set; }
        public string Response { get; set; }
    }
}
