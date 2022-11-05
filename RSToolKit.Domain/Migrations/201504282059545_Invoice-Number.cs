namespace RSToolKit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InvoiceNumber : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Registrants", "InvoiceNumber", c => c.String(defaultValue: ""));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Registrants", "InvoiceNumber");
        }
    }
}
