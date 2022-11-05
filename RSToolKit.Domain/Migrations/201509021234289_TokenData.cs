namespace RSToolKit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class TokenData : DbMigration
    {
        public override void Up()
        {
            DropIndex("dbo.Charges", new[] { "UId" });
            DropIndex("dbo.Charges", new[] { "FormKey" });
            CreateIndex("dbo.Charges", "UId");
            CreateIndex("dbo.Charges", "FormKey");
        }
        
        public override void Down()
        {
            DropIndex("dbo.Charges", new[] { "FormKey" });
            DropIndex("dbo.Charges", new[] { "UId" });
            CreateIndex("dbo.Charges", "FormKey", clustered: true);
            CreateIndex("dbo.Charges", "UId");
        }
    }
}
