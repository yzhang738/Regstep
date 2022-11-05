using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RSToolKit.Domain.Entities;
using RSToolKit.Domain.Entities.Clients;
using System.Security.Principal;
using RSToolKit.Domain.Security;
using RSToolKit.Domain.Exceptions;
using System.Linq.Expressions;
using System.Data.Entity;

namespace RSToolKit.Domain.Data
{
    public class CapacityRepository
        : IRepository<Seating, Guid>
    {
        #region Variables and Properties

        protected EFDbContext _context;
        protected bool _contextInScope;
        protected ILogger _log;

        /// <summary>
        /// The context being used for this repository.
        /// </summary>
        public EFDbContext Context
        {
            get
            {
                return this._context;
            }
        }

        /// <summary>
        /// If this context is in scope for this repository.
        /// </summary>
        public bool ContextInScope
        {
            get
            {
                return this._contextInScope;
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a repository with an in scope context and no user information.
        /// </summary>
        public CapacityRepository()
        {
            this._context = new EFDbContext();
            this._contextInScope = true;
        }

        /// <summary>
        /// Creates a repository with an in scope context and user information.
        /// </summary>
        /// <param name="user">The user that is accessing the database.</param>
        /// <param name="principal">The logged in principal for this repository.</param>
        public CapacityRepository(User user, IPrincipal principal)
            : this()
        {
            if (user != null && principal != null)
                _context.SecuritySettings.SetUser(user, principal);
        }

        /// <summary>
        /// Creates a repository with an out of scope context.
        /// </summary>
        /// <param name="context">The context to use for this database.</param>
        public CapacityRepository(EFDbContext context)
        {
            this._context = context;
            this._contextInScope = false;
        }

        /// <summary>
        /// Creates a repository with an out of scope context and user information.
        /// </summary>
        /// <param name="context">The context to use for this database.</param>
        /// <param name="user">The user that is accessing the database.</param>
        /// <param name="principal">The logged in principal for this repository.</param>
        public CapacityRepository(EFDbContext context, User user, IPrincipal principal)
            : this(context)
        {
            if (user != null && principal != null)
                _context.SecuritySettings.SetUser(user, principal);
        }

        #endregion

        #region Supporting Methods

        /// <summary>
        /// Removes non permissives Seating from a set of transaction requests.
        /// </summary>
        /// <param name="nodes">The set of transaction requests to check.</param>
        /// <param name="accessType">The type of access happening.</param>
        /// <returns>A set of transaction requests that can be used for the desired access type.</returns>
        public IEnumerable<Seating> RemoveNonPermissive(IEnumerable<Seating> nodes, SecurityAccessType accessType)
        {
            var forms = new List<Seating>();
            foreach (var node in nodes)
            {
                if (this._context.CanAccess(node, accessType, nodes.Count() > 1))
                    forms.Add(node);
            }
            return forms;
        }

        /// <summary>
        /// Removes non permissives Seating a transaction request.
        /// </summary>
        /// <param name="node">The Seating to check.</param>
        /// <param name="accessType">The type of access happening.</param>
        /// <returns>The transaction request or null.</returns>
        public Seating RemoveNonPermissive(Seating node, SecurityAccessType accessType)
        {
            var form = RemoveNonPermissive(new List<Seating>() { node }, accessType).FirstOrDefault();
            if (form == null)
                throw new InsufficientPermissionsException(minor: 61);
            return form;
        }

        /// <summary>
        /// Removes non permissives from a set of forms.
        /// </summary>
        /// <param name="nodes">The set of Seating to check.</param>
        /// <param name="accessType">The type of access happening.</param>
        /// <returns>A set of transaction requests that can be used for the desired access type.</returns>
        public async Task<IEnumerable<Seating>> RemoveNonPermissiveAsync(IEnumerable<Seating> nodes, SecurityAccessType accessType)
        {
            return await Task.Run(() =>
            {
                return RemoveNonPermissive(nodes, accessType);
            });
        }

        /// <summary>
        /// Removes non permissive from a form if applicable.
        /// </summary>
        /// <param name="node">The Seating to check.</param>
        /// <param name="accessType">The type of access happening.</param>
        /// <returns>The form or null.</returns>
        public async Task<Seating> RemoveNonPermissiveAsync(Seating node, SecurityAccessType accessType)
        {
            return await RemoveNonPermissiveAsync(node, accessType);
        }

        #endregion

        #region Database Methods

        #region Seater

        /// <summary>
        /// Finds the seater in the database.
        /// </summary>
        /// <param name="key">The key to find with.</param>
        /// <param name="usePermissions">User permissions or not.</param>
        /// <returns>The seater found.</returns>
        public Seater FindSeater(Guid key, bool usePermissions = true)
        {
            var node = Context.Seaters.Find(key);
            if (node == null)
                throw new FormNotFoundException();
            if (this._context.CanAccess(node, SecurityAccessType.Read))
                return node;
            throw new InsufficientPermissionsException();
        }

        /// <summary>
        /// Finds the seater in the database.
        /// </summary>
        /// <param name="key">The key to find with.</param>
        /// <param name="usePermissions">User permissions or not.</param>
        /// <returns>The seater found.</returns>
        public Seater FindSeater(long key, bool usePermissions = true)
        {
            var node = Context.Seaters.FirstOrDefault(s => s.SortingId == key);
            if (node == null)
                throw new FormNotFoundException();
            if (this._context.CanAccess(node, SecurityAccessType.Read))
                return node;
            throw new InsufficientPermissionsException();
        }

        #endregion

        /// <summary>
        /// Finds a Seating request by the primary key.
        /// </summary>
        /// <param name="key">The primary key for the Seating record.</param>
        /// <param name="usePermissions">Set to false to skip permission checks.</param>
        /// <returns>A registrant.</returns>
        /// <exception cref="TransactionNotFoundException">Throws if the transaction request was not found.</exception>
        /// <exception cref="InsufficientPermissionsException">Throws if not enough permissions to read transaction request.</exception>
        public Seating Find(Guid key, bool usePermissions = true)
        {
            var node = Context.Seatings.Find(key);
            if (node == null)
                throw new TransactionNotFoundException();
            if (usePermissions)
                return RemoveNonPermissive(node, SecurityAccessType.Read);
            return node;
        }

        /// <summary>
        /// Finds a Seating by the transaction request's key.
        /// </summary>
        /// <param name="key">The primary key for the Seating record.</param>
        /// <param name="usePermissions">Set to false to skip permission checks.</param>
        /// <returns>A registrant.</returns>
        /// <exception cref="TransactionNotFoundException">Throws if the transaction request was not found.</exception>
        /// <exception cref="InsufficientPermissionsException">Throws if not enough permissions to read transaction request.</exception>
        public Seating First(Expression<Func<Seating, bool>> search, bool usePermissions = true)
        {
            var node = Context.Seatings.FirstOrDefault(search);
            if (node == null)
                throw new TransactionNotFoundException();
            if (usePermissions)
                return RemoveNonPermissive(node, SecurityAccessType.Read);
            return node;
        }

        /// <summary>
        /// Finds a Seating by the primary key.
        /// </summary>
        /// <param name="key">The primary key for the Seating record.</param>
        /// <param name="usePermissions">Set to false to skip permission checks.</param>
        /// <returns>A registrant.</returns>
        /// <exception cref="TransactionNotFoundException">Throws if the transaction request was not found.</exception>
        /// <exception cref="InsufficientPermissionsException">Throws if not enough permissions to read transaction request.</exception>
        public async Task<Seating> FindAsync(Guid key, bool usePermissions = true)
        {
            var node = await Context.Seatings.FindAsync(key);
            if (node == null)
                throw new TransactionNotFoundException();
            if (usePermissions)
                return RemoveNonPermissive(node, SecurityAccessType.Read);
            return node;
        }

        /// <summary>
        /// Finds Seating by the supplied search criteria.
        /// </summary>
        /// <param name="search">The search to perform on the database.</param>
        /// <param name="usePermissions">Set to false to skip permission checks.</param>
        /// <returns>An enumeration of transaction requests.</returns>
        public IEnumerable<Seating> Search(Expression<Func<Seating, bool>> search, bool usePermissions = true)
        {
            var nodes = Context.Seatings.Where(search).ToList().AsEnumerable();
            if (usePermissions)
                nodes = RemoveNonPermissive(nodes, SecurityAccessType.Read);
            return nodes;
        }

        /// <summary>
        /// Finds Seating by the supplied search criteria.
        /// </summary>
        /// <param name="search">The search to perform on the database.</param>
        /// <param name="usePermissions">Set to false to skip permission checks.</param>
        /// <returns>An enumeration of transaction requests.</returns>
        public async Task<IEnumerable<Seating>> SearchAsync(Expression<Func<Seating, bool>> search, bool usePermissions = true)
        {
            return await Task.Run(() =>
            {
                return Search(search, usePermissions);
            });
        }

        /// <summary>
        /// Removes a Seating from the context.
        /// </summary>
        /// <param name="node">The Seating to remove.</param>
        /// <param name="usePermissions">Set to false to skip permission checks.</param>
        /// <returns>True if it did.</returns>
        /// <exception cref="TransactionNotFoundException">Throws if the transaction request was not found.</exception>
        /// <exception cref="InsufficientPermissionsException">Throws if not enough permissions to delete the transaction request.</exception>
        public bool Remove(Seating node, bool usePermissions = true)
        {
            if (node == null)
                throw new TransactionNotFoundException();
            if (usePermissions)
                node = RemoveNonPermissive(node, SecurityAccessType.Write);
            if (node == null)
                throw new InsufficientPermissionsException(minor: 64);
            _RemoveSeating(node);
            return true;
        }

        /// <summary>
        /// Removes a Seating from the context.
        /// </summary>
        /// <param name="node">The Seating to remove.</param>
        /// <param name="usePermissions">Set to false to skip permission checks.</param>
        /// <returns>True if it did.</returns>
        /// <exception cref="TransactionNotFoundException">Throws if the transaction request was not found.</exception>
        /// <exception cref="InsufficientPermissionsException">Throws if not enough permissions to delete transaction request.</exception>
        public async Task<bool> RemoveAsync(Seating node, bool usePermissions = true)
        {
            return await Task.Run(() =>
            {
                return Remove(node);
            });
        }

        /// <summary>
        /// Removes a set of Seatings from the context.
        /// </summary>
        /// <param name="nodes">The set of Seatings to remove.</param>
        /// <param name="usePermissions">Set to false to skip permission checks.</param>
        /// <returns>True if it did.</returns>
        public bool RemoveRange(IEnumerable<Seating> nodes, bool usePermissions = true)
        {
            if (usePermissions)
                nodes = RemoveNonPermissive(nodes, SecurityAccessType.Write);
            foreach (var node in nodes)
                _RemoveSeating(node);
            return true;
        }

        /// <summary>
        /// Removes a set of Seatings from the context.
        /// </summary>
        /// <param name="nodes">The set of Seatings to remove.</param>
        /// <param name="usePermissions">Set to false to skip permission checks.</param>
        /// <returns>True if it did.</returns>
        public async Task<bool> RemoveRangeAsync(IEnumerable<Seating> nodes, bool usePermissions = true)
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
            return this._context.SaveChanges();
        }

        /// <summary>
        /// Saves the context to the database.
        /// </summary>
        /// <returns>the number of records affected.</returns>
        public async Task<int> CommitAsync()
        {
            return await this._context.SaveChangesAsync();
        }

        /// <summary>
        /// Removes a Seating from the context.
        /// </summary>
        /// <param name="node">The transaction request to remove.</param>
        protected void _RemoveSeating(Seating node)
        {
            if (node == null)
                return;
            // Let's see if the node is already attached to a database.
            if (Context.Entry(node).State == EntityState.Detached)
                // The entity is not attached to the database. We return.
                return;
            // We need to move remove the attached transaction details.
            this._context.RemoveSeating(node);
        }

        /// <summary>
        /// Removes a set of Seatings from the context.
        /// </summary>
        /// <param name="nodes">The set of transaction requests to remove.</param>
        protected void _RemoveFormRange(IEnumerable<Seating> nodes)
        {
            foreach (var node in nodes)
                _RemoveSeating(node);
        }

        /// <summary>
        /// Adds the specified Seating to the context.
        /// </summary>
        /// <param name="node">The transaction request to add.</param>
        /// <param name="userPermissions">Set to false to skip permission checks.</param>
        public void Add(Seating node, bool userPermissions = true)
        {
            node = RemoveNonPermissive(node, SecurityAccessType.Write);
            if (node == null)
                return;
            this._context.Seatings.Add(node);
        }

        #endregion

        #region Interfacing

        #region IDisposable

        void IDisposable.Dispose()
        {
            if (this._contextInScope)
                this._context.Dispose();
        }

        #endregion

        #endregion
    }
}
