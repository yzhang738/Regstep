using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RSToolKit.Domain.JItems;

namespace RSToolKit.Domain.Data
{
    public interface ITableInformation
        : IKeepAlive
    {
        /// <summary>
        /// The progress information.
        /// </summary>
        IProgressInfo Info { get; }
        /// <summary>
        /// The sync root used for synchronizing accross threads.
        /// </summary>
        object SyncRoot { get; }
        /// <summary>
        /// Gets a JsonTable.
        /// </summary>
        /// <typeparam name="T">The type of ITableInformation.</typeparam>
        /// <param name="token">The token.</param>
        /// <returns>The table.</returns>
        JsonTable GetTable<T>(ITableToken<T> token) where T : class, ITableInformation;
        /// <summary>
        /// Moves the progressional fraction forward one tick.
        /// </summary>
        /// <param name="message">[optional] The message of the tick.</param>
        /// <param name="details">[optional] The details of the tick.</param>
        /// <returns>The new progressional fraction.</returns>
        float Tick(string message = null, string details = null);
        /// <summary>
        /// Sets the tick fraction by the amount of ticks to completion.
        /// </summary>
        /// <param name="totalTicks">The total number of ticks.</param>
        /// <returns>The tick fraction that was calculated.</returns>
        float SetTickFraction(long totalTicks);
        /// <summary>
        /// Updates the message and details.
        /// </summary>
        /// <param name="message">[optional] The message of the tick.</param>
        /// <param name="details">[optional] The details of the tick.</param>
        /// <returns>The new progressional fraction.</returns>
        void UpdateMessage(string message = null, string details = null);
        /// <summary>
        /// Resets the progress back to zero.
        /// </summary>
        /// <param name="message">[optional] The message to set.</param>
        /// <param name="details">[optional] The details to set.</param>
        void Reset(string message = null, string details = null);
    }
}
