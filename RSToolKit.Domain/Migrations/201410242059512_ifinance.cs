namespace RSToolKit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ifinance : DbMigration
    {
        public override void Up()
        {
            RenameColumn("dbo.TransactionDetails", "CreationDate", "DateCreated");
        }
        
        public override void Down()
        {
            RenameColumn("dbo.TransactionDetails", "DateCreated", "CreationDate");
        }
    }
}
