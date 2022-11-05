using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RSToolKit.Domain.Data
{
    public interface IToken<T>
        where T : class, IKeepAlive
    {
        /// <summary>
        /// The key of the tableInformation.
        /// </summary>
        Guid ItemKey { get; }
        /// <summary>
        /// The key the of the token.
        /// </summary>
        Guid Key { get; }
        /// <summary>
        /// The item that the token pertains to.
        /// </summary>
        T Item { get; }
        /// <summary>
        /// Updates the progress with the specified values.
        /// </summary>
        /// <param name="progress">The fraction of completion.</param>
        /// <param name="message">The message.</param>
        void Update(float progress = 0F, string message = "Processing");
        /// <summary>
        /// Updates the progress by adding the passed progress to the current and setting the message.
        /// </summary>
        /// <param name="progress">The fraction of completion.</param>
        /// <param name="message">The message.</param>
        void UpdateAdd(float progress = 0F, string message = "Processing");
        /// <summary>
        /// Gets a progress message associated with this item.
        /// </summary>
        /// <returns>The progress message.</returns>
        ProgressMessage GetProgress();
    }
}
