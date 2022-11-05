namespace RSToolKit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Formatting : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ComponentBases", "Formatting", c => c.Int());
        }
        
        public override void Down()
        {
            DropColumn("dbo.ComponentBases", "Formatting");
        }
    }
}
