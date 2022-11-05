using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RSToolKit.Domain.JItems;
using RSToolKit.Domain.Entities;
using RSToolKit.Domain.Data;

namespace RSToolKit.Domain.Data
{
    public class PagedReportData
        : IUpdatable
    {
        public Guid Token { get; set; }
        public Guid Table { get; set; }
        public string User { get; set; }
        public DateTime LastActivity { get; set; }
        public List<string> Headers { get; set; }
        public List<JsonSorting> Sortings { get; set; }
        public List<JsonFilter> Filters { get; set; }
        public long Page { get; set; }
        public long RecordsPerPage { get; set; }
        public bool Count { get; set; }
        public bool Average { get; set; }
        public bool Graph { get; set; }
        public Dictionary<string, string> Options { get; set; }
        public string Name { get; set; }
        public long ReportDataId { get; set; }
        public bool SavedReport { get; set; }
        public List<JsonFilterHeader> FilterHeaders { get; set; }
        public List<JsonTableHeader> UsingHeaders { get; set; }
        public bool Favorite { get; set; }
        public DateTime LastUpdate { get; set; }
        public Guid Key
        {
            get
            {
                return Token;
            }
        }
        public Guid ModificationToken { get; set; }

        public PagedReportData()
        {
            RecordsPerPage = RSToolKit.Domain.Constants.DefaultRecordsPerPage;
            Count = Average = Graph = false;
            Page = 1;
            Filters = new List<JsonFilter>();
            Sortings = new List<JsonSorting>();
            Headers = new List<string>();
            LastActivity = DateTime.Now;
            SavedReport = false;
            ReportDataId = -1L;
            Favorite = false;
            FilterHeaders = new List<JsonFilterHeader>();
        }

        public override bool Equals(object obj)
        {
            if (obj is PagedReportData)
                return ((PagedReportData)obj).Token == Token;
            return false;
        }

        public override int GetHashCode()
        {
            return Token.GetHashCode();
        }

    }
}
