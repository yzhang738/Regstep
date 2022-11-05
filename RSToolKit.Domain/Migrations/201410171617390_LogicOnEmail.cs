namespace RSToolKit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class LogicOnEmail : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Logics", "EmailKey", c => c.Guid());
            CreateIndex("dbo.Logics", "EmailKey");
            AddForeignKey("dbo.Logics", "EmailKey", "dbo.RSEmails", "UId");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Logics", "EmailKey", "dbo.RSEmails");
            DropIndex("dbo.Logics", new[] { "EmailKey" });
            DropColumn("dbo.Logics", "EmailKey");
        }
    }
}
