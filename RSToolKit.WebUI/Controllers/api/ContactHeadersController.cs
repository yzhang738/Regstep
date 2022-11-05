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
using RSToolKit.Domain.Entities;

namespace RSToolKit.WebUI.Controllers.API
{
    public class ContactHeadersController : AuthApiController
    {
        [ApiTokenUserAuthorization]
        public IHttpActionResult Get()
        {
            using (var context = new EFDbContext())
            using (var repository = new FormsRepository(context, User, Principal))
            {
                var company = repository.Search<Company>(c => c.UId == User.CompanyKey).FirstOrDefault();
                var contactHeaders = company.ContactHeaders;
                var r_contactHeaders = new List<JContactHeader>();
                foreach (var contactHeader in contactHeaders)
                {
                    var r_contactHeader = new JContactHeader() { id = contactHeader.SortingId, type = contactHeader.Descriminator.GetStringValue(), label = contactHeader.Name, options = contactHeader.DescriminatorOptions };
                    var t_selections = new List<JContactHeaderValueSelection>();
                    foreach (var value in contactHeader.Values)
                        t_selections.Add(new JContactHeaderValueSelection() { id = value.Id, value = value.Value });
                    r_contactHeader.selections = t_selections;
                    if (t_selections.Count == 0)
                        r_contactHeader.selections = null;
                    r_contactHeaders.Add(r_contactHeader);
                }
                return Ok(new ApiData() { success = true, message = "success", data = r_contactHeaders });
            }
        }
    }

    public class JContactHeader
    {
        public long id { get; set; }
        public string type { get; set; }
        public string label { get; set; }
        public IDictionary<string, string> options { get; set; }
        public IEnumerable<JContactHeaderValueSelection> selections { get; set; }
        
        public JContactHeader()
        {
            id = 0L;
            type = "text";
            label = "";
            options = new Dictionary<string, string>();
            selections = new List<JContactHeaderValueSelection>();
        }
    }

    public class JContactHeaderValueSelection
    {
        public long id { get; set; }
        public string value { get; set; }

        public JContactHeaderValueSelection()
        {
            id = 0L;
            value = "";
        }
    }
}
