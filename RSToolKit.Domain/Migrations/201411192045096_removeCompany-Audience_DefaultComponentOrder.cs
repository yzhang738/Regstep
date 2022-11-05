namespace RSToolKit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class removeCompanyAudience_DefaultComponentOrder : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Logics", "CompanyKey", "dbo.Companies");
            DropForeignKey("dbo.DefaultComponentOrders", "CompanyKey", "dbo.Companies");
            DropForeignKey("dbo.Audiences", "CompanyKey", "dbo.Companies");
            DropIndex("dbo.Audiences", new[] { "CompanyKey" });
            DropIndex("dbo.Logics", new[] { "CompanyKey" });
            DropIndex("dbo.DefaultComponentOrders", new[] { "CompanyKey" });
            DropColumn("dbo.Audiences", "CompanyKey");
            DropColumn("dbo.Logics", "CompanyKey");
            DropColumn("dbo.DefaultComponentOrders", "CompanyKey");
        }
        
        public override void Down()
        {
            AddColumn("dbo.DefaultComponentOrders", "CompanyKey", c => c.Guid());
            AddColumn("dbo.Logics", "CompanyKey", c => c.Guid(nullable: false));
            AddColumn("dbo.Audiences", "CompanyKey", c => c.Guid(nullable: false));
            CreateIndex("dbo.DefaultComponentOrders", "CompanyKey");
            CreateIndex("dbo.Logics", "CompanyKey");
            CreateIndex("dbo.Audiences", "CompanyKey");
            AddForeignKey("dbo.Audiences", "CompanyKey", "dbo.Companies", "UId");
            AddForeignKey("dbo.DefaultComponentOrders", "CompanyKey", "dbo.Companies", "UId");
            AddForeignKey("dbo.Logics", "CompanyKey", "dbo.Companies", "UId");
        }
    }
}
