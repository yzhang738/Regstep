using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RSToolKit.Domain.Data;
using System.Threading;
using System.Collections;

namespace RSToolKit.Domain.Collections
{
    /// <summary>
    /// The dictionary of keep alive items. This is a thread safe implementation of the dictionary.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class KeepAliveDictionary<T>
        : IKeepAliveCollection, IDictionary<Guid, IKeepAlive>
        where T : class, IKeepAlive
    {
        /// <summary>
        /// The dictionary for the items.
        /// </summary>
        protected Dictionary<Guid, IKeepAlive> _dic;
        protected List<Guid> _processing;

        /// <summary>
        /// The lock for thread safe activities.
        /// </summary>
        protected object _lock;

        /// <summary>
        /// Initializes the class.
        /// </summary>
        public KeepAliveDictionary()
        {
            this._dic = new Dictionary<Guid,IKeepAlive>();
            this._processing = new List<Guid>();
            this._lock = new object();
        }

        /// <summary>
        /// Initializes the class with a set of data.
        /// </summary>
        /// <param name="collection">The data to initialize the class with.</param>
        public KeepAliveDictionary(IDictionary<Guid, IKeepAlive> collection)
            : this()
        {
            this._dic = new Dictionary<Guid, IKeepAlive>(collection);
        }

        /// <summary>
        /// Clears the collection of items.
        /// </summary>
        public void Clear()
        {
            lock (this._lock)
            {
                this._dic.Clear();
            }
        }

        /// <summary>
        /// Gets the count of the enumerator.
        /// </summary>
        public int Count
        {
            get
            {
                lock (this._lock)
                {
                    return this._dic.Count;
                }
            }
        }

        /// <summary>
        /// The collection is synchronised. This will always return true.
        /// </summary>
        public bool IsSynchronized
        {
            get
            {
                lock (this._lock)
                {
                    return true;
                }
            }
        }

        /// <summary>
        /// Returns the sync root.
        /// </summary>
        public object SyncRoot
        {
            get { return this._lock; }
        }

        /// <summary>
        /// Starts the processing for an item that will be placed in the dictionary.
        /// </summary>
        /// <param name="key">The key of the item that will be placed in here.</param>
        /// <returns><code>true</code> if not already processing, <code>false</code> otherwise.</returns>
        public bool StartProcessing(Guid key)
        {
            lock (this._lock)
            {
                if (this._processing.Contains(key))
                    return false;
                this._processing.Add(key);
            }
            return true;
        }

        /// <summary>
        /// Removes the key from the processing list.
        /// </summary>
        /// <param name="item">The item that has the key to remove.</param>
        protected void _EndProcessing(T item)
        {
            lock (this._lock)
            {
                if (this._processing.Contains(item.Key))
                    this._processing.Remove(item.Key);
            }
        }

        /// <summary>
        /// Checks to see if the dictionary contains the key or the item for that key is currently being processed.
        /// </summary>
        /// <param name="key">The key in question.</param>
        /// <returns><code>true</code> if it is, <code>false</code> otherwise.</returns>
        public bool ContainsOrIsProcessing(Guid key)
        {
            lock (this._lock)
            {
                if (this._dic.ContainsKey(key))
                    return true;
                if (this._processing.Contains(key))
                    return true;
                return false;
            }
        }

        /// <summary>
        /// Checks to see if the key needs to be processed.
        /// </summary>
        /// <param name="key">The key in question.</param>
        /// <returns><code>true</code> if it is, <code>false</code> otherwise.</returns>
        public bool NeedsProcessing(Guid key)
        {
            lock (this._lock)
            {
                if (!this._dic.ContainsKey(key) && !this._processing.Contains(key))
                    return true;
                return false;
            }
        }

        /// <summary>
        /// Checks to see if the key is processing.
        /// </summary>
        /// <param name="key">The key in question.</param>
        /// <returns><code>true</code> if it is, <code>false</code> otherwise.</returns>
        public bool IsProcessing(Guid key)
        {
            lock (this._lock)
            {
                if (this._processing.Contains(key))
                    return true;
                return false;
            }
        }


        /// <summary>
        /// Adds the value to the database. If the value's key already exists, it overwrites it.
        /// </summary>
        /// <param name="value">The value to add.</param>
        public void Add(T value)
        {
            lock (this._lock)
            {
                this._dic[value.Key] = value;
                _EndProcessing(value);
            }
        }

        /// <summary>
        /// Removes the specified value from the dictionary.
        /// </summary>
        /// <param name="value">The value to remove.</param>
        public void Remove(T value)
        {
            lock (this._lock)
            {
                this._dic.Remove(value.Key);
            }
        }

        /// <summary>
        /// Gets the item T based on the T.
        /// </summary>
        /// <param name="key">The key of the item.</param>
        /// <returns>The item found or the default value of <code>T</code>.</returns>
        public T Get(Guid key)
        {
            if (this._processing.Contains(key))
                return null;
            lock (this._lock)
            {
                if (this._dic.ContainsKey(key))
                    return _Get(key);
                return null;
            }
        }

        public async Task<T> GetAsync(Guid key)
        {
            while (this._processing.Contains(key))
                await Task.Delay(10000);
            lock (this._lock)
            {
                if (this._dic.ContainsKey(key))
                    return _Get(key);
                return null;
            }
        }

        /// <summary>
        /// Gets the item from the database without checking for anything.
        /// </summary>
        /// <param name="key">The key of the item to get.</param>
        /// <returns>The item or null if it doesn't exist.</returns>
        protected T _Get(Guid key)
        {
            lock (this._lock)
                return this._dic[key] as T;
        }

        /// <summary>
        /// Checks to see if the dictionary contains a key.
        /// </summary>
        /// <param name="key">The key to check for.</param>
        /// <returns><code>true</code> if it does, <code>false</code> otherwise.</returns>
        public bool ContainsKey(Guid key)
        {
            lock (this._lock)
            {
                return this._dic.ContainsKey(key);
            }
        }

        /// <summary>
        /// Gets an <code>ICollection</code> of keys.
        /// </summary>
        public ICollection<Guid> Keys
        {
            get
            {
                lock (this._lock)
                {
                    return this._dic.Keys;
                }
            }
        }

        /// <summary>
        /// Removes the key from the database.
        /// </summary>
        /// <param name="key">The key of the item in the <code>Dictionary</code>.</param>
        /// <returns><code>true</code> if the key was found, <code>false</code> otherwise.</returns>
        public bool Remove(Guid key)
        {
            lock (this._lock)
            {
                return this._dic.Remove(key);
            }
        }

        /// <summary>
        /// Gets the values of the dictionary.
        /// </summary>
        public IEnumerable<T> Values
        {
            get
            {
                lock (this._lock)
                {
                    return this._dic.Values.OfType<T>();
                }
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            lock (this._lock)
            {
                var enumerator = this._dic.GetEnumerator();
                while (enumerator.MoveNext())
                    yield return enumerator.Current.Value as T;
            }
        }

        #region IEnumerable<KeyValuePair<Guid, IKeepAlive>>
        /// <summary>
        /// Gets the enumerator with items <code>KeyValuePair</code>.
        /// </summary>
        /// <returns>returns the <code>IEnumerator</code>.</returns>
        IEnumerator<KeyValuePair<Guid, IKeepAlive>> IEnumerable<KeyValuePair<Guid, IKeepAlive>>.GetEnumerator()
        {
            lock (this._lock)
            {
                return this._dic.Cast<KeyValuePair<Guid, IKeepAlive>>().GetEnumerator();
            }
        }

        /// <summary>
        /// Gets the enumerator with items <code>KeyValuePair</code>.
        /// </summary>
        /// <returns>returns the <code>IEnumerator</code>.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            lock (this._lock)
            {
                return this._dic.GetEnumerator();
            }
        }
        #endregion

        #region IDictionary<Guid, IKeepAlive>
        void IDictionary<Guid, IKeepAlive>.Add(Guid key, IKeepAlive value)
        {
            lock (this._lock)
            {
                this._dic.Add(value.Key, value);
            }
        }

        ICollection<IKeepAlive> IDictionary<Guid, IKeepAlive>.Values
        {
            get
            {
                lock (this._lock)
                {
                    return this._dic.Values;
                }
            }
        }

        IKeepAlive IDictionary<Guid, IKeepAlive>.this[Guid key]
        {
            get
            {
                lock (this._lock)
                {
                    return this._dic[key];
                }
            }
            set
            {
                var item = value as T;
                if (item != null)
                {
                    lock (this._lock)
                    {
                        this._dic[key] = item;
                    }
                }
            }
        }

        /// <summary>
        /// Tries to get the value from the <code>IDictionary</code>
        /// </summary>
        /// <param name="key">The key of the values</param>
        /// <param name="value"></param>
        /// <returns></returns>
        bool IDictionary<Guid, IKeepAlive>.TryGetValue(Guid key, out IKeepAlive value)
        {
            lock (this._lock)
            {
                return this._dic.TryGetValue(key, out value);
            }
        }
        #endregion

        #region ICollection<KeyValuePair<Guid, IKeepAlive>>
        void ICollection<KeyValuePair<Guid, IKeepAlive>>.Add(KeyValuePair<Guid, IKeepAlive> item)
        {
            lock (this._lock)
            {
                this._dic.Add(item.Key, item.Value as T);
            }
        }

        bool ICollection<KeyValuePair<Guid, IKeepAlive>>.Contains(KeyValuePair<Guid, IKeepAlive> item)
        {
            lock (this._lock)
            {
                return this._dic.ContainsKey(item.Key) && this._dic.ContainsValue(item.Value as T);
            }
        }

        void ICollection<KeyValuePair<Guid, IKeepAlive>>.CopyTo(KeyValuePair<Guid, IKeepAlive>[] array, int arrayIndex)
        {
            lock (this._lock)
            {
                this._dic.Cast<KeyValuePair<Guid, IKeepAlive>>().ToList().CopyTo(array, arrayIndex);
            }
        }

        bool ICollection<KeyValuePair<Guid, IKeepAlive>>.IsReadOnly
        {
            get
            {
                lock (this._lock)
                {
                    return (this._dic as ICollection<KeyValuePair<Guid, T>>).IsReadOnly;
                }
            }
        }

        bool ICollection<KeyValuePair<Guid, IKeepAlive>>.Remove(KeyValuePair<Guid, IKeepAlive> item)
        {
            lock (this._lock)
            {
                return this._dic.Remove(item.Key);
            }
        }
        #endregion

        #region IKeepAliveCollection
        void IKeepAliveCollection.Remove(IKeepAlive value)
        {
            lock (this._lock)
            {
                this._dic.Remove(value.Key);
            }
        }

        IKeepAlive IKeepAliveCollection.Get(Guid key)
        {
            lock (this._lock)
            {
                return this._dic[key] as T;
            }
        }

        void IKeepAliveCollection.Add(IKeepAlive item)
        {
            lock (this._lock)
            {
                this.Add(item as T);
            }
        }
        #endregion

        IEnumerator<IKeepAlive> IEnumerable<IKeepAlive>.GetEnumerator()
        {
            lock (this._lock)
            {
                return Values.Cast<IKeepAlive>().GetEnumerator();
            }
        }
    }
}
