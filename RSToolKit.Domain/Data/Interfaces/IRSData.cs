using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.SqlClient;

namespace RSToolKit.Domain.Data
{
    public interface IRSData
    {
        long SortingId { get; set; }
    }
}