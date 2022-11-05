namespace RSToolKit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class savedtable : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.SavedTables",
                c => new
                    {
                        UId = c.Guid(nullable: false),
                        ParentKey = c.Guid(nullable: false),
                        CompanyKey = c.Guid(nullable: false),
                        Name = c.String(),
                        DateCreated = c.DateTimeOffset(nullable: false, precision: 7),
                        DateModified = c.DateTimeOffset(nullable: false, precision: 7),
                        Owner = c.Guid(nullable: false),
                        Group = c.Guid(nullable: false),
                        Permission = c.String(),
                        ModificationToken = c.Guid(nullable: false),
                        ModifiedBy = c.Guid(nullable: false),
                        SortingId = c.Long(nullable: false),
                        RawJtable = c.String(),
                    })
                .PrimaryKey(t => t.UId, clustered: false)
                .ForeignKey("dbo.Companies", t => t.CompanyKey)
                .Index(t => t.CompanyKey)
                .Index(t => t.SortingId, clustered: true);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.SavedTables", "CompanyKey", "dbo.Companies");
            DropIndex("dbo.SavedTables", new[] { "SortingId" });
            DropIndex("dbo.SavedTables", new[] { "CompanyKey" });
            DropTable("dbo.SavedTables");
        }
    }
}
