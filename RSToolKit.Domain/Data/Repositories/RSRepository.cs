using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RSToolKit.Domain.Security;
using System.Linq.Expressions;
using System.Data.Entity;
using RSToolKit.Domain.Data.Repositories;
using RSToolKit.Domain.Data;
using RSToolKit.Domain.Entities.Manipulations;

namespace RSToolKit.Domain.Data.Repositories
{
    /// <summary>
    /// An abstract class that contains methods to perform CRUD actions on the database.
    /// This class will take the context and turn off lazy loading. If you don't want lazy loading disabled. Then don't use this context.
    /// </summary>
    /// <typeparam name="Titem">The type of database item to manipulate.</typeparam>
    public abstract class RSRepository<Titem>
        : IDisposable
        where Titem : class, IDataItem
    {
        /// <summary>
        /// The context being used.
        /// </summary>
        protected EFDbContext _context;
        /// <summary>
        /// The security permissions for the repository
        /// </summary>
        protected SecureUser _secureUser;
        /// <summary>
        /// Flag for if the context is in scope or not.
        /// </summary>
        protected bool _inScope;
        /// <summary>
        /// 
        /// </summary>
        protected bool _protected;
        /// <summary>
        /// The set of items in the database.
        /// </summary>
        protected DbSet<Titem> _set;
        /// <summary>
        /// The type of the item.
        /// </summary>
        protected Type _type;

        /// <summary>
        /// Initializes the repository with a new context and turns lazy loading off.
        /// </summary>
        /// <param name="secureUser">[optional] The user for permissions.</param>
        public RSRepository(SecureUser secureUser = null)
        {
            this._context = new EFDbContext();
            this._inScope = true;
            this._Initialize();
            this._secureUser = secureUser;
        }

        /// <summary>
        /// Initializes the repository with the supplied context and turns lazy loading off.
        /// </summary>
        /// <param name="context">The context to use.</param>
        /// <param name="secureUser">[optional] The user for permissions.</param>
        public RSRepository(EFDbContext context, SecureUser secureUser = null)
        {
            this._context = context;
            this._inScope = false;
            this._Initialize();
            this._secureUser = secureUser;
        }

        /// <summary>
        /// Initializes the class.
        /// </summary>
        protected void _Initialize()
        {
            if (this._inScope)
                this._context.Configuration.LazyLoadingEnabled = false;
            if (typeof(Titem).IsAssignableFrom(typeof(IProtected)))
                this._protected = true;
            this._set = this._context.Set<Titem>();
            this._type = typeof(Titem);
        }

        /// <summary>
        /// Sets the user to use for permissions.
        /// </summary>
        /// <param name="secureUser">The user.</param>
        public void SetUser(SecureUser secureUser)
        {
            this._secureUser = secureUser;
        }

        /// <summary>
        /// Finds the record in the database.
        /// </summary>
        /// <param name="id">The id of the record.</param>
        /// <param name="ignoreException">Flag to ignore expceptions are thrown.</param>
        /// <param name="ignorePermissions">Flag to ignore permissions.</param>
        /// <returns>The record retrieved.</returns>
        /// <exception cref="RecordNotFoundException">Thrown if the record was not found and the <paramref name="ignoreException"/> flag is not set.</exception>
        public virtual Titem Find(long id, bool ignoreException = false, bool ignorePermissions = false)
        {
            var item = this._set.Find(id);
            if (item == null)
            {
                if (!ignoreException)
                    throw new RecordNotFoundException();
                else
                    return item;
            }
            item = this._RunPermissions(item, SecurityAccessType.Read, ignorePermissions, ignoreException);
            return item;
        }

        /// <summary>
        /// Finds the record in the database.
        /// </summary>
        /// <param name="id">The id of the record.</param>
        /// <param name="ignoreException">Flag to ignore expceptions are thrown.</param>
        /// <returns>The record retrieved.</returns>
        /// <exception cref="RecordNotFoundException">Thrown if the record was not found and the <paramref name="ignoreException"/> flag is not set.</exception>
        public virtual async Task<Titem> FindAsync(long id, bool ignoreException = false)
        {
            return await Task.Run(() => { return Find(id, ignoreException); });
        }

        /// <summary>
        /// Creates a database query.
        /// </summary>
        /// <param name="search">The search criteria.</param>
        /// <param name="ignorePermissions">If we should ignore permissions.</param>
        /// <returns>The query generated.</returns>
        public virtual IQueryable<Titem> Where(Expression<Func<Titem, bool>> search, bool ignorePermissions = false)
        {
            if (!ignorePermissions)
            {
                var locations = new ItemLocationList<Titem>(this._context);
                this._set.Where(search).Select(i => new { Id = i.Id, UId = i.UId, CompanyKey = i.CompanyKey }).ToList().ForEach(l => locations.Create(l.Id, l.UId, l.CompanyKey));
                var ids = this._RunPermissions(locations, SecurityAccessType.Read, ignorePermissions, true);
                return this._set.Where(i => ids.Contains(i.Id));
            }
            return this._set.Where(search);
        }

        /// <summary>
        /// Creates a database query.
        /// </summary>
        /// <param name="search">The search criteria.</param>
        /// <param name="ignorePermissions">If we should ignore permissions.</param>
        /// <returns>The query generated.</returns>
        public virtual IQueryable<Titem> FirstOrDefault(Expression<Func<Titem, bool>> search, bool ignorePermissions = false)
        {
            if (!ignorePermissions)
            {
                var locations = new ItemLocationList<Titem>(this._context);
                this._set.Where(search).Select(i => new { Id = i.Id, UId = i.UId, CompanyKey = i.CompanyKey }).ToList().ForEach(l => locations.Create(l.Id, l.UId, l.CompanyKey));
                var ids = this._RunPermissions(locations, SecurityAccessType.Read, ignorePermissions, true);
                return this._set.Where(i => ids.Contains(i.Id));
            }
            return this._set.Where(search);
        }

        /// <summary>
        /// Marks the itemto be removed from the database.
        /// </summary>
        /// <param name="item">The item to remove.</param>
        public virtual void Delete(Titem item)
        {
            this._RunPermissions(item, SecurityAccessType.Write);
            this._context.Set<Titem>().Remove(item);
        }

        /// <summary>
        /// Checks to see if the user has access for this item.
        /// </summary>
        /// <param name="item">The item in question.</param>
        /// <param name="action">The actin to run.</param>
        /// <param name="ignorePermissions">Flag for ignoring permissions.</param>
        /// <param name="ignoreException">Flag for ignoring exceptions.</param>
        /// <returns>The item or null.</returns>
        /// <exception cref="RecordNotFoundException">Thrown if the record was not found and the <paramref name="ignoreException"/> flag is not set.</exception>
        protected Titem _RunPermissions(Titem item, SecurityAccessType action, bool ignorePermissions = false, bool ignoreException = false)
        {
            if (!this._protected || ignorePermissions)
                return item;
            if (this._secureUser == null)
                return null;
            var protectedItem = item as IProtected;
            //if (protectedItem.GetPermission(this._secureUser, action, ignoreException).HasAccess(action))
                //return item;
            return null;
        }

        /// <summary>
        /// Checks to see if the user has access for this item.
        /// </summary>
        /// <param name="locations">The item locations in the database.</param>
        /// <param name="action">The actin to run.</param>
        /// <param name="ignorePermissions">Flag for ignoring permissions.</param>
        /// <param name="ignoreException">Flag for ignoring exceptions.</param>
        /// <returns>The item or null.</returns>
        /// <exception cref="RecordNotFoundException">Thrown if the record was not found and the <paramref name="ignoreException"/> flag is not set.</exception>
        protected IEnumerable<long> _RunPermissions(ItemLocationList<Titem> locations, SecurityAccessType action, bool ignorePermissions = false, bool ignoreException = false)
        {
            if (!this._protected || ignorePermissions)
                return locations.Select(l => l.Id);
            if (this._secureUser == null)
                return null;
            var uids = locations.Select(l => l.UId).ToList();
            var ids = new List<long>();
            foreach (var location in locations)
            {
                if (location.GetPermission(this._secureUser, action, ignoreException).HasAccess(action))
                {
                    ids.Add(location.Id);
                }
            }
            return ids;
        }

        /// <summary>
        /// Checks to see if the user has access for this item.
        /// </summary>
        /// <param name="items">The items in question.</param>
        /// <param name="action">The actin to run.</param>
        /// <param name="ignorePermissions">Flag for ignoring permissions.</param>
        /// <param name="ignoreException">Flag for ignoring exceptions.</param>
        /// <returns>The item or null.</returns>
        /// <exception cref="RecordNotFoundException">Thrown if the record was not found and the <paramref name="ignoreException"/> flag is not set.</exception>
        protected List<Titem> _RunPermissions(IEnumerable<Titem> items, SecurityAccessType action, bool ignorePermissions = false, bool ignoreException = false)
        {
            if (!this._protected || ignorePermissions)
                return items.ToList();
            if (this._secureUser == null)
                return null;
            var list = new List<Titem>();
            foreach (var item in items)
            {
                var protectedItem = item as IProtected;
                //if (protectedItem.GetPermission(this._secureUser, action, ignoreException).HasAccess(action))
                    //list.Add(item);
            }
            return list;
        }

        /// <summary>
        /// Disposes of the repository.
        /// </summary>
        public void Dispose()
        {
            if (this._inScope)
                this._context.Dispose();
        }

    }
}
