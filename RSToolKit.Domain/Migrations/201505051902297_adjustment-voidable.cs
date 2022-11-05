namespace RSToolKit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class adjustmentvoidable : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Adjustments", "Voided", c => c.Boolean(nullable: false, defaultValue: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Adjustments", "Voided");
        }
    }
}
