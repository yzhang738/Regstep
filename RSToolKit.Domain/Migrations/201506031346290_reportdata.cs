namespace RSToolKit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class reportdata : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ReportDatas",
                c => new
                    {
                        SortingId = c.Long(nullable: false, identity: true),
                        Name = c.String(nullable: false),
                        DateCreated = c.DateTimeOffset(nullable: false, precision: 7),
                        DateModified = c.DateTimeOffset(nullable: false, precision: 7),
                        ModificationToken = c.Guid(nullable: false),
                        ModifiedBy = c.Guid(nullable: false),
                        UId = c.Guid(nullable: false),
                        TableId = c.Guid(nullable: false),
                        RawHeaders = c.String(),
                        RawSortings = c.String(),
                        RawFilters = c.String(),
                        Count = c.Boolean(nullable: false),
                        Average = c.Boolean(nullable: false),
                        Graph = c.Boolean(nullable: false),
                        RecordsPerPage = c.Long(nullable: false),
                    })
                .PrimaryKey(t => t.SortingId)
                .Index(t => t.UId);
            
            DropTable("dbo.Constants");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.Constants",
                c => new
                    {
                        Key = c.String(nullable: false, maxLength: 250),
                        SortingId = c.Long(nullable: false, identity: true),
                        Value = c.String(),
                        Type = c.String(maxLength: 100),
                    })
                .PrimaryKey(t => t.Key);
            
            DropIndex("dbo.ReportDatas", new[] { "UId" });
            DropTable("dbo.ReportDatas");
        }
    }
}
