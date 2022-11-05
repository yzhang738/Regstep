using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.ComponentModel.DataAnnotations;
using RSToolKit.Domain.Data;
using System.ComponentModel.DataAnnotations.Schema;
using RSToolKit.Domain.Entities.Email;
using Newtonsoft.Json;
using System.Web.Mvc;
using System.Text.RegularExpressions;
using EntityFrameworkExtension.Entity;
using System.Data.Entity;
using RSToolKit.Domain.Entities;
using RSToolKit.Domain.Entities.Clients;
using System.Net.Mail;
using HtmlAgilityPack;
using RSToolKit.Domain.JItems;

namespace RSToolKit.Domain.Entities.Email
{
    public class RSHtmlEmail : IEmail
    {
        [JsonIgnore]
        [CascadeDelete]
        public virtual List<Logic> Logics { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Index(IsClustered = true)]
        public long SortingId { get; set; }

        [Key]
        [Index(IsClustered = false)]
        public Guid UId { get; set; }

        [MaxLength(250)]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string Name { get; set; }

        public DateTimeOffset DateCreated { get; set; }
        public DateTimeOffset DateModified { get; set; }

        public Guid ModificationToken { get; set; }
        public Guid ModifiedBy { get; set; }

        [JsonIgnore]
        [ForeignKey("CompanyKey")]
        public virtual Company Company { get; set; }
        public Guid CompanyKey { get; set; }

        [JsonIgnore]
        [ForeignKey("FormKey")]
        public virtual Form Form { get; set; }
        public Guid? FormKey { get; set; }

        [JsonIgnore]
        [ForeignKey("EmailCampaignKey")]
        public virtual EmailCampaign EmailCampaign { get; set; }
        public Guid? EmailCampaignKey { get; set; }

        [JsonIgnore]
        [ForeignKey("SavedListKey")]
        public virtual SavedList SavedList { get; set; }
        public Guid? SavedListKey { get; set; }

        [JsonIgnore]
        [ForeignKey("ContactReportKey")]
        public virtual ContactReport ContactReport { get; set; }
        public Guid? ContactReportKey { get; set; }

        public string PlainText { get; set; }

        [JsonIgnore]
        [NotMapped]
        public IEmailList EmailList
        {
            get
            {
                if (SavedList != null)
                    return SavedList;
                if (ContactReport != null)
                    return ContactReport;
                return null;
            }
        }
        [NotMapped]
        public Guid? EmailListKey { get; set; }

        [MaxLength(500)]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string Subject { get; set; }

        [MaxLength(5000)]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string Description { get; set; }

        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string From { get; set; }

        public string CC { get; set; }
        public string BCC { get; set; }

        public EmailType EmailType  {get; set; }

        public DateTimeOffset? SendTime { get; set; }

        public long IntervalTicks
        {
            get
            {
                if (SendInterval.HasValue)
                    return SendInterval.Value.Ticks;
                return 0;
            }
            set
            {
                if (value != 0)
                    SendInterval = TimeSpan.FromTicks(value);
                else
                    SendInterval = null;
            }
        }

        public int MaxSends { get; set; }

        public bool RepeatSending { get; set; }

        public string To { get; set; }

        //Not Mapped
        [NotMapped]
        public TimeSpan? SendInterval { get; set; }
        [NotMapped]
        public double IntervalSeconds
        {
            get
            {
                if (SendInterval.HasValue)
                    return SendInterval.Value.TotalSeconds;
                return 0;
            }
            set
            {
                if (value != 0)
                    SendInterval = TimeSpan.FromSeconds(value);
                else
                    SendInterval = null;
            }
        }

        [AllowHtml]
        public string Html { get; set; }

        [NotMapped]
        public List<JLogic> JLogics { get; set; }
        public string RawJLogics
        {
            get
            {
                return JsonConvert.SerializeObject(JLogics);
            }
            set
            {
                if (String.IsNullOrEmpty(value))
                {
                    JLogics = new List<JLogic>();
                }
                else
                {
                    try
                    {
                        JLogics = JsonConvert.DeserializeObject<List<JLogic>>(value);
                    }
                    catch (Exception)
                    {
                        JLogics = new List<JLogic>();
                    }
                }
            }
        }

        public RSHtmlEmail()
        {
            Description = "";
            Logics = new List<Logic>();
            EmailType = EmailType.Unclassified;
            From = "";
            Subject = "";
            BCC = "";
            CC = "";
            Name = "New Email";
            PlainText = "";
            SendInterval = null;
            MaxSends = -1;
            JLogics = new List<JLogic>();
        }
       
        public static RSHtmlEmail New(FormsRepository repository, Company company, User user, IEmailHolder holder, Guid? owner = null, Guid? group = null, string permission = "770")
        {
            if (holder == null)
                return null;
            var email = new RSHtmlEmail()
            {
                UId = Guid.NewGuid(),
                Company = company,
                CompanyKey = company.UId,
                Html = "",
                PlainText = ""
            };
            if (holder is Form)
            {
                email.Form = (Form)holder;
                email.FormKey = holder.UId;
                holder.HtmlEmails.Add(email);
            }
            else if (holder is EmailCampaign)
            {
                email.EmailCampaign = (EmailCampaign)holder;
                email.EmailCampaignKey = holder.UId;
                holder.HtmlEmails.Add(email);
            }
            else
            {
                return null;
            }
            repository.Add(email);
            repository.Commit();
            return email;
        }

        public INode GetNode()
        {
            return Form as INode ?? EmailCampaign as INode;
        }

        public RSHtmlEmail DeepCopy(Form form = null, EmailCampaign campaign = null)
        {
            var email = new RSHtmlEmail();
            email.Form = form;
            email.EmailCampaign = campaign;
            if (form != null)
                form.HtmlEmails.Add(email);
            else if (campaign != null)
                campaign.HtmlEmails.Add(email);
            email.BCC = BCC;
            email.CC = CC;
            email.Company = form.Company;
            email.ContactReport = ContactReport;
            email.DateCreated = DateCreated;
            email.DateModified = email.DateModified;
            email.Description = Description;
            email.EmailCampaign = campaign;
            email.EmailType = EmailType;
            email.Form = form;
            email.From = From;
            email.IntervalSeconds = IntervalSeconds;
            email.IntervalTicks = IntervalTicks;
            email.MaxSends = MaxSends;
            email.Name = Name;
            email.SavedList = SavedList;
            email.SendInterval = SendInterval;
            email.Subject = Subject;
            email.To = To;
            email.UId = Guid.NewGuid();
            foreach (var logic in Logics)
            {
                logic.DeepCopy(email, form, Form);
            }
            return email;
        }

        public virtual IEnumerable<Logic> DeepCopyLogics(Form form, Form oldForm)
        {
            var logics = new List<Logic>();
            foreach (var logic in Logics)
                logics.Add(logic.DeepCopy(this, form, oldForm));
            return logics;
        }

        #region methods

        public string Render()
        {
            return Html;
        }

        #endregion

        public EmailData GenerateEmail(RSParser parser)
        {
            var message = new EmailData();
            message.Header = new SendGridHeader();
            message.Header.category.Add(EmailType.GetStringValue());
            message.Header.unique_args.Add("email-uid", UId.ToString());
            message.Type = EmailType.GetStringValue();
            message.EmailId = UId;
            /* Set the subject */
            if (!String.IsNullOrWhiteSpace(Subject))
                message.Subject = parser.ParseAvailable(Subject);
            else
                message.Subject = EmailType.GetStringValue();
            /* We need to render the email and parse it. */
            var htmlBody = parser.ParseAvailable(Render());
            // We need to move any img styles into the html attributes.
            var plainBody = parser.ParseAvailable(PlainText ?? "");
            message.Text = plainBody;
            message.Html = htmlBody;
            /* Create and parse all To, CC, and BCC. */
            var rgx_Name = new Regex(@"{(?<email>[^,]*),\s*(?<name>[^}]*)}", RegexOptions.IgnoreCase);
            foreach (var to in To.Split(';').Where(s => !String.IsNullOrWhiteSpace(s)))
            {
                try
                {
                    if (rgx_Name.IsMatch(to))
                    {
                        var match = rgx_Name.Match(to);
                        message.To.Add(new MailAddress(parser.ParseAvailable(match.Groups["email"].Value.Trim()), parser.ParseAvailable(match.Groups["name"].Value.Trim())));
                    }
                    else
                    {
                        message.To.Add(new MailAddress(parser.ParseAvailable(to.Trim())));
                    }
                }
                catch (Exception) { }
            }
            foreach (var cc in CC.Split(';').Where(s => !String.IsNullOrWhiteSpace(s)))
            {
                try
                {
                    if (rgx_Name.IsMatch(cc))
                    {
                        var match = rgx_Name.Match(cc);
                        message.CC.Add(new MailAddress(parser.ParseAvailable(match.Groups["email"].Value.Trim()), parser.ParseAvailable(match.Groups["name"].Value.Trim())));
                    }
                    else
                    {
                        message.CC.Add(new MailAddress(parser.ParseAvailable(cc.Trim())));
                    }
                }
                catch (Exception) { }
            }
            foreach (var bcc in BCC.Split(';').Where(s => !String.IsNullOrWhiteSpace(s)))
            {
                try
                {
                    if (rgx_Name.IsMatch(bcc))
                    {
                        var match = rgx_Name.Match(bcc);
                        message.BCC.Add(new MailAddress(parser.ParseAvailable(match.Groups["email"].Value.Trim()), parser.ParseAvailable(match.Groups["name"].Value.Trim())));
                    }
                    else
                    {
                        message.BCC.Add(new MailAddress(parser.ParseAvailable(bcc.Trim())));
                    }
                }
                catch (Exception) { }
            }
            message.ReplyToList.Clear();
            try
            {
                if (rgx_Name.IsMatch(From))
                {
                    var match = rgx_Name.Match(From);
                    var address = new MailAddress(parser.ParseAvailable(match.Groups["email"].Value.Trim()), parser.ParseAvailable(match.Groups["name"].Value.Trim()));
                    message.From = address;
                    message.ReplyToList.Add(address);
                }
                else
                {
                    var address = new MailAddress(parser.ParseAvailable(String.IsNullOrWhiteSpace(From) ? "no_reply@regstep.com" : From));
                    message.From = address;
                    message.ReplyToList.Add(address);
                }
            }
            catch (Exception) { }
            message = SmtpServer.GenerateFooter(message, this);
            return message;
        }
    }
}
