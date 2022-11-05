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
    public class ContactsController : AuthApiController
    {
        [ApiTokenUserAuthorization]
        public IHttpActionResult Get()
        {
            using (var context = new EFDbContext())
            using (var repository = new FormsRepository(context, User, Principal))
            {
                var company = repository.Search<Company>(c => c.UId == User.CompanyKey).FirstOrDefault();
                var contacts = company.Contacts;
                var r_contacts = new List<JContact>();
                foreach (var contact in contacts)
                {
                    var r_contact = new JContact() { id = contact.SortingId, email = contact.Email };
                    var r_datas = new List<JContactData>();
                    foreach (var data in contact.Data)
                    {
                        var r_data = new JContactData() { id = data.SortingId, headerId = data.Header.SortingId, raw = data.Value, value = data.GetFormattedValue(), type = data.Header.Descriminator.GetStringValue(), label = data.Header.Name };
                        r_datas.Add(r_data);
                    }
                    r_contact.data = r_datas;
                    r_contacts.Add(r_contact);
                }
                return Ok(new ApiData() { success = true, message = "success", data = r_contacts });
            }
        }
    }

    public class JContact
    {
        public long id { get; set; }
        public string email { get; set; }
        public IEnumerable<JContactData> data { get; set; }

        public JContact()
        {
            id = 0L;
            email = "";
            data = new List<JContactData>();
        }
    }

    public class JContactData
    {
        public long id { get; set; }
        public string type { get; set; }
        public string label { get; set; }
        public long headerId { get; set; }
        public string raw { get; set; }
        public string value { get; set; }

        public JContactData()
        {
            id = 0L;
            headerId = 0L;
            label = type = value = "";
            raw = null;
        }
    }
}
