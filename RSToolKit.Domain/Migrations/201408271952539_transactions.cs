namespace RSToolKit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class transactions : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Contacts", "CompanyKey", c => c.Guid(nullable: false));
            AddColumn("dbo.ContactData", "CompanyKey", c => c.Guid(nullable: false));
            AddColumn("dbo.TransactionRequests", "Currency", c => c.Int(nullable: false));
            AlterColumn("dbo.TransactionRequests", "CompanyKey", c => c.Guid());
            CreateIndex("dbo.Contacts", "CompanyKey");
            CreateIndex("dbo.ContactData", "CompanyKey");
            AddForeignKey("dbo.Contacts", "CompanyKey", "dbo.Companies", "UId");
            AddForeignKey("dbo.ContactData", "CompanyKey", "dbo.Companies", "UId");
            DropColumn("dbo.Contacts", "Company");
            DropColumn("dbo.ContactData", "CompanyUId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.ContactData", "CompanyUId", c => c.Guid(nullable: false));
            AddColumn("dbo.Contacts", "Company", c => c.Guid(nullable: false));
            DropForeignKey("dbo.ContactData", "CompanyKey", "dbo.Companies");
            DropForeignKey("dbo.Contacts", "CompanyKey", "dbo.Companies");
            DropIndex("dbo.ContactData", new[] { "CompanyKey" });
            DropIndex("dbo.Contacts", new[] { "CompanyKey" });
            AlterColumn("dbo.TransactionRequests", "CompanyKey", c => c.Guid(nullable: false));
            DropColumn("dbo.TransactionRequests", "Currency");
            DropColumn("dbo.ContactData", "CompanyKey");
            DropColumn("dbo.Contacts", "CompanyKey");
        }
    }
}
