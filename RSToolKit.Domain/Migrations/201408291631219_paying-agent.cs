namespace RSToolKit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class payingagent : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Registrants", "PayingAgentNumber", c => c.String());
            AddColumn("dbo.Registrants", "PayingAgentName", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Registrants", "PayingAgentName");
            DropColumn("dbo.Registrants", "PayingAgentNumber");
        }
    }
}
