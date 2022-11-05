namespace RSToolKit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class transactions_name_exp : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.TransactionRequests", "Description", c => c.String());
            AddColumn("dbo.TransactionRequests", "NameOnCard", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.TransactionRequests", "NameOnCard");
            DropColumn("dbo.TransactionRequests", "Description");
        }
    }
}
