namespace RSToolKit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class formstart : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Forms", "Start", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Forms", "Start");
        }
    }
}
