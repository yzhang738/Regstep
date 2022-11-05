using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using System.Collections;

namespace RSToolKit.Domain.Data.Repositories
{
    /// <summary>
    /// Holds information about a database item location.
    /// </summary>
    public abstract class ItemLocation<Titem>
        where Titem : class, IDataItem
    {
        /// <summary>
        /// The id of the item.
        /// </summary>
        public long Id { get; protected set; }
        /// <summary>
        /// The global unique identifier.
        /// </summary>
        public Guid UId { get; protected set; }
        /// <summary>
        /// The company being used.
        /// </summary>
        public Guid? CompanyKey { get; protected set; }
        /// <summary>
        /// The parent list.
        /// </summary>
        protected ItemLocationList<Titem> _parent;
        /// <summary>
        /// The database set.
        /// </summary>
        public DbSet<Titem> DbSet
        {
            get
            {
                return _parent.DbSet;
            }
        }
    }

    /// <summary>
    /// Lists of item locations.
    /// </summary>
    /// <typeparam name="Titem">The type of items the locations refer to.</typeparam>
    public class ItemLocationList<Titem>
        : IList<ItemLocation<Titem>>
        where Titem : class, IDataItem
    {
        /// <summary>
        /// The item locations.
        /// </summary>
        protected List<ItemLocation<Titem>> _items;
        public DbSet<Titem> DbSet { get; protected set; }

        /// <summary>
        /// Constructs the class.
        /// </summary>
        public ItemLocationList(EFDbContext context)
        {
            if (context == null)
                throw new ArgumentNullException("context", "The context cannot be null.");
            this._items = new List<ItemLocation<Titem>>();
            DbSet = context.Set<Titem>();
        }

        /// <summary>
        /// Gets or sets the item at the supplied index.
        /// </summary>
        /// <param name="index">The zero based index.</param>
        /// <returns>The item.</returns>
        public ItemLocation<Titem> this[int index]
        {
            get
            {
                return this._items[index];
            }
            set
            {
                return;
            }
        }

        /// <summary>
        /// The count of items.
        /// </summary>
        public int Count
        {
            get
            {
                return this._items.Count;
            }
        }

        /// <summary>
        /// Cheks to see if the list contains a specified item.
        /// </summary>
        /// <param name="item">The item to check.</param>
        /// <returns>True if it does, false otherwise.</returns>
        public bool Contains(ItemLocation<Titem> item)
        {
            return this._items.Contains(item);
        }

        public bool IsReadOnly
        {
            get
            {
                return true;
            }
        }

        void ICollection<ItemLocation<Titem>>.Add(ItemLocation<Titem> item)
        {
        }

        void ICollection<ItemLocation<Titem>>.Clear()
        {
        }

        void ICollection<ItemLocation<Titem>>.CopyTo(ItemLocation<Titem>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator<ItemLocation<Titem>> IEnumerable<ItemLocation<Titem>>.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public int IndexOf(ItemLocation<Titem> item)
        {
            return this._items.IndexOf(item);
        }

        void IList<ItemLocation<Titem>>.Insert(int index, ItemLocation<Titem> item)
        {
            return;
        }

        bool ICollection<ItemLocation<Titem>>.Remove(ItemLocation<Titem> item)
        {
            return false;
        }

        void IList<ItemLocation<Titem>>.RemoveAt(int index)
        {
            return;
        }

        public void Create(long id, Guid uid, Guid companyKey)
        {
            var location = new _ItemLocation(this, id, uid, companyKey);
            this._items.Add(location);
        }

        #region NestedClass
        private sealed class _ItemLocation
            : ItemLocation<Titem>
        {
            public _ItemLocation(ItemLocationList<Titem> parent, long id, Guid uid, Guid companyKey)
            {
                this._parent = parent;
                Id = id;
                UId = uid;
                CompanyKey = companyKey;
            }
        }
        #endregion
    }
}
