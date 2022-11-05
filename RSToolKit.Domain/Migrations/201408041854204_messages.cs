namespace RSToolKit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class messages : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Forms", "UnderMaintenance", c => c.String());
            AddColumn("dbo.Forms", "FormClosed", c => c.String());
            AddColumn("dbo.Forms", "TaxDescription", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Forms", "TaxDescription");
            DropColumn("dbo.Forms", "FormClosed");
            DropColumn("dbo.Forms", "UnderMaintenance");
        }
    }
}
