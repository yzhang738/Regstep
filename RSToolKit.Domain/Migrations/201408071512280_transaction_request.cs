namespace RSToolKit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class transaction_request : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.TransactionRequests", "Cart", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.TransactionRequests", "Cart");
        }
    }
}
