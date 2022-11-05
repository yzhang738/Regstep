namespace RSToolKit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class registrantattended : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Registrants", "Attended", c => c.Boolean(nullable: false, defaultValue: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Registrants", "Attended");
        }
    }
}
