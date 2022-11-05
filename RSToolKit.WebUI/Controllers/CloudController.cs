using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RSToolKit.WebUI.Infrastructure;
using RSToolKit.Domain.Data;
using RSToolKit.WebUI.Models;
using RSToolKit.Domain.Entities;
using RSToolKit.Domain.Entities;
using RSToolKit.Domain.Security;
using RSToolKit.Domain.Engines;
using RSToolKit.Domain.Entities.Components;
using RSToolKit.Domain;

namespace RSToolKit.WebUI.Controllers
{
    [Authorize(Roles = "Super Administrators,Administrators,Cloud Users,Cloud+ Users,Programmers")]
    [RegStepHandleError(typeof(InvalidIdException), "InvalidIdException")]
    public class CloudController
        : RegStepController
    {

        protected FormRepository _formRepo;
        protected InventoryReportRepository _invRepo;
        protected ReportDataRepository _repRep;
        protected FormsRepository Repository;
        protected RegistrantRepository _regRepo;

        #region Constructors
        public CloudController()
            : base()
        {
            this._formRepo = new FormRepository(Context);
            this._repRep = new ReportDataRepository(Context);
            this._invRepo = new InventoryReportRepository(Context);
            this._regRepo = new RegistrantRepository(Context);
            Repositories.Add(this._formRepo);
            Repositories.Add(this._repRep);
            Repositories.Add(this._invRepo);
            Repositories.Add(this._regRepo);
            Log.LoggingMethod = "CloudController";
            Repository = new FormsRepository(Context, AppUser, User);
        }
        #endregion

        [HttpGet]
        [Trail("Dashboard")]
        public ActionResult Index()
        {
            var model = new CloudIndex();
            var reports = this._repRep.Search(r => r.CompanyKey == WorkingCompany.SortingId && r.Favorite).ToList();
            var advReports = this._invRepo.Search(r => r.CompanyKey == WorkingCompany.UId && r.Favorite);
            model.LiveForms = this._formRepo.Search(f => (f.Status == FormStatus.Open || f.Status == FormStatus.Maintenance) && f.CompanyKey == WorkingCompany.UId).ToList();
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
            foreach (var gReport in Context.GlobalReports.Where(gr => gr.CompanyKey == AppUser.WorkingCompanyKey && gr.Favorite).ToList())
            {
                model.GlobalReports.Add(gReport);
            }
            model.FavoriteReports = favReports.OrderBy(t => t.Item1.Name).ThenBy(t => t.Item2.Name);
            return View(model);
        }

        [HttpGet]
        [IsAjax]
        public JsonNetResult Notifications()
        {
            var liveForms = this._formRepo.Search(f => (f.Status == FormStatus.Open || f.Status == FormStatus.Maintenance) && f.CompanyKey == WorkingCompany.UId).ToList();
            var notifications = new List<Notification>();
            foreach (var form in liveForms)
            {
                if (!Context.CanAccess(form, SecurityAccessType.Write))
                    continue;
                foreach (var reg in this._regRepo.Search(r => r.Type == RegistrationType.Live && r.FormKey == form.UId))
                {
                    if (reg.TotalOwed < 0)
                        notifications.Add(new Notification() { Message = "Refund due for " + reg.Form.Name + " to " + reg.Email + ".", Url = Url.Action("Get", "Registrant", new { id = reg.SortingId }), ToolTip = reg.TotalOwed.ToString("c", reg.Form.Culture) });
                }
                foreach (var seating in form.Seatings.ToList())
                {
                    if (seating.RawSeats > 0 && seating.HasWaiters)
                        notifications.Add(new Notification() { Message = "Seatings available for: " + seating.Form.Name + " on seating " + seating.Name + ".", ToolTip = "Seatings available.", Url = @Url.Action("CapacityLimit", new { id = seating.UId }) });
                }
            }
            return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { success = true, notifications = notifications } };
        }

        [HttpGet]
        [IsAjax]
        [ComplexId("id")]
        public JsonNetResult FormStatistics(object id)
        {
            Form form = null;
            if (id is Guid)
                form = this._formRepo.Find((Guid)id);
            else if (id is long)
                form = this._formRepo.First(f => f.SortingId == (long)id);
            else
                throw new InvalidIdException();
            var sales = 0.00m;
            var collected = 0.00m;
            var outstanding = 0.00m;
            var adjustments = 0.00m;
            var invitationSends = 0;
            var invitationOpens = 0;
            var invitationBounces = 0;
            var emails = form.AllEmails.Where(e => e.EmailType == RSToolKit.Domain.Entities.Email.EmailType.Invitation).ToList();
            var recipients = new HashSet<string>();
            foreach (var email in emails)
            {
                foreach (var send in Context.EmailSends.Where(s => s.EmailKey == email.UId).ToList())
                {
                    if (recipients.Contains(send.Recipient.ToLower()))
                    {
                        continue;
                    }
                    recipients.Add(send.Recipient.ToLower());
                    var topEvent = send.TopEvent;
                    if (send.EmailEvents.Count(s => s.Event == "Delivered") > 0)
                    {
                        invitationSends++;
                    }
                    if (send.EmailEvents.Count(s => s.Event == "Opened") > 0)
                    {
                        invitationOpens++;
                    }
                    if (send.EmailEvents.Count(s => s.Event == "Permanent Bounce") > 0)
                    {
                        invitationBounces++;
                    }
                }
            }
            foreach (var registrant in form.Registrants.Where(r => r.Type == RSToolKit.Domain.Entities.RegistrationType.Live && (r.Status == RSToolKit.Domain.Entities.RegistrationStatus.CanceledByCompany || r.Status == RSToolKit.Domain.Entities.RegistrationStatus.Canceled || r.Status == RSToolKit.Domain.Entities.RegistrationStatus.CanceledByAdministrator || r.Status == RSToolKit.Domain.Entities.RegistrationStatus.Submitted)).OrderBy(r => r.Confirmation))
            {
                sales += registrant.Fees;
                collected += registrant.Transactions;
                outstanding += registrant.Fees - registrant.Transactions + registrant.Adjustings;
                adjustments += registrant.Adjustings;
            }
            return new JsonNetResult()
            {
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                Data = new
                {
                    success = true,
                    sales = sales.ToString("c", form.Culture),
                    collected = collected.ToString("c", form.Culture),
                    outstanding = outstanding.ToString("c", form.Culture),
                    adjustments = adjustments.ToString("c", form.Culture),
                    invitationSends = invitationSends,
                    invitationOpens = invitationOpens,
                    invitationBounces = invitationBounces,
                    registrations = form.Registrations(),
                    activeRegistrations = form.ActiveRegistrations(),
                    cancelledRegistrations = form.CanceledRegistrations(),
                    id = form.SortingId
                }
            };
        }

        /*
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
            var table = report.GetReportData(Repository, paging);
            return View("PrintIReport", table);
        }
        //*/

    }
}