using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RSToolKit.Domain.JItems
{
    public class JsonFilter
    {
        public string ActingOn { get; set; }
        public string Value { get; set; }
        public string Equality { get; set; }
        public bool GroupNext { get; set; }
        public string LinkedBy { get; set; }
    }

    /// <summary>
    /// A filter value.
    /// </summary>
    public class JsonFilterValue
    {
        /// <summary>
        /// The value to display.
        /// </summary>
        public string Value { get; set; }
        /// <summary>
        /// The id of the value for filtering.
        /// </summary>
        public string Id { get; set; }
    }

    public class JsonFilterHeader
        : IJsonFilterHeader
    {
        public string Type { get; set; }
        public string Label { get; set; }
        public string Id { get; set; }
        public List<JsonFilterValue> PossibleValues { get; set; }

        public JsonFilterHeader()
        {
            PossibleValues = new List<JsonFilterValue>();
            Type = "text";
        }
    }
}
