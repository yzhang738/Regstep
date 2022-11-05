namespace RSToolKit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemoveRequiredComponentRegistrationData : DbMigration
    {
        public override void Up()
        {
            DropIndex("dbo.RegistrantData", new[] { "VariableUId" });
            AlterColumn("dbo.RegistrantData", "VariableUId", c => c.Guid());
            CreateIndex("dbo.RegistrantData", "VariableUId");
        }
        
        public override void Down()
        {
            DropIndex("dbo.RegistrantData", new[] { "VariableUId" });
            AlterColumn("dbo.RegistrantData", "VariableUId", c => c.Guid(nullable: false));
            CreateIndex("dbo.RegistrantData", "VariableUId");
        }
    }
}
