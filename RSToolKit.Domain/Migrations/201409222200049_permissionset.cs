namespace RSToolKit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class permissionset : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.PermissionSets",
                c => new
                    {
                        Target = c.Guid(nullable: false),
                        Owner = c.Guid(nullable: false),
                        SortingId = c.Long(nullable: false, identity: true),
                        Read = c.Boolean(nullable: false),
                        Execute = c.Boolean(nullable: false),
                        Write = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => new { t.Target, t.Owner }, clustered: false)
                .Index(t => t.SortingId, clustered: true);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.PermissionSets");
        }
    }
}
