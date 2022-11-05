using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RSToolKit.Domain.Data
{
    public class Paging
    {
        protected int? p_totalPages;
        protected long? p_totalRecords;
        protected int? p_recordsPerPage;

        public int Page { get; set; }
        public long TotalRecords
        {
            get
            {
                if (!p_totalRecords.HasValue)
                    return 0;
                return p_totalRecords.Value;
            }
            set
            {
                p_totalRecords = value;
                p_totalPages = null;
            }
        }
        public int RecordsPerPage
        {
            get
            {
                if (!p_recordsPerPage.HasValue)
                    return 0;
                return p_recordsPerPage.Value;
            }
            set
            {
                p_recordsPerPage = value;
                p_totalPages = null;
            }
        }
        public int TotalPages
        {
            get
            {
                if (!p_totalRecords.HasValue || !p_recordsPerPage.HasValue)
                    return 0;
                if (!p_totalPages.HasValue)
                {
                    p_totalPages = (TotalRecords == 0 ? 0 : (int)Math.Ceiling((double)TotalRecords / (double)RecordsPerPage));
                }
                return p_totalPages.Value;
            }
        }

        public Paging()
        {
            Page = 1;
            RecordsPerPage = 25;
            TotalRecords = 0;
            p_totalPages = null;
        }
    }
}
