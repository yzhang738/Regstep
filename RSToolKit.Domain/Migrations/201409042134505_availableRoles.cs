namespace RSToolKit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class availableRoles : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.AvailableRoles",
                c => new
                    {
                        UId = c.Guid(nullable: false),
                        SortingId = c.Long(nullable: false, identity: true),
                        RoleKey = c.String(maxLength: 128),
                        CompanyKey = c.Guid(nullable: false),
                        TotalAvailable = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.UId, clustered: false)
                .ForeignKey("dbo.Companies", t => t.CompanyKey)
                .ForeignKey("dbo.AspNetRoles", t => t.RoleKey)
                .Index(t => t.SortingId, clustered: true)
                .Index(t => t.RoleKey)
                .Index(t => t.CompanyKey);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.AvailableRoles", "RoleKey", "dbo.AspNetRoles");
            DropForeignKey("dbo.AvailableRoles", "CompanyKey", "dbo.Companies");
            DropIndex("dbo.AvailableRoles", new[] { "CompanyKey" });
            DropIndex("dbo.AvailableRoles", new[] { "RoleKey" });
            DropTable("dbo.AvailableRoles");
        }
    }
}
