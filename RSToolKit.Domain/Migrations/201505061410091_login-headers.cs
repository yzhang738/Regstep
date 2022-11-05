namespace RSToolKit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class loginheaders : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Forms", "RawLoginInformation", c => c.String(defaultValue: "[]"));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Forms", "RawLoginInformation");
        }
    }
}
