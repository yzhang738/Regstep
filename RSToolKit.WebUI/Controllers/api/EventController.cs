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
    public class EventController : AuthApiController
    {
        [ApiTokenUserAuthorization]
        public IHttpActionResult Get(long id)
        {
            using (var repository = new FormsRepository())
            {
                repository.DiscardSecurity();
                var form = repository.Search<Form>(f => f.SortingId == id).FirstOrDefault();
                if (form == null)
                    return NotFound();
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
                var datas = new GetEventData();
                var data = new EventInfo();
                datas.success = true;
                datas.message = "Success";
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
                datas.data = data;
                datas.success = true;
                datas.message = "Success";
                return Ok(datas);
            }
        }
    }

    public class EventInfo
    {
        public long id { get; set; }
        public string name { get; set; }
        public DateTimeOffset? eventStart { get; set; }
        public DateTimeOffset? eventEnd { get; set; }
        public DateTimeOffset registrationOpen { get; set; }
        public DateTimeOffset registrationClosed { get; set; }
        public bool isEditable { get; set; }
        public bool isCancellable { get; set; }
        public double latitude { get; set; }
        public double longitude { get; set; }
        public int totalActive { get; set; }
        public int totalCancelled { get; set; }
        public int totalIncomplete { get; set; }
        public string coordinator { get; set; }
        public string coordinatorEmail { get; set; }
        public string coordinatorPhone { get; set; }
        public IEnumerable<string> tags { get; set; }
        public string type { get; set; }
        public decimal totalSales { get; set; }
        public decimal totalCollected { get; set; }
        public decimal totalAdjustments { get; set; }
        public decimal totalOutstanding { get; set; }
        public string status { get; set; }

        public EventInfo()
        {
            id = 0L;
            name = coordinator = coordinatorEmail = coordinatorPhone = type = status = "";
            eventStart = eventEnd = null;
            registrationClosed = registrationOpen = DateTimeOffset.MinValue;
            latitude = longitude = 0L;
            totalActive = totalCancelled = totalIncomplete = -1;
            totalSales = totalCollected = totalAdjustments = totalOutstanding = -1m;
            tags = new string[0];
        }
    }

    public class GetEventData
    {
        public bool success { get; set; }
        public string message { get; set; }
        public EventInfo data { get; set; }

        public GetEventData()
        {
            success = false;
            message = "Not Initialized";
            data = new EventInfo();
        }
    }
}
