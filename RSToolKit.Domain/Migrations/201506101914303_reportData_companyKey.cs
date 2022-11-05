namespace RSToolKit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class reportData_companyKey : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ReportDatas", "CompanyKey", c => c.Long(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.ReportDatas", "CompanyKey");
        }
    }
}
