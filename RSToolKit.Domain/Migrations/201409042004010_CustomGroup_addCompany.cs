namespace RSToolKit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class CustomGroup_addCompany : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.CustomGroups", "CompanyKey", c => c.Guid(nullable: false));
            CreateIndex("dbo.CustomGroups", "CompanyKey");
            AddForeignKey("dbo.CustomGroups", "CompanyKey", "dbo.Companies", "UId");
            DropColumn("dbo.CustomGroups", "Company");
        }
        
        public override void Down()
        {
            AddColumn("dbo.CustomGroups", "Company", c => c.Guid(nullable: false));
            DropForeignKey("dbo.CustomGroups", "CompanyKey", "dbo.Companies");
            DropIndex("dbo.CustomGroups", new[] { "CompanyKey" });
            DropColumn("dbo.CustomGroups", "CompanyKey");
        }
    }
}
