namespace RSToolKit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class hideReview : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ComponentBases", "HideReview", c => c.Boolean());
        }
        
        public override void Down()
        {
            DropColumn("dbo.ComponentBases", "HideReview");
        }
    }
}
