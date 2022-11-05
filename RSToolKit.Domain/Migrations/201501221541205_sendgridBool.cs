namespace RSToolKit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class sendgridBool : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.SmtpServers", "SendGrid", c => c.Boolean(nullable: false, defaultValue: true));
        }
        
        public override void Down()
        {
            DropColumn("dbo.SmtpServers", "SendGrid");
        }
    }
}
