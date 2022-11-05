namespace RSToolKit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class coordinator : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Forms", "CoordinatorName", c => c.String());
            AddColumn("dbo.Forms", "CoordinatorPhone", c => c.String());
            AddColumn("dbo.Forms", "CoordinatorEmail", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Forms", "CoordinatorEmail");
            DropColumn("dbo.Forms", "CoordinatorPhone");
            DropColumn("dbo.Forms", "CoordinatorName");
        }
    }
}
