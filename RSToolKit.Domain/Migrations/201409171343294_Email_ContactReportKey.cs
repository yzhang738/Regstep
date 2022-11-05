namespace RSToolKit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Email_ContactReportKey : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.RSEmails", "ContactReportKey", c => c.Guid());
            CreateIndex("dbo.RSEmails", "ContactReportKey");
            AddForeignKey("dbo.RSEmails", "ContactReportKey", "dbo.ContactReports", "UId");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.RSEmails", "ContactReportKey", "dbo.ContactReports");
            DropIndex("dbo.RSEmails", new[] { "ContactReportKey" });
            DropColumn("dbo.RSEmails", "ContactReportKey");
        }
    }
}
