namespace RSToolKit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class removerequiredrdatavalue : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.RegistrantData", "Value", c => c.String(maxLength: 1000));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.RegistrantData", "Value", c => c.String(nullable: false, maxLength: 1000));
        }
    }
}
