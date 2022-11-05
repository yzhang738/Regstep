using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using System.Web.Routing;
using RSToolKit.Domain.Entities.Clients;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity;
using System.Data.Entity;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using System.Security;
using RSToolKit.Domain.Data;
using Microsoft.AspNet.Identity.Owin;
using RSToolKit.Domain.Entities;
using RSToolKit.Domain.Security;
using RSToolKit.Domain.Entities.Email;
using System.IO;
using System.Net.Mail;
using System.Text.RegularExpressions;
using RSToolKit.Domain;
using RSToolKit.Domain.Entities;
using RSToolKit.Domain.Entities.Components;
using RSToolKit.Domain.Logging;
using RSToolKit.Domain.JItems;
using RSToolKit.Domain.Engines;

namespace RSToolKit.WebUI.Infrastructure
{
    public class RSController
        : Controller, IRegStepController
    {
        protected static Regex rgx_InvoiceName = new Regex(@"\[inv\s*=>\s*invoicedisplayname\]", RegexOptions.IgnoreCase);
        protected static Regex rgx_InvoiceCharge = new Regex(@"\[inv\s*=>\s*charge\]", RegexOptions.IgnoreCase);
        protected static Regex rgx_InvoiceLastFour = new Regex(@"\[inv\s*=>\s*lastfour\]", RegexOptions.IgnoreCase);

        public ILogger iLog = null;
        public User user = null;
        /// <summary>
        /// The current user logged in.
        /// </summary>
        public User AppUser
        {
            get
            {
                return user;
            }
        }
        public Company company = null;
        public EFDbContext Context;
        public Crumb Crumb = null;

        private AppUserManager p_UserManager;
        private AppRoleManager p_RoleManager;

        public FormsRepository Repository;

        public RSController(EFDbContext context)
        {
            Context = context;
            Repository = new FormsRepository(Context);
            iLog = new Logger()
            {
                LoggingMethod = "RSController",
                Thread = "Main"
            };
            p_UserManager = null;
            p_RoleManager = null;
        }

        public RSController()
        {
            Context = new EFDbContext();
            Repository = new FormsRepository(Context);
            iLog = new Logger()
            {
                LoggingMethod = "RSController",
                Thread = "Main"
            };
            p_UserManager = null;
            p_RoleManager = null;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (Context != null)
                    Context.Dispose();
            }
            base.Dispose(disposing);
        }

        // Here we are overriding the OnActionSexecuting to set a few important values.
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (filterContext.RouteData.Values["Controller"].ToString().ToLower() == "account" && filterContext.RouteData.Values["Action"].ToString().ToLower() == "logoff")
                return;
            if (User.Identity.IsAuthenticated)
            {
                var id = User.Identity.GetUserId();
                user = UserManager.FindById(id);
                iLog.User = user;
                if (user != null)
                {
                    Repository = new FormsRepository(Context, user, User);
                    ViewBag.User = user;
                    if (!Request.IsAjaxRequest() && filterContext.RouteData.Values["Controller"].ToString().ToLower() != "register" && filterContext.RouteData.Values["Controller"].ToString().ToLower() != "adminregister" && filterContext.RouteData.Values["Controller"].ToString().ToLower() != "error" && Request.RequestType.ToLower() == "get")
                    {
                        if (!Request.QueryString.AllKeys.Contains("ignorecrumb") || Request.QueryString["ignorecrumb"].ToLower() == "false")
                        {
                            var lastCrumb = user.BreadCrumbs.PeekLast();
                            if (lastCrumb == null || (lastCrumb != null && lastCrumb.Action.ToLower() != filterContext.RouteData.Values["Action"].ToString().ToLower() && lastCrumb.Controller.ToLower() != filterContext.RouteData.Values["Controller"].ToString()))
                            {
                                var t_params = new Dictionary<string, string>();
                                foreach (var key in filterContext.ActionParameters.Keys)
                                {
                                    var t_value = filterContext.ActionParameters[key];
                                    if (t_value == null)
                                        continue;
                                    t_params.Add(key, filterContext.ActionParameters[key].ToString());
                                }
                                Crumb = Crumb.New(filterContext.RouteData.Values["Action"].ToString(), filterContext.RouteData.Values["Controller"].ToString(), t_params);
                                if (Crumb != null)
                                    user.BreadCrumbs.Enqueue(Crumb);
                                else
                                    iLog.Warning("The Crumb was null");
                            }
                            else
                            {
                                Crumb = lastCrumb;
                            }
                        }
                        else
                        {
                            if (Request.QueryString.AllKeys.Contains("crumbid"))
                            {
                                Guid crumb_id;
                                if (Guid.TryParse(Request.QueryString["crumbid"], out crumb_id))
                                {
                                    foreach (var crumb in user.BreadCrumbs)
                                    {
                                        if (crumb.Id == crumb_id)
                                        {
                                            Crumb = crumb;
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                    ViewBag.Crumb = Crumb;
                    company = user.WorkingCompany;
                    if (company == null)
                    {
                        company = user.Company;
                        user.WorkingCompany = company;
                    }
                    ViewBag.UserCompany = user.Company;
                    ViewBag.WorkingCompany = company;
                    ViewBag.Company = company.UId;
                    ViewBag.CompanyKey = company.UId;
                    ViewBag.CompanyName = company.Name;
                    UserManager.Update(user);
                }
                else
                {
                    Repository = new FormsRepository(Context);
                }
            }
            else
            {
                Repository = new FormsRepository(Context);
            }
            ViewBag.Repository = Repository;
        }

        protected override void OnException(ExceptionContext filterContext)
        {
            if (filterContext.ExceptionHandled)
                return;
            try
            {
                if (user != null && Crumb != null)
                {
                    user.BreadCrumbs.CutTail();
                    UserManager.Update(user);
                }
                var log = iLog.Error(filterContext.Exception);
                filterContext.ExceptionHandled = true;
                TempData["Exception"] = filterContext.Exception;
                filterContext.Result = RedirectToAction("Error500", "Error", new { id = log.SortingId });
            }
            catch (Exception)
            { }
        }

        public JsonResult ChangeCurrentCompany(Guid company)
        {
            using (var context = new EFDbContext())
            {
                var result = new JsonResult();
                result.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
                result.Data = new { Success = false };
                if (company == Guid.Empty)
                {
                    return result;
                }
                var comp = context.Companies.FirstOrDefault(c => c.UId == company);
                if (comp == null)
                {
                    return result;
                }
                user.WorkingCompany = comp;
                UserManager.Update(user);
                result.Data = new { Success = true };
                return result;
            }
        }

        /// <summary>
        /// Handles access errors.
        /// </summary>
        /// <param name="node">The node that the error originated from</param>
        /// <param name="accessType">The type of action that was being attempted</param>
        /// <returns>The view to display.</returns>
        public virtual ActionResult HandleAccessError(INode node, SecurityAccessType accessType)
        {
            return RedirectToAction("Error403", "Error", new { node = node, accessType = accessType });
        }
        
        [HttpGet]
        [Authorize]
        [ActionName("Crumb")]
        public virtual JsonNetResult GetCrumb(Guid id)
        {
            foreach (var e_crumb in user.BreadCrumbs)
            {
                if (e_crumb.Id == id)
                    return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = true, Crumb = e_crumb } };
            }
            return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = false } };
        }

        [HttpPut]
        [Authorize]
        [JsonValidateAntiForgeryToken]
        [ActionName("Crumb")]
        public virtual JsonNetResult PutCrumb(Crumb crumb)
        {
            Crumb t_crumb = null;
            foreach (var e_crumb in user.BreadCrumbs)
            {
                if (e_crumb.Id == crumb.Id)
                {
                    t_crumb = e_crumb;
                    break;
                }
            }
            if (t_crumb == null)
                return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = false } };
            t_crumb.Label = crumb.Label;
            t_crumb.Parameters = crumb.Parameters;
            t_crumb.Action = crumb.Action;
            t_crumb.Controller = crumb.Controller;
            t_crumb.ActionDate = DateTimeOffset.UtcNow;
            UserManager.Update(user);
            return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = true, href = Url.Action(t_crumb.Action, t_crumb.Controller, t_crumb.RVD()) } };
        }

        /// <summary>
        /// Sends an email to a person based on the email type.
        /// </summary>
        /// <param name="id">The id of the person to send the email to.</param>
        /// <param name="type">The type of email to send.</param>
        /// <param name="node">The owner of the email.</param>
        /// <returns>True on success and false on failure.</returns>
        protected int SendRegistrantEmail(Guid id, EmailType type, IEmailHolder node = null, IEnumerable<MailAddress> extraEmails = null)
        {
            var registrant = Repository.Search<Registrant>(r => r.UId == id).FirstOrDefault();
            return SendRegistrantEmail(registrant, type, node, extraEmails);
        }

        /// <summary>
        /// Sends an email to a person based on the email type.
        /// </summary>
        /// <param name="person">The person to send the email to.</param>
        /// <param name="type">The type of email to send.</param>
        /// <param name="node">The owner of the email.</param>
        /// <returns>True on success and false on failure.</returns>
        protected int SendRegistrantEmail(Registrant person, EmailType type, IEmailHolder node = null, IEnumerable<MailAddress> extraEmails = null)
        {
            // Now we grab the smtpServer.
            var t_extraEmails = extraEmails ?? new List<MailAddress>();
            SmtpServer smtpServer = null;
            if (node != null)
            {
                smtpServer = Repository.Search<SmtpServer>(s => s.CompanyKey == node.CompanyKey).FirstOrDefault();
            }
            if (smtpServer == null)
                smtpServer = Repository.Search<SmtpServer>(s => s.Name == "Primary").FirstOrDefault();
            if (smtpServer == null)
                return 0;
            var count = 0;
            // We need to create the sendgrid header
            var header = new SendGridHeader();
            // Now we grab the parser and intialize the list of mail messages.
            RSParser parser = null;
            Registrant registrant = null;
            if (person is Registrant) {
                registrant = (Registrant)person;
                var form = registrant.Form;
                if (node == null)
                    node = form;
                var contact = Repository.Search<Contact>(c => c.CompanyKey == form.CompanyKey && c.Name == registrant.Email).FirstOrDefault();
                parser = new RSParser(registrant, Repository, form, null, contact);
            }
            var emailMessages = new List<Tuple<EmailData, Guid?>>();
            var emails = new List<IEmail>();
            // Now we need to populate the email list.
            switch (type)
            {
                case EmailType.Confirmation:
                    #region Confirmation
                    if (registrant == null)
                        break;
                    header.category.Add("registration-confirmation");
                    emails = node.AllEmails.Where(e => e.EmailType == EmailType.Confirmation).ToList();
                    if (emails.Count > 0)
                    {
                        foreach (var email in emails)
                        {
                            var sending = true;
                            if (email.EmailList != null && !email.EmailList.GetAllEmailAddresses().Contains(registrant.Email, StringComparer.InvariantCultureIgnoreCase))
                                sending = false;
                            var work = LogicEngine.RunLogic(email, Repository, registrant: person);
                            if (work.Count() > 0)
                                sending = work.First().Command == JLogicWork.SendEmail;
                            if (!sending)
                                continue;
                            emailMessages.Add(new Tuple<EmailData, Guid?>(email.GenerateEmail(parser), (Guid?)email.UId));
                        }
                    }
                    else
                    {
                        var message = new EmailData()
                        {
                            Subject = node.Name + ": Confirmation",
                            From = new MailAddress("no_reply@regstep.com")
                        };
                        if (System.IO.File.Exists(Server.MapPath("~/App_Data/Emails/" + node.UId + "_confirmation.html")))
                        {
                            using (var fileReader = new StreamReader(Server.MapPath("~/App_Data/Emails/" + node.UId + "_confirmation.html")))
                            {
                                message.Html = parser.ParseAvailable(fileReader.ReadToEnd());
                            }
                        }
                        else if (System.IO.File.Exists(Server.MapPath("~/App_Data/Emails/generic_confirmation.html")))
                        {
                            using (var fileReader = new StreamReader(Server.MapPath("~/App_Data/Emails/generic_confirmation.html")))
                            {
                                message.Html = parser.ParseAvailable(fileReader.ReadToEnd());
                            }
                        }
                        emailMessages.Add(new Tuple<EmailData, Guid?>(message, null));
                    }
                    break;
                    #endregion
                case EmailType.CreditCardReciept:
                    #region Credit Card
                    if (registrant == null)
                        break;
                    header.category.Add("registration-creditcardreciept");
                    /* Grab invoice transaction */
                    var request = registrant.TransactionRequests.OrderBy(t => t.DateCreated).LastOrDefault();
                    if (request == null)
                        return 0;
                    var invoiceName = registrant.Form.Name;
                    if (request.MerchantAccount.Company != null && request.MerchantAccount.Company.UId == Guid.Parse("{27e170e9-80bf-4292-82e3-b8fa348352e3}"))
                        invoiceName = "either RegStep Technologies or Registration Assistant";
                    /* Get emails */
                    emails = node.AllEmails.Where(e => e.EmailType == EmailType.CreditCardReciept).ToList();
                    if (emails.Count > 0)
                    {
                        foreach (var email in emails)
                        {
                            var sending = true;
                            if (email.EmailList != null && !email.EmailList.GetAllEmailAddresses().Contains(registrant.Email, StringComparer.InvariantCultureIgnoreCase))
                                sending = false;
                            var work = LogicEngine.RunLogic(email, Repository, registrant: person);
                            if (work.Count() > 0)
                                sending = work.First().Command == JLogicWork.SendEmail;
                            if (!sending)
                                continue;
                            var message = email.GenerateEmail(parser);
                            message.Text = rgx_InvoiceCharge.Replace(message.Text, Math.Round(registrant.TotalOwed, 2).ToString());
                            message.Html = rgx_InvoiceCharge.Replace(message.Html, Math.Round(registrant.TotalOwed, 2).ToString());
                            emailMessages.Add(new Tuple<EmailData, Guid?>(message, (Guid?)email.UId));
                        }
                    }
                    else
                    {
                        var message = new EmailData()
                        {
                            Subject = registrant.Form.Name + ": Credit Card Reciept",
                            From = new MailAddress("billing@regstep.com")
                        };
                        if (System.IO.File.Exists(Server.MapPath("~/App_Data/Emails/" + node.UId + "_cc.html")))
                        {
                            using (var fileReader = new StreamReader(Server.MapPath("~/App_Data/Emails/" + node.UId + "_cc.html")))
                            {
                                message.Html = parser.ParseAvailable(fileReader.ReadToEnd());
                            }
                        }
                        else if (System.IO.File.Exists(Server.MapPath("~/App_Data/Emails/generic_cc.html")))
                        {
                            using (var fileReader = new StreamReader(Server.MapPath("~/App_Data/Emails/generic_cc.html")))
                            {
                                message.Html = parser.ParseAvailable(fileReader.ReadToEnd());
                            }
                        }
                        emailMessages.Add(new Tuple<EmailData, Guid?>(message, null));
                    }
                    break;
                    #endregion
                case EmailType.BillMeInvoice:
                    #region Bill Me
                    if (registrant == null)
                        break;
                    header.category.Add("registration-billmeinvoice");
                    emails = node.AllEmails.Where(e => e.EmailType == EmailType.BillMeInvoice).ToList();
                    if (emails.Count > 0)
                    {
                        foreach (var email in emails)
                        {
                            var sending = true;
                            if (email.EmailList != null && !email.EmailList.GetAllEmailAddresses().Contains(registrant.Email, StringComparer.InvariantCultureIgnoreCase))
                                sending = false;
                            var work = LogicEngine.RunLogic(email, Repository, registrant: person);
                            if (work.Count() > 0)
                                sending = work.First().Command == JLogicWork.SendEmail;
                            if (!sending)
                                continue;
                            var message = email.GenerateEmail(parser);
                            message.Text = rgx_InvoiceCharge.Replace(message.Text, Math.Round(registrant.TotalOwed, 2).ToString());
                            message.Html = rgx_InvoiceCharge.Replace(message.Html, Math.Round(registrant.TotalOwed, 2).ToString());
                            emailMessages.Add(new Tuple<EmailData, Guid?>(message, (Guid?)email.UId));
                        }
                    }
                    else
                    {
                        var message = new EmailData()
                        {
                            Subject = registrant.Form.Name + ": Billing Invoice",
                            From = new MailAddress("no_reply@regstep.com")
                        };
                        if (System.IO.File.Exists(Server.MapPath("~/App_Data/Emails/" + node.UId + "_invoice.html")))
                        {
                            using (var fileReader = new StreamReader(Server.MapPath("~/App_Data/Emails/" + node.UId + "_invoice.html")))
                            {
                                message.Html = parser.ParseAvailable(fileReader.ReadToEnd());
                            }
                        }
                        else if (System.IO.File.Exists(Server.MapPath("~/App_Data/Emails/generic_invoice.html")))
                        {
                            using (var fileReader = new StreamReader(Server.MapPath("~/App_Data/Emails/generic_invoice.html")))
                            {
                                message.Html = parser.ParseAvailable(fileReader.ReadToEnd());
                            }
                        }
                        emailMessages.Add(new Tuple<EmailData, Guid?>(message, null));
                    }
                    break;
                    #endregion
                case EmailType.Continue:
                    #region Continue
                    if (registrant == null)
                        break;
                    header.category.Add("registration-continue");
                    emails = node.AllEmails.Where(e => e.EmailType == EmailType.Continue).ToList();
                    if (emails.Count > 0)
                    {
                        foreach (var email in emails)
                        {
                            var sending = true;
                            if (email.EmailList != null && !email.EmailList.GetAllEmailAddresses().Contains(registrant.Email, StringComparer.InvariantCultureIgnoreCase))
                                sending = false;
                            var work = LogicEngine.RunLogic(email, Repository, registrant: person);
                            if (work.Count() > 0)
                                sending = work.First().Command == JLogicWork.SendEmail;
                            if (!sending)
                                continue;
                            emailMessages.Add(new Tuple<EmailData, Guid?>(email.GenerateEmail(parser), (Guid?)email.UId));
                        }
                    }
                    else
                    {
                        var message = new EmailData()
                        {
                            Subject = registrant.Form.Name + ": Continue",
                            From = new MailAddress("no_reply@regstep.com")
                        };
                        if (System.IO.File.Exists(Server.MapPath("~/App_Data/Emails/" + node.UId + "_continue.html")))
                        {
                            using (var fileReader = new StreamReader(Server.MapPath("~/App_Data/Emails/" + node.UId + "_continue.html")))
                            {
                                message.Html = parser.ParseAvailable(fileReader.ReadToEnd());
                            }
                        }
                        else if (System.IO.File.Exists(Server.MapPath("~/App_Data/Emails/generic_continue.html")))
                        {
                            using (var fileReader = new StreamReader(Server.MapPath("~/App_Data/Emails/generic_continue.html")))
                            {
                                message.Html = parser.ParseAvailable(fileReader.ReadToEnd());
                            }
                        }
                        emailMessages.Add(new Tuple<EmailData, Guid?>(message, null));
                    }
                    break;
                    #endregion
                case EmailType.Cancellation:
                    #region Cancel
                    if (registrant == null)
                        break;
                    header.category.Add("registration-cancellation");
                    emails = node.AllEmails.Where(e => e.EmailType == EmailType.Cancellation).ToList();
                    if (emails.Count > 0)
                    {
                        foreach (var email in emails)
                        {
                            var sending = true;
                            if (email.EmailList != null && !email.EmailList.GetAllEmailAddresses().Contains(registrant.Email, StringComparer.InvariantCultureIgnoreCase))
                                sending = false;
                            var work = LogicEngine.RunLogic(email, Repository, registrant: person);
                            if (work.Count() > 0)
                                sending = work.First().Command == JLogicWork.SendEmail;
                            if (!sending)
                                continue;
                            emailMessages.Add(new Tuple<EmailData, Guid?>(email.GenerateEmail(parser), (Guid?)email.UId));
                        }
                    }
                    else
                    {
                        var message = new EmailData()
                        {
                            Subject = registrant.Form.Name + ": Cancellation",
                            From = new MailAddress("no_reply@regstep.com")
                        };
                        if (System.IO.File.Exists(Server.MapPath("~/App_Data/Emails/" + node.UId + "_cancellation.html")))
                        {
                            using (var fileReader = new StreamReader(Server.MapPath("~/App_Data/Emails/" + node.UId + "_cancellation.html")))
                            {
                                message.Html = parser.ParseAvailable(fileReader.ReadToEnd());
                            }
                        }
                        else if (System.IO.File.Exists(Server.MapPath("~/App_Data/Emails/generic_cancellation.html")))
                        {
                            using (var fileReader = new StreamReader(Server.MapPath("~/App_Data/Emails/generic_cancellation.html")))
                            {
                                message.Html = parser.ParseAvailable(fileReader.ReadToEnd());
                            }
                        }
                        emailMessages.Add(new Tuple<EmailData, Guid?>(message, null));
                    }
                    break;
                    #endregion
                case EmailType.Invitation:
                    #region Invitation
                    if (registrant == null)
                        break;
                    header.category.Add("registration-invitation");
                    emails = node.AllEmails.Where(e => e.EmailType == EmailType.Invitation).ToList();
                    if (emails.Count > 0)
                    {
                        foreach (var email in emails)
                        {
                            var sending = true;
                            if (email.EmailList != null && !email.EmailList.GetAllEmailAddresses().Contains(registrant.Email, StringComparer.InvariantCultureIgnoreCase))
                                sending = false;
                            var work = LogicEngine.RunLogic(email, Repository, registrant: person);
                            if (work.Count() > 0)
                                sending = work.First().Command == JLogicWork.SendEmail;
                            if (!sending)
                                continue;
                            emailMessages.Add(new Tuple<EmailData, Guid?>(email.GenerateEmail(parser), (Guid?)email.UId));
                        }
                    }
                    else
                    {
                        var message = new EmailData()
                        {
                            Subject = registrant.Form.Name + ": Invitation",
                            From = new MailAddress("no_reply@regstep.com")
                        };
                        if (System.IO.File.Exists(Server.MapPath("~/App_Data/Emails/" + node.UId + "_invitation.html")))
                        {
                            using (var fileReader = new StreamReader(Server.MapPath("~/App_Data/Emails/" + node.UId + "_invitation.html")))
                            {
                                message.Html = parser.ParseAvailable(fileReader.ReadToEnd());
                            }
                        }
                        else if (System.IO.File.Exists(Server.MapPath("~/App_Data/Emails/generic_invitation.html")))
                        {
                            using (var fileReader = new StreamReader(Server.MapPath("~/App_Data/Emails/generic_invitation.html")))
                            {
                                message.Html = parser.ParseAvailable(fileReader.ReadToEnd());
                            }
                        }
                        emailMessages.Add(new Tuple<EmailData, Guid?>(message, null));
                    }
                    break;
                    #endregion
            }
            foreach (var message in emailMessages)
            {
                if (smtpServer.SendEmail(message.Item1, Repository, node.Company, node, person, header, type.GetStringValue(), emailKey: message.Item2, extraEmails: t_extraEmails).Success)
                    count++;
            }
            return count;
        }

        protected bool SendEmail(IPerson person, IEmail email)
        {
            // Now we grab the smtpServer.
            SmtpServer smtpServer = null;
            if (email == null)
                return false;
            IEmailHolder node = email.Form;
            if (node == null)
                node = email.EmailCampaign;
            if (node != null)
                smtpServer = Repository.Search<SmtpServer>(s => s.CompanyKey == node.CompanyKey).FirstOrDefault();
            if (smtpServer == null)
                smtpServer = Repository.Search<SmtpServer>(s => s.Name == "Primary").FirstOrDefault();
            if (smtpServer == null)
                return false;
            // We need to create the sendgrid header
            var header = new SendGridHeader();
            header.category.Add(email.EmailType.GetStringValue());
            header.unique_args.Add("email-uid", email.UId.ToString());
            // Now we grab the parser and intialize the list of mail messages.
            RSParser parser = null;
            Registrant registrant = null;
            Contact contact = null;
            if (person == null)
                return false;
            if (person is Registrant)
            {
                registrant = (Registrant)person;
                var form = registrant.Form;
                if (node == null)
                    node = form;
                contact = Repository.Search<Contact>(c => c.CompanyKey == form.CompanyKey && c.Name == registrant.Email).FirstOrDefault();
                parser = new RSParser(registrant, Repository, form, null, contact);
            }
            else if (person is Contact)
            {
                contact = (Contact)person;
                parser = new RSParser(registrant, Repository, email.Form, email.EmailCampaign, contact);
            }
            if (contact == null && registrant == null)
                return false;
            if (parser == null)
                return false;
            var sending = true;
            var work = LogicEngine.RunLogic(email, Repository, registrant: person as Registrant, contact: person as Contact);
            if (work.Count() > 0)
                sending = work.First().Command == JLogicWork.SendEmail;
            if (!sending)
                return false;
            var message = email.GenerateEmail(parser);
            smtpServer.SendEmail(message, Repository, company, node, person, header, email.EmailType.GetStringValue(), emailKey: email.UId);
            return true;
        }

        /// <summary>
        /// Encodes the string for html.  This is mainly used for encoding a string for an html that is not used in a view.
        /// </summary>
        /// <param name="html">The html that needs to be encoded.</param>
        /// <returns>Returns a string with html encoded escapse characters.</returns>
        protected string EncodeHtml(string html)
        {
            return HttpUtility.HtmlEncode(html);
        }

        public AppUserManager UserManager
        {
            get
            {
                if (p_UserManager == null)
                    p_UserManager = new AppUserManager(new UserStore<User>((EFDbContext)Context));
                return p_UserManager;
            }
        }

        protected IAuthenticationManager AuthManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        public AppRoleManager RoleManager
        {
            get
            {
                if (p_RoleManager == null)
                    p_RoleManager = new AppRoleManager(new RoleStore<AppRole>((EFDbContext)Context));
                return p_RoleManager;
            }
        }

        /// <summary>
        /// Parses an id as a string into either a long sorting id or a unique identifier.
        /// </summary>
        /// <param name="id">The id to parse.</param>
        /// <returns>The id that was parsed, or -1L if unable to parse.</returns>
        protected object ParseId(string id)
        {
            long lid;
            Guid uid;
            object rid = null;
            if (!long.TryParse(id, out lid))
            {
                if (Guid.TryParse(id, out uid))
                {
                    rid = uid;
                }
            }
            else
            {
                rid = lid;
            }
            return rid ?? -1L;
        }

    }
}