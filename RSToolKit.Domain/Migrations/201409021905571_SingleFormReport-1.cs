namespace RSToolKit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class SingleFormReport1 : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.QueryFilters", "GroupKey", "dbo.QueryFilterGroups");
            DropForeignKey("dbo.QueryFilterGroups", "ReportKey", "dbo.Reports");
            DropIndex("dbo.QueryFilterGroups", new[] { "SortingId" });
            DropIndex("dbo.QueryFilterGroups", new[] { "ReportKey" });
            DropIndex("dbo.QueryFilterGroups", new[] { "SingleFormReportKey" });
            DropIndex("dbo.QueryFilters", new[] { "GroupKey" });
            AddColumn("dbo.QueryFilters", "ReportKey", c => c.Guid());
            AddColumn("dbo.QueryFilters", "SingleFormReportKey", c => c.Guid());
            AddColumn("dbo.QueryFilters", "Order", c => c.Int(nullable: false));
            AddColumn("dbo.QueryFilters", "GroupNext", c => c.Boolean(nullable: false));
            CreateIndex("dbo.QueryFilters", "ReportKey");
            CreateIndex("dbo.QueryFilters", "SingleFormReportKey");
            AddForeignKey("dbo.QueryFilters", "ReportKey", "dbo.Reports", "UId");
            DropColumn("dbo.QueryFilters", "GroupKey");
            DropColumn("dbo.QueryFilters", "Priority");
            DropTable("dbo.QueryFilterGroups");
        }
        
        public override void Down()
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
                .PrimaryKey(t => t.UId);
            
            AddColumn("dbo.QueryFilters", "Priority", c => c.Int(nullable: false));
            AddColumn("dbo.QueryFilters", "GroupKey", c => c.Guid(nullable: false));
            DropForeignKey("dbo.QueryFilters", "ReportKey", "dbo.Reports");
            DropIndex("dbo.QueryFilters", new[] { "SingleFormReportKey" });
            DropIndex("dbo.QueryFilters", new[] { "ReportKey" });
            DropColumn("dbo.QueryFilters", "GroupNext");
            DropColumn("dbo.QueryFilters", "Order");
            DropColumn("dbo.QueryFilters", "SingleFormReportKey");
            DropColumn("dbo.QueryFilters", "ReportKey");
            CreateIndex("dbo.QueryFilters", "GroupKey");
            CreateIndex("dbo.QueryFilterGroups", "SingleFormReportKey");
            CreateIndex("dbo.QueryFilterGroups", "ReportKey");
            CreateIndex("dbo.QueryFilterGroups", "SortingId", unique: true, clustered: true);
            AddForeignKey("dbo.QueryFilterGroups", "ReportKey", "dbo.Reports", "UId");
            AddForeignKey("dbo.QueryFilters", "GroupKey", "dbo.QueryFilterGroups", "UId");
        }
    }
}
