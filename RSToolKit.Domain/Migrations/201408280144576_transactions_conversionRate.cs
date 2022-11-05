namespace RSToolKit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class transactions_conversionRate : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.TransactionDetails", "ConversionRate", c => c.Decimal(nullable: false, precision: 18, scale: 2));
        }
        
        public override void Down()
        {
            DropColumn("dbo.TransactionDetails", "ConversionRate");
        }
    }
}
