namespace RSToolKit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class emailEventsAddBccList : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.EmailSends", "RawIgnoreEventsFrom", c => c.String(defaultValue: "[]"));
            AddColumn("dbo.EmailEvents", "Email", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.EmailEvents", "Email");
            DropColumn("dbo.EmailSends", "RawIgnoreEventsFrom");
        }
    }
}
