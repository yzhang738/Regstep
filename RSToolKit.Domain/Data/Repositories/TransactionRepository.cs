using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;
using RSToolKit.Domain.Entities.MerchantAccount;
using System.Security.Principal;
using System.Data.Entity;
using RSToolKit.Domain.Security;
using System.Data.Objects;
using System.Data.Entity.Infrastructure;
using RSToolKit.Domain.Exceptions;
using RSToolKit.Domain.Entities.Clients;
using RSToolKit.Domain.Identity;

namespace RSToolKit.Domain.Data
{
    /// <summary>
    /// Handles the manipulation and retrieval of transactions from the database.
    /// </summary>
    public class TransactionRepository
        : IRepository<TransactionRequest, Guid>, IDisposable
    {
        #region Variables and Properties

        protected EFDbContext _context;
        protected bool _contextInScope, _systemAdmin, _companyAdmin;
        protected ILogger _log;

        /// <summary>
        /// The information of what user is trying to access the database. If no user is supplied, no security checks are done.
        /// </summary>
        public Tuple<User, IPrincipal> UserInfo { get; protected set; }

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
        public TransactionRepository()
        {
            this._context = new EFDbContext();
            this._contextInScope = true;
            UserInfo = null;
        }

        /// <summary>
        /// Creates a repository with an in scope context and user information.
        /// </summary>
        /// <param name="user">The user that is accessing the database.</param>
        /// <param name="principal">The logged in principal for this repository.</param>
        public TransactionRepository(User user, IPrincipal principal)
            : this()
        {
            if (user != null && principal != null)
            {
                UserInfo = new Tuple<User, IPrincipal>(user, principal);
                _systemAdmin = UserInfo.Item2.HasGlobalPermissions();
                _companyAdmin = UserInfo.Item2.IsInRole("Company Administrators");
            }
        }

        /// <summary>
        /// Creates a repository with an out of scope context.
        /// </summary>
        /// <param name="context">The context to use for this database.</param>
        public TransactionRepository(EFDbContext context)
        {
            this._context = context;
            this._contextInScope = false;
            UserInfo = null;
        }

        /// <summary>
        /// Creates a repository with an out of scope context and user information.
        /// </summary>
        /// <param name="context">The context to use for this database.</param>
        /// <param name="user">The user that is accessing the database.</param>
        /// <param name="principal">The logged in principal for this repository.</param>
        public TransactionRepository(EFDbContext context, User user, IPrincipal principal)
            : this(context)
        {
            if (user != null && principal != null)
            {
                UserInfo = new Tuple<User, IPrincipal>(user, principal);
                _systemAdmin = UserInfo.Item2.HasGlobalPermissions();
                _companyAdmin = UserInfo.Item2.IsInRole("Company Administrators");
            }
        }

        #endregion

        #region Supporting Methods

        /// <summary>
        /// Sets the user to be used for checking permissions on data.
        /// </summary>
        /// <param name="user">The users information.</param>
        /// <param name="principal">The principal that is currently logged in.</param>
        public void SetUser(User user, IPrincipal principal)
        {
            UserInfo = new Tuple<User, IPrincipal>(user, principal);
            _systemAdmin = UserInfo.Item2.HasGlobalPermissions();
            _companyAdmin = UserInfo.Item2.IsInRole("Company Administrators");
        }

        /// <summary>
        /// Removes the user from the repository and inhibits reading of permissions. All CRUD actions will succeed.
        /// </summary>
        public void RemoveUser()
        {
            UserInfo = null;
            _systemAdmin = false;
            _companyAdmin = false;
        }

        /// <summary>
        /// Removes non permissives from a set of transaction requests.
        /// </summary>
        /// <param name="nodes">The set of transaction requests to check.</param>
        /// <param name="accessType">The type of access happening.</param>
        /// <returns>A set of transaction requests that can be used for the desired access type.</returns>
        public IEnumerable<TransactionRequest> RemoveNonPermissive(IEnumerable<TransactionRequest> nodes, SecurityAccessType accessType)
        {
            var transactions = new List<TransactionRequest>();
            foreach (var node in nodes)
            {
                if (this._context.CanAccess(node, accessType, nodes.Count() > 1))
                    transactions.Add(node);
            }
            return transactions;
        }

        /// <summary>
        /// Removes non permissives from a transaction request.
        /// </summary>
        /// <param name="node">The transaction request to check.</param>
        /// <param name="accessType">The type of access happening.</param>
        /// <returns>The transaction request or null.</returns>
        public TransactionRequest RemoveNonPermissive(TransactionRequest node, SecurityAccessType accessType)
        {
            var reg = RemoveNonPermissive(new List<TransactionRequest>() { node }, accessType).FirstOrDefault();
            if (reg == null)
                throw new InsufficientPermissionsException(minor: 61);
            return reg;
        }

        /// <summary>
        /// Removes non permissives from a set of transaction requests.
        /// </summary>
        /// <param name="nodes">The set of transaction requests to check.</param>
        /// <param name="accessType">The type of access happening.</param>
        /// <returns>A set of transaction requests that can be used for the desired access type.</returns>
        public async Task<IEnumerable<TransactionRequest>> RemoveNonPermissiveAsync(IEnumerable<TransactionRequest> nodes, SecurityAccessType accessType)
        {
            return await Task.Run(() =>
            {
                return RemoveNonPermissive(nodes, accessType);
            });
        }

        /// <summary>
        /// Removes non permissives from a transaction request.
        /// </summary>
        /// <param name="node">The transaction request to check.</param>
        /// <param name="accessType">The type of access happening.</param>
        /// <returns>The transaction request or null.</returns>
        public async Task<TransactionRequest> RemoveNonPermissiveAsync(TransactionRequest node, SecurityAccessType accessType)
        {
            return await RemoveNonPermissiveAsync(node, accessType);
        }

        /// <summary>
        /// Runs before the context is saved to databse.
        /// </summary>
        protected void OnBeforeSave()
        {
            // Create a list of registrants that have been finished.
            var completedTransactionRequests = new List<long>();
            // The registrants that need finishing.
            var modifiedTransactionRequests = Context.ChangeTracker.Entries<TransactionRequest>().Where(e => e.State == EntityState.Modified || e.State == EntityState.Added).ToList();
            var modifiedTransactionDatas = Context.ChangeTracker.Entries<TransactionDetail>().Where(e => e.State == EntityState.Modified || e.State == EntityState.Added).Select(e => e.Entity.TransactionRequest).Distinct().ToList();
            // We run through each registrant that has been directly modified. There is no chance of duplicates.
            foreach (var modifiedTransactionRequest in modifiedTransactionRequests)
            {
                completedTransactionRequests.Add((modifiedTransactionRequest.Entity as TransactionRequest).SortingId);
                if (!CanAccess(modifiedTransactionRequest.Entity as TransactionRequest, SecurityAccessType.Write))
                    throw new InsufficientPermissionsException("The user has insufficient permissions to " + (modifiedTransactionRequest.State == EntityState.Added ? "add" : "modify") + " registrant with Id " + (modifiedTransactionRequest.Entity as TransactionRequest).SortingId + " and UId " + (modifiedTransactionRequest.Entity as TransactionRequest).UId + ". The data was not saved into the database and the process has been terminated.", minor: (modifiedTransactionRequest.State == EntityState.Added ? 63 : 62));
                UpdateTransactionRequest(modifiedTransactionRequest);
            }
            foreach (var modifiedTransactionRequest in modifiedTransactionDatas.Where(r => !completedTransactionRequests.Contains(r.SortingId)))
            {
                if (!CanAccess(modifiedTransactionRequest, SecurityAccessType.Write))
                    throw new InsufficientPermissionsException("The user has insufficient permissions to modify registrant with Id " + modifiedTransactionRequest.SortingId + " and UId " + modifiedTransactionRequest.UId + ". The data was not saved into the database and the process has been terminated.", minor: 62);
                UpdateTransactionRequest(modifiedTransactionRequest);
            }
        }

        /// <summary>
        /// Updates transaction request's information.
        /// </summary>
        /// <param name="regObject">Either an <code>DbEntityEntry\<Registrant\></code> or <code>Transaction Request</code> object.</param>
        public void UpdateTransactionRequest(object regObject)
        {
            // The registrant in question.
            TransactionRequest node = regObject as TransactionRequest;
            DbEntityEntry<TransactionRequest> entry = regObject as DbEntityEntry<TransactionRequest>;
            if (entry != null)
                node = entry.Entity;
            if (node == null)
                return;
            if (entry == null)
                entry = _context.Entry<TransactionRequest>(node);

            // Add this registration to the list of registrations completed.
            // Get the modifier.
            var modifier = Guid.Empty;
            if (UserInfo != null && UserInfo.Item1 != null)
                modifier = UserInfo.Item1.UId;
            if (entry.State == EntityState.Modified)
            {
                node.DateModified = DateTimeOffset.Now;
            }
        }

        /// <summary>
        /// Checks to see if the user can access the transaction request.
        /// </summary>
        /// <param name="node">The transaction request to check access for.</param>
        /// <param name="accessType">The type of access to check against.</param>
        /// <returns>True if you can access.</returns>
        protected bool CanAccess(TransactionRequest node, SecurityAccessType accessType)
        {
            var nodes = RemoveNonPermissive(new List<TransactionRequest>() { node }, accessType);
            if (nodes.Count() == 0)
                return false;
            return true;
        }

        #endregion

        #region Database Methods

        /// <summary>
        /// Finds a transaction request by the primary key.
        /// </summary>
        /// <param name="key">The primary key for the transaction request record.</param>
        /// <param name="usePermissions">Set to false to skip permission checks.</param>
        /// <returns>A registrant.</returns>
        /// <exception cref="TransactionNotFoundException">Throws if the transaction request was not found.</exception>
        /// <exception cref="InsufficientPermissionsException">Throws if not enough permissions to read transaction request.</exception>
        public TransactionRequest Find(Guid key, bool usePermissions = true)
        {
            var node = Context.TransactionRequests.Find(key);
            if (node == null)
                throw new TransactionNotFoundException();
            if (usePermissions)
                return RemoveNonPermissive(node, SecurityAccessType.Read);
            return node;
        }

        /// <summary>
        /// Finds a registrant by the transaction request's key.
        /// </summary>
        /// <param name="key">The primary key for the transaction request record.</param>
        /// <param name="usePermissions">Set to false to skip permission checks.</param>
        /// <returns>A registrant.</returns>
        /// <exception cref="TransactionNotFoundException">Throws if the transaction request was not found.</exception>
        /// <exception cref="InsufficientPermissionsException">Throws if not enough permissions to read transaction request.</exception>
        public TransactionRequest First(Expression<Func<TransactionRequest, bool>> search, bool usePermissions = true)
        {
            var node = Context.TransactionRequests.FirstOrDefault(search);
            if (node == null)
                throw new TransactionNotFoundException();
            if (usePermissions)
                return RemoveNonPermissive(node, SecurityAccessType.Read);
            return node;
        }

        /// <summary>
        /// Finds a transaction request's by the primary key.
        /// </summary>
        /// <param name="key">The primary key for the transaction request record.</param>
        /// <param name="usePermissions">Set to false to skip permission checks.</param>
        /// <returns>A registrant.</returns>
        /// <exception cref="TransactionNotFoundException">Throws if the transaction request was not found.</exception>
        /// <exception cref="InsufficientPermissionsException">Throws if not enough permissions to read transaction request.</exception>
        public async Task<TransactionRequest> FindAsync(Guid key, bool usePermissions = true)
        {
            var node = await Context.TransactionRequests.FindAsync(key);
            if (node == null)
                throw new TransactionNotFoundException();
            if (usePermissions)
                return RemoveNonPermissive(node, SecurityAccessType.Read);
            return node;
        }

        /// <summary>
        /// Finds transaction requests by the supplied search criteria.
        /// </summary>
        /// <param name="search">The search to perform on the database.</param>
        /// <param name="usePermissions">Set to false to skip permission checks.</param>
        /// <returns>An enumeration of transaction requests.</returns>
        public IEnumerable<TransactionRequest> Search(Expression<Func<TransactionRequest, bool>> search, bool usePermissions = true)
        {
            var nodes = Context.TransactionRequests.Where(search).ToList().AsEnumerable();
            if (usePermissions)
                nodes = RemoveNonPermissive(nodes, SecurityAccessType.Read);
            return nodes;
        }

        /// <summary>
        /// Finds transaction requests by the supplied search criteria.
        /// </summary>
        /// <param name="search">The search to perform on the database.</param>
        /// <param name="usePermissions">Set to false to skip permission checks.</param>
        /// <returns>An enumeration of transaction requests.</returns>
        public async Task<IEnumerable<TransactionRequest>> SearchAsync(Expression<Func<TransactionRequest, bool>> search, bool usePermissions = true)
        {
            return await Task.Run(() =>
            {
                return Search(search, usePermissions);
            });
        }

        /// <summary>
        /// Removes a transaction requests from the context.
        /// </summary>
        /// <param name="node">The transaction request to remove.</param>
        /// <param name="usePermissions">Set to false to skip permission checks.</param>
        /// <returns>True if it did.</returns>
        /// <exception cref="TransactionNotFoundException">Throws if the transaction request was not found.</exception>
        /// <exception cref="InsufficientPermissionsException">Throws if not enough permissions to delete the transaction request.</exception>
        public bool Remove(TransactionRequest node, bool usePermissions = true)
        {
            if (node == null)
                throw new TransactionNotFoundException();
            if (usePermissions)
                node = RemoveNonPermissive(node, SecurityAccessType.Write);
            if (node == null)
                throw new InsufficientPermissionsException(minor: 64);
            RemoveTransactionRequest(node);
            return true;
        }

        /// <summary>
        /// Removes a transaction request from the context.
        /// </summary>
        /// <param name="node">The transaction request to remove.</param>
        /// <param name="usePermissions">Set to false to skip permission checks.</param>
        /// <returns>True if it did.</returns>
        /// <exception cref="TransactionNotFoundException">Throws if the transaction request was not found.</exception>
        /// <exception cref="InsufficientPermissionsException">Throws if not enough permissions to delete transaction request.</exception>
        public async Task<bool> RemoveAsync(TransactionRequest node, bool usePermissions = true)
        {
            return await Task.Run(() =>
            {
                return Remove(node);
            });
        }

        /// <summary>
        /// Removes a set of transaction requests from the context.
        /// </summary>
        /// <param name="nodes">The set of transaction requests to remove.</param>
        /// <param name="usePermissions">Set to false to skip permission checks.</param>
        /// <returns>True if it did.</returns>
        public bool RemoveRange(IEnumerable<TransactionRequest> nodes, bool usePermissions = true)
        {
            if (usePermissions)
                nodes = RemoveNonPermissive(nodes, SecurityAccessType.Write);
            foreach (var node in nodes)
                RemoveTransactionRequest(node);
            return true;
        }

        /// <summary>
        /// Removes a set of transaction requests from the context.
        /// </summary>
        /// <param name="nodes">The set of transaction requests to remove.</param>
        /// <param name="usePermissions">Set to false to skip permission checks.</param>
        /// <returns>True if it did.</returns>
        public async Task<bool> RemoveRangeAsync(IEnumerable<TransactionRequest> nodes, bool usePermissions = true)
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
            OnBeforeSave();
            return this._context.SaveChanges();
        }

        /// <summary>
        /// Saves the context to the database.
        /// </summary>
        /// <returns>the number of records affected.</returns>
        public async Task<int> CommitAsync()
        {
            OnBeforeSave();
            return await this._context.SaveChangesAsync();
        }

        /// <summary>
        /// Removes a transaction request from the context.
        /// </summary>
        /// <param name="node">The transaction request to remove.</param>
        protected void RemoveTransactionRequest(TransactionRequest node)
        {
            if (node == null)
                return;
            // Let's see if the node is already attached to a database.
            if (Context.Entry(node).State == EntityState.Detached)
                // The entity is not attached to the database. We return.
                return;

            // We need to move remove the attached transaction details.
            Context.TransactionDetails.RemoveRange(node.Details);
            Context.TransactionRequests.Remove(node);
        }

        /// <summary>
        /// Removes a set of transaction requests from the context.
        /// </summary>
        /// <param name="nodes">The set of transaction requests to remove.</param>
        protected void RemoveTransactionRequestRange(IEnumerable<TransactionRequest> nodes)
        {
            // We need to move throught the registrant and start removing the items that need removed;

            // We start with Transactions.
            Context.TransactionDetails.RemoveRange(nodes.SelectMany(n => n.Details));
            Context.TransactionRequests.RemoveRange(nodes);
        }

        /// <summary>
        /// Adds the specified transaction request to the context.
        /// </summary>
        /// <param name="node">The transaction request to add.</param>
        /// <param name="userPermissions">Set to false to skip permission checks.</param>
        public void Add(TransactionRequest node, bool userPermissions = true)
        {
            node = RemoveNonPermissive(node, SecurityAccessType.Write);
            if (node == null)
                return;
            this._context.TransactionRequests.Add(node);
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
