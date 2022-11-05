using Microsoft.AspNet.Identity.EntityFramework;
using RSToolKit.Domain.Entities;
using RSToolKit.Domain.Entities.Clients;
using RSToolKit.Domain.Entities;
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

namespace RSToolKit.Domain.Data
{
    public class RegStepContext : IdentityDbContext<User>, IDataContext
    {
        private bool _locked = false;
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

        public RegStepContext(string connString)
            : base(connString)
        {
            _locked = false;
        }

        public RegStepContext()
            : base()
        {
            _locked = false;
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
        /// Saves the data to the database as long as no lock is on the context.
        /// </summary>
        /// <returns>The number of rows affected.</returns>
        public override int SaveChanges()
        {
            if (_locked)
                throw new Exception("The context is locked to save functions.");
            _locked = true;
            var affected = 0;
            try
            {
                affected = base.SaveChanges();
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
            _locked = false;
            return affected;
        }

        public static EFDbContext Create()
        {
            var context = new EFDbContext();
            return context;
        }

    }
}
