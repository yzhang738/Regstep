using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RSToolKit.Domain.Data
{
    /// <summary>
    /// Holds progress information.
    /// </summary>
    public interface IProgressData
    {

        /// <summary>
        /// Moves forward one tick for the specified ITableInformation.
        /// </summary>
        /// <param name="key">The key of the ITableInformation.</param>
        /// <param name="message">The message of the tick.</param>
        /// <param name="details">The details of the tick.</param>
        /// <returns>The new progress.</returns>
        float Tick(Guid key, string message = null, string details = null);

        /// <summary>
        /// Sets the tick interval based on the total number of ticks to be used.
        /// </summary>
        /// <param name="key">The key of the ITableInformation to use.</param>
        /// <param name="totalTicks">The total number of ticks.</param>
        /// <returns></returns>
        float SetTickFraction(Guid key, int totalTicks);

        /// <summary>
        /// Updates the message and details.
        /// </summary>
        /// <param name="key">The key of the ITableInformation.</param>
        /// <param name="message">The message of the tick.</param>
        /// <param name="details">The details of the tick.</param>
        /// <returns>The new progress.</returns>
        void UpdateMessage(Guid key, string message = null, string details = null);
    }
}
