using Microsoft.AspNet.Identity;
using Newtonsoft.Json;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using RSToolKit.Domain;
using RSToolKit.Domain.Data;
using RSToolKit.Domain.Engines;
using RSToolKit.Domain.Entities;
using RSToolKit.Domain.Entities.Clients;
using RSToolKit.Domain.Entities;
using RSToolKit.Domain.Entities.Components;
using RSToolKit.Domain.Entities.Email;
using RSToolKit.Domain.Entities.MerchantAccount;
using RSToolKit.Domain.JItems;
using RSToolKit.Domain.Security;
using RSToolKit.WebUI.Infrastructure;
using RSToolKit.WebUI.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Mvc;
using RSToolKit.Domain.Exceptions;

namespace RSToolKit.WebUI.Controllers
{
    [Authorize(Roles="Super Administrators,Administrators,Cloud Users,Cloud+ Users,Programmers")]
    [RegStepHandleError(typeof(InvalidIdException), "InvalidIdException")]
    public class Cloud2Controller
        : RSController
    {
        //
        // GET: /RSCloud/

        public static DateTimeOffset LastReportCheck = DateTimeOffset.Now;
        public static Dictionary<Guid, ContactUploadListModel> ContactUploadStatus = new Dictionary<Guid, ContactUploadListModel>();
        public static HashSet<PagedReportData> ReportData = new HashSet<PagedReportData>();

        protected static Color excel_HeaderColor = Color.FromArgb(51, 51, 51);
        protected static Color excel_StripeColor = Color.FromArgb(221, 221, 221);

        public static string[] excel_mime = new string[]
        {
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "application/vnd.ms-excel",
            "application/msexcel",
            "application/x-msexcel",
            "application/x-ms-excel",
            "application/x-excel",
            "application/x-dos_ms_excel",
            "application/xls",
            "application/x-xls"
        };

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (LastReportCheck.AddMinutes(30) < DateTimeOffset.Now)
            {
                foreach (var oldData in ReportData.Where(data => data.LastActivity.AddHours(4) < DateTimeOffset.Now))
                    ReportData.Remove(oldData);
                LastReportCheck = DateTimeOffset.Now;
            }
            base.OnActionExecuting(filterContext);
        }

        public static Dictionary<string, string> StaticVariables = new Dictionary<string, string>() { { "email", "Email" }, { "confirmation", "Confirmation" }, { "status", "Status" }, { "dateregistered", "Date Registered" }, { "type", "Type" }, { "audience", "Audience" } };

        public Cloud2Controller(EFDbContext context)
            : base(context)
        {
            iLog.LoggingMethod = "CloudController";
        }

        [HttpGet]
        [CrumbLabel("Dashboard")]
        public ActionResult Index()
        {
            var formRepo = new FormRepository(Context);
            var repRepo = new ReportDataRepository(Context);
            Context.SecuritySettings.SetUser(user, User);
            var model = new CloudModel();
            var reports = repRepo.Search(r => r.CompanyKey == company.SortingId && r.Favorite).ToList();
            var t_advReports = Repository.Search<AdvancedInventoryReport>(r => r.CompanyKey == company.UId && r.Favorite).ToList();
            var advReports = new List<AdvancedInventoryReport>();
            foreach (var rep in t_advReports)
            {
                if (Context.CanAccess(rep, SecurityAccessType.Read))
                    advReports.Add(rep);
            }
            model.LiveForms = formRepo.Search(f => (f.Status == FormStatus.Open || f.Status == FormStatus.Maintenance) && f.CompanyKey == company.UId).ToList();
            var favReports = new List<Tuple<Form, INamedNode>>();
            foreach (var report in reports)
            {
                var t_form = model.LiveForms.FirstOrDefault(f => f.UId == report.TableId);
                if (t_form != null)
                    favReports.Add(new Tuple<Form, INamedNode>(t_form, report));
            }
            foreach (var report in advReports)
            {
                var t_form = model.LiveForms.FirstOrDefault(f => f.UId == report.FormKey);
                if (t_form != null)
                    favReports.Add(new Tuple<Form, INamedNode>(t_form, report));
            }
            model.FavoriteReports = favReports.OrderBy(t => t.Item1.Name).ThenBy(t => t.Item2.Name);
            model.Repository = Repository;
            // Get notifications
            //First lets see if any refunds need processed.
            foreach (var form in model.LiveForms)
            {
                foreach (var reg in form.Registrants.Where(r => r.Type == RegistrationType.Live && (r.Fees + r.Taxes + r.Adjustings - r.Transactions) < 0).ToList())
                {
                    model.Notifications.Add(new Notification() { Message = "Refund due for " + reg.Form.Name + " to " + reg.Email + ".", Url = Url.Action("Get", "Registrant", new { id = reg.SortingId }), ToolTip = reg.TotalOwed.ToString("c", reg.Form.Culture) });
                }
                foreach (var seating in form.Seatings.ToList())
                {
                    if (seating.RawSeats > 0 && seating.HasWaiters)
                        model.Notifications.Add(new Notification() { Message = "Seatings available for: " + seating.Form.Name + " on seating " + seating.Name + ".", ToolTip = "Seatings available.", Url = @Url.Action("CapacityLimit", new { id = seating.UId }) });
                }
            }
            return View(model);
        }
        #region Saved List

        [HttpPut]
        [Authorize(Roles = "Super Administrators,Cloud+ Users,Programmers")]
        [ActionName("SavedList")]
        [JsonValidateAntiForgeryToken]
        public JsonNetResult UpdateSavedList(Guid? id = null, string name = "New Saved List", List<Guid> ids = null)
        {
            ids = ids ?? new List<Guid>();
            var uids = ids.ToArray();
            SavedList savedList = null;
            if (id.HasValue)
                savedList = Repository.Search<SavedList>(s => s.UId == id).FirstOrDefault();
            if (savedList == null)
                savedList = SavedList.New(Repository, company, user, name: name);
            savedList.Contacts = company.Contacts.Where(c => c.UId.In(uids)).ToList();
            Repository.Commit();
            return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = true, Id = savedList.UId.ToString() } };
        }

        [HttpPost]
        [Authorize(Roles = "Super Administrators,Cloud+ Users,Programmers")]
        [ActionName("SavedListContacts")]
        [JsonValidateAntiForgeryToken]
        public JsonNetResult AddSavedListContacts(Guid id, List<Guid> ids)
        {
            var list = Repository.Search<SavedList>(l => l.UId == id).FirstOrDefault();
            if (list == null)
                return new JsonNetResult() { Data = new { Success = true, Message = "Invalid Object" }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            foreach (var c_id in ids)
            {
                var contact = Repository.Search<Contact>(c => c.UId == c_id).FirstOrDefault();
                if (contact == null)
                    continue;
                if (list.Contacts.Where(c => c.UId == contact.UId).Count() != 0)
                    continue;
                list.Contacts.Add(contact);
            }
            Repository.Commit();
            return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = true } };
        }

        [HttpDelete]
        [Authorize(Roles = "Super Administrators,Cloud+ Users,Programmers")]
        [ActionName("SavedListContacts")]
        [JsonValidateAntiForgeryToken]
        public JsonNetResult RemoveSavedListContacts(Guid id, List<Guid> ids)
        {
            var list = Repository.Search<SavedList>(l => l.UId == id).FirstOrDefault();
            if (list == null)
                return new JsonNetResult() { Data = new { Success = true, Message = "Invalid Object" }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            var removed = new List<Guid>();
            foreach (var c_id in ids)
            {
                var contact = list.Contacts.Where(c => c.UId == c_id).FirstOrDefault();
                if (contact == null)
                    continue;
                list.Contacts.Remove(contact);
                removed.Add(contact.UId);
            }
            Repository.Commit();
            return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = true, Removed = removed } };
        }

        [HttpDelete]
        [Authorize(Roles = "Super Administrators,Cloud+ Users,Programmers")]
        [ActionName("SavedList")]
        public JsonNetResult DeleteSavedList(Guid id)
        {
            var savedList = Repository.Search<SavedList>(s => s.UId == id).FirstOrDefault();
            if (savedList == null)
                return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = false, Message = "Invalid Object" } };
            Repository.Remove(savedList);
            Repository.Commit();
            return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = true } };
        }

        #endregion

        #region Contact List

        [HttpGet]
        [Authorize(Roles = "Super Administrators,Cloud+ Users,Programmers")]
        [CrumbLabel("Contact List")]
        [ActionName("Contacts")]
        public ActionResult GetContacts(Guid? id = null)
        {
            ViewBag.SavedList = false;
            IEmailList list = null;
            JTable table = null;
            if (id.HasValue)
                list = Repository.Search<IEmailList>(e => e.UId == id.Value).FirstOrDefault();
            if (Request.IsAjaxRequest())
            {
                if (list != null)
                {
                    table = list.FillJTable();
                    if (list is SavedList)
                        ViewBag.SavedList = "yes";
                }
                else
                {
                    table = new JTable()
                    {
                        Id = "",
                        Name = "Contacts",
                        Parent = company.Name
                    };
                    table.AddHeader(new JTableHeader() { Id = "email", Label = "Email", Editable = false, Type = "text" });
                    foreach (var header in Repository.Search<ContactHeader>(h => h.CompanyKey == company.UId && !h.SavedListKey.HasValue))
                    {
                        var t_type = "text";
                        if (header.Descriminator == ContactDataType.DateTime)
                            t_type = "date";
                        if (header.Descriminator == ContactDataType.Date)
                            t_type = "date";
                        if (header.Descriminator == ContactDataType.Time)
                            t_type = "time";
                        if (header.Descriminator == ContactDataType.Integer || header.Descriminator == ContactDataType.Money)
                            t_type = "number";
                        table.AddHeader(new JTableHeader() { Id = header.UId.ToString(), Label = header.Name, Type = t_type, Editable = true });
                    }
                    foreach (var contact in company.Contacts)
                    {
                        var row = new JTableRow()
                        {
                            Id = contact.UId.ToString()
                        };
                        row.Columns.Add(new JTableColumn() { Id = contact.UId.ToString() + "_email", HeaderId = "email", Value = contact.Email, PrettyValue = "<a href=\"#\" data-action='Contact' data-controller='Cloud' data-options='{\"id\":\"" + contact.UId.ToString() + "\"}'>" + contact.Email + "</a>", Editable = false });
                        foreach (var e_data in contact.Data)
                        {
                            row.Columns.Add(new JTableColumn() { Id = e_data.UId.ToString() + "_" + e_data.HeaderKey.ToString(), HeaderId = e_data.HeaderKey.ToString(), PrettyValue = e_data.PrettyValue, Value = e_data.Value, Editable = true });
                        }
                        table.AddRow(row);
                    }
                    table.Sortings.Add(new JTableSorting() { ActingOn = "email" });
                }
                return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = true, Table = table } };
            }
            else
            {
                if (list == null)
                    id = company.UId;
                return View("ContactList", new Tuple<Guid, string, Guid>(id.Value, "contact list", company.UId));
            }
        }

        [HttpDelete]
        [Authorize(Roles = "Super Administrators,Cloud+ Users,Programmers")]
        [CrumbLabel("Contact List")]
        [ActionName("Contacts")]
        public ActionResult DeleteContacts(List<Guid> ids)
        {
            var a_ids = ids.ToArray();
            var contacts = company.Contacts.Where(c => c.CompanyKey == company.UId && c.UId.In(a_ids)).ToList();
            foreach (var contact in contacts)
            {
                contact.Registrants.ForEach(r => r.ContactKey = null);
                Context.ContactData.RemoveRange(contact.Data);
                foreach (var emailsend in contact.EmailSends)
                    Context.EmailEvents.RemoveRange(emailsend.EmailEvents);
                Context.EmailSends.RemoveRange(contact.EmailSends);
                contact.SavedLists.ForEach(s => s.Contacts.Remove(contact));
            }
            Context.Contacts.RemoveRange(contacts);
            Context.SaveChanges();
            return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = true } };
        }

        // SavedEmailTable
        [HttpPut]
        [Authorize(Roles = "Super Administrators,Cloud+ Users,Programmers")]
        [CrumbLabel("Contact List")]
        [ActionName("SavedEmailTable")]
        public JsonNetResult UpdateSavedEmailTable(JTable table)
        {
            table.Options["type"] = "email table";
            var savedTable = Repository.Search<SavedEmailTable>(s => s.UId.ToString() == table.Id).FirstOrDefault();
            if (savedTable == null)
            {
                savedTable = new SavedEmailTable()
                {
                    UId = Guid.NewGuid(),
                    Name = table.Name,
                    Company = company,
                    Table = table
                };
                Repository.Add(savedTable);
            }
            else
            {
                savedTable.Name = table.Name;
                savedTable.Table = table;
            }
            Repository.Commit();
            return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = true, Id = savedTable.UId.ToString() } };
        }

        // Contacts
        #region Contacts

        [HttpDelete]
        [Authorize(Roles = "Super Administrators,Cloud+ Users,Programmers")]
        [ActionName("Contact")]
        [JsonValidateAntiForgeryToken]
        public JsonNetResult DeleteContact(Guid id)
        {
            var contact = Repository.Search<Contact>(c => c.UId == id).FirstOrDefault();
            if (contact == null)
                return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = false, Message = "Invalid Object" } };
            Repository.Remove(contact);
            Repository.Commit();
            return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = true, UId = contact.UId } };
        }

        [HttpPost]
        [Authorize(Roles = "Super Administrators,Cloud+ Users,Programmers")]
        [ActionName("Contact")]
        [JsonValidateAntiForgeryToken]
        public JsonNetResult NewContact(string Email)
        {
            var currentCount = Repository.Search<Contact>(c => c.CompanyKey == company.UId).Count();
            var clSpotsFree = company.ContactLimit - currentCount;
            if (clSpotsFree < 1)
                return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = false, Message = "Your contact list is full. " } };
            Contact model;
            var email = "[None]";
            try
            {
                var address = new MailAddress(Email);
                email = Email.ToLower();
            }
            catch (Exception e)
            {
                return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = false, Message = "Invalid email.", exception = e.Message } };
            }
            if (Repository.Search<Contact>(c => c.CompanyKey == company.UId && c.Name.ToLower() == email.ToLower(), checkAccess: false).Count() > 0)
                return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = false, Message = "Email in use." } };
            var emailHeaders = Repository.Search<ContactHeader>(h => h.Descriminator == ContactDataType.Email, checkAccess: false).ToList();
            foreach (var emailHeader in emailHeaders)
                if (Repository.Search<ContactData>(d => d.HeaderKey == emailHeader.UId && d.Value.ToLower() == email, checkAccess: false).Count() > 0)
                    return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = false, Message = "Email in use." } };
            model = Contact.New(Repository, company, user, email: email);
            return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = true, Location = Url.Action("Contact", "Cloud", new { id = model.UId }, Request.Url.Scheme) } };
        }

        [HttpGet]
        [Authorize(Roles = "Super Administrators,Cloud+ Users,Programmers")]
        [ActionName("Contact")]
        public ActionResult ViewContact(Guid id)
        {
            var model = Repository.Search<Contact>(c => c.UId == id).FirstOrDefault();
            if (model == null)
                return RedirectToAction("404Error", "Error");
            Crumb.Label = "Contact: " + model.Email;
            UserManager.Update(user);
            return View(model);
        }

        [HttpPut]
        [Authorize(Roles = "Super Administrators,Cloud+ Users,Programmers")]
        [ActionName("ContactData")]
        [JsonValidateAntiForgeryToken]
        public JsonNetResult SaveContactData(JTableRow row, JTableColumn column, JTableHeader header)
        {
            var contact = Repository.Search<Contact>(c => c.UId.ToString() == row.Id).FirstOrDefault();
            if (contact == null)
                return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = false, Message = "Invalid Contact" } };
            var data = contact.Data.FirstOrDefault(d => d.HeaderKey.ToString() == column.HeaderId);
            var t_header = Repository.Search<ContactHeader>(h => h.UId.ToString() == header.Id).FirstOrDefault();
            if (t_header == null)
                return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = false, Message = "Invalid Header" } };
            if (data == null)
                data = ContactData.New(contact, t_header, column.Value);
            else
                data.Value = column.Value;
            Repository.Commit();
            return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = true, Message = "Success", Id = data.UId.ToString(), Value = data.Value, PrettyValue = data.PrettyValue } };
        }

        [HttpPut]
        [Authorize(Roles = "Super Administrators,Cloud+ Users,Programmers")]
        [ActionName("Contact")]
        [JsonValidateAntiForgeryToken]
        public JsonNetResult EditContact(ContactEditModel model)
        {
            var contact = Repository.Search<Contact>(c => c.UId == model.UId).FirstOrDefault();
            if (contact == null)
                return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = false, Message = "Invalid Object" } };
            contact.Email = model.Email;
            if (Repository.Search<Contact>(c => c.CompanyKey == contact.CompanyKey && c.Name == contact.Name && c.UId != contact.UId, checkAccess: false).Count() > 0)
                return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = false, Message = "Email Currently Used" } };
            foreach (var data in model.Data)
            {
                var o_data = contact.Data.FirstOrDefault(d => d.HeaderKey == data.Key);
                if (o_data == null)
                {
                    var header = Repository.Search<ContactHeader>(h => h.UId == data.Key, checkAccess: false).FirstOrDefault();
                    if (header == null)
                        continue;
                    o_data = ContactData.New(contact, header, data.Value);
                }
                else
                {
                    o_data.Value = data.Value;
                }
            }
            Repository.Commit();
            return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = true } };
        }

        #endregion

        // EmailList
        [HttpPost]
        [Authorize(Roles = "Super Administrators,Cloud+ Users,Programmers")]
        [JsonValidateAntiForgeryToken]
        public JsonNetResult EmailList(JTable table, string name, string type, List<Guid> contacts)
        {
            table.Rows.Clear();
            var nameExists = Repository.Search<IEmailList>(l => l.CompanyKey == company.UId && l.Name == name).Count() != 0;
            if (nameExists)
                return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = false, Message = "Name already exists." } };
            switch (type)
            {
                case "SavedList":
                    var slist = SavedList.New(Repository, company, user, name: name);
                    var result = AddSavedListContacts(slist.UId, contacts);
                    return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = true, Id = slist.UId } };
                case "ContactList":
                    table.Name = name;
                    var clist = new SavedEmailTable()
                    {
                        UId = Guid.NewGuid(),
                        Company = company,
                        Name = name,
                        Table = table
                    };
                    Repository.Add(clist);
                    Repository.Commit();
                    return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = true, Id = clist.UId } };
                default:
                    return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = false, Message = "Invalid list type." } };
            }
        }

        [HttpDelete]
        [Authorize(Roles = "Super Administrators,Cloud+ Users,Programmers")]
        [JsonValidateAntiForgeryToken]
        public JsonNetResult EmailList(Guid id)
        {
            var list = Repository.Search<IEmailList>(l => l.CompanyKey == company.UId && l.UId == id).FirstOrDefault();
            if (list == null)
                return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = false, Message = "Invalid object." } };
            Repository.Remove(list);
            Repository.Commit();
            return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = true } };
        }

        // Report Lists
        [HttpGet]
        [Authorize(Roles = "Super Administrators,Cloud+ Users,Programmers")]
        [ActionName("EmailLists")]
        public JsonNetResult EmailLists()
        {
            var t_lists = Repository.Search<IEmailList>(ie => ie.CompanyKey == company.UId).ToList();
            var lists = new List<IEmailList>();
            lists.AddRange(t_lists.OfType<SavedList>());
            lists.AddRange(t_lists.OfType<SavedEmailTable>());
            var files = lists.Select(s => new { Id = s.UId, Name = s.Name + (s is SavedList ? " (static)" : "") }).ToList();
            return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = true, Files = files } };
        }


        [HttpGet]
        [Authorize(Roles = "Super Administrators,Cloud+ Users,Programmers")]
        [ActionName("ContactReports")]
        public ActionResult ContactReports()
        {
            var a_reports = Repository.Search<ContactReport>(r => r.CompanyKey == company.UId).ToList();
            var reports = new List<ContactReport>();
            foreach (var report in a_reports)
            {
            }
            return View(reports);
        }

        // Contact Report (Deprecated)
        #region Contact Report (Deprecated)

        [HttpPut]
        [Authorize(Roles = "Super Administrators,Cloud+ Users,Programmers")]
        [JsonValidateAntiForgeryToken]
        [ActionName("ContactReport")]
        public JsonNetResult UpdateContactReport(ContactReportRequest request)
        {
            Guid key;
            if (!Guid.TryParse(request.Name, out key))
                return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = false, Message = "The provided report could not be found." } };
            var report = Repository.Search<ContactReport>(r => r.UId == key).FirstOrDefault();
            if (report == null)
                return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = false, Message = "The provided report could not be found." } };
            for (var i = 0; i < report.Filters.Count; i++)
            {
                var filter = report.Filters[i];
                if (request.Filters.Where(f => f.UId == filter.UId).Count() == 0)
                {
                    report.Filters.RemoveAt(i);
                    i--;
                }
            }
            foreach (var filter in request.Filters)
            {
                var o_filter = report.Filters.FirstOrDefault(f => f.UId == filter.UId);
                if (o_filter == null)
                {
                    o_filter = QueryFilter.New(filter);
                    report.Filters.Add(o_filter);
                }
                else
                {
                    o_filter.ActingOn = filter.ActingOn;
                    o_filter.Test = filter.Test;
                    o_filter.Value = filter.Value;
                    o_filter.Link = filter.Link;
                    o_filter.GroupNext = filter.GroupNext;
                    o_filter.Order = filter.Order;
                }
            }
            Repository.Commit();
            return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { NewReport = false, Success = true } };
        }

        [HttpDelete]
        [Authorize(Roles = "Super Administrators,Cloud+ Users,Programmers")]
        [JsonValidateAntiForgeryToken]
        [ActionName("ContactReport")]
        public JsonNetResult DeleteContactReport(Guid id)
        {
            var report = Repository.Search<ContactReport>(r => r.UId == id).FirstOrDefault();
            if (report == null)
                return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = false, Message = "Invalid Object" } };
            Repository.Remove(report);
            Repository.Commit();
            return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = true, Location = Url.Action("ContactReports", "Cloud", null, Request.Url.Scheme) } };
        }

        [HttpPost]
        [Authorize(Roles = "Super Administrators,Cloud+ Users,Programmers")]
        [JsonValidateAntiForgeryToken]
        [ActionName("ContactReport")]
        public JsonNetResult NewContactReport(ContactReportRequest request)
        {
            var report = Repository.Search<ContactReport>(r => r.Name == request.Name && r.CompanyKey == company.UId).FirstOrDefault();
            if (report != null)
                return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = false, Message = "The provided report name is already in use." } };
            var filters = new List<QueryFilter>();
            foreach (var filter in request.Filters)
            {
                filters.Add(QueryFilter.New(filter));
            }
            report = ContactReport.New(Repository, company, user, name: request.Name, filters: filters);
            return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { NewReport = true, Success = true, UId = report.UId, Name = report.Name } };
        }

        #endregion

        // Contact Header
        #region Contact Header

        [HttpPost]
        [Authorize(Roles = "Super Administrators,Cloud+ Users,Programmers")]
        [ActionName("ContactHeader")]
        [JsonValidateAntiForgeryToken]
        public JsonNetResult AddContactHeader(ContactHeader model)
        {
            if (model.Name == null)
                return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = false, Message = "Header must have a name." } };
            var result = new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = true, Message = "Success" } };
            if (Repository.Search<ContactHeader>(h => h.Name.ToLower() == model.Name.ToLower() && h.CompanyKey == company.UId && h.SavedListKey == model.SavedListKey, checkAccess: false).Count() > 0)
                return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = false, Message = "Header name already exists." } };
            SavedList s_list = null;
            if (model.SavedListKey.HasValue)
                s_list = Repository.Search<SavedList>(s => s.UId == model.SavedListKey.Value).FirstOrDefault();
            var header = ContactHeader.New(Repository, company, model.Descriminator, s_list);
            header.Name = model.Name;
            header.DescriminatorOptions = model.DescriminatorOptions;
            Repository.Commit();
            return result;
        }

        [HttpPut]
        [Authorize(Roles = "Super Administrators,Cloud+ Users,Programmers")]
        [ActionName("ContactHeader")]
        [JsonValidateAntiForgeryToken]
        public JsonNetResult EditContactHeader(ContactHeader model)
        {
            if (model.Name == null)
                return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = false, Message = "Header must have a name." } };
            var header = Repository.Search<ContactHeader>(h => h.UId == model.UId).FirstOrDefault();
            if (header == null)
                return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = false, Message = "Invalid Object" } };
            SavedList s_list = null;
            if (model.SavedListKey.HasValue)
                s_list = Repository.Search<SavedList>(s => s.UId == model.SavedListKey.Value).FirstOrDefault();
            header.Name = model.Name;
            header.Descriminator = model.Descriminator;
            header.DescriminatorOptions = model.DescriminatorOptions;
            Repository.Commit();
            return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = true } };
        }

        [HttpDelete]
        [Authorize(Roles = "Super Administrators,Cloud+ Users,Programmers")]
        [ActionName("ContactHeader")]
        [JsonValidateAntiForgeryToken]
        public JsonResult RemoveContactHeader(Guid id)
        {
            var header = Repository.Search<ContactHeader>(h => h.UId == id).FirstOrDefault();
            if (header == null)
                return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = false, Message = "Header does not exist." } };
            Repository.Remove(header);
            Repository.Commit();
            return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = true, Id = header.UId } };
        }

        #endregion
        
        #endregion
        /*
        // Contact Upload
        #region Contact Upload

        [HttpGet]
        [Authorize(Roles = "Super Administrators,Cloud+ Users,Programmers")]
        [ActionName("RectifyContactUpload")]
        public ActionResult ReftifyContactUpload(Guid id)
        {
            ContactUploadListModel uploadModel = null;
            if (!ContactUploadStatus.Keys.Contains(id))
            {
                return RedirectToAction("Error404", "error");
            }
            else
            {
                uploadModel = ContactUploadStatus[id];
            }
            ViewBag.FileId = id;
            return View(uploadModel);
        }

        [HttpPut]
        [Authorize(Roles = "Super Administrators,Cloud+ Users,Programmers")]
        [ActionName("RectifyContactUpload")]
        [JsonValidateAntiForgeryToken]
        public JsonNetResult RectifyContactListUpload(RectifyUploadListModel model)
        {
            ContactUploadListModel uploadModel = null;
            if (!ContactUploadStatus.Keys.Contains(model.Id))
            {
                return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = false, Message = "The upload does not exist." } };
            }
            else
            {
                uploadModel = ContactUploadStatus[model.Id];
            }
            uploadModel.NeedsRectified = false;
            var handleRecitfy = new Thread(HandleContactListRectify);
            handleRecitfy.Start(model);

            return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = true, PostBack = Url.Action("ContactUploadStatus", "Cloud", new { id = model.Id }, Request.Url.Scheme) } };
        }

        [HttpGet]
        [Authorize(Roles = "Super Administrators,Cloud+ Users,Programmers")]
        [ActionName("ContactUploadSheet")]
        public ActionResult ContactUploadSheet(Guid id)
        {
            ContactUploadListModel uploadModel = null;
            if (!ContactUploadStatus.Keys.Contains(id))
            {
                return RedirectToAction("Error404", "error");
            }
            else
            {
                uploadModel = ContactUploadStatus[id];
            }
            user.BreadCrumbs.CutTail();
            UserManager.Update(user);
            ViewBag.FileId = id;
            return View(uploadModel);
        }

        [HttpPut]
        [Authorize(Roles = "Super Administrators,Cloud+ Users,Programmers")]
        [ActionName("ContactUploadSheet")]
        [JsonValidateAntiForgeryToken]
        public JsonNetResult ContactUploadSheet(Guid id, int sheet)
        {
            ContactUploadListModel uploadModel = null;
            if (!ContactUploadStatus.Keys.Contains(id))
            {
                return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = false, Message = "The upload does not exist." } };
            }
            else
            {
                uploadModel = ContactUploadStatus[id];
            }
            uploadModel.NeedsSheetSelection = false;
            uploadModel.SheetSelection = sheet;
            var handleUpload = new Thread(HandleContactListUpload);
            handleUpload.Start(uploadModel);
            return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = true, PostBack = Url.Action("ContactUploadStatus", "Cloud", new { id = id }, Request.Url.Scheme) } };
        }

        [HttpGet]
        [Authorize(Roles = "Super Administrators,Cloud+ Users,Programmers")]
        [ActionName("ContactUploadStatus")]
        public JsonNetResult GetContactUploadStatus(Guid id)
        {
            ContactUploadListModel status = null;
            if (ContactUploadStatus.Keys.Contains(id))
            {
                status = ContactUploadStatus[id];
                if (status.CriticalFailure || status.UpdateComplete)
                {
                    ContactUploadStatus.Remove(id);
                    if (System.IO.File.Exists(status.FilePath))
                        System.IO.File.Delete(status.FilePath);
                }
            }
            status = status ?? new ContactUploadListModel()
            {
                CriticalFailure = true,
                Message = "An unhandled error occured."
            };
            return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = status };
        }

        [HttpPost]
        [Authorize(Roles = "Super Administrators,Cloud+ Users,Programmers")]
        [ActionName("ContactUpload")]
        [JsonValidateAntiForgeryToken]
        public JsonNetResult ContactUpload(Guid? id = null)
        {
            // We need to grab the file.
            var file = Request.Files[0];
            if (!excel_mime.Contains(file.ContentType))
                // If the file is not excel we send back a failure message
                return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = false, Rectify = false, Message = "Invalid file format. The file must be excel format." } };

            // Next we need to handle the file;
            // First we assign a file unique identifier
            var fileId = Guid.NewGuid();
            // Now we save the file in case the server crashes.
            // To do this we need to create a file path
            var path = Path.Combine(Server.MapPath("~/TempFiles/ContactListSheets"), fileId.ToString() + "_" + company.UId.ToString() + Path.GetExtension(file.FileName));
            file.SaveAs(path);

            //Now we need to create an object to hold data.
            var uploadModel = new ContactUploadListModel()
            {
                CompanyKey = company.UId,
                SavedListKey = id,
                FilePath = path,
                RectifyLocation = Url.Action("RectifyContactUpload", "Cloud", new { id = fileId }, Request.Url.Scheme),
                SheetLocation = Url.Action("ContactUploadSheet", "Cloud", new { id = fileId }, Request.Url.Scheme),
                User = user
            };
            ContactUploadStatus.Add(fileId, uploadModel);

            // Now we start a new thread to handle the data and return a message that we got the file.
            var handleUpload = new Thread(HandleContactListUpload);
            handleUpload.Start(uploadModel);
            return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = true, PostBack = Url.Action("ContactUploadStatus", "Cloud", new { id = fileId }, Request.Url.Scheme) } };
        }


        #endregion


        #region Email Reports

        [HttpGet]
        [ActionName("EmailReports")]
        public ActionResult EmailReports(Guid id)
        {
            var node = Repository.Search<IEmailHolder>(n => n.UId == id).FirstOrDefault();
            if (node == null)
                return RedirectToAction("Error404", "Errors");
            var table = new JTable()
            {
                Id = node.UId.ToString(),
                Name = node.Name,
            };
            table.Sortings.Add(new JTableSorting() { ActingOn = "email", Ascending = true, Order = 1 });
            var order = 0;
            var t_possibleValues = new List<JTableHeaderPossibleValue>();
            //Recipient
            table.Headers.Add(new JTableHeader() { Id = "recipient", Label = "Recipient", Order = ++order, Editable = false });
            //Email
            var t_emails = node.AllEmails.OrderBy(e => e.Name).Select(e => new JTableHeaderPossibleValue() { Id = e.UId.ToString(), Label = e.Name }).ToList();
            t_emails.Add(new JTableHeaderPossibleValue() { Id = "Generic Email", Label = "Generic Email" });
            table.Headers.Add(new JTableHeader() { Id = "email", Label = "Email", Order = ++order, Type = "itemParent", Editable = false, PossibleValues = t_emails });
            //Email Type
            table.Headers.Add(new JTableHeader() { Id = "type", Label = "Type", Order = ++order, Type = "itemParent", Editable = false, PossibleValues = Enum.GetValues(typeof(EmailType)).Cast<EmailType>().Select(e => new JTableHeaderPossibleValue() { Id = ((int)e).ToString(), Label = e.GetStringValue() }).ToList() });
            //Status
            var possibleStatus = new List<JTableHeaderPossibleValue>()
            {
                new JTableHeaderPossibleValue() { Id = "sent for processing", Label = "Sent For Processing" },
                new JTableHeaderPossibleValue() { Id = "delivered", Label = "Delivered" },
                new JTableHeaderPossibleValue() { Id = "opened", Label = "Opened" },
                new JTableHeaderPossibleValue() { Id = "clicked", Label = "Clicked" },
                new JTableHeaderPossibleValue() { Id = "bounced", Label = "Bounced" }
            };
            table.Headers.Add(new JTableHeader() { Id = "status", Label = "Status", Order = ++order, Type = "multipleSelection", PossibleValues = possibleStatus, Editable = false });
            //Sent for processing
            table.Headers.Add(new JTableHeader() { Id = "sentForProcessing", Label = "Sent For Processing", Order = ++order, Type = "number", Editable = false });
            //Delivered
            table.Headers.Add(new JTableHeader() { Id = "delivered", Label = "Delivered", Order = ++order, Type = "number", Editable = false });
            //Opened
            table.Headers.Add(new JTableHeader() { Id = "opened", Label = "Opened", Order = ++order, Type = "number", Editable = false });
            //Clicked
            table.Headers.Add(new JTableHeader() { Id = "clicked", Label = "Clicked", Order = ++order, Type = "number", Editable = false });
            //Bounced
            table.Headers.Add(new JTableHeader() { Id = "bounced", Label = "Bounced", Order = ++order, Type = "number", Editable = false });
            foreach (var email in node.AllEmails)
            {
                System.Linq.Expressions.Expression<Func<EmailSend, bool>> search;
                if (node is Form)
                {
                    var form = node as Form;
                    if (form.Status == FormStatus.Developement || form.Status == FormStatus.Ready)
                        search = e => e.EmailKey == email.UId && e.Registrant != null && e.Registrant.Type == RegistrationType.Test;
                    else
                        search = e => e.EmailKey == email.UId && e.Registrant != null && e.Registrant.Type == RegistrationType.Live;
                }
                else
                {
                    search = e => e.EmailKey == email.UId;
                }
                foreach (var emailSend in Repository.Search<EmailSend>(search).OrderByDescending(e => e.DateSent).Distinct(new EmailSendComparer_Recipient()).ToList())
                {
                    var row = new JTableRow()
                    {
                        Id = emailSend.UId.ToString()
                    };
                    row.Columns.Add(new JTableColumn() { HeaderId = "recipient", Value = emailSend.Recipient, PrettyValue = emailSend.Recipient });
                    row.Columns.Add(new JTableColumn() { HeaderId = "type", Value = emailSend.EmailKey.ToString(), PrettyValue = email.Name });
                    row.Columns.Add(new JTableColumn() { HeaderId = "email", Value = ((int)email.EmailType).ToString(), PrettyValue = email.EmailType.GetStringValue() });
                    var t_events = new List<string>();
                    var t_eventPretty = "";
                    foreach (var t_event in emailSend.EmailEvents.OrderBy(ev => ev.Date))
                    {
                        t_events.Add(t_event.Event);
                        t_eventPretty = t_event.Event + ", ";
                    }
                    if (t_eventPretty.EndsWith(", "))
                        t_eventPretty = t_eventPretty.Substring(0, t_eventPretty.Length - 1);
                    row.Columns.Add(new JTableColumn() { HeaderId = "status", Value = JsonConvert.SerializeObject(t_events), PrettyValue = t_eventPretty });
                    var events = emailSend.EmailEvents.OrderByDescending(ev => ev.Date).ToList();
                    EmailEvent c_event = null;
                    c_event = events.FirstOrDefault(ev => ev.Event.ToLower() == "sent for processing");
                    row.Columns.Add(new JTableColumn() { HeaderId = "sentForProcessing", Id = emailSend.UId.ToString() + "sentForProcessing", Value = (c_event != null ? "1" : "0"), PrettyValue = (c_event != null ? c_event.Date.ToString("s") : "No") });
                    c_event = events.FirstOrDefault(ev => ev.Event.ToLower() == "delivered");
                    row.Columns.Add(new JTableColumn() { HeaderId = "delivered", Id = emailSend.UId.ToString() + "delivered", Value = (c_event != null ? "1" : "0"), PrettyValue = (c_event != null ? c_event.Date.ToString("s") : "No") });
                    c_event = events.FirstOrDefault(ev => ev.Event.ToLower() == "opened");
                    row.Columns.Add(new JTableColumn() { HeaderId = "opened", Id = emailSend.UId.ToString() + "opened", Value = (c_event != null ? "1" : "0"), PrettyValue = (c_event != null ? c_event.Date.ToString("s") : "No") });
                    c_event = events.FirstOrDefault(ev => ev.Event.ToLower() == "clicked");
                    row.Columns.Add(new JTableColumn() { HeaderId = "clicked", Id = emailSend.UId.ToString() + "clicked", Value = (c_event != null ? "1" : "0"), PrettyValue = (c_event != null ? c_event.Date.ToString("s") : "No") });
                    c_event = events.FirstOrDefault(ev => ev.Event.ToLower() == "bounced");
                    row.Columns.Add(new JTableColumn() { HeaderId = "bounced", Id = emailSend.UId.ToString() + "bounced", Value = (c_event != null ? "1" : "0"), PrettyValue = (c_event != null ? c_event.Date.ToString("s") : "No") });
                    table.AddRow(row);
                }
            }
            if (node is Form)
            {
                foreach (var emailSend in Repository.Search<EmailSend>(es => es.EmailKey == null && es.FormKey.HasValue && es.FormKey == node.UId).OrderBy(es => es.DateSent))
                {
                    var row = new JTableRow()
                    {
                        Id = emailSend.UId.ToString()
                    };
                    row.Columns.Add(new JTableColumn() { HeaderId = "recipient", Value = emailSend.Recipient, PrettyValue = emailSend.Recipient });
                    row.Columns.Add(new JTableColumn() { HeaderId = "type", Value = ((int)EmailType.Unclassified).ToString(), PrettyValue = EmailType.Unclassified.GetStringValue() });
                    row.Columns.Add(new JTableColumn() { HeaderId = "email", Value = "Generic Email", PrettyValue = "Generic Form Email" });
                    var t_events = new List<string>();
                    var t_eventPretty = "";
                    foreach (var t_event in emailSend.EmailEvents.OrderBy(ev => ev.Date))
                    {
                        t_events.Add(t_event.Event.ToLower());
                        t_eventPretty = t_event.Event.ToLower() + ", ";
                    }
                    if (t_events.Count > 0)
                        t_eventPretty = t_eventPretty.Substring(0, t_eventPretty.Length - 2);
                    if (t_eventPretty.EndsWith(", "))
                        t_eventPretty = t_eventPretty.Substring(0, t_eventPretty.Length - 1);
                    row.Columns.Add(new JTableColumn() { HeaderId = "status", Value = JsonConvert.SerializeObject(t_events), PrettyValue = t_eventPretty });
                    var events = emailSend.EmailEvents.OrderByDescending(ev => ev.Date).ToList();
                    EmailEvent c_event = null;
                    c_event = events.FirstOrDefault(ev => ev.Event.ToLower() == "sent for processing");
                    row.Columns.Add(new JTableColumn() { HeaderId = "sentForProcessing", Id = emailSend.UId.ToString() + "sentForProcessing", Value = (c_event != null ? "1" : "0"), PrettyValue = (c_event != null ? c_event.Date.ToString("s") : "No") });
                    c_event = events.FirstOrDefault(ev => ev.Event.ToLower() == "delivered");
                    row.Columns.Add(new JTableColumn() { HeaderId = "delivered", Id = emailSend.UId.ToString() + "delivered", Value = (c_event != null ? "1" : "0"), PrettyValue = (c_event != null ? c_event.Date.ToString("s") : "No") });
                    c_event = events.FirstOrDefault(ev => ev.Event.ToLower() == "opened");
                    row.Columns.Add(new JTableColumn() { HeaderId = "opened", Id = emailSend.UId.ToString() + "opened", Value = (c_event != null ? "1" : "0"), PrettyValue = (c_event != null ? c_event.Date.ToString("s") : "No") });
                    c_event = events.FirstOrDefault(ev => ev.Event.ToLower() == "clicked");
                    row.Columns.Add(new JTableColumn() { HeaderId = "clicked", Id = emailSend.UId.ToString() + "clicked", Value = (c_event != null ? "1" : "0"), PrettyValue = (c_event != null ? c_event.Date.ToString("s") : "No") });
                    c_event = events.FirstOrDefault(ev => ev.Event.ToLower() == "bounced");
                    row.Columns.Add(new JTableColumn() { HeaderId = "bounced", Id = emailSend.UId.ToString() + "bounced", Value = (c_event != null ? "1" : "0"), PrettyValue = (c_event != null ? c_event.Date.ToString("s") : "No") });
                    table.AddRow(row);
                }
            }
            return View("EmailReports", table);
        }

        #endregion

        #region SurveyReport

        [HttpGet]
        public ActionResult SurveyReport(Guid id)
        {
            var form = Repository.Search<Form>(f => f.UId == id).FirstOrDefault();
            if (form == null)
                return RedirectToAction("Error404", "Error");
            var model = new JTable()
            {
                Id = form.UId.ToString(),
                Name = form.Name
            };
            var allComponents = form.GetComponents();
            var order = 0;
            if (form.ParentForm != null)
            {
                model.Headers.Add(new JTableHeader() { Id = "email", Label = "Email", Order = ++order });
            }
            foreach (var component in allComponents)
            {
                if (component is FreeText)
                    continue;
                if (component is RatingSelect)
                {
                    var rs = component as RatingSelect;
                    if (rs.MappedComponent != null)
                    {
                        if (rs.MappedComponent is IComponentItemParent)
                        {
                            foreach (var item in (rs.MappedComponent as IComponentItemParent).Children.OrderBy(i => i.Order))
                            {
                                model.Headers.Add(new JTableHeader() { Id = item.UId.ToString(), Label = item.LabelText, Order = ++order, Group = rs.MappedComponent.LabelText, Type = "rating=>" + JsonConvert.SerializeObject(new { min = rs.MinRating, max = rs.MaxRating, step = rs.Step, type = rs.RatingSelectType.GetStringValue() }) });
                            }
                        }
                        else
                        {
                            model.Headers.Add(new JTableHeader() { Id = component.UId.ToString(), Label = component.LabelText, Order = ++order, Type = "rating" });
                        }
                    }
                }
                else if (component is IComponentItemParent)
                {
                    var values = new List<JTableHeaderPossibleValue>();
                    foreach (var item in (component as IComponentItemParent).Children)
                    {
                        values.Add(new JTableHeaderPossibleValue() { Id = item.UId.ToString(), Label = item.LabelText });
                    }
                    model.Headers.Add(new JTableHeader() { Id = component.UId.ToString(), Label = component.LabelText, Order = ++order, Type = (component is IComponentMultipleSelection ? "multipleSelection" : "itemParent"), PossibleValues = values });
                }
                else
                {
                    model.Headers.Add(new JTableHeader() { Id = component.UId.ToString(), Label = component.LabelText, Order = ++order });
                }
            }
            IEnumerable<Registrant> registrants;
            if (form.Status == FormStatus.Closed || form.Status == FormStatus.Complete || form.Status == FormStatus.Maintenance || form.Status == FormStatus.Open || form.Status == FormStatus.PaymentComplete)
                registrants = form.Registrants.Where(r => r.Type == RegistrationType.Live && r.Status == RegistrationStatus.Submitted);
            else
                registrants = form.Registrants.Where(r => r.Type == RegistrationType.Test && r.Status == RegistrationStatus.Submitted);
            model.TotalRecords = registrants.Count();
            foreach (var registrant in registrants)
            {
                var row = new JTableRow() { Id = registrant.UId.ToString() };
                if (form.ParentForm != null)
                {
                    row.Columns.Add(new JTableColumn() { HeaderId = "email", Id = "email", PrettyValue = registrant.Email, Value = registrant.Email });
                }
                foreach (var data in registrant.Data)
                {
                    row.Columns.Add(new JTableColumn() { HeaderId = data.VariableUId.ToString(), Id = data.UId.ToString(), PrettyValue = data.GetFormattedValue(), Value = data.Value });
                }
                model.AddRow(row);
            }
            return View("SurveyReport", model);
        }

        #endregion
        #region IReport

        [HttpGet]
        [CrumbLabel("Advanced Inventory Reports")]
        public ActionResult SingleFormReports(Guid id)
        {
            var form = Repository.Search<Form>(f => f.UId == id).FirstOrDefault();
            if (form == null)
                return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = false, Message = "Invalid object." } };
            ViewBag.Form = form;
            ViewBag.FormKey = form.UId;
            Crumb.Label = "Advanced Inventory Reports: " + form.Name;
            UserManager.Update(user);
            var reports = Repository.Search<IFormReport>(s => s.FormKey == form.UId).ToList();
            return View(reports);
        }

        [ActionName("IReport")]
        [HttpGet]
        public ActionResult GetIReport(Guid id)
        {
            var report = Repository.Search<IReport>(r => r.UId == id).FirstOrDefault();
            if (report is SingleFormReport)
                return GetSingleFormReport(id);
            else if (report is AdvancedInventoryReport)
                return GetAdvancedInventoryReport(id);
            else
                return RedirectToAction("Error404", "Error");
        }

        [JsonValidateAntiForgeryToken]
        [HttpDelete]
        [ActionName("IReport")]
        public JsonNetResult DeleteIReport(Guid id)
        {
            var report = Repository.Search<IReport>(r => r.UId == id).FirstOrDefault();
            if (report == null)
                return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = false, Message = "Invalid Object" } };
            Repository.Remove(report);
            Repository.Commit();
            return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = true } };
        }

        [HttpGet]
        public ActionResult ViewIReport(Guid id)
        {
            var report = Repository.Search<IReport>(r => r.UId == id).FirstOrDefault();
            if (report is SingleFormReport)
                return ViewSingleFormReport(id);
            else if (report is AdvancedInventoryReport)
                return ViewAdvancedInventoryReport(id);
            else
                return RedirectToAction("Error404", "Error");
        }

        [HttpGet]
        public ActionResult PrintIReport(Guid id)
        {
            var report = Repository.Search<IFormReport>(r => r.UId == id).FirstOrDefault();
            if (report == null)
                return RedirectToAction("404Error", "Error");
            ViewBag.Form = report.Form;
            ViewBag.ReportName = report.Name;
            var paging = new Paging()
            {
                Page = 1,
                RecordsPerPage = -1
            };
            user.BreadCrumbs.CutTail();
            UserManager.Update(user);
            var table = report.GetReportData(Repository, paging);
            return View("PrintIReport", table);
        }


        #endregion
        /*
        #region Advanced Inventory Report

        [HttpGet]
        [ActionName("AdvancedInventoryReport")]
        public ActionResult GetAdvancedInventoryReport(Guid id)
        {
            var report = Repository.Search<AdvancedInventoryReport>(r => r.UId == id).FirstOrDefault();
            if (report == null)
                return RedirectToAction("Error404", "Error");
            Crumb.Label = "Edit Advanced Report " + report.Name;
            UserManager.Update(user);
            return View("AdvancedInventoryReport", report);
        }

        [HttpPost]
        [ActionName("AdvancedInventoryReport")]
        [JsonValidateAntiForgeryToken]
        public JsonNetResult NewAdvancedInventoryReport(Guid id, string name = null)
        {
            var form = Repository.Search<Form>(f => f.UId == id).FirstOrDefault();
            if (form == null)
                return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = false, Message = "Invalid object." } };
            var report = AdvancedInventoryReport.New(Repository, user, company, form, name: name);
            return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = true, Location = Url.Action("AdvancedInventoryReport", "Cloud", new { id = report.UId }, Request.Url.Scheme) } };
        }

        [HttpPut]
        [ActionName("AdvancedInventoryReport")]
        [JsonValidateAntiForgeryToken]
        public JsonNetResult UpdateAdvancedInventoryReport(AdvancedInventoryReport model)
        {
            var report = Repository.Search<AdvancedInventoryReport>(r => r.UId == model.UId).FirstOrDefault();
            if (report == null)
                return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = false, Message = "Invalid Object" } };
            report.Name = model.Name;
            report.Favorite = model.Favorite;
            report.Script = model.Script;
            Repository.Commit();
            return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = true } };
        }

        [HttpDelete]
        [ActionName("AdvancedInventoryReport")]
        [JsonValidateAntiForgeryToken]
        public JsonNetResult DeleteAdvancedInventoryReport(Guid id)
        {
            var report = Repository.Search<AdvancedInventoryReport>(r => r.UId == id).FirstOrDefault();
            if (report == null)
                return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = false, Message = "Invlaid object." } };
            Repository.Remove(report);
            Repository.Commit();
            return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = true } };
        }

        public ActionResult ViewAdvancedInventoryReport(Guid id)
        {
            var report = Repository.Search<AdvancedInventoryReport>(r => r.UId == id).FirstOrDefault();
            if (report == null)
                return RedirectToAction("Error404", "Error");
            Domain.Data.OldJsonData.JsonTable table = null;
            try
            {
                table = report.GetReportData(Repository, null, null);
            }
            catch (RegScriptTableException e)
            {
                return View("AdvancedInventoryReportError", e);
            }
            Crumb.Label = report.Name;
            UserManager.Update(user);
            ViewBag.Form = "View Advanced Report: " + report.Form;
            ViewBag.Report = report;
            return View("ViewAdvancedInventoryReport", table);
        }

        #endregion
        #region Custom Single Form Report

        [HttpPost]
        [ActionName("SingleFormReport")]
        [JsonValidateAntiForgeryToken]
        public JsonNetResult NewSingleFormReport(Guid id, string name = null)
        {
            var form = Repository.Search<Form>(f => f.UId == id).FirstOrDefault();
            if (form == null)
                return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = false, Message = "Invalid object." } };
            var report = SingleFormReport.New(Repository, user, company, form, name: name);
            return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = true, Location = Url.Action("SingleFormReport", "Cloud", new { id = report.UId }, Request.Url.Scheme) } };
        }

        [HttpGet]
        [ActionName("SingleFormReport")]
        public ActionResult GetSingleFormReport(Guid id)
        {
            var report = Repository.Search<SingleFormReport>(r => r.UId == id).FirstOrDefault();
            if (report == null)
                return RedirectToAction("404Error", "Error");
            var form = report.Form;
            //var checkboxgroups = new List<ItemModel>();
            var components = new List<ComponentModel>();
            form.Variables.ToList().ForEach(v =>
            {
                if (v.Component is CheckboxItem || v.Component is RadioItem || v.Component is DropdownItem || v.Component is FreeText)
                {
                    //Do Nothing
                }
                else
                {
                    var items = new List<ItemModel>();
                    var component = new ComponentModel()
                    {
                        Items = items,
                        UId = v.Component.UId,
                        Name = v.Value
                    };
                    if (v.Component is CheckboxGroup)
                    {
                        component.Type = "checkboxgroup";
                        foreach (CheckboxItem item in ((CheckboxGroup)v.Component).Items)
                            items.Add(new ItemModel() { Name = item.LabelText, UId = item.UId });
                    }
                    else if (v.Component is RadioGroup)
                    {
                        component.Type = "radiogroup";
                        foreach (RadioItem item in ((RadioGroup)v.Component).Items)
                            items.Add(new ItemModel() { Name = item.LabelText, UId = item.UId });
                    }
                    else if (v.Component is DropdownGroup)
                    {
                        component.Type = "dropdowngroup";
                        foreach (DropdownItem item in ((DropdownGroup)v.Component).Items)
                            items.Add(new ItemModel() { Name = item.LabelText, UId = item.UId });
                    }
                    else if (v.Component is Input)
                    {
                        var input = (Input)v.Component;
                        if (input.Type == Domain.Entities.Components.InputType.Date || input.Type == Domain.Entities.Components.InputType.DateTime || input.Type == Domain.Entities.Components.InputType.Time)
                            component.Type = "datetime";
                        else
                            component.Type = "input";
                    }
                    else
                        component.Type = "default";
                    components.Add(component);
                }
            });
            ViewBag.Components = components;
            ViewBag.Audiences = form.Audiences.Select(a => new ItemModel() { UId = a.UId, Name = a.Name }).ToList();
            ViewBag.Status = (Enum.GetValues(typeof(RegistrationStatus)) as RegistrationStatus[]).Select(s => new EnumModel() { Index = (int)s, Name = s.GetStringValue() });
            ViewBag.Types = (Enum.GetValues(typeof(RegistrationType)) as RegistrationType[]).Select(s => new EnumModel() { Index = (int)s, Name = s.GetStringValue() });
            ViewBag.Tests = (Enum.GetValues(typeof(LogicTest)) as LogicTest[]).Select(e => new EnumModel() { Index = (int)e, Name = e.GetStringValue() });
            ViewBag.Repository = Repository;
            return View("SingleFormReport", report);
        }

        [HttpGet]
        public ActionResult PrintSingleFormReport(Guid id)
        {
            var report = Repository.Search<SingleFormReport>(r => r.UId == id).FirstOrDefault();
            if (report == null)
                return RedirectToAction("404Error", "Error");
            ViewBag.Form = report.Form;
            ViewBag.ReportName = report.Name;
            var paging = new Paging()
            {
                Page = 1,
                RecordsPerPage = -1
            };
            var table = report.GetReportData(Repository, paging);
            return View("PrintIReport", table);
        }

        [HttpPut]
        [ActionName("SingleFormReport")]
        [JsonValidateAntiForgeryToken]
        public JsonNetResult EditSingleFormReport(SingleFormReport model)
        {
            var report = Repository.Search<SingleFormReport>(r => r.UId == model.UId).FirstOrDefault();
            if (report == null)
                return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = false, Message = "Invalid Object" } };
            foreach (var filter in model.Filters)
            {
                if (filter.UId == Guid.Empty)
                {
                    filter.UId = Guid.NewGuid();
                    report.Filters.Add(filter);
                }
                else
                {
                    var o_filter = report.Filters.Where(f => f.UId == filter.UId).FirstOrDefault();
                    if (o_filter == null)
                        continue;
                    o_filter.Order = filter.Order;
                    o_filter.Link = filter.Link;
                    o_filter.ActingOn = filter.ActingOn;
                    o_filter.GroupNext = filter.GroupNext;
                    o_filter.Test = filter.Test;
                    o_filter.Value = filter.Value;
                }
            }
            foreach (var filter in report.Filters.ToList())
            {
                if (model.Filters.Where(f => f.UId == filter.UId).Count() != 1)
                    report.Filters.Remove(filter);
            }
            report.SortOn = model.SortOn;
            report.Name = model.Name;
            report.Ascending = model.Ascending;
            report.Variables = model.Variables;
            report.Favorite = model.Favorite;
            report.Inventory = model.Inventory;
            Repository.Commit();
            return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = true } };
        }

        [HttpDelete]
        [ActionName("SingleFormReport")]
        [JsonValidateAntiForgeryToken]
        public JsonNetResult DeleteSingleFormReport(Guid id)
        {
            var report = Repository.Search<SingleFormReport>(r => r.UId == id).FirstOrDefault();
            if (report == null)
                return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = false, Message = "Invalid Object" } };
            Repository.Remove(report);
            Repository.Commit();
            return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = true } };
        }

        [HttpGet]
        public ActionResult ViewSingleFormReport(Guid id)
        {
            var report = Repository.Search<SingleFormReport>(r => r.UId == id).FirstOrDefault();
            if (report == null)
                return RedirectToAction("404Error", "Error");
            ViewBag.Name = report.Name;
            return View("ViewSingleFormReport", report);
        }

        [HttpGet]
        public JsonNetResult SingleFormReportData(Guid id, int page, int pageSize)
        {
            var report = Repository.Search<SingleFormReport>(r => r.UId == id).FirstOrDefault();
            if (report == null)
                return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = false, Message = "Invalid Object" } };
            var paging = new Paging()
            {
                Page = page,
                RecordsPerPage = pageSize
            };
            var table = report.GetReportData(Repository, paging);
            return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = true, Data = table } };
        }

        #endregion

        #region Registrant

        /*
        [HttpGet]
        [CrumbLabel("Registrant Change Sets")]
        public ActionResult RegistrantChangeSet(Guid id)
        {
            var registrant = Repository.Search<Registrant>(r => r.UId == id).FirstOrDefault();
            if (registrant == null)
                if (Request.IsAjaxRequest())
                    return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = false, Message = "Invalid object." } };
                else
                    return RedirectToAction("Error404", "Error");
            if (Request.IsAjaxRequest())
            {
                var form = registrant.Form;
                var table = new JTable();
                table.Name = registrant.Email;
                table.Parent = registrant.Form.Name;
                table.Id = registrant.UId.ToString();
                var modifiers = new Dictionary<Guid, String>() { { user.UId, user.UserName }, { Guid.Empty, "Registrant" } };
                table.AddHeader(new JTableHeader() { Id = "confirmation", Label = "Confirmation" });
                table.AddHeader(new JTableHeader() { Id = "email", Label = "Email" });
                if (registrant.Form.Pages.First(p => p.Type == PageType.RSVP).Enabled)
                    table.AddHeader(new JTableHeader() { Id = "rsvp", Label = "RSVP", Type = "boolean" });
                if (registrant.Form.Audiences.Count > 0)
                    table.AddHeader(new JTableHeader() { Id = "audience", Label = "Audience", Type = "itemParent", PossibleValues = form.Audiences.Select(a => new JTableHeaderPossibleValue() { Id = a.UId.ToString(), Label = a.Name }).ToList() });
                table.AddHeader(new JTableHeader() { Id = "lastmodified", Label = "Last Modified On", Type = "datetime" });
                var modHeader = new JTableHeader() { Id = "modifiedby", Label = "Modified By", Type = "itemParent" };
                table.AddHeader(modHeader);
                foreach (var component in registrant.Form.GetComponents())
                {
                    if (component is FreeText)
                        continue;
                    var compType = "text";
                    var possVals = new List<JTableHeaderPossibleValue>();
                    if (component is IComponentItemParent)
                    {
                        compType = "itemParent";
                        possVals = (component as IComponentItemParent).Children.ToList().Select(i => new JTableHeaderPossibleValue() { Id = i.UId.ToString(), Label = i.LabelText }).ToList();
                    }
                    table.AddHeader(new JTableHeader() { Id = component.UId.ToString(), Label = (component.Variable != null && !String.IsNullOrWhiteSpace(component.Variable.Value) ? component.Variable.Value : component.LabelText), Type = compType, PossibleValues = possVals });
                }
                var curRow = new JTableRow() { Id = registrant.UId.ToString() };
                var curCols = new List<JTableColumn>();
                curCols.Add(new JTableColumn() { Id = registrant.UId.ToString() + "_email", HeaderId = "email", Value = registrant.Email.ToLower(), PrettyValue = registrant.Email });
                curCols.Add(new JTableColumn() { Id = registrant.UId.ToString() + "_confirmation", HeaderId = "confirmation", Value = registrant.Confirmation, PrettyValue = registrant.Confirmation });
                if (registrant.Form.Pages.First(p => p.Type == PageType.RSVP).Enabled)
                    curCols.Add(new JTableColumn() { Id = registrant.UId.ToString() + "_rsvp", HeaderId = "rsvp", PrettyValue = registrant.RSVP ? form.RSVPAccept : form.RSVPDecline, Value = registrant.RSVP ? "1" : "0" });
                if (registrant.Form.Audiences.Count > 0)
                {
                    var h_id = "";
                    var h_name = "";
                    if (registrant.Audience != null)
                    {
                        h_id = registrant.Audience.UId.ToString();
                        h_name = registrant.Audience.Name;
                    }
                    curCols.Add(new JTableColumn() { Id = registrant.UId.ToString() + "_audience", HeaderId = "audience", Value = h_id, PrettyValue = h_name });
                }
                curCols.Add(new JTableColumn() { Id = registrant.UId.ToString() + "_lastmodified", HeaderId = "lastmodified", Value = registrant.DateModified.ToString("s"), PrettyValue = registrant.DateModified.ToString(user) });
                if (!modifiers.ContainsKey(registrant.ModifiedBy))
                {
                    var modUser = UserManager.FindById(registrant.ModifiedBy.ToString());
                    if (modUser == null)
                        modifiers.Add(registrant.ModifiedBy, "Removed User");
                    else
                        modifiers.Add(modUser.UId, modUser.UserName);
                }
                curCols.Add(new JTableColumn() { Id = registrant.UId.ToString() + "_modifiedby", HeaderId = "modifiedby", Value = registrant.ModifiedBy.ToString(), PrettyValue = modifiers[registrant.ModifiedBy] });
                foreach (var data in registrant.Data)
                {
                    curCols.Add(new JTableColumn() { Id = data.UId.ToString(), HeaderId = data.VariableUId.ToString(), PrettyValue = data.GetFormattedValue(), Value = data.Value, Editable = true });
                }
                curRow.Columns.AddRange(curCols);
                table.AddRow(curRow);
                foreach (var oReg in registrant.OldRegistrations.OrderByDescending(o => o.DateCreated))
                {
                    var oRow = new JTableRow() { Id = oReg.UId.ToString() };
                    var oCols = new List<JTableColumn>();
                    oCols.Add(new JTableColumn() { Id = oReg.UId.ToString() + "_email", HeaderId = "email", Value = oReg.Email.ToLower(), PrettyValue = oReg.Email });
                    oCols.Add(new JTableColumn() { Id = oReg.UId.ToString() + "_confirmation", HeaderId = "confirmation", Value = oReg.Confirmation, PrettyValue = oReg.Confirmation });
                    if (registrant.Form.Pages.First(p => p.Type == PageType.RSVP).Enabled)
                        oCols.Add(new JTableColumn() { Id = oReg.UId.ToString() + "_rsvp", HeaderId = "rsvp", PrettyValue = oReg.RSVP ? form.RSVPAccept : form.RSVPDecline, Value = registrant.RSVP ? "1" : "0" });
                    if (oReg.Form.Audiences.Count > 0)
                    {
                        var h_id = "";
                        var h_name = "";
                        if (oReg.Audience != null)
                        {
                            h_id = oReg.Audience.UId.ToString();
                            h_name = oReg.Audience.Name;
                        }
                        oCols.Add(new JTableColumn() { Id = oReg.UId.ToString() + "_audience", HeaderId = "audience", Value = h_id, PrettyValue = h_name });
                    }
                    oCols.Add(new JTableColumn() { Id = oReg.UId.ToString() + "_lastmodified", HeaderId = "lastmodified", Value = oReg.DateCreated.ToString("s"), PrettyValue = oReg.DateModified.ToString(user) });
                    if (!modifiers.ContainsKey(oReg.ModifiedBy))
                    {
                        var modUser = UserManager.FindById(oReg.ModifiedBy.ToString());
                        if (modUser == null)
                            modifiers.Add(registrant.ModifiedBy, "Removed User");
                        else
                            modifiers.Add(modUser.UId, modUser.UserName);
                    }
                    oCols.Add(new JTableColumn() { Id = oReg.UId.ToString() + "_modifiedby", HeaderId = "modifiedby", Value = oReg.ModifiedBy.ToString(), PrettyValue = modifiers[oReg.ModifiedBy] });
                    foreach (var data in oReg.Data)
                    {
                        oCols.Add(new JTableColumn() { Id = data.UId.ToString(), HeaderId = data.VariableUId.ToString(), PrettyValue = data.GetFormattedValue(), Value = data.Value });
                    }
                    oRow.Columns.AddRange(oCols);
                    table.AddRow(oRow);
                }
                modHeader.PossibleValues = modifiers.Select(m => new JTableHeaderPossibleValue() { Id = m.Key.ToString(), Label = m.Value }).ToList();
                return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = true, Table = table } };
            }
            else
            {
                Crumb.Label = registrant.Email + " Record Changes";
                return View(registrant);
            }
        }

        [HttpDelete]
        [ActionName("MarkRegistrant")]
        [JsonValidateAntiForgeryToken]
        public JsonNetResult MarkRegistrant(Guid id, bool perm = false)
        {
            var registrant = Repository.Search<Registrant>(r => r.UId == id).FirstOrDefault();
            if (registrant == null)
                return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = false, Message = "Invalid Object" } };
            registrant.Status = RegistrationStatus.Deleted;
            registrant.StatusDate = DateTimeOffset.UtcNow;
            if (perm)
                Repository.Remove(registrant);
            Repository.Commit();
            return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = true } };
        }

        [HttpPost]
        [ActionName("SendEmail")]
        [JsonValidateAntiForgeryToken]
        public JsonNetResult SendEmail(API.api_EmailRequest request)
        {
            System.Web.Http.Results.OkNegotiatedContentResult<RSToolKit.WebUI.Controllers.API.api_EmailResponse> result = null;
            using (var apiController = new API.Email_SendController(Context))
            {
                result = apiController.Post(request) as System.Web.Http.Results.OkNegotiatedContentResult<RSToolKit.WebUI.Controllers.API.api_EmailResponse>;
            }
            if (result == null)
                return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = false, Message = "No Emails Sent" } };
            var data = result.Content;
            if (!data.Success)
                return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = false, Message = "No Emails Sent" } };
            if (data.Sends < 1)
                return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = false, Message = "No Emails Sent" } };
            return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = true, SuccessMessage = data.SuccessMessage, SuccessMessageTitle = data.SuccessMessageTitle, Sends = data.Sends } };
        }

        [HttpPost]
        [ActionName("RegistrantEmail")]
        [JsonValidateAntiForgeryToken]
        public JsonNetResult RegistrantEmail(Guid id, EmailType type = EmailType.Confirmation)
        {
            var registrant = Repository.Search<Registrant>(r => r.UId == id).FirstOrDefault();
            if (registrant == null)
                return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = false, Message = "Invalid Object" } };
            var count = SendRegistrantEmail(id, type);
            if (count > 0)
                return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = true, SuccessMessage = "A total of " + count + " email" + (count > 1 ? "s where" : " was") + " sent.", SuccessMessageTitle = "Emails Sent" } };
            return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = true, SuccessMessage = "No emails sent.", SuccessMessageTitle = "Emails Sent" } };
        }

        [HttpPut]
        [JsonValidateAntiForgeryToken]
        public JsonNetResult RegistrantCopyOldData(string id)
        {
            long l_id;
            Guid uid;
            OldRegistrantData dataPoint = null;
            if (long.TryParse(id, out l_id))
                dataPoint = Repository.Search<OldRegistrantData>(d => d.SortingId == l_id).FirstOrDefault();
            else if (Guid.TryParse(id, out uid))
                dataPoint = Repository.Search<OldRegistrantData>(d => d.UId == uid).FirstOrDefault();
            if (dataPoint == null)
                return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = false, Message = "Invalid data point." } };
            var c_dataPoint = dataPoint.Registrant.CurrentRegistration.Data.FirstOrDefault(d => d.Component.UId == dataPoint.VariableUId);
            if (c_dataPoint == null)
                return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = false, Message = "Invalid component." } };
            c_dataPoint.Value = dataPoint.Value;
            Repository.Commit();
            return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = true, Id = c_dataPoint.UId, Value = c_dataPoint.GetFormattedValue() } };
        }

        #endregion

        #region Emails

        [HttpGet]
        public ActionResult CustomEmails(Guid id)
        {
            var form = Repository.Search<Form>(f => f.UId == id).FirstOrDefault();
            if (form == null)
                return RedirectToAction("Error404", "Error");
            return View(form);
        }

        [HttpGet]
        public ActionResult EmailSend(Guid id)
        {
            var emailSend = Repository.Search<EmailSend>(e => e.UId == id).FirstOrDefault();
            if (emailSend == null)
                return RedirectToAction("Error404", "Error");
            return View(emailSend);
        }

        [HttpGet]
        public ActionResult Email(Guid id)
        {
            return View();
        }

        #endregion

        [HttpGet]
        public FileContentResult RegistrantFile(Guid id)
        {
            var data = Repository.Search<RegistrantData>(d => d.UId == id).FirstOrDefault();
            if (data == null)
                throw new ArgumentNullException();
            if (data.File == null)
                throw new ArgumentNullException();
            var component = Repository.Search<Component>(c => c.UId == data.VariableUId).FirstOrDefault();
            var variable = data.Component.Variable.Value;
            if (component != null)
                variable = component.Variable.Value;
            return File(data.File.BinaryData, data.File.FileType, variable + data.File.Extension);
        }
        
        [AllowAnonymous]
        public FileContentResult RegistrantImageThumbnail(Guid id, Guid component, int? width = null)
        {
            var binaryData = new byte[0];
            var registrant = Repository.Search<Registrant>(r => r.UId == id).FirstOrDefault();
            if (registrant == null)
                return new FileContentResult(binaryData, "image/jpg");
            var dataPoint = registrant.Data.Where(d => d.VariableUId == component).FirstOrDefault();
            if (dataPoint == null)
                return new FileContentResult(binaryData, "image/jpg");
            if (dataPoint.File == null)
                return new FileContentResult(binaryData, "image/jpg");
            using (var m_stream = new MemoryStream(dataPoint.File.BinaryData))
            using (var img_stream = new MemoryStream())
            {
                Image img = Image.FromStream(m_stream);
                Image thumbNail = img;
                if (width.HasValue && width.Value < img.Width)
                {
                    var callback = new Image.GetThumbnailImageAbort(_thumbnailCallback);
                    var height = (width.Value * img.Height) / img.Width;
                    thumbNail = img.GetThumbnailImage(width.Value, height, callback, new IntPtr());
                }
                var encoder = GetEncoderInfo(dataPoint.File.FileType);
                var encoderQuality = Encoder.Quality;
                if (encoder == null)
                    return new FileContentResult(binaryData, "image/jpg");
                var encoderParams = new EncoderParameters(1);
                var encoderParam = new EncoderParameter(encoderQuality, 25L);
                encoderParams.Param[0] = encoderParam;
                thumbNail.Save(img_stream, encoder, encoderParams);
                binaryData = img_stream.ToArray();
            }
            return new FileContentResult(binaryData, dataPoint.File.FileType);
        }

        #region Downloads

        [HttpGet]
        [ActionName("DownloadSavedList")]
        public FileResult DownloadSavedList(Guid id)
        {
            FileResult file = null;
            var savedList = Repository.Search<SavedList>(l => l.UId == id).FirstOrDefault();
            if (savedList == null)
                return null;
            using (var package = new ExcelPackage())
            {
                var book = package.Workbook;
                book.Properties.Author = "RegStep Technologies";
                book.Properties.Title = "Saved List: " + savedList.Name;
                book.Worksheets.Add("Contacts");
                var sheet = book.Worksheets["Contacts"];
                var h_count = 1;
                sheet.Cells[1, 1].Value = "Email";
                var headers = savedList.GetHeaders(Repository);
                foreach (var header in headers)
                {
                    h_count++;
                    sheet.Cells[1, h_count].Value = header.Name;
                }
                sheet.Cells[1, 1, 1, h_count].Style.Fill.PatternType = ExcelFillStyle.Solid;
                sheet.Cells[1, 1, 1, h_count].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(122, 2, 2));
                sheet.Cells[1, 1, 1, h_count].Style.Font.Color.SetColor(System.Drawing.Color.White);
                var border = sheet.Cells[1, 1, 1, h_count].Style.Border;
                border.Bottom.Style = border.Top.Style = border.Left.Style = border.Right.Style = ExcelBorderStyle.Thin;
                var r_count = 1;
                foreach (var contact in savedList.Contacts)
                {
                    //142, 40, 40
                    r_count++;
                    if (r_count % 2 != 0)
                    {
                        sheet.Cells[r_count, 1, r_count, h_count].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        sheet.Cells[r_count, 1, r_count, h_count].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(142, 40, 40));
                    }
                    sheet.Cells[r_count, 1].Value = contact.Email;
                    var h_contactCount = 2;
                    foreach (var header in headers)
                    {
                        var data = contact.Data.Where(d => d.HeaderKey == header.UId).FirstOrDefault();
                        if (data == null)
                            sheet.Cells[r_count, h_contactCount].Value = "";
                        else
                            sheet.Cells[r_count, h_contactCount].Value = data.GetFormattedValue();
                        h_contactCount++;
                    }
                }
                var bin = package.GetAsByteArray();
                file = new FileContentResult(bin, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
                file.FileDownloadName = "Contact List - " + savedList.Name + " .xlsx";
            }
            return file;
        }

        [HttpGet]
        [AllowAnonymous]
        [ActionName("DownloadCustomReport")]
        public FileResult DownloadCustomReport(Guid id)
        {
            FileResult file = null;
            var report = Repository.Search<SingleFormReport>(r => r.UId == id).FirstOrDefault();
            if (report == null)
                return null;
            var paging = new Paging()
            {
                Page = 1,
                RecordsPerPage = -1
            };
            var table = report.GetReportData(Repository, paging, new List<string>() { "confirmation" });
            using (var package = new ExcelPackage())
            {
                var book = package.Workbook;
                book.Properties.Author = "RegStep Technologies";
                book.Properties.Title = "Registration Report: " + report.Form.Name + " - " + report.Name;
                book.Worksheets.Add("Registrations");
                var sheet = book.Worksheets["Registrations"];
                var h_count = 0;
                foreach (var header in table.Headers)
                {
                    h_count++;
                    sheet.Cells[1, h_count].Value = header.Label;
                }
                sheet.Cells[1, 1, 1, h_count].Style.Fill.PatternType = ExcelFillStyle.Solid;
                sheet.Cells[1, 1, 1, h_count].Style.Fill.BackgroundColor.SetColor(excel_HeaderColor);
                sheet.Cells[1, 1, 1, h_count].Style.Font.Color.SetColor(System.Drawing.Color.White);
                var border = sheet.Cells[1, 1, 1, h_count].Style.Border;
                border.Bottom.Style = border.Top.Style = border.Left.Style = border.Right.Style = ExcelBorderStyle.Thin;
                var r_count = 1;
                foreach (var row in table.Rows)
                {
                    r_count++;
                    if (r_count % 2 != 0)
                    {
                        sheet.Cells[r_count, 1, r_count, h_count].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        sheet.Cells[r_count, 1, r_count, h_count].Style.Fill.BackgroundColor.SetColor(excel_StripeColor);
                    }
                    var r_contactCount = 0;
                    foreach (var header in table.Headers)
                    {
                        r_contactCount++;
                        var value = "";
                        var s_data = row.Columns.Where(d => d.HeaderId == header.Id).FirstOrDefault();
                        if (s_data != null)
                            value = s_data.Value;
                        sheet.Cells[r_count, r_contactCount].Value = value;
                    }
                }
                var bin = package.GetAsByteArray();
                file = new FileContentResult(bin, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
                file.FileDownloadName = "Registration Report - " + report.Form.Name + " - " + report.Name + ".xlsx";
            }
            return file;
        }

        [HttpPost]
        [ActionName("CreateSurveyReport")]
        public JsonNetResult CreateSurveyReport(JTable table)
        {
            var form = Repository.Search<Form>(f => f.UId.ToString() == table.Id).FirstOrDefault();
            if (form == null)
                return null;
            IEnumerable<Registrant> registrants;
            if (form.Status == FormStatus.Closed || form.Status == FormStatus.Complete || form.Status == FormStatus.Maintenance || form.Status == FormStatus.Open || form.Status == FormStatus.PaymentComplete)
                registrants = form.Registrants.Where(r => r.Type == RegistrationType.Live && r.Status == RegistrationStatus.Submitted);
            else
                registrants = form.Registrants.Where(r => r.Type == RegistrationType.Test && r.Status == RegistrationStatus.Submitted);
            table.TotalRecords = registrants.Count();
            var items = new List<IComponentItem>();
            foreach (var registrant in registrants)
            {
                var row = new JTableRow() { Id = registrant.UId.ToString() };
                if (form.ParentForm != null)
                {
                    row.Columns.Add(new JTableColumn() { HeaderId = "email", Id = "email", PrettyValue = registrant.Email, Value = registrant.Email });
                }
                foreach (var data in registrant.Data)
                {
                    row.Columns.Add(new JTableColumn() { HeaderId = data.VariableUId.ToString(), Id = data.UId.ToString(), PrettyValue = data.GetFormattedValue(), Value = data.Value });
                }
                table.AddRow(row);
            }
            var filteredRows = new List<JTableRow>();
            for (var i = 0; i < table.Rows.Count; i++)
            {
                var take = false;
                var grouping = true;
                var tests = new List<Tuple<bool, string>>();
                var row = table.Rows[i];
                for (var j = 0; j < table.Filters.Count; j++)
                {
                    var filter = table.Filters[j];
                    var groupTest = true;
                    var first = true;
                    grouping = filter.GroupNext;
                    var test = false;
                    do
                    {
                        if (filter.GroupNext)
                            grouping = false;
                        test = row.TestValue(filter, table.Headers.FirstOrDefault(h => h.Id == filter.ActingOn));
                        switch (filter.Link) {
                            case "and":
                                groupTest = groupTest && test;
                                break;
                            case "or":
                                if (first)
                                    groupTest = test;
                                else
                                    groupTest = groupTest || test;
                                break;
                            case "none":
                            default:
                                groupTest = test;
                                break;
                        }
                        first = false;
                        if (!grouping)
                            break;
                        j++;
                        if (j < table.Filters.Count)
                            filter = table.Filters[j];
                        else
                            break;
                    } while (grouping);
                    tests.Add(new Tuple<bool, string>(groupTest, table.Filters.Count > (j + 1) ? table.Filters[j + 1].Link : "none"));
                }
                take = tests.Count > 0 ? tests[0].Item1 : true;
                for (var j = 1; j < tests.Count; j++) {
                    switch (tests[j - 1].Item2) {
                        case "and":
                            take = take && tests[j].Item1;
                            break;
                        case "or":
                            take = take || tests[j].Item1;
                            break;
                        case "none":
                        default:
                            take = tests[j].Item1;
                            break;
                    }
                }
                if (take) {
                    filteredRows.Add(row);
                }
            }
            if (table.Filters.Count == 0)
            {
                filteredRows = table.Rows;
            }
            // Now we sort
            if (table.Sortings.Count > 0)
            {
                table.Sortings.Sort((a, b) => { return a.Order - b.Order; });
                filteredRows.Sort((a, b) => {
                    var index = 0;
                    var result = 0;
                    var a_data = a.Columns.FirstOrDefault(c => c.HeaderId == table.Sortings[index].ActingOn);
                    var b_data = b.Columns.FirstOrDefault(c => c.HeaderId == table.Sortings[index].ActingOn);
                    if (a_data == null)
                        return 1;
                    else if (b_data == null)
                        return -1;
                    result = a_data.PrettyValue.CompareTo(b_data.PrettyValue);
                    if (!table.Sortings[index].Ascending) {
                        result *= -1;
                    }
                    if (result == 0) {
                        index++;
                        while (result != 0)
                        {
                            if (index >= table.Sortings.Count) {
                                break;
                            }
                            a_data = a.Columns.FirstOrDefault(c => c.HeaderId == table.Sortings[index].ActingOn);
                            b_data = b.Columns.FirstOrDefault(c => c.HeaderId == table.Sortings[index].ActingOn);
                            result = a_data.PrettyValue.CompareTo(b_data.PrettyValue);
                            if (!table.Sortings[index].Ascending) {
                                result *= -1;
                            }
                        }
                    }
                    return result;
                });
                for (var i = 0; i < filteredRows.Count; i++)
                {
                    filteredRows[i].Order = (i + 1);
                }
            }
            using (var package = new ExcelPackage())
            {
                var book = package.Workbook;
                book.Properties.Author = "RegStep Technologies";
                book.Properties.Title = "Survey Report: " + form.Name;
                book.Worksheets.Add("Data");
                var sheet = book.Worksheets["Data"];
                var h_count = 0;
                if (table.Average)
                {
                    var row = new JTableRow();
                    var emailHeader = table.Headers.FirstOrDefault(h => h.Id.ToLower() == "email");
                    if (emailHeader != null)
                        table.Headers.Remove(emailHeader);
                    foreach (var header in table.Headers.OrderBy(h => h.Order))
                    {
                        var count = 0;
                        var takeHeader = false;
                        var col = new JTableColumn()
                        {
                            HeaderId = header.Id                            
                        };
                        if (header.Type == "multipleSelection")
                        {
                            var totals = new Dictionary<string, int>();
                            foreach (var t_row in filteredRows)
                            {
                                var t_col = t_row.Columns.FirstOrDefault(c => c.HeaderId == header.Id);
                                if (t_col == null)
                                    continue;
                                if (String.IsNullOrEmpty(t_col.Value))
                                    continue;
                                takeHeader = true;
                                count++;
                                var selected = JsonConvert.DeserializeObject<List<string>>(t_col.Value);
                                foreach (var select in selected)
                                {
                                    var item = items.FirstOrDefault(i => i.UId.ToString() == select);
                                    if (item == null)
                                    {
                                        item = Repository.Search<Component>(c => c.UId.ToString() == select).OfType<IComponentItem>().FirstOrDefault();
                                        if (item == null)
                                            continue;
                                        items.Add(item);
                                    }
                                    if (!totals.Keys.Contains(item.LabelText))
                                        totals.Add(item.LabelText, 0);
                                    totals[item.LabelText]++;
                                }
                            }
                            if (takeHeader)
                            {
                                foreach (var kvp in totals.OrderBy(kvp => kvp.Value))
                                {
                                    if (table.Average)
                                        col.PrettyValue += kvp.Key + ": " + ((double)kvp.Value / count).ToString("p", form.Culture) + "\r\n";
                                    else
                                        col.PrettyValue += kvp.Key + ": " + kvp.Value.ToString() + "\r\n";
                                }
                                if (col.PrettyValue.EndsWith("\r\n"))
                                    col.PrettyValue = col.PrettyValue.Substring(0, col.PrettyValue.Length - 4);
                                col.Value = col.PrettyValue;
                                row.Columns.Add(col);
                            }
                        }
                        else if (header.Type == "itemParent")
                        {
                            var totals = new Dictionary<string, int>();
                            foreach (var t_row in filteredRows)
                            {
                                var t_col = t_row.Columns.FirstOrDefault(c => c.HeaderId == header.Id);
                                if (t_col == null)
                                    continue;
                                if (String.IsNullOrEmpty(t_col.PrettyValue))
                                    continue;
                                takeHeader = true;
                                count++;
                                if (!totals.Keys.Contains(t_col.PrettyValue))
                                    totals.Add(t_col.PrettyValue, 0);
                                totals[t_col.PrettyValue]++;
                            }
                            if (takeHeader)
                            {
                                foreach (var kvp in totals.OrderBy(kvp => kvp.Value))
                                {
                                    if (table.Average)
                                        col.PrettyValue += kvp.Key + ": " + ((double)kvp.Value / count).ToString("p", form.Culture) + "\r\n";
                                    else
                                        col.PrettyValue += kvp.Key + ": " + kvp.Value.ToString() + "\r\n";
                                }
                                if (col.PrettyValue.EndsWith("\r\n"))
                                    col.PrettyValue = col.PrettyValue.Substring(0, col.PrettyValue.Length - 3);
                                row.Columns.Add(col);
                            }
                        }
                        else if (header.Type == "Number")
                        {
                            var total = 0d;
                            foreach (var t_row in filteredRows)
                            {
                                var t_col = t_row.Columns.FirstOrDefault(c => c.HeaderId == header.Id);
                                if (t_col == null)
                                    continue;
                                if (String.IsNullOrEmpty(t_col.PrettyValue))
                                    continue;
                                double numb;
                                if (!double.TryParse(t_col.Value, out numb))
                                    continue;
                                takeHeader = true;
                                total += numb;
                                count++;
                            }
                            if (takeHeader)
                            {
                                if (table.Average)
                                    col.Value = (total / count).ToString();
                                else
                                    col.Value += total.ToString();
                                col.PrettyValue = col.Value;
                                row.Columns.Add(col);
                            }
                        }
                        if (!takeHeader)
                            table.Headers.Remove(header);
                    }
                    filteredRows.Clear();
                    filteredRows.Add(row);
                }
                foreach (var header in table.Headers.OrderBy(h => h.Order).ToList())
                {
                    h_count++;
                    sheet.Cells[1, h_count].Value = header.Label;
                }
                sheet.Cells[1, 1, 1, h_count].Style.Fill.PatternType = ExcelFillStyle.Solid;
                sheet.Cells[1, 1, 1, h_count].Style.Fill.BackgroundColor.SetColor(excel_HeaderColor);
                sheet.Cells[1, 1, 1, h_count].Style.Font.Color.SetColor(System.Drawing.Color.White);
                var border = sheet.Cells[1, 1, 1, h_count].Style.Border;
                border.Bottom.Style = border.Top.Style = border.Left.Style = border.Right.Style = ExcelBorderStyle.Thin;
                var r_count = 1;
                foreach (var row in filteredRows.OrderBy(r => r.Order).ToList())
                {
                    r_count++;
                    if (r_count % 2 != 0)
                    {
                        sheet.Cells[r_count, 1, r_count, h_count].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        sheet.Cells[r_count, 1, r_count, h_count].Style.Fill.BackgroundColor.SetColor(excel_StripeColor);
                    }
                    var r_contactCount = 0;
                    foreach (var header in table.Headers.OrderBy(h => h.Order).ToList())
                    {
                        r_contactCount++;
                        var value = "";
                        var s_data = row.Columns.Where(d => d.HeaderId == header.Id).FirstOrDefault();
                        if (s_data != null)
                            value = s_data.PrettyValue;
                        if (value.Contains("\r\n"))
                            sheet.Cells[r_count, r_contactCount].Style.WrapText = true;
                        sheet.Cells[r_count, r_contactCount].Value = value;
                    }
                }
                var fileId = Guid.NewGuid();
                using (var fileStream = new FileStream(Server.MapPath("~/App_Data/Reports/" + fileId.ToString() + ".xlsx"), FileMode.Create))
                {
                    package.SaveAs(fileStream);
                }
                return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = true, Id = fileId } };
            }
        }

        [HttpPost]
        [ActionName("CreateReport")]
        [JsonValidateAntiForgeryToken]
        public JsonNetResult CreateReport(JTable table)
        {
            Guid uid;
            System.Globalization.CultureInfo culture;
            if (Guid.TryParse(table.Id ?? "", out uid))
            {
                var node = Repository.Search<IjTableProcessor>(jt => jt.UId == uid).FirstOrDefault();
                if (node == null)
                    return null;
                node.FillJTable(table, (User.IsInRole("Super Administrators") || User.IsInRole("Administrators") || User.IsInRole("Programmers")), noHtml: true);
                culture = node.Culture;
            }
            else
            {
                culture = System.Globalization.CultureInfo.CurrentCulture;
                foreach (var contact in company.Contacts)
                {
                    var row = new JTableRow()
                    {
                        Id = contact.UId.ToString()
                    };
                    row.Columns.Add(new JTableColumn() { Id = contact.UId.ToString() + "_email", HeaderId = "email", Value = contact.Email, PrettyValue = contact.Email, Editable = false });
                    foreach (var e_data in contact.Data)
                    {
                        row.Columns.Add(new JTableColumn() { Id = e_data.UId.ToString() + "_" + e_data.HeaderKey.ToString(), HeaderId = e_data.HeaderKey.ToString(), PrettyValue = e_data.PrettyValue, Value = e_data.Value, Editable = true });
                    }
                    table.AddRow(row);
                }
            }
            var filteredRows = new List<JTableRow>();
            for (var i = 0; i < table.Rows.Count; i++)
            {
                var take = false;
                var grouping = true;
                var tests = new List<Tuple<bool, string>>();
                var row = table.Rows[i];
                for (var j = 0; j < table.Filters.Count; j++)
                {
                    var filter = table.Filters[j];
                    var groupTest = true;
                    var first = true;
                    grouping = filter.GroupNext;
                    var test = false;
                    do
                    {
                        if (!filter.GroupNext)
                            grouping = false;
                        test = row.TestValue(filter, table.Headers.FirstOrDefault(h => h.Id == filter.ActingOn));
                        switch (filter.Link)
                        {
                            case "and":
                                groupTest = groupTest && test;
                                break;
                            case "or":
                                if (first)
                                    groupTest = test;
                                else
                                    groupTest = groupTest || test;
                                break;
                            case "none":
                            default:
                                groupTest = test;
                                break;
                        }
                        first = false;
                        if (!grouping)
                            break;
                        j++;
                        if (j < table.Filters.Count)
                            filter = table.Filters[j];
                        else
                            break;
                    } while (grouping);
                    tests.Add(new Tuple<bool, string>(groupTest, j < (table.Filters.Count - 1) ? table.Filters[j + 1].Link : "none"));
                }
                take = tests.Count > 0 ? tests[0].Item1 : true;
                for (var j = 1; j < tests.Count; j++)
                {
                    switch (tests[j - 1].Item2)
                    {
                        case "and":
                            take = take && tests[j].Item1;
                            break;
                        case "or":
                            take = take || tests[j].Item1;
                            break;
                        case "none":
                        default:
                            take = tests[j].Item1;
                            break;
                    }
                }
                if (take)
                {
                    filteredRows.Add(row);
                }
            }
            if (table.Filters.Count == 0)
            {
                filteredRows = table.Rows;
            }
            // Now we sort
            if (table.Sortings.Count > 0)
            {
                table.Sortings.Sort((a, b) => { return a.Order - b.Order; });
                filteredRows.Sort((a, b) =>
                {
                    var index = 0;
                    var result = 0;
                    var t_header = table.Headers.FirstOrDefault(h => h.Id == table.Sortings[index].ActingOn);
                    if (t_header == null)
                        return 0;
                    var a_data = a.Columns.FirstOrDefault(c => c.HeaderId == table.Sortings[index].ActingOn);
                    var b_data = b.Columns.FirstOrDefault(c => c.HeaderId == table.Sortings[index].ActingOn);
                    string aCompValue = null;
                    string bCompValue = null;
                    if (a_data != null)
                        aCompValue = a_data.Value;
                    if (b_data != null)
                        bCompValue = b_data.Value;
                    if (t_header.SortByPretty)
                    {
                        if (a_data != null)
                            aCompValue = a_data.PrettyValue;
                        else
                            aCompValue = null;
                        if (b_data != null)
                            bCompValue = b_data.PrettyValue;
                        else
                            bCompValue = null;
                    }
                    if (aCompValue == null)
                        return 1;
                    else if (bCompValue == null)
                        return -1;
                    result = aCompValue.CompareTo(bCompValue);
                    if (!table.Sortings[index].Ascending) {
                        result *= -1;
                    }
                    if (result == 0) {
                        index++;
                        while (result == 0) {
                            if (index >= table.Sortings.Count) {
                                break;
                            }
                            a_data = a.Columns.FirstOrDefault(c => c.HeaderId == table.Sortings[index].ActingOn);
                            b_data = b.Columns.FirstOrDefault(c => c.HeaderId == table.Sortings[index].ActingOn);
                            if (t_header.Type == "itemParent" || t_header.Type == "multipleSelection")
                                result = a_data.PrettyValue.CompareTo(b_data.PrettyValue);
                            else
                                result = a_data.Value.CompareTo(b_data.Value);
                            if (!table.Sortings[index].Ascending) {
                                result *= -1;
                            }
                            index++;
                        }
                    }
                    return result;
                });
                for (var i = 0; i < filteredRows.Count; i++)
                {
                    filteredRows[i].Order = (i + 1);
                }
            }
            using (var package = new ExcelPackage())
            {
                var book = package.Workbook;
                book.Properties.Author = "RegStep Technologies";
                book.Properties.Title = "Report: " + table.Name;
                book.Worksheets.Add("Data");
                var sheet = book.Worksheets["Data"];
                var h_count = 0;
                if (table.Average || table.Count)
                {
                    var row = new JTableRow();
                    var emailHeader = table.Headers.FirstOrDefault(h => h.Id.ToLower() == "email");
                    if (emailHeader != null)
                        emailHeader.Removed = true;
                    foreach (var header in table.Headers.OrderBy(h => h.Order))
                    {
                        var count = 0;
                        var takeHeader = false;
                        var col = new JTableColumn()
                        {
                            HeaderId = header.Id
                        };
                        if (header.Type == "multipleSelection")
                        {
                            var totals = new Dictionary<string, int>();
                            foreach (var t_row in filteredRows)
                            {
                                var t_col = t_row.Columns.FirstOrDefault(c => c.HeaderId == header.Id);
                                if (t_col == null)
                                    continue;
                                if (String.IsNullOrEmpty(t_col.Value))
                                    continue;
                                takeHeader = true;
                                count++;
                                var selected = JsonConvert.DeserializeObject<List<string>>(t_col.Value);
                                foreach (var select in selected)
                                {
                                    var item = header.PossibleValues.FirstOrDefault(i => i.Id == select);
                                    if (item == null)
                                        continue;
                                    if (!totals.Keys.Contains(item.Label))
                                        totals.Add(item.Label, 0);
                                    totals[item.Label]++;
                                }
                            }
                            if (takeHeader)
                            {
                                foreach (var kvp in totals.OrderBy(kvp => kvp.Value))
                                {
                                    if (table.Average)
                                        col.PrettyValue += kvp.Key + ": " + ((double)kvp.Value / count).ToString("p", culture) + "\r\n";
                                    else
                                        col.PrettyValue += kvp.Key + ": " + kvp.Value.ToString() + "\r\n";
                                }
                                if (col.PrettyValue.EndsWith("\r\n"))
                                    col.PrettyValue = col.PrettyValue.Substring(0, col.PrettyValue.Length - 4);
                                col.Value = col.PrettyValue;
                                row.Columns.Add(col);
                            }
                        }
                        else if (header.Type == "itemParent")
                        {
                            var totals = new Dictionary<string, int>();
                            foreach (var t_row in filteredRows)
                            {
                                var t_col = t_row.Columns.FirstOrDefault(c => c.HeaderId == header.Id);
                                if (t_col == null)
                                    continue;
                                if (String.IsNullOrEmpty(t_col.PrettyValue))
                                    continue;
                                takeHeader = true;
                                count++;
                                if (!totals.Keys.Contains(t_col.PrettyValue))
                                    totals.Add(t_col.PrettyValue, 0);
                                totals[t_col.PrettyValue]++;
                            }
                            if (takeHeader)
                            {
                                foreach (var kvp in totals.OrderBy(kvp => kvp.Value))
                                {
                                    if (table.Average)
                                        col.PrettyValue += kvp.Key + ": " + ((double)kvp.Value / count).ToString("p", culture) + "\r\n";
                                    else
                                        col.PrettyValue += kvp.Key + ": " + kvp.Value.ToString() + "\r\n";
                                }
                                if (col.PrettyValue.EndsWith("\r\n"))
                                    col.PrettyValue = col.PrettyValue.Substring(0, col.PrettyValue.Length - 3);
                                row.Columns.Add(col);
                            }
                        }
                        else if (header.Type == "Number")
                        {
                            var total = 0d;
                            foreach (var t_row in filteredRows)
                            {
                                var t_col = t_row.Columns.FirstOrDefault(c => c.HeaderId == header.Id);
                                if (t_col == null)
                                    continue;
                                if (String.IsNullOrEmpty(t_col.PrettyValue))
                                    continue;
                                double numb;
                                if (!double.TryParse(t_col.Value, out numb))
                                    continue;
                                takeHeader = true;
                                total += numb;
                                count++;
                            }
                            if (takeHeader)
                            {
                                if (table.Average)
                                    col.Value = (total / count).ToString();
                                else
                                    col.Value += total.ToString();
                                col.PrettyValue = col.Value;
                                row.Columns.Add(col);
                            }
                        }
                        if (!takeHeader)
                            header.Removed = true;
                    }
                    filteredRows.Clear();
                    filteredRows.Add(row);
                }
                foreach (var header in table.Headers.Where(h => !h.Removed).OrderBy(h => h.Order).ToList())
                {
                    h_count++;
                    sheet.Cells[1, h_count].Value = header.Label;
                }
                sheet.Cells[1, 1, 1, h_count].Style.Fill.PatternType = ExcelFillStyle.Solid;
                sheet.Cells[1, 1, 1, h_count].Style.Fill.BackgroundColor.SetColor(excel_HeaderColor);
                sheet.Cells[1, 1, 1, h_count].Style.Font.Color.SetColor(System.Drawing.Color.White);
                var border = sheet.Cells[1, 1, 1, h_count].Style.Border;
                border.Bottom.Style = border.Top.Style = border.Left.Style = border.Right.Style = ExcelBorderStyle.Thin;
                var r_count = 1;
                foreach (var row in filteredRows.OrderBy(r => r.Order).ToList())
                {
                    r_count++;
                    if (r_count % 2 != 0)
                    {
                        sheet.Cells[r_count, 1, r_count, h_count].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        sheet.Cells[r_count, 1, r_count, h_count].Style.Fill.BackgroundColor.SetColor(excel_StripeColor);
                    }
                    var r_contactCount = 0;
                    foreach (var header in table.Headers.Where(h => !h.Removed).OrderBy(h => h.Order).ToList())
                    {
                        r_contactCount++;
                        var value = "";
                        var s_data = row.Columns.Where(d => d.HeaderId == header.Id).FirstOrDefault();
                        if (s_data != null)
                        {
                            if (header.Id == "confirmation")
                            {
                                value = s_data.Value;
                            }
                            else if (header.Id == "balance")
                            {
                                value = decimal.Parse(s_data.Value).ToString("c", culture);
                            }
                            else
                            {
                                value = s_data.PrettyValue;
                            }
                        }
                        if (value.Contains("\r\n"))
                            sheet.Cells[r_count, r_contactCount].Style.WrapText = true;
                        sheet.Cells[r_count, r_contactCount].Value = value;
                    }
                }
                var fileId = Guid.NewGuid();
                using (var fileStream = new FileStream(Server.MapPath("~/App_Data/Reports/" + fileId.ToString() + ".xlsx"), FileMode.Create))
                {
                    package.SaveAs(fileStream);
                }
                return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = true, Id = fileId } };
            }
        }

        [HttpGet]
        [AllowAnonymous]
        [ActionName("DownloadReport")]
        public FileResult DownloadReport(Guid id, bool remove = true)
        {
            FileResult file = null;
            if (!System.IO.File.Exists(Server.MapPath("~/App_Data/Reports/" + id.ToString() + ".xlsx")))
                return null;
            var bytes = System.IO.File.ReadAllBytes(Server.MapPath("~/App_Data/Reports/" + id.ToString() + ".xlsx"));
            file = new FileContentResult(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
            file.FileDownloadName = "Report - " + DateTimeOffset.Now.ToString("s") + ".xlsx";
            if (remove)
                System.IO.File.Delete(Server.MapPath("~/App_Data/Reports/" + id.ToString() + ".xlsx"));
            return file;
        }

        [HttpGet]
        [AllowAnonymous]
        [ActionName("DownloadIReport")]
        public FileResult DownloadIReport(Guid id)
        {
            FileResult file = null;
            var report = Repository.Search<IFormReport>(r => r.UId == id).FirstOrDefault();
            if (report == null)
                return null;
            var paging = new Paging()
            {
                Page = 1,
                RecordsPerPage = -1
            };
            var table = report.GetReportData(Repository, paging);
            using (var package = new ExcelPackage())
            {
                var book = package.Workbook;
                book.Properties.Author = "RegStep Technologies";
                book.Properties.Title = "Registration Report: " + report.Form.Name + " - " + report.Name;
                book.Worksheets.Add("Registrations");
                var sheet = book.Worksheets["Registrations"];
                var h_count = 0;
                foreach (var header in table.Headers.OrderBy(h => h.Order).ToList())
                {
                    h_count++;
                    sheet.Cells[1, h_count].Value = header.Label;
                }
                sheet.Cells[1, 1, 1, h_count].Style.Fill.PatternType = ExcelFillStyle.Solid;
                sheet.Cells[1, 1, 1, h_count].Style.Fill.BackgroundColor.SetColor(excel_HeaderColor);
                sheet.Cells[1, 1, 1, h_count].Style.Font.Color.SetColor(System.Drawing.Color.White);
                var border = sheet.Cells[1, 1, 1, h_count].Style.Border;
                border.Bottom.Style = border.Top.Style = border.Left.Style = border.Right.Style = ExcelBorderStyle.Thin;
                var r_count = 1;
                foreach (var row in table.Rows.OrderBy(r => r.Order).ToList())
                {
                    r_count++;
                    if (r_count % 2 != 0)
                    {
                        sheet.Cells[r_count, 1, r_count, h_count].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        sheet.Cells[r_count, 1, r_count, h_count].Style.Fill.BackgroundColor.SetColor(excel_StripeColor);
                    }
                    var r_contactCount = 0;
                    foreach (var header in table.Headers.OrderBy(h => h.Order).ToList())
                    {
                        r_contactCount++;
                        var value = "";
                        var s_data = row.Columns.Where(d => d.HeaderId == header.Id).FirstOrDefault();
                        if (s_data != null)
                            value = s_data.Value;
                        sheet.Cells[r_count, r_contactCount].Value = value;
                    }
                }
                var bin = package.GetAsByteArray();
                file = new FileContentResult(bin, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
                file.FileDownloadName = "Registration Report - " + report.Form.Name + " - " + report.Name + ".xlsx";
            }
            return file;
        }

        #endregion

        private static ImageCodecInfo GetEncoderInfo(String mimeType)
        {
            int j;
            ImageCodecInfo[] encoders;
            encoders = ImageCodecInfo.GetImageEncoders();
            for (j = 0; j < encoders.Length; ++j)
            {
                if (encoders[j].MimeType == mimeType)
                    return encoders[j];
            }
            return null;
        }

        private bool _thumbnailCallback() { return true; }

        #region Thread Methods

        public static void UpdateContactData(ContactUploadListModel uploadModel, bool overwrite = false)
        {           
            using (var Repository = new FormsRepository())
            {
                var company = Repository.Search<Company>(c => c.UId == uploadModel.CompanyKey).FirstOrDefault();
                if (company == null)
                {
                    uploadModel.CriticalFailure = true;
                    uploadModel.Message = "The company is not valid.";
                }
                var contacts = company.Contacts.ToList();
                var currentCount = Repository.Search<Contact>(c => c.CompanyKey == uploadModel.CompanyKey).Count();
                var clSpotsFree = company.ContactLimit - currentCount;
                var totalData = (uploadModel.Headers.Count + 1) * uploadModel.Contacts.Count;
                var h_perContact = (uploadModel.Headers.Count + 1);
                var h_count = 0;

                // Grab all headers
                List<ContactHeader> c_headers;
                if (uploadModel.SavedListKey.HasValue)
                    c_headers = Repository.Search<ContactHeader>(h => h.CompanyKey == company.UId && (h.SavedListKey == null || h.SavedListKey == uploadModel.SavedListKey.Value)).ToList();
                else
                    c_headers = Repository.Search<ContactHeader>(h => h.CompanyKey == company.UId && h.SavedListKey == null).ToList();

                var savedList = Repository.Search<SavedList>(l => l.UId == uploadModel.SavedListKey).FirstOrDefault();
                
                // Now we add the data to the database.
                foreach (var contact in uploadModel.Contacts)
                {
                    Contact o_contact = null;
                    if (contact.ContactKey.HasValue)
                        o_contact = contacts.FirstOrDefault(c => c.UId == contact.ContactKey);
                    if (o_contact != null && !overwrite)
                    {
                        h_count += h_perContact;
                        uploadModel.UpdatePercent = (float)h_count / (float)totalData;
                        continue;
                    }

                    h_count++;
                    uploadModel.UpdatePercent = (float)h_count / (float)totalData;

                    Contact u_contact;
                    if (o_contact == null)
                    {
                        u_contact = Contact.New(Repository, company, uploadModel.User, email: contact.Email);
                        uploadModel.UpdatePercent = (float)h_count / (float)totalData;
                    }
                    else
                    {
                        u_contact = o_contact;
                        uploadModel.UpdatePercent = (float)h_count / (float)totalData;
                    }
                    foreach (var data in contact.Data)
                    {
                        h_count++;
                        uploadModel.UpdatePercent = (float)h_count / (float)totalData;
                        if (String.IsNullOrWhiteSpace(data.Value))
                            continue;
                        if (!data.HeaderKey.HasValue)
                            continue;
                        var o_data = u_contact.Data.Where(d => d.HeaderKey == data.HeaderKey).FirstOrDefault();
                        if (o_data == null)
                        {
                            var c_header = c_headers.Where(h => h.UId == data.HeaderKey).FirstOrDefault();
                            if (c_header == null)
                                continue;
                            var n_data = ContactData.New(u_contact, c_header, data.Value);
                        }
                        else
                        {
                            o_data.Value = data.Value;
                        }
                    }
                    if (savedList != null)
                    {
                        if (savedList.Contacts.Where(c => c.UId == u_contact.UId).FirstOrDefault() == null)
                            savedList.Contacts.Add(u_contact);
                    }
                }
                uploadModel.UpdateComplete = true;
                Repository.Commit();
            }
        }

        #endregion
        //*/
    }

    #region Paged Report Classes

    #endregion

    #region Supporting Classes Reports

    public class ModificationCheck
    {
        public Guid id { get; set; }
        public string savedId { get; set; }
        public Dictionary<string, string> options { get; set; }
        public List<ModificationToken> items { get; set; }
        public DateTimeOffset lastFullCheck { get; set; }
        
        public ModificationCheck()
        {
            id = Guid.Empty;
            options = new Dictionary<string,string>();
            items = new List<ModificationToken>();
        }
    }

    public class ModificationToken
    {
        public string id { get; set; }
        public string token { get; set; }
    }

    public class ReportTable
    {
        public Guid UId { get; set; }
        public string Name { get; set; }
    }

    public class ReportNameUpdate
    {
        public Guid Company { get; set; }
        public Guid UId { get; set; }
        public string Name { get; set; }
    }

    public class HeaderInformtaion
    {
        public Int64 Id { get; set; }
        public Guid UId { get; set; }
        public Guid Company { get; set; }
        public string Name { get; set; }
    }

    public class ReportsRequestInformation
    {
        public int Page { get; set; }
        public int RecordsPerPage { get; set; }
        public int TotalRecords { get; set; }
        public int TotalPages { get; set; }
        public Guid Company { get; set; }
        public List<SortingInformation> Sortings { get; set; }
        public List<FilterInformation> Filters { get; set; }
    }

    #endregion

    #region Supporting Classes

    public class ajaxRegistrantData
    {
        public string registrantId { get; set; }
        public string value { get; set; }
        public string componentId { get; set; }
        public Dictionary<Guid, bool> waitlistings { get; set; }

        public ajaxRegistrantData()
        {
            waitlistings = new Dictionary<Guid, bool>();
        }
    }

    public class ajaxTransaction
    {
        public string id { get; set; }
        public DateTimeOffset date { get; set; }
        public string amount { get; set; }
        public decimal balance { get; set; }
        public string prettyBalance { get; set; }
        public IEnumerable<ajaxTransactionDetail> details { get; set; }
        public string lastFour { get; set; }
        public string type { get; set; }

        public ajaxTransaction()
        {
            details = new List<ajaxTransactionDetail>();
        }
    }

    public class ajaxTransactionDetail
    {
        public DateTimeOffset date { get; set; }
        public string type { get; set; }
        public string status { get; set; }
        public string id { get; set; }
        public string note { get; set; }
        public string amount { get; set; }
        public IEnumerable<ajaxTransactionDetail> credits { get; set; }
        public bool failed { get; set; }

        public ajaxTransactionDetail()
        {
            credits = new List<ajaxTransactionDetail>();
        }
    }

    public class ajaxRegistrant
    {
        public Guid id { get; set; }
        public DateTimeOffset dateCreated { get; set; }
        public DateTimeOffset dateModified { get; set; }
        public RegistrationStatus status { get; set; }
        public string statusLabel { get; set; }
        public string audience { get; set; }
        public string phone { get; set; }
        public string email { get; set; }
        public Guid formKey { get; set; }
        public IEnumerable<ajaxFinance> finances { get; set; }
        public IEnumerable<ajaxEmailActivity> emailActivities { get; set; }
        public string balance { get; set; }
        public string fullName { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string lastnameFirst { get; set; }
        public IEnumerable<ajaxSeat> seatings { get; set; }
        public string confirmation { get; set; }
        public string invoiceNumber { get; set; }

        public ajaxRegistrant()
        {
            phone = null;
            finances = new List<ajaxFinance>();
            emailActivities = new List<ajaxEmailActivity>();
        }
    }

    public class ajaxSeat
    {
        public bool seated { get; set; }
        public string date { get; set; }
        public string dateSeated { get; set; }
        public string component { get; set; }
    }

    public class ajaxFinance
    {
        public DateTimeOffset date { get; set; }
        public string label { get; set; }
        public string amount { get; set; }
        public bool failed { get; set; }
        public bool voided { get; set; }
        public string creator { get; set; }
        public bool voidable { get; set; }
        public int type { get; set; }
        public long id { get; set; }

        public ajaxFinance()
        {
            failed = false;
            voided = false;
            voidable = false;
            type = 1;
            id = -1;
        }
    }

    public class ajaxEmailActivity
    {
        public string name { get; set; }
        public string deepestEvent { get; set; }
        public DateTimeOffset date { get; set; }
        public string id { get; set; }
    }


    public class RegistrantSaveRequest
    {
        public Guid form { get; set; }
        public Guid company { get; set; }
        public RegistrationData data { get; set; }
    }

    public class FormData
    {
        public Guid Id { get; set; }
        public Guid Company { get; set; }
        public string Name { get; set; }
    }

    public class RegistrationDataRequest
    {
        public Guid Id { get; set; }
        public Guid Company { get; set; }
        public int Page { get; set; }
        public int RecordsPerPage { get; set; }
        public SortType SortType { get; set; }
        public FilterType FilterType { get; set; }
        public bool Live { get; set; }
    }

    public class RegistrationData
    {
        public Guid Id { get; set; }
        public string Confirmation { get; set; }
        public List<RegistrationKey> Data { get; set; }
        public string RegistrationDate { get; set; }

        public RegistrationData()
        {
            Data = new List<RegistrationKey>();
        }
    }

    public class RegistrationKey
    {
        public string Key { get; set; }
        public string Variable { get; set; }
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string Value { get; set; }
        public string ViewValue { get; set; }
    }

    public class SortType
    {
        public string SortColumn { get; set; }
        public bool Ascending { get; set; }
    }

    public class RegistrationInformation
    {
        public int TotalRecords { get; set; }
        public int TotalPages { get; set; }
    }

    public class FilterType
    {
        public string column { get; set; }
        public string value { get; set; }
    }

    public class FormInfo
    {
        public string Name { get; set; }
        public DateTimeOffset OpenDate { get; set; }
        public string DateString { get; set; }
        public Guid Id { get; set; }
        public string Description { get; set; }
        public int Status { get; set; }
    }

    #endregion

}