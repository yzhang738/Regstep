namespace RSToolKit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class globalreportsforms : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.GlobalReports", "RawForms", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.GlobalReports", "RawForms");
        }
    }
}
