namespace RSToolKit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class merchantadjustment : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Adjustments",
                c => new
                    {
                        SortingId = c.Long(nullable: false, identity: true),
                        UId = c.Guid(nullable: false),
                        DateCreated = c.DateTimeOffset(nullable: false, precision: 7),
                        DateModified = c.DateTimeOffset(nullable: false, precision: 7),
                        Owner = c.Guid(nullable: false),
                        Group = c.Guid(nullable: false),
                        ModifiedBy = c.Guid(nullable: false),
                        ModificationToken = c.Guid(nullable: false),
                        Permission = c.String(maxLength: 3),
                        Name = c.String(),
                        Ammount = c.Decimal(nullable: false, precision: 18, scale: 2),
                    })
                .PrimaryKey(t => t.SortingId)
                .ForeignKey("dbo.Registrants", t => t.UId)
                .Index(t => t.UId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Adjustments", "UId", "dbo.Registrants");
            DropIndex("dbo.Adjustments", new[] { "UId" });
            DropTable("dbo.Adjustments");
        }
    }
}
