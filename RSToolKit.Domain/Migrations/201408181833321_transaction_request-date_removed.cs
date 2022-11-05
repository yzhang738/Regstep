namespace RSToolKit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class transaction_requestdate_removed : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.TransactionRequests", "DateCreated", c => c.DateTimeOffset(nullable: false, precision: 7));
            AddColumn("dbo.TransactionRequests", "DateModified", c => c.DateTimeOffset(nullable: false, precision: 7));
            DropColumn("dbo.TransactionRequests", "Date");
        }
        
        public override void Down()
        {
            AddColumn("dbo.TransactionRequests", "Date", c => c.DateTimeOffset(nullable: false, precision: 7));
            DropColumn("dbo.TransactionRequests", "DateModified");
            DropColumn("dbo.TransactionRequests", "DateCreated");
        }
    }
}
