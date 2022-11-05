namespace RSToolKit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class navigation2 : DbMigration
    {
        public override void Up()
        {
            DropPrimaryKey("dbo.UserTrails");
            AlterColumn("dbo.UserTrails", "Id", c => c.Long(nullable: false, identity: true));
            AddPrimaryKey("dbo.UserTrails", "Id");
        }
        
        public override void Down()
        {
            DropPrimaryKey("dbo.UserTrails");
            AlterColumn("dbo.UserTrails", "Id", c => c.Long(nullable: false));
            AddPrimaryKey("dbo.UserTrails", "Id");
        }
    }
}
