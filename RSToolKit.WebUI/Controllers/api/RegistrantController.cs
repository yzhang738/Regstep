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
using RSToolKit.Domain.Engines;
using RSToolKit.Domain.JItems;

namespace RSToolKit.WebUI.Controllers.API
{
    public class RegistrantController : AuthApiController
    {
        [ApiTokenAttendeeAuthorization]
        public IHttpActionResult Get(long id = -1)
        {
            if (id == -1 && User != null)
                return BadRequest("The parameter id must be supplied if logged in as a user.");
            if (User == null)
                id = Registrant.SortingId;
            using (var context = new EFDbContext())
            using (var repository = new FormsRepository(context, User, Principal))
            {
                var registrant = repository.Search<Registrant>(r => r.SortingId == id).FirstOrDefault();
                if (registrant == null)
                    return NotFound();
                var data = new ApiData() { success = true, message = "Registrant found for an authorized user." };
                data.data = ToRegistrationInfo(registrant);
                return Ok(data);
            }
        }

        [ApiTokenAttendeeAuthorization]
        public IHttpActionResult Put(JRegistrant info)
        {
            if (User == null && Registrant.SortingId != info.id)
                return BadRequest("You are not authorized to edit this registrant.");
            using (var context = new EFDbContext())
            using (var repository = new FormsRepository(context, User, Principal))
            {
                var registrant = repository.Search<Registrant>(r => r.SortingId == info.id).FirstOrDefault();
                if (registrant == null)
                    return NotFound();
                registrant.Status = ToStatus(info.status);
                registrant.RSVP = info.rsvp;
                registrant.DateModified = DateTimeOffset.Now;
                registrant.ModifiedBy = User == null ? Guid.Empty : User.UId;
                registrant.Attended = info.attended;
                var success = true;
                string message = null;
                foreach (var data in info.data)
                {
                    RegistrantData m_data;
                    if (data.componentId == 0L)
                    {
                        success = false;
                        message = "No component id supplied for registrant data.";
                        break;
                    }
                    IComponent component;
                    if (data.id < 1)
                    {
                        m_data = new RegistrantData();
                        component = repository.Search<IComponent>(c => c.SortingId == data.componentId).FirstOrDefault();
                        if (component == null)
                        {
                            success = false;
                            message = "Invlalid component id for registrant data.";
                            break;
                        }
                    }
                    else
                    {
                        m_data = registrant.Data.FirstOrDefault(d => d.SortingId == data.id);
                        component = m_data.Component;
                    }
                    if (m_data == null)
                    {
                        success = false;
                        message = "Unknown error.";
                        break;
                    }
                    var status = registrant.SetData(component.UId.ToString(), data.raw);
                    if (!status.Success)
                    {
                        success = false;
                        message = String.Join(", ", status.Errors.Select(e => e.Message).ToArray());
                    }
                }
                if (!success)
                    return Ok(new ApiData() { success = success, message = message });
                repository.Commit();
                return Ok(new ApiData() { success = true, message = "finished", data = ToRegistrationInfo(registrant) });
            }
        }

        protected RegistrationStatus ToStatus(string status)
        {
            switch (status.ToLower())
            {
                case "incomplete":
                    return RegistrationStatus.Incomplete;
                case "submitted":
                    return RegistrationStatus.Submitted;
                case "cancelled":
                    return RegistrationStatus.Canceled;
                case "cancelled by company":
                    return RegistrationStatus.CanceledByCompany;
                case "cancelled by administrator":
                    return RegistrationStatus.CanceledByAdministrator;
                case "mark for deletion":
                    return RegistrationStatus.Deleted;
                default:
                    return RegistrationStatus.Incomplete;
            }
        }
    }
}
