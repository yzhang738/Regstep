namespace RSToolKit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class email : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.EmailSends",
                c => new
                    {
                        UId = c.Guid(nullable: false),
                        SortingId = c.Long(nullable: false, identity: true),
                        RegistrantKey = c.Guid(),
                        ContactKey = c.Guid(),
                        Recipient = c.String(),
                        DateSent = c.DateTimeOffset(nullable: false, precision: 7),
                    })
                .PrimaryKey(t => t.UId)
                .ForeignKey("dbo.Contacts", t => t.ContactKey)
                .ForeignKey("dbo.Registrants", t => t.RegistrantKey)
                .Index(t => t.RegistrantKey)
                .Index(t => t.ContactKey);
            
            AddColumn("dbo.EmailEvents", "EmailSendKey", c => c.Guid(nullable: false));
            AddColumn("dbo.EmailEvents", "Notes", c => c.String());
            AddColumn("dbo.EmailEvents", "EmailSend_UId", c => c.Guid());
            CreateIndex("dbo.EmailEvents", "EmailSend_UId");
            AddForeignKey("dbo.EmailEvents", "EmailSend_UId", "dbo.EmailSends", "UId");
            DropColumn("dbo.EmailEvents", "EmailUId");
            DropColumn("dbo.EmailEvents", "Cats");
            DropColumn("dbo.EmailEvents", "Params");
        }
        
        public override void Down()
        {
            AddColumn("dbo.EmailEvents", "Params", c => c.String());
            AddColumn("dbo.EmailEvents", "Cats", c => c.String());
            AddColumn("dbo.EmailEvents", "EmailUId", c => c.Guid(nullable: false));
            DropForeignKey("dbo.EmailSends", "RegistrantKey", "dbo.Registrants");
            DropForeignKey("dbo.EmailEvents", "EmailSend_UId", "dbo.EmailSends");
            DropForeignKey("dbo.EmailSends", "ContactKey", "dbo.Contacts");
            DropIndex("dbo.EmailEvents", new[] { "EmailSend_UId" });
            DropIndex("dbo.EmailSends", new[] { "ContactKey" });
            DropIndex("dbo.EmailSends", new[] { "RegistrantKey" });
            DropColumn("dbo.EmailEvents", "EmailSend_UId");
            DropColumn("dbo.EmailEvents", "Notes");
            DropColumn("dbo.EmailEvents", "EmailSendKey");
            DropTable("dbo.EmailSends");
        }
    }
}
