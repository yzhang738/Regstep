using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RSToolKit.Domain.Data
{
    public interface IComplexDictionary
        : IUpdatableCollection
    {
        /// <summary>
        /// Gets the progress for the current key.
        /// </summary>
        /// <param name="key">The key to update the progress.</param>
        /// <returns>A float of the progress from 0 to 1.</returns>
        float GetProgress(Guid key);
        /// <summary>
        /// Updates the progress for the key.
        /// </summary>
        /// <param name="key">The key to associate with.</param>
        /// <param name="percent">The percent of progress form 0 to 1.</param>
        void UpdateProgress(Guid key, float percent);
    }
}
