namespace RSToolKit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class JLogic : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ComponentBases", "RawJLogics", c => c.String());
            AddColumn("dbo.RSEmails", "RawJLogics", c => c.String());
            AddColumn("dbo.RSHtmlEmails", "RawJLogics", c => c.String());
            AddColumn("dbo.LogicBlocks", "RawJLogics", c => c.String());
            AddColumn("dbo.Pages", "RawJLogics", c => c.String());
            AddColumn("dbo.Panels", "RawJLogics", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Panels", "RawJLogics");
            DropColumn("dbo.Pages", "RawJLogics");
            DropColumn("dbo.LogicBlocks", "RawJLogics");
            DropColumn("dbo.RSHtmlEmails", "RawJLogics");
            DropColumn("dbo.RSEmails", "RawJLogics");
            DropColumn("dbo.ComponentBases", "RawJLogics");
        }
    }
}
