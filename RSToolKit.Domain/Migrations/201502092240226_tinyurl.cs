namespace RSToolKit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class tinyurl : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.TinyUrls",
                c => new
                    {
                        UId = c.Guid(nullable: false),
                        SortingId = c.Long(nullable: false),
                        CompanyKey = c.Guid(nullable: false),
                        Url = c.String(nullable: false, maxLength: 25),
                    })
                .PrimaryKey(t => t.UId, clustered: false)
                .ForeignKey("dbo.Companies", t => t.CompanyKey)
                .ForeignKey("dbo.Forms", t => t.UId)
                .Index(t => t.UId)
                .Index(t => t.SortingId, clustered: true)
                .Index(t => t.CompanyKey)
                .Index(t => t.Url, unique: true);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.TinyUrls", "UId", "dbo.Forms");
            DropForeignKey("dbo.TinyUrls", "CompanyKey", "dbo.Companies");
            DropIndex("dbo.TinyUrls", new[] { "Url" });
            DropIndex("dbo.TinyUrls", new[] { "CompanyKey" });
            DropIndex("dbo.TinyUrls", new[] { "SortingId" });
            DropIndex("dbo.TinyUrls", new[] { "UId" });
            DropTable("dbo.TinyUrls");
        }
    }
}
