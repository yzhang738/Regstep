namespace RSToolKit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Cloudfinishes1 : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.RSEmails", "Areas");
        }
        
        public override void Down()
        {
            AddColumn("dbo.RSEmails", "Areas", c => c.String());
        }
    }
}
