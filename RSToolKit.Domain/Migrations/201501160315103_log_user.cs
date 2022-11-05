namespace RSToolKit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class log_user : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Logs", "UserKey", c => c.String(maxLength: 128, nullable: true));
            CreateIndex("dbo.Logs", "UserKey");
            AddForeignKey("dbo.Logs", "UserKey", "dbo.AspNetUsers", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Logs", "UserKey", "dbo.AspNetUsers");
            DropIndex("dbo.Logs", new[] { "UserKey" });
            DropColumn("dbo.Logs", "UserKey");
        }
    }
}
