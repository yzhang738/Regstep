namespace RSToolKit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class remove_CCNumber_transactionDetails : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.TransactionDetails", "CardNumber");
        }
        
        public override void Down()
        {
            AddColumn("dbo.TransactionDetails", "CardNumber", c => c.String());
        }
    }
}
