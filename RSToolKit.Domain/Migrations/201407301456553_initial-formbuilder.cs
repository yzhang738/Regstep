namespace RSToolKit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class initialformbuilder : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Audiences",
                c => new
                    {
                        UId = c.Guid(nullable: false),
                        SortingId = c.Long(nullable: false, identity: true),
                        Name = c.String(maxLength: 250),
                        DateCreated = c.DateTimeOffset(nullable: false, precision: 7),
                        DateModified = c.DateTimeOffset(nullable: false, precision: 7),
                        Owner = c.Guid(nullable: false),
                        Group = c.Guid(nullable: false),
                        Permission = c.String(maxLength: 3),
                        ModificationToken = c.Guid(nullable: false),
                        ModifiedBy = c.Guid(nullable: false),
                        FormKey = c.Guid(nullable: false),
                        Label = c.String(maxLength: 100),
                        Order = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.UId, clustered: false)
                .ForeignKey("dbo.Forms", t => t.FormKey)
                .Index(t => t.SortingId, clustered: true, unique: true)
                .Index(t => t.FormKey);
            
            CreateTable(
                "dbo.ComponentBases",
                c => new
                    {
                        UId = c.Guid(nullable: false),
                        SortingId = c.Long(nullable: false, identity: true),
                        Name = c.String(maxLength: 250),
                        Permission = c.String(maxLength: 3),
                        DateCreated = c.DateTimeOffset(nullable: false, precision: 7),
                        DateModified = c.DateTimeOffset(nullable: false, precision: 7),
                        Owner = c.Guid(nullable: false),
                        Group = c.Guid(nullable: false),
                        ModificationToken = c.Guid(nullable: false),
                        ModifiedBy = c.Guid(nullable: false),
                        SeatingKey = c.Guid(),
                        PanelKey = c.Guid(),
                        Description = c.String(maxLength: 500),
                        AgendaStart = c.DateTimeOffset(nullable: false, precision: 7),
                        AgendaEnd = c.DateTimeOffset(nullable: false, precision: 7),
                        Open = c.DateTimeOffset(nullable: false, precision: 7),
                        Close = c.DateTimeOffset(nullable: false, precision: 7),
                        RSVP = c.Int(nullable: false),
                        AdminOnly = c.Boolean(nullable: false),
                        Display = c.Boolean(nullable: false),
                        Enabled = c.Boolean(nullable: false),
                        AltText = c.String(),
                        LabelText = c.String(),
                        Row = c.Int(nullable: false),
                        Order = c.Int(nullable: false),
                        DisplayAgendaDate = c.Boolean(nullable: false),
                        Required = c.Boolean(nullable: false),
                        Locked = c.Boolean(nullable: false),
                        DisplayOrder = c.String(),
                        Style = c.Int(),
                        TimeExclusion = c.Boolean(),
                        DialogText = c.String(maxLength: 1000),
                        ItemsPerRow = c.Int(),
                        AgendaDisplay = c.Int(),
                        CheckboxGroupKey = c.Guid(),
                        DropdownGroupKey = c.Guid(),
                        Html = c.String(),
                        RegexPattern = c.Int(),
                        ComfirmField = c.Boolean(),
                        Type = c.Int(),
                        RegexString = c.String(maxLength: 500),
                        RegexHumanString = c.String(maxLength: 500),
                        RegexErrorString = c.String(maxLength: 250),
                        Length = c.Int(),
                        Height = c.Int(),
                        MinDate = c.DateTimeOffset(precision: 7),
                        MaxDate = c.DateTimeOffset(precision: 7),
                        FileType = c.String(),
                        ItemsPerRow1 = c.Int(),
                        RadioGroupKey = c.Guid(),
                        Discriminator = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => t.UId, clustered: false)
                .ForeignKey("dbo.Panels", t => t.PanelKey)
                .ForeignKey("dbo.Seatings", t => t.SeatingKey)
                .ForeignKey("dbo.ComponentBases", t => t.CheckboxGroupKey)
                .ForeignKey("dbo.ComponentBases", t => t.DropdownGroupKey)
                .ForeignKey("dbo.ComponentBases", t => t.RadioGroupKey)
                .Index(t => t.SortingId, clustered: true, unique: true)
                .Index(t => t.SeatingKey)
                .Index(t => t.PanelKey)
                .Index(t => t.CheckboxGroupKey)
                .Index(t => t.DropdownGroupKey)
                .Index(t => t.RadioGroupKey);
            
            CreateTable(
                "dbo.Logics",
                c => new
                    {
                        UId = c.Guid(nullable: false),
                        SortingId = c.Long(nullable: false, identity: true),
                        Name = c.String(maxLength: 250),
                        Permission = c.String(maxLength: 3),
                        DateCreated = c.DateTimeOffset(nullable: false, precision: 7),
                        DateModified = c.DateTimeOffset(nullable: false, precision: 7),
                        ModificationToken = c.Guid(nullable: false),
                        ModifiedBy = c.Guid(nullable: false),
                        Owner = c.Guid(nullable: false),
                        Group = c.Guid(nullable: false),
                        Description = c.String(maxLength: 1000),
                        Incoming = c.Boolean(nullable: false),
                        Order = c.Int(nullable: false),
                        PageKey = c.Guid(),
                        PanelKey = c.Guid(),
                        ComponentKey = c.Guid(),
                        LogicBlockKey = c.Guid(),
                    })
                .PrimaryKey(t => t.UId, clustered: false)
                .ForeignKey("dbo.LogicBlocks", t => t.LogicBlockKey)
                .ForeignKey("dbo.Pages", t => t.PageKey)
                .ForeignKey("dbo.Panels", t => t.PanelKey)
                .ForeignKey("dbo.ComponentBases", t => t.ComponentKey)
                .Index(t => t.SortingId, clustered: true, unique: true)
                .Index(t => t.PageKey)
                .Index(t => t.PanelKey)
                .Index(t => t.ComponentKey)
                .Index(t => t.LogicBlockKey);
            
            CreateTable(
                "dbo.LogicCommands",
                c => new
                    {
                        UId = c.Guid(nullable: false),
                        SortingId = c.Long(nullable: false, identity: true),
                        LogicKey = c.Guid(nullable: false),
                        FormKey = c.Guid(),
                        Command = c.Int(nullable: false),
                        CommandType = c.Int(nullable: false),
                        Params = c.String(),
                        Order = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.UId, clustered: false)
                .ForeignKey("dbo.Forms", t => t.FormKey)
                .ForeignKey("dbo.Logics", t => t.LogicKey)
                .Index(t => t.SortingId, clustered: true, unique: true)
                .Index(t => t.LogicKey)
                .Index(t => t.FormKey);
            
            CreateTable(
                "dbo.Forms",
                c => new
                    {
                        UId = c.Guid(nullable: false),
                        SortingId = c.Long(nullable: false, identity: true),
                        Name = c.String(maxLength: 250),
                        DateCreated = c.DateTimeOffset(nullable: false, precision: 7),
                        DateModified = c.DateTimeOffset(nullable: false, precision: 7),
                        Owner = c.Guid(nullable: false),
                        Group = c.Guid(nullable: false),
                        Permission = c.String(maxLength: 3),
                        ModificationToken = c.Guid(nullable: false),
                        ModifiedBy = c.Guid(nullable: false),
                        MerchantAccountKey = c.Guid(),
                        FormTemplateKey = c.Guid(),
                        TypeKey = c.Guid(),
                        CompanyKey = c.Guid(nullable: false),
                        InvitationListKey = c.Guid(),
                        Description = c.String(maxLength: 1000),
                        Answer = c.String(maxLength: 250),
                        LastCode = c.String(),
                        Question = c.String(maxLength: 250),
                        Style = c.String(),
                        Header = c.String(),
                        Footer = c.String(),
                        CultureString = c.String(),
                        Status = c.Int(nullable: false),
                        Currency = c.Int(nullable: false),
                        AccessType = c.Int(nullable: false),
                        BillingOption = c.Int(nullable: false),
                        Approval = c.Boolean(nullable: false),
                        Editable = c.Boolean(nullable: false),
                        Cancelable = c.Boolean(nullable: false),
                        Year = c.Int(nullable: false),
                        Price = c.Decimal(precision: 18, scale: 2),
                        Open = c.DateTimeOffset(nullable: false, precision: 7),
                        Close = c.DateTimeOffset(nullable: false, precision: 7),
                    })
                .PrimaryKey(t => t.UId, clustered: false)
                .ForeignKey("dbo.NodeTypes", t => t.TypeKey)
                .ForeignKey("dbo.Companies", t => t.CompanyKey)
                .ForeignKey("dbo.SavedLists", t => t.InvitationListKey)
                .ForeignKey("dbo.MerchantAccountInfo", t => t.MerchantAccountKey)
                .ForeignKey("dbo.FormTemplates", t => t.FormTemplateKey)
                .Index(t => t.SortingId, clustered: true, unique: true)
                .Index(t => t.MerchantAccountKey)
                .Index(t => t.FormTemplateKey)
                .Index(t => t.TypeKey)
                .Index(t => t.CompanyKey)
                .Index(t => t.InvitationListKey);

            CreateTable(
                "dbo.Companies",
                c => new
                    {
                        UId = c.Guid(nullable: false),
                        SortingId = c.Long(nullable: false, identity: true),
                        Name = c.String(maxLength: 250),
                        DateCreated = c.DateTimeOffset(nullable: false, precision: 7),
                        DateModified = c.DateTimeOffset(nullable: false, precision: 7),
                        Owner = c.Guid(nullable: false),
                        Group = c.Guid(nullable: false),
                        Permission = c.String(maxLength: 3),
                        ModificationToken = c.Guid(nullable: false),
                        ModifiedBy = c.Guid(nullable: false),
                        Database = c.String(maxLength: 50),
                        Description = c.String(),
                        ContactLimit = c.Long(nullable: false),
                        ParentCompany = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.UId, clustered: false)
                .Index(t => t.SortingId, clustered: true, unique: true);
            
            CreateTable(
                "dbo.EmailCampaigns",
                c => new
                    {
                        UId = c.Guid(nullable: false),
                        SortingId = c.Long(nullable: false, identity: true),
                        Name = c.String(maxLength: 250),
                        Permission = c.String(maxLength: 3),
                        DateCreated = c.DateTimeOffset(nullable: false, precision: 7),
                        DateModified = c.DateTimeOffset(nullable: false, precision: 7),
                        Owner = c.Guid(nullable: false),
                        Group = c.Guid(nullable: false),
                        ModificationToken = c.Guid(nullable: false),
                        ModifiedBy = c.Guid(nullable: false),
                        CompanyKey = c.Guid(nullable: false),
                        TypeKey = c.Guid(),
                        Description = c.String(),
                    })
                .PrimaryKey(t => t.UId, clustered: false)
                .ForeignKey("dbo.Companies", t => t.CompanyKey)
                .ForeignKey("dbo.NodeTypes", t => t.TypeKey)
                .Index(t => t.SortingId, clustered: true, unique: true)
                .Index(t => t.CompanyKey)
                .Index(t => t.TypeKey);
            
            CreateTable(
                "dbo.CustomTexts",
                c => new
                    {
                        UId = c.Guid(nullable: false),
                        SortingId = c.Long(nullable: false, identity: true),
                        Name = c.String(maxLength: 250),
                        Permission = c.String(maxLength: 3),
                        DateCreated = c.DateTimeOffset(nullable: false, precision: 7),
                        DateModified = c.DateTimeOffset(nullable: false, precision: 7),
                        Owner = c.Guid(nullable: false),
                        Group = c.Guid(nullable: false),
                        ModificationToken = c.Guid(nullable: false),
                        ModifiedBy = c.Guid(nullable: false),
                        FormKey = c.Guid(),
                        EmailCampaignKey = c.Guid(),
                        Variable = c.String(maxLength: 100),
                        Text = c.String(),
                    })
                .PrimaryKey(t => t.UId, clustered: false)
                .ForeignKey("dbo.EmailCampaigns", t => t.EmailCampaignKey)
                .ForeignKey("dbo.Forms", t => t.FormKey)
                .Index(t => t.SortingId, clustered: true, unique: true)
                .Index(t => t.FormKey)
                .Index(t => t.EmailCampaignKey);
            
            CreateTable(
                "dbo.RSEmails",
                c => new
                    {
                        UId = c.Guid(nullable: false),
                        SortingId = c.Long(nullable: false, identity: true),
                        Name = c.String(maxLength: 250),
                        Permission = c.String(maxLength: 3),
                        DateCreated = c.DateTimeOffset(nullable: false, precision: 7),
                        DateModified = c.DateTimeOffset(nullable: false, precision: 7),
                        Owner = c.Guid(nullable: false),
                        Group = c.Guid(nullable: false),
                        ModificationToken = c.Guid(nullable: false),
                        ModifiedBy = c.Guid(nullable: false),
                        EmailTemplateKey = c.Guid(),
                        FormKey = c.Guid(),
                        EmailCampaignKey = c.Guid(),
                        EmailListKey = c.Guid(),
                        Subject = c.String(maxLength: 500),
                        Description = c.String(),
                        From = c.String(),
                        CC = c.String(),
                        BCC = c.String(),
                        EmailType = c.Int(nullable: false),
                        SendTime = c.DateTimeOffset(precision: 7),
                        IntervalTicks = c.Long(nullable: false),
                        MaxSends = c.Int(nullable: false),
                        RepeatSending = c.Boolean(nullable: false),
                        To = c.String(),
                        Areas = c.String(),
                    })
                .PrimaryKey(t => t.UId, clustered: false)
                .ForeignKey("dbo.EmailCampaigns", t => t.EmailCampaignKey)
                .ForeignKey("dbo.EmailLists", t => t.EmailListKey)
                .ForeignKey("dbo.Forms", t => t.FormKey)
                .ForeignKey("dbo.EmailTemplates", t => t.EmailTemplateKey)
                .Index(t => t.SortingId, clustered: true, unique: true)
                .Index(t => t.EmailTemplateKey)
                .Index(t => t.FormKey)
                .Index(t => t.EmailCampaignKey)
                .Index(t => t.EmailListKey);
            
            CreateTable(
                "dbo.EmailAreas",
                c => new
                    {
                        SortingId = c.Long(nullable: false, identity: true),
                        Company = c.Guid(nullable: false),
                        UId = c.Guid(nullable: false),
                        Name = c.String(maxLength: 250),
                        DateCreated = c.DateTimeOffset(nullable: false, precision: 7),
                        DateModified = c.DateTimeOffset(nullable: false, precision: 7),
                        Owner = c.Guid(nullable: false),
                        Group = c.Guid(nullable: false),
                        Permission = c.String(maxLength: 3),
                        ModificationToken = c.Guid(nullable: false),
                        ModifiedBy = c.Guid(nullable: false),
                        Type = c.String(),
                        Html = c.String(),
                        Order = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.SortingId)
                .ForeignKey("dbo.RSEmails", t => t.UId)
                .Index(t => t.UId);
            
            CreateTable(
                "dbo.EmailAreaVariables",
                c => new
                    {
                        SortingId = c.Long(nullable: false, identity: true),
                        Variable = c.String(maxLength: 100),
                        Name = c.String(maxLength: 150),
                        Description = c.String(maxLength: 250),
                        Type = c.String(maxLength: 25),
                        EmailAreaSortingId = c.Long(nullable: false),
                        Value = c.String(),
                    })
                .PrimaryKey(t => t.SortingId)
                .ForeignKey("dbo.EmailAreas", t => t.EmailAreaSortingId)
                .Index(t => t.EmailAreaSortingId);

            CreateTable(
                "dbo.EmailLists",
                c => new
                    {
                        UId = c.Guid(nullable: false),
                        CompanyUId = c.Guid(nullable: false),
                        SortingId = c.Long(nullable: false, identity: true),
                        Name = c.String(maxLength: 250),
                        DateCreated = c.DateTimeOffset(nullable: false, precision: 7),
                        DateModified = c.DateTimeOffset(nullable: false, precision: 7),
                        Owner = c.Guid(nullable: false),
                        Group = c.Guid(nullable: false),
                        Permission = c.String(maxLength: 3),
                        ModificationToken = c.Guid(nullable: false),
                        ModifiedBy = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.UId, clustered: false)
                .Index(t => t.SortingId, clustered: true, unique: true);
            
            CreateTable(
                "dbo.Contacts",
                c => new
                    {
                        UId = c.Guid(nullable: false),
                        Company = c.Guid(nullable: false),
                        SortingId = c.Long(nullable: false, identity: true),
                        Name = c.String(maxLength: 250),
                        DateCreated = c.DateTimeOffset(nullable: false, precision: 7),
                        DateModified = c.DateTimeOffset(nullable: false, precision: 7),
                        Owner = c.Guid(nullable: false),
                        Group = c.Guid(nullable: false),
                        Permission = c.String(maxLength: 3),
                        ModificationToken = c.Guid(nullable: false),
                        ModifiedBy = c.Guid(nullable: false),
                        Email = c.String(),
                        Description = c.String(),
                        SavedList_UId = c.Guid(),
                    })
                .PrimaryKey(t => t.UId, clustered: false)
                .ForeignKey("dbo.SavedLists", t => t.SavedList_UId)
                .Index(t => t.SortingId, clustered: true, unique: true)
                .Index(t => t.SavedList_UId);
            
            CreateTable(
                "dbo.ContactData",
                c => new
                    {
                        SortingId = c.Long(nullable: false, identity: true),
                        UId = c.Guid(nullable: false),
                        Value = c.String(),
                        Header = c.String(),
                        CompanyUId = c.Guid(nullable: false),
                        EmailListUId = c.Guid(),
                        Descriminator = c.String(nullable: false, maxLength: 250),
                    })
                .PrimaryKey(t => t.SortingId)
                .ForeignKey("dbo.Contacts", t => t.UId)
                .Index(t => t.UId);
            
            CreateTable(
                "dbo.EmailTemplates",
                c => new
                    {
                        UId = c.Guid(nullable: false),
                        Description = c.String(),
                        ParentUId = c.Guid(nullable: false),
                        SortingId = c.Long(nullable: false, identity: true),
                        Name = c.String(maxLength: 250),
                        DateCreated = c.DateTimeOffset(nullable: false, precision: 7),
                        DateModified = c.DateTimeOffset(nullable: false, precision: 7),
                        Owner = c.Guid(nullable: false),
                        Group = c.Guid(nullable: false),
                        Permission = c.String(maxLength: 3),
                        ModificationToken = c.Guid(nullable: false),
                        ModifiedBy = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.UId, clustered: false)
                .Index(t => t.SortingId, clustered: true, unique: true);
            
            CreateTable(
                "dbo.EmailTemplateAreas",
                c => new
                    {
                        SortingId = c.Long(nullable: false, identity: true),
                        UId = c.Guid(nullable: false),
                        Name = c.String(maxLength: 250),
                        DateCreated = c.DateTimeOffset(nullable: false, precision: 7),
                        DateModified = c.DateTimeOffset(nullable: false, precision: 7),
                        Owner = c.Guid(nullable: false),
                        Group = c.Guid(nullable: false),
                        Permission = c.String(maxLength: 3),
                        ModificationToken = c.Guid(nullable: false),
                        ModifiedBy = c.Guid(nullable: false),
                        Type = c.String(),
                        Html = c.String(),
                        Default = c.String(),
                    })
                .PrimaryKey(t => t.SortingId)
                .ForeignKey("dbo.EmailTemplates", t => t.UId)
                .Index(t => t.UId);
            
            CreateTable(
                "dbo.TemplateEmailAreaVariables",
                c => new
                    {
                        SortingId = c.Long(nullable: false, identity: true),
                        Variable = c.String(maxLength: 100),
                        Name = c.String(maxLength: 150),
                        Description = c.String(maxLength: 250),
                        Type = c.String(maxLength: 25),
                        EmailTemplateAreaSortingId = c.Long(nullable: false),
                        Value = c.String(),
                    })
                .PrimaryKey(t => t.SortingId)
                .ForeignKey("dbo.EmailTemplateAreas", t => t.EmailTemplateAreaSortingId)
                .Index(t => t.EmailTemplateAreaSortingId);
            
            CreateTable(
                "dbo.EmailTemplateVariables",
                c => new
                    {
                        SortingId = c.Long(nullable: false, identity: true),
                        UId = c.Guid(nullable: false),
                        Variable = c.String(maxLength: 100),
                        Name = c.String(maxLength: 150),
                        Description = c.String(maxLength: 250),
                        Type = c.String(maxLength: 25),
                        Value = c.String(),
                    })
                .PrimaryKey(t => t.SortingId)
                .ForeignKey("dbo.EmailTemplates", t => t.UId)
                .Index(t => t.UId);
            
            CreateTable(
                "dbo.EmailVariables",
                c => new
                    {
                        SortingId = c.Long(nullable: false, identity: true),
                        UId = c.Guid(nullable: false),
                        Variable = c.String(maxLength: 100),
                        Name = c.String(maxLength: 150),
                        Description = c.String(maxLength: 250),
                        Type = c.String(),
                        Value = c.String(),
                    })
                .PrimaryKey(t => t.SortingId)
                .ForeignKey("dbo.RSEmails", t => t.UId)
                .Index(t => t.UId);
            
            CreateTable(
                "dbo.Tags",
                c => new
                    {
                        UId = c.Guid(nullable: false),
                        SortingId = c.Long(nullable: false, identity: true),
                        Name = c.String(maxLength: 250),
                        DateCreated = c.DateTimeOffset(nullable: false, precision: 7),
                        DateModified = c.DateTimeOffset(nullable: false, precision: 7),
                        Owner = c.Guid(nullable: false),
                        Group = c.Guid(nullable: false),
                        Permission = c.String(maxLength: 3),
                        ModificationToken = c.Guid(nullable: false),
                        ModifiedBy = c.Guid(nullable: false),
                        CompanyUId = c.Guid(nullable: false),
                        Description = c.String(maxLength: 1000),
                        CompanyKey = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.UId, clustered: false)
                .ForeignKey("dbo.Companies", t => t.CompanyKey)
                .Index(t => t.SortingId, clustered: true, unique: true)
                .Index(t => t.CompanyKey);
            
            CreateTable(
                "dbo.NodeTypes",
                c => new
                    {
                        UId = c.Guid(nullable: false),
                        SortingId = c.Long(nullable: false, identity: true),
                        Name = c.String(maxLength: 250),
                        Permission = c.String(maxLength: 3),
                        DateCreated = c.DateTimeOffset(nullable: false, precision: 7),
                        DateModified = c.DateTimeOffset(nullable: false, precision: 7),
                        Owner = c.Guid(nullable: false),
                        Group = c.Guid(nullable: false),
                        ModificationToken = c.Guid(nullable: false),
                        ModifiedBy = c.Guid(nullable: false),
                        CompanyKey = c.Guid(nullable: false),
                        Description = c.String(maxLength: 1000),
                    })
                .PrimaryKey(t => t.UId, clustered: false)
                .ForeignKey("dbo.Companies", t => t.CompanyKey)
                .Index(t => t.SortingId, clustered: true, unique: true)
                .Index(t => t.CompanyKey);
            
            CreateTable(
                "dbo.Folders",
                c => new
                    {
                        UId = c.Guid(nullable: false),
                        SortingId = c.Long(nullable: false, identity: true),
                        Name = c.String(maxLength: 250),
                        Permission = c.String(maxLength: 3),
                        DateCreated = c.DateTimeOffset(nullable: false, precision: 7),
                        DateModified = c.DateTimeOffset(nullable: false, precision: 7),
                        Owner = c.Guid(nullable: false),
                        Group = c.Guid(nullable: false),
                        ModificationToken = c.Guid(nullable: false),
                        ModifiedBy = c.Guid(nullable: false),
                        CompanyKey = c.Guid(nullable: false),
                        ParentKey = c.Guid(),
                    })
                .PrimaryKey(t => t.UId, clustered: false)
                .ForeignKey("dbo.Folders", t => t.ParentKey)
                .ForeignKey("dbo.Companies", t => t.CompanyKey)
                .Index(t => t.SortingId, clustered: true, unique: true)
                .Index(t => t.CompanyKey)
                .Index(t => t.ParentKey);
            
            CreateTable(
                "dbo.Pointers",
                c => new
                    {
                        SortingId = c.Long(nullable: false, identity: true),
                        Descriminator = c.String(maxLength: 250),
                        FolderKey = c.Guid(),
                        Target = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.SortingId)
                .ForeignKey("dbo.Folders", t => t.FolderKey)
                .Index(t => t.FolderKey);
            
            CreateTable(
                "dbo.DefaultFormStyles",
                c => new
                    {
                        UId = c.Guid(nullable: false),
                        SortingId = c.Long(nullable: false, identity: true),
                        Name = c.String(maxLength: 250),
                        Permission = c.String(maxLength: 3),
                        DateCreated = c.DateTimeOffset(nullable: false, precision: 7),
                        DateModified = c.DateTimeOffset(nullable: false, precision: 7),
                        Owner = c.Guid(nullable: false),
                        Group = c.Guid(nullable: false),
                        ModificationToken = c.Guid(nullable: false),
                        ModifiedBy = c.Guid(nullable: false),
                        CompanyKey = c.Guid(nullable: false),
                        Variable = c.String(),
                        Value = c.String(),
                        GroupName = c.String(),
                        Sort = c.String(),
                        Type = c.String(),
                        SubSort = c.String(),
                    })
                .PrimaryKey(t => t.UId, clustered: false)
                .ForeignKey("dbo.Companies", t => t.CompanyKey)
                .Index(t => t.SortingId, clustered: true, unique: true)
                .Index(t => t.CompanyKey);
            
            CreateTable(
                "dbo.Reports",
                c => new
                    {
                        UId = c.Guid(nullable: false),
                        Favorite = c.Boolean(nullable: false),
                        SortingId = c.Long(nullable: false, identity: true),
                        Name = c.String(maxLength: 250),
                        DateCreated = c.DateTimeOffset(nullable: false, precision: 7),
                        DateModified = c.DateTimeOffset(nullable: false, precision: 7),
                        Owner = c.Guid(nullable: false),
                        Group = c.Guid(nullable: false),
                        Permission = c.String(maxLength: 3),
                        ModificationToken = c.Guid(nullable: false),
                        ModifiedBy = c.Guid(nullable: false),
                        Company = c.Guid(nullable: false),
                        ReportType = c.Int(nullable: false),
                        Company_UId = c.Guid(),
                    })
                .PrimaryKey(t => t.UId, clustered: false)
                .ForeignKey("dbo.Companies", t => t.Company_UId)
                .Index(t => t.SortingId, clustered: true, unique: true)
                .Index(t => t.Company_UId);
            
            CreateTable(
                "dbo.ReportHeaders",
                c => new
                    {
                        UId = c.Guid(nullable: false),
                        SortingId = c.Long(nullable: false, identity: true),
                        Name = c.String(maxLength: 250),
                        DateCreated = c.DateTimeOffset(nullable: false, precision: 7),
                        DateModified = c.DateTimeOffset(nullable: false, precision: 7),
                        Owner = c.Guid(nullable: false),
                        Group = c.Guid(nullable: false),
                        Permission = c.String(maxLength: 3),
                        ModificationToken = c.Guid(nullable: false),
                        ModifiedBy = c.Guid(nullable: false),
                        Company = c.Guid(nullable: false),
                        ReportUId = c.Guid(nullable: false),
                        Position = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.UId, clustered: false)
                .ForeignKey("dbo.Reports", t => t.ReportUId)
                .Index(t => t.SortingId, clustered: true, unique: true)
                .Index(t => t.ReportUId);
            
            CreateTable(
                "dbo.SqlHeaders",
                c => new
                    {
                        UId = c.Guid(nullable: false),
                        SortingId = c.Long(nullable: false, identity: true),
                        Name = c.String(maxLength: 250),
                        DateCreated = c.DateTimeOffset(nullable: false, precision: 7),
                        DateModified = c.DateTimeOffset(nullable: false, precision: 7),
                        Owner = c.Guid(nullable: false),
                        Group = c.Guid(nullable: false),
                        Permission = c.String(maxLength: 3),
                        ModificationToken = c.Guid(nullable: false),
                        ModifiedBy = c.Guid(nullable: false),
                        Company = c.Guid(nullable: false),
                        SqlTableUId = c.Guid(),
                        ReportHeaderUId = c.Guid(),
                        SqlHeaderUId = c.String(),
                    })
                .PrimaryKey(t => t.UId, clustered: false)
                .ForeignKey("dbo.ReportHeaders", t => t.ReportHeaderUId)
                .ForeignKey("dbo.SqlTables", t => t.SqlTableUId)
                .Index(t => t.SortingId, clustered: true, unique: true)
                .Index(t => t.SqlTableUId)
                .Index(t => t.ReportHeaderUId);
            
            CreateTable(
                "dbo.SqlTables",
                c => new
                    {
                        UId = c.Guid(nullable: false),
                        SortingId = c.Long(nullable: false, identity: true),
                        Name = c.String(maxLength: 250),
                        DateCreated = c.DateTimeOffset(nullable: false, precision: 7),
                        DateModified = c.DateTimeOffset(nullable: false, precision: 7),
                        Owner = c.Guid(nullable: false),
                        Group = c.Guid(nullable: false),
                        Permission = c.String(maxLength: 3),
                        ModificationToken = c.Guid(nullable: false),
                        ModifiedBy = c.Guid(nullable: false),
                        Company = c.Guid(nullable: false),
                        RawSortings = c.String(),
                        RawFilters = c.String(),
                        ReportUId = c.Guid(nullable: false),
                        TableUId = c.Guid(nullable: false),
                        TableType = c.String(maxLength: 150),
                        TableName = c.String(maxLength: 250),
                    })
                .PrimaryKey(t => t.UId, clustered: false)
                .ForeignKey("dbo.Reports", t => t.ReportUId)
                .Index(t => t.SortingId, clustered: true, unique: true)
                .Index(t => t.ReportUId);
            
            CreateTable(
                "dbo.SavedLists",
                c => new
                    {
                        UId = c.Guid(nullable: false),
                        CompanyKey = c.Guid(nullable: false),
                        SortingId = c.Long(nullable: false, identity: true),
                        Name = c.String(maxLength: 250),
                        Permission = c.String(maxLength: 3),
                        DateCreated = c.DateTimeOffset(nullable: false, precision: 7),
                        DateModified = c.DateTimeOffset(nullable: false, precision: 7),
                        Owner = c.Guid(nullable: false),
                        Group = c.Guid(nullable: false),
                        ModificationToken = c.Guid(nullable: false),
                        ModifiedBy = c.Guid(nullable: false),
                        Description = c.String(maxLength: 1000),
                        Company_UId = c.Guid(),
                    })
                .PrimaryKey(t => t.UId, clustered: false)
                .ForeignKey("dbo.Companies", t => t.Company_UId)
                .Index(t => t.SortingId, clustered: true, unique: true)
                .Index(t => t.Company_UId);
            
            CreateTable(
                "dbo.AspNetUsers",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        CompanyKey = c.Guid(nullable: false),
                        DateCreated = c.DateTimeOffset(nullable: false, precision: 7),
                        DateModified = c.DateTimeOffset(nullable: false, precision: 7),
                        PasswordResetTokenExpiration = c.DateTimeOffset(nullable: false, precision: 7),
                        LastPasswordFailureDate = c.DateTimeOffset(nullable: false, precision: 7),
                        PasswordChangeDate = c.DateTimeOffset(nullable: false, precision: 7),
                        LockedDate = c.DateTimeOffset(nullable: false, precision: 7),
                        Birthdate = c.DateTime(nullable: false),
                        Owner = c.Guid(nullable: false),
                        Group = c.Guid(nullable: false),
                        ModificationToken = c.Guid(nullable: false),
                        ModifiedBy = c.Guid(nullable: false),
                        ValidationToken = c.Guid(nullable: false),
                        PasswordResetToken = c.Guid(nullable: false),
                        CurrentCompany = c.Guid(nullable: false),
                        Permission = c.String(maxLength: 3),
                        Crumbs = c.String(),
                        LockReason = c.String(),
                        UTCOffset = c.String(),
                        Comment = c.String(),
                        PasswordQuestion = c.String(),
                        PasswordAnswer = c.String(),
                        FirstName = c.String(),
                        LastName = c.String(),
                        LastIPA = c.String(),
                        CurrentCompanyName = c.String(),
                        IsLocked = c.Boolean(nullable: false),
                        IsConfirmed = c.Boolean(nullable: false),
                        PasswordFailuresSinceLastSuccess = c.Int(nullable: false),
                        Email = c.String(maxLength: 256),
                        EmailConfirmed = c.Boolean(nullable: false),
                        PasswordHash = c.String(),
                        SecurityStamp = c.String(),
                        PhoneNumber = c.String(),
                        PhoneNumberConfirmed = c.Boolean(nullable: false),
                        TwoFactorEnabled = c.Boolean(nullable: false),
                        LockoutEndDateUtc = c.DateTime(),
                        LockoutEnabled = c.Boolean(nullable: false),
                        AccessFailedCount = c.Int(nullable: false),
                        UserName = c.String(nullable: false, maxLength: 256),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Companies", t => t.CompanyKey)
                .Index(t => t.CompanyKey)
                .Index(t => t.UserName, unique: true, name: "UserNameIndex");
            
            CreateTable(
                "dbo.AspNetUserClaims",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.String(nullable: false, maxLength: 128),
                        ClaimType = c.String(),
                        ClaimValue = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.CustomGroups",
                c => new
                    {
                        UId = c.Guid(nullable: false),
                        SortingId = c.Long(nullable: false, identity: true),
                        Company = c.Guid(nullable: false),
                        Name = c.String(),
                        Owner = c.Guid(nullable: false),
                        Group = c.Guid(nullable: false),
                        Permission = c.String(maxLength: 3),
                    })
                .PrimaryKey(t => t.UId, clustered: false)
                .Index(t => t.SortingId, clustered: true, unique: true);
            
            CreateTable(
                "dbo.AspNetUserLogins",
                c => new
                    {
                        LoginProvider = c.String(nullable: false, maxLength: 128),
                        ProviderKey = c.String(nullable: false, maxLength: 128),
                        UserId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.LoginProvider, t.ProviderKey, t.UserId })
                .ForeignKey("dbo.AspNetUsers", t => t.UserId)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.AspNetUserRoles",
                c => new
                    {
                        UserId = c.String(nullable: false, maxLength: 128),
                        RoleId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.UserId, t.RoleId })
                .ForeignKey("dbo.AspNetUsers", t => t.UserId)
                .ForeignKey("dbo.AspNetRoles", t => t.RoleId)
                .Index(t => t.UserId)
                .Index(t => t.RoleId);
            
            CreateTable(
                "dbo.DefaultComponentOrders",
                c => new
                    {
                        UId = c.Guid(nullable: false),
                        SortingId = c.Long(nullable: false, identity: true),
                        FormKey = c.Guid(nullable: false),
                        CompanyKey = c.Guid(),
                        ComponentType = c.String(),
                        Order = c.String(),
                    })
                .PrimaryKey(t => t.UId, clustered: false)
                .ForeignKey("dbo.Companies", t => t.CompanyKey)
                .ForeignKey("dbo.Forms", t => t.FormKey)
                .Index(t => t.SortingId, clustered: true, unique: true)
                .Index(t => t.FormKey)
                .Index(t => t.CompanyKey);
            
            CreateTable(
                "dbo.FormStyles",
                c => new
                    {
                        UId = c.Guid(nullable: false),
                        SortingId = c.Long(nullable: false, identity: true),
                        Name = c.String(maxLength: 250),
                        Permission = c.String(maxLength: 3),
                        DateCreated = c.DateTimeOffset(nullable: false, precision: 7),
                        DateModified = c.DateTimeOffset(nullable: false, precision: 7),
                        Owner = c.Guid(nullable: false),
                        Group = c.Guid(nullable: false),
                        ModificationToken = c.Guid(nullable: false),
                        ModifiedBy = c.Guid(nullable: false),
                        FormKey = c.Guid(nullable: false),
                        Variable = c.String(),
                        Value = c.String(),
                        GroupName = c.String(),
                        Sort = c.String(),
                        Type = c.String(),
                        SubSort = c.String(),
                    })
                .PrimaryKey(t => t.UId, clustered: false)
                .ForeignKey("dbo.Forms", t => t.FormKey)
                .Index(t => t.SortingId, clustered: true, unique: true)
                .Index(t => t.FormKey);
            
            CreateTable(
                "dbo.LogicBlocks",
                c => new
                    {
                        UId = c.Guid(nullable: false),
                        SortingId = c.Long(nullable: false, identity: true),
                        Name = c.String(maxLength: 250),
                        Permission = c.String(maxLength: 3),
                        DateCreated = c.DateTimeOffset(nullable: false, precision: 7),
                        DateModified = c.DateTimeOffset(nullable: false, precision: 7),
                        Owner = c.Guid(nullable: false),
                        Group = c.Guid(nullable: false),
                        ModificationToken = c.Guid(nullable: false),
                        ModifiedBy = c.Guid(nullable: false),
                        FormKey = c.Guid(),
                    })
                .PrimaryKey(t => t.UId, clustered: false)
                .ForeignKey("dbo.Forms", t => t.FormKey)
                .Index(t => t.SortingId, clustered: true, unique: true)
                .Index(t => t.FormKey);
            //Primary Key update end here
            CreateTable(
                "dbo.MerchantAccountInfo",
                c => new
                    {
                        UId = c.Guid(nullable: false),
                        Company = c.Guid(nullable: false),
                        SortingId = c.Long(nullable: false, identity: true),
                        Name = c.String(maxLength: 250),
                        DateCreated = c.DateTimeOffset(nullable: false, precision: 7),
                        DateModified = c.DateTimeOffset(nullable: false, precision: 7),
                        Owner = c.Guid(nullable: false),
                        Group = c.Guid(nullable: false),
                        Permission = c.String(maxLength: 3),
                        ModificationToken = c.Guid(nullable: false),
                        ModifiedBy = c.Guid(nullable: false),
                        UserName = c.String(),
                        HashKey = c.String(),
                        Descriminator = c.String(),
                        Salt = c.String(),
                    })
                .PrimaryKey(t => t.UId);
            
            CreateTable(
                "dbo.Pages",
                c => new
                    {
                        UId = c.Guid(nullable: false),
                        SortingId = c.Long(nullable: false, identity: true),
                        Name = c.String(maxLength: 250),
                        DateCreated = c.DateTimeOffset(nullable: false, precision: 7),
                        DateModified = c.DateTimeOffset(nullable: false, precision: 7),
                        Owner = c.Guid(nullable: false),
                        Group = c.Guid(nullable: false),
                        Permission = c.String(maxLength: 3),
                        ModificationToken = c.Guid(nullable: false),
                        ModifiedBy = c.Guid(nullable: false),
                        FormKey = c.Guid(nullable: false),
                        Description = c.String(maxLength: 1000),
                        RSVP = c.Int(nullable: false),
                        Type = c.Int(nullable: false),
                        AdminOnly = c.Boolean(nullable: false),
                        Display = c.Boolean(nullable: false),
                        Enabled = c.Boolean(nullable: false),
                        Locked = c.Boolean(nullable: false),
                        PageNumber = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.UId)
                .ForeignKey("dbo.Forms", t => t.FormKey)
                .Index(t => t.FormKey);
            
            CreateTable(
                "dbo.Panels",
                c => new
                    {
                        UId = c.Guid(nullable: false),
                        SortingId = c.Long(nullable: false, identity: true),
                        Name = c.String(maxLength: 250),
                        Permission = c.String(maxLength: 3),
                        DateCreated = c.DateTimeOffset(nullable: false, precision: 7),
                        DateModified = c.DateTimeOffset(nullable: false, precision: 7),
                        Owner = c.Guid(nullable: false),
                        Group = c.Guid(nullable: false),
                        ModificationToken = c.Guid(nullable: false),
                        ModifiedBy = c.Guid(nullable: false),
                        PageKey = c.Guid(nullable: false),
                        Description = c.String(maxLength: 1000),
                        RSVP = c.Int(nullable: false),
                        AdminOnly = c.Boolean(nullable: false),
                        Display = c.Boolean(nullable: false),
                        Enabled = c.Boolean(nullable: false),
                        Locked = c.Boolean(nullable: false),
                        Order = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.UId)
                .ForeignKey("dbo.Pages", t => t.PageKey)
                .Index(t => t.PageKey);
            
            CreateTable(
                "dbo.Seatings",
                c => new
                    {
                        UId = c.Guid(nullable: false),
                        SortingId = c.Long(nullable: false, identity: true),
                        Name = c.String(maxLength: 250),
                        Permission = c.String(maxLength: 3),
                        DateCreated = c.DateTimeOffset(nullable: false, precision: 7),
                        DateModified = c.DateTimeOffset(nullable: false, precision: 7),
                        Owner = c.Guid(nullable: false),
                        Group = c.Guid(nullable: false),
                        ModificationToken = c.Guid(nullable: false),
                        ModifiedBy = c.Guid(nullable: false),
                        FormKey = c.Guid(nullable: false),
                        FullLabel = c.String(maxLength: 1000),
                        WaitlistLabel = c.String(maxLength: 1000),
                        WaitlistItemLabel = c.String(maxLength: 1000),
                        SeatinType = c.Int(nullable: false),
                        Waitlistable = c.Boolean(nullable: false),
                        MultipleSeats = c.Boolean(nullable: false),
                        MaxSeats = c.Int(nullable: false),
                        Start = c.DateTimeOffset(nullable: false, precision: 7),
                        End = c.DateTimeOffset(nullable: false, precision: 7),
                    })
                .PrimaryKey(t => t.UId)
                .ForeignKey("dbo.Forms", t => t.FormKey)
                .Index(t => t.FormKey);
            
            CreateTable(
                "dbo.Seaters",
                c => new
                    {
                        UId = c.Guid(nullable: false),
                        SortingId = c.Long(nullable: false, identity: true),
                        SeatingKey = c.Guid(nullable: false),
                        ComponentKey = c.Guid(nullable: false),
                        Confirmation = c.String(),
                        Seated = c.Boolean(nullable: false),
                        Date = c.DateTimeOffset(nullable: false, precision: 7),
                        DateSeated = c.DateTimeOffset(nullable: false, precision: 7),
                    })
                .PrimaryKey(t => t.UId)
                .ForeignKey("dbo.ComponentBases", t => t.ComponentKey)
                .ForeignKey("dbo.Seatings", t => t.SeatingKey)
                .Index(t => t.SeatingKey)
                .Index(t => t.ComponentKey);
            
            CreateTable(
                "dbo.SeatingStyle",
                c => new
                    {
                        UId = c.Guid(nullable: false),
                        SeatingKey = c.Guid(nullable: false),
                        SortingId = c.Long(nullable: false, identity: true),
                        Name = c.String(),
                        Value = c.String(),
                        Variable = c.String(),
                        GroupName = c.String(),
                        Sort = c.String(),
                        SubSort = c.String(),
                        Type = c.String(),
                    })
                .PrimaryKey(t => t.UId)
                .ForeignKey("dbo.Seatings", t => t.SeatingKey)
                .Index(t => t.SeatingKey);
            
            CreateTable(
                "dbo.FormTemplates",
                c => new
                    {
                        UId = c.Guid(nullable: false),
                        SortingId = c.Long(nullable: false, identity: true),
                        Name = c.String(maxLength: 250),
                        DateCreated = c.DateTimeOffset(nullable: false, precision: 7),
                        DateModified = c.DateTimeOffset(nullable: false, precision: 7),
                        Owner = c.Guid(nullable: false),
                        Group = c.Guid(nullable: false),
                        Permission = c.String(maxLength: 3),
                        ModificationToken = c.Guid(nullable: false),
                        ModifiedBy = c.Guid(nullable: false),
                        Company = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.UId);
            
            CreateTable(
                "dbo.Variables",
                c => new
                    {
                        UId = c.Guid(nullable: false),
                        FormKey = c.Guid(nullable: false),
                        SortingId = c.Long(nullable: false, identity: true),
                        Value = c.String(nullable: false, maxLength: 150),
                    })
                .PrimaryKey(t => t.UId)
                .ForeignKey("dbo.ComponentBases", t => t.UId)
                .ForeignKey("dbo.Forms", t => t.FormKey)
                .Index(t => t.UId)
                .Index(t => t.FormKey);
            
            CreateTable(
                "dbo.LogicGroups",
                c => new
                    {
                        UId = c.Guid(nullable: false),
                        SortingId = c.Long(nullable: false, identity: true),
                        LogicKey = c.Guid(nullable: false),
                        Link = c.Int(nullable: false),
                        Order = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.UId)
                .ForeignKey("dbo.Logics", t => t.LogicKey)
                .Index(t => t.LogicKey);
            
            CreateTable(
                "dbo.LogicStatements",
                c => new
                    {
                        UId = c.Guid(nullable: false),
                        SortingId = c.Long(nullable: false, identity: true),
                        LogicGroupKey = c.Guid(nullable: false),
                        FormKey = c.Guid(nullable: false),
                        Variable = c.String(),
                        Value = c.String(),
                        Link = c.Int(nullable: false),
                        Test = c.Int(nullable: false),
                        Order = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.UId)
                .ForeignKey("dbo.Forms", t => t.FormKey)
                .ForeignKey("dbo.LogicGroups", t => t.LogicGroupKey)
                .Index(t => t.LogicGroupKey)
                .Index(t => t.FormKey);
            
            CreateTable(
                "dbo.PriceGroups",
                c => new
                    {
                        UId = c.Guid(nullable: false),
                        SortingId = c.Long(nullable: false, identity: true),
                        Name = c.String(maxLength: 250),
                        Permission = c.String(maxLength: 3),
                        DateCreated = c.DateTimeOffset(nullable: false, precision: 7),
                        DateModified = c.DateTimeOffset(nullable: false, precision: 7),
                        Owner = c.Guid(nullable: false),
                        Group = c.Guid(nullable: false),
                        ModificationToken = c.Guid(nullable: false),
                        ModifiedBy = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.UId)
                .ForeignKey("dbo.ComponentBases", t => t.UId)
                .Index(t => t.UId);
            
            CreateTable(
                "dbo.Prices",
                c => new
                    {
                        UId = c.Guid(nullable: false),
                        PriceGroupKey = c.Guid(nullable: false),
                        SortingId = c.Long(nullable: false, identity: true),
                        Name = c.String(maxLength: 250),
                        DateCreated = c.DateTimeOffset(nullable: false, precision: 7),
                        DateModified = c.DateTimeOffset(nullable: false, precision: 7),
                        Owner = c.Guid(nullable: false),
                        Group = c.Guid(nullable: false),
                        Permission = c.String(maxLength: 3),
                        ModificationToken = c.Guid(nullable: false),
                        ModifiedBy = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.UId)
                .ForeignKey("dbo.PriceGroups", t => t.PriceGroupKey)
                .Index(t => t.PriceGroupKey);
            
            CreateTable(
                "dbo.Price",
                c => new
                    {
                        UId = c.Guid(nullable: false),
                        SortingId = c.Long(nullable: false, identity: true),
                        PricesKey = c.Guid(nullable: false),
                        Ammount = c.Decimal(precision: 18, scale: 2),
                        Start = c.DateTimeOffset(nullable: false, precision: 7),
                    })
                .PrimaryKey(t => t.UId)
                .ForeignKey("dbo.Prices", t => t.PricesKey)
                .Index(t => t.PricesKey);
            
            CreateTable(
                "dbo.PriceStyle",
                c => new
                    {
                        UId = c.Guid(nullable: false),
                        PriceGroupKey = c.Guid(nullable: false),
                        SortingId = c.Long(nullable: false, identity: true),
                        Name = c.String(),
                        Value = c.String(),
                        Variable = c.String(),
                        GroupName = c.String(),
                        Sort = c.String(),
                        SubSort = c.String(),
                        Type = c.String(),
                    })
                .PrimaryKey(t => t.UId)
                .ForeignKey("dbo.PriceGroups", t => t.PriceGroupKey)
                .Index(t => t.PriceGroupKey);
            
            CreateTable(
                "dbo.ComponentStyle",
                c => new
                    {
                        UId = c.Guid(nullable: false),
                        ComponentKey = c.Guid(nullable: false),
                        SortingId = c.Long(nullable: false, identity: true),
                        Name = c.String(),
                        Value = c.String(),
                        Variable = c.String(),
                        GroupName = c.String(),
                        Sort = c.String(),
                        SubSort = c.String(),
                        Type = c.String(),
                    })
                .PrimaryKey(t => t.UId)
                .ForeignKey("dbo.ComponentBases", t => t.ComponentKey)
                .Index(t => t.ComponentKey);
            
            CreateTable(
                "dbo.Registrants",
                c => new
                    {
                        UId = c.Guid(nullable: false),
                        FormKey = c.Guid(nullable: false),
                        AudienceKey = c.Guid(),
                        Email = c.String(nullable: false, maxLength: 250),
                        Confirmation = c.String(nullable: false, maxLength: 50),
                        RSVP = c.Boolean(nullable: false),
                        Status = c.Int(nullable: false),
                        Type = c.Int(nullable: false),
                        SortingId = c.Long(nullable: false, identity: true),
                        Name = c.String(maxLength: 250),
                        Permission = c.String(maxLength: 3),
                        DateCreated = c.DateTimeOffset(nullable: false, precision: 7),
                        DateModified = c.DateTimeOffset(nullable: false, precision: 7),
                        Owner = c.Guid(nullable: false),
                        Group = c.Guid(nullable: false),
                        ModificationToken = c.Guid(nullable: false),
                        ModifiedBy = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.UId)
                .ForeignKey("dbo.Audiences", t => t.AudienceKey)
                .ForeignKey("dbo.Forms", t => t.FormKey)
                .Index(t => t.FormKey)
                .Index(t => t.AudienceKey);
            
            CreateTable(
                "dbo.RegistrantData",
                c => new
                    {
                        UId = c.Guid(nullable: false),
                        SortingId = c.Long(nullable: false, identity: true),
                        Variable = c.String(nullable: false, maxLength: 250),
                        VariableUId = c.Guid(nullable: false),
                        Descriminator = c.String(nullable: false, maxLength: 250),
                        Value = c.String(nullable: false, maxLength: 1000),
                        RegistrantKey = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.UId)
                .ForeignKey("dbo.Registrants", t => t.RegistrantKey)
                .Index(t => t.RegistrantKey);
            
            CreateTable(
                "dbo.OldRegistrants",
                c => new
                    {
                        UId = c.Guid(nullable: false),
                        AudienceKey = c.Guid(),
                        CurrentRegistrantKey = c.Guid(nullable: false),
                        Email = c.String(nullable: false, maxLength: 250),
                        Confirmation = c.String(nullable: false, maxLength: 50),
                        RSVP = c.Boolean(nullable: false),
                        Status = c.Int(nullable: false),
                        Type = c.Int(nullable: false),
                        SortingId = c.Long(nullable: false, identity: true),
                        Name = c.String(maxLength: 250),
                        Permission = c.String(maxLength: 3),
                        DateCreated = c.DateTimeOffset(nullable: false, precision: 7),
                        DateModified = c.DateTimeOffset(nullable: false, precision: 7),
                        Owner = c.Guid(nullable: false),
                        Group = c.Guid(nullable: false),
                        ModificationToken = c.Guid(nullable: false),
                        ModifiedBy = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.UId)
                .ForeignKey("dbo.Audiences", t => t.AudienceKey)
                .ForeignKey("dbo.Registrants", t => t.CurrentRegistrantKey)
                .Index(t => t.AudienceKey)
                .Index(t => t.CurrentRegistrantKey);
            
            CreateTable(
                "dbo.OldRegistrantData",
                c => new
                    {
                        UId = c.Guid(nullable: false),
                        SortingId = c.Long(nullable: false, identity: true),
                        Variable = c.String(nullable: false, maxLength: 250),
                        Value = c.String(nullable: false, maxLength: 1000),
                        Descriminator = c.String(nullable: false, maxLength: 250),
                        VariableUId = c.Guid(nullable: false),
                        RegistrantKey = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.UId)
                .ForeignKey("dbo.OldRegistrants", t => t.RegistrantKey)
                .Index(t => t.RegistrantKey);
            
            CreateTable(
                "dbo.TransactionRequests",
                c => new
                    {
                        UId = c.Guid(nullable: false),
                        SortingId = c.Long(nullable: false, identity: true),
                        Ammount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Confirmation = c.String(),
                        RegistrantUId = c.Guid(),
                        FormUId = c.Guid(nullable: false),
                        CompanyUId = c.Guid(nullable: false),
                        CreditCardUId = c.Guid(nullable: false),
                        TransactionType = c.Int(nullable: false),
                        Status = c.Int(nullable: false),
                        Success = c.Boolean(nullable: false),
                        Mode = c.Int(nullable: false),
                        FirstName = c.String(),
                        LastName = c.String(),
                        Address = c.String(),
                        City = c.String(),
                        State = c.String(),
                        Zip = c.String(),
                        Country = c.String(),
                        Id = c.String(),
                        AuthCode = c.String(),
                        TransactionId = c.String(),
                        MerchantAccount_UId = c.Guid(),
                    })
                .PrimaryKey(t => t.UId)
                .ForeignKey("dbo.CreditCards", t => t.CreditCardUId)
                .ForeignKey("dbo.MerchantAccountInfo", t => t.MerchantAccount_UId)
                .ForeignKey("dbo.Registrants", t => t.RegistrantUId)
                .Index(t => t.RegistrantUId)
                .Index(t => t.CreditCardUId)
                .Index(t => t.MerchantAccount_UId);
            
            CreateTable(
                "dbo.CreditCards",
                c => new
                    {
                        UId = c.Guid(nullable: false),
                        SortingId = c.Long(nullable: false, identity: true),
                        HashNumber = c.String(),
                        Type = c.Int(nullable: false),
                        NameOnCard = c.String(),
                        Address = c.String(),
                        City = c.String(),
                        State = c.String(),
                        Zip = c.String(),
                        Country = c.String(),
                        Phone = c.String(),
                        SecurityCode = c.String(),
                        FormUId = c.Guid(nullable: false),
                        Confirmation = c.String(),
                        CompanyUId = c.Guid(nullable: false),
                        Salt = c.String(),
                        Exp = c.String(),
                        Code = c.String(),
                    })
                .PrimaryKey(t => t.UId);
            
            CreateTable(
                "dbo.CreditCardAccess",
                c => new
                    {
                        UId = c.Guid(nullable: false),
                        SortingId = c.Long(nullable: false, identity: true),
                        UserUId = c.Guid(nullable: false),
                        CreditCardUId = c.Guid(),
                        AccessTime = c.DateTimeOffset(nullable: false, precision: 7),
                        AccessType = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.UId)
                .ForeignKey("dbo.CreditCards", t => t.CreditCardUId)
                .Index(t => t.CreditCardUId);
            
            CreateTable(
                "dbo.TransactionDetails",
                c => new
                    {
                        UId = c.Guid(nullable: false),
                        SortingId = c.Long(nullable: false, identity: true),
                        RequestUId = c.Guid(),
                        FormUId = c.Guid(),
                        CompanyUId = c.Guid(),
                        CreditCardUId = c.Guid(nullable: false),
                        Ammount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Approved = c.Boolean(nullable: false),
                        AuthorizationCode = c.String(),
                        CardNumber = c.String(),
                        InvoiceNumber = c.String(),
                        Message = c.String(),
                        ResponseCode = c.String(),
                        TransactionID = c.String(),
                        TransactionType = c.Int(nullable: false),
                        CreationDate = c.DateTimeOffset(nullable: false, precision: 7),
                    })
                .PrimaryKey(t => t.UId)
                .ForeignKey("dbo.CreditCards", t => t.CreditCardUId)
                .ForeignKey("dbo.TransactionRequests", t => t.RequestUId)
                .Index(t => t.RequestUId)
                .Index(t => t.CreditCardUId);
            
            CreateTable(
                "dbo.Constants",
                c => new
                    {
                        Key = c.String(nullable: false, maxLength: 250),
                        SortingId = c.Long(nullable: false, identity: true),
                        Value = c.String(),
                        Type = c.String(maxLength: 100),
                    })
                .PrimaryKey(t => t.Key);
            
            CreateTable(
                "dbo.CSS",
                c => new
                    {
                        UId = c.Guid(nullable: false),
                        SortingId = c.Long(nullable: false, identity: true),
                        Name = c.String(maxLength: 250),
                        DateCreated = c.DateTimeOffset(nullable: false, precision: 7),
                        DateModified = c.DateTimeOffset(nullable: false, precision: 7),
                        Owner = c.Guid(nullable: false),
                        Group = c.Guid(nullable: false),
                        Permission = c.String(maxLength: 3),
                        ModificationToken = c.Guid(nullable: false),
                        ModifiedBy = c.Guid(nullable: false),
                        StandAlone = c.Boolean(nullable: false),
                        ComponentUId = c.Guid(),
                        StylesheetUId = c.Guid(),
                        Class = c.String(maxLength: 250),
                        RawCss = c.String(),
                        RawSelectors = c.String(),
                    })
                .PrimaryKey(t => t.UId)
                .ForeignKey("dbo.Stylesheets", t => t.StylesheetUId)
                .Index(t => t.StylesheetUId);
            
            CreateTable(
                "dbo.DatabaseAccess",
                c => new
                    {
                        SortingId = c.Long(nullable: false, identity: true),
                        Company = c.Guid(nullable: false),
                        User = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.SortingId);
            
            CreateTable(
                "dbo.EmailEvents",
                c => new
                    {
                        UId = c.Guid(nullable: false),
                        SortingId = c.Long(nullable: false, identity: true),
                        EmailUId = c.Guid(nullable: false),
                        Event = c.String(maxLength: 250),
                        Date = c.DateTimeOffset(nullable: false, precision: 7),
                        Cats = c.String(),
                        Params = c.String(),
                    })
                .PrimaryKey(t => t.UId);
            
            CreateTable(
                "dbo.ErrorReviews",
                c => new
                    {
                        UId = c.Guid(nullable: false),
                        SortingId = c.Long(nullable: false, identity: true),
                        Name = c.String(maxLength: 250),
                        DateCreated = c.DateTimeOffset(nullable: false, precision: 7),
                        DateModified = c.DateTimeOffset(nullable: false, precision: 7),
                        Owner = c.Guid(nullable: false),
                        Group = c.Guid(nullable: false),
                        Permission = c.String(maxLength: 3),
                        ModificationToken = c.Guid(nullable: false),
                        ModifiedBy = c.Guid(nullable: false),
                        ErrorUId = c.Guid(nullable: false),
                        Person = c.Guid(nullable: false),
                        Status = c.Int(nullable: false),
                        Message = c.String(),
                    })
                .PrimaryKey(t => t.UId)
                .ForeignKey("dbo.Errors", t => t.ErrorUId)
                .Index(t => t.ErrorUId);
            
            CreateTable(
                "dbo.Errors",
                c => new
                    {
                        UId = c.Guid(nullable: false),
                        SortingId = c.Long(nullable: false, identity: true),
                        Name = c.String(maxLength: 250),
                        DateCreated = c.DateTimeOffset(nullable: false, precision: 7),
                        DateModified = c.DateTimeOffset(nullable: false, precision: 7),
                        Owner = c.Guid(nullable: false),
                        Group = c.Guid(nullable: false),
                        Permission = c.String(maxLength: 3),
                        ModificationToken = c.Guid(nullable: false),
                        ModifiedBy = c.Guid(nullable: false),
                        Controller = c.String(maxLength: 250),
                        Action = c.String(maxLength: 250),
                        RouteData = c.String(maxLength: 1000),
                        ErrorStatus = c.Int(nullable: false),
                        Source = c.String(maxLength: 1000),
                        Message = c.String(),
                        StackTrace = c.String(),
                        Person = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.UId);
            
            CreateTable(
                "dbo.AspNetRoles",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Name = c.String(nullable: false, maxLength: 256),
                        Discriminator = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.Name, unique: true, name: "RoleNameIndex");
            
            CreateTable(
                "dbo.SmtpServers",
                c => new
                    {
                        UId = c.Guid(nullable: false),
                        SortingId = c.Long(nullable: false, identity: true),
                        Name = c.String(maxLength: 250),
                        DateCreated = c.DateTimeOffset(nullable: false, precision: 7),
                        DateModified = c.DateTimeOffset(nullable: false, precision: 7),
                        Owner = c.Guid(nullable: false),
                        Group = c.Guid(nullable: false),
                        Permission = c.String(maxLength: 3),
                        ModificationToken = c.Guid(nullable: false),
                        ModifiedBy = c.Guid(nullable: false),
                        CompanyUId = c.Guid(nullable: false),
                        Description = c.String(),
                        Username = c.String(maxLength: 250),
                        PasswordHash = c.String(maxLength: 1000),
                        Primary = c.Boolean(nullable: false),
                        Address = c.String(maxLength: 500),
                        SSL = c.Boolean(nullable: false),
                        Port = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.UId);
            
            CreateTable(
                "dbo.Stylesheets",
                c => new
                    {
                        UId = c.Guid(nullable: false),
                        SortingId = c.Long(nullable: false, identity: true),
                        Name = c.String(maxLength: 250),
                        DateCreated = c.DateTimeOffset(nullable: false, precision: 7),
                        DateModified = c.DateTimeOffset(nullable: false, precision: 7),
                        Owner = c.Guid(nullable: false),
                        Group = c.Guid(nullable: false),
                        Permission = c.String(maxLength: 3),
                        ModificationToken = c.Guid(nullable: false),
                        ModifiedBy = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.UId);
            
            CreateTable(
                "dbo.ComponentBaseAudiences",
                c => new
                    {
                        ComponentBase_UId = c.Guid(nullable: false),
                        Audience_UId = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => new { t.ComponentBase_UId, t.Audience_UId })
                .ForeignKey("dbo.ComponentBases", t => t.ComponentBase_UId)
                .ForeignKey("dbo.Audiences", t => t.Audience_UId)
                .Index(t => t.ComponentBase_UId)
                .Index(t => t.Audience_UId);
            
            CreateTable(
                "dbo.ContactEmailLists",
                c => new
                    {
                        Contact_UId = c.Guid(nullable: false),
                        EmailList_UId = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => new { t.Contact_UId, t.EmailList_UId })
                .ForeignKey("dbo.Contacts", t => t.Contact_UId)
                .ForeignKey("dbo.EmailLists", t => t.EmailList_UId)
                .Index(t => t.Contact_UId)
                .Index(t => t.EmailList_UId);
            
            CreateTable(
                "dbo.TagEmailCampaigns",
                c => new
                    {
                        Tag_UId = c.Guid(nullable: false),
                        EmailCampaign_UId = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => new { t.Tag_UId, t.EmailCampaign_UId })
                .ForeignKey("dbo.Tags", t => t.Tag_UId)
                .ForeignKey("dbo.EmailCampaigns", t => t.EmailCampaign_UId)
                .Index(t => t.Tag_UId)
                .Index(t => t.EmailCampaign_UId);
            
            CreateTable(
                "dbo.TagForms",
                c => new
                    {
                        Tag_UId = c.Guid(nullable: false),
                        Form_UId = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => new { t.Tag_UId, t.Form_UId })
                .ForeignKey("dbo.Tags", t => t.Tag_UId)
                .ForeignKey("dbo.Forms", t => t.Form_UId)
                .Index(t => t.Tag_UId)
                .Index(t => t.Form_UId);
            
            CreateTable(
                "dbo.CustomGroupUsers",
                c => new
                    {
                        CustomGroup_UId = c.Guid(nullable: false),
                        User_Id = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.CustomGroup_UId, t.User_Id })
                .ForeignKey("dbo.CustomGroups", t => t.CustomGroup_UId)
                .ForeignKey("dbo.AspNetUsers", t => t.User_Id)
                .Index(t => t.CustomGroup_UId)
                .Index(t => t.User_Id);
            
            CreateTable(
                "dbo.PageAudiences",
                c => new
                    {
                        Page_UId = c.Guid(nullable: false),
                        Audience_UId = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => new { t.Page_UId, t.Audience_UId })
                .ForeignKey("dbo.Pages", t => t.Page_UId)
                .ForeignKey("dbo.Audiences", t => t.Audience_UId)
                .Index(t => t.Page_UId)
                .Index(t => t.Audience_UId);
            
            CreateTable(
                "dbo.PanelAudiences",
                c => new
                    {
                        Panel_UId = c.Guid(nullable: false),
                        Audience_UId = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => new { t.Panel_UId, t.Audience_UId })
                .ForeignKey("dbo.Panels", t => t.Panel_UId)
                .ForeignKey("dbo.Audiences", t => t.Audience_UId)
                .Index(t => t.Panel_UId)
                .Index(t => t.Audience_UId);
            
            CreateTable(
                "dbo.PricesAudiences",
                c => new
                    {
                        Prices_UId = c.Guid(nullable: false),
                        Audience_UId = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => new { t.Prices_UId, t.Audience_UId })
                .ForeignKey("dbo.Prices", t => t.Prices_UId)
                .ForeignKey("dbo.Audiences", t => t.Audience_UId)
                .Index(t => t.Prices_UId)
                .Index(t => t.Audience_UId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.CSS", "StylesheetUId", "dbo.Stylesheets");
            DropForeignKey("dbo.AspNetUserRoles", "RoleId", "dbo.AspNetRoles");
            DropForeignKey("dbo.ErrorReviews", "ErrorUId", "dbo.Errors");
            DropForeignKey("dbo.TransactionRequests", "RegistrantUId", "dbo.Registrants");
            DropForeignKey("dbo.TransactionRequests", "MerchantAccount_UId", "dbo.MerchantAccountInfo");
            DropForeignKey("dbo.TransactionDetails", "RequestUId", "dbo.TransactionRequests");
            DropForeignKey("dbo.TransactionDetails", "CreditCardUId", "dbo.CreditCards");
            DropForeignKey("dbo.TransactionRequests", "CreditCardUId", "dbo.CreditCards");
            DropForeignKey("dbo.CreditCardAccess", "CreditCardUId", "dbo.CreditCards");
            DropForeignKey("dbo.OldRegistrantData", "RegistrantKey", "dbo.OldRegistrants");
            DropForeignKey("dbo.OldRegistrants", "CurrentRegistrantKey", "dbo.Registrants");
            DropForeignKey("dbo.OldRegistrants", "AudienceKey", "dbo.Audiences");
            DropForeignKey("dbo.Registrants", "FormKey", "dbo.Forms");
            DropForeignKey("dbo.RegistrantData", "RegistrantKey", "dbo.Registrants");
            DropForeignKey("dbo.Registrants", "AudienceKey", "dbo.Audiences");
            DropForeignKey("dbo.ComponentBases", "RadioGroupKey", "dbo.ComponentBases");
            DropForeignKey("dbo.ComponentBases", "DropdownGroupKey", "dbo.ComponentBases");
            DropForeignKey("dbo.ComponentBases", "CheckboxGroupKey", "dbo.ComponentBases");
            DropForeignKey("dbo.ComponentStyle", "ComponentKey", "dbo.ComponentBases");
            DropForeignKey("dbo.PriceStyle", "PriceGroupKey", "dbo.PriceGroups");
            DropForeignKey("dbo.Prices", "PriceGroupKey", "dbo.PriceGroups");
            DropForeignKey("dbo.Price", "PricesKey", "dbo.Prices");
            DropForeignKey("dbo.PricesAudiences", "Audience_UId", "dbo.Audiences");
            DropForeignKey("dbo.PricesAudiences", "Prices_UId", "dbo.Prices");
            DropForeignKey("dbo.PriceGroups", "UId", "dbo.ComponentBases");
            DropForeignKey("dbo.LogicStatements", "LogicGroupKey", "dbo.LogicGroups");
            DropForeignKey("dbo.LogicStatements", "FormKey", "dbo.Forms");
            DropForeignKey("dbo.LogicGroups", "LogicKey", "dbo.Logics");
            DropForeignKey("dbo.Logics", "ComponentKey", "dbo.ComponentBases");
            DropForeignKey("dbo.LogicCommands", "LogicKey", "dbo.Logics");
            DropForeignKey("dbo.LogicCommands", "FormKey", "dbo.Forms");
            DropForeignKey("dbo.Variables", "FormKey", "dbo.Forms");
            DropForeignKey("dbo.Variables", "UId", "dbo.ComponentBases");
            DropForeignKey("dbo.Forms", "FormTemplateKey", "dbo.FormTemplates");
            DropForeignKey("dbo.SeatingStyle", "SeatingKey", "dbo.Seatings");
            DropForeignKey("dbo.Seaters", "SeatingKey", "dbo.Seatings");
            DropForeignKey("dbo.Seaters", "ComponentKey", "dbo.ComponentBases");
            DropForeignKey("dbo.Seatings", "FormKey", "dbo.Forms");
            DropForeignKey("dbo.ComponentBases", "SeatingKey", "dbo.Seatings");
            DropForeignKey("dbo.Panels", "PageKey", "dbo.Pages");
            DropForeignKey("dbo.Logics", "PanelKey", "dbo.Panels");
            DropForeignKey("dbo.ComponentBases", "PanelKey", "dbo.Panels");
            DropForeignKey("dbo.PanelAudiences", "Audience_UId", "dbo.Audiences");
            DropForeignKey("dbo.PanelAudiences", "Panel_UId", "dbo.Panels");
            DropForeignKey("dbo.Logics", "PageKey", "dbo.Pages");
            DropForeignKey("dbo.Pages", "FormKey", "dbo.Forms");
            DropForeignKey("dbo.PageAudiences", "Audience_UId", "dbo.Audiences");
            DropForeignKey("dbo.PageAudiences", "Page_UId", "dbo.Pages");
            DropForeignKey("dbo.Forms", "MerchantAccountKey", "dbo.MerchantAccountInfo");
            DropForeignKey("dbo.Logics", "LogicBlockKey", "dbo.LogicBlocks");
            DropForeignKey("dbo.LogicBlocks", "FormKey", "dbo.Forms");
            DropForeignKey("dbo.FormStyles", "FormKey", "dbo.Forms");
            DropForeignKey("dbo.DefaultComponentOrders", "FormKey", "dbo.Forms");
            DropForeignKey("dbo.DefaultComponentOrders", "CompanyKey", "dbo.Companies");
            DropForeignKey("dbo.AspNetUserRoles", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserLogins", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.CustomGroupUsers", "User_Id", "dbo.AspNetUsers");
            DropForeignKey("dbo.CustomGroupUsers", "CustomGroup_UId", "dbo.CustomGroups");
            DropForeignKey("dbo.AspNetUsers", "CompanyKey", "dbo.Companies");
            DropForeignKey("dbo.AspNetUserClaims", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.Forms", "InvitationListKey", "dbo.SavedLists");
            DropForeignKey("dbo.Contacts", "SavedList_UId", "dbo.SavedLists");
            DropForeignKey("dbo.SavedLists", "Company_UId", "dbo.Companies");
            DropForeignKey("dbo.Reports", "Company_UId", "dbo.Companies");
            DropForeignKey("dbo.SqlTables", "ReportUId", "dbo.Reports");
            DropForeignKey("dbo.SqlHeaders", "SqlTableUId", "dbo.SqlTables");
            DropForeignKey("dbo.SqlHeaders", "ReportHeaderUId", "dbo.ReportHeaders");
            DropForeignKey("dbo.ReportHeaders", "ReportUId", "dbo.Reports");
            DropForeignKey("dbo.DefaultFormStyles", "CompanyKey", "dbo.Companies");
            DropForeignKey("dbo.Forms", "CompanyKey", "dbo.Companies");
            DropForeignKey("dbo.Pointers", "FolderKey", "dbo.Folders");
            DropForeignKey("dbo.Folders", "CompanyKey", "dbo.Companies");
            DropForeignKey("dbo.Folders", "ParentKey", "dbo.Folders");
            DropForeignKey("dbo.Forms", "TypeKey", "dbo.NodeTypes");
            DropForeignKey("dbo.EmailCampaigns", "TypeKey", "dbo.NodeTypes");
            DropForeignKey("dbo.NodeTypes", "CompanyKey", "dbo.Companies");
            DropForeignKey("dbo.TagForms", "Form_UId", "dbo.Forms");
            DropForeignKey("dbo.TagForms", "Tag_UId", "dbo.Tags");
            DropForeignKey("dbo.TagEmailCampaigns", "EmailCampaign_UId", "dbo.EmailCampaigns");
            DropForeignKey("dbo.TagEmailCampaigns", "Tag_UId", "dbo.Tags");
            DropForeignKey("dbo.Tags", "CompanyKey", "dbo.Companies");
            DropForeignKey("dbo.EmailVariables", "UId", "dbo.RSEmails");
            DropForeignKey("dbo.RSEmails", "EmailTemplateKey", "dbo.EmailTemplates");
            DropForeignKey("dbo.EmailTemplateVariables", "UId", "dbo.EmailTemplates");
            DropForeignKey("dbo.EmailTemplateAreas", "UId", "dbo.EmailTemplates");
            DropForeignKey("dbo.TemplateEmailAreaVariables", "EmailTemplateAreaSortingId", "dbo.EmailTemplateAreas");
            DropForeignKey("dbo.RSEmails", "FormKey", "dbo.Forms");
            DropForeignKey("dbo.RSEmails", "EmailListKey", "dbo.EmailLists");
            DropForeignKey("dbo.ContactEmailLists", "EmailList_UId", "dbo.EmailLists");
            DropForeignKey("dbo.ContactEmailLists", "Contact_UId", "dbo.Contacts");
            DropForeignKey("dbo.ContactData", "UId", "dbo.Contacts");
            DropForeignKey("dbo.RSEmails", "EmailCampaignKey", "dbo.EmailCampaigns");
            DropForeignKey("dbo.EmailAreas", "UId", "dbo.RSEmails");
            DropForeignKey("dbo.EmailAreaVariables", "EmailAreaSortingId", "dbo.EmailAreas");
            DropForeignKey("dbo.CustomTexts", "FormKey", "dbo.Forms");
            DropForeignKey("dbo.CustomTexts", "EmailCampaignKey", "dbo.EmailCampaigns");
            DropForeignKey("dbo.EmailCampaigns", "CompanyKey", "dbo.Companies");
            DropForeignKey("dbo.Audiences", "FormKey", "dbo.Forms");
            DropForeignKey("dbo.ComponentBaseAudiences", "Audience_UId", "dbo.Audiences");
            DropForeignKey("dbo.ComponentBaseAudiences", "ComponentBase_UId", "dbo.ComponentBases");
            DropIndex("dbo.PricesAudiences", new[] { "Audience_UId" });
            DropIndex("dbo.PricesAudiences", new[] { "Prices_UId" });
            DropIndex("dbo.PanelAudiences", new[] { "Audience_UId" });
            DropIndex("dbo.PanelAudiences", new[] { "Panel_UId" });
            DropIndex("dbo.PageAudiences", new[] { "Audience_UId" });
            DropIndex("dbo.PageAudiences", new[] { "Page_UId" });
            DropIndex("dbo.CustomGroupUsers", new[] { "User_Id" });
            DropIndex("dbo.CustomGroupUsers", new[] { "CustomGroup_UId" });
            DropIndex("dbo.TagForms", new[] { "Form_UId" });
            DropIndex("dbo.TagForms", new[] { "Tag_UId" });
            DropIndex("dbo.TagEmailCampaigns", new[] { "EmailCampaign_UId" });
            DropIndex("dbo.TagEmailCampaigns", new[] { "Tag_UId" });
            DropIndex("dbo.ContactEmailLists", new[] { "EmailList_UId" });
            DropIndex("dbo.ContactEmailLists", new[] { "Contact_UId" });
            DropIndex("dbo.ComponentBaseAudiences", new[] { "Audience_UId" });
            DropIndex("dbo.ComponentBaseAudiences", new[] { "ComponentBase_UId" });
            DropIndex("dbo.AspNetRoles", "RoleNameIndex");
            DropIndex("dbo.ErrorReviews", new[] { "ErrorUId" });
            DropIndex("dbo.CSS", new[] { "StylesheetUId" });
            DropIndex("dbo.TransactionDetails", new[] { "CreditCardUId" });
            DropIndex("dbo.TransactionDetails", new[] { "RequestUId" });
            DropIndex("dbo.CreditCardAccess", new[] { "CreditCardUId" });
            DropIndex("dbo.TransactionRequests", new[] { "MerchantAccount_UId" });
            DropIndex("dbo.TransactionRequests", new[] { "CreditCardUId" });
            DropIndex("dbo.TransactionRequests", new[] { "RegistrantUId" });
            DropIndex("dbo.OldRegistrantData", new[] { "RegistrantKey" });
            DropIndex("dbo.OldRegistrants", new[] { "CurrentRegistrantKey" });
            DropIndex("dbo.OldRegistrants", new[] { "AudienceKey" });
            DropIndex("dbo.RegistrantData", new[] { "RegistrantKey" });
            DropIndex("dbo.Registrants", new[] { "AudienceKey" });
            DropIndex("dbo.Registrants", new[] { "FormKey" });
            DropIndex("dbo.ComponentStyle", new[] { "ComponentKey" });
            DropIndex("dbo.PriceStyle", new[] { "PriceGroupKey" });
            DropIndex("dbo.Price", new[] { "PricesKey" });
            DropIndex("dbo.Prices", new[] { "PriceGroupKey" });
            DropIndex("dbo.PriceGroups", new[] { "UId" });
            DropIndex("dbo.LogicStatements", new[] { "FormKey" });
            DropIndex("dbo.LogicStatements", new[] { "LogicGroupKey" });
            DropIndex("dbo.LogicGroups", new[] { "LogicKey" });
            DropIndex("dbo.Variables", new[] { "FormKey" });
            DropIndex("dbo.Variables", new[] { "UId" });
            DropIndex("dbo.SeatingStyle", new[] { "SeatingKey" });
            DropIndex("dbo.Seaters", new[] { "ComponentKey" });
            DropIndex("dbo.Seaters", new[] { "SeatingKey" });
            DropIndex("dbo.Seatings", new[] { "FormKey" });
            DropIndex("dbo.Panels", new[] { "PageKey" });
            DropIndex("dbo.Pages", new[] { "FormKey" });
            DropIndex("dbo.LogicBlocks", new[] { "FormKey" });
            DropIndex("dbo.FormStyles", new[] { "FormKey" });
            DropIndex("dbo.DefaultComponentOrders", new[] { "CompanyKey" });
            DropIndex("dbo.DefaultComponentOrders", new[] { "FormKey" });
            DropIndex("dbo.AspNetUserRoles", new[] { "RoleId" });
            DropIndex("dbo.AspNetUserRoles", new[] { "UserId" });
            DropIndex("dbo.AspNetUserLogins", new[] { "UserId" });
            DropIndex("dbo.AspNetUserClaims", new[] { "UserId" });
            DropIndex("dbo.AspNetUsers", "UserNameIndex");
            DropIndex("dbo.AspNetUsers", new[] { "CompanyKey" });
            DropIndex("dbo.SavedLists", new[] { "Company_UId" });
            DropIndex("dbo.SqlTables", new[] { "ReportUId" });
            DropIndex("dbo.SqlHeaders", new[] { "ReportHeaderUId" });
            DropIndex("dbo.SqlHeaders", new[] { "SqlTableUId" });
            DropIndex("dbo.ReportHeaders", new[] { "ReportUId" });
            DropIndex("dbo.Reports", new[] { "Company_UId" });
            DropIndex("dbo.DefaultFormStyles", new[] { "CompanyKey" });
            DropIndex("dbo.Pointers", new[] { "FolderKey" });
            DropIndex("dbo.Folders", new[] { "ParentKey" });
            DropIndex("dbo.Folders", new[] { "CompanyKey" });
            DropIndex("dbo.NodeTypes", new[] { "CompanyKey" });
            DropIndex("dbo.Tags", new[] { "CompanyKey" });
            DropIndex("dbo.EmailVariables", new[] { "UId" });
            DropIndex("dbo.EmailTemplateVariables", new[] { "UId" });
            DropIndex("dbo.TemplateEmailAreaVariables", new[] { "EmailTemplateAreaSortingId" });
            DropIndex("dbo.EmailTemplateAreas", new[] { "UId" });
            DropIndex("dbo.ContactData", new[] { "UId" });
            DropIndex("dbo.Contacts", new[] { "SavedList_UId" });
            DropIndex("dbo.EmailAreaVariables", new[] { "EmailAreaSortingId" });
            DropIndex("dbo.EmailAreas", new[] { "UId" });
            DropIndex("dbo.RSEmails", new[] { "EmailListKey" });
            DropIndex("dbo.RSEmails", new[] { "EmailCampaignKey" });
            DropIndex("dbo.RSEmails", new[] { "FormKey" });
            DropIndex("dbo.RSEmails", new[] { "EmailTemplateKey" });
            DropIndex("dbo.CustomTexts", new[] { "EmailCampaignKey" });
            DropIndex("dbo.CustomTexts", new[] { "FormKey" });
            DropIndex("dbo.EmailCampaigns", new[] { "TypeKey" });
            DropIndex("dbo.EmailCampaigns", new[] { "CompanyKey" });
            DropIndex("dbo.Forms", new[] { "InvitationListKey" });
            DropIndex("dbo.Forms", new[] { "CompanyKey" });
            DropIndex("dbo.Forms", new[] { "TypeKey" });
            DropIndex("dbo.Forms", new[] { "FormTemplateKey" });
            DropIndex("dbo.Forms", new[] { "MerchantAccountKey" });
            DropIndex("dbo.LogicCommands", new[] { "FormKey" });
            DropIndex("dbo.LogicCommands", new[] { "LogicKey" });
            DropIndex("dbo.Logics", new[] { "LogicBlockKey" });
            DropIndex("dbo.Logics", new[] { "ComponentKey" });
            DropIndex("dbo.Logics", new[] { "PanelKey" });
            DropIndex("dbo.Logics", new[] { "PageKey" });
            DropIndex("dbo.ComponentBases", new[] { "RadioGroupKey" });
            DropIndex("dbo.ComponentBases", new[] { "DropdownGroupKey" });
            DropIndex("dbo.ComponentBases", new[] { "CheckboxGroupKey" });
            DropIndex("dbo.ComponentBases", new[] { "PanelKey" });
            DropIndex("dbo.ComponentBases", new[] { "SeatingKey" });
            DropIndex("dbo.Audiences", new[] { "FormKey" });
            DropTable("dbo.PricesAudiences");
            DropTable("dbo.PanelAudiences");
            DropTable("dbo.PageAudiences");
            DropTable("dbo.CustomGroupUsers");
            DropTable("dbo.TagForms");
            DropTable("dbo.TagEmailCampaigns");
            DropTable("dbo.ContactEmailLists");
            DropTable("dbo.ComponentBaseAudiences");
            DropTable("dbo.Stylesheets");
            DropTable("dbo.SmtpServers");
            DropTable("dbo.AspNetRoles");
            DropTable("dbo.Errors");
            DropTable("dbo.ErrorReviews");
            DropTable("dbo.EmailEvents");
            DropTable("dbo.DatabaseAccess");
            DropTable("dbo.CSS");
            DropTable("dbo.Constants");
            DropTable("dbo.TransactionDetails");
            DropTable("dbo.CreditCardAccess");
            DropTable("dbo.CreditCards");
            DropTable("dbo.TransactionRequests");
            DropTable("dbo.OldRegistrantData");
            DropTable("dbo.OldRegistrants");
            DropTable("dbo.RegistrantData");
            DropTable("dbo.Registrants");
            DropTable("dbo.ComponentStyle");
            DropTable("dbo.PriceStyle");
            DropTable("dbo.Price");
            DropTable("dbo.Prices");
            DropTable("dbo.PriceGroups");
            DropTable("dbo.LogicStatements");
            DropTable("dbo.LogicGroups");
            DropTable("dbo.Variables");
            DropTable("dbo.FormTemplates");
            DropTable("dbo.SeatingStyle");
            DropTable("dbo.Seaters");
            DropTable("dbo.Seatings");
            DropTable("dbo.Panels");
            DropTable("dbo.Pages");
            DropTable("dbo.MerchantAccountInfo");
            DropTable("dbo.LogicBlocks");
            DropTable("dbo.FormStyles");
            DropTable("dbo.DefaultComponentOrders");
            DropTable("dbo.AspNetUserRoles");
            DropTable("dbo.AspNetUserLogins");
            DropTable("dbo.CustomGroups");
            DropTable("dbo.AspNetUserClaims");
            DropTable("dbo.AspNetUsers");
            DropTable("dbo.SavedLists");
            DropTable("dbo.SqlTables");
            DropTable("dbo.SqlHeaders");
            DropTable("dbo.ReportHeaders");
            DropTable("dbo.Reports");
            DropTable("dbo.DefaultFormStyles");
            DropTable("dbo.Pointers");
            DropTable("dbo.Folders");
            DropTable("dbo.NodeTypes");
            DropTable("dbo.Tags");
            DropTable("dbo.EmailVariables");
            DropTable("dbo.EmailTemplateVariables");
            DropTable("dbo.TemplateEmailAreaVariables");
            DropTable("dbo.EmailTemplateAreas");
            DropTable("dbo.EmailTemplates");
            DropTable("dbo.ContactData");
            DropTable("dbo.Contacts");
            DropTable("dbo.EmailLists");
            DropTable("dbo.EmailAreaVariables");
            DropTable("dbo.EmailAreas");
            DropTable("dbo.RSEmails");
            DropTable("dbo.CustomTexts");
            DropTable("dbo.EmailCampaigns");
            DropTable("dbo.Companies");
            DropTable("dbo.Forms");
            DropTable("dbo.LogicCommands");
            DropTable("dbo.Logics");
            DropTable("dbo.ComponentBases");
            DropTable("dbo.Audiences");
        }
    }
}
