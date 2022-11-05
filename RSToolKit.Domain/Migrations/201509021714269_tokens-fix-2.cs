namespace RSToolKit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class tokensfix2 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ProgressStati",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        FractionPerTick = c.Single(nullable: false),
                        Progress = c.Single(nullable: false),
                        Message = c.String(),
                        Details = c.String(),
                        Complete = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("dbo.TokenDatas", "Discriminator", c => c.String());
            AddColumn("dbo.Tokens", "RawFilters", c => c.String());
            AddColumn("dbo.Tokens", "RawHeaders", c => c.String());
            AddColumn("dbo.Tokens", "RawSortings", c => c.String());
            AddColumn("dbo.Tokens", "RecordsPerPage", c => c.Int(nullable: false));
            AddColumn("dbo.Tokens", "Name", c => c.String());
            AddColumn("dbo.Tokens", "IsReport", c => c.Boolean(nullable: false));
            AddColumn("dbo.Tokens", "Favorite", c => c.Boolean(nullable: false));
            AddColumn("dbo.Tokens", "SavedReportId", c => c.Long(nullable: false));
            DropColumn("dbo.TokenDatas", "RawOptions");
        }
        
        public override void Down()
        {
            AddColumn("dbo.TokenDatas", "RawOptions", c => c.String());
            DropColumn("dbo.Tokens", "SavedReportId");
            DropColumn("dbo.Tokens", "Favorite");
            DropColumn("dbo.Tokens", "IsReport");
            DropColumn("dbo.Tokens", "Name");
            DropColumn("dbo.Tokens", "RecordsPerPage");
            DropColumn("dbo.Tokens", "RawSortings");
            DropColumn("dbo.Tokens", "RawHeaders");
            DropColumn("dbo.Tokens", "RawFilters");
            DropColumn("dbo.TokenDatas", "Discriminator");
            DropTable("dbo.ProgressStati");
        }
    }
}
