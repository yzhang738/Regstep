namespace RSToolKit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class files : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.RegistrantData", "FileKey");
        }
        
        public override void Down()
        {
            AddColumn("dbo.RegistrantData", "FileKey", c => c.Guid());
        }
    }
}
