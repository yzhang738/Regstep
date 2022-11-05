namespace RSToolKit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class promotionCodelimit : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.PromotionCodes", "Limit", c => c.Int(nullable: false, defaultValue: -1));
        }
        
        public override void Down()
        {
            DropColumn("dbo.PromotionCodes", "Limit");
        }
    }
}
