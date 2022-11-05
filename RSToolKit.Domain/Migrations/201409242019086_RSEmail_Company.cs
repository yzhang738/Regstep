namespace RSToolKit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RSEmail_Company : DbMigration
    {
        public override void Up()
        {
            RenameColumn("dbo.SmtpServers", "CompanyUId", "CompanyKey");
            CreateIndex("dbo.SmtpServers", "CompanyKey");
            AddForeignKey("dbo.SmtpServers", "CompanyKey", "dbo.Companies", "UId");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.SmtpServers", "CompanyKey", "dbo.Companies");
            DropIndex("dbo.SmtpServers", new[] { "CompanyKey" });
            RenameColumn("dbo.SmtpServers", "CompanyKey", "CompanyUId");
        }
    }
}
