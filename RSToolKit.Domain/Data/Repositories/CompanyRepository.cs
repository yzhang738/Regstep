using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Principal;
using RSToolKit.Domain.Entities.Clients;
using RSToolKit.Domain.Security;
using RSToolKit.Domain.Exceptions;
using System.Data.Entity;
using System.Linq.Expressions;

namespace RSToolKit.Domain.Data
{
    /// <summary>
    /// Handles data manipulation in the database.
    /// </summary>
    public class CompanyRepository
        : IRepository<Company, Guid>
    {
        #region Members and Properties

        #region Members

        protected ILogger _log;

        #endregion

        #region Properties

        /// <summary>
        /// The context being used for this repository.
        /// </summary>
        public EFDbContext Context { get; protected set; }

        /// <summary>
        /// If this context is in scope for this repository.
        /// </summary>
        public bool ContextInScope { get; protected set; }

        #endregion

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes class construction.
        /// </summary>
        protected void _Initialize()
        {
            Context = null;
        }

        /// <summary>
        /// Finalizes class construction.
        /// <param name="user">[optional] The user that is accessing the database.</param>
        /// <param name="principal">[optional] The logged in principal for this repository.</param>
        /// </summary>
        protected void _Finalize(User user = null, IPrincipal principal = null)
        {
            if (Context == null)
            {
                Context = new EFDbContext();
                ContextInScope = true;
            }
            else
            {
                ContextInScope = false;
            }
            if (user != null && principal != null)
                Context.SecuritySettings.SetUser(user, principal);
        }

        /// <summary>
        /// Creates a repository with an in scope context and no user information.
        /// </summary>
        public CompanyRepository()
        {
            this._Initialize();
        }

        /// <summary>
        /// Creates a repository with an in scope context and user information.
        /// </summary>
        /// <param name="user">The user that is accessing the database.</param>
        /// <param name="principal">The logged in principal for this repository.</param>
        public CompanyRepository(User user, IPrincipal principal)
            : this()
        {
            this._Finalize(user, principal);
        }

        /// <summary>
        /// Creates a repository with an out of scope context.
        /// </summary>
        /// <param name="context">The context to use for this database.</param>
        public CompanyRepository(EFDbContext context)
            : this()
        {
            Context = context;
            this._Finalize();
        }

        /// <summary>
        /// Creates a repository with an out of scope context and user information.
        /// </summary>
        /// <param name="context">The context to use for this database.</param>
        /// <param name="user">The user that is accessing the database.</param>
        /// <param name="principal">The logged in principal for this repository.</param>
        public CompanyRepository(EFDbContext context, User user, IPrincipal principal)
            : this()
        {
            Context = context;
            this._Finalize(user, principal);
        }

        #endregion

        #region Supporting Methods

        /// <summary>
        /// Removes non permissives from a set of companies.
        /// </summary>
        /// <param name="nodes">The set of nodes to check.</param>
        /// <param name="accessType">The type of access happening.</param>
        /// <returns>A set of items that can be used for the desired access type.</returns>
        public IEnumerable<Company> RemoveNonPermissive(IEnumerable<Company> nodes, SecurityAccessType accessType)
        {
            var items = new List<Company>();
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
        public Company RemoveNonPermissive(Company node, SecurityAccessType accessType)
        {
            var form = RemoveNonPermissive(new List<Company>() { node }, accessType).FirstOrDefault();
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
        public async Task<IEnumerable<Company>> RemoveNonPermissiveAsync(IEnumerable<Company> nodes, SecurityAccessType accessType)
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
        public async Task<Company> RemoveNonPermissiveAsync(Company node, SecurityAccessType accessType)
        {
            return await RemoveNonPermissiveAsync(node, accessType);
        }

        #endregion

        #region Database Methods

        /// <summary>
        /// Gets all the items the user has acces to.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Company> GetAll()
        {
            var nodes = Context.Companies.ToList();
            return RemoveNonPermissive(nodes, SecurityAccessType.Read);
        }

        /// <summary>
        /// Finds an item by the primary key.
        /// </summary>
        /// <param name="key">The primary key for the item.</param>
        /// <param name="usePermissions">Set to false to skip permission checks.</param>
        /// <returns>An item.</returns>
        /// <exception cref="InsufficientPermissionsException">Thrown if not enough permissions to read the item.</exception>
        /// <exception cref="CompanyNotFoundException">Thrown if the company was not found in the database.</exception>
        public Company Find(Guid key, bool usePermissions = true)
        {
            var node = Context.Companies.Find(key);
            if (node == null)
                throw new CompanyNotFoundException();
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
        /// <exception cref="InsufficientPermissionsException">Thrown if not enough permissions to read the item.</exception>
        /// <exception cref="CompanyNotFoundException">Thrown if the company was not found in the database.</exception>
        public Company First(Expression<Func<Company, bool>> search, bool usePermissions = true)
        {
            var node = Context.Companies.FirstOrDefault(search);
            if (node == null)
                throw new CompanyNotFoundException();
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
        /// <exception cref="InsufficientPermissionsException">Thrown if not enough permissions to read the item.</exception>
        /// <exception cref="CompanyNotFoundException">Thrown if the company was not found in the database.</exception>
        public async Task<Company> FindAsync(Guid key, bool usePermissions = true)
        {
            var node = await Context.Companies.FindAsync(key);
            if (node == null)
                throw new CompanyNotFoundException();
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
        public IEnumerable<Company> Search(Expression<Func<Company, bool>> search, bool usePermissions = true)
        {
            var nodes = Context.Companies.Where(search).ToList().AsEnumerable();
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
        public async Task<IEnumerable<Company>> SearchAsync(Expression<Func<Company, bool>> search, bool usePermissions = true)
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
        /// <exception cref="InsufficientPermissionsException">Thrown if not enough permissions to read the item.</exception>
        /// <exception cref="CompanyNotFoundException">Thrown if the company was not found in the database.</exception>
        public bool Remove(Company node, bool usePermissions = true)
        {
            if (node == null)
                throw new CompanyNotFoundException();
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
        /// <exception cref="InsufficientPermissionsException">Thrown if not enough permissions to read the item.</exception>
        /// <exception cref="CompanyNotFoundException">Thrown if the company was not found in the database.</exception>
        public async Task<bool> RemoveAsync(Company node, bool usePermissions = true)
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
        public bool RemoveRange(IEnumerable<Company> nodes, bool usePermissions = true)
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
        public async Task<bool> RemoveRangeAsync(IEnumerable<Company> nodes, bool usePermissions = true)
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
        protected void _RemoveItem(Company node)
        {
            if (node == null)
                return;
            // Let's see if the node is already attached to a database.
            if (Context.Entry(node).State == EntityState.Detached)
                // The entity is not attached to the database. We return.
                return;
            // We need to move remove the attached transaction details.
            Context.Companies.Remove(node);
        }

        /// <summary>
        /// Adds the specified transaction request to the context.
        /// </summary>
        /// <param name="node">The transaction request to add.</param>
        /// <param name="userPermissions">Set to false to skip permission checks.</param>
        public void Add(Company node, bool userPermissions = true)
        {
            node = RemoveNonPermissive(node, SecurityAccessType.Write);
            if (node == null)
                return;
            Context.Companies.Add(node);
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
