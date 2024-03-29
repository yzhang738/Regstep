namespace RSToolKit.Domain.EFDbIdentityContextMigrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class initial1 : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.AspNetUsers", "UTCOffset", c => c.String());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.AspNetUsers", "UTCOffset", c => c.Single(nullable: false));
        }
    }
}
