namespace RSToolKit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class file : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.RegistrantFiles",
                c => new
                    {
                        UId = c.Guid(nullable: false),
                        SortingId = c.Long(nullable: false, identity: true),
                        FileType = c.String(nullable: false),
                        Extension = c.String(nullable: false),
                        BinaryData = c.Binary(nullable: false),
                    })
                .PrimaryKey(t => t.UId)
                .ForeignKey("dbo.RegistrantData", t => t.UId)
                .Index(t => t.UId);
            
            AddColumn("dbo.RegistrantData", "FileKey", c => c.Guid());
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.RegistrantFiles", "UId", "dbo.RegistrantData");
            DropIndex("dbo.RegistrantFiles", new[] { "UId" });
            DropColumn("dbo.RegistrantData", "FileKey");
            DropTable("dbo.RegistrantFiles");
        }
    }
}
