namespace RSToolKit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class plainTextEmail : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.AdvancedInventoryReports",
                c => new
                    {
                        UId = c.Guid(nullable: false),
                        SortingId = c.Long(nullable: false, identity: true),
                        CompanyKey = c.Guid(nullable: false),
                        FormKey = c.Guid(nullable: false),
                        Script = c.String(),
                        Name = c.String(maxLength: 250),
                        Permission = c.String(),
                        DateCreated = c.DateTimeOffset(nullable: false, precision: 7),
                        DateModified = c.DateTimeOffset(nullable: false, precision: 7),
                        Owner = c.Guid(nullable: false),
                        Group = c.Guid(nullable: false),
                        ModificationToken = c.Guid(nullable: false),
                        ModifiedBy = c.Guid(nullable: false),
                        Favorite = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.UId, clustered: false)
                .ForeignKey("dbo.Companies", t => t.CompanyKey)
                .ForeignKey("dbo.Forms", t => t.FormKey)
                .Index(t => t.UId)
                .Index(t => t.SortingId, unique: true, clustered: true)
                .Index(t => t.CompanyKey)
                .Index(t => t.FormKey);
            
            AddColumn("dbo.RSEmails", "PlainText", c => c.String());
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.AdvancedInventoryReports", "FormKey", "dbo.Forms");
            DropForeignKey("dbo.AdvancedInventoryReports", "CompanyKey", "dbo.Companies");
            DropIndex("dbo.AdvancedInventoryReports", new[] { "FormKey" });
            DropIndex("dbo.AdvancedInventoryReports", new[] { "CompanyKey" });
            DropIndex("dbo.AdvancedInventoryReports", new[] { "SortingId" });
            DropIndex("dbo.AdvancedInventoryReports", new[] { "UId" });
            DropColumn("dbo.RSEmails", "PlainText");
            DropTable("dbo.AdvancedInventoryReports");
        }
    }
}
