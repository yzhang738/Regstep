namespace RSToolKit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class component_mapped : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ComponentBases", "MappedToKey", c => c.Guid());
            CreateIndex("dbo.ComponentBases", "MappedToKey");
            AddForeignKey("dbo.ComponentBases", "MappedToKey", "dbo.ContactHeaders", "UId");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ComponentBases", "MappedToKey", "dbo.ContactHeaders");
            DropIndex("dbo.ComponentBases", new[] { "MappedToKey" });
            DropColumn("dbo.ComponentBases", "MappedToKey");
        }
    }
}
