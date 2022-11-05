using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RSToolKit.Domain.Data.OldJsonData
{
    public class JsonTable
    {
        public List<JsonTableHeader> Headers { get; set; }
        public List<JsonTableRow> Rows { get; set; }
        public string Name { get; set; }
        public string Id { get; set; }
        public Paging Paging { get; set; }

        public JsonTable()
        {
            Rows = new List<JsonTableRow>();
            Headers = new List<JsonTableHeader>();
            Paging = new Data.Paging();
            Name = Id = "";
        }

        public void AddHeader(JsonTableHeader header)
        {
            header.Order = Headers.Count;
            Headers.Add(header);
        }

        public void AddRow(JsonTableRow row)
        {
            row.Order = Rows.Count;
            Rows.Add(row);
        }

    }

    public class JsonTableHeader
    {
        public string Label { get; set; }
        public string Id { get; set; }
        public int Order { get; set; }

        public JsonTableHeader()
        {
            Label = Id = "";
            Order = 0;
        }
    }

    public class JsonTableRow
    {
        public string Id { get; set; }
        public List<JsonTableColumn> Columns { get; set; }
        public int Order { get; set; }

        public JsonTableRow()
        {
            Id = "";
            Columns = new List<JsonTableColumn>();
            Order = 0;
        }
    }

    public class JsonTableColumn
    {
        public string HeaderId { get; set; }
        public string PrettyValue { get; set; }
        public string Id { get; set; }
        public string Value { get; set; }

        public JsonTableColumn()
        {
            HeaderId = Value = Id = PrettyValue = "";
        }
    }
}
