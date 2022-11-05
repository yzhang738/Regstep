namespace RSToolKit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Unsubscribe : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Unsubscribes",
                c => new
                    {
                        UId = c.Guid(nullable: false),
                        SortingId = c.Long(nullable: false, identity: true),
                        Email = c.String(),
                        UnsubscribeFrom = c.Guid(nullable: false),
                        DateSubmitted = c.DateTimeOffset(nullable: false, precision: 7),
                    })
                .PrimaryKey(t => t.UId, clustered: false)
                .Index(t => t.SortingId, clustered: true);
        }
        
        public override void Down()
        {
            DropTable("dbo.Unsubscribes");
        }
    }
}
