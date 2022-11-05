using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;

namespace RSToolKit.Domain.JItems
{
    public class JTable
    {
        public List<JTableHeader> Headers { get; set; }
        public List<JTableRow> Rows { get; set; }
        public string Parent { get; set; }
        public string Description { get; set; }
        public string Name { get; set; }
        public string Id { get; set; }
        public int RecordsPerPage { get; set; }
        public int TotalRecords { get; set; }
        public List<JTableSorting> Sortings { get; set; }
        public List<JTableFilter> Filters { get; set; }
        public bool Average { get; set; }
        public bool Graph { get; set; }
        public bool Count { get; set; }
        public Dictionary<string, string> Options { get; set; }
        public string SavedId { get; set; }
        public bool Favorite { get; set; }

        public JTable()
        {
            Rows = new List<JTableRow>();
            Headers = new List<JTableHeader>();
            Sortings = new List<JTableSorting>();
            Filters = new List<JTableFilter>();
            Name = Id = "";
            Parent = Description = "";
            RecordsPerPage = 250;
            TotalRecords = 0;
            Average = Graph = Count = false;
            Options = new Dictionary<string, string>();
            SavedId = null;
            Favorite = false;
        }

        public void AddHeader(JTableHeader header)
        {
            header.Order = Headers.Count;
            Headers.Add(header);
        }

        public void AddRow(JTableRow row)
        {
            row.Order = Rows.Count;
            Rows.Add(row);
        }

        public void Filter()
        {

        }
    }

    public class JTableSorting
    {
        public string ActingOn { get; set; }
        public bool Ascending { get; set; }
        public int Order { get; set; }
        
        public JTableSorting()
        {
            ActingOn = "";
            Ascending = true;
            Order = -1;
        }
    }

    public class JTableFilter
    {
        public string ActingOn { get; set; }
        public string Test { get; set; }
        public string Value { get; set; }
        public int Order { get; set; }
        public bool GroupNext { get; set; }
        public string Link { get; set; }
        public bool CaseSensitive { get; set; }

        public JTableFilter()
        {
            ActingOn = Value = "";
            Test = "==";
            Order = -1;
            GroupNext = false;
            Link = "none";
            CaseSensitive = false;
        }
    }

    public class JTableHeader
    {
        public string Label { get; set; }
        public string Id { get; set; }
        public int Order { get; set; }
        public string Group { get; set; }
        public string Type { get; set; }
        public List<JTableHeaderPossibleValue> PossibleValues { get; set; }
        public bool Editable { get; set; }
        public bool Removed { get; set; }
        public bool SortByPretty { get; set; }

        public JTableHeader()
        {
            Label = Id = Group = "";
            Type = "text";
            Order = 0;
            PossibleValues = new List<JTableHeaderPossibleValue>();
            Editable = false;
            Removed = false;
            SortByPretty = false;
        }
    }

    public class JTableHeaderPossibleValue
    {
        public string Id { get; set; }
        public string Label { get; set; }

        public JTableHeaderPossibleValue()
        {
            Id = Label = "";
        }
    }

    public class JTableRow
    {
        public string Id { get; set; }
        public List<JTableColumn> Columns { get; set; }
        public int Order { get; set; }
        public int PreviousSort { get; set; }

        public JTableRow()
        {
            Id = "";
            Columns = new List<JTableColumn>();
            Order = 0;
            PreviousSort = 0;
        }

        public bool TestValue(JTableFilter filter, JTableHeader header)
        {
            var data = Columns.FirstOrDefault(c => c.HeaderId == filter.ActingOn);
            if (data == null)
                return false;
            if (header == null)
                return false;
            if (data.Value == null)
                data.Value = "";
            var integer = 0;
            int.TryParse(filter.Value, out integer);
            if (header.Type == "string" || header.Type == "text") {
                switch (filter.Test) {
                    case "==":
                        return data.Value.ToLower() == filter.Value.ToLower();
                    case "!=":
                        return data.Value.ToLower() != filter.Value.ToLower();
                    case ">":
                        return data.Value.Length > integer;
                    case ">=":
                        return data.Value.Length >= integer;
                    case "<":
                        return data.Value.Length < integer;
                    case "<=":
                        return data.Value.Length <= integer;
                    case "*=":
                        return data.Value.ToLower().Contains(filter.Value.ToLower());
                    case "!*=":
                        return !data.Value.ToLower().Contains(filter.Value.ToLower());
                    case "^=":
                        return data.Value.ToLower().StartsWith(filter.Value.ToLower());
                    case "!^=":
                        return !data.Value.ToLower().StartsWith(filter.Value.ToLower());
                    case "$=":
                        return data.Value.ToLower().EndsWith(filter.Value.ToLower());
                    case "!$=":
                        return data.Value.ToLower().EndsWith(filter.Value.ToLower());
                    case "=rgx=":
                        return System.Text.RegularExpressions.Regex.IsMatch(data.Value.ToLower(), filter.Value.ToLower());
                    case "!=rgx=":
                        return System.Text.RegularExpressions.Regex.IsMatch(data.Value.ToLower(), filter.Value.ToLower());
                }
            } else if (header.Type == "number") {
                float t_value = 0;
                float t_testValue = 0;
                if (!float.TryParse(data.Value, out t_value) || !float.TryParse(filter.Value, out t_testValue))
                    return false;
                switch (filter.Test) {
                    case "==":
                        return t_value == t_testValue;
                    case "!=":
                        return t_value != t_testValue;
                    case ">":
                        return t_value > t_testValue;
                    case ">=":
                        return t_value >= t_testValue;
                    case "<":
                        return t_value < t_testValue;
                    case "<=":
                        return t_value <= t_testValue;
                }
            } else if (header.Type == "multipleSelection") {
                var t_value = JsonConvert.DeserializeObject<List<string>>(data.Value);
                var found = false;
                switch (filter.Test) {
                    case "==":
                        return (t_value.Count == 1) && (t_value[0] == data.Value);
                    case "!=":
                        for (var si = 0; si < t_value.Count; si++) {
                            if (t_value[si] == filter.Value) {
                                found = true;
                                break;
                            }
                        }
                        return !found;
                    case "in":
                        for (var si = 0; si < t_value.Count; si++) {
                            if (t_value[si] == filter.Value) {
                                found = true;
                                break;
                            }
                        }
                        return found;
                    case "notin":
                        for (var si = 0; si < t_value.Count; si++) {
                            if (t_value[si] == filter.Value) {
                                found = true;
                                break;
                            }
                        }
                        return !found;
                }
            } else if (header.Type == "itemParent") {
                var t_value = data.Value;
                switch (filter.Test) {
                    case "==":
                        return t_value == filter.Value;
                    case "!=":
                        return t_value != filter.Value;
                }
            }
            else if (header.Type == "boolean")
            {
                switch (filter.Test) {
                    case "==":
                        return data.Value == filter.Value;
                    case "!=":
                        return data.Value != filter.Value;
                }
            }
            return false;
        }
    }

    public class JTableColumn
    {
        public string HeaderId { get; set; }
        public string PrettyValue { get; set; }
        public string Id { get; set; }
        public string Value { get; set; }
        public bool Editable { get; set; }
        public bool printPretty { get; set; }

        public JTableColumn()
        {
            HeaderId = Value = Id = PrettyValue = "";
            Editable = false;
            printPretty = true;
        }
    }
}