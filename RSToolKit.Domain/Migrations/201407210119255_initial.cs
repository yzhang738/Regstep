namespace RSToolKit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class initial : DbMigration
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
                        FormUId = c.Guid(nullable: false),
                        Order = c.Int(nullable: false),
                        Label = c.String(maxLength: 100),
                    })
                .PrimaryKey(t => t.UId)
                .ForeignKey("dbo.Forms", t => t.FormUId, cascadeDelete: true)
                .Index(t => t.FormUId);
            
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
                        Company = c.Guid(nullable: false),
                        Description = c.String(maxLength: 1000),
                        InvitationTable = c.Guid(nullable: false),
                        MerchantAccountUId = c.Guid(),
                        LastCode = c.String(),
                        Status = c.Int(nullable: false),
                        Currency = c.Int(nullable: false),
                        TypeUId = c.Guid(),
                        Question = c.String(maxLength: 250),
                        Answer = c.String(maxLength: 250),
                        Approval = c.Boolean(nullable: false),
                        Price = c.Decimal(precision: 18, scale: 2),
                        Open = c.DateTimeOffset(nullable: false, precision: 7),
                        Close = c.DateTimeOffset(nullable: false, precision: 7),
                        Year = c.Int(nullable: false),
                        Style = c.String(),
                        Header = c.String(),
                        Footer = c.String(),
                        CultureString = c.String(),
                        AccessType = c.Int(nullable: false),
                        Editable = c.Boolean(nullable: false),
                        Cancelable = c.Boolean(nullable: false),
                        BillingOption = c.Int(nullable: false),
                        FormTemplateUId = c.Guid(),
                    })
                .PrimaryKey(t => t.UId)
                .ForeignKey("dbo.NodeTypes", t => t.TypeUId)
                .ForeignKey("dbo.MerchantAccountInfo", t => t.MerchantAccountUId)
                .ForeignKey("dbo.FormTemplates", t => t.FormTemplateUId)
                .Index(t => t.MerchantAccountUId)
                .Index(t => t.TypeUId)
                .Index(t => t.FormTemplateUId);
            
            CreateTable(
                "dbo.CustomTexts",
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
                        Variable = c.String(maxLength: 100),
                        ParentUId = c.Guid(nullable: false),
                        Text = c.String(),
                        FormUId = c.Guid(),
                        EmailCampaignUId = c.Guid(),
                    })
                .PrimaryKey(t => t.UId)
                .ForeignKey("dbo.EmailCampaigns", t => t.EmailCampaignUId)
                .ForeignKey("dbo.Forms", t => t.FormUId)
                .Index(t => t.FormUId)
                .Index(t => t.EmailCampaignUId);
            
            CreateTable(
                "dbo.EmailCampaigns",
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
                        TypeUId = c.Guid(),
                        Description = c.String(),
                    })
                .PrimaryKey(t => t.UId)
                .ForeignKey("dbo.NodeTypes", t => t.TypeUId)
                .Index(t => t.TypeUId);
            
            CreateTable(
                "dbo.RSEmails",
                c => new
                    {
                        UId = c.Guid(nullable: false),
                        EmailType = c.Int(nullable: false),
                        SendTime = c.DateTimeOffset(precision: 7),
                        IntervalTicks = c.Long(nullable: false),
                        MaxSends = c.Int(nullable: false),
                        RepeatSending = c.Boolean(nullable: false),
                        Subject = c.String(maxLength: 500),
                        FormUId = c.Guid(),
                        EmailCampaignUId = c.Guid(),
                        Description = c.String(),
                        EmailTemplateUId = c.Guid(),
                        CC = c.String(),
                        BCC = c.String(),
                        EmailListUId = c.Guid(),
                        From = c.String(),
                        To = c.String(),
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
                .ForeignKey("dbo.EmailCampaigns", t => t.EmailCampaignUId)
                .ForeignKey("dbo.EmailLists", t => t.EmailListUId)
                .ForeignKey("dbo.Forms", t => t.FormUId)
                .ForeignKey("dbo.EmailTemplates", t => t.EmailTemplateUId)
                .Index(t => t.FormUId)
                .Index(t => t.EmailCampaignUId)
                .Index(t => t.EmailTemplateUId)
                .Index(t => t.EmailListUId);
            
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
                .ForeignKey("dbo.RSEmails", t => t.UId, cascadeDelete: true)
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
                .ForeignKey("dbo.EmailAreas", t => t.EmailAreaSortingId, cascadeDelete: true)
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
                .PrimaryKey(t => t.UId);
            
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
                .PrimaryKey(t => t.UId)
                .ForeignKey("dbo.SavedLists", t => t.SavedList_UId)
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
                .ForeignKey("dbo.Contacts", t => t.UId, cascadeDelete: true)
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
                .PrimaryKey(t => t.UId);
            
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
                .ForeignKey("dbo.EmailTemplates", t => t.UId, cascadeDelete: true)
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
                .ForeignKey("dbo.EmailTemplateAreas", t => t.EmailTemplateAreaSortingId, cascadeDelete: true)
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
                .ForeignKey("dbo.EmailTemplates", t => t.UId, cascadeDelete: true)
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
                .ForeignKey("dbo.RSEmails", t => t.UId, cascadeDelete: true)
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
                    })
                .PrimaryKey(t => t.UId)
                .ForeignKey("dbo.Companies", t => t.CompanyUId, cascadeDelete: true)
                .Index(t => t.CompanyUId);
            
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
                .PrimaryKey(t => t.UId);
            
            CreateTable(
                "dbo.NodeTypes",
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
                    })
                .PrimaryKey(t => t.UId)
                .ForeignKey("dbo.Companies", t => t.CompanyUId, cascadeDelete: true)
                .Index(t => t.CompanyUId);
            
            CreateTable(
                "dbo.DefaultComponentOrders",
                c => new
                    {
                        SortingId = c.Long(nullable: false, identity: true),
                        UId = c.Guid(nullable: false),
                        ComponentType = c.String(),
                        Order = c.String(),
                    })
                .PrimaryKey(t => t.SortingId)
                .ForeignKey("dbo.Forms", t => t.UId, cascadeDelete: true)
                .Index(t => t.UId);
            
            CreateTable(
                "dbo.FormStyles",
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
                        Variable = c.String(),
                        Value = c.String(),
                        GroupName = c.String(),
                        FormUId = c.Guid(nullable: false),
                        Sort = c.String(),
                        Type = c.String(),
                        SubSort = c.String(),
                    })
                .PrimaryKey(t => t.UId)
                .ForeignKey("dbo.Forms", t => t.FormUId, cascadeDelete: true)
                .Index(t => t.FormUId);
            
            CreateTable(
                "dbo.LogicBlocks",
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
                        FormUId = c.Guid(),
                    })
                .PrimaryKey(t => t.UId)
                .ForeignKey("dbo.Forms", t => t.FormUId)
                .Index(t => t.FormUId);
            
            CreateTable(
                "dbo.Logics",
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
                        LogicStatement = c.String(),
                        Then = c.String(),
                        Else = c.String(),
                        Description = c.String(maxLength: 1000),
                        Order = c.Int(nullable: false),
                        ParentUId = c.Guid(nullable: false),
                        Incoming = c.Boolean(nullable: false),
                        LogicBlock_UId = c.Guid(),
                        ComponentBase_UId = c.Guid(),
                    })
                .PrimaryKey(t => t.UId)
                .ForeignKey("dbo.LogicBlocks", t => t.LogicBlock_UId)
                .ForeignKey("dbo.ComponentBases", t => t.ComponentBase_UId)
                .Index(t => t.LogicBlock_UId)
                .Index(t => t.ComponentBase_UId);
            
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
                        Description = c.String(maxLength: 1000),
                        RSVP = c.Int(nullable: false),
                        RawAudiences = c.String(),
                        AdminOnly = c.Boolean(nullable: false),
                        PageNumber = c.Int(nullable: false),
                        FormUId = c.Guid(nullable: false),
                        Display = c.Boolean(nullable: false),
                        Enabled = c.Boolean(nullable: false),
                        Locked = c.Boolean(nullable: false),
                        Type = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.UId)
                .ForeignKey("dbo.Forms", t => t.FormUId, cascadeDelete: true)
                .Index(t => t.FormUId);
            
            CreateTable(
                "dbo.Panels",
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
                        Description = c.String(maxLength: 1000),
                        RSVP = c.Int(nullable: false),
                        RawAudiences = c.String(),
                        AdminOnly = c.Boolean(nullable: false),
                        Order = c.Int(nullable: false),
                        PageUId = c.Guid(nullable: false),
                        Display = c.Boolean(nullable: false),
                        Enabled = c.Boolean(nullable: false),
                        Locked = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.UId)
                .ForeignKey("dbo.Pages", t => t.PageUId, cascadeDelete: true)
                .Index(t => t.PageUId);
            
            CreateTable(
                "dbo.ComponentBases",
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
                        SeatingUId = c.Guid(),
                        RawAudiences = c.String(),
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
                        DisplayOrder = c.String(),
                        DisplayAgendaDate = c.Boolean(nullable: false),
                        Required = c.Boolean(nullable: false),
                        Locked = c.Boolean(nullable: false),
                        PanelUId = c.Guid(),
                        CheckboxGroupUId = c.Guid(),
                        TimeExclusion = c.Boolean(),
                        DialogText = c.String(maxLength: 1000),
                        ItemsPerRow = c.Int(),
                        DropdownGroupUId = c.Guid(),
                        RadioGroupUId = c.Guid(),
                        ItemsPerRow1 = c.Int(),
                        Style = c.Int(),
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
                        Discriminator = c.String(nullable: false, maxLength: 128),
                        Price_UId = c.Guid(),
                    })
                .PrimaryKey(t => t.UId)
                .ForeignKey("dbo.Panels", t => t.PanelUId, cascadeDelete: true)
                .ForeignKey("dbo.PriceGroups", t => t.Price_UId)
                .ForeignKey("dbo.Seatings", t => t.SeatingUId)
                .ForeignKey("dbo.ComponentBases", t => t.CheckboxGroupUId)
                .ForeignKey("dbo.ComponentBases", t => t.DropdownGroupUId)
                .ForeignKey("dbo.ComponentBases", t => t.RadioGroupUId)
                .Index(t => t.SeatingUId)
                .Index(t => t.PanelUId)
                .Index(t => t.CheckboxGroupUId)
                .Index(t => t.DropdownGroupUId)
                .Index(t => t.RadioGroupUId)
                .Index(t => t.Price_UId);
            
            CreateTable(
                "dbo.PriceGroups",
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
                "dbo.Prices",
                c => new
                    {
                        UId = c.Guid(nullable: false),
                        PriceGroupUId = c.Guid(nullable: false),
                        RawAudiences = c.String(),
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
                .ForeignKey("dbo.PriceGroups", t => t.PriceGroupUId, cascadeDelete: true)
                .Index(t => t.PriceGroupUId);
            
            CreateTable(
                "dbo.Price",
                c => new
                    {
                        UId = c.Guid(nullable: false),
                        PricesUId = c.Guid(nullable: false),
                        Ammount = c.Decimal(precision: 18, scale: 2),
                        Start = c.DateTimeOffset(nullable: false, precision: 7),
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
                .ForeignKey("dbo.Prices", t => t.PricesUId, cascadeDelete: true)
                .Index(t => t.PricesUId);
            
            CreateTable(
                "dbo.PriceStyle",
                c => new
                    {
                        UId = c.Guid(nullable: false),
                        PriceGroupUId = c.Guid(nullable: false),
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
                .ForeignKey("dbo.PriceGroups", t => t.PriceGroupUId, cascadeDelete: true)
                .Index(t => t.PriceGroupUId);
            
            CreateTable(
                "dbo.Seatings",
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
                        FormUId = c.Guid(nullable: false),
                        RawAudiences = c.String(),
                        SeatinType = c.Int(nullable: false),
                        Start = c.DateTimeOffset(nullable: false, precision: 7),
                        End = c.DateTimeOffset(nullable: false, precision: 7),
                        MaxSeats = c.Int(nullable: false),
                        Waitlistable = c.Boolean(nullable: false),
                        MultipleSeats = c.Boolean(nullable: false),
                        FullLabel = c.String(maxLength: 1000),
                        WaitlistLabel = c.String(maxLength: 1000),
                        WaitlistItemLabel = c.String(maxLength: 1000),
                    })
                .PrimaryKey(t => t.UId)
                .ForeignKey("dbo.Forms", t => t.FormUId, cascadeDelete: true)
                .Index(t => t.FormUId);
            
            CreateTable(
                "dbo.ComponentStyle",
                c => new
                    {
                        UId = c.Guid(nullable: false),
                        ComponentUId = c.Guid(nullable: false),
                        SortingId = c.Long(nullable: false, identity: true),
                        Name = c.String(),
                        Value = c.String(),
                        Variable = c.String(),
                        GroupName = c.String(),
                        Sort = c.String(),
                        SubSort = c.String(),
                        Type = c.String(),
                        ComponentBase_UId = c.Guid(),
                    })
                .PrimaryKey(t => t.UId)
                .ForeignKey("dbo.ComponentBases", t => t.ComponentUId, cascadeDelete: true)
                .ForeignKey("dbo.ComponentBases", t => t.ComponentBase_UId)
                .Index(t => t.ComponentUId)
                .Index(t => t.ComponentBase_UId);
            
            CreateTable(
                "dbo.Seaters",
                c => new
                    {
                        UId = c.Guid(nullable: false),
                        SortingId = c.Long(nullable: false, identity: true),
                        SeatingUId = c.Guid(nullable: false),
                        Seated = c.Boolean(nullable: false),
                        Confirmation = c.String(),
                        Component = c.Guid(nullable: false),
                        Date = c.DateTimeOffset(nullable: false, precision: 7),
                        DateSeated = c.DateTimeOffset(nullable: false, precision: 7),
                    })
                .PrimaryKey(t => t.UId)
                .ForeignKey("dbo.Seatings", t => t.SeatingUId, cascadeDelete: true)
                .Index(t => t.SeatingUId);
            
            CreateTable(
                "dbo.SeatingStyle",
                c => new
                    {
                        UId = c.Guid(nullable: false),
                        SeatingUId = c.Guid(nullable: false),
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
                .ForeignKey("dbo.Seatings", t => t.SeatingUId, cascadeDelete: true)
                .Index(t => t.SeatingUId);
            
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
                        SortingId = c.Long(nullable: false, identity: true),
                        FormUId = c.Guid(nullable: false),
                        Value = c.String(nullable: false, maxLength: 150),
                    })
                .PrimaryKey(t => t.UId)
                .ForeignKey("dbo.Forms", t => t.FormUId, cascadeDelete: true)
                .Index(t => t.FormUId);
            
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
                .ForeignKey("dbo.CreditCards", t => t.CreditCardUId, cascadeDelete: true)
                .ForeignKey("dbo.MerchantAccountInfo", t => t.MerchantAccount_UId)
                .ForeignKey("dbo.Registrants", t => t.RegistrantUId)
                .Index(t => t.RegistrantUId)
                .Index(t => t.CreditCardUId)
                .Index(t => t.MerchantAccount_UId);
            
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
                .ForeignKey("dbo.CreditCards", t => t.CreditCardUId, cascadeDelete: true)
                .ForeignKey("dbo.TransactionRequests", t => t.RequestUId)
                .Index(t => t.RequestUId)
                .Index(t => t.CreditCardUId);
            
            CreateTable(
                "dbo.Registrants",
                c => new
                    {
                        UId = c.Guid(nullable: false),
                        AudienceGuid = c.Guid(nullable: false),
                        FormUId = c.Guid(nullable: false),
                        RSVP = c.Boolean(nullable: false),
                        Email = c.String(nullable: false, maxLength: 250),
                        Confirmation = c.String(nullable: false, maxLength: 50),
                        Status = c.Int(nullable: false),
                        Type = c.Int(nullable: false),
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
                .PrimaryKey(t => t.UId)
                .ForeignKey("dbo.Forms", t => t.FormUId, cascadeDelete: true)
                .Index(t => t.FormUId);
            
            CreateTable(
                "dbo.RegistrantData",
                c => new
                    {
                        SortingId = c.Long(nullable: false, identity: true),
                        UId = c.Guid(nullable: false),
                        Variable = c.String(nullable: false, maxLength: 250),
                        VariableUId = c.Guid(nullable: false),
                        Descriminator = c.String(nullable: false, maxLength: 250),
                        Value = c.String(nullable: false, maxLength: 1000),
                    })
                .PrimaryKey(t => t.SortingId)
                .ForeignKey("dbo.Registrants", t => t.UId, cascadeDelete: true)
                .Index(t => t.UId);
            
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
                "dbo.DefaultFormStyles",
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
                        Company = c.Guid(),
                        Variable = c.String(),
                        Value = c.String(),
                        GroupName = c.String(),
                        Sort = c.String(),
                        Type = c.String(),
                        SubSort = c.String(),
                    })
                .PrimaryKey(t => t.UId);
            
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
                .ForeignKey("dbo.Errors", t => t.ErrorUId, cascadeDelete: true)
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
                "dbo.Folders",
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
                        ParentUId = c.Guid(),
                    })
                .PrimaryKey(t => t.UId)
                .ForeignKey("dbo.Folders", t => t.ParentUId)
                .Index(t => t.ParentUId);
            
            CreateTable(
                "dbo.Pointers",
                c => new
                    {
                        SortingId = c.Long(nullable: false, identity: true),
                        FolderUId = c.Guid(),
                        Descriminator = c.String(maxLength: 250),
                        Target = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.SortingId)
                .ForeignKey("dbo.Folders", t => t.FolderUId)
                .Index(t => t.FolderUId);
            
            CreateTable(
                "dbo.OldRegistrantData",
                c => new
                    {
                        SortingId = c.Long(nullable: false, identity: true),
                        UId = c.Guid(nullable: false),
                        Variable = c.String(nullable: false, maxLength: 250),
                        VariableUId = c.Guid(nullable: false),
                        Descriminator = c.String(nullable: false, maxLength: 250),
                        Value = c.String(nullable: false, maxLength: 1000),
                    })
                .PrimaryKey(t => t.SortingId)
                .ForeignKey("dbo.OldRegistrants", t => t.UId, cascadeDelete: true)
                .Index(t => t.UId);
            
            CreateTable(
                "dbo.OldRegistrants",
                c => new
                    {
                        UId = c.Guid(nullable: false),
                        AudienceGuid = c.Guid(nullable: false),
                        FormUId = c.Guid(nullable: false),
                        RSVP = c.Boolean(nullable: false),
                        Email = c.String(nullable: false, maxLength: 250),
                        Confirmation = c.String(nullable: false, maxLength: 50),
                        Status = c.Int(nullable: false),
                        Type = c.Int(nullable: false),
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
                .PrimaryKey(t => t.UId)
                .ForeignKey("dbo.Forms", t => t.FormUId, cascadeDelete: true)
                .Index(t => t.FormUId);
            
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
                .PrimaryKey(t => t.UId)
                .ForeignKey("dbo.Reports", t => t.ReportUId, cascadeDelete: true)
                .Index(t => t.ReportUId);
            
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
                    })
                .PrimaryKey(t => t.UId);
            
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
                .PrimaryKey(t => t.UId)
                .ForeignKey("dbo.Reports", t => t.ReportUId, cascadeDelete: true)
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
                .PrimaryKey(t => t.UId)
                .ForeignKey("dbo.ReportHeaders", t => t.ReportHeaderUId)
                .ForeignKey("dbo.SqlTables", t => t.SqlTableUId)
                .Index(t => t.SqlTableUId)
                .Index(t => t.ReportHeaderUId);
            
            CreateTable(
                "dbo.SavedLists",
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
                        Description = c.String(maxLength: 1000),
                    })
                .PrimaryKey(t => t.UId);
            
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
                "dbo.ContactEmailLists",
                c => new
                    {
                        Contact_UId = c.Guid(nullable: false),
                        EmailList_UId = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => new { t.Contact_UId, t.EmailList_UId })
                .ForeignKey("dbo.Contacts", t => t.Contact_UId, cascadeDelete: true)
                .ForeignKey("dbo.EmailLists", t => t.EmailList_UId, cascadeDelete: true)
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
                .ForeignKey("dbo.Tags", t => t.Tag_UId, cascadeDelete: true)
                .ForeignKey("dbo.EmailCampaigns", t => t.EmailCampaign_UId, cascadeDelete: true)
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
                .ForeignKey("dbo.Tags", t => t.Tag_UId, cascadeDelete: true)
                .ForeignKey("dbo.Forms", t => t.Form_UId, cascadeDelete: true)
                .Index(t => t.Tag_UId)
                .Index(t => t.Form_UId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.CSS", "StylesheetUId", "dbo.Stylesheets");
            DropForeignKey("dbo.Contacts", "SavedList_UId", "dbo.SavedLists");
            DropForeignKey("dbo.SqlTables", "ReportUId", "dbo.Reports");
            DropForeignKey("dbo.SqlHeaders", "SqlTableUId", "dbo.SqlTables");
            DropForeignKey("dbo.SqlHeaders", "ReportHeaderUId", "dbo.ReportHeaders");
            DropForeignKey("dbo.ReportHeaders", "ReportUId", "dbo.Reports");
            DropForeignKey("dbo.OldRegistrants", "FormUId", "dbo.Forms");
            DropForeignKey("dbo.OldRegistrantData", "UId", "dbo.OldRegistrants");
            DropForeignKey("dbo.Pointers", "FolderUId", "dbo.Folders");
            DropForeignKey("dbo.Folders", "ParentUId", "dbo.Folders");
            DropForeignKey("dbo.ErrorReviews", "ErrorUId", "dbo.Errors");
            DropForeignKey("dbo.TransactionRequests", "RegistrantUId", "dbo.Registrants");
            DropForeignKey("dbo.Registrants", "FormUId", "dbo.Forms");
            DropForeignKey("dbo.RegistrantData", "UId", "dbo.Registrants");
            DropForeignKey("dbo.TransactionRequests", "MerchantAccount_UId", "dbo.MerchantAccountInfo");
            DropForeignKey("dbo.TransactionDetails", "RequestUId", "dbo.TransactionRequests");
            DropForeignKey("dbo.TransactionDetails", "CreditCardUId", "dbo.CreditCards");
            DropForeignKey("dbo.TransactionRequests", "CreditCardUId", "dbo.CreditCards");
            DropForeignKey("dbo.CreditCardAccess", "CreditCardUId", "dbo.CreditCards");
            DropForeignKey("dbo.Variables", "FormUId", "dbo.Forms");
            DropForeignKey("dbo.Forms", "FormTemplateUId", "dbo.FormTemplates");
            DropForeignKey("dbo.Panels", "PageUId", "dbo.Pages");
            DropForeignKey("dbo.SeatingStyle", "SeatingUId", "dbo.Seatings");
            DropForeignKey("dbo.Seaters", "SeatingUId", "dbo.Seatings");
            DropForeignKey("dbo.Seatings", "FormUId", "dbo.Forms");
            DropForeignKey("dbo.ComponentBases", "RadioGroupUId", "dbo.ComponentBases");
            DropForeignKey("dbo.ComponentBases", "DropdownGroupUId", "dbo.ComponentBases");
            DropForeignKey("dbo.ComponentBases", "CheckboxGroupUId", "dbo.ComponentBases");
            DropForeignKey("dbo.ComponentStyle", "ComponentBase_UId", "dbo.ComponentBases");
            DropForeignKey("dbo.ComponentStyle", "ComponentUId", "dbo.ComponentBases");
            DropForeignKey("dbo.ComponentBases", "SeatingUId", "dbo.Seatings");
            DropForeignKey("dbo.ComponentBases", "Price_UId", "dbo.PriceGroups");
            DropForeignKey("dbo.Logics", "ComponentBase_UId", "dbo.ComponentBases");
            DropForeignKey("dbo.PriceStyle", "PriceGroupUId", "dbo.PriceGroups");
            DropForeignKey("dbo.Prices", "PriceGroupUId", "dbo.PriceGroups");
            DropForeignKey("dbo.Price", "PricesUId", "dbo.Prices");
            DropForeignKey("dbo.ComponentBases", "PanelUId", "dbo.Panels");
            DropForeignKey("dbo.Pages", "FormUId", "dbo.Forms");
            DropForeignKey("dbo.Forms", "MerchantAccountUId", "dbo.MerchantAccountInfo");
            DropForeignKey("dbo.Logics", "LogicBlock_UId", "dbo.LogicBlocks");
            DropForeignKey("dbo.LogicBlocks", "FormUId", "dbo.Forms");
            DropForeignKey("dbo.FormStyles", "FormUId", "dbo.Forms");
            DropForeignKey("dbo.DefaultComponentOrders", "UId", "dbo.Forms");
            DropForeignKey("dbo.CustomTexts", "FormUId", "dbo.Forms");
            DropForeignKey("dbo.TagForms", "Form_UId", "dbo.Forms");
            DropForeignKey("dbo.TagForms", "Tag_UId", "dbo.Tags");
            DropForeignKey("dbo.TagEmailCampaigns", "EmailCampaign_UId", "dbo.EmailCampaigns");
            DropForeignKey("dbo.TagEmailCampaigns", "Tag_UId", "dbo.Tags");
            DropForeignKey("dbo.Forms", "TypeUId", "dbo.NodeTypes");
            DropForeignKey("dbo.EmailCampaigns", "TypeUId", "dbo.NodeTypes");
            DropForeignKey("dbo.NodeTypes", "CompanyUId", "dbo.Companies");
            DropForeignKey("dbo.Tags", "CompanyUId", "dbo.Companies");
            DropForeignKey("dbo.EmailVariables", "UId", "dbo.RSEmails");
            DropForeignKey("dbo.RSEmails", "EmailTemplateUId", "dbo.EmailTemplates");
            DropForeignKey("dbo.EmailTemplateVariables", "UId", "dbo.EmailTemplates");
            DropForeignKey("dbo.EmailTemplateAreas", "UId", "dbo.EmailTemplates");
            DropForeignKey("dbo.TemplateEmailAreaVariables", "EmailTemplateAreaSortingId", "dbo.EmailTemplateAreas");
            DropForeignKey("dbo.RSEmails", "FormUId", "dbo.Forms");
            DropForeignKey("dbo.RSEmails", "EmailListUId", "dbo.EmailLists");
            DropForeignKey("dbo.ContactEmailLists", "EmailList_UId", "dbo.EmailLists");
            DropForeignKey("dbo.ContactEmailLists", "Contact_UId", "dbo.Contacts");
            DropForeignKey("dbo.ContactData", "UId", "dbo.Contacts");
            DropForeignKey("dbo.RSEmails", "EmailCampaignUId", "dbo.EmailCampaigns");
            DropForeignKey("dbo.EmailAreas", "UId", "dbo.RSEmails");
            DropForeignKey("dbo.EmailAreaVariables", "EmailAreaSortingId", "dbo.EmailAreas");
            DropForeignKey("dbo.CustomTexts", "EmailCampaignUId", "dbo.EmailCampaigns");
            DropForeignKey("dbo.Audiences", "FormUId", "dbo.Forms");
            DropIndex("dbo.TagForms", new[] { "Form_UId" });
            DropIndex("dbo.TagForms", new[] { "Tag_UId" });
            DropIndex("dbo.TagEmailCampaigns", new[] { "EmailCampaign_UId" });
            DropIndex("dbo.TagEmailCampaigns", new[] { "Tag_UId" });
            DropIndex("dbo.ContactEmailLists", new[] { "EmailList_UId" });
            DropIndex("dbo.ContactEmailLists", new[] { "Contact_UId" });
            DropIndex("dbo.SqlHeaders", new[] { "ReportHeaderUId" });
            DropIndex("dbo.SqlHeaders", new[] { "SqlTableUId" });
            DropIndex("dbo.SqlTables", new[] { "ReportUId" });
            DropIndex("dbo.ReportHeaders", new[] { "ReportUId" });
            DropIndex("dbo.OldRegistrants", new[] { "FormUId" });
            DropIndex("dbo.OldRegistrantData", new[] { "UId" });
            DropIndex("dbo.Pointers", new[] { "FolderUId" });
            DropIndex("dbo.Folders", new[] { "ParentUId" });
            DropIndex("dbo.ErrorReviews", new[] { "ErrorUId" });
            DropIndex("dbo.CSS", new[] { "StylesheetUId" });
            DropIndex("dbo.RegistrantData", new[] { "UId" });
            DropIndex("dbo.Registrants", new[] { "FormUId" });
            DropIndex("dbo.TransactionDetails", new[] { "CreditCardUId" });
            DropIndex("dbo.TransactionDetails", new[] { "RequestUId" });
            DropIndex("dbo.TransactionRequests", new[] { "MerchantAccount_UId" });
            DropIndex("dbo.TransactionRequests", new[] { "CreditCardUId" });
            DropIndex("dbo.TransactionRequests", new[] { "RegistrantUId" });
            DropIndex("dbo.CreditCardAccess", new[] { "CreditCardUId" });
            DropIndex("dbo.Variables", new[] { "FormUId" });
            DropIndex("dbo.SeatingStyle", new[] { "SeatingUId" });
            DropIndex("dbo.Seaters", new[] { "SeatingUId" });
            DropIndex("dbo.ComponentStyle", new[] { "ComponentBase_UId" });
            DropIndex("dbo.ComponentStyle", new[] { "ComponentUId" });
            DropIndex("dbo.Seatings", new[] { "FormUId" });
            DropIndex("dbo.PriceStyle", new[] { "PriceGroupUId" });
            DropIndex("dbo.Price", new[] { "PricesUId" });
            DropIndex("dbo.Prices", new[] { "PriceGroupUId" });
            DropIndex("dbo.ComponentBases", new[] { "Price_UId" });
            DropIndex("dbo.ComponentBases", new[] { "RadioGroupUId" });
            DropIndex("dbo.ComponentBases", new[] { "DropdownGroupUId" });
            DropIndex("dbo.ComponentBases", new[] { "CheckboxGroupUId" });
            DropIndex("dbo.ComponentBases", new[] { "PanelUId" });
            DropIndex("dbo.ComponentBases", new[] { "SeatingUId" });
            DropIndex("dbo.Panels", new[] { "PageUId" });
            DropIndex("dbo.Pages", new[] { "FormUId" });
            DropIndex("dbo.Logics", new[] { "ComponentBase_UId" });
            DropIndex("dbo.Logics", new[] { "LogicBlock_UId" });
            DropIndex("dbo.LogicBlocks", new[] { "FormUId" });
            DropIndex("dbo.FormStyles", new[] { "FormUId" });
            DropIndex("dbo.DefaultComponentOrders", new[] { "UId" });
            DropIndex("dbo.NodeTypes", new[] { "CompanyUId" });
            DropIndex("dbo.Tags", new[] { "CompanyUId" });
            DropIndex("dbo.EmailVariables", new[] { "UId" });
            DropIndex("dbo.EmailTemplateVariables", new[] { "UId" });
            DropIndex("dbo.TemplateEmailAreaVariables", new[] { "EmailTemplateAreaSortingId" });
            DropIndex("dbo.EmailTemplateAreas", new[] { "UId" });
            DropIndex("dbo.ContactData", new[] { "UId" });
            DropIndex("dbo.Contacts", new[] { "SavedList_UId" });
            DropIndex("dbo.EmailAreaVariables", new[] { "EmailAreaSortingId" });
            DropIndex("dbo.EmailAreas", new[] { "UId" });
            DropIndex("dbo.RSEmails", new[] { "EmailListUId" });
            DropIndex("dbo.RSEmails", new[] { "EmailTemplateUId" });
            DropIndex("dbo.RSEmails", new[] { "EmailCampaignUId" });
            DropIndex("dbo.RSEmails", new[] { "FormUId" });
            DropIndex("dbo.EmailCampaigns", new[] { "TypeUId" });
            DropIndex("dbo.CustomTexts", new[] { "EmailCampaignUId" });
            DropIndex("dbo.CustomTexts", new[] { "FormUId" });
            DropIndex("dbo.Forms", new[] { "FormTemplateUId" });
            DropIndex("dbo.Forms", new[] { "TypeUId" });
            DropIndex("dbo.Forms", new[] { "MerchantAccountUId" });
            DropIndex("dbo.Audiences", new[] { "FormUId" });
            DropTable("dbo.TagForms");
            DropTable("dbo.TagEmailCampaigns");
            DropTable("dbo.ContactEmailLists");
            DropTable("dbo.Stylesheets");
            DropTable("dbo.SmtpServers");
            DropTable("dbo.SavedLists");
            DropTable("dbo.SqlHeaders");
            DropTable("dbo.SqlTables");
            DropTable("dbo.Reports");
            DropTable("dbo.ReportHeaders");
            DropTable("dbo.OldRegistrants");
            DropTable("dbo.OldRegistrantData");
            DropTable("dbo.Pointers");
            DropTable("dbo.Folders");
            DropTable("dbo.Errors");
            DropTable("dbo.ErrorReviews");
            DropTable("dbo.EmailEvents");
            DropTable("dbo.DefaultFormStyles");
            DropTable("dbo.CSS");
            DropTable("dbo.RegistrantData");
            DropTable("dbo.Registrants");
            DropTable("dbo.TransactionDetails");
            DropTable("dbo.TransactionRequests");
            DropTable("dbo.CreditCards");
            DropTable("dbo.CreditCardAccess");
            DropTable("dbo.Constants");
            DropTable("dbo.Variables");
            DropTable("dbo.FormTemplates");
            DropTable("dbo.SeatingStyle");
            DropTable("dbo.Seaters");
            DropTable("dbo.ComponentStyle");
            DropTable("dbo.Seatings");
            DropTable("dbo.PriceStyle");
            DropTable("dbo.Price");
            DropTable("dbo.Prices");
            DropTable("dbo.PriceGroups");
            DropTable("dbo.ComponentBases");
            DropTable("dbo.Panels");
            DropTable("dbo.Pages");
            DropTable("dbo.MerchantAccountInfo");
            DropTable("dbo.Logics");
            DropTable("dbo.LogicBlocks");
            DropTable("dbo.FormStyles");
            DropTable("dbo.DefaultComponentOrders");
            DropTable("dbo.NodeTypes");
            DropTable("dbo.Companies");
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
            DropTable("dbo.EmailCampaigns");
            DropTable("dbo.CustomTexts");
            DropTable("dbo.Forms");
            DropTable("dbo.Audiences");
        }
    }
}
