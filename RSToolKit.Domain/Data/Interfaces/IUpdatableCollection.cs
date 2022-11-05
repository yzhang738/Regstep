using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RSToolKit.Domain.Data
{
    public interface IUpdatableCollection
        : IEnumerable<IUpdatable>
    {
        /// <summary>
        /// The synchronous object to lock when needed.
        /// </summary>
        object SyncRoot { get; }
        /// <summary>
        /// Gets the item T based on the T.
        /// </summary>
        /// <param name="key">The key of the item.</param>
        /// <returns>The item found or the default value of <code>T</code>.</returns>
        IUpdatable Get(Guid key);
    }
}
