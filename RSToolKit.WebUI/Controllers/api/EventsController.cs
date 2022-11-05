using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity;
using RSToolKit.Domain.Entities.Clients;
using RSToolKit.Domain.Data;
using RSToolKit.Domain;
using RSToolKit.Domain.Entities;
using RSToolKit.WebUI.Infrastructure;

namespace RSToolKit.WebUI.Controllers.API
{
    public class EventsController : AuthApiController
    {
        [ApiTokenUserAuthorization]
        public IHttpActionResult Get(bool active = true)
        {
            using (var repository = new FormsRepository())
            {
                repository.User = User;
                repository.Principal = Principal;
                IEnumerable<Form> forms = new List<Form>();
                if (active)
                    forms = repository.Search<Form>(f => f.Status == FormStatus.Open || f.Status == FormStatus.PaymentComplete || f.Status == FormStatus.Complete);
                else
                    forms = repository.Search<Form>(f => f.Status != FormStatus.Archive);
                var datas = new GetEventsData();
                var events = new List<EventInfo>();
                foreach (var form in forms)
                {
                    var regType = form.Status == FormStatus.Developement ? RegistrationType.Test : RegistrationType.Live;
                    decimal sales = 0;
                    decimal collected = 0;
                    decimal outstanding = 0;
                    decimal adjustments = 0;
                    foreach (var registrant in form.Registrants.Where(r => r.Type == RSToolKit.Domain.Entities.RegistrationType.Live && (r.Status == RSToolKit.Domain.Entities.RegistrationStatus.CanceledByCompany || r.Status == RSToolKit.Domain.Entities.RegistrationStatus.Canceled || r.Status == RSToolKit.Domain.Entities.RegistrationStatus.CanceledByAdministrator || r.Status == RSToolKit.Domain.Entities.RegistrationStatus.Submitted)))
                    {
                        sales += registrant.Fees;
                        collected += registrant.Transactions;
                        outstanding += registrant.Fees - registrant.Transactions + registrant.Adjustings;
                        adjustments += registrant.Adjustings;
                    }
                    var data = new EventInfo();
                    data.coordinator = form.CoordinatorName;
                    data.coordinatorEmail = form.CoordinatorEmail;
                    data.coordinatorPhone = form.CoordinatorPhone;
                    data.id = form.SortingId;
                    data.name = form.Name;
                    data.eventStart = form.EventStart;
                    data.eventEnd = form.EventEnd;
                    data.isCancellable = form.Cancelable;
                    data.isEditable = form.Editable;
                    data.registrationClosed = form.Close;
                    data.registrationOpen = form.Open;
                    data.tags = form.Tags.Select(t => t.Name);
                    data.totalActive = form.Registrations();
                    data.totalAdjustments = adjustments;
                    data.totalCancelled = form.Registrants.Count(r => r.Type == regType && (r.Status == RegistrationStatus.Canceled || r.Status == RegistrationStatus.CanceledByAdministrator || r.Status == RegistrationStatus.CanceledByCompany));
                    data.totalCollected = collected;
                    data.totalIncomplete = form.Registrants.Count(r => r.Type == regType && r.Status == RegistrationStatus.Incomplete);
                    data.totalOutstanding = outstanding;
                    data.totalSales = sales;
                    data.type = form.Type != null ? form.Type.Name : "";
                    data.status = form.Status.GetStringValue();
                    events.Add(data);
                }
                datas.data = events;
                datas.success = true;
                datas.message = "Success";
                return Ok(datas);
            }
        }
    }

    public class GetEventsData
    {
        public bool success { get; set; }
        public string message { get; set; }
        public IEnumerable<EventInfo> data { get; set; }

        public GetEventsData()
        {
            success = false;
            message = "Not Initialized.";
            data = new List<EventInfo>();
        }
    }
}
