using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using RSToolKit.Domain.Entities;
using RSToolKit.WebUI.Infrastructure;
using RSToolKit.Domain.Data;
using RSToolKit.Domain.Exceptions;
using Newtonsoft.Json;
using System.Threading;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.IO;
using RSToolKit.Domain;
using RSToolKit.WebUI.Models.Views.Capacity;

namespace RSToolKit.WebUI.Controllers
{
    public class CapacityController
        : RegStepController
    {
        protected CapacityRepository _capRepo;
        protected FormRepository _formRepo;
        protected Seating _capacity;

        #region Constructors
        public CapacityController()
            : base()
        {
            this._capRepo = new CapacityRepository(Context);
            Repositories.Add(this._capRepo);
            this._formRepo = new FormRepository(Context);
            Repositories.Add(this._formRepo);
            Log.LoggingMethod = "CloudController";
            this._capacity = null;
        }

        public CapacityController(EFDbContext context)
            : base(context)
        {
            this._capRepo = new CapacityRepository(Context);
            Repositories.Add(this._capRepo);
            this._formRepo = new FormRepository(Context);
            Repositories.Add(this._formRepo);
            Log.LoggingMethod = "CloudController";
            this._capacity = null;
        }
        #endregion

        [HttpGet]
        [ComplexId("id")]
        public ActionResult Index(object id)
        {
            var capacity = this._GetCapacity(id);
            if (capacity == null)
                throw new InvalidIdException("You must supply a valid capacity id.");
            UpdateTrailLabel("Capacity Limit: " + capacity.Name);
            ViewBag.CanEdit = Context.CanAccess(capacity, Domain.Security.SecurityAccessType.Write, true);
            return View("Index", capacity);
        }

        [HttpGet]
        [ComplexId("id")]
        public ActionResult Get(object id)
        {
            // First we need to see if the object is a capacity limit.
            var capacity = this._GetCapacity(id);
            if (capacity != null)
                return Index(id);
            var form = this._GetForm(id);
            if (form != null)
            {
                UpdateTrailLabel(form.Name + ": Capacity Limits");
                var model = new CapacitiesView(form);
                foreach (var cap in form.Seatings)
                {
                    if (Context.CanAccess(cap, Domain.Security.SecurityAccessType.Write, true))
                        model.Capacities.First(c => c.Id == cap.SortingId).CanWrite = true;
                }
                return View("Capacities", model);
            }
            throw new InvalidIdException("You must supply either a form or a capacity id that is valid.");
        }

        [HttpPut]
        [ComplexId("id")]
        [JsonValidateAntiForgeryToken]
        public JsonNetResult Promote(object id)
        {
            var seater = this._GetSeater(id);
            if (seater.Seated)
                return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { success = false, message = "The registrant is already seated." } };
            this._Promote(seater);
            Context.SaveChanges();
            return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { success = true } };
        }

        [HttpPut]
        [ComplexId("id")]
        [JsonValidateAntiForgeryToken]
        public JsonNetResult AutoManage(object id)
        {
            var seating = this._GetCapacity(id);
            var waiters = seating.Seaters.Where(s => !s.Seated).OrderBy(s => s.Date).ToList();
            var w_index = 0;
            var w_max = waiters.Count;
            while (seating.RawSeats > 0)
            {
                var waiter = waiters[w_index];
                if (w_index < w_max)
                {
                    this._Promote(waiter);
                }
            }
            Context.SaveChanges();
            return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = true } };
        }

        [HttpGet]
        [ComplexId("id")]
        public FileResult Download(object id)
        {
            this._GetCapacity(id);
            if (this._capacity == null)
                return null;
            byte[] fileBytes;
            using (var package = new ExcelPackage())
            {
                var book = package.Workbook;
                book.Properties.Author = "RegStep Technologies";
                book.Properties.Title = "Capacity Report: " + this._capacity.Name;
                book.Worksheets.Add("Overview");
                book.Worksheets.Add("Seated");
                book.Worksheets.Add("Waitlisted");
                var overviewSheet = book.Worksheets["Overview"];
                overviewSheet.Cells[1, 1].Value = "Seated";
                overviewSheet.Cells[1, 2].Value = this._capacity.Seated.Count().ToString();
                overviewSheet.Cells[2, 1].Value = "Max Seats";
                overviewSheet.Cells[2, 2].Value = this._capacity.MaxSeats == -1 ? "Unlimited Seats" : this._capacity.MaxSeats.ToString();
                overviewSheet.Cells[3, 1].Value = "Available Seats";
                overviewSheet.Cells[3, 2].Value = this._capacity.MaxSeats == -1 ? "Unlimited Seats" : this._capacity.AvailableSeats.ToString();
                overviewSheet.Cells[4, 1].Value = "Waitlistings";
                overviewSheet.Cells[4, 2].Value = this._capacity.Waitlistable ? this._capacity.Waiters.ToString() : "Not Waitlistable";
                var seatedSheet = book.Worksheets["Seated"];
                seatedSheet.Cells[1, 1, 1, 6].Style.Fill.PatternType = ExcelFillStyle.Solid;
                seatedSheet.Cells[1, 1, 1, 6].Style.Fill.BackgroundColor.SetColor(WebConstants.Excel_HeaderColor);
                seatedSheet.Cells[1, 1, 1, 6].Style.Font.Color.SetColor(System.Drawing.Color.White);
                var seatedBorder = seatedSheet.Cells[1, 1, 1, 6].Style.Border;
                seatedBorder.Bottom.Style = seatedBorder.Top.Style = seatedBorder.Left.Style = seatedBorder.Right.Style = ExcelBorderStyle.Thin;
                seatedSheet.Cells[1, 1].Value = "Confirmation Number";
                seatedSheet.Cells[1, 2].Value = "Email";
                seatedSheet.Cells[1, 3].Value = "Name";
                seatedSheet.Cells[1, 4].Value = "Component";
                seatedSheet.Cells[1, 5].Value = "Date Seated";
                seatedSheet.Cells[1, 6].Value = "Registration Status";
                var row = 1;
                foreach (var seat in this._capacity.Seated.OrderBy(s => s.Component.LabelText).ThenBy(s => s.DateSeated))
                {
                    row++;
                    if (row % 2 != 0)
                    {
                        seatedSheet.Cells[row, 1, row, 6].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        seatedSheet.Cells[row, 1, row, 6].Style.Fill.BackgroundColor.SetColor(WebConstants.Excel_StripeColor);
                    }
                    seatedSheet.Cells[row, 1].Value = seat.Registrant.Confirmation;
                    seatedSheet.Cells[row, 2].Value = seat.Registrant.Email;
                    seatedSheet.Cells[row, 3].Value = seat.Registrant.FullName;
                    seatedSheet.Cells[row, 4].Value = seat.Component.LabelText;
                    seatedSheet.Cells[row, 5].Value = seat.DateSeated.UserDateString(AppUser);
                    seatedSheet.Cells[row, 6].Value = seat.Registrant.Status.GetStringValue();
                }

                var waitSheet = book.Worksheets["Waitlisted"];
                waitSheet.Cells[1, 1, 1, 6].Style.Fill.PatternType = ExcelFillStyle.Solid;
                waitSheet.Cells[1, 1, 1, 6].Style.Fill.BackgroundColor.SetColor(WebConstants.Excel_HeaderColor);
                waitSheet.Cells[1, 1, 1, 6].Style.Font.Color.SetColor(System.Drawing.Color.White);
                var waitBorder = waitSheet.Cells[1, 1, 1, 6].Style.Border;
                waitBorder.Bottom.Style = waitBorder.Top.Style = waitBorder.Left.Style = waitBorder.Right.Style = ExcelBorderStyle.Thin;
                waitSheet.Cells[1, 1].Value = "Confirmation Number";
                waitSheet.Cells[1, 2].Value = "Email";
                waitSheet.Cells[1, 3].Value = "Name";
                waitSheet.Cells[1, 4].Value = "Component";
                waitSheet.Cells[1, 5].Value = "Date Waitlisted";
                waitSheet.Cells[1, 6].Value = "Registration Status";
                row = 1;
                foreach (var seat in this._capacity.Waited.OrderBy(s => s.Component.LabelText).ThenBy(s => s.DateSeated))
                {
                    row++;
                    if (row % 2 != 0)
                    {
                        waitSheet.Cells[row, 1, row, 6].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        waitSheet.Cells[row, 1, row, 6].Style.Fill.BackgroundColor.SetColor(WebConstants.Excel_StripeColor);
                    }
                    waitSheet.Cells[row, 1].Value = seat.Registrant.Confirmation;
                    waitSheet.Cells[row, 2].Value = seat.Registrant.Email;
                    waitSheet.Cells[row, 3].Value = seat.Registrant.FullName;
                    waitSheet.Cells[row, 4].Value = seat.Component.LabelText;
                    waitSheet.Cells[row, 5].Value = seat.DateSeated.UserDateString(AppUser);
                    waitSheet.Cells[row, 6].Value = seat.Registrant.Status.GetStringValue();
                }
                fileBytes = package.GetAsByteArray();
            }
            Response.AddHeader("Content-Disposition", "inline; filename=" + this._capacity.Name + ".xlsx");
            return new FileContentResult(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
        }

        /// <summary>
        /// Gets the form from the database.
        /// </summary>
        /// <param name="id">The id of the form as either a <code>Guid</code> or <code>long</code>.</param>
        /// <returns></returns>
        protected Form _GetForm(object id)
        {
            Form form = null;
            if (id is Guid)
                form = this._formRepo.Search(f => f.UId == (Guid)id).FirstOrDefault();
            else if (id is long)
                form = this._formRepo.Search(f => f.SortingId == (long)id).FirstOrDefault();
            else
                throw new FormNotFoundException();
            return form;
        }

        /// <summary>
        /// Gets the capacity from the database.
        /// </summary>
        /// <param name="id">The id of the capacity as either a <code>Guid</code> or <code>long</code>.</param>
        /// <returns></returns>
        protected Seating _GetCapacity(object id)
        {
            if (id is Guid)
                this._capacity = this._capRepo.Search(s => s.UId == (Guid)id).FirstOrDefault();
            else if (id is long)
                this._capacity = this._capRepo.Search(s => s.SortingId == (long)id).FirstOrDefault();
            else
                throw new InvalidIdException();
            return this._capacity;
        }

        /// <summary>
        /// Gets the seater from the database.
        /// </summary>
        /// <param name="id">The id of the seater object.</param>
        /// <returns>The seater found.</returns>
        protected Seater _GetSeater(object id)
        {
            if (id is Guid)
                return this._capRepo.FindSeater((Guid)id);
            else if (id is long)
                return this._capRepo.FindSeater((long)id);
            throw new InvalidIdException();
        }

        /// <summary>
        /// Updates the registrant in the loaded table information.
        /// </summary>
        /// <param name="id">The unique identifier of the registrant.</param>
        protected void _UpdateRegistrantInTable(Guid id)
        {
            ThreadPool.QueueUserWorkItem(FormReportController.UpdateTableRegistrantCallback, id);
        }

        /// <summary>
        /// Promotes the seater to seated.
        /// </summary>
        /// <param name="seater">The seater to promote.</param>
        protected void _Promote(Seater seater)
        {
            seater.Seated = true;
            seater.DateSeated = DateTimeOffset.UtcNow;
            // Add the value to the registrant data.
            var component = seater.Component;
            if (seater.Component is IComponentItem)
                component = ((IComponentItem)seater.Component).Parent;
            var datapoint = seater.Registrant.Data.FirstOrDefault(d => d.VariableUId == component.UId);
            if (datapoint == null)
            {
                datapoint = new RegistrantData()
                {
                    RegistrantKey = seater.Registrant.UId,
                    Registrant = seater.Registrant,
                    Component = component,
                    VariableUId = component.UId,
                    UId = Guid.NewGuid()
                };
            }
            if (component is IComponentMultipleSelection)
            {
                var icms = (IComponentMultipleSelection)component;
                var selections = JsonConvert.DeserializeObject<List<Guid>>(datapoint.Value);
                // If there is a time exclusion, we need to remove those that clash with the new item
                var items = icms.Children.ToList();
                if (icms.TimeExclusion)
                {
                    var pickedItem = seater.Component;
                    // If time exclusion is set, we need to check it here in case javascript was turned off.
                    var t_items = new List<IComponentItem>();
                    items.ForEach(i => { if (selections.Contains(i.UId)) t_items.Add(i); });
                    var collidedItems = new List<IComponentItem>();
                    foreach (var selection in selections)
                        // Here we check each item against the other.
                        collidedItems.AddRange(t_items.Where(i => i.UId != pickedItem.UId && i.AgendaEnd >= pickedItem.AgendaStart && i.AgendaEnd <= pickedItem.AgendaEnd).ToList());
                    foreach (var collided in collidedItems)
                    {
                        // If the collided item we are removing has seating, we need to give up that seat as well.
                        if (collided.Seating != null)
                        {
                            var o_seating = collided.Seating.Seaters.FirstOrDefault(s => s.RegistrantKey == seater.RegistrantKey && s.ComponentKey == collided.UId);
                            if (o_seating != null)
                            {
                                // Delete old seating.
                                Context.RemoveSeater(o_seating);
                            }
                        }
                        t_items.Remove(collided);
                    }
                    selections = t_items.Select(i => i.UId).ToList();
                }
                // We add the new item
                if (!selections.Contains(seater.ComponentKey))
                    selections.Add(seater.ComponentKey);
                datapoint.Value = JsonConvert.SerializeObject(selections);
            }
            else if (component is IComponentItemParent)
            {
                // We just replace the value since only one value is allowed.  We need to check if the old value was tied to a seating though.
                var oldKey = Guid.Empty;
                if (Guid.TryParse(datapoint.Value, out oldKey))
                {
                    var oldComponent = Context.Components.Find(oldKey);
                    if (oldComponent != null)
                    {
                        if (oldComponent.Seating != null)
                        {
                            var o_seating = oldComponent.Seating.Seaters.FirstOrDefault(s => s.RegistrantKey == seater.RegistrantKey && s.ComponentKey == oldKey);
                            if (o_seating != null)
                            {
                                // Delete old seating.
                                Context.RemoveSeater(o_seating);
                            }
                        }
                    }
                }
                // Update the value
                datapoint.Value = seater.ComponentKey.ToString();
            }
            this._UpdateRegistrantInTable(seater.Registrant.UId);
        }

    }
}