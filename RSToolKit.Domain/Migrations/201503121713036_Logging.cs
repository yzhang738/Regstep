namespace RSToolKit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Logging : DbMigration
    {
        public override void Up()
        {
            RenameColumn("dbo.Logs", "Id", "SortingId");
            RenameColumn("dbo.Logs", "Date", "DateCreated");
            AddColumn("dbo.Logs", "DateModified", c => c.DateTimeOffset(nullable: false, precision: 7));
            AddColumn("dbo.Logs", "ModificationToken", c => c.Guid(nullable: false, defaultValue: Guid.Empty));
            AddColumn("dbo.Logs", "ModifiedBy", c => c.Guid(nullable: false, defaultValue: Guid.Empty));
            DropColumn("dbo.Seaters", "Confirmation");
        }
        
        public override void Down()
        {
            RenameColumn("dbo.Logs", "SortingId", "Id");
            RenameColumn("dbo.Logs", "DateCreated", "Date");
            DropColumn("dbo.Logs", "ModifiedBy");
            DropColumn("dbo.Logs", "ModificationToken");
            DropColumn("dbo.Logs", "DateModified");
            AddColumn("dbo.Seaters", "Confirmation", c => c.String());
        }
    }
}
