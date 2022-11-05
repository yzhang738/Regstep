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
using RSToolKit.Domain.Entities;
using RSToolKit.Domain.Identity;
using RSToolKit.Domain.Entities.Clients;

namespace RSToolKit.Domain.Data
{
    /// <summary>
    /// Handles the manipulation and retrieval of Forms from the database.
    /// </summary>
    public class FormRepository
        : IRepository<Form, Guid>, IDisposable
    {
        #region Variables and Properties

        protected EFDbContext _context;
        protected bool _contextInScope;
        protected ILogger _log;
        protected HashSet<Guid> _updatedForms;

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
        public FormRepository()
        {
            this._context = new EFDbContext();
            this._contextInScope = true;
            this._context.AddAddedItemDelegate(UpdateForms);
            this._context.AddModifiedDelegate(UpdateForms);
            this._context.AddOnSaveCompleteDelegate(OnSave);
            this._updatedForms = new HashSet<Guid>();
        }

        /// <summary>
        /// Creates a repository with an in scope context and user information.
        /// </summary>
        /// <param name="user">The user that is accessing the database.</param>
        /// <param name="principal">The logged in principal for this repository.</param>
        public FormRepository(User user, IPrincipal principal)
            : this()
        {
            if (user != null && principal != null)
                _context.SecuritySettings.SetUser(user, principal);
        }

        /// <summary>
        /// Creates a repository with an out of scope context.
        /// </summary>
        /// <param name="context">The context to use for this database.</param>
        public FormRepository(EFDbContext context)
        {
            this._context = context;
            this._contextInScope = false;
            this._context.AddAddedItemDelegate(UpdateForms);
            this._context.AddModifiedDelegate(UpdateForms);
            this._context.AddOnSaveCompleteDelegate(OnSave);
            this._updatedForms = new HashSet<Guid>();
        }

        /// <summary>
        /// Creates a repository with an out of scope context and user information.
        /// </summary>
        /// <param name="context">The context to use for this database.</param>
        /// <param name="user">The user that is accessing the database.</param>
        /// <param name="principal">The logged in principal for this repository.</param>
        public FormRepository(EFDbContext context, User user, IPrincipal principal)
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
        public IEnumerable<Form> RemoveNonPermissive(IEnumerable<Form> nodes, SecurityAccessType accessType)
        {
            var forms = new List<Form>();
            foreach (var node in nodes)
            {
                if (this._context.CanAccess(node, accessType, nodes.Count() > 1))
                    forms.Add(node);
            }
            return forms;
        }

        /// <summary>
        /// Removes non permissives from a transaction request.
        /// </summary>
        /// <param name="node">The form to check.</param>
        /// <param name="accessType">The type of access happening.</param>
        /// <returns>The transaction request or null.</returns>
        public Form RemoveNonPermissive(Form node, SecurityAccessType accessType)
        {
            var form = RemoveNonPermissive(new List<Form>() { node }, accessType).FirstOrDefault();
            if (form == null)
                throw new InsufficientPermissionsException(minor: 61);
            return form;
        }

        /// <summary>
        /// Removes non permissives from a set of forms.
        /// </summary>
        /// <param name="nodes">The set of forms to check.</param>
        /// <param name="accessType">The type of access happening.</param>
        /// <returns>A set of transaction requests that can be used for the desired access type.</returns>
        public async Task<IEnumerable<Form>> RemoveNonPermissiveAsync(IEnumerable<Form> nodes, SecurityAccessType accessType)
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
        public async Task<Form> RemoveNonPermissiveAsync(Form node, SecurityAccessType accessType)
        {
            return await RemoveNonPermissiveAsync(node, accessType);
        }

        /// <summary>
        /// Updates the forms.
        /// </summary>
        /// <param name="nodes">The forms to update.</param>
        public void UpdateForms(IEnumerable<DbEntityEntry> nodes)
        {
            foreach (var entity in nodes.Where(e => e.Entity is Form))
                UpdateForm(entity.Cast<Form>());
            foreach (var entity in nodes.Where(e => e.Entity is Seater))
                UpdateForm(this._context.Entry<Form>(((Seater)entity.Entity).Seating.Form));
            foreach (var entity in nodes.Where(e => e.Entity is Registrant))
                UpdateForm(this._context.Entry<Form>(((Registrant)entity.Entity).Form));
            foreach (var entity in nodes.Where(e => e.Entity is RegistrantData))
                UpdateForm(this._context.Entry<Form>(((RegistrantData)entity.Entity).Registrant.Form));
        }

        /// <summary>
        /// Updates the form.
        /// </summary>
        /// <param name="entity">The form entry to update.</param>
        protected void UpdateForm(DbEntityEntry<Form> entity)
        {
            var form = entity.Entity as Form;
            if (this._updatedForms.Contains(form.UId))
                return;
            this._updatedForms.Add(form.UId);
            var seatExpired = DateTimeOffset.Now;
            var min30 = TimeSpan.FromMinutes(30);
            foreach (var capacity in form.Seatings)
            {
                foreach (var seater in capacity.Seaters.Where(s => s.Seated && s.Registrant.Status == RegistrationStatus.Incomplete && (seatExpired - s.DateSeated) > min30).ToList())
                {
                    IComponent comp = seater.Component;
                    if (comp is IComponentItem)
                        seater.Registrant.RemoveItem(comp as IComponentItem);
                    this._context.Entry<Seater>(seater).State = EntityState.Deleted;
                }
            }
        }

        /// <summary>
        /// Clears the updated forms hashset.
        /// </summary>
        public void OnSave()
        {
            this._updatedForms.Clear();
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
        public Form Find(Guid key, bool usePermissions = true)
        {
            var node = Context.Forms.Find(key);
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
        public Form First(Expression<Func<Form, bool>> search, bool usePermissions = true)
        {
            var node = Context.Forms.FirstOrDefault(search);
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
        public async Task<Form> FindAsync(Guid key, bool usePermissions = true)
        {
            var node = await Context.Forms.FindAsync(key);
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
        public IEnumerable<Form> Search(Expression<Func<Form, bool>> search, bool usePermissions = true)
        {
            var nodes = Context.Forms.Where(search).ToList().AsEnumerable();
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
        public async Task<IEnumerable<Form>> SearchAsync(Expression<Func<Form, bool>> search, bool usePermissions = true)
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
        public bool Remove(Form node, bool usePermissions = true)
        {
            if (node == null)
                throw new TransactionNotFoundException();
            if (usePermissions)
                node = RemoveNonPermissive(node, SecurityAccessType.Write);
            if (node == null)
                throw new InsufficientPermissionsException(minor: 64);
            _RemoveForm(node);
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
        public async Task<bool> RemoveAsync(Form node, bool usePermissions = true)
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
        public bool RemoveRange(IEnumerable<Form> nodes, bool usePermissions = true)
        {
            if (usePermissions)
                nodes = RemoveNonPermissive(nodes, SecurityAccessType.Write);
            foreach (var node in nodes)
                _RemoveForm(node);
            return true;
        }

        /// <summary>
        /// Removes a set of transaction requests from the context.
        /// </summary>
        /// <param name="nodes">The set of transaction requests to remove.</param>
        /// <param name="usePermissions">Set to false to skip permission checks.</param>
        /// <returns>True if it did.</returns>
        public async Task<bool> RemoveRangeAsync(IEnumerable<Form> nodes, bool usePermissions = true)
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
        protected void _RemoveForm(Form node)
        {
            if (node == null)
                return;
            // Let's see if the node is already attached to a database.
            if (Context.Entry(node).State == EntityState.Detached)
                // The entity is not attached to the database. We return.
                return;
            // We need to move remove the attached transaction details.
            throw new NotSupportedException("This has not been built yet.");
        }

        /// <summary>
        /// Removes a set of transaction requests from the context.
        /// </summary>
        /// <param name="nodes">The set of transaction requests to remove.</param>
        protected void _RemoveFormRange(IEnumerable<Form> nodes)
        {
            foreach (var node in nodes)
                _RemoveForm(node);
        }

        /// <summary>
        /// Adds the specified transaction request to the context.
        /// </summary>
        /// <param name="node">The transaction request to add.</param>
        /// <param name="userPermissions">Set to false to skip permission checks.</param>
        public void Add(Form node, bool userPermissions = true)
        {
            node = RemoveNonPermissive(node, SecurityAccessType.Write);
            if (node == null)
                return;
            this._context.Forms.Add(node);
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
