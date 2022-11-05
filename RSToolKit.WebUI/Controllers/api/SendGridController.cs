using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Newtonsoft.Json;
using RSToolKit.WebUI.Models;
using RSToolKit.Domain.Data;
using System.Threading.Tasks;
using System.Net.Mail;
using System.IO;
using System.Web.Hosting;
using RSToolKit.Domain.Entities.Email;
using RSToolKit.Domain;

namespace RSToolKit.WebUI.Controllers.API
{
    [AllowAnonymous]
    public class SendGridController : ApiController
    {
        public async Task Post()
        {
            try
            {
                using (var Repository = new FormsRepository())
                {
                    var rawJson = await Request.Content.ReadAsStringAsync();
                    var events = JsonConvert.DeserializeObject<List<SendGridEvent>>(rawJson);
                    foreach (var e in events)
                    {
                        if (e.sg_event.ToLower() == "clicked")
                            continue;
                        var emailEvent = new EmailEvent() { Date = UnixTimeStampToDateTime(e.timestamp), Email = e.email };
                        emailEvent.Response = e.response;
                        switch (e.sg_event.ToLower())
                        {
                            case "processed":
                                emailEvent.Event = "Sending";
                                emailEvent.Details = "Awaiting delivery results.";
                                break;
                            case "deferred":
                                emailEvent.Event = "Sending";
                                emailEvent.Details = "Delivery deferred.";
                                emailEvent.Notes = "Recipient email server is temporarily unable to accept delivery request. Attempts made: " + e.attempt;
                                break;
                            case "delivered":
                                emailEvent.Event = "Delivered";
                                emailEvent.Details = "Unconfirmed delivery.";
                                emailEvent.Notes = "Email was accepted by the recipient's email server. Delivery to recipient's inbox cannot be confirmed unless recipient opens the email.";
                                break;
                            case "open":
                                emailEvent.Event = "Opened";
                                emailEvent.Details = "Email was opened by the recipient.";
                                break;
                            case "bounce":
                                emailEvent.Notes = e.reason;
                                switch (e.type.ToLower())
                                {
                                    case "bounce":
                                        emailEvent.Event = "Permanent Bounce";
                                        emailEvent.Details = "Email bounced. Delivery will not be attempted for email address again.";
                                        break;
                                    case "blocked":
                                    default:
                                        emailEvent.Event = "Temporary Bounce";
                                        emailEvent.Details = "Email bounced. Delivery for email address will be attempted on any subsequent sends.";
                                        break;
                                }
                                break;
                            case "spamreport":
                                emailEvent.Event = "Spam Report";
                                emailEvent.Details = "Recipient has reported this email as SPAM.";
                                emailEvent.Notes = "Delivery will not be attempted again.";
                                break;
                            case "dropped":
                                emailEvent.Event = "Dropped";
                                emailEvent.Details = "Email delivery was not attempted.";
                                switch (e.reason.ToLower())
                                {
                                    case "invalid smtpapi header":
                                        emailEvent.Notes = "Unclassified error.";
                                        break;
                                    case "unsubscribed address":
                                        emailEvent.Notes = "Recipient has previously unsubscribed.";
                                        break;
                                    case "bounced address":
                                        emailEvent.Notes = "Delivery to this recipient has previously been recorded as a Permanent Bounce.";
                                        break;
                                    case "spam reporting address":
                                        emailEvent.Notes = "Recipient has previously reported emails as SPAM.";
                                        break;
                                    case "invalid":
                                        emailEvent.Notes = "Email address format is incorrect or known invalid email domain.";
                                        break;
                                    case "recipient list over package quoata":
                                        emailEvent.Notes = "List size exceeds account limits.";
                                        break;
                                }
                                break;
                            default:
                                continue;
                        }
                        Guid id;
                        long lid;
                        EmailSend emailSend = null;
                        if (Guid.TryParse(e.email_id, out id))
                        {
                            emailSend = Repository.Search<EmailSend>(s => s.UId == id).FirstOrDefault();
                        }
                        else if (long.TryParse(e.email_id, out lid))
                        {
                            emailSend = Repository.Search<EmailSend>(s => s.SortingId == lid).FirstOrDefault();
                        }
                        if (emailSend == null)
                            continue;
                        if (emailSend.IgnoreEventsFrom.Contains(e.email.ToLower()))
                            continue;
                        var emails = new List<string>() { emailSend.Recipient.ToLower() };
                        if (emailSend.Registrant != null)
                            emails.Add(emailSend.Registrant.Email.ToLower());
                        if (emailSend.Contact != null)
                            emails.AddRange(emailSend.Contact.GetEmails());
                        if (e.email.ToLower().In(emails.ToArray()))
                            emailSend.EmailEvents.Add(emailEvent);
                    }
                    Repository.Commit();
                }
            }
            catch (Exception e)
            {
            }
        }

        protected static DateTimeOffset UnixTimeStampToDateTime(int unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            var dtDateTime = new DateTimeOffset(1970, 1, 1, 0, 0, 0, 0, new TimeSpan(0));
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp);
            return dtDateTime;
        }

    }

    public class SendGridMap
    {
        public string SendGridEvent { get; set; }
        public string LocalEvent { get; set; }
        public string Notes { get; set; }
        public string Details { get; set; }

        public SendGridMap()
        {
            SendGridEvent = LocalEvent = Details;
            Notes = null;
        }

        public SendGridMap(string sendGridEvent, string localEvent, string details, string notes = null)
        {
            SendGridEvent = sendGridEvent;
            LocalEvent = localEvent;
            Details = details;
            Notes = notes;
        }
    }
}
