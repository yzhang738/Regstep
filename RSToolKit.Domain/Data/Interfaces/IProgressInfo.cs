using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RSToolKit.Domain.Data
{
    /// <summary>
    /// Holds information about the progress of a process.
    /// </summary>
    public interface IProgressInfo
    {
        /// <summary>
        /// The key of the process this is referencing.
        /// </summary>
        Guid Key { get; }
        /// <summary>
        /// The progress of the action taking place.
        /// </summary>
        float Progress { get; }
        /// <summary>
        /// The status message of the progress.
        /// </summary>
        string Message { get; }
        /// <summary>
        /// The details of the message.
        /// </summary>
        string Details { get; }
        /// <summary>
        /// Flag for the full completion of process.
        /// </summary>
        bool Complete { get; set; }
        /// <summary>
        /// Determines if the item is synchronous or not.
        /// </summary>
        bool IsSynchronous { get; }
        /// <summary>
        /// Gets the fraction to advance per tick.
        /// </summary>
        float FractionPerTick { get; }
        /// <summary>
        /// Updates the progress with the specified values.
        /// </summary>
        /// <param name="progress">The fraction of completion.</param>
        /// <param name="message">The message.</param>
        void Update(float progress = 0F, string message = "Processing", string details = null);
        /// <summary>
        /// Updates the progress by adding the passed progress to the current and setting the message.
        /// </summary>
        /// <param name="progress">The fraction of completion.</param>
        /// <param name="message">The message.</param>
        void UpdateAdd(float progress = 0F, string message = "Processing", string details = null);
        /// <summary>
        /// Sets the tick fraction by the amount of ticks to completion.
        /// </summary>
        /// <param name="totalTicks">The total number of ticks.</param>
        /// <returns>The tick fraction that was calculated.</returns>
        float SetTickFraction(long totalTicks);
        /// <summary>
        /// Moves the progressional fraction forward one tick.
        /// </summary>
        /// <param name="message">The message of the tick.</param>
        /// <param name="details">The details of the tick.</param>
        /// <returns>The new progressional fraction.</returns>
        float Tick(string message = "Processing", string details = null);
        /// <summary>
        /// Updates the message and details.
        /// </summary>
        /// <param name="message">The message of the tick.</param>
        /// <param name="details">The details of the tick.</param>
        /// <returns>The new progressional fraction.</returns>
        void UpdateMessage(string message = "Processing", string details = null);
        /// <summary>
        /// Resets the progress back to zero.
        /// </summary>
        /// <param name="message">[optional] The message to set.</param>
        /// <param name="details">[optional] The details to set.</param>
        void Reset(string message = null, string details = null);
        /// <summary>
        /// Sets the progress as failed.
        /// </summary>
        /// <param name="message">The fail message.</param>
        void Fail(string message = null);

    }
}
