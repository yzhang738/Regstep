namespace RSToolKit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class formhideshoppingcart : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Forms", "DisableShoppingCart", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Forms", "DisableShoppingCart");
        }
    }
}
