namespace RSToolKit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class companyKey_AccessLog : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AccessLog", "CompanyKey", c => c.Guid());
            CreateIndex("dbo.AccessLog", "CompanyKey");
            AddForeignKey("dbo.AccessLog", "CompanyKey", "dbo.Companies", "UId");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.AccessLog", "CompanyKey", "dbo.Companies");
            DropIndex("dbo.AccessLog", new[] { "CompanyKey" });
            DropColumn("dbo.AccessLog", "CompanyKey");
        }
    }
}
