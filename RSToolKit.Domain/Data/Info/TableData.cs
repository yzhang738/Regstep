using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RSToolKit.Domain.JItems;

namespace RSToolKit.Domain.Data.Info
{
    public class TableData
    {
        /// <summary>
        /// The raw rows.
        /// </summary>
        public List<JsonTableRow> Rows { get; set; }
        /// <summary>
        /// All of the headers.
        /// </summary>
        public List<JsonTableHeader> Headers { get; set; }
    }
}
