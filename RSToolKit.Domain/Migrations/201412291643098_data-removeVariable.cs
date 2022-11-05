namespace RSToolKit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class dataremoveVariable : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.RegistrantData", "Variable");
            DropColumn("dbo.RegistrantData", "Descriminator");
        }
        
        public override void Down()
        {
            AddColumn("dbo.RegistrantData", "Descriminator", c => c.String(nullable: false, maxLength: 250));
            AddColumn("dbo.RegistrantData", "Variable", c => c.String(nullable: false, maxLength: 250));
        }
    }
}
