namespace RSToolKit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addType : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Adjustments", "AdjustmentType", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Adjustments", "AdjustmentType");
        }
    }
}
