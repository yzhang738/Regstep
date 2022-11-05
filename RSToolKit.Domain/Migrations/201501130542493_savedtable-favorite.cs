namespace RSToolKit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class savedtablefavorite : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.SingleFormReports", "ParentKey", c => c.Guid(nullable: false));
            AddColumn("dbo.SavedTables", "Favorite", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.SavedTables", "Favorite");
            DropColumn("dbo.SingleFormReports", "ParentKey");
        }
    }
}
