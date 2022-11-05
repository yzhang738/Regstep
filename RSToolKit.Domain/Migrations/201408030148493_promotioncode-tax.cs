namespace RSToolKit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class promotioncodetax : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.PromotionCodes",
                c => new
                    {
                        UId = c.Guid(nullable: false),
                        SortingId = c.Long(nullable: false, identity: true),
                        FormKey = c.Guid(nullable: false),
                        Action = c.Int(nullable: false),
                        Ammount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Code = c.String(),
                        Description = c.String(),
                    })
                .PrimaryKey(t => t.UId)
                .ForeignKey("dbo.Forms", t => t.FormKey)
                .Index(t => t.FormKey);
            
            CreateTable(
                "dbo.PromotionCodeEntries",
                c => new
                    {
                        UId = c.Guid(nullable: false),
                        SortingId = c.Long(nullable: false, identity: true),
                        CodeKey = c.Guid(nullable: false),
                        RegistrantKey = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.UId)
                .ForeignKey("dbo.PromotionCodes", t => t.CodeKey)
                .ForeignKey("dbo.Registrants", t => t.RegistrantKey)
                .Index(t => t.CodeKey)
                .Index(t => t.RegistrantKey);
            
            AddColumn("dbo.Forms", "Tax", c => c.Decimal(precision: 18, scale: 2));
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.PromotionCodes", "FormKey", "dbo.Forms");
            DropForeignKey("dbo.PromotionCodeEntries", "RegistrantKey", "dbo.Registrants");
            DropForeignKey("dbo.PromotionCodeEntries", "CodeKey", "dbo.PromotionCodes");
            DropIndex("dbo.PromotionCodeEntries", new[] { "RegistrantKey" });
            DropIndex("dbo.PromotionCodeEntries", new[] { "CodeKey" });
            DropIndex("dbo.PromotionCodes", new[] { "FormKey" });
            DropColumn("dbo.Forms", "Tax");
            DropTable("dbo.PromotionCodeEntries");
            DropTable("dbo.PromotionCodes");
        }
    }
}
