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
using RSToolKit.Domain.Entities.Reports;
using Newtonsoft.Json;
using RSToolKit.WebUI.Infrastructure.Engines;
using System.Text.RegularExpressions;

namespace RSToolKit.WebUI.Controllers
{
    [Authorize(Roles = "Super Administrators,Administrators,Cloud Users,Cloud+ Users")]
    public class GlobalReportController
        : RegStepController
    {
        /// <summary>
        /// The report currently being used.
        /// </summary>
        protected GlobalReport _report;

        /// <summary>
        /// Initializes the controller and sets up the repositories.
        /// </summary>
        public GlobalReportController()
            : base()
        {
            Log.LoggingMethod = "GlobalReportController";
        }

        [HttpGet]
        [ComplexId("id")]
        public ActionResult Index(object id)
        {
            var report = this._GetReport(id);
            if (!report.GetPermission(Context.SecuritySettings, SecurityAccessType.Write).HasAccess(SecurityAccessType.Write))
                throw new InsufficientPermissionsException();
            return View("Index", report);
        }

        [HttpGet]
        [ComplexId("id")]
        [ActionName("View")]
        public ActionResult ViewReport(object id)
        {
            this._GetReport(id);
            if (!this._report.GetPermission(Context.SecuritySettings, SecurityAccessType.Read).HasAccess(SecurityAccessType.Read))
                throw new InsufficientPermissionsException();
            if (Request.IsAjaxRequest())
            {
                Domain.Data.Info.Progress progress = null;
                var table = this._report.Execute(ref progress);
                if (progress.HardFail)
                    return JsonNetResult.Failure(location: Url.Action("Error", new { id = this._report.Id }), data: new { progress = progress });
                if (table == null)
                    return JsonNetResult.Failure(location: Url.Action("View", new { id = this._report.Id }), data: new { progress = progress });
                return JsonNetResult.Success(data: table);
            }
            else
            {
                ThreadPool.QueueUserWorkItem(LoadViewCallback, this._report.Id);
                return View("View", this._report);
            }
        }

        [HttpGet]
        public ActionResult List()
        {
            var reports = Context.GlobalReports.Where(c => c.CompanyKey == AppUser.WorkingCompanyKey).ToList();
            return View("List", reports);
        }

        [HttpPut]
        [IsAjax]
        [JsonValidateAntiForgeryToken]
        public JsonNetResult Put(GlobalReport model)
        {
            var report = Context.GlobalReports.Find(model.Id);
            if (report == null)
                throw new ReportNotFoundException();
            if (!report.GetPermission(Context.SecuritySettings, SecurityAccessType.Write).HasAccess(SecurityAccessType.Write))
                throw new InsufficientPermissionsException();
            report.Name = model.Name;
            report.Script = model.Script;
            var formIDs = new List<long>();
            foreach (Match match in Regex.Matches(model.Script, @"using ([^;]*);"))
            {
                var ids = match.Groups[1].Value.Split(' ');
                foreach (var id in ids)
                {
                    long lid;
                    if (long.TryParse(id.Trim(), out lid) && !formIDs.Contains(lid))
                        formIDs.Add(lid);
                }
            }
            report.Forms = formIDs;
            report.Favorite = model.Favorite;
            Domain.Data.Info.Progress progress = null;
            WebConstants.ProgressStati.TryRemove(report.Id + "_globalreport", out progress);
            report.ClearCache();
            Context.SaveChanges();
            return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { success = true } };
        }

        [HttpPost]
        [IsAjax]
        [JsonValidateAntiForgeryToken]
        public JsonNetResult New(string name = "New Inventory Report")
        {
            var report = new GlobalReport() {
                CompanyKey = AppUser.WorkingCompanyKey.Value,
                UId = Guid.NewGuid(),
                Name = name
            };
            var permission = new PermissionSet()
            {
                Target = report.UId,
                Owner = report.CompanyKey,
                Read = true,
                Write = true,
                Execute = true
            };
            Context.GlobalReports.Add(report);
            Context.PermissionsSets.Add(permission);
            Context.SaveChanges();
            return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { success = true, type = "url:" + Url.Action("Get", "GlobalReport", new { id = report.Id }) } };
        }

        [HttpDelete]
        [IsAjax]
        [JsonValidateAntiForgeryToken]
        [ComplexId("id")]
        public JsonNetResult Delete(object id)
        {
            var report = _GetReport(id);
            if (!this._report.GetPermission(Context.SecuritySettings, SecurityAccessType.Write).HasAccess(SecurityAccessType.Write))
                throw new InsufficientPermissionsException();
            report.ClearCache();
            Context.GlobalReports.Remove(report);
            Context.SaveChanges();
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
        public ActionResult Error(object id)
        {
            this._GetReport(id);
            Domain.Data.Info.Progress progress = null;
            WebConstants.ProgressStati.TryGetValue(this._report.Id + "_globalreport", out progress);
            if (progress == null)
                throw new ReportDataNotAttachedException();
            var html = this._report.Script;
            var error = progress.Exception.Message;
            var exception = progress.Exception as RegScriptException;
            if (exception != null)
            {
                html = html.Insert(exception.End, "</span>");
                html = html.Insert(exception.Start, "<span style=\"background-color: red; color: white;\">");
            }
            html = html.Replace(Environment.NewLine, "<br />");
            return View("Error", new Tuple<string, string>(error, html));
        }

        [HttpGet]
        [ComplexId("id")]
        public FileResult Download(object id)
        {
            if (!(id is long))
                throw new InvalidIdException("The id must be a number.");
            var report = Context.GlobalReports.Find((long)id);
            if (!report.GetPermission(Context.SecuritySettings, SecurityAccessType.Execute).HasAccess(SecurityAccessType.Execute))
                throw new InsufficientPermissionsException();
            var name = report.Name;
            Domain.Data.Info.Progress progress = null;
            var tables = report.Execute(ref progress);
            byte[] fileBytes;
            using (var package = new ExcelPackage())
            {
                var book = package.Workbook;
                book.Properties.Author = "RegStep Technologies";
                book.Properties.Title = "Inventory Report: " + name;
                foreach (var table in tables)
                {
                    book.Worksheets.Add(table.Name);
                    var sheet = book.Worksheets[table.Name];
                    if (table.Vertical)
                    {
                        sheet.Cells[1, 1].Value = "Label";
                        sheet.Cells[1, 2].Value = "Count";
                        sheet.Cells[1, 1, 1, 2].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        sheet.Cells[1, 1, 1, 2].Style.Fill.BackgroundColor.SetColor(WebConstants.Excel_HeaderColor);
                        sheet.Cells[1, 1, 1, 2].Style.Font.Color.SetColor(System.Drawing.Color.White);
                        var row = table.Rows.First();
                        var start = 2;
                        foreach (var value in row.Values)
                        {
                            if (start % 2 != 0)
                            {
                                sheet.Cells[start, 1, start, 2].Style.Fill.PatternType = ExcelFillStyle.Solid;
                                sheet.Cells[start, 1, start, 2].Style.Fill.BackgroundColor.SetColor(WebConstants.Excel_StripeColor);
                            }
                            sheet.Cells[start, 1].Value = value.HeaderValue;
                            sheet.Cells[start++, 2].Value = value.Value;
                        }
                    }
                    else
                    {
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
                                var value = row.Values.FirstOrDefault(v => v.HeaderValue == header.Value);
                                var val = value != null ? value.Value : "";
                                sheet.Cells[rowIndex, colIndex].Value = val;
                            }
                        }
                    }
                }
                fileBytes = package.GetAsByteArray();
            }
            Response.AddHeader("Content-Disposition", "inline; filename=" + name + ".xlsx");
            return new FileContentResult(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
        }

        public static void LoadViewCallback(object obj)
        {
            Domain.Data.Info.Progress progress = null;
            try
            {
                if (!(obj is long))
                    return;
                var id = (long)obj;
                using (var context = new EFDbContext())
                {
                    var report = context.GlobalReports.Find((long)id);
                    if (report == null)
                        return;
                    report.Execute(ref progress);
                }
            }
            catch (Exception e)
            {
                if (progress == null)
                    return;
                progress.HardFail = true;
                progress.Exception = e;
            }
        }

        /// <summary>
        /// Gets the <code>AdvancedInventoryReport</code> from the database.
        /// </summary>
        /// <param name="id">The id of the report.</param>
        /// <returns>The report located.</returns>
        protected GlobalReport _GetReport(object id)
        {
            if (id is Guid)
                this._report = Context.GlobalReports.FirstOrDefault(r => r.UId == (Guid)id);
            else if (id is long)
                this._report = Context.GlobalReports.Find((long)id);
            else
                throw new InvalidIdException();
            if (this._report == null)
                throw new RegStepException("The global report does not exist.");
            if (!this._report.GetPermission(Context.SecuritySettings, SecurityAccessType.Read).HasAccess(SecurityAccessType.Read))
                throw new InsufficientPermissionsException();
            return this._report;
        }
    }
}