using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RSToolKit.Domain.Data;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using RSToolKit.Domain.JItems;
using Newtonsoft.Json;
using RSToolKit.Domain.Security;

namespace RSToolKit.Domain.Entities
{
    /// <summary>
    /// Holds information for a report.
    /// </summary>
    public class ReportData
        : INamedNode, IReportData
    {
        /// <summary>
        /// The id of the report.
        /// </summary>
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public long SortingId { get; set; }
        /// <summary>
        /// The name of the report.
        /// </summary>
        [Required]
        public string Name { get; set; }
        /// <summary>
        /// The date the report was created.
        /// </summary>
        public DateTimeOffset DateCreated { get; set; }
        /// <summary>
        /// The last date of modification.
        /// </summary>
        public DateTimeOffset DateModified { get; set; }
        /// <summary>
        /// The token that is generated each time the report is modified.
        /// </summary>
        public Guid ModificationToken { get; set; }
        /// <summary>
        /// The Guid of the person who modified the report.
        /// </summary>
        public Guid ModifiedBy { get; set; }
        /// <summary>
        /// A Guid of the report.
        /// </summary>
        [Index(IsClustered = false)]
        public Guid UId { get; set; }
        /// <summary>
        /// The id of the table being used.
        /// </summary>
        public Guid TableId { get; set; }
        /// <summary>
        /// The Json object of the headers being used.
        /// </summary>
        [MaxLength]
        public string RawHeaders { get; set; }
        /// <summary>
        /// The Json object of the sortings being used.
        /// </summary>
        [MaxLength]
        public string RawSortings { get; set; }
        /// <summary>
        /// The Json object of the filters being used.
        /// </summary>
        [MaxLength]
        public string RawFilters { get; set; }
        /// <summary>
        /// If the report is a count report.
        /// </summary>
        public bool Count { get; set; }
        /// <summary>
        /// If the report is a average report.
        /// </summary>
        public bool Average { get; set; }
        /// <summary>
        /// If the report is a graph report.
        /// </summary>
        public bool Graph { get; set; }
        /// <summary>
        /// The default amount of records per page.
        /// </summary>
        public long RecordsPerPage { get; set; }
        /// <summary>
        /// Tells us if the report is a favorite.
        /// </summary>
        public bool Favorite { get; set; }
        /// <summary>
        /// Company Key
        /// </summary>
        public long CompanyKey { get; set; }
        /// <summary>
        /// The table that the data is used for.
        /// </summary>
        [NotMapped]
        public IJsonTable Table { get; set; }
        /// <summary>
        /// The type of report.
        /// </summary>
        public ReportType Type { get; set; }

        /// <summary>
        /// Initializes a blank report.
        /// </summary>
        public ReportData()
        {
            RawHeaders = "[]";
            RawFilters = "[]";
            RawSortings = "[]";
            TableId = Guid.Empty;
            DateCreated = DateModified = DateTimeOffset.Now;
            ModifiedBy = Guid.Empty;
            Favorite = false;
            CompanyKey = -1;
            Type = ReportType.Form;
        }

        /// <summary>
        /// Gets the enumeration of JsonFilters to use in this report.
        /// </summary>
        /// <returns>The enumerable set of JsonFilters.</returns>
        public List<JsonFilter> GetFilters()
        {
            try
            {
                return JsonConvert.DeserializeObject<List<JsonFilter>>(RawFilters);
            }
            catch (Exception)
            {
                return new List<JsonFilter>();
            }
        }

        /// <summary>
        /// Sets the enumeration of JsonSortings to use int this report.
        /// </summary>
        /// <param name="filters">The filters to apply.</param>
        public void SetFilters(IEnumerable<JsonFilter> filters)
        {
            try
            {
                RawFilters = JsonConvert.SerializeObject(filters);
            }
            catch (Exception)
            {
                RawFilters = "[]";
            }
        }

        /// <summary>
        /// Gets the enumeration of JsonSorting to use in this report.
        /// </summary>
        /// <returns>The enumerable set of JsonSortings.</returns>
        public List<JsonSorting> GetSortings()
        {
            try
            {
                return JsonConvert.DeserializeObject<List<JsonSorting>>(RawSortings);
            }
            catch (Exception)
            {
                return new List<JsonSorting>();
            }
        }

        /// <summary>
        /// Sets the enumeration of JsonSortings to use for in this report.
        /// </summary>
        /// <param name="sortings">The sortings to apply.</param>
        public void SetSortings(IEnumerable<JsonSorting> sortings)
        {
            try
            {
                RawSortings = JsonConvert.SerializeObject(sortings);
            }
            catch (Exception)
            {
                RawSortings = "[]";
            }
        }

        /// <summary>
        /// Gets the headers being used for this report.
        /// </summary>
        /// <returns>The enumerable set of headers as long SortingIds</returns>
        public List<string> GetHeaders()
        {
            try
            {
                return JsonConvert.DeserializeObject<List<string>>(RawHeaders);
            }
            catch (Exception)
            {
                return new List<string>();
            }
        }

        /// <summary>
        /// Sets the headers to use for this report.
        /// </summary>
        /// <param name="headers">The set of headers represented by <code>long</code> integers.</param>
        public void SetHeaders(IEnumerable<string> headers)
        {
            try
            {
                RawHeaders = JsonConvert.SerializeObject(headers);
            }
            catch (Exception)
            {
                RawHeaders = "[]";
            }
        }

        /// <summary>
        /// Sets the headers to use for this report.
        /// </summary>
        /// <param name="headers">The set of headers represented by <code>JsonTableHeader</code>.</param>
        public void SetHeaders(IEnumerable<JsonTableHeader> headers)
        {
            RawHeaders = JsonConvert.SerializeObject(headers.Select(h => h.Id));
        }
    }

    /// <summary>
    /// Holds information to discriminate between <code>ReportData</code> types.
    /// </summary>
    public enum ReportType
    {
        /// <summary>
        /// Form Report
        /// </summary>
        Form = 0,
        /// <summary>
        /// Form Invitation Report
        /// </summary>
        Invitation = 1,
        /// <summary>
        /// Email Report
        /// </summary>
        Email = 2
    }
}
