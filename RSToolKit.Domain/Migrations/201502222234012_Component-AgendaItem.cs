namespace RSToolKit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ComponentAgendaItem : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ComponentBases", "AgendaItem", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.ComponentBases", "AgendaItem");
        }
    }
}
