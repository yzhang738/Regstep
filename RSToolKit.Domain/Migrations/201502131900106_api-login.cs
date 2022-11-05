namespace RSToolKit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class apilogin : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AspNetUsers", "ApiToken", c => c.String());
            AddColumn("dbo.AspNetUsers", "ApiTokenExpiration", c => c.DateTimeOffset(precision: 7));
            AddColumn("dbo.Registrants", "ApiToken", c => c.String());
            AddColumn("dbo.Registrants", "ApiTokenExpiration", c => c.DateTimeOffset(precision: 7));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Registrants", "ApiTokenExpiration");
            DropColumn("dbo.Registrants", "ApiToken");
            DropColumn("dbo.AspNetUsers", "ApiTokenExpiration");
            DropColumn("dbo.AspNetUsers", "ApiToken");
        }
    }
}
