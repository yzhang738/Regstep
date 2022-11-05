namespace RSToolKit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class fixtokens : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Tokens",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        SortingId = c.Long(nullable: false, identity: true),
                        Key = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropIndex("dbo.Tokens", new[] { "Id" });
            DropTable("dbo.Tokens");
        }
    }
}
