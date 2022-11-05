using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RSToolKit.Domain.Data;

namespace RSToolKit.Domain.Entities.Navigation
{
    /// <summary>
    /// A collection of <code>Pheromone</code> items that use the FILO principle.
    /// </summary>
    public class Trail<T>
        : IEnumerable<T>
        where T : class, ITrailItem
    {
        /// <summary>
        /// The array of ITrailItems
        /// </summary>
        protected T[] _items;
        /// <summary>
        /// The oldest item.
        /// </summary>
        protected int _head;
        /// <summary>
        /// The current location to place the next item.
        /// </summary>
        protected int _tail;
        /// <summary>
        /// The position of the last item added.
        /// </summary>
        protected int _position;
        /// <summary>
        /// The size of the array.
        /// </summary>
        protected int _size;
        /// <summary>
        /// The count of items in the array.
        /// </summary>
        protected int _count;
        /// <summary>
        /// The max size allowable for the array.
        /// </summary>
        protected int _maxSize;
        /// <summary>
        /// The grow factor to multiply the current size by when growing.
        /// </summary>
        protected float _growFactor;

        /// <summary>
        /// Gets the current count of the <code>ITrailItem</code>s.
        /// </summary>
        public int Count { get { return this._count; } }

        /// <summary>
        /// Initializes an empty <code>Trail</code>.
        /// <remarks>The grow factor is 1.2, the max size is <code>int.MaxValue / 2</code>, and the starting size is 10.</remarks>
        /// </summary>
        public Trail()
        {
            this._count = 0;
            this._size = 10;
            this._maxSize = int.MaxValue / 2;
            this._position = 0;
            this._head = 0;
            this._tail = 0;
            this._growFactor = 1.2F;
            this._items = new T[this._size];
        }

        /// <summary>
        /// Initializes the class with the specified collection. The size becomes the count of the collection times the growth factor.
        /// </summary>
        /// <param name="collection">The collection to use to fill the <code>Trail</code>.</param>
        public Trail(IEnumerable<T> collection)
            : base()
        {
            this._count = collection.Count();
            this._size = (int)Math.Ceiling(this._count * _growFactor);
            if (this._size < 10)
                this._size = 10;
            if (this._size > this._maxSize)
                this._maxSize = this._size;
            this._items = new T[this._size];
            for (var i = 0; i < collection.Count(); i++)
                this._items[i] = collection.ElementAt(i);
            this._position = this._count - 1;
            this._tail = this._count % this._items.Length;
        }

        /// <summary>
        /// Initializes the class with the specified collection and max size. The size becomes the count of the collection times the growth factor or the max size.
        /// </summary>
        /// <param name="collection">The collection to use to fill the <code>Trail</code>.</param>
        /// <param name="maxSize">The max size to use.</param>
        public Trail(IEnumerable<T> collection, int? maxSize)
            : base()
        {
            this._maxSize = maxSize ?? this._maxSize;
            this._count = collection.Count();
            this._size = (int)Math.Ceiling(this._count * _growFactor);
            if (this._size < 10)
                this._size = 10;
            if (this._size > this._maxSize)
                this._maxSize = this._size;
            this._items = new T[this._size];
            for (var i = 0; i < collection.Count(); i++)
                this._items[i] = collection.ElementAt(i);
            this._position = this._count - 1;
            this._tail = this._count % this._items.Length;
        }

        /// <summary>
        /// Initializes the class with the specified values.
        /// </summary>
        /// <param name="growFactor">The grow factor to use.</param>
        /// <param name="initialSize">The initial size of the array.</param>
        public Trail (int growFactor, int initialSize)
            : base()
        {
            this._growFactor = growFactor;
            this._size = initialSize;
            this._items = new T[this._size];
        }

        /// <summary>
        /// Initializes the class with the max size.
        /// </summary>
        /// <param name="maxSize">The max size to use.</param>
        public Trail (int maxSize)
            : base()
        {
            this._maxSize = maxSize;
        }
        
        /// <summary>
        /// Adds the item to the list.
        /// </summary>
        /// <param name="item">The item to add.</param>
        public void Push(T item)
        {
            if (_size == _count && _size < _maxSize)
                this._Grow();
            this._items[_tail] = item;
            this._count++;
            this._position = _tail;
            this._tail = (this._tail + 1) % this._items.Length;
            if (this._count >= this._items.Length)
            {
                if (this._count > this._items.Length)
                    this._count--;
                this._head = (this._head + 1) % this._items.Length;
            }
        }

        /// <summary>
        /// Removes the oldest item.
        /// </summary>
        /// <returns>The item removed.</returns>
        public T Pop()
        {
            if (this._count == 0)
                return null;
            var item = this._items[this._head];
            this._items[this._head] = null;
            this._count--;
            this._head = (this._head + 1) % this._items.Length;
            return item;
        }

        /// <summary>
        /// Rips off the most recent item.
        /// </summary>
        /// <returns>Returns the item ripped off.</returns>
        public T Rip()
        {
            if (this._count == 0)
                return null;
            var item = this._items[this._position];
            this._tail = this._position;
            this._position = (this._tail - 1) % this._items.Length;
            return item;
        }

        /// <summary>
        /// Gets the head item without removing it.
        /// </summary>
        /// <returns>The item retrieved.</returns>
        public T Peek()
        {
            return this._items[this._head];
        }

        /// <summary>
        /// Gets the last item without removing it.
        /// </summary>
        /// <returns>The item retrieved.</returns>
        public T PeekTail()
        {
            if (this._position == -1)
                return null;
            return this._items[this._position];
        }

        /// <summary>
        /// Gets the array of items.
        /// </summary>
        /// <returns>The items in an array.</returns>
        public T[] GetArray()
        {
            return this._GetArray();
        }

        /// <summary>
        /// Increases the size of the underlying array.
        /// </summary>
        protected void _Grow()
        {
            if (this._size == this._maxSize)
                return;
            this._size = (int)Math.Ceiling(this._size * this._growFactor);
            if (this._size > this._maxSize)
                this._size = this._maxSize;
            this._items = this._GetArray(_size);
        }

        /// <summary>
        /// Copies the old array to a new array in the right order and starts them from index 0 in the returned array.
        /// </summary>
        /// <param name="size">The size of the new array.</param>
        /// <returns>The new array.</returns>
        protected T[] _GetArray(int? size = null)
        {
            size = size ?? this._count;
            var items = new T[size.Value];
            if (this._count == 0)
                return items;
            var firstPart = (this._items.Length - this._head < this._count) ? this._items.Length - this._head : this._count;
            Array.Copy(this._items, this._head, items, 0, firstPart);
            var secondPart = this._count - firstPart;
            if (secondPart > 0)
                Array.Copy(this._items, 0, items, this._items.Length - this._head, secondPart);
            this._head = 0;
            this._tail = _count;
            this._position = _count - 1;
            this._items = items;
            return items;
        }

        #region IEnumerable
        /// <summary>
        /// Gets the Enumerator for the class.
        /// </summary>
        /// <returns></returns>
        public IEnumerator<T> GetEnumerator()
        {
            foreach (var item in this._GetArray())
                yield return item;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        #endregion
    }
}
