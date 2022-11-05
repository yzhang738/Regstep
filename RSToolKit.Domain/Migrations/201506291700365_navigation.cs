namespace RSToolKit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class navigation : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.UserTrails",
                c => new
                    {
                        Id = c.Long(nullable: false),
                        UserId = c.String(),
                        RawTrail = c.String(defaultValue: "[]"),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.UserTrails");
        }
    }
}
