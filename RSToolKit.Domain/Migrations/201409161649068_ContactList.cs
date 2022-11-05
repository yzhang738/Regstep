namespace RSToolKit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ContactList : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Contacts", "EmailList_UId", "dbo.EmailLists");
            DropForeignKey("dbo.RSEmails", "EmailListKey", "dbo.EmailLists");
            DropIndex("dbo.RSEmails", new[] { "EmailListKey" });
            DropIndex("dbo.Contacts", new[] { "EmailList_UId" });
            DropPrimaryKey("dbo.SavedListContacts");
            RenameTable(name: "dbo.SavedListContacts", newName: "ContactSavedLists");
            CreateTable(
                "dbo.ContactReports",
                c => new
                    {
                        UId = c.Guid(nullable: false),
                        SortingId = c.Long(nullable: false, identity: true),
                        CompanyKey = c.Guid(nullable: false),
                        Name = c.String(maxLength: 250),
                        Description = c.String(),
                        Permission = c.String(maxLength: 3),
                        DateCreated = c.DateTimeOffset(nullable: false, precision: 7),
                        DateModified = c.DateTimeOffset(nullable: false, precision: 7),
                        Owner = c.Guid(nullable: false),
                        Group = c.Guid(nullable: false),
                        ModificationToken = c.Guid(nullable: false),
                        ModifiedBy = c.Guid(nullable: false),
                        RawHeaders = c.String(),
                    })
                .PrimaryKey(t => t.UId, clustered: false)
                .Index(t => t.SortingId, clustered: true)
                .ForeignKey("dbo.Companies", t => t.CompanyKey)
                .Index(t => t.CompanyKey);
            
            AddColumn("dbo.RSEmails", "SavedListKey", c => c.Guid());
            AddColumn("dbo.QueryFilters", "ContactReportKey", c => c.Guid());
            AddPrimaryKey("dbo.ContactSavedLists", new[] { "Contact_UId", "SavedList_UId" });
            CreateIndex("dbo.RSEmails", "SavedListKey");
            CreateIndex("dbo.QueryFilters", "ContactReportKey");
            AddForeignKey("dbo.RSEmails", "SavedListKey", "dbo.SavedLists", "UId");
            AddForeignKey("dbo.QueryFilters", "ContactReportKey", "dbo.ContactReports", "UId");
            DropColumn("dbo.RSEmails", "EmailListKey");
            DropColumn("dbo.Contacts", "Email");
            DropColumn("dbo.Contacts", "EmailList_UId");
            DropTable("dbo.EmailLists");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.EmailLists",
                c => new
                    {
                        UId = c.Guid(nullable: false),
                        CompanyUId = c.Guid(nullable: false),
                        SortingId = c.Long(nullable: false, identity: true),
                        Name = c.String(maxLength: 250),
                        DateCreated = c.DateTimeOffset(nullable: false, precision: 7),
                        DateModified = c.DateTimeOffset(nullable: false, precision: 7),
                        Owner = c.Guid(nullable: false),
                        Group = c.Guid(nullable: false),
                        Permission = c.String(maxLength: 3),
                        ModificationToken = c.Guid(nullable: false),
                        ModifiedBy = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.UId);
            
            AddColumn("dbo.Contacts", "EmailList_UId", c => c.Guid());
            AddColumn("dbo.Contacts", "Email", c => c.String());
            AddColumn("dbo.RSEmails", "EmailListKey", c => c.Guid());
            DropForeignKey("dbo.QueryFilters", "ContactReportKey", "dbo.ContactReports");
            DropForeignKey("dbo.ContactReports", "CompanyKey", "dbo.Companies");
            DropForeignKey("dbo.RSEmails", "SavedListKey", "dbo.SavedLists");
            DropIndex("dbo.ContactReports", new[] { "CompanyKey" });
            DropIndex("dbo.QueryFilters", new[] { "ContactReportKey" });
            DropIndex("dbo.RSEmails", new[] { "SavedListKey" });
            DropPrimaryKey("dbo.ContactSavedLists");
            DropColumn("dbo.QueryFilters", "ContactReportKey");
            DropColumn("dbo.RSEmails", "SavedListKey");
            DropTable("dbo.ContactReports");
            AddPrimaryKey("dbo.SavedListContacts", new[] { "SavedList_UId", "Contact_UId" });
            CreateIndex("dbo.Contacts", "EmailList_UId");
            CreateIndex("dbo.RSEmails", "EmailListKey");
            AddForeignKey("dbo.RSEmails", "EmailListKey", "dbo.EmailLists", "UId");
            AddForeignKey("dbo.Contacts", "EmailList_UId", "dbo.EmailLists", "UId");
            RenameTable(name: "dbo.ContactSavedLists", newName: "SavedListContacts");
        }
    }
}
