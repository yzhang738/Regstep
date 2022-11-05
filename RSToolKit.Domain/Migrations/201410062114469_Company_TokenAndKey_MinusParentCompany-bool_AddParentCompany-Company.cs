namespace RSToolKit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Company_TokenAndKey_MinusParentCompanybool_AddParentCompanyCompany : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Companies", "Key", c => c.String());
            AddColumn("dbo.Companies", "Token", c => c.Guid());
            AddColumn("dbo.Companies", "CompanyKey", c => c.Guid());
            CreateIndex("dbo.Companies", "CompanyKey");
            AddForeignKey("dbo.Companies", "CompanyKey", "dbo.Companies", "UId");
            DropColumn("dbo.Companies", "ParentCompany");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Companies", "ParentCompany", c => c.Boolean(nullable: false));
            DropForeignKey("dbo.Companies", "CompanyKey", "dbo.Companies");
            DropIndex("dbo.Companies", new[] { "CompanyKey" });
            DropColumn("dbo.Companies", "CompanyKey");
            DropColumn("dbo.Companies", "Token");
            DropColumn("dbo.Companies", "Key");
        }
    }
}
