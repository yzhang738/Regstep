using RSToolKit.Domain.Collections;
using RSToolKit.Domain.Data;
using RSToolKit.Domain.Entities;
using RSToolKit.Domain.Exceptions;
using RSToolKit.Domain.JItems;
using RSToolKit.WebUI.Infrastructure;
using RSToolKit.WebUI.Models.Views.FormEmailReport;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Mvc;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using RSToolKit.Domain;
using System.IO;

namespace RSToolKit.WebUI.Controllers
{
    [Authorize(Roles = "Super Administrators,Administrators,Cloud Users,Cloud+ Users,Programmers")]
    public class FormEmailReportController
        : RegStepController
    {
        /// <summary>
        /// The <code>FormRepository</code> for the controller.
        /// </summary>
        protected FormRepository _formRepo;
        /// <summary>
        /// The repository for saved reports.
        /// </summary>
        protected ReportDataRepository _repRepo;
        /// <summary>
        /// The tokens.
        /// </summary>
        protected TokenDictionary<TableInformation> _tokens;
        /// <summary>
        /// The current form.
        /// </summary>
        protected Form _form;

        /// <summary>
        /// Initializes the controller with a supplied context.
        /// </summary>
        public FormEmailReportController(EFDbContext context)
            : base(context)
        {
            this._formRepo = new FormRepository(Context);
            this._repRepo = new ReportDataRepository(Context);
            Repositories.Add(_formRepo);
            Log.LoggingMethod = "FormInvitationReportController";
            this._tokens = JsonTables.GetTokens(JsonTokenType.FormEmailReport);
            this._form = null;
        }

        /// <summary>
        /// Initializes the controller with a new context.
        /// </summary>
        public FormEmailReportController()
            : base()
        {
            this._formRepo = new FormRepository(Context);
            this._repRepo = new ReportDataRepository(Context);
            Repositories.Add(_formRepo);
            Log.LoggingMethod = "FormInvitationReportController";
            this._tokens = JsonTables.GetTokens(JsonTokenType.FormEmailReport);
            this._form = null;
        }

        // GET: FormReports
        [HttpGet]
        [ComplexId("id")]
        public ActionResult Get(object id, int page = 1)
        {
            if (Request.IsAjaxRequest())
            {
                // This is an ajax request. We need to see if we recieved a token.
                if (!(id is Guid))
                    // No token was supplied.
                    throw new InvalidIdException("You must present a valid unique identifier.");
                var token = this._tokens.Get((Guid)id);
                if (token == null)
                    // The token does not exist.
                    throw new TokenNotFoundException();
                if (token.Item == null)
                    // The item isn't set, we need to set it.
                    token.Item = this._tokens.GetInfo(token.ItemKey);
                if (token.Item == null)
                    // The item still is null.
                    throw new TokenNotFoundException("The token does not have a table yet.");
                token.Page = page;
                var progress = token.GetProgress();
                if (progress.Complete)
                    return JsonNetResult.Success(data: this._CreateJsonPage(token));
                return JsonNetResult.Failure(message: "The table is still processing", data: progress);
            }

            // Non ajax call, we return the view.
            var table = this._GetTable(id);
            return View("Get", new GetView() { FormKey = this._form.UId, FormName = this._form.Name });
        }

        [HttpGet]
        [ComplexId("id")]
        public ActionResult Index(object id, int page = 1)
        {
            return Index(id, page);
        }

        #region CRUD

        [HttpPost]
        [JsonValidateAntiForgeryToken]
        [ComplexId("id")]
        public JsonNetResult NewToken(object id, string reportType = "email")
        {
            // First we grab the table data.
            var table = this._GetTable(id);
            var token = this._tokens.Create(this._form.UId);
            token.Options[TokenOption.FormReportType] = reportType;
            return JsonNetResult.Success(location: Url.Action("Get", "FormEmailReport", new { id = token.Key }, Request.Url.Scheme), data: new { token = token.Key });
        }

        #endregion

        #region Update Reports

        [HttpPut]
        [JsonValidateAntiForgeryToken]
        public JsonNetResult Filters(Guid token, List<JsonFilter> filters)
        {
            filters = filters ?? new List<JsonFilter>();
            var t = JsonTables.Get(JsonTokenType.FormEmailReport, token);
            t.Filters = filters ?? new List<JsonFilter>();
            t.Page = 1;
            return JsonNetResult.Success(data: this._CreateJsonPage(t));
        }

        [HttpPut]
        [JsonValidateAntiForgeryToken]
        public JsonNetResult Sortings(Guid token, List<JsonSorting> sortings)
        {
            sortings = sortings ?? new List<JsonSorting>();
            var t = JsonTables.Get(JsonTokenType.FormEmailReport, token);
            t.Sortings = sortings ?? new List<JsonSorting>();
            t.Page = 1;
            return JsonNetResult.Success(data: this._CreateJsonPage(t));
        }

        [HttpPut]
        [JsonValidateAntiForgeryToken]
        public JsonNetResult Fields(Guid token, List<string> headers)
        {
            var t = JsonTables.Get(JsonTokenType.FormEmailReport, token);
            headers = headers ?? new List<string>();
            t.Page = 1;
            var usingHeaders = new List<JsonTableHeader>();
            foreach (var header in headers)
            {
                var t_header = t.Item.Headers.FirstOrDefault(h => h.Id == header);
                if (t_header != null)
                    usingHeaders.Add(t_header);
            }
            t.Headers = usingHeaders;
            return JsonNetResult.Success(data: this._CreateJsonPage(t));
        }

        [HttpPut]
        [JsonValidateAntiForgeryToken]
        public JsonNetResult RecordsPerPage(Guid token, int recordsPerPage)
        {
            var t = JsonTables.Get(JsonTokenType.FormEmailReport, token);
            t.Page = 1;
            t.RecordsPerPage = recordsPerPage;
            return JsonNetResult.Success(data: this._CreateJsonPage(t));
        }

        #endregion

        [HttpPost]
        [JsonValidateAntiForgeryToken]
        public JsonNetResult CreateXcel(Guid token)
        {
            var t = this._tokens.Get((Guid)token);
            var page = this._CreateJsonPage(t, true);
            if (t == null)
                // The token does not exist.
                throw new TokenNotFoundException();
            Guid fileId = Guid.Empty;
            using (var package = new ExcelPackage())
            {
                var book = package.Workbook;
                book.Properties.Author = "RegStep Technologies";
                book.Properties.Title = "Email Report";
                book.Worksheets.Add("Data");
                var sheet = book.Worksheets["Data"];
                var h_count = 0;
                foreach (var header in page.Headers)
                {
                    h_count++;
                    sheet.Cells[1, h_count].Value = header.Value;
                }
                sheet.Cells[1, 1, 1, h_count].Style.Fill.PatternType = ExcelFillStyle.Solid;
                sheet.Cells[1, 1, 1, h_count].Style.Fill.BackgroundColor.SetColor(WebConstants.Excel_HeaderColor);
                sheet.Cells[1, 1, 1, h_count].Style.Font.Color.SetColor(System.Drawing.Color.White);
                var border = sheet.Cells[1, 1, 1, h_count].Style.Border;
                border.Bottom.Style = border.Top.Style = border.Left.Style = border.Right.Style = ExcelBorderStyle.Thin;
                var r_count = 1;
                foreach (var row in page.Rows)
                {
                    r_count++;
                    if (r_count % 2 != 0)
                    {
                        sheet.Cells[r_count, 1, r_count, h_count].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        sheet.Cells[r_count, 1, r_count, h_count].Style.Fill.BackgroundColor.SetColor(WebConstants.Excel_StripeColor);
                    }
                    var r_contactCount = 0;
                    foreach (var header in page.Headers)
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
                                    displayValue = t_amount.ToString("c", this._form.Culture);
                            }
                        }
                        sheet.Cells[r_count, ++r_contactCount].Value = displayValue;
                    }
                }
                fileId = Guid.NewGuid();
                using (var fileStream = new FileStream(Server.MapPath("~/App_Data/Reports/" + fileId.ToString() + ".xlsx"), FileMode.Create))
                {
                    package.SaveAs(fileStream);
                }
            }
            if (fileId != Guid.Empty)
                return JsonNetResult.Success(location: Url.Action("Download", "FormEmailReport", new { id = fileId }, Request.Url.Scheme));
            return JsonNetResult.Failure();
        }

        [HttpGet]
        public FileResult Download(Guid id)
        {
            FileResult file = null;
            if (!System.IO.File.Exists(Server.MapPath("~/App_Data/Reports/" + id.ToString() + ".xlsx")))
                return null;
            var bytes = System.IO.File.ReadAllBytes(Server.MapPath("~/App_Data/Reports/" + id.ToString() + ".xlsx"));
            file = new FileContentResult(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
            file.FileDownloadName = "Report - " + DateTimeOffset.Now.ToString("s") + ".xlsx";
            System.IO.File.Delete(Server.MapPath("~/App_Data/Reports/" + id.ToString() + ".xlsx"));
            return file;
        }

        #region Supporting Methods

        /// <summary>
        /// Creates a <code>JsonTable</code> of paged data based on the inormation specified in <paramref name="token"/>
        /// </summary>
        /// <param name="token">The token of the user table data.</param>
        /// <returns>A <code>JsonTable</code> with a single page of data.</returns>
        protected JsonTable _CreateJsonPage(JsonTableToken<TableInformation> token, bool all = false)
        {
            #region filtering

            if (token.Item == null)
                throw new ReportDataNotFoundException("The table data was not found in memory.");

            if (token.Headers.Count == 0)
                token.Headers = token.Item.Headers.Where(h => !h.HideByDefault).ToList();

            if (token.Sortings.Count == 0)
                token.Sortings.AddRange(new List<JsonSorting>() { new JsonSorting() { Id = "EmailType", Ascending = true }, new JsonSorting() { Id = "EmailName", Ascending = true }, new JsonSorting() { Id = "EmailName", Ascending = true }, new JsonSorting() { Id = "LastName", Ascending = true } });

            List<JsonTableRow> rows_unfiltered = token.Item.Rows;
            List<JsonTableField> fields = token.Item.Headers.Select(h => h.GetField(token.Headers.Contains(h))).ToList();

            var formReportType = "email";
            if (token.Options.TryGetValue(TokenOption.FormReportType, out formReportType) && formReportType.ToLower() == "invitation")
            {
                // Here we sort out non invitations.
                rows_unfiltered = new List<JsonTableRow>();
                foreach (var row in token.Item.Rows)
                {
                    if (row.Values.First(v => v.HeaderId == "EmailType").RawData == "1")
                        rows_unfiltered.Add(row);
                }
            }

            List<JsonTableRow> rows = token.Filters.Count > 0 ? new List<JsonTableRow>() : rows_unfiltered;

            if (token.Filters.Count > 0)
            {
                for (var i = 0; i < rows_unfiltered.Count; i++)
                {
                    var take = false;
                    var grouping = true;
                    var tests = new List<Tuple<bool, string>>();
                    var row = rows_unfiltered[i];
                    for (var j = 0; j < token.Filters.Count; j++)
                    {
                        var filter = token.Filters[j];
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
                            if (j < token.Filters.Count)
                                filter = token.Filters[j];
                            else
                                break;
                        } while (grouping);
                        tests.Add(new Tuple<bool, string>(groupTest, j < (token.Filters.Count - 1) ? token.Filters[j + 1].LinkedBy : "none"));
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

            #endregion

            #region sorting

            IOrderedEnumerable<JsonTableRow> rows_ordered;
            if (token.Sortings.Count > 0)
            {
                var sort = token.Sortings.First();
                if (sort.Ascending)
                    rows_ordered = rows.OrderBy(r => r.GetSortValue(sort.Id));
                else
                    rows_ordered = rows.OrderByDescending(r => r.GetSortValue(sort.Id));
                for (var s_i = 1; s_i < token.Sortings.Count; s_i++)
                {
                    var n_sort = token.Sortings[s_i];
                    if (n_sort.Ascending)
                        rows_ordered = rows_ordered.ThenBy(r => r.GetSortValue(n_sort.Id));
                    else
                        rows_ordered = rows_ordered.ThenByDescending(r => r.GetSortValue(n_sort.Id));
                }
            }
            else
            {
                var sort = new JsonSorting() { Id = "email", Ascending = true };
                if (sort.Ascending)
                    rows_ordered = rows.OrderBy(r => r.GetSortValue(sort.Id));
                else
                    rows_ordered = rows.OrderByDescending(r => r.GetSortValue(sort.Id));
            }
            rows = rows_ordered.ToList();

            #endregion

            #region headers
            var finalRows = new List<JsonTableRow>();
            foreach (var row in rows)
            {
                var finishedRow = new JsonTableRow()
                {
                    Id = row.Id,
                    Token = row.Token,
                    ModificationToken = row.ModificationToken,
                    DateModified = row.DateModified,
                    Editable = row.Editable
                };
                finalRows.Add(finishedRow);
                for (var i = 0; i < row.Values.Count; i++)
                {
                    var value = row.Values[i];
                    if (token.Headers.Contains(value.Header))
                        finishedRow.Values.Add(value);
                }
            }

            var totalRecords = finalRows.Count;
            var totalPages = totalRecords / token.RecordsPerPage + (totalRecords % token.RecordsPerPage > 0 ? 1 : 0);

            #endregion

            var page = new JsonTable()
            {
                Page = token.Page,
                RecordsPerPage = token.RecordsPerPage,
                Filters = token.Filters,
                Headers = token.Headers,
                Id = token.ItemKey,
                Token = token.Key,
                Sortings = token.Sortings,
                Key = token.Key,
                Name = "Invitation Report",
                Rows = all ? finalRows : finalRows.Skip((token.Page - 1) * token.RecordsPerPage).Take(token.RecordsPerPage).ToList(),
                Fields = fields,
                TotalPages = totalPages,
                TotalRecords = totalRecords
            };

            return page;
        }
        
        /// <summary>
        /// Creates a <code>JsonTable</code> of paged data based on the inormation specified in <paramref name="token"/>
        /// </summary>
        /// <param name="token">The token of the user table data.</param>
        /// <returns>A <code>JsonTable</code> with a single page of data.</returns>
        protected JsonTable _CreateJsonPage(Guid token, bool all = false)
        {
            return this._CreateJsonPage(JsonTables.Get(JsonTokenType.FormEmailReport, token));
        }

        /// <summary>
        /// Gets the table from the token dictionary.
        /// </summary>
        /// <param name="id">The id of the form as either long or Guid.</param>
        /// <returns>The table information.</returns>
        protected TableInformation _GetTable(object id)
        {
            if (id is Guid)
                this._form = _formRepo.Find((Guid)id);
            else if (id is long)
                this._form = _formRepo.First(f => f.SortingId == (long)id);
            else
                throw new InvalidIdException();
            var table = this._tokens.GetInfo(this._form.UId);
            if (table != null)
                return table;
            // The table has not been created. We need to create it now.
            ThreadPool.QueueUserWorkItem(GetTableCallback, id);
            return null;
        }

        /// <summary>
        /// A thread callback for generating the report.
        /// </summary>
        /// <param name="id">The id of the form as a long or Guid.</param>
        public static void GetTableCallback(object id)
        {
            using (var context = new EFDbContext())
            {
                var tokens = JsonTables.GetTokens(JsonTokenType.FormEmailReport);
                Form form = null;
                if (id is Guid)
                    form = context.Forms.Find((Guid)id);
                else if (id is long)
                    form = context.Forms.FirstOrDefault(f => f.SortingId == (long)id);
                if (form == null)
                    return;
                form.GetTableInformation(tokens, ReportType.Invitation);
            }
        }

        #endregion
    }
}