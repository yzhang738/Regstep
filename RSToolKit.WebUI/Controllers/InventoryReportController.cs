using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RSToolKit.WebUI.Infrastructure;
using RSToolKit.Domain.Data;
using RSToolKit.Domain.Entities;
using RSToolKit.Domain.Exceptions;
using RSToolKit.Domain.Security;
using System.Threading;
using RSToolKit.Domain.Collections;
using RSToolKit.Domain.JItems;
using OfficeOpenXml;
using OfficeOpenXml.Style;

namespace RSToolKit.WebUI.Controllers
{
    public class InventoryReportController
        : RegStepController
    {
        /// <summary>
        /// The repository for manipulating invitation reports.
        /// </summary>
        protected InventoryReportRepository _repRepo;
        /// <summary>
        /// The repository for manipulating forms.
        /// </summary>
        protected FormRepository _formRepo;
        /// <summary>
        /// The tokens.
        /// </summary>
        protected TokenDictionary<TableInformation> _tokens;
        /// <summary>
        /// The report currently being used.
        /// </summary>
        protected AdvancedInventoryReport _report;

        /// <summary>
        /// Initializes the controller and sets up the repositories.
        /// </summary>
        public InventoryReportController()
            : base()
        {
            this._repRepo = new InventoryReportRepository(Context);
            Repositories.Add(this._repRepo);
            this._formRepo = new FormRepository(Context);
            Repositories.Add(this._formRepo);
            Log.LoggingMethod = "InventoryReportController";
            this._tokens = JsonTables.GetTokens(JsonTokenType.InventoryReport);
        }

        [HttpGet]
        [ComplexId("id")]
        public ActionResult Index(object id)
        {
            var report = this._GetReport(id);
            return View("Index", report);
        }

        [HttpGet]
        [ComplexId("id")]
        public ActionResult View(object id)
        {
            if (Request.IsAjaxRequest())
            {
                JsonTableToken<TableInformation> token = null; 
                if (id is long)
                {
                    // There is no token yet.
                    var table = this._GetTable(id);
                    token = this._tokens.Create(this._report.UId);
                    return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { success = false, retry = true, progress = 0, message = "Gathering data.", token = token.Key } };
                }
                else
                {
                    token = this._tokens.Get((Guid)id);
                    if (token == null)
                        // The token does not exist.
                        throw new TokenNotFoundException();
                    if (token.Item == null)
                        // The item isn't set, we need to set it.
                        token.Item = this._tokens.GetInfo(token.ItemKey);
                    if (token.Item == null)
                        // The item still is null.
                        throw new TokenNotFoundException("The token does not have a table yet.");
                    var progress = token.GetProgress();
                    if (progress.Progress < 0)
                    {
                        this._tokens.RemoveInfo(token.ItemKey);
                        throw new RegStepException(progress.Message + ": " + progress.Details);
                    }
                    if (progress.Complete)
                        return JsonNetResult.Success(data: this._CreateJsonPage(token));
                    return JsonNetResult.Failure(message: "The table is still processing", data: progress);
                }
            }
            this._GetReport(id);
            return View("View", this._report);
        }

        [HttpGet]
        [ComplexId("id")]
        public ActionResult List(object id)
        {
            Form form = null;
            if (id is Guid)
                form = this._formRepo.Find((Guid)id);
            else if (id is long)
                form = this._formRepo.First(f => f.SortingId == (long)id);
            else
                throw new InvalidIdException();
            ViewBag.FormKey = form.UId;
            var reports = this._repRepo.Search(r => r.FormKey == form.UId);
            return View("List", reports);
        }

        [HttpPut]
        [IsAjax]
        [JsonValidateAntiForgeryToken]
        public JsonNetResult Put(AdvancedInventoryReport model)
        {
            var report = this._repRepo.Find(model.UId);
            if (report == null)
                throw new ReportNotFoundException();
            report.Name = model.Name;
            report.Favorite = model.Favorite;
            report.Script = model.Script;
            this._repRepo.Commit();
            return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { success = true } };
        }

        [HttpPost]
        [IsAjax]
        [JsonValidateAntiForgeryToken]
        public JsonNetResult New(Guid id, string name = "New Inventory Report")
        {
            var form = this._formRepo.Find(id);
            if (form == null)
                throw new ReportNotFoundException();
            var report = new AdvancedInventoryReport() {
                Form = form,
                FormKey = form.UId,
                Company = form.Company,
                CompanyKey = form.CompanyKey,
                UId = Guid.NewGuid(),
                Name = name
            };
            this._repRepo.Add(report);
            this._repRepo.Commit();
            Context.PermissionsSets.AddRange(SecuritySettings.CreateDefaultPermissions(report));
            Context.SaveChanges();
            return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { success = true, type = "url:" + Url.Action("Get", "InventoryReport", new { id = report.SortingId }) } };
        }

        [HttpDelete]
        [IsAjax]
        [JsonValidateAntiForgeryToken]
        [ComplexId("id")]
        public JsonNetResult Delete(object id)
        {
            var report = _GetReport(id);
            this._repRepo.Remove(report);
            this._repRepo.Commit();
            return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { success = true, type = "refresh" } };
        }

        [HttpGet]
        [ComplexId("id")]
        public ActionResult Get(object id)
        {
            return Index(id);
        }

        [HttpGet]
        [ComplexId("id")]
        public FileResult CreateXcel(object id)
        {
            var table = _GetTable(id);
            var name = Context.AdvancedInventoryReports.Where(a => a.UId == table.Key).Select(a => a.Name).First();
            byte[] fileBytes;
            using (var package = new ExcelPackage())
            {
                var book = package.Workbook;
                book.Properties.Author = "RegStep Technologies";
                book.Properties.Title = "Inventory Report: " + name;
                book.Worksheets.Add("Report");
                var sheet = book.Worksheets["Report"];
                var headerIndex = 0;
                foreach (var header in table.Headers)
                {
                    ++headerIndex;
                    sheet.Cells[1, headerIndex].Value = header.Value;
                }
                sheet.Cells[1, 1, 1, headerIndex].Style.Fill.PatternType = ExcelFillStyle.Solid;
                sheet.Cells[1, 1, 1, headerIndex].Style.Fill.BackgroundColor.SetColor(WebConstants.Excel_HeaderColor);
                sheet.Cells[1, 1, 1, headerIndex].Style.Font.Color.SetColor(System.Drawing.Color.White);
                var rowIndex = 1;
                foreach (var row in table.Rows)
                {
                    rowIndex++;
                    var colIndex = 0;
                    if (rowIndex % 2 != 0)
                    {
                        sheet.Cells[rowIndex, 1, rowIndex, headerIndex].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        sheet.Cells[rowIndex, 1, rowIndex, headerIndex].Style.Fill.BackgroundColor.SetColor(WebConstants.Excel_StripeColor);
                    }
                    foreach (var header in table.Headers)
                    {
                        colIndex++;
                        var value = row.Values.FirstOrDefault(v => v.HeaderId == header.Id);
                        var val = value != null ? value.Value : "";
                        sheet.Cells[rowIndex, colIndex].Value = val;
                    }
                }
                fileBytes = package.GetAsByteArray();
            }
            Response.AddHeader("Content-Disposition", "inline; filename=" + name + ".xlsx");
            return new FileContentResult(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
        }


        /// <summary>
        /// Gets the <code>AdvancedInventoryReport</code> from the database.
        /// </summary>
        /// <param name="id">The id of the report.</param>
        /// <returns>The report located.</returns>
        protected AdvancedInventoryReport _GetReport(object id)
        {
            if (id is Guid)
                this._report = this._repRepo.Find((Guid)id);
            else if (id is long)
                this._report = this._repRepo.First(r => r.SortingId == (long)id);
            else
                throw new InvalidIdException();
            return this._report;
        }

        /// <summary>
        /// Gets the table from the token dictionary.
        /// </summary>
        /// <param name="id">The id of the advanced report as either long or Guid.</param>
        /// <returns>The table information.</returns>
        protected TableInformation _GetTable(object id)
        {
            this._GetReport(id);
            var table = this._tokens.GetInfo(this._report.UId);
            /*
            if (table != null)
                return table;
            //*/
            ThreadPool.QueueUserWorkItem(GetTableCallback, id);
            return null;
        }

        /// <summary>
        /// A thread callback for generating the report.
        /// </summary>
        /// <param name="id">The id of the form as a long or Guid.</param>
        protected void GetTableCallback(object id)
        {
            using (var context = new EFDbContext())
            {
                var tokens = JsonTables.GetTokens(JsonTokenType.InventoryReport);
                AdvancedInventoryReport report = null;
                if (id is Guid)
                    report = context.AdvancedInventoryReports.Find((Guid)id);
                else if (id is long)
                    report = context.AdvancedInventoryReports.FirstOrDefault(f => f.SortingId == (long)id);
                if (report == null)
                    return;
                try
                {
                    report.GetTableInformation(tokens);
                }
                catch (Exception e)
                {
                    var table = tokens.GetInfo(report.UId);
                    if (table != null)
                        table.Fail(e.Message);
                }
           }
        }

        /// <summary>
        /// Creates a <code>JsonTable</code> of paged data based on the inormation specified in <paramref name="token"/>
        /// </summary>
        /// <param name="token">The token of the user table data.</param>
        /// <returns>A <code>JsonTable</code> with a single page of data.</returns>
        protected JsonTable _CreateJsonPage(JsonTableToken<TableInformation> token, bool all = false)
        {
            var page = new JsonTable()
            {
                Page = 1,
                Headers = token.Item.Headers,
                Id = token.ItemKey,
                Token = token.Key,
                Sortings = token.Sortings,
                Key = token.Key,
                Name = "Invitation Report",
                Rows = token.Item.Rows,
            };
            return page;
        }
    }
}