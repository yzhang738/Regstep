namespace RSToolKit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class permissionsetkey_change : DbMigration
    {
        public override void Up()
        {
            DropPrimaryKey("dbo.PermissionSets");
            AddPrimaryKey("dbo.PermissionSets", "SortingId");
        }
        
        public override void Down()
        {
            DropPrimaryKey("dbo.PermissionSets");
            AddPrimaryKey("dbo.PermissionSets", new[] { "Target", "Owner" });
        }
    }
}
