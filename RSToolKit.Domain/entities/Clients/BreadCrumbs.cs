using System;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Diagnostics.Contracts;
using System.Diagnostics;
using System.Security.Permissions;
using System.Collections.Generic;

namespace RSToolKit.Domain.Entities.Clients
{
    /// <summary>
    /// Holds <code>Crumb</code>s in a FIFO collection.
    /// </summary>
    public class BreadCrumbs
        : ICollection, ICloneable, IEnumerable<Crumb>
    {

        #region Private Variables

        internal Crumb[] _crumb;
        internal int _head;
        internal int _tail;
        internal int _size;
        internal int _growFactor;
        internal int _maxSize;
        internal int _version;
        internal int _lastTail;
        [NonSerialized]
        internal Object _syncroot;

        internal const int _MinimumGrow = 4;
        internal const int _ShrinkThreshold = 32;

        #endregion

        #region Properties

        /// <summary>
        /// The count of the held <code>Crumb</code>s.
        /// </summary>
        public virtual int Count
        {
            get { return _size; }
        }

        /// <summary>
        /// Whether the <code>BreadCrumbs</code> is synchronized or not.
        /// </summary>
        public virtual bool IsSynchronized
        {
            get { return false; }
        }

        /// <summary>
        /// Gets the synchronized root.
        /// </summary>
        public virtual Object SyncRoot
        {
            get
            {
                if (_syncroot == null)
                    System.Threading.Interlocked.CompareExchange(ref _syncroot, new Object(), null);
                return _syncroot;
            }
        }
        #endregion

        #region Contructors

        /// <summary>
        /// Initiates the class.
        /// <code>capacity: 10</code>
        /// <code>maxSize: 10</code>
        /// <code>growFactor: (float)2.0</code>
        /// </summary>
        public BreadCrumbs()
            : this(10, 10, (float)2.0)
        { }

        /// <summary>
        /// Initiates the class with the specified values.
        /// <code>maxSize: 10</code>
        /// <code>growFactor: (float)2.0</code>
        /// </summary>
        /// <param name="capacity">The initial capacity.</param>
        public BreadCrumbs(int capacity)
            : this(capacity, 10, (float)2.0)
        { }

        /// <summary>
        /// Initiates the class with the specified values.
        /// <code>growFactor: (float)2.0</code>
        /// </summary>
        /// <param name="capacity">The initial capacity.</param>
        /// <param name="maxSize">The max size.</param>
        public BreadCrumbs(int capacity, int maxSize)
            : this (capacity, maxSize, (float)2.0)
        { }

        /// <summary>
        /// Initiates the class with the specified attributes.
        /// </summary>
        /// <param name="capacity">The initial capacity.</param>
        /// <param name="maxSize">The maxSize.</param>
        /// <param name="growFactor">The growth factor.</param>
        public BreadCrumbs(int capacity, int maxSize, float growFactor)
        {
            if (capacity < 0)
                throw new ArgumentOutOfRangeException("capacity", "Need a non negative number.");
            if (maxSize < 0)
                throw new ArgumentOutOfRangeException("maxSize", "Need a non negative number.");
            if (!(growFactor >= 1.0 && growFactor <= 10.0))
                throw new ArgumentOutOfRangeException("growFactor", "The number must be between 1 and 10");
            Contract.EndContractBlock();

            _crumb = new Crumb[capacity];
            _maxSize = maxSize;
            _head = 0;
            _tail = 0;
            _size = 0;
            _lastTail = 0;
            _growFactor = (int)(growFactor * 100);
        }
        
        /// <summary>
        /// Initiates the class with the specified values.
        /// <code>capacity: 10</code>
        /// <code>maxSize: 10</code>
        /// <code>growFactor: (float)2.0</code>
        /// </summary>
        /// <param name="col">The collection to use to initialize the object.</param>
        public BreadCrumbs(ICollection col)
            : this((col == null ? 32 : col.Count))
        {
            if (col == null)
                throw new ArgumentNullException("col", "The collection cannot be null.");
            if (col.GetType().GetGenericArguments()[0] == typeof(Crumb))
                throw new ArgumentException("col", "The collection must be of type Crumb.");
            Contract.EndContractBlock();
        }

        #endregion

        /// <summary>
        /// Clones the object.
        /// </summary>
        /// <returns>A clone of <code>BreadCrumb</code> cast as an <code>Object</code>.</returns>
        public virtual Object Clone()
        {
            BreadCrumbs bc = new BreadCrumbs(_size, _maxSize);
            bc._size = _size;
            int numToCopy = _size;
            int firstPart = (_crumb.Length - _head < numToCopy) ? _crumb.Length - _head : numToCopy;
            Array.Copy(_crumb, _head, bc._crumb, 0, firstPart);
            numToCopy -= firstPart;
            if (numToCopy > 0)
                Array.Copy(_crumb, 0, bc._crumb, _crumb.Length - _head, numToCopy);
            bc._version = _version;
            return bc;
        }

        /// <summary>
        /// Clears the object of <code>Crumb</code>s.
        /// </summary>
        public virtual void Clear()
        {
            if (_head < _tail)
                Array.Clear(_crumb, _head, _tail);
            else
            {
                Array.Clear(_crumb, _head, _crumb.Length - _head);
                Array.Clear(_crumb, 0, _tail);
            }
            _head = 0;
            _tail = 0;
            _size = 0;
            _version++;
        }

        /// <summary>
        /// Copies the current object to the specified array.
        /// </summary>
        /// <param name="array">The array to copy to.</param>
        /// <param name="index">The index to start at.</param>
        public virtual void CopyTo(Array array, int index)
        {
            if (array == null)
                throw new ArgumentNullException("array", "Cannot be null."); ;
            if (array.Rank != 1)
                throw new ArgumentException("array", "The array must be of single dimension.");
            if (array.GetType().GetGenericArguments()[0] != typeof(Crumb))
                throw new ArgumentException("array", "The array must be of type Crumb.");
            if (index < 0)
                throw new ArgumentException("index", "The index must be non negative.");
            Contract.EndContractBlock();
            int arrayLen = array.Length;
            if (arrayLen - index < _size)
                throw new ArgumentException("Invalid index marker.  The index cannot be larger than the length.");
            int numToCopy = _size;
            if (numToCopy == 0)
                return;
            int firstPart = (_crumb.Length - _head < numToCopy) ? _crumb.Length - _head : numToCopy;
            Array.Copy(_crumb, _head, array, index, firstPart);
            numToCopy -= firstPart;
            if (numToCopy > 0)
                Array.Copy(_crumb, 0, array, index + _crumb.Length - _head, numToCopy);
        }

        /// <summary>
        /// Enques the specified <code>Crumb</code>.
        /// </summary>
        /// <param name="cr">The <code>Crumb</code> to add.</param>
        public virtual void Enqueue(Crumb cr)
        {
            if (_size == _crumb.Length)
            {
                int newCapacity = (int)((long)_crumb.Length * (long)_growFactor / 100);
                if (newCapacity < _crumb.Length + _MinimumGrow)
                    newCapacity = _crumb.Length + _MinimumGrow;
                if (newCapacity > _maxSize)
                        newCapacity = _maxSize;
                if (newCapacity == _size)
                    SetCapacity(newCapacity);
            }
            if (_size >= _maxSize)
                Dequeue();
            _crumb[_tail] = cr;
            _lastTail = _tail;
            _tail = (_tail + 1) % _crumb.Length;
            _size++;
            _version++;
        }

        /// <summary>
        /// Updates the last <code>Crumb</code> added.
        /// </summary>
        /// <param name="label">The label to use to update the crumb.</param>
        public virtual void UpdateLastLabel(string label)
        {
            _crumb[_lastTail].Label = label;
        }

        /// <summary>
        /// Peeks at the last <code>Crumb.</code>
        /// </summary>
        /// <returns>The last <code>Crumb</code> added.</returns>
        public virtual Crumb PeekLast()
        {
            return _crumb[_lastTail];
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new BreadCrumbsEnumerator(this);
        }

        /// <summary>
        /// Deques the oldest <code>Crumb</code>.
        /// </summary>
        /// <returns>The <code>Crumb</code> that was dequeued.</returns>
        public virtual Crumb Dequeue()
        {
            if (Count == 0)
                throw new InvalidOperationException("BreadCrumbs is empty and cannot be dequeued.");
            Contract.EndContractBlock();
            Crumb removed = _crumb[_head];
            _crumb[_head] = null;
            _head = (_head + 1) % _crumb.Length;
            _size--;
            _version++;
            return removed;
        }

        /// <summary>
        /// Trims the tail of the <code>BreadCrumbs</code> object.
        /// </summary>
        /// <param name="amount">The amount to trim.</param>
        public virtual void CutTail(int amount = 1)
        {
            Contract.EndContractBlock();
            if (amount >= Count)
                Clear();
            var _tail = _lastTail;
            while (amount != 0)
            {
                _crumb[_tail] = null;
                _tail = (_tail - 1) % _crumb.Length;
                amount--;
            }
            _tail = (_tail + 1) % _crumb.Length;
        }

        /// <summary>
        /// Peeks at the oldest <code>Crumb</code>.
        /// </summary>
        /// <returns>The <code>Crumb</code> that was added first.</returns>
        public virtual Crumb Peek()
        {
            if (Count == 0)
                throw new InvalidOperationException("BreadCrumbs is empty and cannot be peeked.");
            Contract.EndContractBlock();
            return _crumb[_head];
        }

        /// <summary>
        /// Gets a synchronized version of the <code>BreadCrumbs</code> object.
        /// </summary>
        /// <param name="bc">The <code>BreadCrumbs</code> object.</param>
        /// <returns>The <code>SynchronizedBreadCrumbs</code> cast as a <code>BreadCrumbs</code>.</returns>
        [HostProtection(Synchronization = true)]
        public static BreadCrumbs Synchronized(BreadCrumbs bc)
        {
            if (bc == null)
                throw new ArgumentNullException("bc", "The breadcrmbs cannot be null.");
            Contract.EndContractBlock();
            return new SynchronizedBreadCrumbs(bc);
        }

        /// <summary>
        /// Checks to see if the <code>Crumb</code> exists in the current object.
        /// </summary>
        /// <param name="c">The <code>Crumb</code> to check.</param>
        /// <returns><code>True</code> if it exists and <code>False</code> otherwise.</returns>
        public virtual bool Contains(Crumb c)
        {
            int index = _head;
            int count = _size;

            while (count-- > 0)
            {
                if (c == null)
                {
                    if (_crumb[index] == null)
                        return true;
                    else if (_crumb[index] != null && _crumb[index].Equals(c))
                    {
                        return true;
                    }
                    index = (index + 1) % _crumb.Length;
                }
            }
            return false;
        }

        /// <summary>
        /// Gets the element of the object.
        /// </summary>
        /// <param name="i">The zero based index of the item.</param>
        /// <returns>The <code>Crumb</code> that was found.</returns>
        internal Crumb GetElement(int i)
        {
            return _crumb[(_head + i) % _crumb.Length];
        }

        /// <summary>
        /// Gets the array version of the object.
        /// </summary>
        /// <returns>The array of <code>Crumbs</code>.</returns>
        public virtual Crumb[] ToArray()
        {
            var arr = new Crumb[_size];
            if (_size == 0)
                return arr;
            if (_head < _tail)
            {
                Array.Copy(_crumb, _head, arr, 0, _size);
            }
            else
            {
                Array.Copy(_crumb, _head, arr, 0, _crumb.Length - _head);
                Array.Copy(_crumb, 0, arr, _crumb.Length - _head, _tail);
            }
            return arr;
        }

        /// <summary>
        /// Sets the capacity of the object.
        /// </summary>
        /// <param name="capacity">The capacity of the object.</param>
        private void SetCapacity(int capacity)
        {
            var arr = new Crumb[capacity];
            if (_size > 0)
            {
                if (_head < _tail)
                {
                    Array.Copy(_crumb, _head, arr, 0, _size);
                }
                else
                {
                    Array.Copy(_crumb, _head, arr, 0, _crumb.Length - _head);
                    Array.Copy(_crumb, 0, arr, _crumb.Length - _head, _tail);
                }
            }
            _crumb = arr;
            _head = 0;
            _tail = (_size == capacity) ? 0 : _size;
            _version++;
        }

        /// <summary>
        /// Trims the size of the <code>BreadCrumbs</code> object.
        /// </summary>
        public virtual void TrimToSize()
        {
            SetCapacity(_size);
        }

        /// <summary>
        /// Gets a bread Trail
        /// </summary>
        /// <param name="urlHelper">The <code>UrlHelper</code> for building the urls.</param>
        /// <param name="amount">The amount of crumbs to use.</param>
        /// <param name="seperator">The seperator to use.</param>
        /// <param name="cssClasses">The css classes to use on the anchor tag.</param>
        /// <param name="wrapper">
        /// The html to wrap the anchor tag in.
        /// The {0} is used to denote where the anchor tag goes.
        /// The {1} is used to denote where the seperator goes.
        /// <example><div>{0}{1}</div></example>
        /// </param>
        /// <returns></returns>
        public string GetBreadTrail(System.Web.Mvc.UrlHelper urlHelper, int amount = 5, string seperator = "<span class=\"glyphicon glyphicon-chevron-right glyphicon-small\"></span>&nbsp;", string cssClasses = "", string wrapper = "{0}{1}")
        {
            var crumbs = ToArray().ToList();
            if (crumbs == null)
                return "";
            crumbs = crumbs.OrderByDescending(c => c.ActionDate).Take(amount).OrderBy(c => c.ActionDate).ToList();
            var links = new string[crumbs.Count];
            var r_string = "";
            for (var i = 0; i < crumbs.Count; i++)
            {
                var t_link = urlHelper.Action(crumbs[i].Action, crumbs[i].Controller, crumbs[i].RVD());
                links[i] = String.Format(wrapper, "<a id=\"crumb_" + crumbs[i].Id.ToString() + "\" href=\"" + t_link + "\" class=\"" + cssClasses + "\">" + crumbs[i].Label + "</a>", seperator);
            }
            for (var i = 0; i < crumbs.Count; i++)
            {
                if (i != 0)
                    r_string += seperator;
                r_string += links[i];
            }
            return r_string;
        }

        /// <summary>
        /// The enumerator to use for the <code>BreadCrumbs</code>.
        /// </summary>
        /// <returns></returns>
        public virtual IEnumerator<Crumb> GetEnumerator()
        {
            return new BreadCrumbsEnumerator(this);
        }
    }

    /// <summary>
    /// The synchonrized <code>BreadCrumbs</code> holder.
    /// </summary>
    public class SynchronizedBreadCrumbs
        : BreadCrumbs
    {
        private BreadCrumbs _c;
        private Object root;

        internal SynchronizedBreadCrumbs (BreadCrumbs c)
        {
            this._c = c;
            root = _c.SyncRoot;
        }

        public override bool IsSynchronized
        {
            get
            {
                return true;
            }
        }

        public override object SyncRoot
        {
            get
            {
                return root;
            }
        }

        public override int Count
        {
            get
            {
                lock (root)
                {
                    return _c.Count;
                }
            }
        }

        public override void Clear()
        {
            lock (root)
            {
                _c.Clear();
            }
        }

        public override Object Clone()
        {
            lock (root)
            {
                return new SynchronizedBreadCrumbs((BreadCrumbs)_c.Clone());
            }
        }

        public override bool Contains (Crumb c)
        {
            lock (root)
            {
                return _c.Contains(c);
            }
        }

        public override void CopyTo(Array array, int index)
        {
            lock (root)
            {
                _c.CopyTo(array, index);
            }
        }

        public override void Enqueue(Crumb cr)
        {
            lock (root)
            {
                _c.Enqueue(cr);
            }
        }

        public override Crumb Dequeue()
        {
            lock (root)
            {
                return _c.Dequeue();
            }
        }

        public override IEnumerator<Crumb> GetEnumerator()
        {
            lock (root)
            {
                return _c.GetEnumerator();
            }
        }

        public override Crumb Peek()
        {
            lock (root)
            {
                return _c.Peek();
            }
        }

        public override Crumb[] ToArray()
        {
            lock (root)
            {
                return _c.ToArray();
            }
        }

        public override void TrimToSize()
        {
            lock (root)
            {
                _c.TrimToSize();
            }
        }

    }

    /// <summary>
    /// The enumerator for <code>BreadCrumbs</code>.
    /// </summary>
    public class BreadCrumbsEnumerator
        : ICloneable, IEnumerator<Crumb>
    {
        private BreadCrumbs _c;
        private int _index;
        private int _version;
        private Object _currentElement;

        internal BreadCrumbsEnumerator(BreadCrumbs c)
        {
            _c = c;
            _version = _c._version;
            _index = 0;
            _currentElement = _c._crumb;
            if (_c._size == 0)
                _index = -1;
        }

        public Object Clone()
        {
            return MemberwiseClone();
        }

        public virtual bool MoveNext()
        {
            if (_version != _c._version)
                throw new InvalidOperationException("The version numbers do not match.");
            if (_index < 0)
            {
                _currentElement = _c._crumb;
                return false;
            }

            _currentElement = _c.GetElement(_index);
            _index++;

            if (_index == _c._size)
                _index = -1;
            return true;
        }

        Object IEnumerator.Current
        {
            get
            {
                return Current;
            }
        }

        public virtual void Reset()
        {
            if (_version != _c._version)
                throw new InvalidOperationException("The version of the BreadCrumbs has changed.");
            if (_c._size == 0)
                _index = -1;
            else
                _index = 0;
            _currentElement = _c._crumb;
        }

        public Crumb Current
        {
            get
            {
                if (_currentElement == _c._crumb)
                {
                    if (_index == 0)
                        throw new InvalidOperationException("The enumeration has not yet started.");
                    else
                        throw new InvalidOperationException("The enumeration has ended.");
                }
                return (Crumb)_currentElement;
            }
        }

        public void Dispose() { }
    }

    /// <summary>
    /// A class to hold all the values of the crumb.
    /// </summary>
    public class Crumb
    {
        internal string _label;
        
        /// <summary>
        /// The id of the crumb as a <code>Guid</code>.
        /// </summary>
        [JsonProperty("id")]
        public Guid Id { get; set; }
        /// <summary>
        /// The date the action happened.
        /// </summary>
        [JsonProperty("actionDate")]
        public DateTimeOffset ActionDate { get; set; }
        /// <summary>
        /// The action that happened.
        /// </summary>
        [JsonProperty("action")]
        public string Action { get; set; }
        /// <summary>
        /// The controller that handled the action.
        /// </summary>
        [JsonProperty("controller")]
        public string Controller { get; set; }
        /// <summary>
        /// The label of the action.
        /// </summary>
        [JsonProperty("label")]
        public string Label
        {
            get
            {
                if (String.IsNullOrWhiteSpace(_label))
                    return Action;
                return _label;
            }
            set
            {
                _label = value;
            }
        }
        /// <summary>
        /// The parameters that went along with the action.
        /// </summary>
        [JsonProperty("parameters")]
        public Dictionary<string, string> Parameters { get; set; }

        /// <summary>
        /// Initializes the class with default values.
        /// </summary>
        public Crumb()
        {
            ActionDate = DateTimeOffset.UtcNow;
            Action = "Action";
            _label = "";
            Controller = "Controler";
            Parameters = new Dictionary<string, string>();
            Id = Guid.Empty;
        }

        /// <summary>
        /// Creates a new crumb with the specified values.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="controller">The controller.</param>
        /// <param name="parameters">The parameters.</param>
        /// <param name="label">The label.</param>
        /// <returns>The <code>Crumb</code> that was created.</returns>
        public static Crumb New(string action, string controller, Dictionary<string, string> parameters = null, string label = "")
        {
            var crumb = new Crumb()
            {
                Action = action,
                Controller = controller,
                Parameters = parameters ?? new Dictionary<string, string>(),
                Label = label,
                Id = Guid.NewGuid()
            };
            return crumb;
        }

        /// <summary>
        /// Creates a route value dictionary to be used.
        /// </summary>
        /// <returns>The <code>RouteValueDictionary</code> made.</returns>
        public System.Web.Routing.RouteValueDictionary RVD()
        {
            var rvd = new System.Web.Routing.RouteValueDictionary();
            foreach (var kvp in Parameters)
            {
                rvd.Add(kvp.Key, kvp.Value);
            }
            rvd.Add("crumbid", Id);
            return rvd;
        }
    }
}
