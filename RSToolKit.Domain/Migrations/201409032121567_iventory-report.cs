namespace RSToolKit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class iventoryreport : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.SingleFormReports", "Inventory", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.SingleFormReports", "Inventory");
        }
    }
}
