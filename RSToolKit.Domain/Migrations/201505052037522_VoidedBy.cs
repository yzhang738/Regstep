namespace RSToolKit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class VoidedBy : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Adjustments", "VoidedBy", c => c.String(defaultValue: ""));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Adjustments", "VoidedBy");
        }
    }
}
