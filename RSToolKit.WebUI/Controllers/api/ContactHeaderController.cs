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
    public class ContactHeaderController : AuthApiController
    {
        [ApiTokenUserAuthorization]
        public IHttpActionResult Get(long id)
        {
            using (var context = new EFDbContext())
            using (var repository = new FormsRepository(context, User, Principal))
            {
                var company = repository.Search<Company>(c => c.UId == User.CompanyKey).FirstOrDefault();
                var contactHeader = company.ContactHeaders.FirstOrDefault(h => h.SortingId == id);
                var r_contactHeader = new JContactHeader() { id = contactHeader.SortingId, type = contactHeader.Descriminator.GetStringValue(), label = contactHeader.Name, options = contactHeader.DescriminatorOptions };
                var t_selections = new List<JContactHeaderValueSelection>();
                foreach (var value in contactHeader.Values)
                    t_selections.Add(new JContactHeaderValueSelection() { id = value.Id, value = value.Value });
                r_contactHeader.selections = t_selections;
                if (t_selections.Count == 0)
                    r_contactHeader.selections = null;
                return Ok(new ApiData() { success = true, message = "success", data = r_contactHeader });
            }
        }

        [ApiTokenUserAuthorization]
        public IHttpActionResult Post(JContactHeader header)
        {
            using (var context = new EFDbContext())
            using (var repository = new FormsRepository(context, User, Principal))
            {
                var company = repository.Search<Company>(c => c.UId == User.CompanyKey).FirstOrDefault();
                var contactHeader = repository.Search<ContactHeader>(c => c.SortingId == header.id).FirstOrDefault();
                if (contactHeader != null)
                    return Ok(new ApiData() { success = false, message = "The header already exists. Use PUT method" });
                contactHeader = new ContactHeader();
                contactHeader.Descriminator = ContactHeaderType(header.type);
                contactHeader.DescriminatorOptions = header.options as Dictionary<string, string>;
                contactHeader.Name = header.label;
                contactHeader.Values.Clear();
                foreach (var value in header.selections)
                    contactHeader.Values.Add(new ContactHeaderSelectionValue() { Value = value.value, Id = value.id });
                company.ContactHeaders.Add(contactHeader);
                repository.Commit();
                var r_contactHeader = new JContactHeader() { id = contactHeader.SortingId, type = contactHeader.Descriminator.GetStringValue(), label = contactHeader.Name, options = contactHeader.DescriminatorOptions };
                if (contactHeader.Descriminator == ContactDataType.Email)
                    r_contactHeader.type = "email";
                else if (contactHeader.Descriminator == ContactDataType.SingleSelection)
                    r_contactHeader.type = "singleSelection";
                var t_selections = new List<JContactHeaderValueSelection>();
                foreach (var value in contactHeader.Values)
                    t_selections.Add(new JContactHeaderValueSelection() { id = value.Id, value = value.Value });
                r_contactHeader.selections = t_selections;
                if (t_selections.Count == 0)
                    r_contactHeader.selections = null;
                return Ok(new ApiData() { success = true, message = "success", data = r_contactHeader });
            }
        }

        [ApiTokenUserAuthorization]
        public IHttpActionResult Put(JContactHeader header)
        {
            using (var context = new EFDbContext())
            using (var repository = new FormsRepository(context, User, Principal))
            {
                var company = repository.Search<Company>(c => c.UId == User.CompanyKey).FirstOrDefault();
                var contactHeader = repository.Search<ContactHeader>(c => c.SortingId == header.id).FirstOrDefault();
                if (contactHeader == null)
                    return Post(header);
                contactHeader = new ContactHeader();
                contactHeader.Descriminator = ContactHeaderType(header.type);
                contactHeader.DescriminatorOptions = header.options as Dictionary<string, string>;
                contactHeader.Name = header.label;
                contactHeader.Values.Clear();
                foreach (var value in header.selections)
                    contactHeader.Values.Add(new ContactHeaderSelectionValue() { Value = value.value, Id = value.id });
                company.ContactHeaders.Add(contactHeader);
                repository.Commit();
                var r_contactHeader = new JContactHeader() { id = contactHeader.SortingId, type = contactHeader.Descriminator.GetStringValue(), label = contactHeader.Name, options = contactHeader.DescriminatorOptions };
                var t_selections = new List<JContactHeaderValueSelection>();
                foreach (var value in contactHeader.Values)
                    t_selections.Add(new JContactHeaderValueSelection() { id = value.Id, value = value.Value });
                r_contactHeader.selections = t_selections;
                if (t_selections.Count == 0)
                    r_contactHeader.selections = null;
                return Ok(new ApiData() { success = true, message = "success", data = r_contactHeader });
            }
        }

        protected ContactDataType ContactHeaderType(string type)
        {
            switch (type)
            {
                case "email":
                    return ContactDataType.Email;
                case "date":
                    return ContactDataType.Date;
                case "datetime":
                    return ContactDataType.DateTime;
                case "number":
                    return ContactDataType.Number;
                case "money":
                    return ContactDataType.Money;
                case "text":
                    return ContactDataType.Text;
                case "secure":
                    return ContactDataType.SecureText;
                case "Integer":
                    return ContactDataType.Integer;
                case "time":
                    return ContactDataType.Time;
                case "multipleSelection":
                    return ContactDataType.MultipleSelection;
                case "singelSelection":
                    return ContactDataType.SingleSelection;
                default:
                    return ContactDataType.Text;
            }
        }
    }
}
