namespace RSToolKit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class initialformbuilder_FormFormCloseMessage : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Forms", "FormCloseMessage", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Forms", "FormCloseMessage");
        }
    }
}
