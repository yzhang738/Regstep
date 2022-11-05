namespace RSToolKit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class merchantaccount : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.TransactionRequests", "Address2", c => c.String());
            AddColumn("dbo.TransactionDetails", "CVVResponse", c => c.String());
            AddColumn("dbo.MerchantAccountInfo", "CompanyKey", c => c.Guid(nullable: true));
            AddColumn("dbo.MerchantAccountInfo", "UserKey", c => c.String());
            AddColumn("dbo.MerchantAccountInfo", "KeySalt", c => c.String());
            AddColumn("dbo.MerchantAccountInfo", "UserSalt", c => c.String());
            AlterColumn("dbo.MerchantAccountInfo", "Descriminator", c => c.String(nullable: false));
            CreateIndex("dbo.MerchantAccountInfo", "CompanyKey");
            AddForeignKey("dbo.MerchantAccountInfo", "CompanyKey", "dbo.Companies", "UId");
            DropColumn("dbo.MerchantAccountInfo", "Company");
            DropColumn("dbo.MerchantAccountInfo", "UserName");
            DropColumn("dbo.MerchantAccountInfo", "Salt");
        }
        
        public override void Down()
        {
            AddColumn("dbo.MerchantAccountInfo", "Salt", c => c.String());
            AddColumn("dbo.MerchantAccountInfo", "UserName", c => c.String());
            AddColumn("dbo.MerchantAccountInfo", "Company", c => c.Guid(nullable: false));
            DropForeignKey("dbo.MerchantAccountInfo", "CompanyKey", "dbo.Companies");
            DropIndex("dbo.MerchantAccountInfo", new[] { "CompanyKey" });
            AlterColumn("dbo.MerchantAccountInfo", "Descriminator", c => c.String());
            DropColumn("dbo.MerchantAccountInfo", "UserSalt");
            DropColumn("dbo.MerchantAccountInfo", "KeySalt");
            DropColumn("dbo.MerchantAccountInfo", "UserKey");
            DropColumn("dbo.MerchantAccountInfo", "CompanyKey");
            DropColumn("dbo.TransactionDetails", "CVVResponse");
            DropColumn("dbo.TransactionRequests", "Address2");
        }
    }
}
