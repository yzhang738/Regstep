namespace RSToolKit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class registrantquickfeesupdate2 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Registrants", "Transactions", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AddColumn("dbo.Registrants", "Adjustings", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            DropColumn("dbo.Registrants", "Credits");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Registrants", "Credits", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            DropColumn("dbo.Registrants", "Adjustings");
            DropColumn("dbo.Registrants", "Transactions");
        }
    }
}
