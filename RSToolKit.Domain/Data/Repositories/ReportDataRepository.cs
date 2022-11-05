using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;
using RSToolKit.Domain.Entities;
using RSToolKit.Domain.Entities.Clients;
using System.Security.Principal;
using RSToolKit.Domain.Entities;
using System.Data.Entity;
using RSToolKit.Domain.Security;
using System.Data.Objects;
using System.Data.Entity.Infrastructure;
using RSToolKit.Domain.Exceptions;

namespace RSToolKit.Domain.Data
{
    public class ReportDataRepository
        : IRepository<ReportData, long>
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
        public ReportDataRepository()
        {
            this._context = new EFDbContext();
            this._contextInScope = true;
        }

        /// <summary>
        /// Creates a repository with an in scope context and user information.
        /// </summary>
        /// <param name="user">The user that is accessing the database.</param>
        /// <param name="principal">The logged in principal for this repository.</param>
        public ReportDataRepository(User user, IPrincipal principal)
            : this()
        {
            if (user != null && principal != null)
                _context.SecuritySettings.SetUser(user, principal);
        }

        /// <summary>
        /// Creates a repository with an out of scope context.
        /// </summary>
        /// <param name="context">The context to use for this database.</param>
        public ReportDataRepository(EFDbContext context)
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
        public ReportDataRepository(EFDbContext context, User user, IPrincipal principal)
            : this(context)
        {
            if (user != null && principal != null)
                _context.SecuritySettings.SetUser(user, principal);
        }

        #endregion

        #region Supporting Methods

        /// <summary>
        /// Removes non permissives from a set of transaction requests.
        /// </summary>
        /// <param name="nodes">The set of transaction requests to check.</param>
        /// <param name="accessType">The type of access happening.</param>
        /// <returns>A set of transaction requests that can be used for the desired access type.</returns>
        public IEnumerable<ReportData> RemoveNonPermissive(IEnumerable<ReportData> nodes, SecurityAccessType accessType)
        {
            var reports = new List<ReportData>();
            foreach (var node in nodes)
            {
                if (_context.CanAccess(node, accessType, nodes.Count() > 1))
                    reports.Add(node);
            }
            return reports;
        }

        /// <summary>
        /// Removes non permissives from a transaction request.
        /// </summary>
        /// <param name="node">The form to check.</param>
        /// <param name="accessType">The type of access happening.</param>
        /// <returns>The transaction request or null.</returns>
        public ReportData RemoveNonPermissive(ReportData node, SecurityAccessType accessType)
        {
            var reg = RemoveNonPermissive(new List<ReportData>() { node }, accessType).FirstOrDefault();
            if (reg == null)
                throw new InsufficientPermissionsException(minor: 61);
            return reg;
        }

        /// <summary>
        /// Removes non permissives from a set of forms.
        /// </summary>
        /// <param name="nodes">The set of forms to check.</param>
        /// <param name="accessType">The type of access happening.</param>
        /// <returns>A set of transaction requests that can be used for the desired access type.</returns>
        public async Task<IEnumerable<ReportData>> RemoveNonPermissiveAsync(IEnumerable<ReportData> nodes, SecurityAccessType accessType)
        {
            return await Task.Run(() =>
            {
                return RemoveNonPermissive(nodes, accessType);
            });
        }

        /// <summary>
        /// Removes non permissive from a form if applicable.
        /// </summary>
        /// <param name="node">The form to check.</param>
        /// <param name="accessType">The type of access happening.</param>
        /// <returns>The form or null.</returns>
        public async Task<ReportData> RemoveNonPermissiveAsync(ReportData node, SecurityAccessType accessType)
        {
            return await RemoveNonPermissiveAsync(node, accessType);
        }

        /// <summary>
        /// Updates form's information.
        /// </summary>
        /// <param name="regObject">Either an <code>DbEntityEntry\<ReportData\></code> or <code>ReportData</code> object.</param>
        public void UpdateReportDatas(IEnumerable<DbEntityEntry<INode>> entries)
        {
            foreach (var entry in entries.OfType<DbEntityEntry<ReportData>>())
            {
                // The registrant in question.
                ReportData node = null;
                if (entry != null)
                    node = entry.Entity;
                if (node == null)
                    return;
            }
        }

        #endregion

        #region Database Methods

        /// <summary>
        /// Finds a form request by the primary key.
        /// </summary>
        /// <param name="key">The primary key for the form record.</param>
        /// <param name="usePermissions">Set to false to skip permission checks.</param>
        /// <returns>A registrant.</returns>
        /// <exception cref="TransactionNotFoundException">Throws if the transaction request was not found.</exception>
        /// <exception cref="InsufficientPermissionsException">Throws if not enough permissions to read transaction request.</exception>
        public ReportData Find(long key, bool usePermissions = true)
        {
            var node = Context.ReportData.Find(key);
            if (node == null)
                throw new ReportDataNotFoundException();
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
        public ReportData First(Expression<Func<ReportData, bool>> search, bool usePermissions = true)
        {
            var node = Context.ReportData.FirstOrDefault(search);
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
        public async Task<ReportData> FindAsync(long key, bool usePermissions = true)
        {
            var node = await Context.ReportData.FindAsync(key);
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
        public IEnumerable<ReportData> Search(Expression<Func<ReportData, bool>> search, bool usePermissions = true)
        {
            var nodes = Context.ReportData.Where(search).ToList().AsEnumerable();
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
        public async Task<IEnumerable<ReportData>> SearchAsync(Expression<Func<ReportData, bool>> search, bool usePermissions = true)
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
        public bool Remove(ReportData node, bool usePermissions = true)
        {
            if (node == null)
                throw new TransactionNotFoundException();
            if (usePermissions)
                node = RemoveNonPermissive(node, SecurityAccessType.Write);
            if (node == null)
                throw new InsufficientPermissionsException(minor: 64);
            _RemoveReportData(node);
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
        public async Task<bool> RemoveAsync(ReportData node, bool usePermissions = true)
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
        public bool RemoveRange(IEnumerable<ReportData> nodes, bool usePermissions = true)
        {
            if (usePermissions)
                nodes = RemoveNonPermissive(nodes, SecurityAccessType.Write);
            foreach (var node in nodes)
                _RemoveReportData(node);
            return true;
        }

        /// <summary>
        /// Removes a set of transaction requests from the context.
        /// </summary>
        /// <param name="nodes">The set of transaction requests to remove.</param>
        /// <param name="usePermissions">Set to false to skip permission checks.</param>
        /// <returns>True if it did.</returns>
        public async Task<bool> RemoveRangeAsync(IEnumerable<ReportData> nodes, bool usePermissions = true)
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
        /// Removes a transaction request from the context.
        /// </summary>
        /// <param name="node">The transaction request to remove.</param>
        protected void _RemoveReportData(ReportData node)
        {
            if (node == null)
                return;
            // Let's see if the node is already attached to a database.
            if (Context.Entry(node).State == EntityState.Detached)
                // The entity is not attached to the database. We return.
                return;
            _context.RemoveReportData(node);
        }

        /// <summary>
        /// Removes a set of transaction requests from the context.
        /// </summary>
        /// <param name="nodes">The set of transaction requests to remove.</param>
        protected void _RemoveReportDataRange(IEnumerable<ReportData> nodes)
        {
            foreach (var node in nodes)
                _RemoveReportData(node);
        }

        /// <summary>
        /// Adds the specified transaction request to the context.
        /// </summary>
        /// <param name="node">The transaction request to add.</param>
        /// <param name="userPermissions">Set to false to skip permission checks.</param>
        public void Add(ReportData node, bool userPermissions = true)
        {
            node = RemoveNonPermissive(node, SecurityAccessType.Write);
            if (node == null)
                return;
            this._context.ReportData.Add(node);
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
