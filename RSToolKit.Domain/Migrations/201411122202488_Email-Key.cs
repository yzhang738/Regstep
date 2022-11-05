namespace RSToolKit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class EmailKey : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.EmailSends", "EmailKey", c => c.Guid());
        }
        
        public override void Down()
        {
            DropColumn("dbo.EmailSends", "EmailKey");
        }
    }
}
