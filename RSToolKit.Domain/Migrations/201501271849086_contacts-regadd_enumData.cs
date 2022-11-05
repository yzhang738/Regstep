namespace RSToolKit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class contactsregadd_enumData : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Registrants", "ContactKey", c => c.Guid());
            DropColumn("dbo.ContactHeaders", "Descriminator");
            AddColumn("dbo.ContactHeaders", "Descriminator", c => c.Int(nullable: false, defaultValue: 6));
            CreateIndex("dbo.Registrants", "ContactKey");
            AddForeignKey("dbo.Registrants", "ContactKey", "dbo.Contacts", "UId");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Registrants", "ContactKey", "dbo.Contacts");
            DropIndex("dbo.Registrants", new[] { "ContactKey" });
            AddColumn("dbo.ContactHeaders", "Descriminator", c => c.String(unicode: true));
            DropColumn("dbo.ContactHeaders", "Descriminator");
            DropColumn("dbo.Registrants", "ContactKey");
        }
    }
}
