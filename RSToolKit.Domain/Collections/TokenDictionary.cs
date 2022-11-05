using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RSToolKit.Domain.Data;
using System.Collections;

namespace RSToolKit.Domain.Collections
{
    /// <summary>
    /// Holds tokens.
    /// </summary>
    public class TokenDictionary<T>
        : IDictionary<Guid, JsonTableToken<T>>, ITokenDictionary
        where T : class, ITableInformation
    {
        /// <summary>
        /// The dictionary for the items.
        /// </summary>
        protected Dictionary<Guid, JsonTableToken<T>> _dic;
        /// <summary>
        /// Holds the <code>ITableInformation</code>.
        /// </summary>
        protected Dictionary<Guid, T> _info;

        /// <summary>
        /// The lock for thread safe activities.
        /// </summary>
        protected object _lock;

        /// <summary>
        /// Initializes the class.
        /// </summary>
        public TokenDictionary()
        {
            this._dic = new Dictionary<Guid, JsonTableToken<T>>();
            this._info = new Dictionary<Guid, T>();
            this._lock = new object();
        }

        /// <summary>
        /// Initializes the class with a set of data.
        /// </summary>
        /// <param name="collection">The data to initialize the class with.</param>
        public TokenDictionary(IDictionary<Guid, JsonTableToken<T>> collection)
            : this()
        {
            this._dic = new Dictionary<Guid, JsonTableToken<T>>(collection);
            this._info = new Dictionary<Guid, T>();
            foreach (var kvp in this._dic)
                if (kvp.Value.Item != null)
                    this._info.Add(kvp.Value.Item.Key, kvp.Value.Item);
        }

        /// <summary>
        /// Clears the collection of tokens.
        /// </summary>
        /// <param name="clearTables">Set to true to cleat TableInformation as well as tokens.</param>
        public void Clear(bool clearTables)
        {
            lock (this._lock)
            {
                this._dic.Clear();
                if (clearTables)
                    this._info.Clear();
            }
        }

        /// <summary>
        /// Clears the collection of tokens.
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
                return true;
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
        /// Creates an IToken and adds it to the dictionary.
        /// </summary>
        /// <param name="itemKey">The key of the table information.</param>
        /// <returns></returns>
        public JsonTableToken<T> Create(Guid itemKey)
        {
            lock (this._lock)
            {
                var info = this.GetInfo(itemKey);
                JsonTableToken<T> token = new JsonTableToken<T>(Guid.NewGuid(), itemKey, info);
                this._dic.Add(token.Key, token);
                return token;
            }
        }

        /// <summary>
        /// Removes the specified value from the dictionary.
        /// </summary>
        /// <param name="value">The value to remove.</param>
        public void Remove(JsonTableToken<T> token)
        {
            lock (this._lock)
            {
                this._dic.Remove(token.Key);
            }
        }

        /// <summary>
        /// Gets the item T based on the T.
        /// </summary>
        /// <param name="key">The key of the item.</param>
        /// <returns>The item found or the default value of <code>T</code>.</returns>
        public JsonTableToken<T> Get(Guid key)
        {
            lock (this._lock)
            {
                if (this._dic.ContainsKey(key))
                    return _Get(key);
                return null;
            }
        }

        /// <summary>
        /// Gets the IToken asynchronously.
        /// </summary>
        /// <param name="key">The key of the item.</param>
        /// <returns>The IToken.</returns>
        public async Task<JsonTableToken<T>> GetAsync(Guid key)
        {
            return await Task.Run(() => this.Get(key));
        }

        /// <summary>
        /// Gets the item from the database without checking for anything.
        /// </summary>
        /// <param name="key">The key of the item to get.</param>
        /// <returns>The item or null if it doesn't exist.</returns>
        protected JsonTableToken<T> _Get(Guid key)
        {
            lock (this._lock)
                return this._dic[key];
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
        public IEnumerable<JsonTableToken<T>> Values
        {
            get
            {
                lock (this._lock)
                {
                    return this._dic.Values;
                }
            }
        }

        /// <summary>
        /// Gets the enumerator.
        /// </summary>
        /// <returns>The IEnumerator.</returns>
        public IEnumerator<JsonTableToken<T>> GetEnumerator()
        {
            lock (this._lock)
            {
                var enumerator = this._dic.GetEnumerator();
                while (enumerator.MoveNext())
                    yield return enumerator.Current.Value;
            }
        }

        /// <summary>
        /// Moves forward one tick for the specified ITableInformation.
        /// </summary>
        /// <param name="key">The key of the ITableInformation.</param>
        /// <param name="message">The message of the tick.</param>
        /// <param name="details">The details of the tick.</param>
        /// <returns>The new progress.</returns>
        public float Tick(Guid key, string message = "Processing", string details = null)
        {
            lock (this._lock)
            {
                if (_info.ContainsKey(key))
                    return _info[key].Tick(message, details);
            }
            return 1;
        }

        /// <summary>
        /// Sets the tick interval based on the total number of ticks to be used.
        /// </summary>
        /// <param name="key">The key of the ITableInformation to use.</param>
        /// <param name="totalTicks">The total number of ticks.</param>
        /// <returns></returns>
        public float SetTickFraction(Guid key, long totalTicks)
        {
            lock (this._lock)
            {
                if (this._info.ContainsKey(key))
                    return this._info[key].SetTickFraction(totalTicks);
            }
            return 1;
        }

        /// <summary>
        /// Resets the progress back to zero.
        /// </summary>
        /// <param name="key">The key of the ITableInformation.</param>
        /// <param name="message">[optional] The message to set.</param>
        /// <param name="details">[optional] The details to set.</param>
        public void Reset(Guid key, string message = null, string details = null)
        {
            lock (this._lock)
            {
                if (this._info.ContainsKey(key))
                    this._info[key].Reset(message, details);
            }
        }

        /// <summary>
        /// Updates the message for a specified key.
        /// </summary>
        /// <param name="key">The key of the ITableInformation.</param>
        /// <param name="message">The message of the tick.</param>
        /// <param name="details">The details of the tick.</param>
        public void UpdateMessage(Guid key, string message = "Processing", string details = null)
        {
            lock (this._lock)
            {
                if (this._info.ContainsKey(key))
                    this._info[key].UpdateMessage(message, details);
            }
        }

        /// <summary>
        /// Gets a list of ITableInformation items.
        /// </summary>
        /// <returns>The tables.</returns>
        public IEnumerable<T> Tables()
        {
            return this._info.Values;
        }

        #region ITokenDictionary

        void ITokenDictionary.Add(ITableInformation table)
        {
            if (table is T)
            {
                AddInfo(table as T);
            }
        }

        #endregion

        #region Info
        /// <summary>
        /// Gets an ITableInformation.
        /// </summary>
        /// <param name="key">The key to search with.</param>
        /// <returns>The information.</returns>
        public T GetInfo(Guid key)
        {
            lock (this._lock)
            {
                if (this._info.ContainsKey(key))
                    return this._info[key];
                return null;
            }
        }

        /// <summary>
        /// Adds a ITableInformation.
        /// </summary>
        /// <param name="info">The info to add.</param>
        public void AddInfo(T info)
        {
            lock (this._lock)
            {
                if (this._info.ContainsKey(info.Key))
                {
                    // We need to remove it.
                    this._info.Remove(info.Key);
                }
                this._info[info.Key] = info;
            }
        }

        /// <summary>
        /// Removes the info from the dictionary.
        /// </summary>
        /// <param name="info">The info to remove.</param>
        public void RemoveInfo(T info)
        {
            lock (this._lock)
            {
                if (this._info.ContainsValue(info))
                    this._info.Remove(info.Key);
                var itemsToRemove = this._dic.Values.Where(v => v.ItemKey == info.Key).Select(v => v.Key);
                foreach (var token in itemsToRemove)
                    this._dic.Remove(token);                  
            }
        }

        /// <summary>
        /// Remove the info based on the key.
        /// </summary>
        /// <param name="key">The key.</param>
        public void RemoveInfo(Guid key)
        {
            lock (this._lock)
            {
                if (this._info.ContainsKey(key))
                    this._info.Remove(key);
                this._dic.Where(kvp => kvp.Value.ItemKey == key).Select(kvp => kvp.Key).ToList().ForEach(k => this._dic.Remove(k));
            }
        }
        #endregion

        #region KeepAlive
        /// <summary>
        /// Kills table information that haven't been used in x amount of time. The interval defaults to 600 minutes.
        /// </summary>
        /// <param name="interval">The interval to use for killing items.</param>
        public void KillItems(TimeSpan? interval = null)
        {
            interval = interval ?? TimeSpan.FromMinutes(600);
            var removedInfos = new List<ITableInformation>();
            foreach (var kvp in this._info.Where(i => DateTime.Now - i.Value.LastActivity > interval))
                removedInfos.Add(kvp.Value);
            foreach (var info in removedInfos)
            {
                this._info.Remove(info.Key);
                this._dic.Where(kvp => kvp.Value.ItemKey == info.Key).ToList().ForEach(kvp => this._dic.Remove(kvp.Key));
            }
        }
        #endregion

        #region IEnumerable<KeyValuePair<Guid, Token<T>>>
        IEnumerator<KeyValuePair<Guid, JsonTableToken<T>>> IEnumerable<KeyValuePair<Guid, JsonTableToken<T>>>.GetEnumerator()
        {
            lock (this._lock)
            {
                return this._dic.Cast<KeyValuePair<Guid, JsonTableToken<T>>>().GetEnumerator();
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            lock (this._lock)
            {
                return this._dic.GetEnumerator();
            }
        }
        #endregion

        #region IDictionary<Guid, Token<T>>
        void IDictionary<Guid, JsonTableToken<T>>.Add(Guid key, JsonTableToken<T> value)
        {
            throw new InvalidOperationException("You cannot add an item.");
        }

        ICollection<JsonTableToken<T>> IDictionary<Guid, JsonTableToken<T>>.Values
        {
            get
            {
                lock (this._lock)
                {
                    return this._dic.Values;
                }
            }
        }

        JsonTableToken<T> IDictionary<Guid, JsonTableToken<T>>.this[Guid key]
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
                var item = value;
                if (item != null)
                {
                    lock (this._lock)
                    {
                        this._dic[key] = item;
                    }
                }
            }
        }

        bool IDictionary<Guid, JsonTableToken<T>>.TryGetValue(Guid key, out JsonTableToken<T> value)
        {
            lock (this._lock)
            {
                return this._dic.TryGetValue(key, out value);
            }
        }
        #endregion

        #region ICollection<KeyValuePair<Guid, Token<T>>>
        void ICollection<KeyValuePair<Guid, JsonTableToken<T>>>.Add(KeyValuePair<Guid, JsonTableToken<T>> item)
        {
            lock (this._lock)
            {
                this._dic.Add(item.Key, item.Value);
            }
        }

        bool ICollection<KeyValuePair<Guid, JsonTableToken<T>>>.Contains(KeyValuePair<Guid, JsonTableToken<T>> item)
        {
            lock (this._lock)
            {
                return this._dic.ContainsKey(item.Key) && this._dic.ContainsValue(item.Value);
            }
        }

        void ICollection<KeyValuePair<Guid, JsonTableToken<T>>>.CopyTo(KeyValuePair<Guid, JsonTableToken<T>>[] array, int arrayIndex)
        {
            lock (this._lock)
            {
                this._dic.Cast<KeyValuePair<Guid, JsonTableToken<T>>>().ToList().CopyTo(array, arrayIndex);
            }
        }

        bool ICollection<KeyValuePair<Guid, JsonTableToken<T>>>.IsReadOnly
        {
            get
            {
                lock (this._lock)
                {
                    return (this._dic as ICollection<KeyValuePair<Guid, JsonTableToken<T>>>).IsReadOnly;
                }
            }
        }

        bool ICollection<KeyValuePair<Guid, JsonTableToken<T>>>.Remove(KeyValuePair<Guid, JsonTableToken<T>> item)
        {
            lock (this._lock)
            {
                return this._dic.Remove(item.Key);
            }
        }
        #endregion
    }
}
