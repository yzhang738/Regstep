using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RSToolKit.WebUI.Infrastructure;
using RSToolKit.Domain.Entities;
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

namespace RSToolKit.WebUI.Infrastructure
{
    [RegStepHandleError(typeof(InvalidIdException), "InvalidIdException")]
    [RegStepHandleError(typeof(ReportDataNotFoundException), "ReportDataNotFound")]
    [RegStepHandleError(typeof(InsufficientPermissionsException), "InsufficientPermissionsException")]
    [RegStepHandleError(typeof(ReportDataNotAttachedException), "ReportDataNotAttached")]
    public class ReportController
        : RegStepController
    {
        protected ReportDataRepository _repRepo;
        protected System.Globalization.CultureInfo _culture;

        protected static Color _st_excelHeaderColor = Color.FromArgb(51, 51, 51);
        protected static Color _st_excelStripeColor = Color.FromArgb(221, 221, 221);

        #region Contructors
        public ReportController()
            : base()
        {
            _Initialize();
        }

        public ReportController(EFDbContext context)
            : base(context)
        {
            _Initialize();
        }

        protected void _Initialize()
        {
            _repRepo = new ReportDataRepository(Context);
            Repositories.Add(_repRepo);
            Log.LoggingMethod = "ReportController";
        }
        #endregion

        [HttpGet]
        [AllowAnonymous]
        public FileResult Download(Guid token, bool remove = true)
        {
            FileResult file = null;
            if (!System.IO.File.Exists(Server.MapPath("~/App_Data/Reports/" + token.ToString() + ".xlsx")))
                return null;
            var bytes = System.IO.File.ReadAllBytes(Server.MapPath("~/App_Data/Reports/" + token.ToString() + ".xlsx"));
            file = new FileContentResult(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
            file.FileDownloadName = "Report - " + DateTimeOffset.Now.ToString("s") + ".xlsx";
            if (remove)
                System.IO.File.Delete(Server.MapPath("~/App_Data/Reports/" + token.ToString() + ".xlsx"));
            return file;
        }

        #region Supporting Methods

        /// <summary>
        /// Creates the Xcel file to download.
        /// </summary>
        /// <param name="reportData">The <code>PagedReportData</code> to use.</param>
        /// <param name="culture">The <code>CultureInfo</code> to use.</param>
        /// <returns>The id of the file to download or null.</returns>
        protected Guid _CreateXcel(PagedReportData reportData, System.Globalization.CultureInfo culture = null)
        {
            culture = culture ?? System.Globalization.CultureInfo.CurrentCulture;
            var jsonTableRows = new List<JsonTableRow>();
            var usingRows = jsonTableRows.ToList();
            var headersUnordered = reportData.FilterHeaders.ToList();
            var headers = new List<JsonFilterHeader>();
            foreach (var hId in reportData.Headers)
            {
                var t_header = headersUnordered.FirstOrDefault(h => h.Id.ToLower() == hId.ToLower());
                if (t_header != null)
                    headers.Add(t_header);
            }
            var unusingHeaders = new List<JsonFilterHeader>();
            using (var package = new ExcelPackage())
            {
                var book = package.Workbook;
                book.Properties.Author = "RegStep Technologies";
                book.Properties.Title = "Report: " + reportData.Name;
                book.Worksheets.Add("Data");
                var sheet = book.Worksheets["Data"];
                var h_count = 0;
                #region Count and Average
                if (reportData.Average || reportData.Count)
                {
                    var row = new JsonTableRow();
                    var emailHeader = headers.FirstOrDefault(h => h.Id.ToLower() == "email");
                    if (emailHeader != null)
                        headers.Remove(emailHeader);
                    var usingVals = new List<JsonTableValue>();
                    foreach (var header in headers)
                    {
                        var count = 0;
                        var takeHeader = false;
                        var val = new JsonTableValue()
                        {
                            HeaderId = header.Id
                        };
                        if (header.Type == "multipleSelection")
                        {
                            var totals = new Dictionary<string, int>();
                            foreach (var t_row in jsonTableRows)
                            {
                                var t_col = t_row.Values.FirstOrDefault(c => c.HeaderId == header.Id);
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
                                    if (!totals.Keys.Contains(item.Value))
                                        totals.Add(item.Value, 0);
                                    totals[item.Value]++;
                                }
                            }
                            if (takeHeader)
                            {
                                foreach (var kvp in totals.OrderBy(kvp => kvp.Value))
                                {
                                    if (reportData.Average)
                                        val.Value += kvp.Key + ": " + ((double)kvp.Value / count).ToString("p", culture) + "\r\n";
                                    else
                                        val.Value += kvp.Key + ": " + kvp.Value.ToString() + "\r\n";
                                }
                                if (val.Value.EndsWith("\r\n"))
                                    val.Value = val.Value.Substring(0, val.Value.Length - 4);
                                val.Value = val.Value;
                                usingVals.Add(val);
                            }
                        }
                        else if (header.Type == "itemParent")
                        {
                            var totals = new Dictionary<string, int>();
                            foreach (var t_row in jsonTableRows)
                            {
                                var t_col = t_row.Values.FirstOrDefault(c => c.HeaderId == header.Id);
                                if (t_col == null)
                                    continue;
                                if (String.IsNullOrEmpty(t_col.Value))
                                    continue;
                                takeHeader = true;
                                count++;
                                if (!totals.Keys.Contains(t_col.Value))
                                    totals.Add(t_col.Value, 0);
                                totals[t_col.Value]++;
                            }
                            if (takeHeader)
                            {
                                foreach (var kvp in totals.OrderBy(kvp => kvp.Value))
                                {
                                    if (reportData.Average)
                                        val.Value += kvp.Key + ": " + ((double)kvp.Value / count).ToString("p", culture) + "\r\n";
                                    else
                                        val.Value += kvp.Key + ": " + kvp.Value.ToString() + "\r\n";
                                }
                                if (val.Value.EndsWith("\r\n"))
                                    val.Value = val.Value.Substring(0, val.Value.Length - 3);
                                usingVals.Add(val);
                            }
                        }
                        else if (header.Type == "Number")
                        {
                            var total = 0d;
                            foreach (var t_row in jsonTableRows)
                            {
                                var t_col = t_row.Values.FirstOrDefault(c => c.HeaderId == header.Id);
                                if (t_col == null)
                                    continue;
                                if (String.IsNullOrEmpty(t_col.Value))
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
                                if (reportData.Average)
                                    val.Value = (total / count).ToString();
                                else
                                    val.Value += total.ToString();
                                usingVals.Add(val);
                            }
                        }
                        if (!takeHeader)
                            unusingHeaders.Add(header);
                    }
                    usingRows.Clear();
                    row.Values = usingVals;
                    usingRows.Add(row);
                }
                #endregion
                foreach (var unHeader in unusingHeaders)
                    headers.Remove(unHeader);
                foreach (var header in headers)
                {
                    h_count++;
                    sheet.Cells[1, h_count].Value = header.Label;
                }
                sheet.Cells[1, 1, 1, h_count].Style.Fill.PatternType = ExcelFillStyle.Solid;
                sheet.Cells[1, 1, 1, h_count].Style.Fill.BackgroundColor.SetColor(_st_excelHeaderColor);
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
                        sheet.Cells[r_count, 1, r_count, h_count].Style.Fill.BackgroundColor.SetColor(_st_excelStripeColor);
                    }
                    var r_contactCount = 0;
                    foreach (var value in row.Values)
                    {
                        var displayValue = value.Value;
                        if (value.HeaderId.ToLower().In("balance", "fees", "transactions", "adjustments", "tax"))
                        {
                            decimal t_amount;
                            if (decimal.TryParse(displayValue, out t_amount))
                                displayValue = t_amount.ToString("c", culture);
                        }
                        sheet.Cells[r_count, ++r_contactCount].Value = displayValue;
                    }
                }
                var fileId = Guid.NewGuid();
                using (var fileStream = new FileStream(Server.MapPath("~/App_Data/Reports/" + fileId.ToString() + ".xlsx"), FileMode.Create))
                {
                    package.SaveAs(fileStream);
                }
                return fileId;
            }
        }

        /// <summary>
        /// Filters the table with the specified filters in the <paramref name="paged"/>.
        /// The <paramref name="paged"/> parameters is upadated with the necesary information.
        /// </summary>
        /// <param name="paged">The <code>PagedReportData</code> that holds the desired information.</param>
        /// <returns>An <code>ICollection</code> of <code>JsonTableRow</code> that was stored into <paramref name="paged"/>.</returns>
        protected IEnumerable<JsonTableRow> _FilterTable(PagedReportData paged, JsonTableInformation table)
        {
            List<JsonTableRow> rows = paged.Filters.Count() > 0 ? new List<JsonTableRow>() : table.Rows.ToList();
            if (paged.Filters.Count() == 0)
            {
                return rows;
            }
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
                    grouping = filter.GroupNext;
                    var test = false;
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
                        if (j < paged.Filters.Count())
                            filter = paged.Filters.ElementAt(j);
                        else
                            break;
                    } while (grouping);
                    tests.Add(new Tuple<bool, string>(groupTest, j < (paged.Filters.Count() - 1) ? paged.Filters.ElementAt(j + 1).LinkedBy : "none"));
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
        protected IEnumerable<JsonTableRow> _TableHeaders(PagedReportData paged, JsonTableInformation table)
        {
            var rows = new List<JsonTableRow>();
            foreach (var row in table.Rows)
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
            _SortTable(paged);
            return rows;
        }

        /// <summary>
        /// Sorts the table with the specified sortings in the <paramref name="paged"/>.
        /// The <paramref name="paged"/> parameters is upadated with the necesary information.
        /// </summary>
        /// <param name="paged">The <code>PagedReportData</code> that holds the desired information.</param>
        /// <returns>An <code>ICollection</code> of <code>JsonTableRow</code> that was stored into <paramref name="paged"/>.</returns>
        protected IEnumerable<JsonTableRow> _SortTable(PagedReportData paged)
        {
            IOrderedEnumerable<JsonTableRow> rows;
            return null;
        }

        /// <summary>
        /// Creates a <code>JsonTable</code> of paged data based on the inormation specified in <paramref name="paged"/>
        /// </summary>
        /// <param name="paged">The information to use to create the paged data.</param>
        /// <returns>A <code>JsonTable</code> with a single page of data.</returns>
        protected JsonTable _CreateJsonPage(PagedReportData paged)
        {
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
            return page;
        }

        #endregion
    }
}