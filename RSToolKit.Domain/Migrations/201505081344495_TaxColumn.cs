namespace RSToolKit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class TaxColumn : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Registrants", "Taxes", c => c.Decimal(nullable: false, precision: 18, scale: 2, defaultValue: 0m));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Registrants", "Taxes");
        }
    }
}
