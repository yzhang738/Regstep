namespace RSToolKit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class reportDat_favorite : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ReportDatas", "Favorite", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.ReportDatas", "Favorite");
        }
    }
}
