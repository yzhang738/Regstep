namespace RSToolKit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class registrantremoverequired : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Registrants", "Email", c => c.String(maxLength: 250));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Registrants", "Email", c => c.String(nullable: false, maxLength: 250));
        }
    }
}
