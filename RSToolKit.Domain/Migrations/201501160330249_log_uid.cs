namespace RSToolKit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class log_uid : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Logs", "UId", c => c.Guid());
            CreateIndex("dbo.Logs", "UId");
        }
        
        public override void Down()
        {
            DropIndex("dbo.Logs", new[] { "UId" });
            DropColumn("dbo.Logs", "UId");
        }
    }
}
