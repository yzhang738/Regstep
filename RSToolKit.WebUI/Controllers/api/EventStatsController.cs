using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using RSToolKit.WebUI.Infrastructure;
using RSToolKit.Domain.Entities;
using RSToolKit.Domain.Data;
using RSToolKit.Domain;

namespace RSToolKit.WebUI.Controllers.API
{
    public class EventStatsController : AuthApiController
    {
        [ApiTokenUserAuthorization]
        public IHttpActionResult Get(long id)
        {
            using (var repository = new FormsRepository())
            {
                repository.User = User;
                repository.Principal = Principal;
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
                var datas = new GetEventStatsData();
                var data = new EventStats();
                datas.success = true;
                datas.message = "Success";
                data.totalCancelled = form.Registrants.Count(r => r.Type == regType && (r.Status == RegistrationStatus.Canceled || r.Status == RegistrationStatus.CanceledByAdministrator || r.Status == RegistrationStatus.CanceledByCompany));
                data.registered = form.Registrations();
                data.cancelledByAdministrator = form.Registrants.Count(r => r.Type == regType && r.Status == RegistrationStatus.CanceledByAdministrator);
                data.cancelledByCompany = form.Registrants.Count(r => r.Type == regType && r.Status == RegistrationStatus.CanceledByCompany);
                data.cancelledByRegistrant = form.Registrants.Count(r => r.Type == regType && r.Status == RegistrationStatus.Canceled);
                data.incompletes = form.Registrants.Count(r => r.Type == regType && r.Status == RegistrationStatus.Incomplete);
                data.attended = form.Registrants.Count(r => r.Type == regType && r.Attended);
                datas.data = data;
                datas.success = true;
                datas.message = "Success";
                return Ok(datas);
            }
        }
    }

    public class GetEventStatsData
    {
        public bool success { get; set; }
        public string message { get; set; }
        public EventStats data { get; set; }

        public GetEventStatsData()
        {
            success = false;
            message = "Not Initiated";
            data = null;
        }
    }

    public class EventStats
    {
        public int registered { get; set; }
        public int totalCancelled { get; set; }
        public int cancelledByRegistrant { get; set; }
        public int cancelledByCompany { get; set; }
        public int cancelledByAdministrator { get; set; }
        public int attended { get; set; }
        public int incompletes { get; set; }

        public EventStats()
        {
            registered = totalCancelled = cancelledByAdministrator = cancelledByCompany = cancelledByRegistrant = attended = incompletes = 0;
        }
    }
}
