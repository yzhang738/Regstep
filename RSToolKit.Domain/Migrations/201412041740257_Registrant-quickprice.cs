namespace RSToolKit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Registrantquickprice : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Registrants", "Fees", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AddColumn("dbo.Registrants", "Credits", c => c.Decimal(nullable: false, precision: 18, scale: 2));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Registrants", "Credits");
            DropColumn("dbo.Registrants", "Fees");
        }
    }
}
