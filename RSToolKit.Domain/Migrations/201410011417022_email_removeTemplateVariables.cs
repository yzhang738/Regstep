namespace RSToolKit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class email_removeTemplateVariables : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.EmailTemplates", "ParentUId");
            DropColumn("dbo.EmailTemplates", "DateCreated");
            DropColumn("dbo.EmailTemplates", "DateModified");
            DropColumn("dbo.EmailTemplates", "Owner");
            DropColumn("dbo.EmailTemplates", "Group");
            DropColumn("dbo.EmailTemplates", "Permission");
            DropColumn("dbo.EmailTemplates", "ModificationToken");
            DropColumn("dbo.EmailTemplates", "ModifiedBy");
        }
        
        public override void Down()
        {
            AddColumn("dbo.EmailTemplates", "ModifiedBy", c => c.Guid(nullable: false));
            AddColumn("dbo.EmailTemplates", "ModificationToken", c => c.Guid(nullable: false));
            AddColumn("dbo.EmailTemplates", "Permission", c => c.String(maxLength: 3));
            AddColumn("dbo.EmailTemplates", "Group", c => c.Guid(nullable: false));
            AddColumn("dbo.EmailTemplates", "Owner", c => c.Guid(nullable: false));
            AddColumn("dbo.EmailTemplates", "DateModified", c => c.DateTimeOffset(nullable: false, precision: 7));
            AddColumn("dbo.EmailTemplates", "DateCreated", c => c.DateTimeOffset(nullable: false, precision: 7));
            AddColumn("dbo.EmailTemplates", "ParentUId", c => c.Guid(nullable: false));
        }
    }
}
