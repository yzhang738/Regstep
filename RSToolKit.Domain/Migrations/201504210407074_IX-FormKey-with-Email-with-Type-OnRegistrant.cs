namespace RSToolKit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class IXFormKeywithEmailwithTypeOnRegistrant : DbMigration
    {
        public override void Up()
        {
            DropIndex("dbo.Registrants", new[] { "FormKey" });
            CreateIndex("dbo.Registrants", new[] { "FormKey", "Email", "Type" }, unique: true);
            DropColumn("dbo.Registrants", "Confirmation");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Registrants", "Confirmation", c => c.String(nullable: false, maxLength: 50));
            DropIndex("dbo.Registrants", new[] { "FormKey", "Email", "Type" });
            CreateIndex("dbo.Registrants", "FormKey");
        }
    }
}
