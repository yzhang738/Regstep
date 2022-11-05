namespace RSToolKit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addProtectedName : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Charges", "Name", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Charges", "Name");
        }
    }
}
