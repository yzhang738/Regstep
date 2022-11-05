namespace RSToolKit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class logNotes : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.LogNotes",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        LogKey = c.Long(nullable: false),
                        Note = c.String(nullable: false, maxLength: 4000),
                        Status = c.Int(nullable: false),
                        UserKey = c.String(maxLength: 128),
                        Date = c.DateTimeOffset(nullable: false, precision: 7),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Logs", t => t.LogKey)
                .ForeignKey("dbo.AspNetUsers", t => t.UserKey)
                .Index(t => t.LogKey)
                .Index(t => t.UserKey);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.LogNotes", "UserKey", "dbo.AspNetUsers");
            DropForeignKey("dbo.LogNotes", "LogKey", "dbo.Logs");
            DropIndex("dbo.LogNotes", new[] { "UserKey" });
            DropIndex("dbo.LogNotes", new[] { "LogKey" });
            DropTable("dbo.LogNotes");
        }
    }
}
