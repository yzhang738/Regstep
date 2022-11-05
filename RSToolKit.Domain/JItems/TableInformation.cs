using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RSToolKit.Domain.Data;

namespace RSToolKit.Domain.JItems
{
    /// <summary>
    /// Holds information that can be used to construct a JsonTable.
    /// </summary>
    public class TableInformation
        : ITableInformation
    {
        /// <summary>
        /// The last time the information was accessed.
        /// </summary>
        protected DateTime _lastActivity;
        /// <summary>
        /// The entity the information pertains to.
        /// </summary>
        public Guid Key { get; protected set; }
        /// <summary>
        /// The raw rows.
        /// </summary>
        public List<JsonTableRow> Rows { get; set; }
        /// <summary>
        /// All of the headers.
        /// </summary>
        public List<JsonTableHeader> Headers { get; set; }
        /// <summary>
        /// All of the headers to use for filtering.
        /// </summary>
        public List<JsonFilterHeader> FilterHeaders { get; set; }
        /// <summary>
        /// The info to hold progress information.
        /// </summary>
        public IProgressInfo Info { get; protected set; }
        /// <summary>
        /// The object that is used to lock the information for synchronization.
        /// </summary>
        public object SyncRoot { get; protected set; }
        /// <summary>
        /// The last time the information was accessed.
        /// </summary>
        public DateTime LastActivity
        {
            get
            {
                return this._lastActivity;
            }
            set
            {
                lock (SyncRoot)
                {
                    this._lastActivity = value;
                }
            }
        }

        /// <summary>
        /// Initializes the table.
        /// </summary>
        /// <param name="key"></param>
        public TableInformation(Guid key)
        {
            SyncRoot = new object();
            Key = key;
            Rows = new List<JsonTableRow>();
            Headers = new List<JsonTableHeader>();
            FilterHeaders = new List<JsonFilterHeader>();
            LastActivity = DateTime.Now;
            Info = new ProgressInfo(key: key);
        }

        /// <summary>
        /// Gets a JsonTable from the TabelInformation.
        /// </summary>
        /// <param name="token">The token of the table.</param>
        /// <returns>The JsonTable.</returns>
        public JsonTable GetTable<T>(ITableToken<T> token)
            where T : class, ITableInformation
        {
            var table = new JsonTable();
            List<JsonTableRow> rows = new List<JsonTableRow>();
            #region Filtering
            if (token.Filters == null || token.Filters.Count() == 0)
            {
                rows = Rows;
            }
            else
            {
                for (var i = 0; i < Rows.Count; i++)
                {
                    var take = false;
                    var grouping = true;
                    var tests = new List<Tuple<bool, string>>();
                    var row = table.Rows.ElementAt(i);
                    for (var j = 0; j < token.Filters.Count(); j++)
                    {
                        var filter = token.Filters[j];
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
                            if (j < token.Filters.Count())
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
            table.Rows = rows;
            #endregion
            table.TotalRecords = table.Rows.Count;
            table.RecordsPerPage = token.RecordsPerPage;
            table.TotalRecords = rows.Count;
            table.TotalPages = (table.TotalRecords / token.RecordsPerPage) + (table.TotalRecords % table.RecordsPerPage > 0 ? 1 : 0);
            #region Sorting
            if (token.Sortings != null && token.Sortings.Count() > 0)
            {
                IOrderedEnumerable<JsonTableRow> oRows;
                var sort = token.Sortings.First();
                if (sort.Ascending)
                    oRows = table.Rows.OrderBy(r => r.GetSortValue(sort.Id));
                else
                    oRows = table.Rows.OrderByDescending(r => r.GetSortValue(sort.Id));
                for (var s_i = 1; s_i < token.Sortings.Count(); s_i++)
                {
                    var n_sort = token.Sortings[s_i];
                    if (n_sort.Ascending)
                        oRows = oRows.ThenBy(r => r.GetSortValue(n_sort.Id));
                    else
                        oRows = oRows.ThenByDescending(r => r.GetSortValue(n_sort.Id));
                }
                table.Rows = oRows.ToList();
            }
            #endregion
            #region Take
            if (token.Page > 0)
            {
                table.Rows = table.Rows.Skip((token.Page - 1) * token.RecordsPerPage).Take(token.RecordsPerPage).ToList();
            }
            #endregion
            #region Headers
            if (token.Headers != null && token.Headers.Count() > 0)
            {
                foreach (var row in table.Rows)
                {
                    foreach (var header in Headers.Where(h => !token.Headers.Any(h2 => h2.Id == h.Id)))
                    {
                        var value = row.Values.FirstOrDefault(v => v.HeaderId == header.Id);
                        if (value != null)
                            row.Values.Remove(value);
                    }
                }
            }
            else
            {
                token.Headers = Headers;
            }
            #endregion
            table.Filters = token.Filters.ToList() ?? new List<JsonFilter>();
            table.Sortings = token.Sortings.ToList() ?? new List<JsonSorting>();
            table.Headers = token.Headers.ToList();
            table.RecordsPerPage = token.RecordsPerPage;
            table.Page = token.Page;
            table.Key = Key;
            table.Id = Info.Key;
            return table;
        }

        /// <summary>
        /// Moves the progressional fraction forward one tick.
        /// </summary>
        /// <param name="message">The message of the tick.</param>
        /// <param name="details">The details of the tick.</param>
        /// <returns>The new progressional fraction.</returns>
        public float Tick(string message = null, string details = null)
        {
            return Info.Tick(message, details);
        }

        /// <summary>
        /// Updates the message of the progress.
        /// </summary>
        /// <param name="message">The message of the tick.</param>
        /// <param name="details">The details of the tick.</param>
        /// <returns>The new progressional fraction.</returns>
        public void UpdateMessage(string message = null, string details = null)
        {
            Info.UpdateMessage(message, details);
        }

        /// <summary>
        /// Resets the progress back to zero.
        /// </summary>
        /// <param name="message">The message of the tick.</param>
        /// <param name="details">The details of the tick.</param>
        /// <returns>The new progressional fraction.</returns>
        public void Reset(string message = null, string details = null)
        {
            Info.UpdateMessage(message, details);
        }


        /// <summary>
        /// Sets the tick fraction by the amount of ticks to completion.
        /// </summary>
        /// <param name="totalTicks">The total number of ticks.</param>
        /// <returns>The tick fraction that was calculated.</returns>
        public float SetTickFraction(long totalTicks)
        {
            return Info.SetTickFraction(totalTicks);
        }

        /// <summary>
        /// Sets the progress as failed.
        /// </summary>
        /// <param name="message">The fail message.</param>
        public void Fail(string message = null)
        {
            Info.Fail(message);
        }
    }
}
