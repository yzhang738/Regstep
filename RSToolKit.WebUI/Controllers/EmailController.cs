using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using RSToolKit.WebUI.Infrastructure;
using RSToolKit.Domain.Entities;
using RSToolKit.Domain.Data;
using RSToolKit.Domain.Entities.Email;
using System.Linq;
using System.Data.Entity;
using RSToolKit.Domain.Errors;
using Newtonsoft.Json;
using RSToolKit.Domain.Security;
using RSToolKit.WebUI.Models;
using RSToolKit.Domain.Entities;
using System.Net.Mail;
using RSToolKit.Domain;
using RSToolKit.WebUI.Controllers.API;
using RSToolKit.Domain.Engines;
using RSToolKit.Domain.JItems;

namespace RSToolKit.WebUI.Controllers
{
    [Authorize(Roles = "Super Administrators,Email Builders,Programmers")]
    public class EmailController : RSController
    {

        public EmailController(EFDbContext context)
            : base(context)
        {
            iLog.LoggingMethod = "EmailController";
        }

        [HttpGet]
        public ActionResult Index()
        {
            List<IEmailHolder> holder = new List<IEmailHolder>();
            using (var context = new EFDbContext())
            {
                var forms = context.Forms.Where(f => f.CompanyKey == company.UId).ToList().ToArray();
                var emailCampaigns = context.EmailCampaigns.Where(e => e.CompanyKey == company.UId).ToList().ToArray();
                holder.AddRange(emailCampaigns);
                holder.AddRange(forms);
            }
            return View(holder);
        }

        #region Universal

        [HttpPost]
        [ActionName("SendNow")]
        [JsonValidateAntiForgeryToken]
        public JsonNetResult SendNow(Guid id)
        {
            var apiController = new Email_SendController(Context);
            var data = apiController.Post(new api_EmailRequest() { id = id }) as System.Web.Http.Results.OkNegotiatedContentResult<api_EmailResponse>;
            if (data == null)
                return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = false, Message = "No emails sent." } };
            var result = data.Content;
            return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = result };
        }

        [HttpPost]
        [ActionName("SendEmailType")]
        [JsonValidateAntiForgeryToken]
        public JsonNetResult SendEmailType(string formId, EmailType emailType)
        {
            var apiController = new Email_SendController(Context);
            var emailsSent = 0;
            Form form = null;
            var id = 0L;
            var uid = Guid.Empty;
            if (Guid.TryParse(formId, out uid))
                form = Repository.Search<Form>(f => f.UId == uid).FirstOrDefault();
            if (long.TryParse(formId, out id))
                form = Repository.Search<Form>(f => f.SortingId == id).FirstOrDefault();

            if (form == null)
                return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new api_EmailResponse() { Message = "Server Error: Form does not exist. " } };

            var emails = form.AllEmails.Where(e => e.EmailType == emailType);
            foreach (var email in emails)
            {
                var data = apiController.Post(new api_EmailRequest() { id = email.UId }) as System.Web.Http.Results.OkNegotiatedContentResult<api_EmailResponse>;
                if (data == null)
                    continue;
                var result = data.Content;
                emailsSent += result.Sends;
            }

            var response = new api_EmailResponse();
            response.Sends = emailsSent;
            response.SuccessMessage = emailsSent + " email" + (emailsSent != 1 ? "s where" : " was") + " where sent.";
            return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = response };
        }

        [HttpDelete]
        [ActionName("Email")]
        [JsonValidateAntiForgeryToken]
        public JsonNetResult DeleteEmail(Guid id)
        {
            var ehid = Guid.Empty;
            var email = Repository.Search<IEmail>(e => e.UId == id).FirstOrDefault();
            if (email == null)
                return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = false, Message = "Invalid Object." } };
            if (email is RSEmail)
                Repository.Remove((RSEmail)email);
            else if (email is RSHtmlEmail)
                Repository.Remove((RSHtmlEmail)email);
            Repository.Commit();
            return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = true, Location = "refresh" } };
        }

        [HttpPost]
        [ActionName("Email")]
        [JsonValidateAntiForgeryToken]
        public JsonNetResult NewEmail(Guid id, Guid? templateId)
        {
            IEmailHolder holder = null;
            holder = Repository.Search<IEmailHolder>(eh => eh.UId == id).FirstOrDefault();
            if (holder == null)
                return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = false, Message = "Invalid Object" } };
            EmailTemplate template = null;
            if (templateId.HasValue)
                template = Repository.Search<EmailTemplate>(t => t.UId == templateId.Value).FirstOrDefault();
            IEmail email = null;
            if (template == null)
                email = RSHtmlEmail.New(Repository, company, user, holder);
            else
                email = RSEmail.New(Repository, company, user, holder, template);
            if (email == null)
                return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = false, Message = "Unknown Error Occured" } };
            Repository.Commit();
            return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = true, Location = Url.Action("Email", "Email", new { id = email.UId }, Request.Url.Scheme) } };
        }

        [HttpGet]
        [ActionName("Email")]
        public ActionResult EditEmail(Guid id)
        {
            var email = Repository.Search<IEmail>(e => e.UId == id).FirstOrDefault();
            if (email == null)
                return RedirectToAction("Index");
            ViewBag.Repository = Repository;
            if (email is RSHtmlEmail)
                return View("EditHtmlEmail", email);
            return View("EditEmail", email);
        }

        [HttpPut]
        [ActionName("Email")]
        [JsonValidateAntiForgeryToken]
        public JsonResult EditEmail(RSEmail model)
        {
            var email = Repository.Search<RSEmail>(e => e.UId == model.UId).FirstOrDefault();
            if (email == null)
                return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = false, Message = "Invalid Object." } };
            // Update email data
            if (model.EmailListKey.HasValue)
            {
                var list = Repository.Search<IEmailList>(l => l.UId == model.EmailListKey.Value).FirstOrDefault();
                if (list == null)
                {
                    email.SavedList = null;
                    email.SavedListKey = null;
                    email.ContactReport = null;
                    email.ContactReportKey = null;
                }
                else if (list is ContactReport)
                {
                    email.SavedList = null;
                    email.SavedListKey = null;
                    email.ContactReport = (ContactReport)list;
                    email.ContactReportKey = list.UId;
                }
                else if (list is SavedList)
                {
                    email.SavedList = (SavedList)list;
                    email.SavedListKey = list.UId;
                    email.ContactReport = null;
                    email.ContactReportKey = null;
                }
            }
            else
            {
                email.SavedList = null;
                email.SavedListKey = null;
                email.ContactReport = null;
                email.ContactReportKey = null;
            }
            email.PlainText = model.PlainText ?? "";
            email.To = model.To ?? "";
            email.From = model.From ?? "no_reply@regstep.com";
            email.CC = model.CC ?? "";
            email.BCC = model.BCC ?? "";
            email.Description = model.Description ?? "";
            email.EmailType = model.EmailType;
            email.IntervalSeconds = model.IntervalSeconds;
            email.MaxSends = model.MaxSends;
            email.Name = model.Name ?? "Email " + user.UserName;
            email.SendTime = model.SendTime;
            email.Subject = model.Subject ?? "No Subject";
            if (model.IntervalSeconds != 0)
                email.SendInterval = TimeSpan.FromSeconds(model.IntervalSeconds);
            else
                email.SendInterval = null;

            //Update areas and add new ones as needed.
            foreach (var area in model.EmailAreas)
            {
                var d_area = email.EmailAreas.Where(ea => ea.SortingId == area.SortingId).FirstOrDefault();
                if (d_area == null)
                {
                    d_area = EmailArea.New(email, area.Type, area.Html, area.Order);
                    foreach (var variable in area.Variables)
                    {
                        d_area.Variables.Add(new EmailAreaVariable() { Description = variable.Description, Name = variable.Name, Type = variable.Type, Variable = variable.Variable, Value = variable.Value });
                    }
                }
                foreach (var variable in area.Variables)
                {
                    var d_variable = d_area.Variables.FirstOrDefault(v => v.Variable == variable.Variable);
                    if (d_variable != null)
                        d_variable.Value = variable.Value;
                }
                d_area.Order = area.Order;
                d_area.Html = area.Html;
                d_area.Type = area.Type;
            }

            //Delete areas no longer in use
            foreach (var area in email.EmailAreas.ToList())
            {
                if (area.SortingId != 0 && model.EmailAreas.Count(ea => ea.SortingId == area.SortingId) == 0)
                {
                    Repository.Remove(area);
                }
            }

            foreach (var variable in model.Variables)
            {
                var d_variable = email.Variables.FirstOrDefault(v => v.Variable == variable.Variable);
                if (d_variable != null)
                    d_variable.Value = variable.Value;
            }
            Repository.Commit();

            return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = true, Location = Url.Action("Email", "Email", new { id = email.UId }, Request.Url.Scheme) } };
        }

        [HttpPut]
        [ActionName("HtmlEmail")]
        [JsonValidateAntiForgeryToken]
        public JsonResult EditHtmlEmail(RSHtmlEmail model)
        {
            var email = Repository.Search<RSHtmlEmail>(e => e.UId == model.UId).FirstOrDefault();
            if (email == null)
                return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = false, Message = "Invalid Object." } };
            // Update email data
            if (model.EmailListKey.HasValue)
            {
                var list = Repository.Search<IEmailList>(l => l.UId == model.EmailListKey.Value).FirstOrDefault();
                if (list == null)
                {
                    email.SavedList = null;
                    email.SavedListKey = null;
                    email.ContactReport = null;
                    email.ContactReportKey = null;
                }
                else if (list is ContactReport)
                {
                    email.SavedList = null;
                    email.SavedListKey = null;
                    email.ContactReport = (ContactReport)list;
                    email.ContactReportKey = list.UId;
                }
                else if (list is SavedList)
                {
                    email.SavedList = (SavedList)list;
                    email.SavedListKey = list.UId;
                    email.ContactReport = null;
                    email.ContactReportKey = null;
                }
            }
            else
            {
                email.SavedList = null;
                email.SavedListKey = null;
                email.ContactReport = null;
                email.ContactReportKey = null;
            }
            email.PlainText = model.PlainText ?? "";
            email.To = model.To ?? "";
            email.From = model.From ?? "no_reply@regstep.com";
            email.CC = model.CC ?? "";
            email.BCC = model.BCC ?? "";
            email.Description = model.Description ?? "";
            email.EmailType = model.EmailType;
            email.IntervalSeconds = model.IntervalSeconds;
            email.MaxSends = model.MaxSends;
            email.Name = model.Name ?? "Email " + user.UserName;
            email.SendTime = model.SendTime;
            email.Subject = model.Subject ?? "No Subject";
            email.Html = model.Html;

            Repository.Commit();

            return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = true, Location = Url.Action("Email", "Email", new { id = email.UId }, Request.Url.Scheme) } };
        }

        [HttpGet]
        [ActionName("Emails")]
        public ActionResult GetEmails(Guid id)
        {
            IEmailHolder holder = Repository.Search<EmailCampaign>(ec => ec.UId == id).FirstOrDefault();
            if (holder != null)
                return ViewEmailCampaign(id);
            holder = Repository.Search<Form>(f => f.UId == id).FirstOrDefault();
            if (holder != null)
                return FormEmails(id);
            return RedirectToAction("Error404", "Error");
        }

        [HttpPost]
        [JsonValidateAntiForgeryToken]
        public JsonNetResult TestSend(Guid id, string to, Guid? smtpServer = null)
        {
            var email = Repository.Search<IEmail>(e => e.UId == id).FirstOrDefault();
            if (email == null)
                return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = false, Message = "Invalid Object" } };
            SmtpServer server = null;
            if (smtpServer == null)
                server = Repository.Search<SmtpServer>(s => s.CompanyKey == company.UId && s.UId == smtpServer).FirstOrDefault();
            if (server == null)
                server = Repository.Search<SmtpServer>(s => s.CompanyKey == company.UId && s.Primary).FirstOrDefault();
            if (server == null)
                server = Repository.Search<SmtpServer>(s => s.Name == "Primary").FirstOrDefault();
            if (server == null)
                return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = false, Message = "Invalid Smtp Server" } };
            to = String.IsNullOrWhiteSpace(to) ? user.Email : to;
            var parser = new RSParser(repository: Repository);
            if (email.Form != null)
                parser.Form = email.Form;
            if (email.EmailCampaign != null)
                parser.EmailCampaign = email.EmailCampaign;
            var message = email.GenerateEmail(parser);
            MailAddress ma_to = null;
            try
            {
                ma_to = new MailAddress(to);
            }
            catch (Exception) { }
            if (ma_to != null)
            {
                message.To.Clear();
                message.To.Add(ma_to);
            }
            server.SendEmail(message, Repository, company, email.Form == null ? (INamedNode)email.EmailCampaign : (INamedNode)email.Form);
            return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = true } };
        }

        [HttpGet]
        public ActionResult PreviewEmail(Guid id)
        {
            var email = Repository.Search<IEmail>(e => e.UId == id).FirstOrDefault();
            if (email == null)
                return RedirectToAction("Index");
            var parser = new RSParser(repository: Repository);
            if (email.Form != null)
                parser.Form = email.Form;
            if (email.EmailCampaign != null)
                parser.EmailCampaign = email.EmailCampaign;
            var html = email.GenerateEmail(parser).Html;
            return View(new MvcHtmlString(html));
        }

        [HttpPut]
        [JsonValidateAntiForgeryToken]
        public JsonNetResult SendCustomEmail(Guid id)
        {
            var email = Repository.Search<RSEmail>(e => e.UId == id).FirstOrDefault();
            IEmailHolder parent = email.Form;
            if (parent == null)
                parent = email.EmailCampaign;
            if (email == null)
                return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = false, Message = "Invalid Object." } };
            if (email.Logics.Count < 1)
                return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = false, Message = "No Logic" } };
            var smtpServer = Repository.Search<SmtpServer>(s => s.CompanyKey == parent.CompanyKey).FirstOrDefault();
            if (smtpServer == null)
                smtpServer = Repository.Search<SmtpServer>(s => s.Name == "Primary").FirstOrDefault();
            if (smtpServer == null)
                return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = false, Message = "No Smtp Server." } };
            var allPersons = new List<IPerson>();
            RSParser parser = null;
            if (parent is Form)
            {
                var form = (Form)parent;
                allPersons = form.Registrants.ToList<IPerson>();
                parser = new RSParser(repository: Repository, form: form);
            }
            else if (parent is EmailCampaign)
            {
                var campaign = (EmailCampaign)parent;
                if (email.EmailList == null)
                    allPersons = Repository.Search<Contact>(c => c.CompanyKey == company.UId).ToList<IPerson>();
                else
                    allPersons = email.EmailList.GetAllEmails(Repository).ToList();
                parser = new RSParser(repository: Repository, campaign: campaign);
            }
            foreach (var person in allPersons)
            {
                var work = LogicEngine.RunLogic(email, Repository, registrant: person as Registrant, contact: person as Contact);
                //var work = email.RunLogic(person, true, Repository).ToList();
                if (work.Count() > 0 && work.First().Command != JLogicWork.SendEmail)
                    continue;
                if (parent is Form)
                    parser.Registrant = (Registrant)person;
                else
                    parser.Contact = (Contact)person;
                var message = email.GenerateEmail(parser);
                smtpServer.SendEmail(message, Repository, company, parent, person, null, email.EmailType.GetStringValue());
            }
            return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = true } };
        }

        #endregion

        #region Logic

        [HttpGet]
        public ActionResult Logics(Guid id)
        {
            var email = Repository.Search<IEmail>(e => e.UId == id).FirstOrDefault();
            if (email == null)
                return RedirectToAction("Index");
            return View(email);
        }

        [HttpPost]
        [ActionName("Logic")]
        [JsonValidateAntiForgeryToken]
        public ActionResult NewLogic(Guid id)
        {
            ILogicHolder parent = Repository.Search<ILogicHolder>(l => l.UId == id).FirstOrDefault();
            if (parent == null)
                return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = false, Message = "Invalid Object." } };
            var logic = Logic.New(Repository, parent, company, user, incoming: true);
            return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = true, Location = Url.Action("Logic", "Email", new { id = logic.UId }, Request.Url.Scheme) } };
        }

        [HttpGet]
        [ActionName("Logic")]
        public ActionResult EditLogic(Guid id)
        {
            var logic = Repository.Search<Logic>(c => c.UId == id).FirstOrDefault();
            if (logic == null)
                return RedirectToAction("Index");
            IEmailHolder holder = logic.TheEmail.Form;
            if (holder == null)
                holder = logic.TheEmail.EmailCampaign;
            foreach (var group in logic.LogicGroups)
            {
                foreach (var statement in group.LogicStatements)
                {
                    switch (statement.Variable.ToLower())
                    {
                        case "audience":
                            var audience = statement.Form.Audiences.FirstOrDefault(a => a.UId == Guid.Parse(statement.Value));
                            if (audience != null)
                                statement.ValueName = audience.Name;
                            break;
                        case "status":
                            statement.ValueName = ((RegistrationStatus)(int.Parse(statement.Value))).GetStringValue();
                            break;
                        case "type":
                            statement.ValueName = ((RegistrationType)(int.Parse(statement.Value))).GetStringValue();
                            break;
                        default:
                            statement.ValueName = statement.Form.GetValueName(statement.Variable, statement.Value);
                            break;
                    }
                    statement.VariableName = statement.Form.GetVariableName(statement.Variable);
                }
            }
            if (holder is Form)
                return View("FormLogic", logic);
            else
                return View("CampaignLogic", logic);
        }

        [HttpPut]
        [ActionName("Logic")]
        [JsonValidateAntiForgeryToken]
        public JsonNetResult EditLogic(Logic model)
        {
            var logic = Repository.Search<Logic>(c => c.UId == model.UId).FirstOrDefault();
            if (logic == null)
                return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = false, Message = "Invalid Object" } };
            logic.Incoming = model.Incoming;
            // Now we iterate through the logic groups send over the header
            foreach (var h_group in model.LogicGroups)
            {
                var group = logic.LogicGroups.Where(l => l.UId == h_group.UId).FirstOrDefault();
                if (group == null)
                {
                    h_group.UId = Guid.NewGuid();
                    logic.LogicGroups.Add(h_group);
                }
                else
                {
                    group.Link = h_group.Link;
                    group.Order = h_group.Order;
                    foreach (var h_statement in h_group.LogicStatements)
                    {
                        var statement = group.LogicStatements.Where(l => l.UId == h_statement.UId).FirstOrDefault();
                        if (statement == null)
                        {
                            h_statement.UId = Guid.NewGuid();
                            group.LogicStatements.Add(h_statement);
                        }
                        else
                        {
                            statement.Order = h_statement.Order;
                            statement.Link = h_statement.Link;
                            statement.FormKey = h_statement.FormKey;
                            statement.Variable = h_statement.Variable;
                            statement.Value = h_statement.Value;
                            statement.VariableName = h_statement.VariableName;
                            statement.ValueName = h_statement.ValueName;
                            statement.Test = h_statement.Test;
                        }
                    }
                }
            }
            for (var i = 0; i < logic.LogicGroups.Count; i++)
            {
                var group = logic.LogicGroups[i];
                var h_group = model.LogicGroups.Where(g => g.UId == group.UId).FirstOrDefault();
                if (h_group == null)
                {
                    Repository.Remove(group);
                    i--;
                }
                else
                {
                    for (var j = 0; j < group.LogicStatements.Count; j++)
                    {
                        var statement = group.LogicStatements[j];
                        var h_statement = h_group.LogicStatements.Where(s => s.UId == statement.UId).FirstOrDefault();
                        if (h_statement == null)
                        {
                            Repository.Remove(statement);
                            i--;
                        }
                    }
                }
            }
            foreach (var h_command in model.Commands)
            {
                var command = logic.Commands.FirstOrDefault(l => l.UId == h_command.UId);
                if (command == null)
                {
                    h_command.UId = Guid.NewGuid();
                    logic.Commands.Add(h_command);
                }
                else
                {
                    command.Order = h_command.Order;
                    command.Parameters = h_command.Parameters;
                    command.FormKey = h_command.FormKey;
                    command.CommandType = h_command.CommandType;
                    command.Command = h_command.Command;
                }
            }
            for (var i = 0; i < logic.Commands.Count; i++)
            {
                var command = logic.Commands[i];
                var h_command = model.Commands.FirstOrDefault(c => c.UId == command.UId);
                if (h_command == null)
                {
                    Repository.Remove(command);
                    i--;
                }
            }
            Repository.Commit();
            return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = true, Errors = false } };
        }

        [HttpDelete]
        [ActionName("Logic")]
        [JsonValidateAntiForgeryToken]
        public JsonNetResult DeleteLogic(Guid id)
        {
            var logic = Repository.Search<Logic>(l => l.UId == id).FirstOrDefault();
            if (logic == null)
                return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = false, Message = "Invalid Object" } };
            var email = logic.Email;
            IEmailHolder holder = logic.Email.Form;
            if (holder == null)
                holder = logic.Email.EmailCampaign;
            Repository.Remove(logic);
            int order = 1;
            foreach (var lgc in email.Logics.OrderBy(l => l.Order))
                lgc.Order = order++;
            Repository.Commit();
            return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = true, Location = Url.Action("Logics", "Email", new { id = holder.UId }) } };
        }

        #endregion

        #region Email Form

        [HttpGet]
        public ActionResult Forms()
        {
            var forms = Repository.Search<Form>(f => f.CompanyKey == company.UId).ToList();
            return View(forms);
        }

        [HttpGet]
        public ActionResult FormEmails(Guid id)
        {
            var form = Repository.Search<Form>(f => f.UId == id).FirstOrDefault();
            if (form == null)
                return RedirectToAction("Error404", "error");
            var model = new FormEmailsModel()
            {
                Form = form
            };
            foreach (var email in form.Emails)
                model.Emails.Add(email);
            return View(model);
        }

        #endregion

        #region Email Campaign

        [HttpGet]
        [ActionName("EmailCampaigns")]
        public ActionResult EmailCampaigns()
        {
            var campaigns = Repository.Search<EmailCampaign>(e => e.CompanyKey == company.UId).ToList();
            campaigns = campaigns.OrderByDescending(e => e.DateCreated).ToList();
            return View(campaigns);
        }

        [HttpPost]
        [ActionName("EmailCampaign")]
        [ValidateAntiForgeryToken]
        public JsonNetResult NewEmailCampaign()
        {
            var campaign = EmailCampaign.New(Repository, company, user);
            Repository.Add(campaign);
            Repository.Commit();
            return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = true, Location = Url.Action("EmailCampaign", "Email", new { id = campaign.UId }) } };
        }

        [HttpGet]
        [ActionName("EmailCampaign")]
        public ActionResult ViewEmailCampaign(Guid id)
        {
            EmailCampaign emailCampaign = null;
            emailCampaign = Repository.Search<EmailCampaign>(ec => ec.UId == id).FirstOrDefault();
            if (emailCampaign == null)
                throw new EmailException("Invalid Email Campaign.");
            return View(emailCampaign);
        }

        [HttpPut]
        [ActionName("EmailCampaign")]
        [ValidateAntiForgeryToken]
        public ActionResult SaveEmailCampaign(EmailCampaign ec)
        {
            if (!ModelState.IsValid)
                return View(ec);
            EmailCampaign emailCampaign = null;
            emailCampaign = Repository.Search<EmailCampaign>(e => e.UId == e.UId).FirstOrDefault();
            if (emailCampaign == null)
                throw new EmailException("Invalid Email Campaign.");
            emailCampaign.Name = ec.Name;
            emailCampaign.Tags = ec.Tags;
            emailCampaign.Description = ec.Description;
            Repository.Commit();
            return View(emailCampaign);
        }

        #endregion

        [HttpGet]
        [ComplexId("id")]
        public JsonNetResult Events(object id)
        {
            EmailSend send = null;
            if (id is Guid)
                send = Repository.Search<EmailSend>(s => s.UId == (Guid)id).FirstOrDefault();
            if (id is long)
                send = Repository.Search<EmailSend>(s => s.SortingId == (long)id).FirstOrDefault();
            if (send == null)
                return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = false, Message = "Invalid Object" } };
            var recip = send.Recipient;
            if (send.Registrant != null)
                recip = send.Registrant.UId.ToString();
            else if (send.Contact != null)
                recip = send.Contact.UId.ToString();
            return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = true, Id = send.EmailKey, Recipient = recip, Events = send.EmailEvents.OrderBy(e => e.Date).ToList() } };
        }

        [HttpGet]
        public JsonNetResult EmailList(Guid id)
        {
            var send = Repository.Search<EmailSend>(s => s.UId == id).FirstOrDefault();
            if (send == null)
                return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = false, Message = "Invalid Object" } };

            var sends = new List<EmailSend>();
            IEmail email = null;
            if (send.EmailKey.HasValue)
                email = Repository.Search<IEmail>(e => e.UId == send.EmailKey.Value).FirstOrDefault();
            if (email != null)
                sends = Repository.Search<EmailSend>(e => e.EmailKey == send.EmailKey && ((e.RegistrantKey != null && e.RegistrantKey == send.RegistrantKey) || (e.ContactKey != null && e.ContactKey == send.ContactKey))).ToList();

            if (sends.Count == 0)
                sends.Add(send);

            var recip = send.Recipient;
            if (send.Registrant != null)
                recip = send.Registrant.UId.ToString();
            else if (send.Contact != null)
                recip = send.Contact.UId.ToString();
            var t_data = new { Sends = sends };
            return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = true, Id = send.EmailKey, Recipient = recip, Sends = sends.OrderBy(e => e.DateSent).ToList() } };
        }
    }
}
