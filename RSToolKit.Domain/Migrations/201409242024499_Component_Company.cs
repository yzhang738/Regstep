namespace RSToolKit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Component_Company : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ComponentBases", "CompanyKey", c => c.Guid(nullable: false, defaultValue: Guid.Parse("c2a247ac-7b3c-49ab-bcdd-09e81b5304d3")));
            CreateIndex("dbo.ComponentBases", "CompanyKey");
            AddForeignKey("dbo.ComponentBases", "CompanyKey", "dbo.Companies", "UId");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ComponentBases", "CompanyKey", "dbo.Companies");
            DropIndex("dbo.ComponentBases", new[] { "CompanyKey" });
            DropColumn("dbo.ComponentBases", "CompanyKey");
        }
    }
}
