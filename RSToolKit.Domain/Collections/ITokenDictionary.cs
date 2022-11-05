using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RSToolKit.Domain.Data;

namespace RSToolKit.Domain.Collections
{
    /// <summary>
    /// Interface for a token dictionary.
    /// </summary>
    public interface ITokenDictionary
    {
        /// <summary>
        /// Adds the ITableInformation to the token dictionary.
        /// </summary>
        /// <param name="table">The table to add.</param>
        void Add(ITableInformation table);

        /// <summary>
        /// Moves forward one tick for the specified ITableInformation.
        /// </summary>
        /// <param name="key">The key of the ITableInformation.</param>
        /// <param name="message">The message of the tick.</param>
        /// <param name="details">The details of the tick.</param>
        /// <returns>The new progress.</returns>
        float Tick(Guid key, string message = "Processing", string details = null);

        /// <summary>
        /// Sets the tick interval based on the total number of ticks to be used.
        /// </summary>
        /// <param name="key">The key of the ITableInformation to use.</param>
        /// <param name="totalTicks">The total number of ticks.</param>
        /// <returns></returns>
        float SetTickFraction(Guid key, long totalTicks);

        /// <summary>
        /// Updates the message for a specified key.
        /// </summary>
        /// <param name="key">The key of the ITableInformation.</param>
        /// <param name="message">The message of the tick.</param>
        /// <param name="details">The details of the tick.</param>
        void UpdateMessage(Guid key, string message = "Processing", string details = null);

        /// <summary>
        /// Resets the progress back to zero.
        /// </summary>
        /// <param name="key">The key of the ITableInformation to use.</param>
        /// <param name="message">[optional] The message to set.</param>
        /// <param name="details">[optional] The details to set.</param>
        void Reset(Guid key, string message = null, string details = null);

    }
}
