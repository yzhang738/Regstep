namespace RSToolKit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class payment_record : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Adjustments", "TransactionId", c => c.String());
            AddColumn("dbo.Adjustments", "Type", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Adjustments", "Type");
            DropColumn("dbo.Adjustments", "TransactionId");
        }
    }
}
