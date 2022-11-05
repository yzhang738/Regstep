using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RSToolKit.Domain.Data;

namespace RSToolKit.Domain.Collections
{
    public class ComplexDictionary<T>
        : KeepAliveDictionary<T>, IComplexDictionary
        where T : class, IUpdatable
    {
        /// <summary>
        /// Holds progress information for the key.
        /// </summary>
        protected Dictionary<Guid, float> _progress;

        /// <summary>
        /// Creates a new complex dictionary.
        /// </summary>
        public ComplexDictionary()
            : base()
        {
            this._progress = new Dictionary<Guid, float>();
        }

        IUpdatable IUpdatableCollection.Get(Guid key)
        {
            lock (this._lock)
            {
                return this._dic[key] as IUpdatable;
            }
        }

        /// <summary>
        /// Gets the progress for the current key.
        /// </summary>
        /// <param name="key">The key to update the progress.</param>
        /// <returns>A float of the progress from 0 to 1.</returns>
        public float GetProgress(Guid key)
        {
            lock (this._lock)
            {
                if (this._progress.ContainsKey(key))
                    return this._progress[key];
                if (this._processing.Contains(key))
                    return 0.25F;
                if (!this._dic.ContainsKey(key))
                    return 0.1F;
            }
            return 1;
        }

        /// <summary>
        /// Updates the progress for the key.
        /// </summary>
        /// <param name="key">The key to associate with.</param>
        /// <param name="percent">The percent of progress form 0 to 1.</param>
        public void UpdateProgress(Guid key, float percent)
        {
            lock (this._lock)
            {
                if (percent >= 1 && this._progress.ContainsKey(key))
                    this._progress.Remove(key);
                this._progress[key] = percent;
            }
        }

        IEnumerator<IUpdatable> IEnumerable<IUpdatable>.GetEnumerator()
        {
            lock (this._lock)
            {
                var enumerator = this._dic.Values.GetEnumerator();
                while (enumerator.MoveNext())
                    yield return enumerator.Current as IUpdatable;
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
