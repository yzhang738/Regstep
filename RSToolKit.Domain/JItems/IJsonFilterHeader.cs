using System.Collections.Generic;

namespace RSToolKit.Domain.JItems
{
    /// <summary>
    /// Holds the information to be used in a header.
    /// </summary>
    public interface IJsonFilterHeader
    {
        /// <summary>
        /// The id of the filter.
        /// </summary>
        string Id { get; }
        /// <summary>
        /// The label of the filter.
        /// </summary>
        string Label { get; }
        /// <summary>
        /// The possible values of the filter.
        /// </summary>
        List<JsonFilterValue> PossibleValues { get; }
        /// <summary>
        /// The type of the filter.
        /// </summary>
        string Type { get; }
    }
}