namespace RSToolKit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class EmailBuilder : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.RSEmails", "CompanyKey", c => c.Guid(nullable: false));
            CreateIndex("dbo.RSEmails", "CompanyKey");
            AddForeignKey("dbo.RSEmails", "CompanyKey", "dbo.Companies", "UId");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.RSEmails", "CompanyKey", "dbo.Companies");
            DropIndex("dbo.RSEmails", new[] { "CompanyKey" });
            DropColumn("dbo.RSEmails", "CompanyKey");
        }
    }
}
