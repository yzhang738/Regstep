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

// Complete
namespace RSToolKit.Domain.Entities.Email
{
    public class RSEmail : IEmail
    {
        [JsonIgnore]
        [CascadeDelete]
        public virtual List<Logic> Logics { get; set; }

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

        [JsonIgnore]
        [ForeignKey("CompanyKey")]
        public virtual Company Company { get; set; }
        public Guid CompanyKey { get; set; }

        [ForeignKey("EmailTemplateKey")]
        public virtual EmailTemplate Template { get; set; }
        public Guid? EmailTemplateKey { get; set; }

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

        [CascadeDelete]
        public virtual List<EmailArea> EmailAreas { get; set; }
        [CascadeDelete]
        public virtual List<EmailVariable> Variables { get; set; }

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

        public RSEmail()
        {
            Description = "";
            EmailAreas = new List<EmailArea>();
            Logics = new List<Logic>();
            EmailType = EmailType.Unclassified;
            Variables = new List<EmailVariable>();
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
     
        public static RSEmail New(FormsRepository repository, Company company, User user, IEmailHolder holder, EmailTemplate template, Guid? owner = null, Guid? group = null, string permission = "770")
        {
            if (holder == null)
                return null;
            if (template == null)
                return null;
            var email = new RSEmail()
            {
                UId = Guid.NewGuid(),
                Company = company,
                CompanyKey = company.UId,
            };
            email.SetTemplate(template);
            if (holder is Form)
            {
                email.Form = (Form)holder;
                email.FormKey = holder.UId;
                holder.Emails.Add(email);
            }
            else if (holder is EmailCampaign)
            {
                email.EmailCampaign = (EmailCampaign)holder;
                email.EmailCampaignKey = holder.UId;
                holder.Emails.Add(email);
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

        #region methods

        public string Render()
        {
            var html = Template.EmailAreas.Where(ea => ea.Type == "html").FirstOrDefault().Html;
            var areas = EmailAreas.Where(ea => ea.Type != "html").Select(ea => ea.Type).Distinct().ToList();
            foreach (var area in areas)
            {
                var t_areas = EmailAreas.Where(ea => ea.Type == area).OrderBy(ea => ea.Order).ToList();
                var t_t_area = Template.EmailAreas.Where(ea => ea.Type == area).FirstOrDefault();
                var t_html = "";
                foreach (var t_area in t_areas)
                {
                    var t_a_html = t_t_area.Html;
                    t_a_html = t_area.Render(t_a_html);
                    foreach (var variable in t_area.Variables)
                    {
                        t_a_html = Regex.Replace(t_a_html, @"@render_var_" + variable.Variable, String.IsNullOrWhiteSpace(variable.Value) ? "" : variable.Value, RegexOptions.IgnoreCase);
                    }
                    t_html += t_a_html;
                }
                html = Regex.Replace(html, @"@render_" + area, t_html, RegexOptions.IgnoreCase);
            }
            foreach (var variable in Variables)
            {
                html = Regex.Replace(html, @"@render_var_" + variable.Variable, String.IsNullOrWhiteSpace(variable.Value) ? "" : variable.Value, RegexOptions.IgnoreCase);
            }
            return html;
        }

        public MailMessage GetMailMessage()
        {
            var message = new MailMessage();
            string email = Render();
            message.Body = email;
            message.IsBodyHtml = true;
            message.Subject = Subject;
            return message;
        }

        public bool SetTemplate(EmailTemplate template)
        {
            if (template == null)
                return false;
            Template = template;
            EmailAreas.Clear();
            Variables.Clear();
            foreach (var v in template.Variables)
            {
                Variables.Add(new EmailVariable() { Description = v.Description, Name = v.Name, Variable = v.Variable, Value = v.Value, Type = v.Type });
            }
            foreach (var a in template.EmailAreas.Where(a => a.Type != "html"))
            {
                var area = EmailArea.New(this, a.Type, a.Default, 1);
                foreach (var v in a.Variables)
                {
                    area.Variables.Add(new EmailAreaVariable() { Description = v.Description, Name = v.Name, Type = v.Type, Variable = v.Variable, Value = v.Value });
                }
                EmailAreas.Add(area);
            }
            return true;
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
            htmlBody = Regex.Replace(htmlBody, @"<!--editorignore-->", "", RegexOptions.IgnoreCase);
            htmlBody = Regex.Replace(htmlBody, @"<!--endeditorignore-->", "", RegexOptions.IgnoreCase);
            // We need to move any img styles into the html attributes.
            var agile_htmlBody = new HtmlDocument();
            agile_htmlBody.LoadHtml(htmlBody);
            var images = agile_htmlBody.DocumentNode.SelectNodes("//img");
            if (images != null)
            {
                foreach (var element in images)
                {
                    var style = element.Attributes.Where(a => a.Name == "style").FirstOrDefault();
                    var width = element.Attributes.Where(a => a.Name == "width").FirstOrDefault();
                    var height = element.Attributes.Where(a => a.Name == "height").FirstOrDefault();
                    if (height != null)
                        element.Attributes.Remove(height);
                    if (style != null)
                    {
                        var v_width = "";
                        var w_match = Regex.Match(style.Value, @"width:\s*(?<value>[^;$]*)");
                        if (Regex.IsMatch(style.Value, @"height:\s*(?<value>[^;$]*)"))
                            style.Value = Regex.Replace(style.Value, @"height:\s*(?<value>[^;$]*)", "");
                        if (w_match != null)
                            v_width = w_match.Groups["value"].Value;
                        if (width == null)
                            element.Attributes.Add("width", Regex.Replace(v_width, @"\D", ""));
                    }
                    else
                    {
                        element.Attributes.Add("style", "");
                        style = element.Attributes.Where(a => a.Name == "style").FirstOrDefault();
                    }
                    if (!Regex.IsMatch(style.Value, @"display:\s*(?<value>[^;$]*)"))
                        style.Value = "display: block;" + style.Value;
                    if (!Regex.IsMatch(style.Value, @"border:\s*(?<value>[^;$]*)"))
                        style.Value = "border: none;" + style.Value;
                    if (!Regex.IsMatch(style.Value, @"outline:\s*(?<value>[^;$]*)"))
                        style.Value = "outline: none;" + style.Value;
                }
            }
            htmlBody = agile_htmlBody.DocumentNode.InnerHtml;
            var plainBody = parser.ParseAvailable(PlainText ?? "");
            message.Text = plainBody;
            message.Html = htmlBody;
            var rgx_Name = new Regex(@"{(?<email>[^,]*),\s*(?<name>[^}]*)}", RegexOptions.IgnoreCase);
            if (!String.IsNullOrWhiteSpace(To))
            {
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
            }
            if (!String.IsNullOrWhiteSpace(CC))
            {
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
            }
            if (!String.IsNullOrWhiteSpace(BCC))
            {
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
            }
            message.ReplyToList.Clear();
            if (String.IsNullOrWhiteSpace(From))
                From = "no_reply@regstep.com";
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

        public RSEmail DeepCopy(Form form = null, EmailCampaign campaign = null)
        {
            var email = new RSEmail();
            email.Form = form;
            email.EmailCampaign = campaign;
            if (form != null)
                form.Emails.Add(email);
            else if (campaign != null)
                campaign.Emails.Add(email);
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
            email.Template = Template;
            email.To = To;
            email.UId = Guid.NewGuid();
            foreach (var logic in Logics)
            {
                logic.DeepCopy(email, form, Form);
            }
            foreach (var area in EmailAreas)
            {
                area.DeepCopy(email);
            }
            foreach (var variable in Variables)
            {
                variable.DeepCopy(email);
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

    }

    public enum EmailType
    {
        [StringValue("Unclassified")]
        Unclassified = 0,
        [Form]
        [StringValue("Invitation")]
        Invitation = 1,
        [Form]
        [StringValue("Confirmation")]
        Confirmation = 2,
        [Form]
        [StringValue("Credit Card Receipt")]
        CreditCardReciept = 3,
        [Form]
        [StringValue("Bill Me Invoice")]
        BillMeInvoice = 4,
        [Form]
        [StringValue("Paying Agent")]
        PayingAgent = 5,
        [Form]
        [StringValue("Cancellation")]
        Cancellation = 6,
        [Form]
        [StringValue("Continue")]
        Continue = 7,
        [Form]
        [StringValue("Registered")]
        Registered = 8,
        [Form]
        [StringValue("Not Registered")]
        NotRegistered = 9,
        [Form]
        [StringValue("RSVP Yes")]
        Attending = 10,
        [Form]
        [StringValue("RSVP No")]
        NotAttending = 11,
        [StringValue("Custom")]
        Custom = 12,
        [Form]
        [StringValue("Incomplete")]
        Incomplete = 13,
        [Form]
        [StringValue("Share Email")]
        ShareEmail = 14
    }
}
