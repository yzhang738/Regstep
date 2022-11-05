namespace RSToolKit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class cloud : DbMigration
    {
        public override void Up()
        {
            DropIndex("dbo.Reports", new[] { "Company_UId" });
            DropIndex("dbo.SqlHeaders", new[] { "SqlTableUId" });
            DropIndex("dbo.SqlHeaders", new[] { "ReportHeaderUId" });
            RenameColumn(table: "dbo.Reports", name: "Company_UId", newName: "CompanyKey");
            RenameColumn(table: "dbo.ReportHeaders", name: "ReportUId", newName: "ReportKey");
            RenameColumn(table: "dbo.SqlTables", name: "ReportUId", newName: "ReportKey");
            RenameColumn(table: "dbo.SqlHeaders", name: "ReportHeaderUId", newName: "ReportHeaderKey");
            RenameColumn(table: "dbo.SqlHeaders", name: "SqlTableUId", newName: "SqlTableKey");
            RenameIndex(table: "dbo.ReportHeaders", name: "IX_ReportUId", newName: "IX_ReportKey");
            RenameIndex(table: "dbo.SqlTables", name: "IX_ReportUId", newName: "IX_ReportKey");
            AddColumn("dbo.SqlHeaders", "DataKey", c => c.String());
            AddColumn("dbo.SqlTables", "DataKey", c => c.Guid(nullable: false));
            AddColumn("dbo.SqlTables", "DataDescriminator", c => c.String(maxLength: 150));
            AlterColumn("dbo.Reports", "CompanyKey", c => c.Guid(nullable: false));
            AlterColumn("dbo.SqlHeaders", "SqlTableKey", c => c.Guid(nullable: false));
            AlterColumn("dbo.SqlHeaders", "ReportHeaderKey", c => c.Guid(nullable: false));
            CreateIndex("dbo.Reports", "CompanyKey");
            CreateIndex("dbo.SqlHeaders", "ReportHeaderKey");
            CreateIndex("dbo.SqlHeaders", "SqlTableKey");
            DropColumn("dbo.Reports", "Company");
            DropColumn("dbo.ReportHeaders", "Company");
            DropColumn("dbo.SqlHeaders", "Company");
            DropColumn("dbo.SqlHeaders", "SqlHeaderUId");
            DropColumn("dbo.SqlTables", "Company");
            DropColumn("dbo.SqlTables", "TableUId");
            DropColumn("dbo.SqlTables", "TableType");
            DropColumn("dbo.SqlTables", "TableName");
        }
        
        public override void Down()
        {
            AddColumn("dbo.SqlTables", "TableName", c => c.String(maxLength: 250));
            AddColumn("dbo.SqlTables", "TableType", c => c.String(maxLength: 150));
            AddColumn("dbo.SqlTables", "TableUId", c => c.Guid(nullable: false));
            AddColumn("dbo.SqlTables", "Company", c => c.Guid(nullable: false));
            AddColumn("dbo.SqlHeaders", "SqlHeaderUId", c => c.String());
            AddColumn("dbo.SqlHeaders", "Company", c => c.Guid(nullable: false));
            AddColumn("dbo.ReportHeaders", "Company", c => c.Guid(nullable: false));
            AddColumn("dbo.Reports", "Company", c => c.Guid(nullable: false));
            DropIndex("dbo.SqlHeaders", new[] { "SqlTableKey" });
            DropIndex("dbo.SqlHeaders", new[] { "ReportHeaderKey" });
            DropIndex("dbo.Reports", new[] { "CompanyKey" });
            AlterColumn("dbo.SqlHeaders", "ReportHeaderKey", c => c.Guid());
            AlterColumn("dbo.SqlHeaders", "SqlTableKey", c => c.Guid());
            AlterColumn("dbo.Reports", "CompanyKey", c => c.Guid());
            DropColumn("dbo.SqlTables", "DataDescriminator");
            DropColumn("dbo.SqlTables", "DataKey");
            DropColumn("dbo.SqlHeaders", "DataKey");
            RenameIndex(table: "dbo.SqlTables", name: "IX_ReportKey", newName: "IX_ReportUId");
            RenameIndex(table: "dbo.ReportHeaders", name: "IX_ReportKey", newName: "IX_ReportUId");
            RenameColumn(table: "dbo.SqlHeaders", name: "SqlTableKey", newName: "SqlTableUId");
            RenameColumn(table: "dbo.SqlHeaders", name: "ReportHeaderKey", newName: "ReportHeaderUId");
            RenameColumn(table: "dbo.SqlTables", name: "ReportKey", newName: "ReportUId");
            RenameColumn(table: "dbo.ReportHeaders", name: "ReportKey", newName: "ReportUId");
            RenameColumn(table: "dbo.Reports", name: "CompanyKey", newName: "Company_UId");
            CreateIndex("dbo.SqlHeaders", "ReportHeaderUId");
            CreateIndex("dbo.SqlHeaders", "SqlTableUId");
            CreateIndex("dbo.Reports", "Company_UId");
        }
    }
}
