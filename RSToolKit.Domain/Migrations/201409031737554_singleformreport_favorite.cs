namespace RSToolKit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class singleformreport_favorite : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.RSEmails", "Areas", c => c.String());
            AddColumn("dbo.SingleFormReports", "Favorite", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.SingleFormReports", "Favorite");
            DropColumn("dbo.RSEmails", "Areas");
        }
    }
}
