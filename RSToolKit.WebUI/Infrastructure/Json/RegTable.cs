using System.Collections.Generic;
using Newtonsoft.Json;

namespace RSToolKit.WebUI.Infrastructure.Json
{
    public class RegTable
    {
        public List<RegTableHeader> Headers { get; set; }
        public List<RegTableRow> Rows { get; set; }
        public string Name { get; set; }
        public bool Vertical { get; set; }

        public RegTable()
        {
            Headers = new List<RegTableHeader>();
            Rows = new List<RegTableRow>();
            Name = "unammed";
            Vertical = false;
        }
    }

    public class RegTableHeader
    {
        public string Value { get; set; }
        public int Order { get; set; }
    }

    public class RegTableRow
    {
        public List<RegTableRowValue> Values { get; set; }
        public int Order { get; set; }

        public RegTableRow()
        {
            Values = new List<RegTableRowValue>();
        }
    }

    public class RegTableRowValue
    {
        public string Value { get; set; }
        [JsonIgnore]
        public RegTableHeader Header { get; set; }
        public string HeaderValue
        {
            get
            {
                if (Header == null)
                    return "";
                return Header.Value;
            }
        }
    }
}