namespace RSToolKit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class initialformbuilder_FormRSVPAccept_RSVPDecline : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Forms", "RSVPAccept", c => c.String());
            AddColumn("dbo.Forms", "RSVPDecline", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Forms", "RSVPDecline");
            DropColumn("dbo.Forms", "RSVPAccept");
        }
    }
}
