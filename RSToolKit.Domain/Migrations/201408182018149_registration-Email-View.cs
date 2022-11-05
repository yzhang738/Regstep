namespace RSToolKit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class registrationEmailView : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.RegistrantNotes",
                c => new
                    {
                        SortingId = c.Long(nullable: false, identity: true),
                        UId = c.Guid(nullable: false),
                        Note = c.String(nullable: false),
                        ModifiedBy = c.Guid(nullable: false),
                        Date = c.DateTimeOffset(nullable: false, precision: 7),
                    })
                .PrimaryKey(t => t.SortingId)
                .ForeignKey("dbo.Registrants", t => t.UId)
                .Index(t => t.UId);
            
            AddColumn("dbo.EmailSends", "EmailDescription", c => c.String());
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.RegistrantNotes", "UId", "dbo.Registrants");
            DropIndex("dbo.RegistrantNotes", new[] { "UId" });
            DropColumn("dbo.EmailSends", "EmailDescription");
            DropTable("dbo.RegistrantNotes");
        }
    }
}
