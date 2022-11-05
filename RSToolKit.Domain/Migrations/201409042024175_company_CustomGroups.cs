namespace RSToolKit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class company_CustomGroups : DbMigration
    {
        public override void Up()
        {
            DropPrimaryKey("dbo.CustomGroupUsers");
            RenameTable(name: "dbo.CustomGroupUsers", newName: "UserCustomGroups");
            AddPrimaryKey("dbo.UserCustomGroups", new[] { "User_Id", "CustomGroup_UId" });
        }
        
        public override void Down()
        {
            DropPrimaryKey("dbo.UserCustomGroups");
            RenameTable(name: "dbo.UserCustomGroups", newName: "CustomGroupUsers");
            AddPrimaryKey("dbo.CustomGroupUsers", new[] { "CustomGroup_UId", "User_Id" });
        }
    }
}
