namespace RSToolKit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class email_updates1 : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.EmailAreas", "Company");
            DropColumn("dbo.EmailAreas", "Name");
            DropColumn("dbo.EmailAreas", "DateCreated");
            DropColumn("dbo.EmailAreas", "DateModified");
            DropColumn("dbo.EmailAreas", "Owner");
            DropColumn("dbo.EmailAreas", "Group");
            DropColumn("dbo.EmailAreas", "Permission");
            DropColumn("dbo.EmailAreas", "ModificationToken");
            DropColumn("dbo.EmailAreas", "ModifiedBy");
            DropColumn("dbo.EmailTemplateAreas", "Name");
            DropColumn("dbo.EmailTemplateAreas", "DateCreated");
            DropColumn("dbo.EmailTemplateAreas", "DateModified");
            DropColumn("dbo.EmailTemplateAreas", "Owner");
            DropColumn("dbo.EmailTemplateAreas", "Group");
            DropColumn("dbo.EmailTemplateAreas", "Permission");
            DropColumn("dbo.EmailTemplateAreas", "ModificationToken");
            DropColumn("dbo.EmailTemplateAreas", "ModifiedBy");
        }
        
        public override void Down()
        {
            AddColumn("dbo.EmailTemplateAreas", "ModifiedBy", c => c.Guid(nullable: false));
            AddColumn("dbo.EmailTemplateAreas", "ModificationToken", c => c.Guid(nullable: false));
            AddColumn("dbo.EmailTemplateAreas", "Permission", c => c.String(maxLength: 3));
            AddColumn("dbo.EmailTemplateAreas", "Group", c => c.Guid(nullable: false));
            AddColumn("dbo.EmailTemplateAreas", "Owner", c => c.Guid(nullable: false));
            AddColumn("dbo.EmailTemplateAreas", "DateModified", c => c.DateTimeOffset(nullable: false, precision: 7));
            AddColumn("dbo.EmailTemplateAreas", "DateCreated", c => c.DateTimeOffset(nullable: false, precision: 7));
            AddColumn("dbo.EmailTemplateAreas", "Name", c => c.String(maxLength: 250));
            AddColumn("dbo.EmailAreas", "ModifiedBy", c => c.Guid(nullable: false));
            AddColumn("dbo.EmailAreas", "ModificationToken", c => c.Guid(nullable: false));
            AddColumn("dbo.EmailAreas", "Permission", c => c.String(maxLength: 3));
            AddColumn("dbo.EmailAreas", "Group", c => c.Guid(nullable: false));
            AddColumn("dbo.EmailAreas", "Owner", c => c.Guid(nullable: false));
            AddColumn("dbo.EmailAreas", "DateModified", c => c.DateTimeOffset(nullable: false, precision: 7));
            AddColumn("dbo.EmailAreas", "DateCreated", c => c.DateTimeOffset(nullable: false, precision: 7));
            AddColumn("dbo.EmailAreas", "Name", c => c.String(maxLength: 250));
            AddColumn("dbo.EmailAreas", "Company", c => c.Guid(nullable: false));
        }
    }
}
