namespace RSToolKit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class globalReports : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.GlobalReports",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        UId = c.Guid(nullable: false),
                        CompanyKey = c.Guid(nullable: false),
                        RowVersion = c.Binary(nullable: false, fixedLength: true, timestamp: true, storeType: "rowversion"),
                        Script = c.String(),
                        Deleted = c.Boolean(nullable: false),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.UId, unique: true);
            
            CreateTable(
                "dbo.ItemAccesses",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        Target = c.Guid(nullable: false),
                        AccessType = c.Int(nullable: false),
                        Subject = c.String(),
                        Date = c.DateTimeOffset(nullable: false, precision: 7),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("dbo.Charges", "Deleted", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropIndex("dbo.GlobalReports", new[] { "UId" });
            DropColumn("dbo.Charges", "Deleted");
            DropTable("dbo.ItemAccesses");
            DropTable("dbo.GlobalReports");
        }
    }
}
