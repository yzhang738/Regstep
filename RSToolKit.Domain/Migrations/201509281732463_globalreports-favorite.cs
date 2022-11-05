namespace RSToolKit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class globalreportsfavorite : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.GlobalReports", "Favorite", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.GlobalReports", "Favorite");
        }
    }
}
