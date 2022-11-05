namespace RSToolKit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class adjustment : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Adjustments", "Amount", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AddColumn("dbo.PromotionCodes", "Amount", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AddColumn("dbo.Seatings", "SeatingType", c => c.Int(nullable: false));
            AddColumn("dbo.Price", "Amount", c => c.Decimal(precision: 18, scale: 2));
            DropColumn("dbo.Adjustments", "Ammount");
            DropColumn("dbo.PromotionCodes", "Ammount");
            DropColumn("dbo.Seatings", "SeatinType");
            DropColumn("dbo.Price", "Ammount");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Price", "Ammount", c => c.Decimal(precision: 18, scale: 2));
            AddColumn("dbo.Seatings", "SeatinType", c => c.Int(nullable: false));
            AddColumn("dbo.PromotionCodes", "Ammount", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AddColumn("dbo.Adjustments", "Ammount", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            DropColumn("dbo.Price", "Amount");
            DropColumn("dbo.Seatings", "SeatingType");
            DropColumn("dbo.PromotionCodes", "Amount");
            DropColumn("dbo.Adjustments", "Amount");
        }
    }
}
