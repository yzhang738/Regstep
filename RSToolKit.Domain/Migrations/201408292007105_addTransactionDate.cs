namespace RSToolKit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addTransactionDate : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Adjustments", "TransactionDate", c => c.DateTimeOffset(precision: 7));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Adjustments", "TransactionDate");
        }
    }
}
