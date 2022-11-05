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
    public class RegistrationsController : AuthApiController
    {
        [ApiTokenUserAuthorization]
        public IHttpActionResult Get(long id)
        {
            using (var context = new EFDbContext())
            using (var repository = new FormsRepository(context, User, Principal))
            {
                var form = repository.Search<Form>(f => f.SortingId == id).FirstOrDefault();
                if (form == null)
                    return NotFound();
                var datas = new ApiData();
                var registrants = new List<JRegistrant>();
                var regType = form.Status == FormStatus.Developement ? RegistrationType.Test : RegistrationType.Live;
                foreach (var registrant in form.Registrants.Where(r => r.Type == regType && r.Status != RegistrationStatus.Deleted).ToList())
                {
                    var t_reg = ToRegistrationInfo(registrant);
                    registrants.Add(t_reg);
                }
                datas.data = registrants;
                datas.success = true;
                datas.message = "Success";
                return Ok(datas);
            }
        }

        [ApiTokenUserAuthorization]
        public IHttpActionResult Post([FromUri]long eventid, [FromBody]IEnumerable<JTableFilter> filters)
        {
            filters = filters ?? new List<JTableFilter>();
            using (var context = new EFDbContext())
            using (var repository = new FormsRepository(context, User, Principal))
            {
                var form = repository.Search<Form>(f => f.SortingId == eventid).FirstOrDefault();
                if (form == null)
                    return NotFound();
                var datas = new ApiData();
                var registrants = new List<JRegistrant>();
                var regType = form.Status == FormStatus.Developement ? RegistrationType.Test : RegistrationType.Live;
                foreach (var registrant in FilterEngine.Filter(form.Registrants.Where(r => r.Type == regType && r.Status != RegistrationStatus.Deleted).ToList(), filters))
                {
                    var t_reg = ToRegistrationInfo(registrant);
                    registrants.Add(t_reg);
                }
                datas.data = registrants;
                datas.success = true;
                datas.message = "Success";
                return Ok(datas);
            }
        }
    }
}
