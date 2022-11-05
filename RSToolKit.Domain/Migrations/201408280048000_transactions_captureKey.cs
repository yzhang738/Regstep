namespace RSToolKit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class transactions_captureKey : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.TransactionDetails", "CaptureKey", c => c.Guid());
            CreateIndex("dbo.TransactionDetails", "CaptureKey");
            AddForeignKey("dbo.TransactionDetails", "CaptureKey", "dbo.TransactionDetails", "UId");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.TransactionDetails", "CaptureKey", "dbo.TransactionDetails");
            DropIndex("dbo.TransactionDetails", new[] { "CaptureKey" });
            DropColumn("dbo.TransactionDetails", "CaptureKey");
        }
    }
}
