namespace RSToolKit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class HtmlEmail : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.RSHtmlEmails",
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
                        FormKey = c.Guid(),
                        EmailCampaignKey = c.Guid(),
                        SavedListKey = c.Guid(),
                        ContactReportKey = c.Guid(),
                        PlainText = c.String(),
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
                        Html = c.String(),
                    })
                .PrimaryKey(t => t.UId, clustered: false)
                .ForeignKey("dbo.Companies", t => t.CompanyKey)
                .ForeignKey("dbo.ContactReports", t => t.ContactReportKey)
                .ForeignKey("dbo.EmailCampaigns", t => t.EmailCampaignKey)
                .ForeignKey("dbo.Forms", t => t.FormKey)
                .ForeignKey("dbo.SavedLists", t => t.SavedListKey)
                .Index(t => t.UId, clustered: false)
                .Index(t => t.SortingId, clustered: true, unique: true)
                .Index(t => t.CompanyKey)
                .Index(t => t.FormKey)
                .Index(t => t.EmailCampaignKey)
                .Index(t => t.SavedListKey)
                .Index(t => t.ContactReportKey);
            
            AddColumn("dbo.Logics", "HtmlEmailKey", c => c.Guid());
            CreateIndex("dbo.Logics", "HtmlEmailKey");
            AddForeignKey("dbo.Logics", "HtmlEmailKey", "dbo.RSHtmlEmails", "UId");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.RSHtmlEmails", "SavedListKey", "dbo.SavedLists");
            DropForeignKey("dbo.Logics", "HtmlEmailKey", "dbo.RSHtmlEmails");
            DropForeignKey("dbo.RSHtmlEmails", "FormKey", "dbo.Forms");
            DropForeignKey("dbo.RSHtmlEmails", "EmailCampaignKey", "dbo.EmailCampaigns");
            DropForeignKey("dbo.RSHtmlEmails", "ContactReportKey", "dbo.ContactReports");
            DropForeignKey("dbo.RSHtmlEmails", "CompanyKey", "dbo.Companies");
            DropIndex("dbo.RSHtmlEmails", new[] { "ContactReportKey" });
            DropIndex("dbo.RSHtmlEmails", new[] { "SavedListKey" });
            DropIndex("dbo.RSHtmlEmails", new[] { "EmailCampaignKey" });
            DropIndex("dbo.RSHtmlEmails", new[] { "FormKey" });
            DropIndex("dbo.RSHtmlEmails", new[] { "CompanyKey" });
            DropIndex("dbo.RSHtmlEmails", new[] { "SortingId" });
            DropIndex("dbo.RSHtmlEmails", new[] { "UId" });
            DropIndex("dbo.Logics", new[] { "HtmlEmailKey" });
            DropColumn("dbo.Logics", "HtmlEmailKey");
            DropTable("dbo.RSHtmlEmails");
        }
    }
}
