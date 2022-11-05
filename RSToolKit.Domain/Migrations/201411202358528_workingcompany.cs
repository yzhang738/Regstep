namespace RSToolKit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class workingcompany : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AspNetUsers", "WorkingCompanyKey", c => c.Guid());
            CreateIndex("dbo.AspNetUsers", "WorkingCompanyKey");
            AddForeignKey("dbo.AspNetUsers", "WorkingCompanyKey", "dbo.Companies", "UId");
            DropColumn("dbo.AspNetUsers", "CurrentCompany");
            DropColumn("dbo.AspNetUsers", "CurrentCompanyName");
        }
        
        public override void Down()
        {
            AddColumn("dbo.AspNetUsers", "CurrentCompanyName", c => c.String());
            AddColumn("dbo.AspNetUsers", "CurrentCompany", c => c.Guid(nullable: false));
            DropForeignKey("dbo.AspNetUsers", "WorkingCompanyKey", "dbo.Companies");
            DropIndex("dbo.AspNetUsers", new[] { "WorkingCompanyKey" });
            DropColumn("dbo.AspNetUsers", "WorkingCompanyKey");
        }
    }
}
