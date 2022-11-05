namespace RSToolKit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Contact : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.ContactData", "CompanyKey", "dbo.Companies");
            DropForeignKey("dbo.ContactEmailLists", "Contact_UId", "dbo.Contacts");
            DropForeignKey("dbo.ContactEmailLists", "EmailList_UId", "dbo.EmailLists");
            DropForeignKey("dbo.Contacts", "SavedList_UId", "dbo.SavedLists");
            DropIndex("dbo.Contacts", new[] { "SavedList_UId" });
            DropIndex("dbo.ContactData", new[] { "CompanyKey" });
            DropIndex("dbo.SavedLists", new[] { "Company_UId" });
            DropIndex("dbo.ContactEmailLists", new[] { "Contact_UId" });
            DropIndex("dbo.ContactEmailLists", new[] { "EmailList_UId" });
            DropColumn("dbo.SavedLists", "CompanyKey");
            RenameColumn(table: "dbo.SavedLists", name: "Company_UId", newName: "CompanyKey");
            CreateTable(
                "dbo.ContactHeaders",
                c => new
                    {
                        UId = c.Guid(nullable: false),
                        SortingId = c.Long(nullable: false, identity: true),
                        Name = c.String(),
                        CompanyKey = c.Guid(nullable: false),
                        SavedListKey = c.Guid(),
                        Descriminator = c.String(),
                        RawDescriminatorOptions = c.String(),
                    })
                .PrimaryKey(t => t.UId, clustered: false)
                .ForeignKey("dbo.Companies", t => t.CompanyKey)
                .ForeignKey("dbo.SavedLists", t => t.SavedListKey)
                .Index(t => t.SortingId, clustered: true)
                .Index(t => t.CompanyKey)
                .Index(t => t.SavedListKey);
            
            CreateTable(
                "dbo.SavedListContacts",
                c => new
                    {
                        SavedList_UId = c.Guid(nullable: false),
                        Contact_UId = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => new { t.SavedList_UId, t.Contact_UId })
                .ForeignKey("dbo.SavedLists", t => t.SavedList_UId, cascadeDelete: true)
                .ForeignKey("dbo.Contacts", t => t.Contact_UId, cascadeDelete: true)
                .Index(t => t.SavedList_UId)
                .Index(t => t.Contact_UId);
            
            AddColumn("dbo.Contacts", "EmailList_UId", c => c.Guid());
            AddColumn("dbo.ContactData", "HeaderKey", c => c.Guid(nullable: false));
            AlterColumn("dbo.SavedLists", "CompanyKey", c => c.Guid(nullable: false));
            CreateIndex("dbo.Contacts", "EmailList_UId");
            CreateIndex("dbo.ContactData", "HeaderKey");
            CreateIndex("dbo.SavedLists", "CompanyKey");
            AddForeignKey("dbo.ContactData", "HeaderKey", "dbo.ContactHeaders", "UId");
            AddForeignKey("dbo.Contacts", "EmailList_UId", "dbo.EmailLists", "UId");
            DropColumn("dbo.Contacts", "SavedList_UId");
            DropColumn("dbo.ContactData", "Header");
            DropColumn("dbo.ContactData", "CompanyKey");
            DropColumn("dbo.ContactData", "EmailListUId");
            DropColumn("dbo.ContactData", "Descriminator");
            DropTable("dbo.ContactEmailLists");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.ContactEmailLists",
                c => new
                    {
                        Contact_UId = c.Guid(nullable: false),
                        EmailList_UId = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => new { t.Contact_UId, t.EmailList_UId });
            
            AddColumn("dbo.ContactData", "Descriminator", c => c.String(nullable: false, maxLength: 250));
            AddColumn("dbo.ContactData", "EmailListUId", c => c.Guid());
            AddColumn("dbo.ContactData", "CompanyKey", c => c.Guid(nullable: false));
            AddColumn("dbo.ContactData", "Header", c => c.String());
            AddColumn("dbo.Contacts", "SavedList_UId", c => c.Guid());
            DropForeignKey("dbo.Contacts", "EmailList_UId", "dbo.EmailLists");
            DropForeignKey("dbo.ContactData", "HeaderKey", "dbo.ContactHeaders");
            DropForeignKey("dbo.ContactHeaders", "SavedListKey", "dbo.SavedLists");
            DropForeignKey("dbo.SavedListContacts", "Contact_UId", "dbo.Contacts");
            DropForeignKey("dbo.SavedListContacts", "SavedList_UId", "dbo.SavedLists");
            DropForeignKey("dbo.ContactHeaders", "CompanyKey", "dbo.Companies");
            DropIndex("dbo.SavedListContacts", new[] { "Contact_UId" });
            DropIndex("dbo.SavedListContacts", new[] { "SavedList_UId" });
            DropIndex("dbo.SavedLists", new[] { "CompanyKey" });
            DropIndex("dbo.ContactHeaders", new[] { "SavedListKey" });
            DropIndex("dbo.ContactHeaders", new[] { "CompanyKey" });
            DropIndex("dbo.ContactData", new[] { "HeaderKey" });
            DropIndex("dbo.Contacts", new[] { "EmailList_UId" });
            AlterColumn("dbo.SavedLists", "CompanyKey", c => c.Guid());
            DropColumn("dbo.ContactData", "HeaderKey");
            DropColumn("dbo.Contacts", "EmailList_UId");
            DropTable("dbo.SavedListContacts");
            DropTable("dbo.ContactHeaders");
            RenameColumn(table: "dbo.SavedLists", name: "CompanyKey", newName: "Company_UId");
            AddColumn("dbo.SavedLists", "CompanyKey", c => c.Guid(nullable: false));
            CreateIndex("dbo.ContactEmailLists", "EmailList_UId");
            CreateIndex("dbo.ContactEmailLists", "Contact_UId");
            CreateIndex("dbo.SavedLists", "Company_UId");
            CreateIndex("dbo.ContactData", "CompanyKey");
            CreateIndex("dbo.Contacts", "SavedList_UId");
            AddForeignKey("dbo.Contacts", "SavedList_UId", "dbo.SavedLists", "UId");
            AddForeignKey("dbo.ContactEmailLists", "EmailList_UId", "dbo.EmailLists", "UId", cascadeDelete: true);
            AddForeignKey("dbo.ContactEmailLists", "Contact_UId", "dbo.Contacts", "UId", cascadeDelete: true);
            AddForeignKey("dbo.ContactData", "CompanyKey", "dbo.Companies", "UId");
        }
    }
}
