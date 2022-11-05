namespace RSToolKit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class removeType : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.Adjustments", "Type");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Adjustments", "Type", c => c.String());
        }
    }
}
