namespace RSToolKit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Registrant_propStatusDate : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Registrants", "StatusDate", c => c.DateTimeOffset(nullable: false, precision: 7));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Registrants", "StatusDate");
        }
    }
}
