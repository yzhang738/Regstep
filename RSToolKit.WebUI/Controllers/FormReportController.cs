using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RSToolKit.WebUI.Infrastructure;
using RSToolKit.Domain.Exceptions;
using RSToolKit.Domain.Data;
using RSToolKit.Domain.Entities;
using System.Threading.Tasks;
using RSToolKit.Domain.JItems;
using RSToolKit.Domain.Security;
using System.Collections.Concurrent;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using Newtonsoft.Json;
using System.Drawing;
using System.IO;
using RSToolKit.Domain;
using RSToolKit.Domain.Collections;
using RSToolKit.Domain.Logging;
using System.Text.RegularExpressions;
using System.Threading;
using RSToolKit.Domain.Data.Info;

namespace RSToolKit.WebUI.Controllers
{
    [Authorize(Roles = "Super Administrators,Administrators,Cloud Users,Cloud+ Users,Programmers")]
    [RegStepHandleError(typeof(InvalidIdException), "InvalidIdException")]
    [RegStepHandleError(typeof(ReportDataNotFoundException), "ReportDataNotFound")]
    [RegStepHandleError(typeof(InsufficientPermissionsException), "InsufficientPermissionsException")]
    [RegStepHandleError(typeof(ReportDataNotAttachedException), "ReportDataNotAttached")]
    public class FormReportController
        : RegStepController
    {

        protected ReportDataRepository _repRepo = null;
        protected FormRepository _formRepo = null;
        protected bool _canEdit = false;
        protected Token _token = null;
        protected TokenData _data = null;
        protected Progress _progress = null;
        protected JsonTable _page = null;
        /*
        public static KeepAliveDictionary<PagedReportData> st_ReportDatas = new KeepAliveDictionary<PagedReportData>();
        public static ComplexDictionary<JsonTableInformation> st_TableRows = new ComplexDictionary<JsonTableInformation>();
        //*/
        protected Color _excelHeaderColor = Color.FromArgb(51, 51, 51);
        protected Color _excelStripeColor = Color.FromArgb(221, 221, 221);

        /// <summary>
        /// Initializes the controller with a supplied context.
        /// </summary>
        public FormReportController(EFDbContext context)
            : base(context)
        {
            this._repRepo = new ReportDataRepository(context);
            this._formRepo = new FormRepository(context);
            Repositories.Add(_repRepo);
            Log.LoggingMethod = "FormReportController";
        }

        /// <summary>
        /// Initializes the controller with a new context.
        /// </summary>
        public FormReportController()
            : base()
        {
            this._repRepo = new ReportDataRepository(Context);
            this._formRepo = new FormRepository(Context);
            Repositories.Add(_repRepo);
            Log.LoggingMethod = "FormReportController";
        }

        // GET: FormReports
        [HttpGet]
        [ComplexId("id")]
        public ActionResult Index(object id, int page = 1)
        {
            if (Request.IsAjaxRequest())
            {
                // We are in an ajax call so we look for a tokenized report.
                if (!(id is Guid))
                    throw new InvalidIdException("You must present a Guid token to get data through an Ajax call.");
                this._RetrieveByToken((Guid)id);
                if (this._progress == null)
                    return JsonNetResult.Success(data: new { progress = new Progress() });
                if (!this._progress.Complete)
                    // The json table isn't ready, we send back a failure repsonse with retry set to true and the progress.
                    return JsonNetResult.Success(data: new { progress = this._progress });
                // We get the page and return it.
                this._canEdit = Context._CanAccess(this._token.Key, AppUser.WorkingCompanyKey.Value, SecurityAccessType.Write, true);
                var jsonPage = this._CreatePage(page);
                return JsonNetResult.Success(data: new { progress = this._progress, table = jsonPage, canEdit = this._canEdit });
                //return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { success = true, table = jsonPage, canEdit = this._canEdit } };
            }
            else
            {
                // This is a normal request. We serve up the html template and the subsequent ajax call takes care of the rest.
                if (id is long && (long)id == -1)
                {
                    // The user is requesting a list of all the reports available.
                    var reports = new List<Tuple<Form, List<object>>>();
                    var forms = this._formRepo.Search(f => f.CompanyKey == WorkingCompany.UId).ToList();
                    var globalForms = Context.GlobalReports.Where(f => f.CompanyKey == WorkingCompany.UId).ToList();
                    foreach (var form in forms)
                    {
                        var formData = new Tuple<Form, List<object>>(form, new List<object>());
                        formData.Item2.AddRange(_repRepo.Search(r => r.TableId == form.UId).OrderBy(r => r.Name).ToList());
                        formData.Item2.AddRange(globalForms.Where(r => r.Forms.Contains(form.SortingId)));
                        reports.Add(formData);
                    }
                    UpdateTrailLabel(WorkingCompany.Name + ": Form Reports");
                    return View("FormReports", reports.OrderBy(r => r.Item1.Name).ToList());
                }
                Form table = null;
                ReportData report = null;
                // First thing we do is see if this is a report that has previously been saved.
                if (id is long)
                {
                    try
                    {
                        report = this._repRepo.Find((long)id);
                        table = this._formRepo.Find(report.TableId);
                    }
                    catch (Exception)
                    { }
                }
                if (report == null)
                {
                    // It wasn't a report, so we need to look for a IJsonTable in the database.
                    if (!(id is Guid))
                        // We didn't get a guid, so we can't do anything.
                        throw new InvalidIdException("The id did not apply to a saved report and was not a Guid. It must be a guid of a form if not pertaining to a saved report.");
                    // We need to look for the jTableProcessor in the database
                    // We will first check for a form.
                    table = this._formRepo.Find((Guid)id);
                }
                if (table == null)
                    // There was none, we need send back a error.
                    throw new ReportDataNotAttachedException();
                return View("Report", new Tuple<Form, ReportData>(table, report));
            }
        }

        [HttpGet]
        [ComplexId("id")]
        public ActionResult Get(object id, int page = 1)
        {
            return Index(id, page);
        }

        [HttpGet]
        [ActionName("SavedReports")]
        public async Task<ActionResult> GetSavedReports(Guid id)
        {
            this._RetrieveByToken(id);
            var reports = await this._repRepo.SearchAsync(rd => rd.TableId == this._data.DataKey && rd.Type == ReportType.Form);
            return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { success = true, files = reports.Select(s => new { id = s.SortingId, name = s.Name }).OrderBy(s => s.name).ToList() } };
        }

        [HttpGet]
        public ActionResult Permissions(Guid id)
        {
            this._RetrieveByToken(id);
            if (!this._token.IsReport)
                throw new ReportDataNotAttachedException("The tokenized report is not attached to a saved report.");
            var report = this._repRepo.Find(this._token.SavedReportId);
            if (report == null)
                throw new ReportDataNotAttachedException("The tokenized report is not attached to a saved report. Save will not work. Try Save As instead.");
            return RedirectToAction("Permissions", "Security", new { id = report.UId });
        }

        #region CRUD

        [HttpPost]
        [JsonValidateAntiForgeryToken]
        public JsonNetResult SaveAs(Guid token, string name, string saveId = "")
        {
            this._RetrieveByToken(token);
            long lid;
            if (long.TryParse(saveId, out lid))
            {
                // An id was supplied. We need to grab that report and overwrite it.
                var report = this._repRepo.Find(lid);
                if (report.Name == name)
                {
                    report.SetHeaders(this._token.Headers);
                    report.SetFilters(this._token.Filters);
                    report.SetSortings(this._token.Sortings);
                    report.Type = ReportType.Form;
                    this._token.SavedReportId = report.SortingId;
                    Context.SaveChanges();
                    return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { success = true, id = report.SortingId, name = report.Name } };
                }
            }
            var form = this._formRepo.Find(this._token.Key);
            // no save id was supplied, so we create a new report.
            var newReport = new ReportData();
            newReport.UId = Guid.NewGuid();
            newReport.TableId = this._token.Key;
            newReport.Name = name;
            newReport.SetHeaders(this._token.Headers);
            newReport.SetFilters(this._token.Filters);
            newReport.SetSortings(this._token.Sortings);
            newReport.RecordsPerPage = this._token.RecordsPerPage;
            newReport.CompanyKey = form.Company.SortingId;
            newReport.Type = ReportType.Form;
            var permissionSet = new PermissionSet()
            {
                Owner = form.Company.UId,
                Execute = true,
                Read = true,
                Target = newReport.UId,
                Write = true
            };
            Context.PermissionsSets.Add(permissionSet);
            Context.ReportData.Add(newReport);
            Context.SaveChanges();
            this._token.SavedReportId = newReport.SortingId;
            return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { success = true, id = newReport.SortingId, name = newReport.Name } };
        }

        [HttpPut]
        [JsonValidateAntiForgeryToken]
        public JsonNetResult Save(Guid token)
        {
            this._RetrieveByToken(token);
            if (!this._token.IsReport)
                throw new ReportDataNotAttachedException("The tokenized report is not attached to a saved report. Save will not work. Try Save As instead.");
            var report = this._repRepo.Find(this._token.SavedReportId);
            if (report == null)
                throw new ReportDataNotAttachedException("The tokenized report is not attached to a saved report. Save will not work. Try Save As instead.");

            report.SetFilters(this._token.Filters);
            report.SetSortings(this._token.Sortings);
            report.SetHeaders(this._token.Headers);
            report.RecordsPerPage = this._token.RecordsPerPage;
            report.Type = ReportType.Form;
            this._repRepo.Commit();
            return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { success = true } };
        }

        [HttpDelete]
        [JsonValidateAntiForgeryToken]
        public JsonNetResult Delete(Guid token)
        {
            this._RetrieveByToken(token);
            if (!this._token.IsReport)
                throw new ReportDataNotAttachedException("The tokenized report is not attached to a saved report. Delete will not work.");
            var report = this._repRepo.Find(this._token.SavedReportId);
            if (report == null)
                throw new ReportDataNotAttachedException("The tokenized report is not attached to a saved report. Delete will not work.");
            this._repRepo.Remove(report);
            this._repRepo.Commit();
            return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { success = true, id = this._token.Key } };
        }

        [HttpPost]
        [JsonValidateAntiForgeryToken]
        [ComplexId("id")]
        public JsonNetResult NewToken(object id)
        {
            this._token = new Token()
            {
                Id = Guid.NewGuid()
            };
            HttpContext.Server.ScriptTimeout = 1200;
            // Create the variable to hold the table.
            ReportData report = null;
            Guid? tableId = null;
            // First thing we do is see if this is a report that has previously been saved.
            if (id is long)
            {
                // The id is a long then it must be referencing a report data.
                report = this._repRepo.Find((long)id);
                // We set the table id.
                tableId = report.TableId;
            }
            if (report == null)
            {
                // It wasn't a report, so we need to look for a IJsonTable in the database.
                if (!(id is Guid))
                    // We didn't get a guid, so we can't do anything.
                    throw new InvalidIdException("Guid not supplied and id did not correspond to a saved table.");
                // We need to look for the jTableProcessor in the database
                // We will first check for a form.
                tableId = id as Guid?;
            }
            else
            {
                this._token.Filters = report.GetFilters();
                this._token.Headers = report.GetHeaders();
                this._token.Sortings = report.GetSortings();
                this._token.RecordsPerPage = (int)report.RecordsPerPage;
                this._token.Name = report.Name;
                this._token.IsReport = true;
                this._token.SavedReportId = report.SortingId;
                this._token.Favorite = report.Favorite;
            }
            if (!tableId.HasValue)
                throw new FormNotFoundException("The form that is being referenced could not be found in the database.");

            // We need to grab table data.

            var form = Context.Forms.Where(f => f.UId == tableId.Value).FirstOrDefault();
            if (form == null)
                throw new FormNotFoundException("The form that is being referenced could not be found in the database.");

            this._token.Key = tableId.Value;
            Context.Tokens.Add(this._token);
            Context.SaveChanges();

            var liveString = form.Status.In(FormStatus.Open, FormStatus.Maintenance, FormStatus.PaymentComplete, FormStatus.Closed) ? "-live" : "-test";
            this._data = Context.TokenData.Where(d => d.DataKey == this._token.Key && d.Discriminator == "formreport" + liveString).FirstOrDefault();
            var pKey = form.UId.ToString() + "_formreport" + liveString;
            if (this._data == null)
            {
                // The table is not yet loaded.
                this._LoadData(tableId.Value, pKey);
            }
            return JsonNetResult.Success(location: Url.Action("Get", "FormReport", new { id = this._token.Id }, Request.Url.Scheme), data: new { token = this._token.Id });
        }

        [HttpPost]
        [JsonValidateAntiForgeryToken]
        public JsonNetResult Refresh(long id)
        {
            var form = Context.Forms.Where(f => f.SortingId == id).FirstOrDefault();
            if (form == null)
                return JsonNetResult.Failure();
            var formStatus = form.Status;
            var liveString = formStatus.In(FormStatus.Complete, FormStatus.Open, FormStatus.PaymentComplete, FormStatus.Maintenance, FormStatus.Closed, FormStatus.Archive) ? "-live" : "-test";
            var p_key = form.UId + "_formreport" + liveString;
            var data = Context.TokenData.Where(d => d.DataKey == form.UId && d.Discriminator == "formreport" + liveString).FirstOrDefault();
            if (data == null)
                return JsonNetResult.Failure();
            Context.TokenData.Remove(data);
            Context.SaveChanges();
            var t_p = new Progress();
            WebConstants.ProgressStati.TryRemove(p_key, out t_p);
            return JsonNetResult.Success(location: Url.Action("Get", "FormReport", new { id = form.UId }, Request.Url.Scheme));
        }

        #endregion

        #region Update Reports

        [HttpPut]
        [JsonValidateAntiForgeryToken]
        public JsonNetResult Favorite(Guid token)
        {
            this._RetrieveByToken(token);
            if (!this._token.IsReport)
                throw new ReportDataNotAttachedException("The tokenized report was not attached to a saved report. Favorite will not work.");
            var report = _repRepo.Find(this._token.SavedReportId);
            report.Favorite = !report.Favorite;
            Context.SaveChanges();
            return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { success = true, favorite = report.Favorite } };
        }

        [HttpPut]
        [JsonValidateAntiForgeryToken]
        public JsonNetResult Filters(Guid token, List<JsonFilter> filters)
        {
            this._RetrieveByToken(token);
            this._token.Filters = filters ?? new List<JsonFilter>();
            Context.SaveChanges();
            var jsonPage = this._CreatePage(1);
            return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { success = true, table = jsonPage } };
        }

        [HttpPut]
        [JsonValidateAntiForgeryToken]
        public JsonNetResult Sortings(Guid token, List<JsonSorting> sortings)
        {
            this._RetrieveByToken(token);
            this._token.Sortings = sortings ?? new List<JsonSorting>();
            Context.SaveChanges();
            var jsonPage = this._CreatePage(1);
            return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { success = true, table = jsonPage } };
        }

        [HttpPut]
        [JsonValidateAntiForgeryToken]
        public JsonNetResult Fields(Guid token, List<string> headers)
        {
            this._RetrieveByToken(token);
            this._token.Headers = headers ?? new List<string>();
            Context.SaveChanges();
            var jsonPage = this._CreatePage(1);
            return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { success = true, table = jsonPage } };
        }

        [HttpPut]
        [JsonValidateAntiForgeryToken]
        public JsonNetResult RecordsPerPage(Guid token, int recordsPerPage)
        {
            this._RetrieveByToken(token);
            this._token.RecordsPerPage = recordsPerPage;
            Context.SaveChanges();
            var jsonPage = this._CreatePage(1);
            return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { success = true, table = jsonPage } };
        }

        #endregion

        [HttpGet]
        public FileResult Download(Guid token)
        {
            this._RetrieveByToken(token);
            var tableInfo = Context.Forms.Where(f => f.UId == this._token.Key).Select(f => new { Name = f.Name, Culture = f.CultureString }).FirstOrDefault();
            System.Globalization.CultureInfo culture = new System.Globalization.CultureInfo(tableInfo.Culture);
            var jTable = _CreatePage(-1);
            var jsonTableRows = jTable.Rows;
            var usingRows = jsonTableRows.ToList();
            using (var package = new ExcelPackage())
            {
                var book = package.Workbook;
                book.Properties.Author = "RegStep Technologies";
                book.Properties.Title = "Report: " + tableInfo.Name;
                book.Worksheets.Add("Data");
                var sheet = book.Worksheets["Data"];
                var h_count = 0;
                var headers = new List<JsonTableHeader>(jTable.Headers.Where(h => !h.Hidden));
                foreach (var header in headers)
                {
                    h_count++;
                    sheet.Cells[1, h_count].Value = header.Value;
                }
                sheet.Cells[1, 1, 1, h_count].Style.Fill.PatternType = ExcelFillStyle.Solid;
                sheet.Cells[1, 1, 1, h_count].Style.Fill.BackgroundColor.SetColor(this._excelHeaderColor);
                sheet.Cells[1, 1, 1, h_count].Style.Font.Color.SetColor(System.Drawing.Color.White);
                var border = sheet.Cells[1, 1, 1, h_count].Style.Border;
                border.Bottom.Style = border.Top.Style = border.Left.Style = border.Right.Style = ExcelBorderStyle.Thin;
                var r_count = 1;
                foreach (var row in usingRows)
                {
                    r_count++;
                    if (r_count % 2 != 0)
                    {
                        sheet.Cells[r_count, 1, r_count, h_count].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        sheet.Cells[r_count, 1, r_count, h_count].Style.Fill.BackgroundColor.SetColor(this._excelStripeColor);
                    }
                    var r_contactCount = 0;
                    foreach (var header in headers)
                    {
                        var value = row.Values.FirstOrDefault(v => v.HeaderId == header.Id);
                        var displayValue = value.Value;
                        if (value == null)
                            displayValue = "";
                        else
                        {
                            if (value.HeaderId.ToLower().In("balance", "fees", "transactions", "adjustments", "tax"))
                            {
                                decimal t_amount;
                                if (decimal.TryParse(displayValue, out t_amount))
                                    displayValue = t_amount.ToString("c", culture);
                            }
                        }
                        displayValue = Regex.Replace(displayValue, @"<br\s?/>", Environment.NewLine);
                        sheet.Cells[r_count, ++r_contactCount].Value = displayValue;
                    }
                }
                var fileId = Guid.NewGuid();
                var fileResult = new FileContentResult(package.GetAsByteArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
                fileResult.FileDownloadName = "RegStepReport-" + this._token.Name + "-" + DateTimeOffset.Now.ToEpoch() + ".xlsx";
                return fileResult;
            }
        }

        #region Supporting Methods

        /// <summary>
        /// Retrieves the token data by the user token.
        /// </summary>
        /// <param name="key">The token of the token data.</param>
        /// <returns>The token data.</returns>
        protected bool _RetrieveByToken(Guid key)
        {
            this._token = Context.Tokens.Find(key);
            if (this._token == null)
                throw new TokenNotFoundException();
            // Now that we found the token. We need to get the table data.
            var formStatus = Context.Forms.Where(f => f.UId == this._token.Key).Select(f => f.Status).First();
            var liveString = formStatus.In(FormStatus.Complete, FormStatus.Open, FormStatus.PaymentComplete, FormStatus.Maintenance, FormStatus.Closed, FormStatus.Archive) ? "-live" : "-test";
            var p_key = this._token.Key + "_formreport" + liveString;
            // Lets see if there is a status object associated with the item.
            if (WebConstants.ProgressStati.ContainsKey(this._token.Key + "_formreport" + liveString))
            {
                // There was a progress.
                this._progress = WebConstants.ProgressStati[p_key];
                this._data = Context.TokenData.Where(d => d.DataKey == this._token.Key && d.Discriminator == "formreport" + liveString).FirstOrDefault();
                if (this._progress.Complete && DateTimeOffset.Now.Subtract(this._data.LastAccess) > TimeSpan.FromHours(6))
                {
                    // The item was last accessed more than six hours ago, lets update it.
                    this._LoadData(this._token.Key, p_key);
                }
            }
            else
            {
                // There was no progress, so we must load it.
                this._LoadData(this._token.Key, p_key);
            }
            return true;
        }

        /// <summary>
        /// Loads the data into the database. If it already exists, then it overwrite it.
        /// </summary>
        /// <param name="key">The id of the form.</param>
        protected void _LoadData(Guid key, string pKey)
        {
            // We need to see if there is already a progress.
            if (WebConstants.ProgressStati.TryGetValue(pKey, out this._progress))
            {
                // There was a progress. We need to see if it was a hard fault.
                if (this._progress.HardFail)
                    // The report is a hard fail, we don't try to load it again.
                    return;
                // The report is still being loaded. We don't mess with it.
                return;
            }
            ThreadPool.QueueUserWorkItem(LoadDataCallback, new object[] { key, pKey });
        }

        /// <summary>
        /// Loads the data into the database.
        /// </summary>
        /// <param name="key">The id of the form.</param>
        public static async void LoadDataCallback(object obj)
        {
            if (!(obj is object[]))
                // The parameter is wrong.
                return;
            object[] param = obj as object[];
            if (!(param[0] is Guid) || !(param[1] is string))
                // params are invalid.
                return;
            var key = (Guid)param[0];
            var pKey = (string)param[1];

            Progress progress = null;
            if (WebConstants.ProgressStati.TryGetValue(pKey, out progress))
            {
                // There was a progress.
                if (progress.HardFail)
                    // it is a hard fail. We just return;
                    return;
                if (!progress.Complete)
                    // It is still loading. We just return;
                    return;
                if (progress.Failed)
                    // It is a simple fail. We reset and continue;
                    progress.Reset();
            }
            else
            {
                // No progress found, so we create it and drive on.
                progress = new Progress(pKey);
                WebConstants.ProgressStati.TryAdd(pKey, progress);
            }

            // Now we grab the database information.
            using (var formsContext = new EFDbContext())
            using (var dataContext = new EFDbContext())
            {
                var form = formsContext.Forms.Find(key);
                if (form == null)
                    return;
                var liveString = form.Status.In(FormStatus.Complete, FormStatus.Open, FormStatus.PaymentComplete, FormStatus.Maintenance, FormStatus.Closed, FormStatus.Archive) ? "-live" : "-test";
                var data = dataContext.TokenData.Where(d => d.DataKey == key && d.Discriminator == ("formreport" + liveString)).FirstOrDefault();
                if (data == null)
                {
                    // No data so we create it.
                    data = new TokenData()
                    {
                        DataKey = key,
                        Discriminator = "formreport" + liveString,
                        Data = null,
                        LastAccess = DateTimeOffset.Now
                    };
                    dataContext.TokenData.Add(data);
                }
                // Now we reset the last access and null the data out.
                data.LastAccess = DateTimeOffset.Now;
                data.Data = null;
                dataContext.SaveChanges();
                var tableData = await form.GetRegistrationJson(progress);
                if (tableData == null)
                    return;
                data.Data = JsonConvert.SerializeObject(tableData);
                dataContext.SaveChanges();
                progress.Complete = true;
            }
        }

        protected JsonTable _CreatePage(int pageNumber)
        {
            if (this._data == null || this._data.Data == null)
                return new JsonTable();
            var data = JsonConvert.DeserializeObject<TableData>(this._data.Data);
            var page = new JsonTable()
            {
                RecordsPerPage = this._token.RecordsPerPage,
                Page = pageNumber,
                Count = false,
                Graph = false,
                Average = false,
                Name = this._token.Name,
                Sortings = this._token.Sortings,
                Filters = this._token.Filters,
                Favorite = this._token.Favorite,
                Headers = data.Headers.DeepClone(),
                Id = this._token.Id,
                SavedReport = this._token.IsReport,
                SavedReportId = this._token.SavedReportId,
                Token = this._token.Id,
            };

            #region Filters

            List<JsonTableRow> rows = this._token.Filters.Count() > 0 ? new List<JsonTableRow>() : data.Rows.ToList();
            if (this._token.Filters.Count() > 0)
            {
                for (var i = 0; i < data.Rows.Count; i++)
                {
                    var take = false;
                    var grouping = true;
                    var tests = new List<Tuple<bool, string>>();
                    var row = data.Rows[i];
                    for (var j = 0; j < this._token.Filters.Count; j++)
                    {
                        var filter = this._token.Filters[j];
                        var header = data.Headers.First(h => h.Id == filter.ActingOn);
                        var groupTest = true;
                        var first = true;
                        var test = false;
                        grouping = true;
                        do
                        {
                            if (!filter.GroupNext)
                                grouping = false;
                            test = row.TestValue(header, filter.Value, filter.Equality, false);
                            switch (filter.LinkedBy)
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
                            if (j < this._token.Filters.Count)
                                filter = this._token.Filters[j];
                            else
                                break;
                        } while (grouping);
                        tests.Add(new Tuple<bool, string>(groupTest, j < (this._token.Filters.Count - 1) ? this._token.Filters[j + 1].LinkedBy : "none"));
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
                        rows.Add(row);
                    }
                }
            }
            page.Rows = rows;


            #endregion

            #region Headers

            // Hiding headers not using.
            page.Headers.ForEach(h => h.Hidden = false);
            if (this._token.Headers.Count > 0)
                page.Headers.Where(h => !h.Id.In(this._token.Headers)).ToList().ForEach(h => h.Hidden = true);

            // Sort headers.
            var orderedHeaders = new List<JsonTableHeader>();
            foreach (var hId in this._token.Headers.Reverse<string>())
            {
                var header = page.Headers.First(h => h.Id == hId);
                page.Headers.MoveToFront(header);
            }

            #endregion

            #region Sorting Registrants

            IOrderedEnumerable<JsonTableRow> oRows;
            if (this._token.Sortings.Count > 0)
            {
                var sort = this._token.Sortings.First();
                if (sort.Ascending)
                    oRows = page.Rows.OrderBy(r => r.GetSortValue(sort.Id, data.Headers.First(h => h.Id == sort.Id)));
                else
                    oRows = page.Rows.OrderByDescending(r => r.GetSortValue(sort.Id, data.Headers.First(h => h.Id == sort.Id)));
                for (var s_i = 1; s_i < this._token.Sortings.Count; s_i++)
                {
                    var n_sort = this._token.Sortings[s_i];
                    if (n_sort.Ascending)
                        oRows = oRows.ThenBy(r => r.GetSortValue(n_sort.Id, data.Headers.First(h => h.Id == sort.Id)));
                    else
                        oRows = oRows.ThenByDescending(r => r.GetSortValue(n_sort.Id, data.Headers.First(h => h.Id == sort.Id)));
                }
            }
            else
            {
                var sort = new JsonSorting() { Id = "Email", Ascending = true };
                if (sort.Ascending)
                    oRows = page.Rows.OrderBy(r => r.GetSortValue(sort.Id, data.Headers.First(h => h.Id == sort.Id)));
                else
                    oRows = page.Rows.OrderByDescending(r => r.GetSortValue(sort.Id, data.Headers.First(h => h.Id == sort.Id)));
            }
            page.Rows = oRows.ToList();

            #endregion

            page.TotalRecords = page.Rows.Count;

            if (pageNumber == -1)
            {
                page.TotalPages = 1;
                page.Page = 1;
                return page;
            }

            page.TotalPages = page.TotalRecords / page.RecordsPerPage + (page.TotalRecords % page.RecordsPerPage > 0 ? 1 : 0);
            page.Rows = page.Rows.Skip((int)(page.RecordsPerPage * (page.Page - 1))).Take((int)page.RecordsPerPage).ToList();
            return page;
        }

        /// <summary>
        /// Updates the registrant in the <code>JsonTableInformation</code>.
        /// </summary>
        /// <param name="regId">The id of the registrant to update.</param>
        public static void UpdateTableRegistrant(Guid regId)
        {
            using (var context = new EFDbContext())
            {
                var registrant = context.Registrants.Find(regId);
                var liveString = registrant.Type == RegistrationType.Live ? "-live" : "-test";
                var data = context.TokenData.Where(da => da.DataKey == registrant.FormKey && da.Discriminator == ("formreport" + liveString)).FirstOrDefault();
                if (data == null)
                    return;

                // There was a table, so we need to update that row in the table.
                var d = JsonConvert.DeserializeObject<TableData>(data.Data);
                var row = d.Rows.FirstOrDefault(r => r.Id == registrant.SortingId);
                // There was a table, so we need to update that row in the table.
                if (row != null)
                {
                    // The row existed, we need clear the values currently there.
                    if (registrant.DateModified < row.DateModified)
                        return;
                    row.Values.Clear();
                    row.ModificationToken = registrant.ModificationToken;
                    row.DateModified = registrant.DateModified;
                }
                else
                {
                    // The row did not exists so we need to add it.
                    row = new JsonTableRow() { Id = registrant.SortingId, Token = registrant.UId };
                    d.Rows.Add(row);
                }
                // Now that we have the row, we need to add back in the updated values.
                foreach (var header in d.Headers)
                    row.Values.Add(registrant.GetJsonTableValue(header));
                data.Data = JsonConvert.SerializeObject(d);
                context.SaveChanges();
            }
            return;
        }

        /// <summary>
        /// Updates the registrant in the <code>JsonTableInformation</code>.
        /// </summary>
        /// <param name="id">The id of the registrant to update. It must be a unique identifier.</param>
        public static void UpdateTableRegistrantCallback(object id)
        {
            if (!(id is Guid))
                return;
            var regId = (Guid)id;
            using (var context = new EFDbContext())
            {
                var registrant = context.Registrants.Find(regId);
                var liveString = registrant.Type == RegistrationType.Live ? "-live" : "-test";
                var data = context.TokenData.Where(da => da.DataKey == registrant.FormKey && da.Discriminator == ("formreport" + liveString)).FirstOrDefault();
                if (data == null)
                    return;

                // There was a table, so we need to update that row in the table.
                TableData d;
                try
                {
                    d = JsonConvert.DeserializeObject<TableData>(data.Data);
                }
                catch (Exception)
                {
                    return;
                }
                var row = d.Rows.FirstOrDefault(r => r.Id == registrant.SortingId);
                // There was a table, so we need to update that row in the table.
                if (row != null)
                {
                    // The row existed, we need clear the values currently there.
                    if (registrant.DateModified < row.DateModified)
                        return;
                    row.Values.Clear();
                    row.ModificationToken = registrant.ModificationToken;
                    row.DateModified = registrant.DateModified;
                }
                else
                {
                    // The row did not exists so we need to add it.
                    row = new JsonTableRow() { Id = registrant.SortingId, Token = registrant.UId };
                    d.Rows.Add(row);
                }
                // Now that we have the row, we need to add back in the updated values.
                foreach (var header in d.Headers)
                    row.Values.Add(registrant.GetJsonTableValue(header));
                data.Data = JsonConvert.SerializeObject(d);
                context.SaveChanges();
            }
            return;
        }

        /// <summary>
        /// Clears the table of registrations.
        /// </summary>
        /// <param name="id">The id of the table.</param>
        public static void ClearTableCallback(object id)
        {
            if (!(id is Guid))
                return;
            var formId = (Guid)id;
            using (var context = new EFDbContext())
            {
                var data = context.TokenData.Where(da => da.DataKey == formId).FirstOrDefault();
                if (data == null)
                    return;
                var d = JsonConvert.DeserializeObject<TableData>(data.Data);
                d.Rows.Clear();
                data.Data = JsonConvert.SerializeObject(d);
                context.SaveChanges();
            }
        }

        /// <summary>
        /// Removes registrant from the table.
        /// </summary>
        /// <param name="id">The id of the registrant.</param>
        public static void RemoveTableRegistrantCallback(object state)
        {
            // We need to make sure the state passed is an array of objects.
            if (!(state is object[]))
                return;
            var args = state as object[];
            // It needs to have 2 items.
            if (args.Length != 2)
                return;
            // The first item needs to be a long
            if (!(args[0] is long))
                return;
            // The second item needs to be a Guid.
            if (!(args[1] is Guid))
                return;
            var token = (Guid)args[1];
            var id = (long)args[0];
            using (var context = new EFDbContext())
            {
                var data = context.TokenData.Where(da => da.DataKey == token).FirstOrDefault();
                if (data == null)
                    return;
                var d = JsonConvert.DeserializeObject<TableData>(data.Data);
                // There was a table, so we need to update that row in the table.
                var row = d.Rows.FirstOrDefault(r => r.Id == id);
                if (row != null)
                    d.Rows.Remove(row);
                data.Data = JsonConvert.SerializeObject(d);
                context.SaveChanges();
            }
        }

        /// <summary>
        /// Updates the registrant in the <code>JsonTableInformation</code>.
        /// </summary>
        /// <param name="regId">The id of the registrant to update.</param>
        public static void RemoveTableRegistrant(Guid regId)
        {
            using (var context = new EFDbContext())
            {
                var registrant = context.Registrants.Find(regId);
                var liveString = registrant.Type == RegistrationType.Live ? "-live" : "-test";
                var data = context.TokenData.Where(da => da.DataKey == registrant.FormKey && da.Discriminator == ("formreport" + liveString)).FirstOrDefault();
                if (data == null)
                    return;
                var d = JsonConvert.DeserializeObject<TableData>(data.Data);
                // There was a table, so we need to update that row in the table.
                var row = d.Rows.FirstOrDefault(r => r.Id == registrant.SortingId);
                if (row != null)
                    d.Rows.Remove(row);
                data.Data = JsonConvert.SerializeObject(d);
                context.SaveChanges();
            }
        }


        /*
        /// <summary>
        /// Finds the data in memory or creates it if it isn't there.
        /// </summary>
        /// <param name="id">The id of the form to use.</param>
        /// <returns>The information.</returns>
        protected _TableData _GetTableData(Guid id)
        {
            Form form = this._formRepo.Find(id);
            var table = st_TableRows.Get(id);
            // We need to ensure that the table is there and that it is the correct status. If it is not, we need to reload it.
            if (table == null)
                LoadFormReport(id);
            if (table != null)
            {
                if (table.Options["status"] == "test" && !form.Status.In(FormStatus.Developement, FormStatus.Ready))
                {
                    st_TableRows.Remove(table);
                    table = null;
                    LoadFormReport(id);
                }
                else if (table.Options["status"] == "live" && form.Status.In(FormStatus.Developement, FormStatus.Ready))
                {
                    st_TableRows.Remove(table);
                    table = null;
                    LoadFormReport(id);
                }
            }
            return new _TableData(form, table);
        }

        /// <summary>
        /// Finds the data in memory or creates it if it isn't there.
        /// </summary>
        /// <param name="token">The token of the <code>PagedReportData</code>.</param>
        /// <returns>The information.</returns>
        protected _TableData _GetTableDataByToken(Guid token)
        {
            var paged = st_ReportDatas.Get(token);
            if (paged == null)
                throw new ReportDataNotFoundException("The report with token " + token.ToString() + " was not found in memory.");
            Form form = this._formRepo.Find(paged.Table);
            if (form == null)
                throw new FormNotFoundException("The form attached to this tokenized report cannot be found.");
            ReportData report = null;
            if (paged.SavedReport)
                report = this._repRepo.Find(paged.ReportDataId);
            var table = st_TableRows.Get(form.UId);
            if (table == null)
                LoadFormReport(form.UId);
            if (table != null)
            {
                if (table.Options["status"] == "test" && !form.Status.In(FormStatus.Developement, FormStatus.Ready))
                {
                    st_TableRows.Remove(table);
                    table = null;
                    LoadFormReport(form.UId);
                }
                else if (table.Options["status"] == "live" && form.Status.In(FormStatus.Developement, FormStatus.Ready))
                {
                    st_TableRows.Remove(table);
                    table = null;
                    LoadFormReport(form.UId);
                }
            }
            return new _TableData(form, table, paged, report);
        }

        /// <summary>
        /// Finds the data in the database.
        /// </summary>
        /// <param name="id">The id of the <code>ReportData</code>.</param>
        /// <returns>The report data.</returns>
        protected ReportData _GetReportData(long id)
        {
            return this._repRepo.Find(id);
        }

        /// <summary>
        /// Filters the table with the specified filters in the <paramref name="paged"/>.
        /// The <paramref name="paged"/> parameters is upadated with the necesary information.
        /// </summary>
        /// <param name="paged">The <code>PagedReportData</code> that holds the desired information.</param>
        /// <returns>An <code>ICollection</code> of <code>JsonTableRow</code> that was stored into <paramref name="paged"/>.</returns>
        protected List<JsonTableRow> _FilterTable(PagedReportData paged)
        {
            var table = st_TableRows.Get(paged.Table);
            if (table == null)
                throw new ReportDataNotFoundException("The table data was not found in memory.");
            List<JsonTableRow> rows = paged.Filters.Count() > 0 ? new List<JsonTableRow>() : table.Rows.ToList();
            if (paged.Filters.Count() == 0)
                return rows;
            for (var i = 0; i < table.Rows.Count(); i++)
            {
                var take = false;
                var grouping = true;
                var tests = new List<Tuple<bool, string>>();
                var row = table.Rows.ElementAt(i);
                for (var j = 0; j < paged.Filters.Count(); j++)
                {
                    var filter = paged.Filters.ElementAt(j);
                    var groupTest = true;
                    var first = true;
                    var test = false;
                    grouping = true;
                    do
                    {
                        if (!filter.GroupNext)
                            grouping = false;
                        test = row.TestValue(filter.ActingOn, filter.Value, filter.Equality, false);
                        switch (filter.LinkedBy)
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
                        if (j < paged.Filters.Count)
                            filter = paged.Filters[j];
                        else
                            break;
                    } while (grouping);
                    tests.Add(new Tuple<bool, string>(groupTest, j < (paged.Filters.Count - 1) ? paged.Filters[j + 1].LinkedBy : "none"));
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
                    rows.Add(row);
                }
            }
            return rows;
        }

        /// <summary>
        /// Adds the table headers specified in <paramref name="paged"/>.
        /// The <paramref name="paged"/> parameters is upadated with the necesary information.
        /// The <paramref name="paged"/> is also sorted.
        /// </summary>
        /// <param name="paged">The <code>PagedReportData</code> that holds the desired information.</param>
        /// <returns>An <code>ICollection</code> of <code>JsonTableRow</code> that was stored into <paramref name="paged"/>.</returns>
        protected List<JsonTableRow> _TableHeaders(PagedReportData paged, ref List<JsonTableRow> pagedRows)
        {
            var table = st_TableRows.Get(paged.Table);
            if (table == null)
                throw new ReportDataNotFoundException("The table data was not found in memory.");
            var rows = new List<JsonTableRow>();
            foreach (var row in table.Rows.Intersect(pagedRows))
            {
                var newRow = new JsonTableRow()
                {
                    Id = row.Id,
                    Editable = row.Editable,
                    Token = row.Token,
                };
                rows.Add(newRow);
                var values = new List<JsonTableValue>();
                foreach (var header in paged.UsingHeaders)
                    values.Add(row.Values.First(v => v.HeaderId == header.Id));
                newRow.Values = values;
            }
            pagedRows = rows;
            _SortTable(paged, ref pagedRows);
            return pagedRows;
        }

        /// <summary>
        /// Sorts the table with the specified sortings in the <paramref name="paged"/>.
        /// The <paramref name="paged"/> parameters is upadated with the necesary information.
        /// </summary>
        /// <param name="paged">The <code>PagedReportData</code> that holds the desired information.</param>
        /// <returns>An <code>ICollection</code> of <code>JsonTableRow</code> that was stored into <paramref name="paged"/>.</returns>
        protected List<JsonTableRow> _SortTable(PagedReportData paged, ref List<JsonTableRow> pagedRows)
        {
            IOrderedEnumerable<JsonTableRow> rows;
            if (paged.Sortings.Count() > 0)
            {
                var sort = paged.Sortings.First();
                if (sort.Ascending)
                    rows = pagedRows.OrderBy(r => r.GetSortValue(sort.Id));
                else
                    rows = pagedRows.OrderByDescending(r => r.GetSortValue(sort.Id));
                for (var s_i = 1; s_i < paged.Sortings.Count(); s_i++)
                {
                    var n_sort = paged.Sortings.ElementAt(s_i);
                    if (n_sort.Ascending)
                        rows = rows.ThenBy(r => r.GetSortValue(n_sort.Id));
                    else
                        rows = rows.ThenByDescending(r => r.GetSortValue(n_sort.Id));
                }
            }
            else
            {
                var sort = new JsonSorting() { Id = "email", Ascending = true };
                if (sort.Ascending)
                    rows = pagedRows.OrderBy(r => r.GetSortValue(sort.Id));
                else
                    rows = pagedRows.OrderByDescending(r => r.GetSortValue(sort.Id));
            }
            pagedRows = rows.ToList();
            return pagedRows;
        }

        /// <summary>
        /// Creates a <code>JsonTable</code> of paged data based on the inormation specified in <paramref name="paged"/>
        /// </summary>
        /// <param name="paged">The information to use to create the paged data.</param>
        /// <param name="includeAllRows">Set to true to include all the rows in the form.</param>
        /// <returns>A <code>JsonTable</code> with a single page of data.</returns>
        protected JsonTable _CreateJsonPage(PagedReportData paged, bool includeAllRows = false)
        {
            var info = _GetTableDataByToken(paged.Token);
            var page = new JsonTable()
            {
                RecordsPerPage = paged.RecordsPerPage,
                Page = paged.Page,
                Count = paged.Count,
                Graph = paged.Graph,
                Average = paged.Average,
                Name = paged.Name ?? "Registrants",
                Sortings = paged.Sortings,
                Filters = paged.Filters,
                Favorite = paged.Favorite,
                Headers = paged.UsingHeaders,
                Id = paged.Token,
                SavedReport = paged.SavedReport,
                SavedReportId = paged.ReportDataId,
                Token = paged.Token
            };
            page.Id = paged.Token;
            var rows = _FilterTable(info.TokenizedReport);
            _TableHeaders(info.TokenizedReport, ref rows);
            if (includeAllRows)
            {
                page.RecordsPerPage = rows.Count;
                page.Rows = rows;
            }
            var totalRecords = info.Table.Rows.Count;
            page.TotalPages = totalRecords / page.RecordsPerPage + (totalRecords % page.RecordsPerPage > 0 ? 1 : 0);
            page.TotalRecords = totalRecords;
            if (!includeAllRows)
                page.Rows = rows.Skip((int)(paged.RecordsPerPage * (paged.Page - 1))).Take((int)paged.RecordsPerPage).ToList();
            else
                page.Rows = rows;
            return page;
        }

        /// <summary>
        /// Creates a <code>JsonTable</code> of paged data based on the inormation specified in <paramref name="paged"/>
        /// </summary>
        /// <param name="info">The _TableData info to use.</param>
        /// <param name="includeAllRows">Set to true to include all the rows in the form.</param>
        /// <returns>A <code>JsonTable</code> with a single page of data.</returns>
        protected JsonTable _CreateJsonPage(_TableData info, bool includeAllRows = false)
        {
            this._canEdit = Context.CanAccess(info.Form, SecurityAccessType.Write, true);
            var page = new JsonTable()
            {
                RecordsPerPage = info.TokenizedReport.RecordsPerPage,
                Page = info.TokenizedReport.Page,
                Count = info.TokenizedReport.Count,
                Graph = info.TokenizedReport.Graph,
                Average = info.TokenizedReport.Average,
                Name = info.TokenizedReport.Name ?? "Registrants",
                Sortings = info.TokenizedReport.Sortings,
                Filters = info.TokenizedReport.Filters,
                Favorite = info.TokenizedReport.Favorite,
                Headers = info.TokenizedReport.UsingHeaders,
                Id = info.TokenizedReport.Token,
                SavedReport = info.TokenizedReport.SavedReport,
                SavedReportId = info.TokenizedReport.ReportDataId,
                Token = info.TokenizedReport.Token
            };
            page.Id = info.TokenizedReport.Token;
            var rows = _FilterTable(info.TokenizedReport);
            _TableHeaders(info.TokenizedReport, ref rows);
            if (includeAllRows)
            {
                page.RecordsPerPage = rows.Count;
                page.Rows = rows;
            }
            var totalRecords = rows.Count;
            page.TotalPages = totalRecords / page.RecordsPerPage + (totalRecords % page.RecordsPerPage > 0 ? 1 : 0);
            page.TotalRecords = totalRecords;
            page.Rows = rows.Skip((int)(info.TokenizedReport.RecordsPerPage * (info.TokenizedReport.Page - 1))).Take((int)info.TokenizedReport.RecordsPerPage).ToList();
            return page;
        }

        /// <summary>
        /// Updates the registrant in the <code>JsonTableInformation</code>.
        /// </summary>
        /// <param name="regId">The id of the registrant to update.</param>
        public static void UpdateTableRegistrant(Guid regId)
        {
            lock (st_TableRows.SyncRoot)
            {
                using (var context = new EFDbContext())
                {
                    var registrant = context.Registrants.Find(regId);
                    var table = st_TableRows.Get(registrant.FormKey);
                    if (table != null)
                    {
                        // There was a table, so we need to update that row in the table.
                        var row = table.Rows.FirstOrDefault(r => r.Id == registrant.SortingId);
                        if (row != null)
                        {
                            // The row existed, we need to update the rows.
                            row.Values.Clear();
                            foreach (var header in table.Headers)
                                row.Values.Add(registrant.GetJsonTableValue(header));
                            table.ModificationToken = Guid.NewGuid();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Updates the registrant in the <code>JsonTableInformation</code>.
        /// </summary>
        /// <param name="id">The id of the registrant to update. It must be a unique identifier.</param>
        public static void UpdateTableRegistrantCallback(object id)
        {
            if (!(id is Guid))
                return;
            var regId = (Guid)id;
            lock (st_TableRows.SyncRoot)
            {
                using (var context = new EFDbContext())
                {
                    var registrant = context.Registrants.Find(regId);
                    var table = st_TableRows.Get(registrant.FormKey);
                    if (table != null)
                    {
                        if (registrant.Type == RegistrationType.Live && table.Options["status"] != "live")
                            return;
                        if (registrant.Type == RegistrationType.Test && table.Options["status"] != "test")
                            return;
                        // There was a table, so we need to update that row in the table.
                        var row = table.Rows.FirstOrDefault(r => r.Id == registrant.SortingId);
                        if (row != null)
                        {
                            // The row existed, we need clear the values currently there.
                            if (registrant.DateModified < row.DateModified)
                                return;
                            row.Values.Clear();
                            row.ModificationToken = registrant.ModificationToken;
                            row.DateModified = registrant.DateModified;
                        }
                        else
                        {
                            // The row did not exists so we need to add it.
                            row = new JsonTableRow() { Id = registrant.SortingId, Token = registrant.UId };
                            table.Rows.Add(row);
                        }
                        // Now that we have the row, we need to add back in the updated values.
                        foreach (var header in table.Headers)
                            row.Values.Add(registrant.GetJsonTableValue(header));
                        // The table has been modified so we create a new modification token for synchonisity.
                        // This will force the paged data to refilter and resort to have the most up to date information available.
                        table.ModificationToken = Guid.NewGuid();
                    }
                }
            }
        }


        /// <summary>
        /// Updates the <code>JsonTableInformation</code> specified.
        /// </summary>
        /// <param name="table">The table to update.</param>
        public void UpdateJsonTableInformation(IUpdatable table)
        {
            LoadFormReport(table.Key);
        }

        /// <summary>
        /// A protected class that holds the information of the the table data.
        /// </summary>
        protected class _TableData
        {
            /// <summary>
            /// The referenced form.
            /// </summary>
            public Form Form { get; protected set; }
            /// <summary>
            /// The <code>JsonTableInformation</code>.
            /// </summary>
            public JsonTableInformation Table { get; protected set; }
            /// <summary>
            /// The saved report data.
            /// </summary>
            public ReportData ReportData { get; set; }
            /// <summary>
            /// The paged report data in memory.
            /// </summary>
            public PagedReportData TokenizedReport { get; set; }
            /// <summary>
            /// Gets the progress of the table.
            /// </summary>
            public float Progress
            {
                get
                {
                    return st_TableRows.GetProgress(Form.UId);
                }
            }

            /// <summary>
            /// Initializes the class with a form and table.
            /// </summary>
            /// <param name="form">The desired form.</param>
            /// <param name="table">The <code>JsonTable</code>.</param>
            /// <param name="data">The data int he database as a saved report.</param>
            /// <param name="paged">The paged report data in memory.</param>
            public _TableData(Form form, JsonTableInformation table = null, PagedReportData paged = null, ReportData data = null)
            {
                Form = form;
                Table = table;
                ReportData = data;
                TokenizedReport = paged;
            }
        }
        //*/
        #endregion
    }
}