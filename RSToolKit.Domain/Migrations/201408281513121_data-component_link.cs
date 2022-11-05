namespace RSToolKit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class datacomponent_link : DbMigration
    {
        public override void Up()
        {
            CreateIndex("dbo.RegistrantData", "VariableUId");
            AddForeignKey("dbo.RegistrantData", "VariableUId", "dbo.ComponentBases", "UId");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.RegistrantData", "VariableUId", "dbo.ComponentBases");
            DropIndex("dbo.RegistrantData", new[] { "VariableUId" });
        }
    }
}
