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
    public class ContactController : AuthApiController
    {
        [ApiTokenUserAuthorization]
        public IHttpActionResult Get(long id)
        {
            using (var context = new EFDbContext())
            using (var repository = new FormsRepository(context, User, Principal))
            {
                var company = repository.Search<Company>(c => c.UId == User.CompanyKey).FirstOrDefault();
                var a_contact = company.Contacts.FirstOrDefault(c => c.SortingId == id);
                if (a_contact == null)
                    return Ok(new ApiData() { success = false, message = "Contact not found." });
                var r_contact = new JContact();
                r_contact.email = a_contact.Email;
                var r_datas = new List<JContactData>();
                foreach (var data in a_contact.Data)
                {
                    var r_data = new JContactData() { id = data.SortingId, headerId = data.Header.SortingId, raw = data.Value, value = data.GetFormattedValue(), type = data.Header.Descriminator.GetStringValue(), label = data.Header.Name };
                    r_datas.Add(r_data);
                }
                r_contact.data = r_datas;
                return Ok(new ApiData() { success = true, message = "success", data = r_contact });
            }
        }

        [ApiTokenUserAuthorization]
        public IHttpActionResult Put(JContact contact)
        {
            using (var context = new EFDbContext())
            using (var repository = new FormsRepository(context, User, Principal))
            {
                var company = repository.Search<Company>(c => c.UId == User.CompanyKey).FirstOrDefault();
                var a_contact = company.Contacts.FirstOrDefault(c => c.SortingId == contact.id);
                if (a_contact == null)
                    return Post(contact);
                // Check to see if email is still good.
                if (EmailInUse(repository, contact.id, contact.email))
                    return Ok(new ApiData() { success = false, message = "Email (" + contact.email + ") is already being use.", data = null });
                a_contact.Email = contact.email;

                foreach (var data in contact.data)
                {
                    if (data.type == "Email")
                        if (EmailInUse(repository, contact.id, data.raw))
                            return Ok(new ApiData() { success = false, message = "Email (" + data.raw + ") is already in use." });
                    var contactData = a_contact.Data.FirstOrDefault(d => d.SortingId == data.id);
                    if (contactData == null)
                    {
                        contactData = new ContactData();
                        var header = repository.Search<ContactHeader>(h => h.SortingId == data.headerId).FirstOrDefault();
                        if (header == null)
                            continue;
                        contactData.Header = header;
                        a_contact.Data.Add(contactData);
                    }
                    contactData.SetValue(data.raw);
                }
                repository.Commit();

                var r_contact = new JContact();
                r_contact.email = a_contact.Email;
                var r_datas = new List<JContactData>();
                foreach (var data in a_contact.Data)
                {
                    var r_data = new JContactData() { id = data.SortingId, headerId = data.Header.SortingId, raw = data.Value, value = data.GetFormattedValue(), type = data.Header.Descriminator.GetStringValue(), label = data.Header.Name };
                    r_datas.Add(r_data);
                }
                r_contact.data = r_datas;
                return Ok(new ApiData() { success = true, message = "success", data = r_contact });
            }
        }

        [ApiTokenUserAuthorization]
        public IHttpActionResult Post(JContact contact)
        {
            using (var context = new EFDbContext())
            using (var repository = new FormsRepository(context, User, Principal))
            {
                var company = repository.Search<Company>(c => c.UId == User.CompanyKey).FirstOrDefault();
                var a_contact = company.Contacts.FirstOrDefault(c => c.SortingId == contact.id);
                if (a_contact != null)
                    return Ok(new ApiData() { success = false, message = "The contact already exists." });
                company.Contacts.Add(a_contact);
                a_contact = new Contact();
                // Check to see if email is still good.
                if (EmailInUse(repository, contact.id, contact.email))
                    return Ok(new ApiData() { success = false, message = "Email (" + contact.email + ") is already being use.", data = null });
                a_contact.Email = contact.email;

                foreach (var data in contact.data)
                {
                    if (data.type == "Email")
                        if (EmailInUse(repository, contact.id, data.raw))
                            return Ok(new ApiData() { success = false, message = "Email (" + data.raw + ") is already in use." });
                    var contactData = a_contact.Data.FirstOrDefault(d => d.SortingId == data.id);
                    if (contactData == null)
                    {
                        contactData = new ContactData();
                        var header = repository.Search<ContactHeader>(h => h.SortingId == data.headerId).FirstOrDefault();
                        if (header == null)
                            continue;
                        contactData.Header = header;
                        a_contact.Data.Add(contactData);
                    }
                    contactData.SetValue(data.raw);
                }
                repository.Commit();

                var r_contact = new JContact();
                r_contact.email = a_contact.Email;
                var r_datas = new List<JContactData>();
                foreach (var data in a_contact.Data)
                {
                    var r_data = new JContactData() { id = data.SortingId, headerId = data.Header.SortingId, raw = data.Value, value = data.GetFormattedValue(), type = data.Header.Descriminator.GetStringValue(), label = data.Header.Name };
                    r_datas.Add(r_data);
                }
                r_contact.data = r_datas;
                return Ok(new ApiData() { success = true, message = "success", data = r_contact });
            }
        }

        protected bool EmailInUse(FormsRepository repository, long id, string email)
        {
            return repository.Search<Contact>(c => c.SortingId != id && c.Email.ToLower() != email.ToLower() && c.Data.Count(d => d.Header.Descriminator == ContactDataType.Email && d.Value.ToLower() == email.ToLower()) == 0).FirstOrDefault() != null;
        }
    }
}
