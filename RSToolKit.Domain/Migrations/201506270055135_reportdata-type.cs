namespace RSToolKit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class reportdatatype : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ReportDatas", "Type", c => c.Int(nullable: false, defaultValue: 0));
        }
        
        public override void Down()
        {
            DropColumn("dbo.ReportDatas", "Type");
        }
    }
}
