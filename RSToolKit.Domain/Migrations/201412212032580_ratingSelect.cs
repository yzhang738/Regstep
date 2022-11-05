namespace RSToolKit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ratingSelect : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Forms", "Survey", c => c.Boolean(nullable: false, defaultValue: false));
            AddColumn("dbo.Forms", "ParentFormKey", c => c.Guid());
            AddColumn("dbo.ComponentBases", "MappedComponentKey", c => c.Guid());
            AddColumn("dbo.ComponentBases", "MaxRating", c => c.Int());
            AddColumn("dbo.ComponentBases", "MinRating", c => c.Int());
            AddColumn("dbo.ComponentBases", "RatingSelectType", c => c.Int());
            AddColumn("dbo.ComponentBases", "Step", c => c.Int());
            AddColumn("dbo.ComponentBases", "Color", c => c.String());
            CreateIndex("dbo.Forms", "ParentFormKey");
            CreateIndex("dbo.ComponentBases", "MappedComponentKey");
            AddForeignKey("dbo.ComponentBases", "MappedComponentKey", "dbo.ComponentBases", "UId");
            AddForeignKey("dbo.Forms", "ParentFormKey", "dbo.Forms", "UId");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Forms", "ParentFormKey", "dbo.Forms");
            DropForeignKey("dbo.ComponentBases", "MappedComponentKey", "dbo.ComponentBases");
            DropIndex("dbo.ComponentBases", new[] { "MappedComponentKey" });
            DropIndex("dbo.Forms", new[] { "ParentFormKey" });
            DropColumn("dbo.ComponentBases", "Color");
            DropColumn("dbo.ComponentBases", "Step");
            DropColumn("dbo.ComponentBases", "RatingSelectType");
            DropColumn("dbo.ComponentBases", "MinRating");
            DropColumn("dbo.ComponentBases", "MaxRating");
            DropColumn("dbo.ComponentBases", "MappedComponentKey");
            DropColumn("dbo.Forms", "ParentFormKey");
            DropColumn("dbo.Forms", "Survey");
        }
    }
}
