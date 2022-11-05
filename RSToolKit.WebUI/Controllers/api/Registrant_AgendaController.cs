using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using RSToolKit.WebUI.Infrastructure;
using RSToolKit.Domain.Data;
using RSToolKit.Domain.Entities;

namespace RSToolKit.WebUI.Controllers.API
{
    public class Registrant_AgendaController : AuthApiController
    {
        [ApiTokenAttendeeAuthorization]
        public IHttpActionResult Get(long id = -1)
        {
            if (id == -1 && User != null)
                return BadRequest("The parameter id must be supplied if logged in as a user.");
            using (var context = new EFDbContext())
            using (var repository = new FormsRepository(context, User, Principal))
            {
                Registrant registrant;
                if (User == null)
                    registrant = repository.Search<Registrant>(r => r.SortingId == Registrant.SortingId).FirstOrDefault();
                else
                    registrant = repository.Search<Registrant>(r => r.SortingId == id).FirstOrDefault();
                if (registrant == null)
                    return NotFound();
                return Ok(new ApiData() { success = true, message = "", data = GetAgenda(registrant) });
            }
        }
    }
}
