namespace RSToolKit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class formeventinfo : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Forms", "EventTimeZone", c => c.String());
            AddColumn("dbo.Registrants", "RawAttendace", c => c.String());
            AlterColumn("dbo.Forms", "EventStart", c => c.DateTime());
            AlterColumn("dbo.Forms", "EventEnd", c => c.DateTime());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Forms", "EventEnd", c => c.DateTimeOffset(precision: 7));
            AlterColumn("dbo.Forms", "EventStart", c => c.DateTimeOffset(precision: 7));
            DropColumn("dbo.Registrants", "RawAttendace");
            DropColumn("dbo.Forms", "EventTimeZone");
        }
    }
}
