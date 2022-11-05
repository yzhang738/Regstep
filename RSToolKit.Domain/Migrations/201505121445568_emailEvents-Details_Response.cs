namespace RSToolKit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class emailEventsDetails_Response : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.EmailEvents", "Details", c => c.String());
            AddColumn("dbo.EmailEvents", "Response", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.EmailEvents", "Response");
            DropColumn("dbo.EmailEvents", "Details");
        }
    }
}
