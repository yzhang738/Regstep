namespace RSToolKit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AccessLog_Type : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AccessLog", "Type", c => c.String());
            AlterColumn("dbo.AccessLog", "AccessKey", c => c.String(maxLength: 128));
            CreateIndex("dbo.AccessLog", "AccessKey");
            AddForeignKey("dbo.AccessLog", "AccessKey", "dbo.AspNetUsers", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.AccessLog", "AccessKey", "dbo.AspNetUsers");
            DropIndex("dbo.AccessLog", new[] { "AccessKey" });
            AlterColumn("dbo.AccessLog", "AccessKey", c => c.Guid());
            DropColumn("dbo.AccessLog", "Type");
        }
    }
}
