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

namespace RSToolKit.WebUI.Controllers.API
{
    [ApiAuthorize]
    public class Email_EmailSendController : ApiController
    {
        protected EFDbContext Context;
        protected FormsRepository Repository;
        protected bool _inScope;

        public Email_EmailSendController()
        {
            Context = new EFDbContext();
            Repository = new FormsRepository(Context);
            _inScope = true;
        }

        public Email_EmailSendController(EFDbContext context)
        {
            Context = (EFDbContext)context;
            Repository = new FormsRepository(Context);
            _inScope = true;
        }

        public JsonNetResult Get(Guid id)
        {
            var send = Repository.Search<EmailSend>(es => es.UId == id).FirstOrDefault();
            if (send == null)
                return new JsonNetResult() { JsonRequestBehavior = System.Web.Mvc.JsonRequestBehavior.AllowGet, Data = new { Success = false, Message = "Invalid emailsend object." } };
            IEnumerable<pr_EmailEventInformation> events = send.EmailEvents.OrderByDescending(e => e.Date).Select(e => new pr_EmailEventInformation() { date = e.Date, emailEvent = e.Event, note = e.Notes });
            return new JsonNetResult() { JsonRequestBehavior = System.Web.Mvc.JsonRequestBehavior.AllowGet, Data = new { Success = true, Data = events }};
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

        private class pr_EmailEventInformation
        {
            public string emailEvent { get; set; }
            public DateTimeOffset date { get; set; }
            public string note { get; set; }

            public pr_EmailEventInformation()
            {
                emailEvent = note = "";
                date = DateTimeOffset.MinValue;
            }
        }

        // Api Request
        public class api_Requset
        {
            public Guid id { get; set; }
        }

    }
}