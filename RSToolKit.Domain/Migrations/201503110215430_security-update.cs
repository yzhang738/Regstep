namespace RSToolKit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class securityupdate : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Reports", "CompanyKey", "dbo.Companies");
            DropForeignKey("dbo.ReportHeaders", "ReportKey", "dbo.Reports");
            DropForeignKey("dbo.SqlHeaders", "ReportHeaderKey", "dbo.ReportHeaders");
            DropForeignKey("dbo.SqlHeaders", "SqlTableKey", "dbo.SqlTables");
            DropForeignKey("dbo.SqlTables", "ReportKey", "dbo.Reports");
            DropForeignKey("dbo.QueryFilters", "ReportKey", "dbo.Reports");
            DropForeignKey("dbo.ErrorReviews", "ErrorUId", "dbo.Errors");
            DropIndex("dbo.QueryFilters", new[] { "ReportKey" });
            DropIndex("dbo.Reports", new[] { "CompanyKey" });
            DropIndex("dbo.ReportHeaders", new[] { "ReportKey" });
            DropIndex("dbo.SqlHeaders", new[] { "ReportHeaderKey" });
            DropIndex("dbo.SqlHeaders", new[] { "SqlTableKey" });
            DropIndex("dbo.SqlTables", new[] { "ReportKey" });
            DropIndex("dbo.ErrorReviews", new[] { "ErrorUId" });
            AddColumn("dbo.CreditCards", "ModifiedBy", c => c.Guid(nullable: false));
            AddColumn("dbo.CreditCards", "ModificationToken", c => c.Guid(nullable: false));
            AddColumn("dbo.CreditCards", "DateCreated", c => c.DateTimeOffset(nullable: false, precision: 7));
            AddColumn("dbo.CreditCards", "DateModified", c => c.DateTimeOffset(nullable: false, precision: 7));
            AddColumn("dbo.CreditCards", "Name", c => c.String());
            AddColumn("dbo.LogicBlocks", "EmailCampaignKey", c => c.Guid());
            CreateIndex("dbo.LogicBlocks", "EmailCampaignKey");
            CreateIndex("dbo.TransactionRequests", "CompanyKey");
            AddForeignKey("dbo.LogicBlocks", "EmailCampaignKey", "dbo.EmailCampaigns", "UId");
            AddForeignKey("dbo.TransactionRequests", "CompanyKey", "dbo.Companies", "UId");
            DropColumn("dbo.AspNetUsers", "Owner");
            DropColumn("dbo.AspNetUsers", "Group");
            DropColumn("dbo.AspNetUsers", "Permission");
            DropColumn("dbo.Companies", "Permission");
            DropColumn("dbo.Companies", "Owner");
            DropColumn("dbo.Companies", "Group");
            DropColumn("dbo.AdvancedInventoryReports", "Permission");
            DropColumn("dbo.AdvancedInventoryReports", "Owner");
            DropColumn("dbo.AdvancedInventoryReports", "Group");
            DropColumn("dbo.Forms", "Owner");
            DropColumn("dbo.Forms", "Group");
            DropColumn("dbo.Forms", "Permission");
            DropColumn("dbo.Audiences", "Owner");
            DropColumn("dbo.Audiences", "Group");
            DropColumn("dbo.Audiences", "Permission");
            DropColumn("dbo.ComponentBases", "Permission");
            DropColumn("dbo.ComponentBases", "Owner");
            DropColumn("dbo.ComponentBases", "Group");
            DropColumn("dbo.Logics", "Permission");
            DropColumn("dbo.Logics", "Owner");
            DropColumn("dbo.Logics", "Group");
            DropColumn("dbo.RSEmails", "Permission");
            DropColumn("dbo.RSEmails", "Owner");
            DropColumn("dbo.RSEmails", "Group");
            DropColumn("dbo.ContactReports", "Permission");
            DropColumn("dbo.ContactReports", "Owner");
            DropColumn("dbo.ContactReports", "Group");
            DropColumn("dbo.QueryFilters", "ReportKey");
            DropColumn("dbo.CustomTexts", "Permission");
            DropColumn("dbo.CustomTexts", "Owner");
            DropColumn("dbo.CustomTexts", "Group");
            DropColumn("dbo.RSHtmlEmails", "Permission");
            DropColumn("dbo.RSHtmlEmails", "Owner");
            DropColumn("dbo.RSHtmlEmails", "Group");
            DropColumn("dbo.Contacts", "Permission");
            DropColumn("dbo.Contacts", "Owner");
            DropColumn("dbo.Contacts", "Group");
            DropColumn("dbo.Registrants", "Permission");
            DropColumn("dbo.Registrants", "Owner");
            DropColumn("dbo.Registrants", "Group");
            DropColumn("dbo.Adjustments", "Owner");
            DropColumn("dbo.Adjustments", "Group");
            DropColumn("dbo.Adjustments", "Permission");
            DropColumn("dbo.OldRegistrants", "Permission");
            DropColumn("dbo.OldRegistrants", "Owner");
            DropColumn("dbo.OldRegistrants", "Group");
            DropColumn("dbo.Seatings", "Permission");
            DropColumn("dbo.Seatings", "Owner");
            DropColumn("dbo.Seatings", "Group");
            DropColumn("dbo.TransactionDetails", "CompanyKey");
            DropColumn("dbo.Tags", "Owner");
            DropColumn("dbo.Tags", "Group");
            DropColumn("dbo.Tags", "Permission");
            DropColumn("dbo.Pages", "Owner");
            DropColumn("dbo.Pages", "Group");
            DropColumn("dbo.Pages", "Permission");
            DropColumn("dbo.Panels", "Permission");
            DropColumn("dbo.Panels", "Owner");
            DropColumn("dbo.Panels", "Group");
            DropColumn("dbo.PriceGroups", "Permission");
            DropColumn("dbo.PriceGroups", "Owner");
            DropColumn("dbo.PriceGroups", "Group");
            DropColumn("dbo.Prices", "Owner");
            DropColumn("dbo.Prices", "Group");
            DropColumn("dbo.Prices", "Permission");
            DropColumn("dbo.FormTemplates", "Owner");
            DropColumn("dbo.FormTemplates", "Group");
            DropColumn("dbo.FormTemplates", "Permission");
            DropColumn("dbo.FormTemplates", "Company");
            DropColumn("dbo.CustomGroups", "Permission");
            DropColumn("dbo.CustomGroups", "Owner");
            DropColumn("dbo.CustomGroups", "Group");
            DropColumn("dbo.Folders", "Owner");
            DropColumn("dbo.Folders", "Group");
            DropColumn("dbo.DefaultFormStyles", "Permission");
            DropColumn("dbo.DefaultFormStyles", "Owner");
            DropColumn("dbo.DefaultFormStyles", "Group");
            DropColumn("dbo.CSS", "Owner");
            DropColumn("dbo.CSS", "Group");
            DropColumn("dbo.CSS", "Permission");
            DropColumn("dbo.SavedEmailTables", "Owner");
            DropColumn("dbo.SavedEmailTables", "Group");
            DropColumn("dbo.SavedEmailTables", "Permission");
            DropColumn("dbo.SavedTables", "Owner");
            DropColumn("dbo.SavedTables", "Group");
            DropColumn("dbo.SavedTables", "Permission");
            DropColumn("dbo.SmtpServers", "Owner");
            DropColumn("dbo.SmtpServers", "Group");
            DropColumn("dbo.SmtpServers", "Permission");
            DropColumn("dbo.Stylesheets", "Owner");
            DropColumn("dbo.Stylesheets", "Group");
            DropColumn("dbo.Stylesheets", "Permission");
            DropForeignKey("dbo.Reports", "FK_dbo.Reports_dbo.Companies_Company_UId");
            DropForeignKey("dbo.Reportheaders", "FK_dbo.ReportHeaders_dbo.Reports_ReportUId");
            DropForeignKey("dbo.SqlTables", "FK_dbo.SqlTables_dbo.Reports_ReportUId");
            DropForeignKey("dbo.SqlHeaders", "FK_dbo.SqlHeaders_dbo.ReportHeaders_ReportHeaderUId");
            DropForeignKey("dbo.SqlHeaders", "FK_dbo.SqlHeaders_dbo.SqlTables_SqlTableUId");
            DropTable("dbo.SqlTables");
            DropTable("dbo.SqlHeaders");
            DropTable("dbo.ReportHeaders");
            DropTable("dbo.Reports");
            DropTable("dbo.DatabaseAccess");
            DropTable("dbo.ErrorReviews");
            DropTable("dbo.Errors");
        }
        
        public override void Down()
        {
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
                .PrimaryKey(t => t.UId);
            
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
                "dbo.SqlTables",
                c => new
                    {
                        UId = c.Guid(nullable: false),
                        ReportKey = c.Guid(nullable: false),
                        SortingId = c.Long(nullable: false, identity: true),
                        Name = c.String(maxLength: 250),
                        DateCreated = c.DateTimeOffset(nullable: false, precision: 7),
                        DateModified = c.DateTimeOffset(nullable: false, precision: 7),
                        Owner = c.Guid(nullable: false),
                        Group = c.Guid(nullable: false),
                        Permission = c.String(maxLength: 3),
                        ModificationToken = c.Guid(nullable: false),
                        ModifiedBy = c.Guid(nullable: false),
                        RawSortings = c.String(),
                        RawFilters = c.String(),
                        DataKey = c.Guid(nullable: false),
                        DataDescriminator = c.String(maxLength: 150),
                    })
                .PrimaryKey(t => t.UId);
            
            CreateTable(
                "dbo.SqlHeaders",
                c => new
                    {
                        UId = c.Guid(nullable: false),
                        ReportHeaderKey = c.Guid(nullable: false),
                        SqlTableKey = c.Guid(nullable: false),
                        SortingId = c.Long(nullable: false, identity: true),
                        Name = c.String(maxLength: 250),
                        DateCreated = c.DateTimeOffset(nullable: false, precision: 7),
                        DateModified = c.DateTimeOffset(nullable: false, precision: 7),
                        Owner = c.Guid(nullable: false),
                        Group = c.Guid(nullable: false),
                        Permission = c.String(maxLength: 3),
                        ModificationToken = c.Guid(nullable: false),
                        ModifiedBy = c.Guid(nullable: false),
                        DataKey = c.String(),
                    })
                .PrimaryKey(t => t.UId);
            
            CreateTable(
                "dbo.ReportHeaders",
                c => new
                    {
                        UId = c.Guid(nullable: false),
                        ReportKey = c.Guid(nullable: false),
                        SortingId = c.Long(nullable: false, identity: true),
                        Name = c.String(maxLength: 250),
                        DateCreated = c.DateTimeOffset(nullable: false, precision: 7),
                        DateModified = c.DateTimeOffset(nullable: false, precision: 7),
                        Owner = c.Guid(nullable: false),
                        Group = c.Guid(nullable: false),
                        Permission = c.String(maxLength: 3),
                        ModificationToken = c.Guid(nullable: false),
                        ModifiedBy = c.Guid(nullable: false),
                        Position = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.UId);
            
            CreateTable(
                "dbo.Reports",
                c => new
                    {
                        UId = c.Guid(nullable: false),
                        CompanyKey = c.Guid(nullable: false),
                        SortingId = c.Long(nullable: false, identity: true),
                        Favorite = c.Boolean(nullable: false),
                        Name = c.String(maxLength: 250),
                        DateCreated = c.DateTimeOffset(nullable: false, precision: 7),
                        DateModified = c.DateTimeOffset(nullable: false, precision: 7),
                        Owner = c.Guid(nullable: false),
                        Group = c.Guid(nullable: false),
                        Permission = c.String(maxLength: 3),
                        ModificationToken = c.Guid(nullable: false),
                        ModifiedBy = c.Guid(nullable: false),
                        ReportType = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.UId);
            
            AddColumn("dbo.Stylesheets", "Permission", c => c.String(maxLength: 3));
            AddColumn("dbo.Stylesheets", "Group", c => c.Guid(nullable: false));
            AddColumn("dbo.Stylesheets", "Owner", c => c.Guid(nullable: false));
            AddColumn("dbo.SmtpServers", "Permission", c => c.String(maxLength: 3));
            AddColumn("dbo.SmtpServers", "Group", c => c.Guid(nullable: false));
            AddColumn("dbo.SmtpServers", "Owner", c => c.Guid(nullable: false));
            AddColumn("dbo.SavedTables", "Permission", c => c.String());
            AddColumn("dbo.SavedTables", "Group", c => c.Guid(nullable: false));
            AddColumn("dbo.SavedTables", "Owner", c => c.Guid(nullable: false));
            AddColumn("dbo.SavedEmailTables", "Permission", c => c.String());
            AddColumn("dbo.SavedEmailTables", "Group", c => c.Guid(nullable: false));
            AddColumn("dbo.SavedEmailTables", "Owner", c => c.Guid(nullable: false));
            AddColumn("dbo.CSS", "Permission", c => c.String(maxLength: 3));
            AddColumn("dbo.CSS", "Group", c => c.Guid(nullable: false));
            AddColumn("dbo.CSS", "Owner", c => c.Guid(nullable: false));
            AddColumn("dbo.DefaultFormStyles", "Group", c => c.Guid(nullable: false));
            AddColumn("dbo.DefaultFormStyles", "Owner", c => c.Guid(nullable: false));
            AddColumn("dbo.DefaultFormStyles", "Permission", c => c.String(maxLength: 3));
            AddColumn("dbo.Folders", "Group", c => c.Guid(nullable: false));
            AddColumn("dbo.Folders", "Owner", c => c.Guid(nullable: false));
            AddColumn("dbo.CustomGroups", "Group", c => c.Guid(nullable: false));
            AddColumn("dbo.CustomGroups", "Owner", c => c.Guid(nullable: false));
            AddColumn("dbo.CustomGroups", "Permission", c => c.String(maxLength: 3));
            AddColumn("dbo.FormTemplates", "Company", c => c.Guid(nullable: false));
            AddColumn("dbo.FormTemplates", "Permission", c => c.String(maxLength: 3));
            AddColumn("dbo.FormTemplates", "Group", c => c.Guid(nullable: false));
            AddColumn("dbo.FormTemplates", "Owner", c => c.Guid(nullable: false));
            AddColumn("dbo.Prices", "Permission", c => c.String(maxLength: 3));
            AddColumn("dbo.Prices", "Group", c => c.Guid(nullable: false));
            AddColumn("dbo.Prices", "Owner", c => c.Guid(nullable: false));
            AddColumn("dbo.PriceGroups", "Group", c => c.Guid(nullable: false));
            AddColumn("dbo.PriceGroups", "Owner", c => c.Guid(nullable: false));
            AddColumn("dbo.PriceGroups", "Permission", c => c.String(maxLength: 3));
            AddColumn("dbo.Panels", "Group", c => c.Guid(nullable: false));
            AddColumn("dbo.Panels", "Owner", c => c.Guid(nullable: false));
            AddColumn("dbo.Panels", "Permission", c => c.String(maxLength: 3));
            AddColumn("dbo.Pages", "Permission", c => c.String(maxLength: 3));
            AddColumn("dbo.Pages", "Group", c => c.Guid(nullable: false));
            AddColumn("dbo.Pages", "Owner", c => c.Guid(nullable: false));
            AddColumn("dbo.Tags", "Permission", c => c.String(maxLength: 3));
            AddColumn("dbo.Tags", "Group", c => c.Guid(nullable: false));
            AddColumn("dbo.Tags", "Owner", c => c.Guid(nullable: false));
            AddColumn("dbo.TransactionDetails", "CompanyKey", c => c.Guid());
            AddColumn("dbo.Seatings", "Group", c => c.Guid(nullable: false));
            AddColumn("dbo.Seatings", "Owner", c => c.Guid(nullable: false));
            AddColumn("dbo.Seatings", "Permission", c => c.String(maxLength: 3));
            AddColumn("dbo.OldRegistrants", "Group", c => c.Guid(nullable: false));
            AddColumn("dbo.OldRegistrants", "Owner", c => c.Guid(nullable: false));
            AddColumn("dbo.OldRegistrants", "Permission", c => c.String(maxLength: 3));
            AddColumn("dbo.Adjustments", "Permission", c => c.String(maxLength: 3));
            AddColumn("dbo.Adjustments", "Group", c => c.Guid(nullable: false));
            AddColumn("dbo.Adjustments", "Owner", c => c.Guid(nullable: false));
            AddColumn("dbo.Registrants", "Group", c => c.Guid(nullable: false));
            AddColumn("dbo.Registrants", "Owner", c => c.Guid(nullable: false));
            AddColumn("dbo.Registrants", "Permission", c => c.String(maxLength: 3));
            AddColumn("dbo.Contacts", "Group", c => c.Guid(nullable: false));
            AddColumn("dbo.Contacts", "Owner", c => c.Guid(nullable: false));
            AddColumn("dbo.Contacts", "Permission", c => c.String(maxLength: 3));
            AddColumn("dbo.RSHtmlEmails", "Group", c => c.Guid(nullable: false));
            AddColumn("dbo.RSHtmlEmails", "Owner", c => c.Guid(nullable: false));
            AddColumn("dbo.RSHtmlEmails", "Permission", c => c.String(maxLength: 3));
            AddColumn("dbo.CustomTexts", "Group", c => c.Guid(nullable: false));
            AddColumn("dbo.CustomTexts", "Owner", c => c.Guid(nullable: false));
            AddColumn("dbo.CustomTexts", "Permission", c => c.String(maxLength: 3));
            AddColumn("dbo.QueryFilters", "ReportKey", c => c.Guid());
            AddColumn("dbo.ContactReports", "Group", c => c.Guid(nullable: false));
            AddColumn("dbo.ContactReports", "Owner", c => c.Guid(nullable: false));
            AddColumn("dbo.ContactReports", "Permission", c => c.String(maxLength: 3));
            AddColumn("dbo.RSEmails", "Group", c => c.Guid(nullable: false));
            AddColumn("dbo.RSEmails", "Owner", c => c.Guid(nullable: false));
            AddColumn("dbo.RSEmails", "Permission", c => c.String(maxLength: 3));
            AddColumn("dbo.Logics", "Group", c => c.Guid(nullable: false));
            AddColumn("dbo.Logics", "Owner", c => c.Guid(nullable: false));
            AddColumn("dbo.Logics", "Permission", c => c.String(maxLength: 3));
            AddColumn("dbo.ComponentBases", "Group", c => c.Guid(nullable: false));
            AddColumn("dbo.ComponentBases", "Owner", c => c.Guid(nullable: false));
            AddColumn("dbo.ComponentBases", "Permission", c => c.String(maxLength: 3));
            AddColumn("dbo.Audiences", "Permission", c => c.String(maxLength: 3));
            AddColumn("dbo.Audiences", "Group", c => c.Guid(nullable: false));
            AddColumn("dbo.Audiences", "Owner", c => c.Guid(nullable: false));
            AddColumn("dbo.Forms", "Permission", c => c.String(maxLength: 3));
            AddColumn("dbo.Forms", "Group", c => c.Guid(nullable: false));
            AddColumn("dbo.Forms", "Owner", c => c.Guid(nullable: false));
            AddColumn("dbo.AdvancedInventoryReports", "Group", c => c.Guid(nullable: false));
            AddColumn("dbo.AdvancedInventoryReports", "Owner", c => c.Guid(nullable: false));
            AddColumn("dbo.AdvancedInventoryReports", "Permission", c => c.String());
            AddColumn("dbo.Companies", "Group", c => c.Guid(nullable: false));
            AddColumn("dbo.Companies", "Owner", c => c.Guid(nullable: false));
            AddColumn("dbo.Companies", "Permission", c => c.String(maxLength: 3));
            AddColumn("dbo.AspNetUsers", "Permission", c => c.String(maxLength: 3));
            AddColumn("dbo.AspNetUsers", "Group", c => c.Guid(nullable: false));
            AddColumn("dbo.AspNetUsers", "Owner", c => c.Guid(nullable: false));
            DropForeignKey("dbo.TransactionRequests", "CompanyKey", "dbo.Companies");
            DropForeignKey("dbo.LogicBlocks", "EmailCampaignKey", "dbo.EmailCampaigns");
            DropIndex("dbo.TransactionRequests", new[] { "CompanyKey" });
            DropIndex("dbo.LogicBlocks", new[] { "EmailCampaignKey" });
            DropColumn("dbo.LogicBlocks", "EmailCampaignKey");
            DropColumn("dbo.CreditCards", "Name");
            DropColumn("dbo.CreditCards", "DateModified");
            DropColumn("dbo.CreditCards", "DateCreated");
            DropColumn("dbo.CreditCards", "ModificationToken");
            DropColumn("dbo.CreditCards", "ModifiedBy");
            CreateIndex("dbo.ErrorReviews", "ErrorUId");
            CreateIndex("dbo.SqlTables", "ReportKey");
            CreateIndex("dbo.SqlHeaders", "SqlTableKey");
            CreateIndex("dbo.SqlHeaders", "ReportHeaderKey");
            CreateIndex("dbo.ReportHeaders", "ReportKey");
            CreateIndex("dbo.Reports", "CompanyKey");
            CreateIndex("dbo.QueryFilters", "ReportKey");
            AddForeignKey("dbo.ErrorReviews", "ErrorUId", "dbo.Errors", "UId");
            AddForeignKey("dbo.QueryFilters", "ReportKey", "dbo.Reports", "UId");
            AddForeignKey("dbo.SqlTables", "ReportKey", "dbo.Reports", "UId");
            AddForeignKey("dbo.SqlHeaders", "SqlTableKey", "dbo.SqlTables", "UId");
            AddForeignKey("dbo.SqlHeaders", "ReportHeaderKey", "dbo.ReportHeaders", "UId");
            AddForeignKey("dbo.ReportHeaders", "ReportKey", "dbo.Reports", "UId");
            AddForeignKey("dbo.Reports", "CompanyKey", "dbo.Companies", "UId");
        }
    }
}
