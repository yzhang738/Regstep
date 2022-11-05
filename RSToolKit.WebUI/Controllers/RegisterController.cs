using Newtonsoft.Json;
using RSToolKit.Domain;
using RSToolKit.Domain.Data;
using RSToolKit.Domain.Entities;
using RSToolKit.Domain.Entities.Components;
using RSToolKit.Domain.Security;
using RSToolKit.WebUI.Infrastructure;
using RSToolKit.WebUI.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using RSToolKit.Domain.Entities.MerchantAccount;
using System.IO;
using System.Net.Mail;
using System.Net;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.html;
using iTextSharp.text.html.simpleparser;
using RSToolKit.Domain.Entities.Email;
using System.Globalization;
using RSToolKit.Domain.Logging;
using RSToolKit.Domain.JItems;
using RSToolKit.Domain.Engines;
using System.Threading.Tasks;
using System.Threading;

namespace RSToolKit.WebUI.Controllers
{
    public class RegisterController
        : RSController
    {
        protected static Dictionary<Guid, EmailSendToken> EmailTokens = new Dictionary<Guid, EmailSendToken>();
        protected static DateTimeOffset timeSensitiveCheck = DateTimeOffset.MinValue;
        protected static Dictionary<long, Guid> customLoginTokens = new Dictionary<long, Guid>();

        protected static bool IsChecking = false;
        protected static object CheckLock = new object();

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);
            if (timeSensitiveCheck.AddMinutes(30) < DateTimeOffset.Now)
            {
                timeSensitiveCheck = DateTimeOffset.Now;
                ThreadPool.QueueUserWorkItem(CheckCallback, null);
            }
        }

        public static void CheckCallback(object args)
        {
            lock (CheckLock)
            {
                IsChecking = true;
                // First we check for old email tokens.
                EmailTokens.Where(t => t.Value.Creation.AddMinutes(30) < DateTimeOffset.UtcNow).ToList().ForEach(t => EmailTokens.Remove(t.Key));

                // Now we remove expired seating assignments.
                var seatExpired = DateTimeOffset.Now.AddMinutes(-30);
                using (var context = new EFDbContext())
                {
                    var seaters = context.Seaters.Where(s => s.Registrant.Status == RegistrationStatus.Incomplete && s.Date < seatExpired).ToList();
                    foreach (var seater in seaters)
                    {
                        IComponent comp = seater.Component;
                        if (comp is IComponentItem)
                            seater.Registrant.RemoveItem(comp as IComponentItem);
                        context.Seaters.Remove(seater);
                    }

                    // Now we remove expired promotion codes.
                    foreach (var entry in context.PromotionCodeEntries.Where(p => p.Code.Limit != -1 && p.Registrant.Status == RegistrationStatus.Incomplete && p.Registrant.DateModified < seatExpired).ToList())
                        context.PromotionCodeEntries.Remove(entry);
                    context.SaveChanges();

                }
                IsChecking = false;
            }
        }

        protected Registrant _registrant;
        protected Form _form;
        protected Page _page;

        public Registrant Reg { get { return _registrant; } }
        public Form Form { get { return _form; } }
        public Page Page { get { return _page; } }
        public Dictionary<string, IEnumerable<JLogicCommand>> LogicCommands { get; set; }

        #region Contructors

        private readonly Dictionary<string, string[]> AllowedTypes = new Dictionary<string, string[]>()
        {
            {
                "picture", new string[]
                {
                    "image/jpeg",
                    "image/bmp",
                    "image/gif",
                    "image/tiff",
                    "image/png"
                }
            },
            {
                "pdf", new string[]
                {
                    "application/pdf"
                }
            },
            {
                "text", new string[]
                {
                    "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                    "application/msword",
                    "text/richtext",
                    "text/plain",
                    "application/msword",
                    "application/x-iwork-pages-sffpages"
                }
            }
        };

        public RegisterController()
            : base()
        {
            iLog.LoggingMethod = "RegisterController";
            LogicCommands = new Dictionary<string, IEnumerable<JLogicCommand>>();
            this._page = null;
            this._form = null;
            this._registrant = null;
        }


        #endregion

        #region Closed, Maintenance, Not Editable

        [HttpGet]
        public ActionResult FormClosed(Guid id)
        {
            this._form = Context.Forms.Where(f => f.UId == id).FirstOrDefault();
            if (this._form == null)
                return RedirectToAction("InvalidForm");
            var model = new RegistrationHtml(this);
            return View(model);
        }

        [HttpGet]
        public ActionResult UnderMaintenance(Guid id)
        {
            this._form = Context.Forms.Where(f => f.UId == id).FirstOrDefault();
            if (this._form == null)
                return RedirectToAction("InvalidForm");
            var model = new RegistrationHtml(this);
            return View(model);
        }

        [HttpGet]
        public ActionResult NotEditable(Guid id, Guid registrantKey)
        {
            this._form = this._form ?? Context.Forms.Where(f => f.UId == id).FirstOrDefault();
            if (this._form == null)
                return View("InvalidForm");
            ViewBag.Form = this._form;
            var registrant = this._registrant ?? Context.Registrants.Where(r => r.UId == id).FirstOrDefault();
            if (registrant == null)
                return View("InvalidRegistrant", new RegistrationHtml(this));
            var model = new RegistrationHtml(this);
            return View("NotEditable", model);
        }

        #endregion

        #region Starting

        #region Already Started

        [HttpGet]
        public ActionResult EmailUsed(Guid id, Guid token, long logId = 0)
        {
            this._form = Context.Forms.Where(f => f.UId == id).FirstOrDefault();
            if (this._form == null)
                return RedirectToAction("InvalidForm");
            ViewBag.Form = this._form;
            ViewBag.Token = token;
            ViewBag.LogId = logId;
            var model = new RegistrationHtml(this);
            return View("EmailUsed", model);
        }

        [HttpPost]
        [ActionName("EmailUsed")]
        [JsonValidateAntiForgeryToken]
        public JsonNetResult EmailUsedSendEmail(Guid Token)
        {
            if (!EmailTokens.Keys.Contains(Token))
                return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = false, Message = "Invalid Email Token" } };
            var token = EmailTokens[Token];
            this._form = this._form ?? Context.Forms.Where(f => f.UId == token.FormKey).FirstOrDefault();
            if (this._form == null)
                return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = false, Message = "Invalid Form" } };
            ViewBag.Form = this._form;
            var registrant = this._form.Registrants.Where(r => r.Email == token.Email).FirstOrDefault();
            if (registrant == null)
                return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = false, Message = "Invalid Email" } };

            SendRegistrantEmail(registrant, EmailType.Continue, registrant.Form);

            EmailTokens.Remove(Token);

            return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = true } };
        }

        [HttpGet]
        public ActionResult RegistrationCompleted(Guid id, Guid token)
        {
            this._form = this._form ?? Context.Forms.Where(f => f.UId == id).FirstOrDefault();
            if (this._form == null)
                return View("InvalidForm");
            ViewBag.Form = this._form;
            ViewBag.Token = token;
            var model = new RegistrationHtml(this);
            return View("RegistrationCompleted", model);
        }

        [HttpGet]
        public ActionResult RegistrationDeleted(Guid id, Guid token)
        {
            this._form = Context.Forms.Where(f => f.UId == id).FirstOrDefault();
            if (this._form == null)
                return View("InvalidForm");
            ViewBag.Form = this._form;
            ViewBag.Token = token;
            var model = new RegistrationHtml(this);
            return View("RegistrationDeleted", model);
        }

        [HttpPost]
        [ActionName("RegistrationCompleted")]
        [JsonValidateAntiForgeryToken]
        public JsonNetResult RegistrationCompletedSendEmail(Guid Token)
        {
            if (!EmailTokens.Keys.Contains(Token))
                return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = false, Message = "Invalid Email Token" } };
            var token = EmailTokens[Token];
            this._form = Context.Forms.Where(f => f.UId == token.FormKey).FirstOrDefault();
            if (this._form == null)
                return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = false, Message = "Invalid Form" } };
            ViewBag.Form = this._form;
            this._registrant = this._form.Registrants.Where(r => r.Email == token.Email).FirstOrDefault();
            if (this._registrant == null)
                return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = false, Message = "Invalid Email" } };

            SendRegistrantEmail(this._registrant, EmailType.Confirmation, this._form);

            EmailTokens.Remove(Token);

            return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = true } };
        }

        #endregion

        #region Errors

        [HttpGet]
        public ActionResult NotInList(Guid id)
        {
            this._form = this._form ?? Context.Forms.Where(f => f.UId == id).FirstOrDefault();
            if (this._form == null)
                return View("InvalidForm");
            ViewBag.Form = this._form;
            var model = new RegistrationHtml(this);
            return View("NotInList", model);
        }

        #endregion

        [HttpGet]
        public ActionResult CustomLogin(long formId, string email = "", bool live = true)
        {
            this._form = Context.Forms.Where(f => f.SortingId == formId).FirstOrDefault();
            if (this._form == null)
                return RedirectToAction("InvalidForm");
            if (live)
            {
                if (this._form.Close < DateTimeOffset.UtcNow || this._form.Status == FormStatus.Closed)
                    return RedirectToAction("FormClosed", new { id = this._form.UId });
                if (this._form.Status == FormStatus.Maintenance)
                    return RedirectToAction("UnderMaintenance", new { id = this._form.UId });
            }

            ViewBag.Email = email.ToLower();
            return View(new Tuple<RegistrationHtml, bool>(new RegistrationHtml(this), live));
        }

        [HttpPost]
        [JsonValidateAntiForgeryToken]
        public ActionResult CustomLogin(long formId, bool live, Dictionary<long, string> loginData)
        {
            this._form = Context.Forms.Where(f => f.SortingId == formId).FirstOrDefault();
            if (this._form == null)
                return RedirectToAction("InvalidForm");
            IEnumerable<Contact> matchedContacts = this._form.Company.Contacts;
            var emailUsedInCredentials = false;
            foreach (var kvp in loginData)
            {
                if (kvp.Key == -1)
                {
                    emailUsedInCredentials = true;
                    matchedContacts = matchedContacts.Where(c => c.Name.ToLower() == kvp.Value.ToLower());
                }
                else
                {
                    var header = this._form.Company.ContactHeaders.FirstOrDefault(c => c.SortingId == kvp.Key);
                    if (header == null)
                        throw new Exception("Contact header invalid.");
                    matchedContacts = matchedContacts.Where(c => c.Data.FirstOrDefault(h => h.Header.SortingId == kvp.Key) != null && c.Data.FirstOrDefault(h => h.Header.SortingId == kvp.Key).Value == kvp.Value);
                }
            }
            if (matchedContacts.Count() > 1)
            {
                ViewBag.Error = "Too many matches. Please contact the form administrator for help.";
                return CustomLoginExcess(this._form.SortingId);
            }
            if (matchedContacts.Count() == 0)
            {
                ViewBag.Error = "Invalid Credentials.";
                return View(new Tuple<RegistrationHtml, bool>(new RegistrationHtml(this), live));
            }

            var contact = matchedContacts.First();
            var regType = live ? RegistrationType.Live : RegistrationType.Test;
            this._registrant = Context.Registrants.Where(r => r.Email == contact.Email  && r.FormKey == this._form.UId && r.Type == regType).FirstOrDefault();
            var prevStarted = true;
            if (this._registrant == null)
            {
                prevStarted = false;
                this._registrant = new Registrant()
                {
                    Email = contact.Name,
                    Form = this._form,
                    FormKey = this._form.UId,
                    Status = RegistrationStatus.Incomplete,
                    Type = live ? RegistrationType.Live : RegistrationType.Test,
                    UId = Guid.NewGuid(),
                    DateCreated = DateTimeOffset.Now,
                    Contact = contact,
                    ContactKey = contact.UId
                };
                Context.Registrants.Add(this._registrant);
            }
            var model = new RegistrationHtml(this);
            if (this._registrant.Status.In(RegistrationStatus.Canceled, RegistrationStatus.CanceledByAdministrator, RegistrationStatus.CanceledByCompany, RegistrationStatus.Submitted))
                return ShowConfirmation(this._registrant.UId);
            if (this._registrant.Status == RegistrationStatus.Deleted)
                return RegistrationDeleted(this._registrant.UId, Guid.Empty);
            // Now we find the contact

            #region Contact Data Fill
            if (!prevStarted)
            {
                foreach (var component in this._form.GetComponents())
                {
                    if (component.MappedToKey == null)
                        continue;
                    var c_data = contact.Data.Where(d => d.HeaderKey == component.MappedToKey).FirstOrDefault();
                    if (c_data == null)
                        continue;
                    var t_data = new RegistrantData()
                    {
                        VariableUId = component.UId,
                        Component = component as Component
                    };
                    if (component is Input)
                    {
                        t_data.Value = c_data.Value;
                        this._registrant.Data.Add(t_data);
                    }
                    else if (component is CheckboxGroup)
                    {
                        if (string.IsNullOrWhiteSpace(c_data.Value))
                            continue;
                        var items = Context.Components.OfType<CheckboxItem>().Where(i => i.CheckboxGroupKey == component.UId).ToList();
                        var r_items = c_data.Value.Split(',');
                        var c_items = new List<CheckboxItem>();
                        foreach (var r_item in r_items)
                        {
                            var r_t_item = r_item.Trim().ToLower();
                            var c_item = items.FirstOrDefault(i => i.LabelText.ToLower() == r_t_item);
                            if (c_item == null)
                                c_item = items.FirstOrDefault(i => Regex.Replace(i.LabelText.ToLower(), @"\s", "") == Regex.Replace(r_t_item, @"\s", ""));
                            if (c_item == null)
                                c_item = items.FirstOrDefault(i => i.UId.ToString().ToLower() == r_t_item);
                            if (c_item != null)
                                c_items.Add(c_item);
                        }
                        t_data.Value = JsonConvert.SerializeObject(c_items.Select(i => i.UId));
                        this._registrant.Data.Add(t_data);
                    }
                    else if (component is DropdownGroup)
                    {
                        var items = Context.Components.OfType<DropdownItem>().Where(i => i.DropdownGroupKey == component.UId).ToList();
                        var c_item = items.FirstOrDefault(i => i.LabelText.ToLower() == (String.IsNullOrWhiteSpace(c_data.Value) ? "" : c_data.Value.ToLower()));
                        if (c_item == null)
                            c_item = items.FirstOrDefault(i => i.UId.ToString().ToLower() == (String.IsNullOrWhiteSpace(c_data.Value) ? "" : c_data.Value.ToLower()));
                        if (c_item != null)
                        {
                            t_data.Value = c_item.UId.ToString();
                            this._registrant.Data.Add(t_data);
                        }
                    }
                    else if (component is RadioGroup)
                    {
                        var items = Context.Components.OfType<RadioItem>().Where(i => i.RadioGroupKey == component.UId).ToList();
                        var c_item = items.FirstOrDefault(i => i.LabelText.ToLower() == (String.IsNullOrWhiteSpace(c_data.Value) ? "" : c_data.Value.ToLower()));
                        if (c_item == null)
                            c_item = items.FirstOrDefault(i => i.UId.ToString().ToLower() == (String.IsNullOrWhiteSpace(c_data.Value) ? "" : c_data.Value.ToLower()));
                        if (c_item != null)
                        {
                            t_data.Value = c_item.UId.ToString();
                            this._registrant.Data.Add(t_data);
                        }
                    }
                }
            }
            #endregion
            Context.SaveChanges();

            //SendRegistrantEmail(this._registrant, EmailType.Continue, this._registrant.Form);
            model.Page = this._form.Pages.Where(p => p.Type == PageType.RSVP).FirstOrDefault();
            if (model.Page != null)
            {
                if (model.Page.Enabled)
                {
                    ViewBag.First = true;
                    ViewBag.Form = this._form;
                    return View("RSVP", model);
                }
                model.Page = this._form.Pages.Where(p => p.Type == PageType.Audience).First();
                if (model.Page.Enabled && model.Form.Audiences.Count > 0)
                    if ((this._registrant.RSVP && model.Page.RSVP == RSVPType.Decline) || (!this._registrant.RSVP && model.Page.RSVP == RSVPType.Accept))
                        return Next(this._registrant.UId, 3);
                    else
                    {
                        ViewBag.RSVPEnabled = this._form.Pages.Where(p => p.Type == PageType.RSVP).First().Enabled;
                        ViewBag.Form = this._form;
                        return View("Audience", model);
                    }
            }
            return Next(this._registrant.UId, (this._form.Survey ? 1 : 3));
        }

        public ActionResult CustomLoginExcess(long id)
        {
            this._form = Context.Forms.Where(f => f.SortingId == id).FirstOrDefault();
            if (this._form == null)
                return RedirectToAction("InvalidForm");
            return View("CustomLoginExcess", new RegistrationHtml(this));
        }

        [HttpGet]
        public ActionResult Start(Guid id, string email = "", bool live = true)
        {
            email = email ?? "";
            this._form = this._form ?? Context.Forms.Where(f => f.UId == id).FirstOrDefault();
            if (this._form == null)
                return RedirectToAction("InvalidForm");
            if (this._form.AccessType == FormAccessType.CustomLogIn)
                return RedirectToAction("CustomLogin", new { formId = this._form.SortingId, email = email });
            ViewBag.Form = this._form;
            ViewBag.Email = email;
            ViewBag.Live = live;
            if (live)
            {
                if (this._form.Close < DateTimeOffset.UtcNow || this._form.Status == FormStatus.Closed)
                    return RedirectToAction("FormClosed", new { id = id });
                if (this._form.Status == FormStatus.Maintenance)
                    return RedirectToAction("UnderMaintenance", new { id = id });
            }
            var model = new RegistrationHtml(this);
            return View("Start", new Tuple<RegistrationHtml, bool>(model, live));       
        }

        [HttpPost]
        [JsonValidateAntiForgeryToken]
        public ActionResult Start(string Email, Guid FormKey, bool Live, long contactId = -1, Guid loginToken = default(Guid))
        {
            #region new code
            Email = Email.ToLower();
            this._form = this._form ?? Context.Forms.Find(FormKey);
            if (this._form == null)
                return RedirectToAction("InvalidForm");
            var components = this._form.GetUnorderedComponents().Where(c => c.MappedToKey.HasValue);
            var possiblePrimaryContact = Context.Contacts.FirstOrDefault(c => c.Name == Email && c.CompanyKey == this._form.CompanyKey);
            var possibleSecondaryContacts = Context.ContactData.Where(c => c.Value == Email).ToList();
            Contact contact = null;
            if (this._form.AccessType == FormAccessType.CustomLogIn)
            {
                if (!customLoginTokens.ContainsKey(contactId) || customLoginTokens[contactId] != loginToken)
                {
                    ViewBag.Error = "Invalid credentials.";
                    return View("CustomLogin", new Tuple<RegistrationHtml, bool>(new RegistrationHtml(this), Live));
                }
                contact = this._form.Company.Contacts.FirstOrDefault(c => c.SortingId == contactId);
                if (contact == null)
                {
                    ViewBag.Error = "Invalid credentials.";
                    return View("CustomLogin", new Tuple<RegistrationHtml, bool>(new RegistrationHtml(this), Live));
                }
            }
            ViewBag.Form = this._form;
            if (Live)
            {
                if (this._form.Close < DateTimeOffset.UtcNow || this._form.Status == FormStatus.Closed)
                    return RedirectToAction("FormClosed", new { id = FormKey });
                if (this._form.Status == FormStatus.Maintenance)
                    return RedirectToAction("UnderMaintenance", new { id = FormKey });
            }
            var regType = Live ? RegistrationType.Live : RegistrationType.Test;
            this._registrant = Context.Registrants.Where(r => r.Email == Email && r.FormKey == FormKey && r.Type == regType).FirstOrDefault();
            if (this._registrant != null)
            {
                if (Live)
                {
                    if (this._registrant.Status == RegistrationStatus.Submitted)
                    {
                        var token = EmailSendToken.New(this._form.UId, Email);
                        EmailTokens.Add(token.Token, token);
                        return RedirectToAction("RegistrationCompleted", new { id = FormKey, token = token.Token });
                    }
                    if (this._registrant.Status == RegistrationStatus.Deleted)
                    {
                        var token = EmailSendToken.New(this._form.UId, Email);
                        EmailTokens.Add(token.Token, token);
                        return RedirectToAction("RegistrationDeleted", new { id = FormKey, token = token.Token });
                    }
                    else
                    {
                        var token = EmailSendToken.New(this._form.UId, Email);
                        EmailTokens.Add(token.Token, token);
                        return RedirectToAction("EmailUsed", new { id = FormKey, token = token.Token });
                    }
                }
                else
                {
                    Repository.Remove(this._registrant);
                    //Context.Registrants.Remove(this._registrant);
                    Context.SaveChanges();
                    this._registrant = null;
                }
            }
            var model = new RegistrationHtml(this);
            try
            {
                var mailAddress = new MailAddress(Email);
            }
            catch (Exception)
            {
                ViewBag.Error = "Invalid Email Format.";
                return View("Start", new Tuple<RegistrationHtml, bool>(model, Live));
            }

            // Now we find the contact
            #region Contacts
            if (contact == null)
                contact = possiblePrimaryContact;
            if (contact == null)
            {
                foreach (var cData in possibleSecondaryContacts)
                {
                    if (cData.Header.Descriminator == ContactDataType.Email)
                    {
                        contact = cData.Contact;
                        break;
                    }
                }
            }
            var inList = false;
            if (contact != null && this._form.EmailList != null && this._form.EmailList.GetAllEmails(Repository).FirstOrDefault(c => c.UId == contact.UId) != null)
                inList = true;
            if (contact != null)
            {
                if (contact != null && Live)
                {
                    // Since we have multiple emails assigned to contacts. If any of the other emails are being used, we need to not allow this one to be used too.
                    var otherEmails = contact.GetEmails().Where(e => e != Email.ToLower()).ToArray();
                    var prevReg = this._form.Registrants.FirstOrDefault(r => r.Type == RegistrationType.Live && r.Email.ToLower().In(otherEmails));
                    if (prevReg != null)
                    {
                        // We found one of the other emails. This is a no no.
                        if (prevReg.Status == RegistrationStatus.Submitted)
                        {
                            var token = EmailSendToken.New(this._form.UId, Email);
                            EmailTokens.Add(token.Token, token);
                            return RedirectToAction("RegistrationCompleted", new { id = FormKey, token = token.Token });
                        }
                        if (prevReg.Status == RegistrationStatus.Deleted)
                        {
                            var token = EmailSendToken.New(this._form.UId, Email);
                            EmailTokens.Add(token.Token, token);
                            return RedirectToAction("RegistrationDeleted", new { id = FormKey, token = token.Token });
                        }
                        else
                        {
                            var token = EmailSendToken.New(this._form.UId, prevReg.Email);
                            EmailTokens.Add(token.Token, token);
                            var log = iLog.Warning("Registrant using email " + Email + " was in contact list for company " + company.Name + " (" + company.UId + ") and had previously registered with email " + prevReg.Email + ". The user was not allowed to register.", company: company);
                            return RedirectToAction("EmailUsed", new { id = FormKey, token = token.Token, logId = log.SortingId });
                        }
                    }
                }
            }
            #endregion

            if (!inList && this._form.AccessType == FormAccessType.InvitationOnly)
                return RedirectToAction("NotInList", new { id = this._form.UId });

            this._registrant = new Registrant()
            {
                Email = Email,
                Form = this._form,
                FormKey = this._form.UId,
                Status = RegistrationStatus.Incomplete,
                Type = Live ? RegistrationType.Live : RegistrationType.Test,
                UId = Guid.NewGuid(),
                DateCreated = DateTimeOffset.Now
            };
            Context.Registrants.Add(this._registrant);

            // Now we do the survey specific stuff
            #region Survey

            if (this._form.Survey && this._form.ParentFormKey != null)
            {
                var parentReg = this._form.ParentForm.Registrants.FirstOrDefault(e => e.Email.ToLower() == this._registrant.Email);
                if (parentReg == null)
                    return View("SurveyEmailDoesNotExist", model);
            }

            #endregion

            model.Registrant = this._registrant;

            #region Contact Data Fill
            if (contact != null)
            {
                this._registrant.Contact = contact;
                foreach (var component in components)
                {
                    if (component.MappedToKey == null)
                        continue;
                    var c_data = contact.Data.Where(d => d.HeaderKey == component.MappedToKey).FirstOrDefault();
                    if (c_data == null)
                        continue;
                    var t_data = new RegistrantData()
                        {
                            VariableUId = component.UId,
                            Component = component as Component
                        };
                    if (component is Input)
                    {
                        t_data.Value = c_data.Value;
                        this._registrant.Data.Add(t_data);
                    }
                    else if (component is CheckboxGroup)
                    {
                        if (string.IsNullOrWhiteSpace(c_data.Value))
                            continue;
                        var items = Context.Components.OfType<CheckboxItem>().Where(i => i.CheckboxGroupKey == component.UId).ToList();
                        var r_items = c_data.Value.Split(',');
                        var c_items = new List<CheckboxItem>();
                        foreach (var r_item in r_items)
                        {
                            var r_t_item = r_item.Trim().ToLower();
                            var c_item = items.FirstOrDefault(i => i.LabelText.ToLower() == r_t_item);
                            if (c_item == null)
                                c_item = items.FirstOrDefault(i => Regex.Replace(i.LabelText.ToLower(), @"\s", "") == Regex.Replace(r_t_item, @"\s", ""));
                            if (c_item == null)
                                c_item = items.FirstOrDefault(i => i.UId.ToString().ToLower() == r_t_item);
                            if (c_item != null)
                                c_items.Add(c_item);
                        }
                        t_data.Value = JsonConvert.SerializeObject(c_items.Select(i => i.UId));
                        this._registrant.Data.Add(t_data);
                    }
                    else if (component is DropdownGroup)
                    {
                        var items = Context.Components.OfType<DropdownItem>().Where(i => i.DropdownGroupKey == component.UId).ToList();
                        var c_item = items.FirstOrDefault(i => i.LabelText.ToLower() == (String.IsNullOrWhiteSpace(c_data.Value) ? "" : c_data.Value.ToLower()));
                        if (c_item == null)
                            c_item = items.FirstOrDefault(i => i.UId.ToString().ToLower() == (String.IsNullOrWhiteSpace(c_data.Value) ? "" : c_data.Value.ToLower()));
                        if (c_item != null)
                        {
                            t_data.Value = c_item.UId.ToString();
                            this._registrant.Data.Add(t_data);
                        }
                    }
                    else if (component is RadioGroup)
                    {
                        var items = Context.Components.OfType<RadioItem>().Where(i => i.RadioGroupKey == component.UId).ToList();
                        var c_item = items.FirstOrDefault(i => i.LabelText.ToLower() == (String.IsNullOrWhiteSpace(c_data.Value) ? "" : c_data.Value.ToLower()));
                        if (c_item == null)
                            c_item = items.FirstOrDefault(i => i.UId.ToString().ToLower() == (String.IsNullOrWhiteSpace(c_data.Value) ? "" : c_data.Value.ToLower()));
                        if (c_item != null)
                        {
                            t_data.Value = c_item.UId.ToString();
                            this._registrant.Data.Add(t_data);
                        }
                    }
                }
            }
            #endregion
            var saveTask = Context.SaveChanges();

            SendRegistrantEmail(this._registrant, EmailType.Continue, this._registrant.Form);
            model.Page = this._form.Pages.Where(p => p.Type == PageType.RSVP).FirstOrDefault();
            if (model.Page != null)
            {
                if (model.Page.Enabled)
                {
                    ViewBag.First = true;
                    ViewBag.Form = this._form;
                    return View("RSVP", model);
                }
                model.Page = this._form.Pages.Where(p => p.Type == PageType.Audience).First();
                if (model.Page.Enabled && model.Form.Audiences.Count > 0)
                    if ((this._registrant.RSVP && model.Page.RSVP == RSVPType.Decline) || (!this._registrant.RSVP && model.Page.RSVP == RSVPType.Accept))
                        return Next(this._registrant.UId, 3);
                    else
                    {
                        ViewBag.RSVPEnabled = this._form.Pages.Where(p => p.Type == PageType.RSVP).First().Enabled;
                        ViewBag.Form = this._form;
                        return View("Audience", model);
                    }
            }
            return Next(this._registrant.UId, (this._form.Survey ? 1 : 3));
            //*/
            #endregion
        }

        [HttpGet]
        public ActionResult SmartLink(long formId, string confNumb)
        {
            this._form = this._form ?? Context.Forms.Where(f => f.SortingId == formId).FirstOrDefault();
            if (this._form == null)
                return RedirectToAction("Error404", "Error");
            var sortingId = -1L;
            if (!Int64.TryParse(confNumb, System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture, out sortingId))
                sortingId = -1L;
            var registrant = this._form.Registrants.FirstOrDefault(r => r.SortingId == sortingId);
            if (registrant == null)
                return RedirectToAction("Start", "Register", new { id = this._form.UId, email = confNumb });
            if (registrant.Status == RegistrationStatus.Submitted)
                return RedirectToAction("ShowConfirmation", new { id = registrant.UId });
            if (registrant.Status == RegistrationStatus.Incomplete)
                return RedirectToAction("ContinueRegistration", "Register", new { id = registrant.UId });
            if (registrant.Status == RegistrationStatus.Deleted)
                return RedirectToAction("Error404", "Error");
            return RedirectToAction("ShowConfirmation", new { id = registrant.UId });

        }

        #endregion

        #region Registration Status

        [HttpGet]
        public ActionResult ContinueRegistration(Guid id)
        {
            this._registrant = this._registrant ?? Context.Registrants.Where(r => r.UId == id).FirstOrDefault();
            if (this._registrant == null)
                return RedirectToAction("InvalidRegistrant");
            this._form = this._registrant.Form;
            ViewBag.Form = this._form;
            var live = this._registrant.Type == RegistrationType.Live;
            if (this._registrant.Status == RegistrationStatus.Submitted)
                return ShowConfirmation(this._registrant.UId);
            if (live)
            {
                if (this._form.Close < DateTimeOffset.UtcNow || this._form.Status == FormStatus.Closed)
                    return FormClosed(this._registrant.Form.UId);
                if (this._form.Status == FormStatus.Maintenance)
                    return UnderMaintenance(this._registrant.Form.UId);
            }
            var model = new RegistrationHtml(this);
            if (model.Page != null && model.Page.Enabled)
                return View("RSVP", model);
            if (this._form.Survey)
            {
                model.Page = this._form.Pages.OrderBy(p => p.PageNumber).First(p => p.Type == PageType.UserDefined);
                return Next(this._registrant.UId, model.Page.PageNumber);
            }
            model.Page = this._form.Pages.Where(p => p.Type == PageType.Audience).First();
            if (model.Page.Enabled && model.Form.Audiences.Count > 0)
                if ((this._registrant.RSVP && model.Page.RSVP == RSVPType.Decline) || (!this._registrant.RSVP && model.Page.RSVP == RSVPType.Accept))
                {
                    return Next(this._registrant.UId, 3);
                }
                else
                {
                    ViewBag.RSVPEnabled = this._form.Pages.Where(p => p.Type == PageType.RSVP).First().Enabled;
                    return View("Audience", model);
                }
            return Next(this._registrant.UId, 3);
        }

        [HttpGet]
        public ActionResult Cancel(Guid id)
        {
            this._registrant = this._registrant ?? Context.Registrants.Where(r => r.UId == id).FirstOrDefault();
            var form = this._registrant.Form;
            ViewBag.Form = form;
            this._registrant.Status = RegistrationStatus.Canceled;
            this._registrant.StatusDate = DateTime.UtcNow;
            var viewModel = new RegistrationHtml(this);
            Context.SaveChanges();
            SendRegistrantEmail(this._registrant, EmailType.Cancellation, this._registrant.Form);
            return View(viewModel);
        }

        [HttpGet]
        public ActionResult Edit(Guid id)
        {
            this._registrant = this._registrant ?? Context.Registrants.Where(r => r.UId == id).FirstOrDefault();
            if (this._registrant == null)
                return RedirectToAction("InvalidRegistrant");
            this._form = this._registrant.Form;
            if (!this._form.Editable)
                return NotEditable(this._form.UId, this._registrant.UId);
            ViewBag.Form = this._form;
            this._page = this._form.Pages.Where(p => p.Type == PageType.RSVP).First();
            var model = new RegistrationHtml(this);
            if (model.Page.Enabled)
                return View("RSVP", model);
            model.Page = this._form.Pages.Where(p => p.Type == PageType.Audience).First();
            if (model.Page.Enabled && model.Form.Audiences.Count > 0)
                if ((this._registrant.RSVP && model.Page.RSVP == RSVPType.Decline) || (!this._registrant.RSVP && model.Page.RSVP == RSVPType.Accept))
                    return RedirectToAction("Next", new { RegistrantKey = this._registrant.UId, PageNumber = 3 });
                else
                {
                    ViewBag.RSVPEnabled = this._form.Pages.Where(p => p.Type == PageType.RSVP).First().Enabled;
                    return View("Audience", model);
                }
            return Next(this._registrant.UId, 3);
        }

        #endregion

        #region RSVP and Audience

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RSVP(bool RSVP, Guid RegistrantKey, int PageNumber)
        {
            this._registrant = this._registrant ?? Context.Registrants.FirstOrDefault(r => r.UId == RegistrantKey);
            if (this._registrant == null)
                return RedirectToAction("InvalidRegistrant");
            this._form = this._registrant.Form;
            ViewBag.Form = this._form;
            if (this._registrant.Type == RegistrationType.Live)
            {
                if (this._form.Close < DateTimeOffset.UtcNow || this._form.Status == FormStatus.Closed)
                    return RedirectToAction("FormClosed", new { id = this._form.UId });
                if (this._form.Status == FormStatus.Maintenance)
                    return RedirectToAction("UnderMaintenance", new { id = this._form.UId });
                if (this._form.Status != FormStatus.Open)
                    return RedirectToAction("FormClosed", new { id = this._form.UId });
            }
            this._registrant.RSVP = RSVP;
            this._registrant.ModifiedBy = Guid.Empty;
            Context.SaveChanges();
            this._page = this._form.Pages.Where(p => p.Type == PageType.Audience).First();
            var model = new RegistrationHtml(this);
            if (model.Page.Enabled && model.Form.Audiences.Count > 0)
                if ((this._registrant.RSVP && model.Page.RSVP == RSVPType.Decline) || (!this._registrant.RSVP && model.Page.RSVP == RSVPType.Accept))
                    return Next(RegistrantKey, 3);
                else
                    return View("Audience", model);
            return Next(RegistrantKey, 3);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Audience(Guid Audience, Guid RegistrantKey, int PageNumber)
        {
            this._registrant = this._registrant ?? Context.Registrants.Where(r => r.UId == RegistrantKey).FirstOrDefault();
            if (this._registrant == null)
                return RedirectToAction("InvalidRegistrant");
            this._form = this._registrant.Form;
            ViewBag.Form = this._form;
            if (this._registrant.Type == RegistrationType.Live)
            {
                if (this._form.Close < DateTimeOffset.UtcNow || this._form.Status == FormStatus.Closed)
                    return RedirectToAction("FormClosed", new { id = this._form.UId });
                if (this._form.Status == FormStatus.Maintenance)
                    return RedirectToAction("UnderMaintenance", new { id = this._form.UId });
            }


            this._registrant.AudienceKey = Audience;
            this._registrant.ModifiedBy = Guid.Empty;
            Context.SaveChanges();
            return Next(RegistrantKey, 3);
        }

        #endregion

        #region Confirmation

        [HttpGet]
        public ActionResult Confirmation(Guid RegistrantKey)
        {
            this._registrant = this._registrant ?? Context.Registrants.Where(r => r.UId == RegistrantKey).FirstOrDefault();
            if (this._registrant == null)
                return RedirectToAction("InvalidRegistrant");
            this._form = this._registrant.Form;
            this._registrant.UpdateAccounts();
            ViewBag.Form = this._form;
            this._registrant.Status = RegistrationStatus.Submitted;
            this._registrant.StatusDate = DateTimeOffset.UtcNow;
            SendRegistrantEmail(this._registrant, EmailType.Confirmation, this._registrant.Form);
            this._page = this._form.Pages.Where(p => p.Type == PageType.Confirmation).First();
            var model = new RegistrationHtml(this);
            Context.SaveChanges();
            return View("Confirmation", model);
        }

        [HttpGet]
        public ActionResult ShowConfirmation(Guid RegistrantKey)
        {
            this._registrant = this._registrant ?? Context.Registrants.Where(r => r.UId == RegistrantKey).FirstOrDefault();
            if (this._registrant == null)
                return RedirectToAction("InvalidRegistrant");
            this._form = this._registrant.Form;
            this._registrant.UpdateAccounts();
            ViewBag.Form = this._form;
            this._page = this._form.Pages.Where(p => p.Type == PageType.Confirmation).First();
            var model = new RegistrationHtml(this);
            this._registrant.UpdateAccounts();
            Context.SaveChanges();
            if (this._form.Survey)
                return View("SurveyConfirmation", model);
            return View("Confirmation", model);
        }

        #endregion

        #region Next and Back

        [HttpGet]
        public ActionResult Next(Guid RegistrantKey, int PageNumber)
        {
            this._registrant = this._registrant ?? Context.Registrants.FirstOrDefault(r => r.UId == RegistrantKey);
            if (this._registrant == null)
                return RedirectToAction("InvalidRegistrant");
            this._form = this._registrant.Form;
            ViewBag.Form = this._form;
            if (this._registrant.Type == RegistrationType.Live)
            {
                if (this._form.Close < DateTimeOffset.UtcNow || this._form.Status == FormStatus.Closed)
                    return RedirectToAction("FormClosed", new { id = this._form.UId });
                if (this._form.Status == FormStatus.Maintenance)
                    return RedirectToAction("UnderMaintenance", new { id = this._form.UId });
            }
            this._page = this._form.Pages.Where(p => p.PageNumber == PageNumber).FirstOrDefault();
            var model = new RegistrationHtml(this);
            if (model.Page == null)
            {
                if (!this._form.Survey)
                {
                    if (PageNumber != int.MaxValue)
                    {
                        return View("Review", new RegistrationHtml(this));
                    }
                    if (this._form.DisableShoppingCart)
                    {
                        return Confirmation(RegistrantKey);
                    }
                    this._registrant.UpdateAccounts();
                    var skipPayment = this._registrant.Data.Where(d => d.Component.Variable != null && d.Component.Variable.Value == "__SkipPayment" && d.Value == "true").Count() == 1;
                    var unbalanced = this._registrant.TotalOwed > 0;
                    var noPromo = this._registrant.Data.Where(d => d.Component.Variable != null && d.Component.Variable.Value == "__NoPromo" && d.Value == "true").Count() > 0;
                    if (noPromo)
                        this._registrant.PromotionalCodes.Clear();
                    if (this._form.PromotionalCodes.Count > 0 && unbalanced && !noPromo)
                    {
                        return PromotionCodes(RegistrantKey);
                    }
                    //We reached the end of the form. First thing we do is check if there is a balanced owed.
                    if (unbalanced && !skipPayment)
                    {
                        return ShoppingCart(RegistrantKey);
                    }
                    return Confirmation(RegistrantKey);
                }
                else
                {
                    model.Page = this._form.Pages.First(p => p.Type == PageType.Confirmation);
                    this._registrant.Status = RegistrationStatus.Submitted;
                    Context.SaveChanges();
                    SendRegistrantEmail(this._registrant, EmailType.Confirmation, this._form);
                    return View("SurveyConfirmation", model);
                }
            }

            bool skip = false;

            if (!model.Page.Enabled || (model.Page.AdminOnly && model.Registrant.Type == RegistrationType.Live))
                return RedirectToAction("Next", new { RegistrantKey = RegistrantKey, PageNumber = ++PageNumber });
            if ((this._registrant.RSVP && model.Page.RSVP == RSVPType.Decline) || (!this._registrant.RSVP && model.Page.RSVP == RSVPType.Accept))
                skip = true;
            if (model.Page.Audiences.Count > 0 && (this._registrant.Audience == null || !model.Page.Audiences.Contains(this._registrant.Audience)))
                skip = true;

            if (model.Page.Type == PageType.RSVP)
                return View("RSVP", model);
            else if (model.Page.Type == PageType.Audience && model.Form.Audiences.Count > 0)
                return View("Audience", model);

            var blank = model.IsBlank(model.Page, this._registrant);
            skip = skip || blank;
            IEnumerable<JLogicCommand> commands = new List<JLogicCommand>();
            if (!skip)
                commands = LogicEngine.RunLogic(model.Page, registrant: this._registrant, allCommands: LogicCommands);
            foreach (var command in commands)
            {
                switch (command.Command)
                {
                    case JLogicWork.PageSkip:
                        skip = true;
                        break;
                    case JLogicWork.SetVar:
                        if (!command.Parameters.Keys.Contains("Variable") || String.IsNullOrWhiteSpace(command.Parameters["Variable"]))
                            continue;
                        if (!command.Parameters.Keys.Contains("Value") || String.IsNullOrWhiteSpace(command.Parameters["Value"]))
                            continue;
                        this._registrant.SetData(command.Parameters["Variable"], command.Parameters["Value"], ignoreValidation: true);
                        break;
                }
            }

            if (skip)
            {
                SetPageValuesBlank(model.Page, this._registrant, blank);
                return Next(RegistrantKey, ++PageNumber);
            }
            this._page = this._form.Pages.Where(p => p.PageNumber == PageNumber).FirstOrDefault();
            var viewModel = new RegistrationHtml(this);
            Context.SaveChanges();
            return View("Next", viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Next(RegistrationModel model)
        {
            #region Initialization
            this._registrant = this._registrant ?? Context.Registrants.FirstOrDefault(r => r.UId == model.RegistrantKey);
            if (this._registrant == null)
                return RedirectToAction("InvalidRegistrant");
            var formData = Request.Form;
            var errors = new List<SetDataError>();
            this._form = this._registrant.Form;

            ViewBag.Form = this._form;
            if (this._registrant.Type == RegistrationType.Live)
            {
                if (this._form.Close < DateTimeOffset.UtcNow || this._form.Status == FormStatus.Closed)
                    return RedirectToAction("FormClosed", new { id = this._form.UId });
                if (this._form.Status == FormStatus.Maintenance)
                    return RedirectToAction("UnderMaintenance", new { id = this._form.UId });
            }

            #endregion

            #region Run through uploaded files

            foreach (var fileKey in Request.Files.AllKeys)
            {
                Guid fUId;
                if (!Guid.TryParse(fileKey, out fUId))
                    continue;
                var fileComponent = this._form.GetUnorderedComponents().Where(c => c.UId == fUId).OfType<Input>().FirstOrDefault();
                if (fileComponent == null)
                    continue;
                var f_dp = this._registrant.Data.FirstOrDefault(d => d.VariableUId == fUId);
                if (f_dp == null)
                {
                    f_dp = new RegistrantData()
                    {
                        VariableUId = fUId,
                        Value = "",
                        Registrant = this._registrant,
                        RegistrantKey = this._registrant.UId
                    };
                    this._registrant.Data.Add(f_dp);
                }
                f_dp.Component = fileComponent;
                if (Request.Files[fileKey].ContentLength < 1)
                {
                    if (fileComponent.Required && f_dp != null && f_dp.Value != "UPLOADED")
                    {
                        errors.Add(new SetDataError(fileComponent.UId.ToString(), "This field is required."));
                        continue;
                    }
                    if (f_dp.Value == "UPLOADED")
                        continue;
                }
                var file = Request.Files[fileKey];
                var fileSize = file.ContentLength;
                if (fileSize > 2097152)
                {
                    errors.Add(new SetDataError(fileComponent.UId.ToString(), "Your file cannot exceed 2 Mb."));
                    continue;
                }
                var mimeType = file.ContentType;
                if (!AllowedTypes[fileComponent.FileType].Contains(mimeType))
                {
                    errors.Add(new SetDataError(fileComponent.UId.ToString(), "Unauthorized file type. The file must be a " + fileComponent.FileType + "."));
                    continue;
                }
                var stream = file.InputStream;
                if (f_dp.File == null)
                    f_dp.File = new RegistrantFile()
                    {
                        FileType = mimeType,
                        Extension = Path.GetExtension(file.FileName),
                        BinaryData = new byte[fileSize]
                    };
                stream.Read(f_dp.File.BinaryData, 0, file.ContentLength);
                f_dp.Value = "UPLOADED";
            }

            #endregion

            #region Run through non file components

            //First we run through waitlistings
            foreach (var kvp in model.Waitlistings)
            {
                // Lets grab the component
                var component = this._form.GetUnorderedComponents().Where(c => c.UId == kvp.Key).FirstOrDefault();
                if (component == null)
                    component = this._form.GetComponentItems().Where(c => c.UId == kvp.Key).FirstOrDefault();
                if (component == null || component.Seating == null)
                    continue;
                if (component.Seating.Waitlistable)
                {
                    // It is waitlisting
                    if (kvp.Value)
                    {
                        if (component.Seating.Seaters.Where(s => s.ComponentKey == component.UId && s.RegistrantKey == this._registrant.UId && s.Seated == false).Count() < 1)
                        {
                            // Not currently waitlisting.
                            var t_seat = Seater.New(component.Seating, this._registrant, component as Component, false);
                        }
                    }
                    else
                    {
                        var seater = component.Seating.Seaters.Where(s => s.ComponentKey == component.UId && s.RegistrantKey == this._registrant.UId && s.Seated == false).FirstOrDefault();
                        if (seater != null)
                            Context.Seaters.Remove(seater); // Remove waitlisting if previously was.
                    }
                }
            }

            //First thing we do is iterate over the components sent to us.
            foreach (var kvp in model.Components)
            {
                var result = this._registrant.SetData(kvp.Key.ToString(), kvp.Value, resetValueOnError: false);
                result.Errors.ForEach(e => errors.Add(e));
            }
            #endregion

            #region Run page advance logic

            this._page = this._form.Pages.Where(p => p.PageNumber == model.PageNumber).First();
            var commands = LogicEngine.RunLogic(this._page, registrant: this._registrant, onLoad: false, allCommands: LogicCommands);
            foreach (var command in commands)
            {
                switch (command.Command)
                {
                    case JLogicWork.FormHalt:
                        errors.Add(new SetDataError("", command.Parameters["Text"]));
                        break;
                    case JLogicWork.PageHalt:
                        var message = command.Parameters["Text"];
                        errors.Add(new SetDataError("", message));
                        break;
                    case JLogicWork.SetVar:
                        if (!command.Parameters.Keys.Contains("Variable") || String.IsNullOrWhiteSpace(command.Parameters["Variable"]))
                            continue;
                        if (!command.Parameters.Keys.Contains("Value") || String.IsNullOrWhiteSpace(command.Parameters["Value"]))
                            continue;
                        this._registrant.SetData(command.Parameters["Variable"], command.Parameters["Value"], ignoreValidation: true);
                        break;
                }
            }
            #endregion

            //Check for form errors.  If there are, it halts the advancement and returns the form again.
            if (errors.Count > 0)
            {
                this._page = this._form.Pages.Where(p => p.PageNumber == model.PageNumber).First();
                var errorModel = new RegistrationHtml(this, errors: errors);
                return View(errorModel);
            }
            Context.SaveChanges();
            return Next(this._registrant.UId, model.PageNumber + 1);
        }

        [HttpGet]
        public ActionResult Back(Guid id, int pageNumber)
        {
            this._registrant = this._registrant ?? Context.Registrants.Where(r => r.UId == id).FirstOrDefault();
            if (this._registrant == null)
                return RedirectToAction("InvalidRegistrant");

            this._form = this._registrant.Form;

            ViewBag.Form = this._form;
            bool live = this._registrant.Type == RegistrationType.Live;
            this._page = this._form.Pages.Where(p => p.PageNumber == (pageNumber - 1)).FirstOrDefault();
            var model = new RegistrationHtml(this);
            if (model.Page == null)
            {
                model.Page = this._form.Pages.Where(p => p.Type == PageType.RSVP).First();
                if (model.Page.Enabled)
                    return View("RSVP", model);
                model.Page = this._form.Pages.Where(p => p.Type == PageType.Audience).First();
                if (model.Page.Audiences.Count == 0)
                    if (!((model.Page.RSVP == RSVPType.Decline && !this._registrant.RSVP) || (model.Page.RSVP == RSVPType.Accept && this._registrant.RSVP)))
                        return Next(this._registrant.UId, 3);
                return View("Audience", model);
            }


            if (!model.Page.Enabled)
                return Back(this._registrant.UId, pageNumber - 1);
            if ((this._registrant.RSVP && model.Page.RSVP == RSVPType.Decline) || (!this._registrant.RSVP && model.Page.RSVP == RSVPType.Accept))
                return Back(this._registrant.UId, pageNumber - 1);
            if (model.Page.Audiences.Count != 0 && (this._registrant.Audience != null && !model.Page.Audiences.Contains(this._registrant.Audience)))
                return Back(this._registrant.UId, pageNumber - 1);

            var commands = LogicEngine.RunLogic(model.Page, registrant: this._registrant, allCommands: LogicCommands);
            foreach (var command in commands)
            {
                switch (command.Command)
                {
                    case JLogicWork.PageSkip:
                        return Back(this._registrant.UId, pageNumber - 1);
                }
            }

            if (model.Page.Type == PageType.Audience)
            {
                if (model.Page.Enabled && model.Form.Audiences.Count > 0)
                    if ((this._registrant.RSVP && model.Page.RSVP == RSVPType.Decline) || (!this._registrant.RSVP && model.Page.RSVP == RSVPType.Accept))
                        return Back(this._registrant.UId, pageNumber - 1);
                    else
                        return View("Audience", model);
                else
                    return Back(this._registrant.UId, pageNumber - 1);
            }
            else if (model.Page.Type == PageType.RSVP)
                return View("RSVP", model);

            if (model.Page.IsBlank(this._registrant, allCommands: LogicCommands))
                return Back(this._registrant.UId, (pageNumber - 1));

            return Next(this._registrant.UId, (pageNumber - 1));
        }

        #endregion

        #region Merchant

        [HttpGet]
        public ActionResult PromotionCodes(Guid RegistrantKey)
        {
            this._registrant = this._registrant ?? Context.Registrants.Where(r => r.UId == RegistrantKey).FirstOrDefault();
            if (this._registrant == null)
                return RedirectToAction("InvalidRegistrant");
            this._registrant.UpdateAccounts();
            this._form = this._registrant.Form;

            ViewBag.Form = this._form;
            if (this._registrant.Type == RegistrationType.Live)
            {
                if (this._form.Close < DateTimeOffset.UtcNow || this._form.Status == FormStatus.Closed)
                    return RedirectToAction("FormClosed", new { id = this._form.UId });
                if (this._form.Status == FormStatus.Maintenance)
                    return RedirectToAction("UnderMaintenance", new { id = this._form.UId });
            }

            if (this._form.PromotionalCodes.Count == 0)
            {
                return ShoppingCart(this._registrant.UId);
            }
            var model = new RegistrationHtml(this);
            return View("PromotionCodes", model);
        }

        [HttpPost]
        public JsonNetResult AddPromotionCode(Guid registrantKey, string codeEntered)
        {
            this._registrant = this._registrant ?? Context.Registrants.Where(r => r.UId == registrantKey).FirstOrDefault();
            if (this._registrant == null)
                return new JsonNetResult()
                {
                    Data = new { Success = false, Message = "Invalid Registrant." },
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                };
            this._registrant.UpdateAccounts();
            this._form = this._registrant.Form;
            if (this._form == null)
                return new JsonNetResult()
                {
                    Data = new { Success = false, Message = "Invalid Form." },
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                };

            var code = Context.PromotionCodes.Where(c => c.Code == codeEntered && c.FormKey == this._form.UId).FirstOrDefault();
            if (code == null)
                return new JsonNetResult()
                {
                    Data = new { Success = false, Message = "Invalid Code." },
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                };
            var userCode = this._registrant.PromotionalCodes.Where(c => c.CodeKey == code.UId).FirstOrDefault();
            if (userCode != null)
                return new JsonNetResult()
                {
                    Data = new { Success = false, Message = "Code Used." },
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                };
            var usedCount = Context.PromotionCodeEntries.Where(c => c.CodeKey == code.UId && c.Registrant.Type == this._registrant.Type).Count();
            if (usedCount >= code.Limit && code.Limit != -1)
                return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = false, Message = "Code no longer available." } };
            var entry = new PromotionCodeEntry()
            {
                Code = code,
                CodeKey = code.UId,
                Registrant = this._registrant,
                RegistrantKey = this._registrant.UId,
                UId = Guid.NewGuid()
            };
            Context.PromotionCodeEntries.Add(entry);
            Context.SaveChanges();
            return new JsonNetResult()
            {
                Data = new { Success = true, Message = code.Description },
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        [HttpGet]
        public ActionResult CreditCard(Guid id)
        {
            this._registrant = this._registrant ?? Context.Registrants.Where(r => r.UId == id).FirstOrDefault();
            if (this._registrant == null)
                return RedirectToAction("InvalidRegistrant");
            this._registrant.UpdateAccounts();
            this._form = this._registrant.Form;
            ViewBag.Form = this._form;
            var forceBillMe = this._registrant.Data.Where(d => d.Component.Variable != null && d.Component.Variable.Value == "__ForceBillMe" && d.Value == "true").Count() == 1;
            if (forceBillMe)
                return BillMe(this._registrant.UId);
            var model = new CreditCardModel()
            {
                RegistrantKey = this._registrant.UId,
                FormKey = this._registrant.FormKey
            };
            model.Desc = this._form.MerchantAccount.Descriminator;
            ViewBag.RegHtml = new RegistrationHtml(this);
            if (this._registrant.TotalOwed < 0m)
            {
                return Confirmation(this._registrant.UId);
            }
            return View("CreditCard", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreditCard(CreditCardModel model)
        {
            #region Initialization
            var formData = Request.Form;
            var errors = new Dictionary<Guid, string>();
            this._form = this._form ?? Context.Forms.Where(f => f.UId == model.FormKey).FirstOrDefault();
            if (this._form == null)
                return RedirectToAction("InvalidForm");

            this._registrant = this._registrant ?? Context.Registrants.Where(r => r.UId == model.RegistrantKey && r.FormKey == this._form.UId).FirstOrDefault();
            if (this._registrant == null)
                return RedirectToAction("InvalidRegistrant");
            this._registrant.UpdateAccounts();
            #endregion
            try
            {

                ViewBag.Form = this._form;
                var ammount = this._registrant.TotalOwed;
                if (this._form.MerchantAccount == null)
                    return Confirmation(this._registrant.UId);
                IMerchantAccount<TransactionRequest> gateway = this._form.MerchantAccount.GetGateway();
                model.Desc = this._form.MerchantAccount.Descriminator;
                //Now we build the transaction Request.
                var request = new TransactionRequest()
                {
                    Zip = model.ZipCode,
                    Ammount = ammount,
                    CVV = model.CardCode.Trim(),
                    CardNumber = model.CardNumber.Trim(),
                    NameOnCard = model.NameOnCard.Trim(),
                    ExpMonthAndYear = model.ExpMonth + model.ExpYear,
                    Cart = JsonConvert.SerializeObject(this._registrant.GetShoppingCartItems().ToCart()),
                    CompanyKey = this._form.CompanyKey,
                    MerchantAccountKey = this._form.MerchantAccountKey,
                    FormKey = this._form.UId,
                    Form = this._form,
                    RegistrantKey = this._registrant.UId,
                    Registrant = this._registrant,
                    TransactionType = TransactionType.AuthorizeCapture,
                    Mode = (this._registrant.Type == RegistrationType.Live ? ServiceMode.Live : ServiceMode.Test),
                    LastFour = model.CardNumber.GetLast(4),
                    DateCreated = DateTimeOffset.UtcNow,
                    DateModified = DateTimeOffset.UtcNow,
                    Currency = this._form.Currency,
                    Address = model.Line1,
                    Address2 = model.Line2,
                    City = model.City,
                    Country = model.Country,
                    State = model.State,
                    Phone = model.Phone
                };
                if (this._form.MerchantAccount.Descriminator == "paypal")
                {
                    request.CardType = model.CardType;
                }
                var firstAndLast = Regex.Replace(model.NameOnCard, @"\s\w\s", " ");
                var spaceIndex = firstAndLast.IndexOf(' ');
                if (spaceIndex == -1)
                {
                    model.Errors.Add("NameOnCard", "You must supply a first and last name.");
                    request.FirstName = "";
                    request.LastName = model.NameOnCard;
                    return View(model);
                }
                else
                {
                    request.FirstName = firstAndLast.Substring(0, spaceIndex).Trim();
                    request.LastName = firstAndLast.Substring(spaceIndex).Trim();
                    if (request.LastName.Length < 2)
                    {
                        model.Errors.Add("NameOnCard", "You must supply a first and last name.");
                    }
                }
                TransactionDetail td;
                if (this._registrant.Type == RegistrationType.Live)
                {
                    request.Mode = ServiceMode.Live;
                    if (String.IsNullOrEmpty(model.CardNumber))
                        model.Errors["CardNumber"] = "Invalid credit card number.";
                    if (!CCHelper.ValidateCard(model.CardNumber))
                        model.Errors["CardNumber"] = "Invalid credit card number.";
                    if (String.IsNullOrEmpty(model.CardCode))
                        model.Errors["CardCode"] = "Invalid card code entered.";
                    if (!Regex.IsMatch(model.CardCode, @"^[0-9]+$"))
                        model.Errors["CardCode"] = "Invalid card code entered.";
                    if (String.IsNullOrWhiteSpace(model.NameOnCard))
                        model.Errors["NameOnCard"] = "You must enter a name.";
                    if (String.IsNullOrWhiteSpace(model.ZipCode))
                        model.Errors["ZipCode"] = "You must enter a zip code.";
                    if (model.Errors.Count > 0)
                    {
                        ViewBag.RegHtml = new RegistrationHtml(this);
                        return View(model);
                    }
                    td = gateway.AuthorizeCapture(request);
                }
                else
                {
                    td = new TransactionDetail()
                    {
                        Approved = true,
                        Ammount = request.Ammount,
                        AuthorizationCode = "TEST",
                        FormKey = request.FormKey,
                        Message = "Success",
                        TransactionType = TransactionType.AuthorizeCapture,
                        TransactionID = "__TEST__" + Guid.NewGuid().ToString()
                    };
                }
                if (!this._registrant.TransactionRequests.Contains(request))
                    this._registrant.TransactionRequests.Add(request);
                if (!request.Details.Contains(td))
                    request.Details.Add(td);
                Context.TransactionRequests.Add(request);
                this._registrant.UpdateAccounts();
                Context.SaveChanges();
                ViewBag.RegHtml = new RegistrationHtml(this);

                if (td != null && td.Approved)
                {
                    ViewBag.Test = request.Mode == ServiceMode.Live;
                    return View("CreditCardConfirmation", new ChargeConfirmationModel() { TrId = request.UId, Id = this._registrant.UId });
                }
                else
                {
                    model.Errors[""] = "The charge was declined by the credit card company.";
                    ViewBag.RegHtml = new RegistrationHtml(this);
                    return View(model);
                }
            }
            catch (Exception sex)
            {
                iLog.Error(sex);
                ViewBag.RegHtml = new RegistrationHtml(this);
                model.Errors[""] = "There was an issue while attempting to process your credit card. Please double check your information and ensure you are including all required fields. The name on card must include a given name and surname.";
                return View(model);
            }
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChargeConfirmation(ChargeConfirmationModel model)
        {
            this._registrant = this._registrant ?? Context.Registrants.Where(r => r.UId == model.Id).FirstOrDefault();
            if (this._registrant == null)
                return RedirectToAction("InvalidRegistrant");
            this._registrant.UpdateAccounts();
            this._form = this._registrant.Form;
            ViewBag.Form = this._form;
            ViewBag.RegHtml = new RegistrationHtml(this);
            MailAddress email;
            var emails = new List<MailAddress>();
            try
            {
                email = new MailAddress(model.Email);
                emails.Add(email);
            }
            catch (Exception)
            {
                model.Email = null;
            }
            SendRegistrantEmail(this._registrant, EmailType.CreditCardReciept, this._registrant.Form, emails);
            return Confirmation(this._registrant.UId);
        }

        [HttpGet]
        public ActionResult BillMe(Guid id)
        {
            this._registrant = this._registrant ?? Context.Registrants.Where(r => r.UId == id).FirstOrDefault();
            var forceCreditCard = this._registrant.Data.Where(d => d.Component.Variable != null && d.Component.Variable.Value == "__ForceCreditCard" && d.Value != null && d.Value == "true").Count() == 1;
            if (forceCreditCard)
                return CreditCard(this._registrant.UId);
            if (this._registrant == null)
                return RedirectToAction("InvalidRegistrant");
            this._form = this._registrant.Form;
            ViewBag.Form = this._form;
            ViewBag.RegHtml = new RegistrationHtml(this);
            Context.SaveChanges();
            return View("BillMe", this._registrant);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult BillMe(Guid id, string email, string payingAgentNumber, string payingAgentName)
        {
            this._registrant = this._registrant ?? Context.Registrants.Where(r => r.UId == id).FirstOrDefault();
            if (this._registrant == null)
                return RedirectToAction("InvalidRegistrant");
            this._registrant.UpdateAccounts();
            this._form = this._registrant.Form;
            this._registrant.PayingAgentNumber = payingAgentNumber;
            this._registrant.PayingAgentName = payingAgentName;
            ViewBag.Form = this._form;
            ViewBag.RegHtml = new RegistrationHtml(this);
            MailAddress t_email;
            var emails = new List<MailAddress>();
            try
            {
                t_email = new MailAddress(email);
                emails.Add(t_email);
            }
            catch (Exception)
            {
                email = null;
            }
            SendRegistrantEmail(this._registrant, EmailType.BillMeInvoice, this._registrant.Form, emails);
            return Confirmation(this._registrant.UId);
        }

        [HttpGet]
        public ActionResult ShoppingCart(Guid RegistrantKey)
        {
            this._registrant = this._registrant ?? Context.Registrants.Where(r => r.UId == RegistrantKey).FirstOrDefault();
            if (this._registrant == null)
                return RedirectToAction("InvalidRegistrant");
            this._registrant.UpdateAccounts();
            this._form = this._registrant.Form;

            ViewBag.Form = this._form;
            if (this._registrant.Type == RegistrationType.Live)
            {
                if (this._form.Close < DateTimeOffset.UtcNow || this._form.Status == FormStatus.Closed)
                    return RedirectToAction("FormClosed", new { id = this._form.UId });
                if (this._form.Status == FormStatus.Maintenance)
                    return RedirectToAction("UnderMaintenance", new { id = this._form.UId });
            }


            var skipPayment = this._registrant.Data.Where(d => d.Component.Variable != null && d.Component.Variable.Value == "__SkipPayment" && d.Value == "true").Count() == 1;
            var unbalanced = this._registrant.TotalOwed > 0;
            if (skipPayment)
                return Confirmation(RegistrantKey);
            var billMeOnly = this._registrant.Data.Where(d => d.Component.Variable != null && d.Component.Variable.Value == "__BillMe" && d.Value == "true").Count() == 1;
            ViewBag.BillMeOnly = billMeOnly;
            var model = new RegistrationHtml(this);
            Context.SaveChanges();
            return View("ShoppingCart", model);
        }

        #endregion

        #region Sharing

        public ActionResult ShareEmail(Guid id, IEnumerable<string> recipients, Dictionary<string, string> values, Guid regKey)
        {
            this._registrant = this._registrant ?? Context.Registrants.Where(r => r.UId == regKey).FirstOrDefault();
            var apiController = new API.Email_SendController(Context);
            var data = apiController.Post(new API.api_EmailRequest() { id = id, recipients = recipients.ToList(), values = values, scheduled = false, regId = this._registrant.SortingId }) as System.Web.Http.Results.OkNegotiatedContentResult<API.api_EmailResponse>;
            return ShowConfirmation(regKey);
        }

        #endregion

        [HttpGet]
        public FileContentResult Invoice(Guid id)
        {
            this._registrant = this._registrant ?? Context.Registrants.Where(r => r.UId == id).First();
            this._registrant.UpdateAccounts();
            var binaryData = new byte[0];
            using (var m_stream = new MemoryStream())
            {
                var titleFont = FontFactory.GetFont("Arial", 18, Font.BOLD);
                var subTitleFont = FontFactory.GetFont("Arial", 14, Font.BOLD);
                var boldTableFont = FontFactory.GetFont("Arial", 12, Font.BOLD);
                var endingMessageFont = FontFactory.GetFont("Arial", 10, Font.ITALIC);
                var bodyFont = FontFactory.GetFont("Arial", 12, Font.NORMAL);
                var document = new Document(PageSize.A4, 40, 25, 40, 65);
                PdfWriter.GetInstance(document, m_stream);
                document.Open();
                document.Add(new Paragraph(this._registrant.Form.Name + ": Invoice/Reciept", titleFont));
                var table = new Table(4);
                table.DefaultHorizontalAlignment = 0;
                table.DefaultCellBorder = 0;
                table.WidthPercentage = 100;
                table.SpaceInsideCell = 5;
                table.Border = 0;
                table.AddCell(new Phrase("Item", boldTableFont));
                table.AddCell(new Phrase("Price", boldTableFont));
                table.AddCell(new Phrase("Quanity", boldTableFont));
                table.AddCell(new Phrase("Total", boldTableFont));

                var shoppingCart = this._registrant.GetShoppingCartItems();
                var runningTotal = 0.00m;
                
                foreach (var item in shoppingCart.Items)
                {
                    var total = (decimal)item.Quanity * item.Ammount;
                    runningTotal += total;
                    table.AddCell(new Phrase(item.Name, bodyFont));
                    table.AddCell(new Phrase(Math.Round(item.Ammount).ToString("c", this._registrant.Form.Culture), bodyFont));
                    table.AddCell(new Phrase(item.Quanity.ToString(), bodyFont));
                    table.AddCell(new Phrase(Math.Round(total).ToString("c", this._registrant.Form.Culture), bodyFont));
                }
                table.AddCell(new Phrase(""));
                table.AddCell(new Phrase(""));
                table.AddCell(new Phrase("Total Fees:", boldTableFont));
                table.AddCell(new Phrase(Math.Round(runningTotal).ToString("c", this._registrant.Form.Culture), boldTableFont));

                foreach (var code in this._registrant.PromotionalCodes.Where(c => c.Code.Action == RSToolKit.Domain.Entities.ShoppingCartAction.Add))
                {
                    runningTotal += code.Code.Amount;
                    table.AddCell(new Phrase(code.Code.Code, bodyFont));
                    table.AddCell(new Phrase("Add", bodyFont));
                    table.AddCell(new Phrase(Math.Round(code.Code.Amount).ToString("c", this._registrant.Form.Culture), bodyFont));
                    table.AddCell(new Phrase(Math.Round(runningTotal).ToString("c", this._registrant.Form.Culture), bodyFont));
                }
                foreach (var code in this._registrant.PromotionalCodes.Where(c => c.Code.Action == RSToolKit.Domain.Entities.ShoppingCartAction.Subtract))
                {
                    runningTotal -= code.Code.Amount;
                    table.AddCell(new Phrase(code.Code.Code, bodyFont));
                    table.AddCell(new Phrase("Subtract", bodyFont));
                    table.AddCell(new Phrase(Math.Round(code.Code.Amount).ToString("c", this._registrant.Form.Culture), bodyFont));
                    table.AddCell(new Phrase(Math.Round(runningTotal).ToString("c", this._registrant.Form.Culture), bodyFont));
                }
                foreach (var code in this._registrant.PromotionalCodes.Where(c => c.Code.Action == RSToolKit.Domain.Entities.ShoppingCartAction.Multiply))
                {
                    var workingTotal = runningTotal;
                    runningTotal *= code.Code.Amount;
                    table.AddCell(new Phrase(code.Code.Code, bodyFont));
                    table.AddCell(new Phrase("Percentage (" + ((1 - code.Code.Amount).ToString("p", this._registrant.Form.Culture)) + ")", bodyFont));
                    table.AddCell(new Phrase(Math.Round((workingTotal * (1 - code.Code.Amount))).ToString("c", this._registrant.Form.Culture), bodyFont));
                    table.AddCell(new Phrase(Math.Round(runningTotal).ToString("c", this._registrant.Form.Culture), bodyFont));
                }
                if (this._registrant.PromotionalCodes.Count > 0)
                {
                    table.AddCell(new Phrase(""));
                    table.AddCell(new Phrase(""));
                    table.AddCell(new Phrase("Less Discounts:", boldTableFont));
                    table.AddCell(new Phrase(Math.Round(runningTotal).ToString("c", this._registrant.Form.Culture), boldTableFont));
                }
                if (this._registrant.Form.Tax.HasValue)
                {
                    runningTotal += shoppingCart.Taxes();
                    table.AddCell(new Phrase(this._registrant.Form.TaxDescription, bodyFont));
                    table.AddCell(new Phrase(""));
                    table.AddCell(new Phrase(this._registrant.Form.Tax.Value.ToString("p", this._registrant.Form.Culture), bodyFont));
                    table.AddCell(new Phrase(Math.Round(shoppingCart.Taxes()).ToString("c", this._registrant.Form.Culture), bodyFont));

                    table.AddCell(new Phrase(""));
                    table.AddCell(new Phrase(""));
                    table.AddCell(new Phrase("Total:", boldTableFont));
                    table.AddCell(new Phrase(Math.Round(runningTotal).ToString("c", this._registrant.Form.Culture), boldTableFont));
                }
                foreach (var detail in this._registrant.Details())
                {
                    var charge = (detail.TransactionType == RSToolKit.Domain.Entities.MerchantAccount.TransactionType.AuthorizeCapture || detail.TransactionType == RSToolKit.Domain.Entities.MerchantAccount.TransactionType.Capture);
                    if (charge)
                    {
                        runningTotal -= detail.Ammount;
                    }
                    else
                    {
                        runningTotal += detail.Ammount;
                    }
                    table.AddCell(new Phrase(charge ? "Payment" : "Refund", bodyFont));
                    table.AddCell(new Phrase(detail.DateCreated.LocalDateTime.ToString(this._registrant.Form.Culture), bodyFont));
                    table.AddCell(new Phrase(""));
                    table.AddCell(new Phrase((charge ? "-" : "") + Math.Round(detail.Ammount).ToString("c", this._registrant.Form.Culture), bodyFont));
                }
                table.AddCell(new Phrase(""));
                table.AddCell(new Phrase(""));
                table.AddCell(new Phrase("Grand Total:", boldTableFont));
                table.AddCell(new Phrase(Math.Round(this._registrant.TotalOwed).ToString("c", this._registrant.Form.Culture), boldTableFont));

                document.Add(table);

                document.Close();
                binaryData = m_stream.ToArray();
            }

            return File(binaryData, "application/pdf", "invoice.pdf");
        }

        private void SetPageValuesBlank(Page page, Registrant registrant, bool blank = false)
        {
            if (page.AdminOnly)
                return;
            var components = Context.Components.Where(c => c.Panel.PageKey == page.UId && !c.Panel.AdminOnly && c.Panel.Enabled).ToList();
            foreach (var component in components)
            {
                if (component.AdminOnly || !component.Enabled || component is FreeText || component is CheckboxItem || component is RadioItem || component is DropdownItem)
                    continue;
                registrant.SetData(component.UId.ToString(), null, ignoreRequired: true);
                /*
                var data = registrant.Data.FirstOrDefault(d => d.VariableUId == component.UId);
                if (data == null)
                {
                    data = new RegistrantData()
                    {
                        VariableUId = component.UId,
                        Component = component
                    };
                    registrant.Data.Add(data);
                }
                data.Value = null;
                //*/
            }
            Context.SaveChanges();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                if (this._registrant != null)
                    this._UpdateRegistrantInTable(this._registrant.UId);
            base.Dispose(disposing);
        }

        /// <summary>
        /// Updates the registrant in the loaded table information.
        /// </summary>
        /// <param name="id">The unique identifier of the registrant.</param>
        protected void _UpdateRegistrantInTable(Guid id)
        {
            ThreadPool.QueueUserWorkItem(FormReportController.UpdateTableRegistrantCallback, id);
        }
    
    }

    public class EmailSendToken
    {
        public DateTimeOffset Creation { get; set; }
        public string Email { get; set; }
        public Guid FormKey { get; set; }
        public Guid Token { get; set; }

        public EmailSendToken()
        {
            Creation = DateTimeOffset.UtcNow;
        }

        public static EmailSendToken New(Guid Form, string Email)
        {
            var token = new EmailSendToken();
            token.FormKey = Form;
            token.Email = Email;
            token.Token = Guid.NewGuid();
            return token;
        }
    }
}