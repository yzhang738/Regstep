using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using RSToolKit.Domain.Data;
using RSToolKit.Domain.Entities.Email;
using RSToolKit.WebUI.Infrastructure;
using RSToolKit.Domain;
using RSToolKit.Domain.Entities;
using RSToolKit.Domain.Entities;
using RSToolKit.Domain.JItems;
using RSToolKit.Domain.Engines;

namespace RSToolKit.WebUI.Controllers.API
{
    [ApiAuthorize]
    public class Email_SendController : ApiController
    {

        protected EFDbContext Context;
        protected FormsRepository Repository;
        protected bool _inScope;

        public Email_SendController()
        {
            Context = new EFDbContext();
            Repository = new FormsRepository(Context);
            _inScope = true;
        }

        public Email_SendController(EFDbContext context)
        {
            Context = (EFDbContext)context;
            Repository = new FormsRepository(Context);
            _inScope = true;
        }

        public IHttpActionResult Get()
        {
            return Ok();
        }

        [ApiAuthorize(Roles = "EmailBuilders,Cloud Users,Cloud+ Users")]
        public IHttpActionResult Post(api_EmailRequest req)
        {
            var email = Repository.Search<IEmail>(e => e.UId == req.id).FirstOrDefault();
            if (email == null)
                return NotFound();
            if (req.scheduled)
            {
                email.SendTime = null;
                Repository.Commit();
            }
            var count = 0;
            // We need to check if a recipient was provided.
            if (req.recipients != null && req.recipients.Count > 0)
            {
                // A recipient was provided, so we use that for sending emails.
                foreach (var recipient in req.recipients)
                {
                    Guid r_id = Guid.Empty;
                    if (Guid.TryParse(recipient, out r_id))
                    {
                        var person = Repository.Search<IPerson>(p => p.UId == r_id).FirstOrDefault();
                        if (person == null)
                            continue;
                        if (SendEmail(person, email))
                            count++;
                    }
                    else
                    {
                        if (SendEmail(recipient, email, req.values, req.regId))
                            count++;
                    }
                }
            }
            else
            {
                // No recipients supplied.  We either must be attached to a form, or the email needs to contain a list and no form.
                if (email.Form == null)
                {
                    if (email.EmailList != null)
                    {
                        foreach (var person in email.EmailList.GetAllEmails(Repository))
                        {
                            if (SendEmail(person, email))
                                count++;
                        }
                    }
                }
                else
                {
                    var regType =  (email.Form.Status == FormStatus.Developement || email.Form.Status == FormStatus.Ready) ? RegistrationType.Test : RegistrationType.Live;
                    // Now we need to check to see what kind of email it is.
                    if (email.EmailType == EmailType.Attending)
                    {
                        foreach (var registrant in email.Form.Registrants.Where(r => r.RSVP && r.Status == RegistrationStatus.Submitted && r.Type == regType))
                        {
                            if (SendEmail(registrant, email))
                                count++;
                        }
                    }
                    else if (email.EmailType == EmailType.NotAttending)
                    {
                        foreach (var registrant in email.Form.Registrants.Where(r => !r.RSVP && r.Status == RegistrationStatus.Submitted && r.Type == regType))
                        {
                            if (SendEmail(registrant, email))
                                count++;
                        }
                    }
                    else if (email.EmailType == EmailType.Registered)
                    {
                        foreach (var registrant in email.Form.Registrants.Where(r => r.Status == RegistrationStatus.Submitted && r.Type == regType))
                        {
                            if (SendEmail(registrant, email))
                                count++;
                        }
                    }
                    else if (email.EmailType == EmailType.NotRegistered)
                    {
                        #region Not Registered
                        var t_emailsSent = new List<string>();
                        if (email.EmailList == null && email.Form.EmailList == null)
                        {
                            // No email list or form email list so we just send to incompletes.
                            foreach (var registrant in email.Form.Registrants.Where(r => r.Status == RegistrationStatus.Incomplete && r.Type == regType))
                            {
                                if (SendEmail(registrant, email))
                                {
                                    t_emailsSent.Add(registrant.Email.ToLower());
                                    count++;
                                }
                            }
                        }
                        else if (email.EmailList == null && email.Form.EmailList != null)
                        {
                            // There is a from email list so we use that to compare.  We still send emails to incompletes as well.
                            foreach (var registrant in email.Form.Registrants.Where(r => r.Status == RegistrationStatus.Incomplete && r.Type == regType))
                            {
                                if (SendEmail(registrant, email))
                                {
                                    t_emailsSent.Add(registrant.Email.ToLower());
                                    count++;
                                }
                            }
                            // Now we find any people on the email list that are not in the registrant list.
                            foreach (var person in email.Form.EmailList.GetAllEmails(Repository))
                            {
                                if (!t_emailsSent.Contains(person.Email.ToLower()) && email.Form.Registrants.FirstOrDefault(r => r.Email == person.Email && r.Type == regType) == null)
                                    if (SendEmail(person, email))
                                        count++;
                            }
                        }
                        else if (email.EmailList != null)
                        {
                            // There is a from email list so we use that to compare.  We still send emails to incompletes as well.
                            foreach (var registrant in email.Form.Registrants.Where(r => r.Status == RegistrationStatus.Incomplete && r.Type == regType))
                            {
                                if (SendEmail(registrant, email))
                                {
                                    t_emailsSent.Add(registrant.Email.ToLower());
                                    count++;
                                }
                            }
                            // No we send emails based on the email list alone.
                            foreach (var person in email.EmailList.GetAllEmails(Repository))
                            {
                                if (!t_emailsSent.Contains(person.Email.ToLower()) && email.Form.Registrants.FirstOrDefault(r => r.Email == person.Email && r.Type == regType) == null)
                                    if (SendEmail(person, email))
                                        count++;
                            }
                        }
                        #endregion
                    }
                    else if (email.EmailType == EmailType.Incomplete)
                    {
                        foreach (var registrant in email.Form.Registrants.Where(r => r.Status == RegistrationStatus.Incomplete && r.Type == regType))
                        {
                            if (SendEmail(registrant, email))
                            {
                                count++;
                            }
                        }
                    }
                    else if (email.EmailType == EmailType.Invitation && email.EmailList != null)
                    {
                        #region Invitation
                        var t_emailsSent = new List<string>();
                        var t_people = email.EmailList.GetAllEmails(Repository);
                        var t_allowableEmails = new List<string>();
                        t_people.OfType<Contact>().ToList().ForEach(c =>
                        {
                            t_allowableEmails.Add(c.Email.ToLower());
                            foreach (var e_alternateEmail in c.Data.Where(d => d.Header.Descriminator == ContactDataType.Email && d.Value != null))
                                t_allowableEmails.Add(e_alternateEmail.Value.ToLower());
                        });
                        // There is a email list so we use that to compare.  We still send emails to incompletes as well.
                        foreach (var registrant in email.Form.Registrants.Where(r => r.Status == RegistrationStatus.Incomplete && r.Type == regType && t_allowableEmails.Contains(r.Email.ToLower(), StringComparer.InvariantCultureIgnoreCase)))
                        {    
                            if (SendEmail(registrant, email))
                            {
                                t_emailsSent.Add(registrant.Email.ToLower());
                                count++;
                            }
                        }
                        // We get all emails of registrants that are already registered.
                        var t_registeredEmails = email.Form.Registrants.Where(r => r.Status == RegistrationStatus.Submitted && r.Type == regType).Select(r => r.Email.ToLower());
                        // Now we send emails based on the email list alone.
                        foreach (var person in t_people)
                        {
                            var t_alreadySent = false;
                            var t_allEmails = new List<string>();
                            t_allEmails.Add(person.Email.ToLower());
                            var t_contact = person as Contact;
                            if (t_contact == null)
                                continue;
                            t_contact.Data.Where(d => d.Header.Descriminator == ContactDataType.Email && d.Value != null).ToList().ForEach(e =>
                            {
                                t_allEmails.Add(e.Value.ToLower());
                            });
                            t_allEmails.ForEach(e =>
                            {
                                if (t_emailsSent.Contains(e) || t_registeredEmails.Contains(e))
                                    t_alreadySent = true;
                                
                            });
                            if (t_alreadySent)
                                continue;
                            if (SendEmail(person, email))
                                count++;
                        }
                        #endregion
                    }
                    else if (email.EmailType == EmailType.BillMeInvoice)
                    {
                        foreach (var registrant in email.Form.Registrants.Where(r => r.Status == RegistrationStatus.Submitted && r.TotalOwed > 0 && r.Type == regType))
                        {
                            if (SendEmail(registrant, email))
                                count++;
                        }
                    }
                    else if (email.EmailType == EmailType.CreditCardReciept)
                    {
                        foreach (var registrant in email.Form.Registrants.Where(r => r.Status == RegistrationStatus.Submitted && r.TransactionRequests.Where(tr => tr.Details.Where(d => d.Approved && d.TransactionType == Domain.Entities.MerchantAccount.TransactionType.AuthorizeCapture).Count() > 0).Count() > 0 && r.Type == regType))
                        {
                            if (SendEmail(registrant, email))
                                count++;
                        }
                    }
                    else if (email.EmailType == EmailType.Confirmation)
                    {
                        foreach (var registrant in email.Form.Registrants.Where(r => r.Status == RegistrationStatus.Submitted && r.Type == regType))
                        {
                            if (SendEmail(registrant, email))
                                count++;
                        }
                    }
                    else if (email.EmailList != null)
                    {
                        foreach (var person in email.EmailList.GetAllEmails(Repository))
                        {
                            if (SendEmail(person, email))
                                count++;
                        }
                    }
                }
            }
            return Ok(new api_EmailResponse() { SuccessMessage = count + " email" + (count > 1 ? "s where" : " was") + " sent.", Sends = count });
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
            if (!SmtpServer.CanSend(email, person, Repository))
                return false;
            if (person is Registrant)
            {
                registrant = (Registrant)person;
                var form = registrant.Form;
                if (node == null)
                    node = form;
                contact = Repository.Search<Contact>(c => c.CompanyKey == form.CompanyKey && c.Name == registrant.Email).FirstOrDefault();
                if (contact == null)
                {
                    contact = Repository.Search<Contact>(c => c.Data.Where(d => d.Header.Descriminator == ContactDataType.Email).Select(d => d.Value.ToLower()).ToList().Contains(registrant.Email)).FirstOrDefault();
                }
                parser = new RSParser(registrant, Repository, form, null, contact);
            }
            else if (person is Contact)
            {
                contact = (Contact)person;
                var form = node as Form;
                if (form != null)
                {
                    var emails = contact.GetEmails().ToArray();
                    registrant = form.Registrants.FirstOrDefault(r => r.Email.ToLower().In(emails));
                }
                parser = new RSParser(registrant, Repository, email.Form, email.EmailCampaign, contact);
            }
            if (contact == null && registrant == null)
                return false;
            if (parser == null)
                return false;
            var sending = true;
            var work = LogicEngine.RunLogic(email, Repository, registrant: person as Registrant, contact: person as Contact);
            //var work = email.RunLogic(person, true, Repository).ToList();
            if (work.Count() > 0)
                sending = work.First().Command == JLogicWork.SendEmail;
            if (!sending)
                return false;
            var message = email.GenerateEmail(parser);
            smtpServer.SendEmail(message, Repository, email.Company, node, person, header, email.EmailType.GetStringValue(), email.UId, addContact: contact);
            return true;
        }

        protected bool SendEmail(string recipient, IEmail email, Dictionary<string, string> values, long regKey = -1)
        {
            // Now we grab the smtpServer.
            SmtpServer smtpServer = null;
            var registrant = Repository.Search<Registrant>(r => r.SortingId == regKey).FirstOrDefault();
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
            RSParser parser = new RSParser(registrant, Repository, email.Form, email.EmailCampaign);
            var message = email.GenerateEmail(parser);
            foreach (var kvp in values)
            {
                message.Html = message.Html.Replace("__" + kvp.Key + "__", kvp.Value);
                message.Text = message.Text.Replace("__" + kvp.Key + "__", kvp.Value);
            }
            smtpServer.SendEmail(message, Repository, email.Company, node, recipient, header, email.EmailType.GetStringValue());
            return true;
        }

        protected override void Dispose(bool disposing)
        {
            if (_inScope)
            {
                Context.Dispose();
                Repository.Dispose();
            }
            base.Dispose(disposing);
        }
    }

    public class api_EmailRequest
    {
        public Guid id { get; set; }
        public bool scheduled { get; set; }
        public List<string> recipients { get; set; }
        public Dictionary<string, string> values { get; set; }
        public long regId { get; set; }

        public api_EmailRequest()
        {
            recipients = new List<string>();
            scheduled = false;
            values = new Dictionary<string, string>();
            regId = -1;
        }
    }

    public class api_EmailResponse
    {
        public bool Success { get; set; }
        public string SuccessMessage { get; set; }
        public string SuccessMessageTitle { get; set; }
        public string Message { get; set; }
        public int Sends { get; set; }
        public string Location { get; set; }

        public api_EmailResponse()
        {
            Success = true;
            SuccessMessage = "";
            SuccessMessageTitle = "Email Sends";
            Message = "Inernal Server Error";
            Sends = 0;
            Location = "showmessage";
        }
    }

}
