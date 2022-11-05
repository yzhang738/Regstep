namespace RSToolKit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class transactionDetailResponse : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.TransactionDetails", "Response", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.TransactionDetails", "Response");
        }
    }
}
