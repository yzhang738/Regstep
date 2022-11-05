using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RSToolKit.Domain.JItems;

namespace RSToolKit.Domain.Data
{
    /// <summary>
    /// Represents an item that can generate a table based on a reportData class.
    /// </summary>
    public interface IJsonTable
    {
        Guid UId { get; set; }
        string Name { get; set; }
        Task<JsonTable> CreateTable(PagedReportData reportData, bool showDeleted = false);
    }
}
