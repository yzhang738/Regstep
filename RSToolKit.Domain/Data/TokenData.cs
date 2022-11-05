using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;
using RSToolKit.Domain.Data.Info;
using RSToolKit.Domain.JItems;

namespace RSToolKit.Domain.Data
{
    /// <summary>
    /// Holds information for the token data.
    /// </summary>
    public class TokenData
    {
        /// <summary>
        /// The id of the data for data retrieval purposes only.
        /// </summary>
        [Key]
        public long Id { get; set; }
        /// <summary>
        /// The last time the data was accessed.
        /// </summary>
        public DateTimeOffset LastAccess { get; set; }
        /// <summary>
        /// The concurrency item.
        /// </summary>
        [Timestamp]
        public byte[] RowVersion { get; set; }
        /// <summary>
        /// The key to the data.
        /// </summary>
        [Index(IsClustered = false)]
        public Guid DataKey { get; set; }
        /// <summary>
        /// The data.
        /// </summary>
        public string Data { get; set; }
        /// <summary>
        /// The options for the data.
        /// </summary>
        public string Discriminator { get; set; }
    }

    public class Token
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long SortingId { get; set; }
        [Index(IsClustered = false)]
        public Guid Id { get; set; }
        public Guid Key { get; set; }
        [NotMapped]
        public List<JsonFilter> Filters { get; set; }
        public string RawFilters
        {
            get
            {
                try
                {
                    return JsonConvert.SerializeObject(Filters);
                }
                catch (Exception)
                {
                    return "[]";
                }
            }
            set
            {
                try
                {
                    Filters = JsonConvert.DeserializeObject<List<JsonFilter>>(value);
                }
                catch (Exception)
                {
                    Filters = new List<JsonFilter>();
                }
            }
        }
        [NotMapped]
        public List<string> Headers { get; set; }
        public string RawHeaders
        {
            get
            {
                try
                {
                    return JsonConvert.SerializeObject(Headers);
                }
                catch (Exception)
                {
                    return "[]";
                }
            }
            set
            {
                try
                {
                    Headers = JsonConvert.DeserializeObject<List<string>>(value);
                }
                catch (Exception)
                {
                    Headers = new List<string>();
                }
            }


        }
        [NotMapped]
        public List<JsonSorting> Sortings { get; set; }
        public string RawSortings
        {
            get
            {
                try
                {
                    return JsonConvert.SerializeObject(Sortings);
                }
                catch (Exception)
                {
                    return "[]";
                }
            }
            set
            {
                try
                {
                    Sortings = JsonConvert.DeserializeObject<List<JsonSorting>>(value);
                }
                catch (Exception)
                {
                    Sortings = new List<JsonSorting>();
                }
            }


        }
        public int RecordsPerPage { get; set; }
        public string Name { get; set; }
        public bool IsReport { get; set; }
        public bool Favorite { get; set; }
        public long SavedReportId { get; set; }

        public Token()
        {
            Filters = new List<JsonFilter>();
            Sortings = new List<JsonSorting>();
            Headers = new List<string>();
            RecordsPerPage = 15;
            Name = "Registrants";
            IsReport = false;
            Favorite = false;
            SavedReportId = -1;
        }
    }
}
