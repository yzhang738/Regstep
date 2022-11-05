using Microsoft.AspNet.Identity.EntityFramework;
using RSToolKit.Domain.Entities;
using RSToolKit.Domain.Entities.Clients;
using RSToolKit.Domain.Entities.Components;
using RSToolKit.Domain.Entities.Email;
using RSToolKit.Domain.Entities.MerchantAccount;
using RSToolKit.Domain.Errors;
using RSToolKit.Domain.Security;
using System;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;
using System.Data.Entity.ModelConfiguration;
using System.Data.Entity.Core.Objects;
using RSToolKit.Domain.Logging;
using System.Collections.Generic;
using HtmlAgilityPack;
using RSToolKit.Domain.Exceptions;
using System.Data.Entity.Infrastructure;
using System.Threading.Tasks;
using RSToolKit.Domain.Entities.Navigation;
using RSToolKit.Domain.Entities.Finances;
using RSToolKit.Domain.Data.Info;
using RSToolKit.Domain.Entities.Manipulations;
using RSToolKit.Domain.Entities.Reports;

namespace RSToolKit.Domain.Data
{
    /// <summary>
    /// Holds the database information for the application.
    /// </summary>
    public class EFDbContext
        : IdentityDbContext<User>, IDataContext
    {

        private bool _locked = false;

        public SecuritySettings SecuritySettings { get; set; }

        public delegate void AddedItemDelegate(IEnumerable<DbEntityEntry> nodes);
        protected List<AddedItemDelegate> _addedItemDelegates;
        public delegate void ModifiedItemDelegate(IEnumerable<DbEntityEntry> nodes);
        protected List<ModifiedItemDelegate> _modifiedItemDelegates;
        public delegate void DeletedItemDelegate(IEnumerable<DbEntityEntry> nodes);
        protected List<DeletedItemDelegate> _deletedItemDelegates;
        public delegate void OnSaveCompleteDelegate();
        protected List<OnSaveCompleteDelegate> _onSaveCompleteDelegates;


        #region DbSets

        //Components
        public DbSet<Component> Components { get; set; }
        public DbSet<CustomGroup> CustomGroups { get; set; }
        public DbSet<Company> Companies { get; set; }
        public DbSet<SmtpServer> SmtpServers { get; set; }
        public DbSet<EmailEvent> EmailEvents { get; set; }
        public DbSet<Stylesheet> Stylesheets { get; set; }
        public DbSet<CSS> CSS { get; set; }
        public DbSet<PriceGroup> PriceGroups { get; set; }
        public DbSet<Prices> Prices { get; set; }
        public DbSet<Price> Price { get; set; }
        public DbSet<Audience> Audiences { get; set; }
        public DbSet<RSEmail> RSEmails { get; set; }
        public DbSet<EmailTemplate> EmailTemplates { get; set; }
        public DbSet<EmailArea> EmailAreas { get; set; }
        public DbSet<EmailTemplateArea> EmailTemplateAreas { get; set; }
        public DbSet<SavedList> SavedLists { get; set; }
        public DbSet<Contact> Contacts { get; set; }
        public DbSet<ContactData> ContactData { get; set; }
        public DbSet<CustomText> CustomTexts { get; set; }
        public DbSet<LogicBlock> LogicBlocks { get; set; }
        public DbSet<Logic> Logics { get; set; }
        public DbSet<Registrant> Registrants { get; set; }
        public DbSet<RegistrantData> RegistrantData { get; set; }
        public DbSet<OldRegistrant> OldRegistrants { get; set; }
        public DbSet<OldRegistrantData> OldRegistrantData { get; set; }
        public DbSet<Form> Forms { get; set; }
        public DbSet<Seating> Seatings { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<EmailCampaign> EmailCampaigns { get; set; }
        public DbSet<Variable> Variables { get; set; }
        public DbSet<Folder> Folders { get; set; }
        public DbSet<Pointer> Pointers { get; set; }
        public DbSet<CreditCard> CreditCards { get; set; }
        public DbSet<MerchantAccountInfo> MerchantAccountInfo { get; set; }
        public DbSet<TransactionRequest> TransactionRequests { get; set; }
        public DbSet<TransactionDetail> TransactionDetails { get; set; }
        public DbSet<EmailVariable> EmailVariables { get; set; }
        public DbSet<EmailTemplateVariable> EmailTemplateVariables { get; set; }
        public DbSet<TemplateEmailAreaVariable> TemplateEmailAreaVariables { get; set; }
        public DbSet<EmailAreaVariable> EmailAreaVariables { get; set; }
        public DbSet<DefaultFormStyle> DefaultFormStyles { get; set; }
        public DbSet<FormStyle> FormStyles { get; set; }
        public DbSet<NodeType> Types { get; set; }
        public DbSet<FormTemplate> FormTemplates { get; set; }
        public DbSet<ComponentStyle> ComponentStyles { get; set; }
        public DbSet<PriceStyle> PriceStyles { get; set; }
        public DbSet<SeatingStyle> SeatingStyles { get; set; }
        public DbSet<DefaultComponentOrder> DefaultComponentOrders { get; set; }
        public DbSet<Page> Pages { get; set; }
        public DbSet<Panel> Panels { get; set; }
        public DbSet<Seater> Seaters { get; set; }
        public DbSet<LogicGroup> LogicGroups { get; set; }
        public DbSet<LogicStatement> LogicStatements { get; set; }
        public DbSet<LogicCommand> LogicCommands { get; set; }
        public DbSet<PromotionCode> PromotionCodes { get; set; }
        public DbSet<PromotionCodeEntry> PromotionCodeEntries { get; set; }
        public DbSet<RegistrantFile> RegistrantFiles { get; set; }
        public DbSet<EmailSend> EmailSends { get; set; }
        public DbSet<SingleFormReport> SingleFormReports { get; set; }
        public DbSet<QueryFilter> QueryFilters { get; set; }
        public DbSet<Adjustment> Adjustments { get; set; }
        public DbSet<Unsubscribe> Unsubscribes { get; set; }
        public DbSet<AvailableRoles> AvailableRoles { get; set; }
        public DbSet<ContactHeader> ContactHeaders { get; set; }
        public DbSet<ContactReport> ContactReports { get; set; }
        public DbSet<PermissionSet> PermissionsSets { get; set; }
        public DbSet<AdvancedInventoryReport> AdvancedInventoryReports { get; set; }
        public DbSet<RSHtmlEmail> RSHtmlEmails { get; set; }
        public DbSet<AccessLog> AccessLog { get; set; }
        public DbSet<SavedTable> SavedTables { get; set; }
        public DbSet<Log> Logs { get; set; }
        public DbSet<LogNote> LogNotes { get; set; }
        public DbSet<SavedEmailTable> SavedEmailTables { get; set; }
        public DbSet<ReportData> ReportData { get; set; }
        public DbSet<TinyUrl> TinyUrls { get; set; }
        public DbSet<Counter> Counters { get; set; }
        public DbSet<UserTrail> UserTrails { get; set; }
        public DbSet<CompanyLogo> Logos { get; set; }
        public DbSet<Charge> Charges { get; set; }
        public DbSet<SecurityLogEntry> SecurityLogEntries { get; set; }
        public DbSet<TokenData> TokenData { get; set; }
        public DbSet<Token> Tokens { get; set; }
        public DbSet<ProgressStatus> ProgressStati { get; set; }
        public DbSet<RegistrantNote> RegistrantNotes { get; set; }
        public DbSet<ItemAccess> ItemAccess { get; set; }
        public DbSet<GlobalReport> GlobalReports { get; set; }
        #endregion

        /// <summary>
        /// Initaites the context with the specified connection string.
        /// </summary>
        /// <param name="connString">The connection string to use.</param>
        public EFDbContext(string connString)
            : base(connString)
        {
            _locked = false;
            SecuritySettings = new Security.SecuritySettings();
            this._addedItemDelegates = new List<AddedItemDelegate>();
            this._modifiedItemDelegates = new List<ModifiedItemDelegate>();
            this._deletedItemDelegates = new List<DeletedItemDelegate>();
            this._onSaveCompleteDelegates = new List<OnSaveCompleteDelegate>();
            ((IObjectContextAdapter)this).ObjectContext.CommandTimeout = 600;
        }

        /// <summary>
        /// Initiate the default context.
        /// </summary>
        public EFDbContext()
            : base()
        {
            _locked = false;
            SecuritySettings = new Security.SecuritySettings();
            this._addedItemDelegates = new List<AddedItemDelegate>();
            this._modifiedItemDelegates = new List<ModifiedItemDelegate>();
            this._deletedItemDelegates = new List<DeletedItemDelegate>();
            this._onSaveCompleteDelegates = new List<OnSaveCompleteDelegate>();
            ((IObjectContextAdapter)this).ObjectContext.CommandTimeout = 600;
        }

        /// <summary>
        /// Locks the database from performing any saves on data.
        /// </summary>
        public void Lock()
        {
            _locked = true;
        }

        /// <summary>
        /// Unlocks the database so data can be saved.
        /// </summary>
        public void Unlock()
        {
            _locked = false;
        }

        /// <summary>
        /// Rolls the context back to it's original state.
        /// </summary>
        public void RollBackChanges()
        {
            _locked = true;
            var changedEntries = ChangeTracker.Entries().Where(x => x.State != EntityState.Unchanged).ToList();

            foreach (var entry in changedEntries.Where(x => x.State == EntityState.Modified))
            {
                entry.CurrentValues.SetValues(entry.OriginalValues);
                entry.State = EntityState.Unchanged;
            }

            foreach (var entry in changedEntries.Where(x => x.State == EntityState.Added))
            {
                entry.State = EntityState.Detached;
            }

            foreach (var entry in changedEntries.Where(x => x.State == EntityState.Deleted))
            {
                entry.State = EntityState.Unchanged;
            }
            _locked = false;
        }

        /// <summary>
        /// Runs when the context is being built.
        /// </summary>
        /// <param name="modelBuilder">The modelBuilder.</param>
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            //modelBuilder.Conventions.Remove<ManyToManyCascadeDeleteConvention>();
            modelBuilder.Conventions.Remove<OneToManyCascadeDeleteConvention>();
            modelBuilder.Entity<User>()
                .HasRequired<Company>(u => u.Company)
                .WithMany(c => c.Users)
                .HasForeignKey(c => c.CompanyKey);
            modelBuilder.Entity<User>()
                .HasOptional<Company>(u => u.WorkingCompany)
                .WithMany(c => c.WorkingUsers)
                .HasForeignKey(u => u.WorkingCompanyKey);
            modelBuilder.Entity<User>()
                .HasMany<CustomGroup>(u => u.CustomGroups)
                .WithMany(c => c.Users);
            base.OnModelCreating(modelBuilder);
        }

        /// <summary>
        /// Gets the type of an object.
        /// </summary>
        /// <param name="obj">The object to get the type of.</param>
        /// <returns>The type of object.</returns>
        public Type GetObjectType(object obj)
        {
            try
            {
                return ObjectContext.GetObjectType(obj.GetType()).BaseType;
            }
            catch (Exception)
            {
                return typeof(object);
            }
        }

        /// <summary>
        /// Adds the delegate to the <code>_onSaveCompleteDelegates</code> to be called before saving.
        /// </summary>
        /// <param name="sDelegate">The delegate to add.</param>
        public void AddOnSaveCompleteDelegate(OnSaveCompleteDelegate sDelegate)
        {
            this._onSaveCompleteDelegates.Add(sDelegate);
        }

        /// <summary>
        /// Adds the delegate to the ModifiedItemsDelegate to be called before saving.
        /// </summary>
        /// <param name="mDelegate">The delegate to add to the list of delegates.</param>
        public void AddModifiedDelegate(ModifiedItemDelegate mDelegate)
        {
            _modifiedItemDelegates.Add(mDelegate);
        }

        /// <summary>
        /// Adds the delegate to the AddedItemsDelegate to be called before saving.
        /// </summary>
        /// <param name="aDelegate">The delegate to add to the list of delegates.</param>
        public void AddAddedItemDelegate(AddedItemDelegate aDelegate)
        {
            _addedItemDelegates.Add(aDelegate);
        }

        /// <summary>
        /// Adds the delegate to the DeletedItemsDelegate to be called before saving.
        /// </summary>
        /// <param name="dDelegate">The delegate to add to the list of delegates.</param>
        public void AddDeletedItemDelegate(DeletedItemDelegate dDelegate)
        {
            _deletedItemDelegates.Add(dDelegate);
        }

        /// <summary>
        /// Saves the data to the database as long as no lock is on the context.
        /// </summary>
        /// <returns>The number of rows affected.</returns>
        public override int SaveChanges()
        {
            if (this._locked)
                throw new Exception("The context is locked to save functions.");
            this._locked = true;
            this._OnBeforeSave();
            var affected = 0;
            try
            {
                affected = base.SaveChanges();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                foreach (var entry in ex.Entries)
                {
                    entry.OriginalValues.SetValues(entry.CurrentValues);
                }
            }
            catch (Exception sex)
            {
                var iLog = new Logger()
                {
                    Thread = "Main",
                    LoggingMethod = "Global"
                };
                iLog.Error(sex);
                var exc = new Exception("An sql exception was handled.", sex);
                throw exc;
            }
            this._OnSaveComplete();
            this._locked = false;
            return affected;
        }

        /// <summary>
        /// Saves the changes asynchronoulsy to the database.
        /// </summary>
        /// <returns>The number of records affected.</returns>
        public async override Task<int> SaveChangesAsync()
        {
            return await Task.Run(() =>
            {
                return SaveChanges();
            });
        }

        /// <summary>
        /// Runs after all saving is complete.
        /// </summary>
        protected void _OnSaveComplete()
        {
            foreach (var t_delegate in this._onSaveCompleteDelegates)
            {
                t_delegate();
            }
        }

        /// <summary>
        /// Runs before saving happens.
        /// </summary>
        protected void _OnBeforeSave()
        {
            var modified = ChangeTracker.Entries().Where(e => e.State == EntityState.Modified);
            var added = ChangeTracker.Entries().Where(e => e.State == EntityState.Added);
            var deleted = ChangeTracker.Entries().Where(e => e.State == EntityState.Deleted);
            foreach (var _thisDelegate in _addedItemDelegates)
                _thisDelegate(added);
            foreach (var _thisDelegate in _modifiedItemDelegates)
                _thisDelegate(modified);
            foreach (var _thisDelegate in _deletedItemDelegates)
                _thisDelegate(deleted);
            foreach (var entity in ChangeTracker.Entries().Where(e => e.State == EntityState.Added || e.State == EntityState.Modified))
            {
                var item = entity.Entity as INode;
                if (item == null)
                    continue;
                item.DateModified = DateTimeOffset.Now;
                item.ModificationToken = Guid.NewGuid();
                if (SecuritySettings.AppUser != null)
                    item.ModifiedBy = SecuritySettings.AppUser.UId;
            }
        }

        /// <summary>
        /// Creates a new context.
        /// </summary>
        /// <returns>The context that is created.</returns>
        public static EFDbContext Create()
        {
            var context = new EFDbContext();
            return context;
        }

        #region Removal Helpers

        /// <summary>
        /// Removes the <code>MerchantAccountInfo</code> from the database.
        /// </summary>
        /// <param name="node">The <code>MerchantAccountInfo</code> to remove.</param>
        public void RemoveMerchantAccount(MerchantAccountInfo node)
        {
            node.Forms.ForEach(f => { f.MerchantAccount = null; f.MerchantAccountKey = null; });
            MerchantAccountInfo.Remove(node);
        }

        /// <summary>
        /// Marks the ReportData to be removed from the underlying database.
        /// </summary>
        /// <param name="node">The ReportData to be removed.</param>
        public void RemoveReportData(ReportData node)
        {
            if (node == null)
                return;
            if (!CanAccess(node, Security.SecurityAccessType.Write))
                return;
            // Let's see if the node is already attached to a database.
            if (Entry(node).State == EntityState.Detached)
                // The entity is not attached to the database. We return.
                return;
            ReportData.Remove(node);
        }

        /// <summary>
        /// Marks the CustomText to be removed from the underlying database.
        /// </summary>
        /// <param name="node">The CustomText to be removed.</param>
        public void RemoveCustomText(CustomText node)
        {
            if (node == null)
                return;
            if (!CanAccess(node, Security.SecurityAccessType.Write))
                return;
            // Let's see if the node is already attached to a database.
            if (Entry(node).State == EntityState.Detached)
                // The entity is not attached to the database. We return.
                return;
            CustomTexts.Remove(node);
        }

        /// <summary>
        /// Marks the LogicBlock to be removed from the underlying database.
        /// </summary>
        /// <param name="node">The LogicBlock to be removed.</param>
        public void RemoveLogicBlock(LogicBlock node)
        {
            if (node == null)
                return;
            if (!CanAccess(node, Security.SecurityAccessType.Write))
                return;
            // Let's see if the node is already attached to a database.
            if (Entry(node).State == EntityState.Detached)
                // The entity is not attached to the database. We return.
                return;
            node.Logics.ForEach(l => RemoveLogic(l));
            LogicBlocks.Remove(node);
        }

        #region Registrant
        /// <summary>
        /// Marks the registrant to be removed from the underlying database.
        /// </summary>
        /// <param name="node">The registrant to remove.</param>
        public void RemoveRegistrant(Registrant node)
        {
            if (node == null)
                return;
            if (!CanAccess(node, Security.SecurityAccessType.Write))
                return;
            // Let's see if the node is already attached to a database.
            if (Entry(node).State == EntityState.Detached)
                // The entity is not attached to the database. We return.
                return;

            // We need to move throught the registrant and start removing the items that need removed;

            // We start with Transactions.
            foreach (var transaction in node.TransactionRequests)
            {
                // We don't ever want to lose transaction data, so we just remove the key to the registrant.
                transaction.RegistrantKey = null;
            }

            // Now we need to remove all the data points.
            node.Data.ForEach(d => RemoveRegistrantData(d));

            // Now we remove old registrations
            // We grab all the old registration data.
            var oldReg_data = node.OldRegistrations.SelectMany(r => r.Data);
            // And then we remove it.
            OldRegistrantData.RemoveRange(oldReg_data);
            // Then we remove all the old registrations.
            OldRegistrants.RemoveRange(node.OldRegistrations);

            // Now we remove promotion code entries.
            PromotionCodeEntries.RemoveRange(node.PromotionalCodes);

            // Now we remove email sends.
            // We grab all email events.
            var email_events = node.EmailSends.SelectMany(e => e.EmailEvents);
            // Then we remove them.
            EmailEvents.RemoveRange(email_events);
            // Then we remove the sends.
            EmailSends.RemoveRange(node.EmailSends);

            // Next we remove adjustments.
            Adjustments.RemoveRange(node.Adjustments);

            // Now we remove all seats
            Seaters.RemoveRange(node.Seatings);
            Registrants.Remove(node);
        }

        /// <summary>
        /// Marks the RegistrantData to be removed from the underlying database.
        /// </summary>
        /// <param name="node">The RegistrantData to be reomved.</param>
        public void RemoveRegistrantData(RegistrantData node)
        {
            if (node == null)
                return;
            if (!CanAccess(node, Security.SecurityAccessType.Write))
                return;
            // Let's see if the node is already attached to a database.
            if (Entry(node).State == EntityState.Detached)
                // The entity is not attached to the database. We return.
                return;
            RemoveRegistrantFile(node.File);
            RegistrantData.Remove(node);
        }

        /// <summary>
        /// Marks the RegistrantFile to be removed from the underlying database.
        /// </summary>
        /// <param name="node"></param>
        public void RemoveRegistrantFile(RegistrantFile node)
        {
            if (node == null)
                return;
            if (!CanAccess(node, Security.SecurityAccessType.Write))
                return;
            // Let's see if the node is already attached to a database.
            if (Entry(node).State == EntityState.Detached)
                // The entity is not attached to the database. We return.
                return;
            RegistrantFiles.Remove(node);
        }
        #endregion

        #region Form
        /// <summary>
        /// Marks the form to be removed from the underlying database.
        /// </summary>
        /// <param name="form">The form to be removed.</param>
        public void RemoveForm(Form node)
        {
            if (node == null)
                return;
            if (!CanAccess(node, Security.SecurityAccessType.Write))
                return;
            // Let's see if the node is already attached to a database.
            if (Entry(node).State == EntityState.Detached)
                // The entity is not attached to the database. We return.
                return;
            RemoveTinyUrl(node.TineyUrl);
            node.Emails.ForEach(e => RemoveRSEmail(e));
            node.LogicBlocks.ForEach(l => RemoveLogicBlock(l));
            node.Pages.ForEach(p => RemovePage(p));
            node.Audiences.ForEach(a => RemoveAudience(a));
            node.Variables.ForEach(v => RemoveVariable(v));
            node.Seatings.ForEach(s => RemoveSeating(s));
            node.CustomTexts.ForEach(c => RemoveCustomText(c));
            node.FormStyles.ForEach(f => RemoveFormStyle(f));
            node.DefaultComponentOrders.ForEach(d => RemoveDefaultComponentOrder(d));
            node.PromotionalCodes.ForEach(p => RemovePromotionCode(p));
            node.Registrants.ForEach(r => RemoveRegistrant(r));
            node.HtmlEmails.ForEach(h => RemoveRSHtmlEmail(h));
            node.CustomReports.ForEach(c => RemoveCustomReport(c));
            ReportData.Where(d => d.TableId == node.UId).ToList().ForEach(r => RemoveReportData(r));
            node.InventoryReports.ForEach(r => RemoveAdvancedInventoryReport(r));
            Forms.Remove(node);
        }

        /// <summary>
        /// Marks the SingleFormReport to be removed from the underlying databse.
        /// </summary>
        /// <param name="node">The SingleFormReport to be removed.</param>
        public void RemoveCustomReport(SingleFormReport node)
        {
            if (node == null)
                return;
            if (!CanAccess(node, Security.SecurityAccessType.Write))
                return;
            // Let's see if the node is already attached to a database.
            if (Entry(node).State == EntityState.Detached)
                // The entity is not attached to the database. We return.
                return;
            node.Filters.ForEach(f => RemoveQueryFilter(f));
            SingleFormReports.Remove(node);
        }

        /// <summary>
        /// Removes the item from the database.
        /// </summary>
        /// <param name="node">The <code>AdvancedInventoryReport</code> to remove.</param>
        public void RemoveAdvancedInventoryReport(AdvancedInventoryReport node)
        {
            AdvancedInventoryReports.Remove(node);
        }

        /// <summary>
        /// Marks the QueryFilter to be removed from the underlying database.
        /// </summary>
        /// <param name="node">The QueryFilter to be removed.</param>
        public void RemoveQueryFilter(QueryFilter node)
        {
            if (node == null)
                return;
            if (!CanAccess(node, Security.SecurityAccessType.Write))
                return;
            // Let's see if the node is already attached to a database.
            if (Entry(node).State == EntityState.Detached)
                // The entity is not attached to the database. We return.
                return;
            QueryFilters.Remove(node);
        }

        /// <summary>
        /// Marks the FormStyle to be removed from the underlying database.
        /// </summary>
        /// <param name="node">The FormStyle to be removed.</param>
        public void RemoveFormStyle(FormStyle node)
        {
            if (node == null)
                return;
            if (!CanAccess(node, Security.SecurityAccessType.Write))
                return;
            // Let's see if the node is already attached to a database.
            if (Entry(node).State == EntityState.Detached)
                // The entity is not attached to the database. We return.
                return;
            FormStyles.Remove(node);
        }

        #region PromotionCodes
        /// <summary>
        /// Marks the PromotionCode to be removed fromt he underlying database.
        /// </summary>
        /// <param name="node"></param>
        public void RemovePromotionCode(PromotionCode node)
        {
            if (node == null)
                return;
            if (!CanAccess(node, Security.SecurityAccessType.Write))
                return;
            // Let's see if the node is already attached to a database.
            if (Entry(node).State == EntityState.Detached)
                // The entity is not attached to the database. We return.
                return;
            node.Entries.ForEach(e => RemovePromotionCodeEntry(e));
            PromotionCodes.Remove(node);
        }

        /// <summary>
        /// Marks the PromotionCodeEntry to be removed from the underlying database.
        /// </summary>
        /// <param name="node">The PromotionCodeEntry to be removed.</param>
        public void RemovePromotionCodeEntry(PromotionCodeEntry node)
        {
            if (node == null)
                return;
            if (!CanAccess(node, Security.SecurityAccessType.Write))
                return;
            // Let's see if the node is already attached to a database.
            if (Entry(node).State == EntityState.Detached)
                // The entity is not attached to the database. We return.
                return;
            PromotionCodeEntries.Remove(node);
        }

        /// <summary>
        /// Marks the DefaultComponentOrder to be removed from the underlying database.
        /// </summary>
        /// <param name="node">The DefaultComponentOrder to be removed.</param>
        public void RemoveDefaultComponentOrder(DefaultComponentOrder node)
        {
            if (node == null)
                return;
            if (!CanAccess(node, Security.SecurityAccessType.Write))
                return;
            // Let's see if the node is already attached to a database.
            if (Entry(node).State == EntityState.Detached)
                // The entity is not attached to the database. We return.
                return;
            DefaultComponentOrders.Remove(node);
        }
        #endregion

        /// <summary>
        /// Marks the Page to be removed from the underlying database
        /// </summary>
        /// <param name="node">The page to be removed.</param>
        public void RemovePage(Page node)
        {
            if (node == null)
                return;
            if (!CanAccess(node, Security.SecurityAccessType.Write))
                return;
            // Let's see if the node is already attached to a database.
            if (Entry(node).State == EntityState.Detached)
                // The entity is not attached to the database. We return.
                return;
            node.Logics.ForEach(l => RemoveLogic(l));
            node.Panels.ForEach(p => RemovePanel(p));
            Pages.Remove(node);
        }

        /// <summary>
        /// Marks the Panel to be removed fromt he underlying database.
        /// </summary>
        /// <param name="node">The panel to be removed.</param>
        public void RemovePanel(Panel node)
        {
            if (node == null)
                return;
            if (!CanAccess(node, Security.SecurityAccessType.Write))
                return;
            // Let's see if the node is already attached to a database.
            if (Entry(node).State == EntityState.Detached)
                // The entity is not attached to the database. We return.
                return;
            node.Components.ForEach(c => RemoveComponent(c));
            node.Logics.ForEach(l => RemoveLogic(l));
        }

        /// <summary>
        /// Marks the Audience to be removed from the underlying database.
        /// </summary>
        /// <param name="node"></param>
        public void RemoveAudience(Audience node)
        {
            if (node == null)
                return;
            if (!CanAccess(node, Security.SecurityAccessType.Write))
                return;
            // Let's see if the node is already attached to a database.
            if (Entry(node).State == EntityState.Detached)
                // The entity is not attached to the database. We return.
                return;
            Audiences.Remove(node);
        }

        #region Seating
        /// <summary>
        /// Marks the Seating to be removed fromt he underlying database.
        /// </summary>
        /// <param name="node">The Seating to be removed.</param>
        public void RemoveSeating(Seating node)
        {
            if (node == null)
                return;
            if (!CanAccess(node, Security.SecurityAccessType.Write))
                return;
            // Let's see if the node is already attached to a database.
            if (Entry(node).State == EntityState.Detached)
                // The entity is not attached to the database. We return.
                return;
            node.Seaters.ForEach(s => RemoveSeater(s));
            node.Styles.ForEach(s => RemoveSeatingStyle(s));
            Seatings.Remove(node);
        }

        /// <summary>
        /// Mark the SeatingStyle to be removed from the underlying database.
        /// </summary>
        /// <param name="node">The SeatingStyle to be removed.</param>
        public void RemoveSeatingStyle(SeatingStyle node)
        {
            if (node == null)
                return;
            if (!CanAccess(node, Security.SecurityAccessType.Write))
                return;
            // Let's see if the node is already attached to a database.
            if (Entry(node).State == EntityState.Detached)
                // The entity is not attached to the database. We return.
                return;
            SeatingStyles.Remove(node);
        }

        /// <summary>
        /// Marks the Seater to be removed from the underlying database.
        /// </summary>
        /// <param name="node">The Seater to be removed.</param>
        public void RemoveSeater(Seater node)
        {
            if (node == null)
                return;
            if (!CanAccess(node, Security.SecurityAccessType.Write))
                return;
            // Let's see if the node is already attached to a database.
            if (Entry(node).State == EntityState.Detached)
                // The entity is not attached to the database. We return.
                return;
            Seaters.Remove(node);
        }
        #endregion

        #region Component
        /// <summary>
        /// Marks the Component to be removed from the underlying database.
        /// </summary>
        /// <param name="node">The Component to be removed.</param>
        public void RemoveComponent(Component node)
        {
            if (node == null)
                return;
            if (!CanAccess(node, Security.SecurityAccessType.Write))
                return;
            // Let's see if the node is already attached to a database.
            if (Entry(node).State == EntityState.Detached)
                // The entity is not attached to the database. We return.
                return;
            node.RegistrantDatas.ForEach(r => RemoveRegistrantData(r));
            RemoveVariable(node.Variable);
            node.Styles.ForEach(s => RemoveComponentStyle(s));
            node.Logics.ForEach(l => RemoveLogic(l));
            node.Seaters.ForEach(s => RemoveSeater(s));
            if (node is IComponentItemParent)
                (node as IComponentItemParent).Children.ToList().ForEach(i => RemoveComponent(i as Component));
            Components.Remove(node);
        }

        /// <summary>
        /// Marks the Variable to be removed from the underlying database.
        /// </summary>
        /// <param name="node">The Variabel to be removed.</param>
        public void RemoveVariable(Variable node)
        {
            if (node == null)
                return;
            if (!CanAccess(node, Security.SecurityAccessType.Write))
                return;
            // Let's see if the node is already attached to a database.
            if (Entry(node).State == EntityState.Detached)
                // The entity is not attached to the database. We return.
                return;
            Variables.Remove(node);
        }

        /// <summary>
        /// Marks the ComponentStyle to be removed from the underlying database.
        /// </summary>
        /// <param name="node">The ComponentStyle to be removed.</param>
        public void RemoveComponentStyle(ComponentStyle node)
        {
            if (node == null)
                return;
            if (!CanAccess(node, Security.SecurityAccessType.Write))
                return;
            // Let's see if the node is already attached to a database.
            if (Entry(node).State == EntityState.Detached)
                // The entity is not attached to the database. We return.
                return;
            ComponentStyles.Remove(node);
        }
        #endregion

        /// <summary>
        /// Marks the tiny url to be removed from underlying the database.
        /// </summary>
        /// <param name="node"></param>
        public void RemoveTinyUrl(TinyUrl node)
        {
            if (node == null)
                return;
            if (!CanAccess(node, Security.SecurityAccessType.Write))
                return;
            // Let's see if the node is already attached to a database.
            if (Entry(node).State == EntityState.Detached)
                // The entity is not attached to the database. We return.
                return;
            TinyUrls.Remove(node);
        }
        #endregion

        #region Logic
        /// <summary>
        /// Marks the Logic to be removed from the underlying database.
        /// </summary>
        /// <param name="node">The logic to be removed.</param>
        public void RemoveLogic(Logic node)
        {
            if (node == null)
                return;
            if (!CanAccess(node, Security.SecurityAccessType.Write))
                return;
            // Let's see if the node is already attached to a database.
            if (Entry(node).State == EntityState.Detached)
                // The entity is not attached to the database. We return.
                return;
            node.LogicGroups.ForEach(l => RemoveLogicGroup(l));
            node.Commands.ForEach(c => RemoveLogicCommand(c));
            Logics.Remove(node);
        }

        /// <summary>
        /// Marks the LogicGroup to be removed from the underlying database.
        /// </summary>
        /// <param name="node">The LogicGroup to remove.</param>
        public void RemoveLogicGroup(LogicGroup node) {
                        if (node == null)
                return;
            if (!CanAccess(node, Security.SecurityAccessType.Write))
                return;
            // Let's see if the node is already attached to a database.
            if (Entry(node).State == EntityState.Detached)
                // The entity is not attached to the database. We return.
                return;
            node.LogicStatements.ForEach(l => RemoveLogicStatement(l));
            LogicGroups.Remove(node);
        }

        /// <summary>
        /// Marks the LogicStatement to be removed from the underlying database.
        /// </summary>
        /// <param name="node">The LogicStatement to remove.</param>
        public void RemoveLogicStatement(LogicStatement node)
        {
            if (node == null)
                return;
            if (!CanAccess(node, Security.SecurityAccessType.Write))
                return;
            // Let's see if the node is already attached to a database.
            if (Entry(node).State == EntityState.Detached)
                // The entity is not attached to the database. We return.
                return;
            LogicStatements.Remove(node);
        }

        /// <summary>
        /// Marks the LogicCommand for removal from the underlying database.
        /// </summary>
        /// <param name="node">The LogicCommand to remove.</param>
        public void RemoveLogicCommand(LogicCommand node)
        {
            if (node == null)
                return;
            if (!CanAccess(node, Security.SecurityAccessType.Write))
                return;
            // Let's see if the node is already attached to a database.
            if (Entry(node).State == EntityState.Detached)
                // The entity is not attached to the database. We return.
                return;
            LogicCommands.Remove(node);
        }
        #endregion

        #region Email
        public void RemoveRSHtmlEmail(RSHtmlEmail node)
        {
            if (node == null)
                return;
            if (!CanAccess(node, Security.SecurityAccessType.Write))
                return;
            // Let's see if the node is already attached to a database.
            if (Entry(node).State == EntityState.Detached)
                // The entity is not attached to the database. We return.
                return;
            node.Logics.ForEach(l => RemoveLogic(l));
            RSHtmlEmails.Remove(node);
        }

        /// <summary>
        /// Marks the RSEmail to be removed from the underlying database.
        /// </summary>
        /// <param name="node">The RSEmail to be removed.</param>
        public void RemoveRSEmail(RSEmail node)
        {
            if (node == null)
                return;
            if (!CanAccess(node, Security.SecurityAccessType.Write))
                return;
            // Let's see if the node is already attached to a database.
            if (Entry(node).State == EntityState.Detached)
                // The entity is not attached to the database. We return.
                return;
            node.Logics.ForEach(l => RemoveLogic(l));
            node.EmailAreas.ForEach(e => RemoveEmailArea(e));
            node.Variables.ForEach(v => RemoveEmailVariable(v));
            RSEmails.Remove(node);
        }

        /// <summary>
        /// Marks the EmailVariable to be removed from the underlying database.
        /// </summary>
        /// <param name="node">The EmailVariable to be removed.</param>
        public void RemoveEmailVariable(EmailVariable node)
        {
            if (node == null)
                return;
            if (!CanAccess(node, Security.SecurityAccessType.Write))
                return;
            // Let's see if the node is already attached to a database.
            if (Entry(node).State == EntityState.Detached)
                // The entity is not attached to the database. We return.
                return;
            EmailVariables.Remove(node);
        }

        /// <summary>
        /// Marks the EmailArea to be removed from the underlying database.
        /// </summary>
        /// <param name="node">The EmailArea to remove.</param>
        public void RemoveEmailArea(EmailArea node)
        {
            if (node == null)
                return;
            if (!CanAccess(node, Security.SecurityAccessType.Write))
                return;
            // Let's see if the node is already attached to a database.
            if (Entry(node).State == EntityState.Detached)
                // The entity is not attached to the database. We return.
                return;
            node.Variables.ForEach(v => RemoveEmailAreaVariable(v));
            EmailAreas.Remove(node);
        }

        /// <summary>
        /// Marks the EmailAreaVariable to be removed from the underlying database.
        /// </summary>
        /// <param name="node">The EmailAreaVariable to be removed.</param>
        public void RemoveEmailAreaVariable(EmailAreaVariable node)
        {
            if (node == null)
                return;
            if (!CanAccess(node, Security.SecurityAccessType.Write))
                return;
            // Let's see if the node is already attached to a database.
            if (Entry(node).State == EntityState.Detached)
                // The entity is not attached to the database. We return.
                return;
            EmailAreaVariables.Remove(node);
        }
        #endregion

        #endregion

        /// <summary>
        /// Checks to see if the database object can be accessed.
        /// </summary>
        /// <param name="node">The object to check for permissions on.</param>
        /// <param name="action">The action taken on the object.</param>
        /// <param name="ignoreException">Set to true if you don't want an error thrown if permission check fails.</param>
        /// <returns>True if database object can be accessed, false otherwise.</returns>
        /// <exception cref="InsufficientPermissionsException">Thrown if permission check fails and <paramref name="ignoreException"/> is not set to true.</exception>
        /// <exception cref="NotImplementedException">Thrown if the node is <code>IRequirePermissions</code> but the permissions are not checked.</exception>
        public bool CanAccess(object node, SecurityAccessType action, bool ignoreException = false)
        {
            if (!(node is IRequirePermissions || node is IPermissionHolder))
                // The node does not require permissions, we return true.
                return true;
            if (!SecuritySettings.UseSecurity)
                return true;
            if (SecuritySettings.GlobalPermissions)
                return true;
            IPermissionHolder permissionHolder = null;
            if (node is IPermissionHolder)
                permissionHolder = node as IPermissionHolder;
            else
                permissionHolder = (node as IRequirePermissions).GetPermissionHolder();
            if (permissionHolder == null)
                // The item is of type IRequirePermissions, but the permissions are not checked. We throw an exception.
                throw new NotImplementedException("The permissions where not check. The permissions for IRequirePermissions for object of type " + GetObjectType(node) + " need to be implemented.");
            return _CanAccess(permissionHolder.UId, permissionHolder.CompanyKey, action, ignoreException);
        }

        /// <summary>
        /// Checks to see if the database object can be accessed.
        /// </summary>
        /// <param name="id">The id of the database object to check for permissions on.</param>
        /// <param name="companyId">The id of the company owning the database object.</param>
        /// <param name="action">The action taken on the object.</param>
        /// <param name="ignoreException">Set to true if you don't want an error thrown if permission check fails.</param>
        /// <returns>True if database object can be accessed, false otherwise.</returns>
        /// <exception cref="InsufficientPermissionsException">Thrown if permission check fails and <paramref name="ignoreException"/> is not set to true.</exception>
        public bool _CanAccess(Guid id, Guid companyId, SecurityAccessType action, bool ignoreException)
        {
            if (SecuritySettings.GlobalPermissions)
                return true;
            if (SecuritySettings.CompanyAdministrator && SecuritySettings.AppUser.WorkingCompanyKey == companyId)
                return true;
            IEnumerable<PermissionSet> permissions;
            using (var context = new EFDbContext())
                // We query the databse for the permissions sets.
                permissions = context.PermissionsSets.Where(p => p.Target == id).ToList();
            if (permissions.Count() == 0)
                // No permissions so we return true;
                return true;
            // First we check anonymous permissions.
            if (_RunPermission(permissions.FirstOrDefault(p => p.Owner == Guid.Empty), action))
                return true;
            // Anonymous permissions failed. Now we check company permissions.
            if (_RunPermission(permissions.FirstOrDefault(p => p.Owner == companyId), action))
                return true;
            // Company permissions failed. Now we check custom group permissions.
            foreach (var group in SecuritySettings.AppUser.CustomGroups)
            {
                if (_RunPermission(permissions.FirstOrDefault(p => p.Owner == group.UId), action))
                    return true;
                // Permissions for this group failed. Lets move on to the next one.
            }
            // All permissions failed. We either now throw an exception or return false depending on the ignoreException flag.
            if (!ignoreException)
                throw new InsufficientPermissionsException();
            return false;
        }

        /// <summary>
        /// Checks access on the permissions specified with the desired action.
        /// </summary>
        /// <param name="permission">The permission set.</param>
        /// <param name="action">The action taken.</param>
        /// <returns>True if action is allowed, false otherwise.</returns>
        protected bool _RunPermission(PermissionSet permission, SecurityAccessType action)
        {
            if (permission == null)
                return false;
            switch (action)
            {
                case SecurityAccessType.Read:
                    if (permission.Read)
                        return true;
                    break;
                case SecurityAccessType.Write:
                    if (permission.Write)
                        return true;
                    break;
                case SecurityAccessType.Execute:
                    if (permission.Execute)
                        return true;
                    break;
            }
            return false;
        }
    }
}
