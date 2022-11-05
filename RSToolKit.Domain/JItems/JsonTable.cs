using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using RSToolKit.Domain.Data;

namespace RSToolKit.Domain.JItems
{

    public static class JsonExtensions
    {
        public static List<JsonTableHeader> DeepClone(this List<JsonTableHeader> list)
        {
            var newList = new List<JsonTableHeader>();
            foreach (var header in list)
                newList.Add(header.DeepClone());
            return newList;
        }
    }


    public class JsonTableRow
    {
        public long Id { get; set; }
        public List<JsonTableValue> Values { get; set; }
        public Guid Token { get; set; }
        public bool Editable { get; set; }
        public DateTimeOffset DateModified { get; set; }
        public Guid ModificationToken { get; set; }

        public JsonTableRow()
        {
            Values = new List<JsonTableValue>();
            DateModified = DateTimeOffset.MinValue;
            ModificationToken = Guid.Empty;
        }

        public object GetSortValue(string id, JsonTableHeader header)
        {
            id = id.ToLower();
            var value = Values.FirstOrDefault(v => v.HeaderId.ToLower() == id);
            if (value == null)
                return null;
            return value.SortValue(header);
        }

        public object GetSortValue(string id)
        {
            id = id.ToLower();
            var value = Values.FirstOrDefault(v => v.HeaderId.ToLower() == id);
            if (value == null)
                return null;
            return value.SortValue(null);
        }


        public override bool Equals(object obj)
        {
            if (obj is JsonTableRow)
                return (obj as JsonTableRow).Id.Equals(Id);
            return false;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public bool TestValue(string field, string testValue, string test, bool caseSensitive = false)
        {
            var testingValue = testValue;

            var data = Values.FirstOrDefault(d => d.HeaderId == field);
            if (data == null)
            {
                var lr_field = field.ToLower();
                data = Values.FirstOrDefault(d => d.HeaderId.ToLower() == lr_field);
            }
            if (String.IsNullOrWhiteSpace(testingValue))
            {
                // The testing value is null, so we need to check for equals and not equals only.
                if (data.IsBlank() && test == "==")
                    return true;
                if (!data.IsBlank() && test == "!=")
                    return true;
                return false;

            }
            if (data == null)
                return false;
            var thisValue = data.Value;
            if (!caseSensitive && testingValue != null)
            {
                testingValue = testingValue.ToLower();
                thisValue = thisValue.ToLower();
            }

            if (data.Header.Type == "multiple selection")
            {
                #region Type: multiple secection
                long lid;
                if (!long.TryParse(testValue, out lid))
                    return true;
                var selections = new List<long>();
                try
                {
                    selections = JsonConvert.DeserializeObject<List<long>>(data.RawData);
                }
                catch (Exception)
                {
                    return false;
                }
                selections = selections ?? new List<long>();
                var iP_contains = selections.Contains(lid);
                var count = selections.Count;
                switch (test)
                {
                    case "==":
                        if (iP_contains && count == 1)
                            return true;
                        return false;
                    case "!=":
                        if (iP_contains && count == 1)
                            return false;
                        return true;
                    case "*=":
                    case "in":
                        if (iP_contains)
                            return true;
                        return false;
                    case "!*=":
                    case "not in":
                    case "notin":
                        if (!iP_contains)
                            return true;
                        return false;
                }
                #endregion
            }
            else if (data.Header.Type == "single selection")
            {
                #region Type: single selection
                if (!caseSensitive)
                {
                    thisValue = data.RawData;
                    thisValue = data.RawData ?? "";
                    thisValue = thisValue.ToLower();
                }
                switch (test)
                {
                    case "==":
                        if (testingValue == thisValue)
                            return true;
                        return false;
                    case "!=":
                        if (testingValue != thisValue)
                            return true;
                        return false;
                }
                return false;
                #endregion
            }
            else if (data.Header.Type.StartsWith("text"))
            {
                #region Type: string
                var value = data.Value ?? "";
                if (!caseSensitive)
                    value = value.ToLower();
                if (data.Header.Type.EndsWith("raw"))
                    value = data.RawData ?? "";
                switch (test)
                {
                    case "==":
                        return value == testingValue;
                    case "!=":
                        return value != testingValue;
                    case "^=":
                        return value.StartsWith(testingValue);
                    case "!^=":
                        return !value.StartsWith(testingValue);
                    case "$=":
                        return value.EndsWith(testingValue);
                    case "!$=":
                        return !value.EndsWith(testingValue);
                    case "*=":
                        return value.Contains(testingValue);
                    case "!*=":
                        return !value.Contains(testingValue);
                    case ">":
                    case ">=":
                    case "<":
                    case "<=":
                        int t_cmp;
                        if (!int.TryParse(testingValue, out t_cmp))
                            return false;
                        var lT = value.Length.CompareTo(t_cmp);
                        if (lT == 0 && test.In(">=", "<="))
                            return true;
                        if (lT > 0 && test.In(">", ">="))
                            return true;
                        if (lT < 0 && test.In("<", "<="))
                            return true;
                        return false;
                    case "=rgx=":
                    case "!=rgx=":
                        var rgx = new Regex(testValue);
                        if (test == "=rgx=")
                            return rgx.IsMatch(value);
                        return !rgx.IsMatch(value);
                }
                #endregion
            }
            else if (data.Header.Type == "date" || data.Header.Type == "datetime")
            {
                var testDate = DateTime.MinValue;
                var valueDate = DateTime.MinValue;
                var validDates = DateTime.TryParse(testingValue ?? "", out testDate) && DateTime.TryParse(data.RawData ?? "", out valueDate);
                switch (test)
                {
                    case "==":
                        return testDate == valueDate;
                    case "!=":
                        return valueDate != testDate;
                    case ">":
                    case ">=":
                    case "<":
                    case "<=":
                        var dT = valueDate.CompareTo(testDate);
                        if (dT == 0 && test.In(">=", "<="))
                            return true;
                        if (dT > 0 && test.In(">", ">="))
                            return true;
                        if (dT < 0 && test.In("<", "<="))
                            return true;
                        return false;
                }
            }
            else if (data.Header.Type == "number")
            {
                var testFloat = float.Parse(testingValue);
                var valueFloat = float.Parse(data.RawData);
                switch (test)
                {
                    case "==":
                        return testFloat == valueFloat;
                    case "!=":
                        return testFloat != valueFloat;
                    case ">":
                    case ">=":
                    case "<":
                    case "<=":
                        var dT = valueFloat.CompareTo(testFloat);
                        if (dT == 0 && test.In(">=", "<="))
                            return true;
                        if (dT > 0 && test.In(">", ">="))
                            return true;
                        if (dT < 0 && test.In("<", "<="))
                            return true;
                        return false;
                }
            }
            return false;
        }

        public bool TestValue(JsonTableHeader header, string testValue, string test, bool caseSensitive = false)
        {
            var testingValue = testValue;

            var data = Values.FirstOrDefault(d => d.HeaderId == header.Id);
            data.Header = header;
            if (String.IsNullOrWhiteSpace(testingValue))
            {
                // The testing value is null, so we need to check for equals and not equals only.
                if (data.IsBlank() && test == "==")
                    return true;
                if (!data.IsBlank() && test == "!=")
                    return true;
                return false;

            }
            if (data == null)
                return false;
            var thisValue = data.Value;
            if (!caseSensitive)
            {
                if (testingValue != null)
                    testingValue = testingValue.ToLower();
                if (thisValue != null)
                    thisValue = thisValue.ToLower();
            }

            if (data.Header.Type == "multiple selection")
            {
                #region Type: multiple secection
                long lid;
                if (!long.TryParse(testValue, out lid))
                    return true;
                var selections = new List<long>();
                try
                {
                    selections = JsonConvert.DeserializeObject<List<long>>(data.RawData);
                }
                catch (Exception)
                {
                    return false;
                }
                selections = selections ?? new List<long>();
                var iP_contains = selections.Contains(lid);
                var count = selections.Count;
                switch (test)
                {
                    case "==":
                        if (iP_contains && count == 1)
                            return true;
                        return false;
                    case "!=":
                        if (iP_contains && count == 1)
                            return false;
                        return true;
                    case "*=":
                    case "in":
                        if (iP_contains)
                            return true;
                        return false;
                    case "!*=":
                    case "not in":
                    case "notin":
                        if (!iP_contains)
                            return true;
                        return false;
                }
                #endregion
            }
            else if (data.Header.Type == "single selection")
            {
                #region Type: single selection
                if (!caseSensitive)
                {
                    thisValue = data.RawData;
                    thisValue = data.RawData ?? "";
                    if (thisValue != null)
                        thisValue = thisValue.ToLower();
                }
                switch (test)
                {
                    case "==":
                        if (testingValue == thisValue)
                            return true;
                        return false;
                    case "!=":
                        if (testingValue != thisValue)
                            return true;
                        return false;
                }
                return false;
                #endregion
            }
            else if (data.Header.Type.StartsWith("text"))
            {
                #region Type: string
                var value = data.Value ?? "";
                if (!caseSensitive)
                    value = value.ToLower();
                if (data.Header.Type.EndsWith("raw"))
                    value = data.RawData ?? "";
                switch (test)
                {
                    case "==":
                        return value == testingValue;
                    case "!=":
                        return value != testingValue;
                    case "^=":
                        return value.StartsWith(testingValue);
                    case "!^=":
                        return !value.StartsWith(testingValue);
                    case "$=":
                        return value.EndsWith(testingValue);
                    case "!$=":
                        return !value.EndsWith(testingValue);
                    case "*=":
                        return value.Contains(testingValue);
                    case "!*=":
                        return !value.Contains(testingValue);
                    case ">":
                    case ">=":
                    case "<":
                    case "<=":
                        int t_cmp;
                        if (!int.TryParse(testingValue, out t_cmp))
                            return false;
                        var lT = value.Length.CompareTo(t_cmp);
                        if (lT == 0 && test.In(">=", "<="))
                            return true;
                        if (lT > 0 && test.In(">", ">="))
                            return true;
                        if (lT < 0 && test.In("<", "<="))
                            return true;
                        return false;
                    case "=rgx=":
                    case "!=rgx=":
                        var rgx = new Regex(testValue);
                        if (test == "=rgx=")
                            return rgx.IsMatch(value);
                        return !rgx.IsMatch(value);
                }
                #endregion
            }
            else if (data.Header.Type == "date" || data.Header.Type == "datetime")
            {
                var testDate = DateTime.MinValue;
                var valueDate = DateTime.MinValue;
                var validDates = DateTime.TryParse(testingValue ?? "", out testDate) && DateTime.TryParse(data.RawData ?? "", out valueDate);
                switch (test)
                {
                    case "==":
                        return testDate == valueDate;
                    case "!=":
                        return valueDate != testDate;
                    case ">":
                    case ">=":
                    case "<":
                    case "<=":
                        var dT = valueDate.CompareTo(testDate);
                        if (dT == 0 && test.In(">=", "<="))
                            return true;
                        if (dT > 0 && test.In(">", ">="))
                            return true;
                        if (dT < 0 && test.In("<", "<="))
                            return true;
                        return false;
                }
            }
            else if (data.Header.Type == "number")
            {
                var testFloat = float.Parse(testingValue);
                var valueFloat = float.Parse(data.RawData);
                switch (test)
                {
                    case "==":
                        return testFloat == valueFloat;
                    case "!=":
                        return testFloat != valueFloat;
                    case ">":
                    case ">=":
                    case "<":
                    case "<=":
                        var dT = valueFloat.CompareTo(testFloat);
                        if (dT == 0 && test.In(">=", "<="))
                            return true;
                        if (dT > 0 && test.In(">", ">="))
                            return true;
                        if (dT < 0 && test.In("<", "<="))
                            return true;
                        return false;
                }
            }
            return false;
        }

    }

    public class JsonTableValue
    {
        protected object _sortValue = null;
        public string Id { get; set; }
        public string HeaderId { get; set; }
        [JsonIgnore]
        public JsonTableHeader Header { get; set; }
        public string Value { get; set; }
        public bool Editable { get; set; }
        public string SubScript { get; set; }
        public string RawData { get; set; }
        [JsonIgnore]
        public string Type { get; set; }
        public string Link { get; set; }

        public object SortValue(JsonTableHeader header)
        {
            header = header ?? Header;
            if (IsBlank())
                return null;
            if (_sortValue == null)
            {
                _sortValue = Value;
                if (header.Type == "date")
                {
                    DateTime date;
                    if (!DateTime.TryParse(RawData, out date))
                        return null;
                    _sortValue = date;
                }
                else if (header.Type == "number")
                {
                    float numb;
                    if (!float.TryParse(RawData, out numb))
                        return null;
                    _sortValue = numb;
                }
            }
            return _sortValue;
        }


        public JsonTableValue()
        {
            Id = "";
            HeaderId = "";
            Value = "";
            Editable = false;
            Header = null;
            Type = "text";
            SubScript = null;
            Link = null;
        }

        /// <summary>
        /// Checks to see if <code>Value</code> is blank.
        /// </summary>
        /// <returns></returns>
        public bool IsBlank()
        {
            if (String.IsNullOrWhiteSpace(Value))
                return true;
            if (Value == "[]" && Header.Type.In("multiple selection"))
                return true;
            return false;
        }
    }

    /// <summary>
    /// Holds information about a table field for sorting and filtering.
    /// </summary>
    public class JsonTableField
        : IJsonTableField
    {
        /// <summary>
        /// Flag for if the field is currently being used.
        /// </summary>
        public bool Using { get; set; }
        /// <summary>
        /// Label to display.
        /// </summary>
        public string Label { get; set; }
        /// <summary>
        /// Id of the table field.
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// The type of header.
        /// </summary>
        public string Type { get; set; }
        /// <summary>
        /// The possible values if applicable for a filter.
        /// </summary>
        public List<JsonFilterValue> PossibleValues { get; set; }

        /// <summary>
        /// Initializes the class.
        /// </summary>
        public JsonTableField()
        {
            Using = true;
            PossibleValues = new List<JsonFilterValue>();
            Type = "text";
            Id = Label = "";
        }
    }

    /// <summary>
    /// Holds information about a table header and the filter information to go along with it.
    /// </summary>
    public class JsonTableHeader
        : IJsonFilterHeader
    {
        /// <summary>
        /// The id of the header.
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// The token of the header in the database or an empty token if not in the database.
        /// </summary>
        public Guid Token { get; set; }
        /// <summary>
        /// The value of the header.
        /// </summary>
        public string Value { get; set; }
        /// <summary>
        /// Flag for a header that has editable data.
        /// </summary>
        public bool Editable { get; set; }
        /// <summary>
        /// The type of header.
        /// </summary>
        public string Type { get; set; }
        /// <summary>
        /// The possible values if applicable for a filter.
        /// </summary>
        public List<JsonFilterValue> PossibleValues { get; set; }
        /// <summary>
        /// The label of the filter. It returns the header value.
        /// </summary>
        public string Label
        {
            get
            {
                return Value;
            }
        }
        /// <summary>
        /// If the header should be hidden by default.
        /// </summary>
        [JsonIgnore]
        public bool HideByDefault { get; set; }

        public bool Hidden { get; set; }

        /// <summary>
        /// Initializes the class.
        /// </summary>
        public JsonTableHeader()
        {
            Editable = false;
            Value = "";
            Token = Guid.Empty;
            Id = "";
            Type = "text";
            PossibleValues = new List<JsonFilterValue>();
            Hidden = false;
        }

        public virtual JsonTableHeader DeepClone()
        {
            var newHeader = new JsonTableHeader()
            {
                Id = Id,
                Token = Token,
                Value = Value,
                Editable = Editable,
                Type = Type,
                Hidden = Hidden,
            };
            foreach (var pValue in PossibleValues)
                newHeader.PossibleValues.Add(new JsonFilterValue() { Id = pValue.Id, Value = pValue.Value });

            return newHeader;
        }

        /// <summary>
        /// Gets the JsonTableField.
        /// </summary>
        /// <param name="use">If the field is currently being used.</param>
        /// <returns>The field.</returns>
        public JsonTableField GetField(bool use = true)
        {
            return new JsonTableField() { Id = Id, Label = Value, Using = use, PossibleValues = PossibleValues, Type = Type };
        }

        /// <summary>
        /// Checks to see if the Ids match.
        /// </summary>
        /// <param name="obj">The object to check against.</param>
        /// <returns>True if match, false otherwise.</returns>
        public override bool Equals(object obj)
        {
            if (!(obj is JsonTableHeader))
                return false;
            var h = obj as JsonTableHeader;
            return h.Id == Id;
        }

        /// <summary>
        /// Gets the hash code of the Id.
        /// </summary>
        /// <returns>The hash code.</returns>
        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }

    public class JsonTable
        : IKeepAlive
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public List<JsonTableHeader> Headers { get; set; }
        public List<JsonTableRow> Rows { get; set; }
        public List<JsonFilter> Filters { get; set; }
        public List<JsonSorting> Sortings { get; set; }
        public List<JsonTableField> Fields { get; set; }
        public bool Count { get; set; }
        public bool Graph { get; set; }
        public bool Average { get; set; }
        public long RecordsPerPage { get; set; }
        public long Page { get; set; }
        public long TotalPages { get; set; }
        public long TotalRecords { get; set; }
        public bool SavedReport { get; set; }
        public long SavedReportId { get; set; }
        public Guid Token { get; set; }
        public bool Favorite { get; set; }
        public DateTime LastActivity { get; set; }
        public Guid Key
        {
            get
            {
                return Token;
            }
            set
            {
                Token = value;
            }
        }
        public Guid ModificationToken { get; set; }

        public JsonTable()
        {
            Favorite = false;
            Headers = new List<JsonTableHeader>();
            Rows = new List<JsonTableRow>();
            Filters = new List<JsonFilter>();
            Sortings = new List<JsonSorting>();
            LastActivity = DateTime.Now;
            Key = Guid.Empty;
            ModificationToken = Guid.NewGuid();
        }
    }
}
