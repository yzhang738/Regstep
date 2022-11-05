using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RSToolKit.Domain.Entities.MerchantAccount;
using System.Security.Principal;
using RSToolKit.Domain.Entities.Clients;
using RSToolKit.Domain.Security;
using RSToolKit.Domain.Exceptions;
using System.Data.Entity;
using System.Linq.Expressions;
using System.Data.Entity.Infrastructure;

namespace RSToolKit.Domain.Data
{
    public class MerchantAccountRepository
        : IRepository<MerchantAccountInfo, Guid>
    {
        #region Variables and Properties

        protected ILogger _log;
        protected HashSet<Guid> _updatedForms;

        /// <summary>
        /// The context being used for this repository.
        /// </summary>
        public EFDbContext Context { get; protected set; }

        /// <summary>
        /// If this context is in scope for this repository.
        /// </summary>
        public bool ContextInScope { get; protected set; }

        #endregion

        #region Constructors

        protected void _Initialize()
        {
        }

        /// <summary>
        /// Creates a repository with an in scope context and no user information.
        /// </summary>
        public MerchantAccountRepository()
        {
            Context = new EFDbContext();
            ContextInScope = true;
        }

        /// <summary>
        /// Creates a repository with an in scope context and user information.
        /// </summary>
        /// <param name="user">The user that is accessing the database.</param>
        /// <param name="principal">The logged in principal for this repository.</param>
        public MerchantAccountRepository(User user, IPrincipal principal)
            : this()
        {
            if (user != null && principal != null)
                Context.SecuritySettings.SetUser(user, principal);
        }

        /// <summary>
        /// Creates a repository with an out of scope context.
        /// </summary>
        /// <param name="context">The context to use for this database.</param>
        public MerchantAccountRepository(EFDbContext context)
        {
            Context = context;
            ContextInScope = false;
        }

        /// <summary>
        /// Creates a repository with an out of scope context and user information.
        /// </summary>
        /// <param name="context">The context to use for this database.</param>
        /// <param name="user">The user that is accessing the database.</param>
        /// <param name="principal">The logged in principal for this repository.</param>
        public MerchantAccountRepository(EFDbContext context, User user, IPrincipal principal)
            : this(context)
        {
            if (user != null && principal != null)
                Context.SecuritySettings.SetUser(user, principal);
        }

        #endregion

        #region Supporting Methods

        /// <summary>
        /// Removes non permissives from a set of transaction requests.
        /// </summary>
        /// <param name="nodes">The set of nodes to check.</param>
        /// <param name="accessType">The type of access happening.</param>
        /// <returns>A set of items that can be used for the desired access type.</returns>
        public IEnumerable<MerchantAccountInfo> RemoveNonPermissive(IEnumerable<MerchantAccountInfo> nodes, SecurityAccessType accessType)
        {
            var items = new List<MerchantAccountInfo>();
            foreach (var node in nodes)
            {
                if (Context.CanAccess(node, accessType, nodes.Count() > 1))
                    items.Add(node);
            }
            return items;
        }

        /// <summary>
        /// Removes non permissives from set of items.
        /// </summary>
        /// <param name="node">The item to check.</param>
        /// <param name="accessType">The type of access happening.</param>
        /// <returns>The item.</returns>
        /// <exception cref="InsufficientPermissionsException">Thrown if the user cannot access the item.</exception>
        public MerchantAccountInfo RemoveNonPermissive(MerchantAccountInfo node, SecurityAccessType accessType)
        {
            var form = RemoveNonPermissive(new List<MerchantAccountInfo>() { node }, accessType).FirstOrDefault();
            if (form == null)
                throw new InsufficientPermissionsException(minor: 61);
            return form;
        }

        /// <summary>
        /// Removes non permissives from a set of items.
        /// </summary>
        /// <param name="nodes">The set of items to check.</param>
        /// <param name="accessType">The type of access happening.</param>
        /// <returns>A set of items that can be used for the desired access type.</returns>
        public async Task<IEnumerable<MerchantAccountInfo>> RemoveNonPermissiveAsync(IEnumerable<MerchantAccountInfo> nodes, SecurityAccessType accessType)
        {
            return await Task.Run(() =>
            {
                return RemoveNonPermissive(nodes, accessType);
            });
        }

        /// <summary>
        /// Removes non permissive from an item.
        /// </summary>
        /// <param name="node">The item to check.</param>
        /// <param name="accessType">The type of access happening.</param>
        /// <returns>The item.</returns>
        /// <exception cref="InsufficientPermissionsException">Thrown if the user cannot access the item.</exception>
        public async Task<MerchantAccountInfo> RemoveNonPermissiveAsync(MerchantAccountInfo node, SecurityAccessType accessType)
        {
            return await RemoveNonPermissiveAsync(node, accessType);
        }

        #endregion

        #region Database Methods

        /// <summary>
        /// Finds an item by the primary key.
        /// </summary>
        /// <param name="key">The primary key for the item.</param>
        /// <param name="usePermissions">Set to false to skip permission checks.</param>
        /// <returns>An item.</returns>
        /// <exception cref="TransactionNotFoundException">Throws if the item was not found.</exception>
        /// <exception cref="InsufficientPermissionsException">Throws if not enough permissions to read the item.</exception>
        public MerchantAccountInfo Find(Guid key, bool usePermissions = true)
        {
            var node = Context.MerchantAccountInfo.Find(key);
            if (node == null)
                throw new TransactionNotFoundException();
            if (usePermissions)
                return RemoveNonPermissive(node, SecurityAccessType.Read);
            return node;
        }

        /// <summary>
        /// Finds an item by the expression supplied.
        /// </summary>
        /// <param name="search">The expression for the item.</param>
        /// <param name="usePermissions">Set to false to skip permission checks.</param>
        /// <returns>An item.</returns>
        /// <exception cref="TransactionNotFoundException">Throws if the item was not found.</exception>
        /// <exception cref="InsufficientPermissionsException">Throws if not enough permissions to read the item.</exception>
        public MerchantAccountInfo First(Expression<Func<MerchantAccountInfo, bool>> search, bool usePermissions = true)
        {
            var node = Context.MerchantAccountInfo.FirstOrDefault(search);
            if (node == null)
                throw new TransactionNotFoundException();
            if (usePermissions)
                return RemoveNonPermissive(node, SecurityAccessType.Read);
            return node;
        }

        /// <summary>
        /// Finds an item by the primary key.
        /// </summary>
        /// <param name="key">The primary key for the item.</param>
        /// <param name="usePermissions">Set to false to skip permission checks.</param>
        /// <returns>An item.</returns>
        /// <exception cref="TransactionNotFoundException">Throws if the item was not found.</exception>
        /// <exception cref="InsufficientPermissionsException">Throws if not enough permissions to read the item.</exception>
        public async Task<MerchantAccountInfo> FindAsync(Guid key, bool usePermissions = true)
        {
            var node = await Context.MerchantAccountInfo.FindAsync(key);
            if (node == null)
                throw new TransactionNotFoundException();
            if (usePermissions)
                return RemoveNonPermissive(node, SecurityAccessType.Read);
            return node;
        }

        /// <summary>
        /// Finds items by the supplied search criteria.
        /// </summary>
        /// <param name="search">The search to perform on the database.</param>
        /// <param name="usePermissions">Set to false to skip permission checks.</param>
        /// <returns>An enumeration of items.</returns>
        public IEnumerable<MerchantAccountInfo> Search(Expression<Func<MerchantAccountInfo, bool>> search, bool usePermissions = true)
        {
            var nodes = Context.MerchantAccountInfo.Where(search).ToList().AsEnumerable();
            if (usePermissions)
                nodes = RemoveNonPermissive(nodes, SecurityAccessType.Read);
            return nodes;
        }

        /// <summary>
        /// Finds items by the supplied search criteria.
        /// </summary>
        /// <param name="search">The search to perform on the database.</param>
        /// <param name="usePermissions">Set to false to skip permission checks.</param>
        /// <returns>An enumeration of items.</returns>
        public async Task<IEnumerable<MerchantAccountInfo>> SearchAsync(Expression<Func<MerchantAccountInfo, bool>> search, bool usePermissions = true)
        {
            return await Task.Run(() =>
            {
                return Search(search, usePermissions);
            });
        }

        /// <summary>
        /// Removes an item from the context.
        /// </summary>
        /// <param name="node">The item to remove.</param>
        /// <param name="usePermissions">Set to false to skip permission checks.</param>
        /// <returns>True if it did.</returns>
        /// <exception cref="TransactionNotFoundException">Throws if the item was not found.</exception>
        /// <exception cref="InsufficientPermissionsException">Throws if not enough permissions to delete the item.</exception>
        public bool Remove(MerchantAccountInfo node, bool usePermissions = true)
        {
            if (node == null)
                throw new TransactionNotFoundException();
            if (usePermissions)
                node = RemoveNonPermissive(node, SecurityAccessType.Write);
            if (node == null)
                throw new InsufficientPermissionsException(minor: 64);
            _RemoveItem(node);
            return true;
        }

        /// <summary>
        /// Removes an item from the context.
        /// </summary>
        /// <param name="node">The item to remove.</param>
        /// <param name="usePermissions">Set to false to skip permission checks.</param>
        /// <returns>True if it did.</returns>
        /// <exception cref="TransactionNotFoundException">Throws if the item was not found.</exception>
        /// <exception cref="InsufficientPermissionsException">Throws if not enough permissions to delete the item.</exception>
        public async Task<bool> RemoveAsync(MerchantAccountInfo node, bool usePermissions = true)
        {
            return await Task.Run(() =>
            {
                return Remove(node);
            });
        }

        /// <summary>
        /// Removes a set of transaction requests from the context.
        /// </summary>
        /// <param name="nodes">The set of items to remove.</param>
        /// <param name="usePermissions">Set to false to skip permission checks.</param>
        /// <returns>True if it did.</returns>
        public bool RemoveRange(IEnumerable<MerchantAccountInfo> nodes, bool usePermissions = true)
        {
            if (usePermissions)
                nodes = RemoveNonPermissive(nodes, SecurityAccessType.Write);
            foreach (var node in nodes)
                _RemoveItem(node);
            return true;
        }

        /// <summary>
        /// Removes a set of items from the context.
        /// </summary>
        /// <param name="nodes">The set of items to remove.</param>
        /// <param name="usePermissions">Set to false to skip permission checks.</param>
        /// <returns>True if it did.</returns>
        public async Task<bool> RemoveRangeAsync(IEnumerable<MerchantAccountInfo> nodes, bool usePermissions = true)
        {
            return await Task.Run(() =>
            {
                return RemoveRange(nodes);
            });
        }

        /// <summary>
        /// Saves the context to the database.
        /// </summary>
        /// <returns>the number of records affected.</returns>
        public int Commit()
        {
            return Context.SaveChanges();
        }

        /// <summary>
        /// Saves the context to the database.
        /// </summary>
        /// <returns>the number of records affected.</returns>
        public async Task<int> CommitAsync()
        {
            return await Context.SaveChangesAsync();
        }

        /// <summary>
        /// Removes an item from the context.
        /// </summary>
        /// <param name="node">The item to remove.</param>
        protected void _RemoveItem(MerchantAccountInfo node)
        {
            if (node == null)
                return;
            // Let's see if the node is already attached to a database.
            if (Context.Entry(node).State == EntityState.Detached)
                // The entity is not attached to the database. We return.
                return;
            // We need to move remove the attached transaction details.
            Context.RemoveMerchantAccount(node);
        }

        /// <summary>
        /// Adds the specified transaction request to the context.
        /// </summary>
        /// <param name="node">The transaction request to add.</param>
        /// <param name="userPermissions">Set to false to skip permission checks.</param>
        public void Add(MerchantAccountInfo node, bool userPermissions = true)
        {
            node = RemoveNonPermissive(node, SecurityAccessType.Write);
            if (node == null)
                return;
            Context.MerchantAccountInfo.Add(node);
        }

        #endregion

        #region Interfacing

        #region IDisposable

        void IDisposable.Dispose()
        {
            if (ContextInScope)
                Context.Dispose();
        }

        #endregion

        #endregion
    }
}
