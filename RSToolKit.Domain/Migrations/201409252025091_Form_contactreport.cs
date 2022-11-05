namespace RSToolKit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Form_contactreport : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Forms", "ContactReportKey", c => c.Guid());
            CreateIndex("dbo.Forms", "ContactReportKey");
            AddForeignKey("dbo.Forms", "ContactReportKey", "dbo.ContactReports", "UId");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Forms", "ContactReportKey", "dbo.ContactReports");
            DropIndex("dbo.Forms", new[] { "ContactReportKey" });
            DropColumn("dbo.Forms", "ContactReportKey");
        }
    }
}