namespace RSToolKit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class eventstart : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Forms", "EventStart", c => c.DateTimeOffset(precision: 7));
            AddColumn("dbo.Forms", "EventEnd", c => c.DateTimeOffset(precision: 7));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Forms", "EventEnd");
            DropColumn("dbo.Forms", "EventStart");
        }
    }
}
