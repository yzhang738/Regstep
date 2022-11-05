namespace RSToolKit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class SFR : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.QueryFilterGroups",
                c => new
                    {
                        UId = c.Guid(nullable: false),
                        SortingId = c.Long(nullable: false, identity: true),
                        ReportKey = c.Guid(),
                        SingleFormReportKey = c.Guid(),
                        Order = c.Int(nullable: false),
                        Link = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.UId, clustered: false)
                .ForeignKey("dbo.Reports", t => t.ReportKey)
                .ForeignKey("dbo.SingleFormReports", t => t.SingleFormReportKey)
                .Index(t => t.SortingId, unique: true, clustered: true)
                .Index(t => t.ReportKey)
                .Index(t => t.SingleFormReportKey);
            
            CreateTable(
                "dbo.QueryFilters",
                c => new
                    {
                        UId = c.Guid(nullable: false),
                        SortingId = c.Long(nullable: false, identity: true),
                        GroupKey = c.Guid(nullable: false),
                        Link = c.Int(nullable: false),
                        Test = c.Int(nullable: false),
                        ActingOn = c.String(),
                        Value = c.String(),
                        Priority = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.UId, clustered: false)
                .ForeignKey("dbo.QueryFilterGroups", t => t.GroupKey)
                .Index(t => t.SortingId, unique: true, clustered: true)
                .Index(t => t.GroupKey);
            
            CreateTable(
                "dbo.SingleFormReports",
                c => new
                    {
                        UId = c.Guid(nullable: false),
                        SortingId = c.Long(nullable: false, identity: true),
                        Name = c.String(maxLength: 250),
                        CompanyKey = c.Guid(nullable: false),
                        DateCreated = c.DateTimeOffset(nullable: false, precision: 7),
                        DateModified = c.DateTimeOffset(nullable: false, precision: 7),
                        Permission = c.String(),
                        ModificationToken = c.Guid(nullable: false),
                        ModifiedBy = c.Guid(nullable: false),
                        Owner = c.Guid(nullable: false),
                        Group = c.Guid(nullable: false),
                        FormKey = c.Guid(nullable: false),
                        RawVariables = c.String(),
                        SortOn = c.String(),
                        Ascending = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.UId, clustered: false)
                .ForeignKey("dbo.Companies", t => t.CompanyKey)
                .ForeignKey("dbo.Forms", t => t.FormKey)
                .Index(t => t.UId)
                .Index(t => t.SortingId, unique: true, clustered: true)
                .Index(t => t.CompanyKey)
                .Index(t => t.FormKey);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.SingleFormReports", "FormKey", "dbo.Forms");
            DropForeignKey("dbo.QueryFilterGroups", "SingleFormReportKey", "dbo.SingleFormReports");
            DropForeignKey("dbo.SingleFormReports", "CompanyKey", "dbo.Companies");
            DropForeignKey("dbo.QueryFilterGroups", "ReportKey", "dbo.Reports");
            DropForeignKey("dbo.QueryFilters", "GroupKey", "dbo.QueryFilterGroups");
            DropIndex("dbo.SingleFormReports", new[] { "FormKey" });
            DropIndex("dbo.SingleFormReports", new[] { "CompanyKey" });
            DropIndex("dbo.SingleFormReports", new[] { "SortingId" });
            DropIndex("dbo.SingleFormReports", new[] { "UId" });
            DropIndex("dbo.QueryFilters", new[] { "GroupKey" });
            DropIndex("dbo.QueryFilters", new[] { "SortingId" });
            DropIndex("dbo.QueryFilterGroups", new[] { "SingleFormReportKey" });
            DropIndex("dbo.QueryFilterGroups", new[] { "ReportKey" });
            DropIndex("dbo.QueryFilterGroups", new[] { "SortingId" });
            DropTable("dbo.SingleFormReports");
            DropTable("dbo.QueryFilters");
            DropTable("dbo.QueryFilterGroups");
        }
    }
}
