using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RSToolKit.Domain.Entities;
using RSToolKit.Domain.Entities.Clients;
using RSToolKit.Domain.Entities.Components;
using RSToolKit.Domain.Entities;
using RSToolKit.Domain.Entities.Email;
using RSToolKit.Domain.Entities.MerchantAccount;
using RSToolKit.Domain.Security;
using RSToolKit.Domain.Errors;
using System.Data.Entity;
using System.Linq.Expressions;

namespace RSToolKit.Domain.Data
{
    public interface IDataContext
        : IDbContext, IDisposable
    {
        DbSet<Component> Components { get; set; }
        DbSet<CustomGroup> CustomGroups { get; set; }
        DbSet<Company> Companies { get; set; }
        DbSet<SmtpServer> SmtpServers { get; set; }
        DbSet<EmailEvent> EmailEvents { get; set; }
        DbSet<Stylesheet> Stylesheets { get; set; }
        DbSet<CSS> CSS { get; set; }
        DbSet<PriceGroup> PriceGroups { get; set; }
        DbSet<Prices> Prices { get; set; }
        DbSet<Price> Price { get; set; }
        DbSet<Audience> Audiences { get; set; }
        DbSet<RSEmail> RSEmails { get; set; }
        DbSet<EmailTemplate> EmailTemplates { get; set; }
        DbSet<EmailArea> EmailAreas { get; set; }
        DbSet<EmailTemplateArea> EmailTemplateAreas { get; set; }
        DbSet<SavedList> SavedLists { get; set; }
        DbSet<Contact> Contacts { get; set; }
        DbSet<ContactData> ContactData { get; set; }
        DbSet<CustomText> CustomTexts { get; set; }
        DbSet<LogicBlock> LogicBlocks { get; set; }
        DbSet<Logic> Logics { get; set; }
        DbSet<Registrant> Registrants { get; set; }
        DbSet<RegistrantData> RegistrantData { get; set; }
        DbSet<OldRegistrant> OldRegistrants { get; set; }
        DbSet<OldRegistrantData> OldRegistrantData { get; set; }
        DbSet<Form> Forms { get; set; }
        DbSet<Seating> Seatings { get; set; }
        DbSet<Tag> Tags { get; set; }
        DbSet<EmailCampaign> EmailCampaigns { get; set; }
        DbSet<Variable> Variables { get; set; }
        DbSet<Folder> Folders { get; set; }
        DbSet<Pointer> Pointers { get; set; }
        DbSet<CreditCard> CreditCards { get; set; }
        DbSet<MerchantAccountInfo> MerchantAccountInfo { get; set; }
        DbSet<TransactionRequest> TransactionRequests { get; set; }
        DbSet<TransactionDetail> TransactionDetails { get; set; }
        DbSet<EmailVariable> EmailVariables { get; set; }
        DbSet<EmailTemplateVariable> EmailTemplateVariables { get; set; }
        DbSet<TemplateEmailAreaVariable> TemplateEmailAreaVariables { get; set; }
        DbSet<EmailAreaVariable> EmailAreaVariables { get; set; }
        DbSet<DefaultFormStyle> DefaultFormStyles { get; set; }
        DbSet<FormStyle> FormStyles { get; set; }
        DbSet<NodeType> Types { get; set; }
        DbSet<FormTemplate> FormTemplates { get; set; }
        DbSet<ComponentStyle> ComponentStyles { get; set; }
        DbSet<PriceStyle> PriceStyles { get; set; }
        DbSet<SeatingStyle> SeatingStyles { get; set; }
        DbSet<DefaultComponentOrder> DefaultComponentOrders { get; set; }
        DbSet<Page> Pages { get; set; }
        DbSet<Panel> Panels { get; set; }
        DbSet<Seater> Seaters { get; set; }
        DbSet<LogicGroup> LogicGroups { get; set; }
        DbSet<LogicStatement> LogicStatements { get; set; }
        DbSet<LogicCommand> LogicCommands { get; set; }
        DbSet<PromotionCode> PromotionCodes { get; set; }
        DbSet<PromotionCodeEntry> PromotionCodeEntries { get; set; }
        DbSet<RegistrantFile> RegistrantFiles { get; set; }
        DbSet<EmailSend> EmailSends { get; set; }
        DbSet<SingleFormReport> SingleFormReports { get; set; }
        DbSet<QueryFilter> QueryFilters { get; set; }
        DbSet<Adjustment> Adjustments { get; set; }
        DbSet<Unsubscribe> Unsubscribes { get; set; }
        DbSet<AvailableRoles> AvailableRoles { get; set; }
        DbSet<ContactHeader> ContactHeaders { get; set; }
        DbSet<ContactReport> ContactReports { get; set; }
        DbSet<PermissionSet> PermissionsSets { get; set; }
        DbSet<AdvancedInventoryReport> AdvancedInventoryReports { get; set; }
        DbSet<RSHtmlEmail> RSHtmlEmails { get; set; }
    }
}
