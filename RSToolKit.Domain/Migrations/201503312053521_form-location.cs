namespace RSToolKit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class formlocation : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Forms", "Location", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Forms", "Location");
        }
    }
}
