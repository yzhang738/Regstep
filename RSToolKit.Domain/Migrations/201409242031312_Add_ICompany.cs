namespace RSToolKit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Add_ICompany : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Audiences", "CompanyKey", c => c.Guid(nullable: false, defaultValue: Guid.Parse("c2a247ac-7b3c-49ab-bcdd-09e81b5304d3")));
            AddColumn("dbo.Logics", "CompanyKey", c => c.Guid(nullable: false, defaultValue: Guid.Parse("c2a247ac-7b3c-49ab-bcdd-09e81b5304d3")));
            CreateIndex("dbo.Audiences", "CompanyKey");
            CreateIndex("dbo.Logics", "CompanyKey");
            AddForeignKey("dbo.Logics", "CompanyKey", "dbo.Companies", "UId");
            AddForeignKey("dbo.Audiences", "CompanyKey", "dbo.Companies", "UId");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Audiences", "CompanyKey", "dbo.Companies");
            DropForeignKey("dbo.Logics", "CompanyKey", "dbo.Companies");
            DropIndex("dbo.Logics", new[] { "CompanyKey" });
            DropIndex("dbo.Audiences", new[] { "CompanyKey" });
            DropColumn("dbo.Logics", "CompanyKey");
            DropColumn("dbo.Audiences", "CompanyKey");
        }
    }
}
