namespace RSToolKit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class cascadeon : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.ComponentBaseAudiences", "ComponentBase_UId", "dbo.ComponentBases");
            DropForeignKey("dbo.ComponentBaseAudiences", "Audience_UId", "dbo.Audiences");
            DropForeignKey("dbo.PageAudiences", "Page_UId", "dbo.Pages");
            DropForeignKey("dbo.PageAudiences", "Audience_UId", "dbo.Audiences");
            DropForeignKey("dbo.PanelAudiences", "Panel_UId", "dbo.Panels");
            DropForeignKey("dbo.PanelAudiences", "Audience_UId", "dbo.Audiences");
            DropForeignKey("dbo.PricesAudiences", "Prices_UId", "dbo.Prices");
            DropForeignKey("dbo.PricesAudiences", "Audience_UId", "dbo.Audiences");
            DropForeignKey("dbo.TagForms", "Tag_UId", "dbo.Tags");
            DropForeignKey("dbo.TagForms", "Form_UId", "dbo.Forms");
            DropForeignKey("dbo.TagEmailCampaigns", "Tag_UId", "dbo.Tags");
            DropForeignKey("dbo.TagEmailCampaigns", "EmailCampaign_UId", "dbo.EmailCampaigns");
            DropForeignKey("dbo.ContactEmailLists", "Contact_UId", "dbo.Contacts");
            DropForeignKey("dbo.ContactEmailLists", "EmailList_UId", "dbo.EmailLists");
            DropForeignKey("dbo.CustomGroupUsers", "CustomGroup_UId", "dbo.CustomGroups");
            DropForeignKey("dbo.CustomGroupUsers", "User_Id", "dbo.AspNetUsers");
            AddForeignKey("dbo.ComponentBaseAudiences", "ComponentBase_UId", "dbo.ComponentBases", "UId", cascadeDelete: true);
            AddForeignKey("dbo.ComponentBaseAudiences", "Audience_UId", "dbo.Audiences", "UId", cascadeDelete: true);
            AddForeignKey("dbo.PageAudiences", "Page_UId", "dbo.Pages", "UId", cascadeDelete: true);
            AddForeignKey("dbo.PageAudiences", "Audience_UId", "dbo.Audiences", "UId", cascadeDelete: true);
            AddForeignKey("dbo.PanelAudiences", "Panel_UId", "dbo.Panels", "UId", cascadeDelete: true);
            AddForeignKey("dbo.PanelAudiences", "Audience_UId", "dbo.Audiences", "UId", cascadeDelete: true);
            AddForeignKey("dbo.PricesAudiences", "Prices_UId", "dbo.Prices", "UId", cascadeDelete: true);
            AddForeignKey("dbo.PricesAudiences", "Audience_UId", "dbo.Audiences", "UId", cascadeDelete: true);
            AddForeignKey("dbo.TagForms", "Tag_UId", "dbo.Tags", "UId", cascadeDelete: true);
            AddForeignKey("dbo.TagForms", "Form_UId", "dbo.Forms", "UId", cascadeDelete: true);
            AddForeignKey("dbo.TagEmailCampaigns", "Tag_UId", "dbo.Tags", "UId", cascadeDelete: true);
            AddForeignKey("dbo.TagEmailCampaigns", "EmailCampaign_UId", "dbo.EmailCampaigns", "UId", cascadeDelete: true);
            AddForeignKey("dbo.ContactEmailLists", "Contact_UId", "dbo.Contacts", "UId", cascadeDelete: true);
            AddForeignKey("dbo.ContactEmailLists", "EmailList_UId", "dbo.EmailLists", "UId", cascadeDelete: true);
            AddForeignKey("dbo.CustomGroupUsers", "CustomGroup_UId", "dbo.CustomGroups", "UId", cascadeDelete: true);
            AddForeignKey("dbo.CustomGroupUsers", "User_Id", "dbo.AspNetUsers", "Id", cascadeDelete: true);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.CustomGroupUsers", "User_Id", "dbo.AspNetUsers");
            DropForeignKey("dbo.CustomGroupUsers", "CustomGroup_UId", "dbo.CustomGroups");
            DropForeignKey("dbo.ContactEmailLists", "EmailList_UId", "dbo.EmailLists");
            DropForeignKey("dbo.ContactEmailLists", "Contact_UId", "dbo.Contacts");
            DropForeignKey("dbo.TagEmailCampaigns", "EmailCampaign_UId", "dbo.EmailCampaigns");
            DropForeignKey("dbo.TagEmailCampaigns", "Tag_UId", "dbo.Tags");
            DropForeignKey("dbo.TagForms", "Form_UId", "dbo.Forms");
            DropForeignKey("dbo.TagForms", "Tag_UId", "dbo.Tags");
            DropForeignKey("dbo.PricesAudiences", "Audience_UId", "dbo.Audiences");
            DropForeignKey("dbo.PricesAudiences", "Prices_UId", "dbo.Prices");
            DropForeignKey("dbo.PanelAudiences", "Audience_UId", "dbo.Audiences");
            DropForeignKey("dbo.PanelAudiences", "Panel_UId", "dbo.Panels");
            DropForeignKey("dbo.PageAudiences", "Audience_UId", "dbo.Audiences");
            DropForeignKey("dbo.PageAudiences", "Page_UId", "dbo.Pages");
            DropForeignKey("dbo.ComponentBaseAudiences", "Audience_UId", "dbo.Audiences");
            DropForeignKey("dbo.ComponentBaseAudiences", "ComponentBase_UId", "dbo.ComponentBases");
            AddForeignKey("dbo.CustomGroupUsers", "User_Id", "dbo.AspNetUsers", "Id");
            AddForeignKey("dbo.CustomGroupUsers", "CustomGroup_UId", "dbo.CustomGroups", "UId");
            AddForeignKey("dbo.ContactEmailLists", "EmailList_UId", "dbo.EmailLists", "UId");
            AddForeignKey("dbo.ContactEmailLists", "Contact_UId", "dbo.Contacts", "UId");
            AddForeignKey("dbo.TagEmailCampaigns", "EmailCampaign_UId", "dbo.EmailCampaigns", "UId");
            AddForeignKey("dbo.TagEmailCampaigns", "Tag_UId", "dbo.Tags", "UId");
            AddForeignKey("dbo.TagForms", "Form_UId", "dbo.Forms", "UId");
            AddForeignKey("dbo.TagForms", "Tag_UId", "dbo.Tags", "UId");
            AddForeignKey("dbo.PricesAudiences", "Audience_UId", "dbo.Audiences", "UId");
            AddForeignKey("dbo.PricesAudiences", "Prices_UId", "dbo.Prices", "UId");
            AddForeignKey("dbo.PanelAudiences", "Audience_UId", "dbo.Audiences", "UId");
            AddForeignKey("dbo.PanelAudiences", "Panel_UId", "dbo.Panels", "UId");
            AddForeignKey("dbo.PageAudiences", "Audience_UId", "dbo.Audiences", "UId");
            AddForeignKey("dbo.PageAudiences", "Page_UId", "dbo.Pages", "UId");
            AddForeignKey("dbo.ComponentBaseAudiences", "Audience_UId", "dbo.Audiences", "UId");
            AddForeignKey("dbo.ComponentBaseAudiences", "ComponentBase_UId", "dbo.ComponentBases", "UId");
        }
    }
}
