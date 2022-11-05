namespace RSToolKit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class rename : DbMigration
    {
        public override void Up()
        {
            RenameTable(name: "dbo.ComponentBases", newName: "Components");
            RenameTable(name: "dbo.ComponentBaseAudiences", newName: "ComponentAudiences");
            RenameColumn(table: "dbo.ComponentAudiences", name: "ComponentBase_UId", newName: "Component_UId");
            RenameIndex(table: "dbo.ComponentAudiences", name: "IX_ComponentBase_UId", newName: "IX_Component_UId");
        }
        
        public override void Down()
        {
            RenameIndex(table: "dbo.ComponentAudiences", name: "IX_Component_UId", newName: "IX_ComponentBase_UId");
            RenameColumn(table: "dbo.ComponentAudiences", name: "Component_UId", newName: "ComponentBase_UId");
            RenameTable(name: "dbo.ComponentAudiences", newName: "ComponentBaseAudiences");
            RenameTable(name: "dbo.Components", newName: "ComponentBases");
        }
    }
}
