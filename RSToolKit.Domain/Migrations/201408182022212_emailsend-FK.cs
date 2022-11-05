namespace RSToolKit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class emailsendFK : DbMigration
    {
        public override void Up()
        {
            DropIndex("dbo.EmailEvents", new[] { "EmailSend_UId" });
            DropColumn("dbo.EmailEvents", "EmailSendKey");
            RenameColumn(table: "dbo.EmailEvents", name: "EmailSend_UId", newName: "EmailSendKey");
            AlterColumn("dbo.EmailEvents", "EmailSendKey", c => c.Guid(nullable: false));
            CreateIndex("dbo.EmailEvents", "EmailSendKey");
        }
        
        public override void Down()
        {
            DropIndex("dbo.EmailEvents", new[] { "EmailSendKey" });
            AlterColumn("dbo.EmailEvents", "EmailSendKey", c => c.Guid());
            RenameColumn(table: "dbo.EmailEvents", name: "EmailSendKey", newName: "EmailSend_UId");
            AddColumn("dbo.EmailEvents", "EmailSendKey", c => c.Guid(nullable: false));
            CreateIndex("dbo.EmailEvents", "EmailSend_UId");
        }
    }
}
