namespace RSToolKit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class numberselector : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ComponentBases", "Min", c => c.Int());
            AddColumn("dbo.ComponentBases", "Max", c => c.Int());
        }
        
        public override void Down()
        {
            DropColumn("dbo.ComponentBases", "Max");
            DropColumn("dbo.ComponentBases", "Min");
        }
    }
}
