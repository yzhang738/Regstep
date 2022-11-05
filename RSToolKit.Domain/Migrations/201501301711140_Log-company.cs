namespace RSToolKit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Logcompany : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Logs", "CompanyKey", c => c.Guid());
            CreateIndex("dbo.Logs", "CompanyKey");
            AddForeignKey("dbo.Logs", "CompanyKey", "dbo.Companies", "UId");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Logs", "CompanyKey", "dbo.Companies");
            DropIndex("dbo.Logs", new[] { "CompanyKey" });
            DropColumn("dbo.Logs", "CompanyKey");
        }
    }
}
