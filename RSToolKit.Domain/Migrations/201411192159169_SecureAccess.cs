namespace RSToolKit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class SecureAccess : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.CreditCardAccess", "CreditCardKey", "dbo.CreditCards");
            DropIndex("dbo.CreditCardAccess", new[] { "CreditCardKey" });
            CreateTable(
                "dbo.AccessLog",
                c => new
                    {
                        SortingId = c.Long(nullable: false, identity: true),
                        AccessedItem = c.Guid(nullable: false),
                        AccessKey = c.Guid(),
                        AccessTime = c.DateTimeOffset(nullable: false, precision: 7),
                        AccessType = c.Int(nullable: false),
                        Note = c.String(),
                    })
                .PrimaryKey(t => t.SortingId);
            
            DropColumn("dbo.CreditCards", "CompanyKey");
            DropTable("dbo.CreditCardAccess");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.CreditCardAccess",
                c => new
                    {
                        UId = c.Guid(nullable: false),
                        CreditCardKey = c.Guid(),
                        SortingId = c.Long(nullable: false, identity: true),
                        UserUId = c.Guid(nullable: false),
                        AccessTime = c.DateTimeOffset(nullable: false, precision: 7),
                        AccessType = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.UId);
            
            AddColumn("dbo.CreditCards", "CompanyKey", c => c.Guid(nullable: false));
            DropTable("dbo.AccessLog");
            CreateIndex("dbo.CreditCardAccess", "CreditCardKey");
            AddForeignKey("dbo.CreditCardAccess", "CreditCardKey", "dbo.CreditCards", "UId");
        }
    }
}
