using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;

namespace RSToolKit.Domain.Data
{
    /// <summary>
    /// A thread safe implementation of a collection that holds <code>IKeepAlive</code> items.
    /// </summary>
    public interface IKeepAliveCollection
        : IEnumerable<IKeepAlive>
    {
        /// <summary>
        /// The synchronous object to lock when needed.
        /// </summary>
        object SyncRoot { get; }
        /// <summary>
        /// Removes the item from the collection.
        /// </summary>
        /// <param name="item">The item to remove.</param>
        void Remove(IKeepAlive item);
        /// <summary>
        /// Gets the item T based on the T.
        /// </summary>
        /// <param name="key">The key of the item.</param>
        /// <returns>The item found or the default value of <code>T</code>.</returns>
        IKeepAlive Get(Guid key);
        /// <summary>
        /// Adds an item to the ICollection
        /// </summary>
        /// <param name="item">The item to add.</param>
        void Add(IKeepAlive item);
        /// <summary>
        /// Checks to see if the key is processing.
        /// </summary>
        /// <param name="key">The key in question.</param>
        /// <returns><code>true</code> if it is, <code>false</code> otherwise.</returns>
        bool IsProcessing(Guid key);

    }
}
