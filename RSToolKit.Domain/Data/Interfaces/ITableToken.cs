using System;
using RSToolKit.Domain.JItems;
using System.Collections.Generic;
namespace RSToolKit.Domain.Data
{
    public interface ITableToken<T>
        : IToken<T>
        where T : class, ITableInformation
    {
        /// <summary>
        /// The filters being used.
        /// </summary>
        List<JsonFilter> Filters { get; set; }
        /// <summary>
        /// The headers being used.
        /// </summary>
        List<JsonTableHeader> Headers { get; set; }
        /// <summary>
        /// The page the token is on.
        /// </summary>
        int Page { get; set; }
        /// <summary>
        /// The records per page.
        /// </summary>
        int RecordsPerPage { get; set; }
        /// <summary>
        /// The sortings being used.
        /// </summary>
        List<JsonSorting> Sortings { get; set; }
    }
}
