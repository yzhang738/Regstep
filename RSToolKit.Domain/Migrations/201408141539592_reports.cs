namespace RSToolKit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class reports : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.EmailSends", "FormKey", c => c.Guid());
            AddColumn("dbo.EmailSends", "Type", c => c.Int(nullable: false));
            CreateIndex("dbo.EmailSends", "FormKey");
            AddForeignKey("dbo.EmailSends", "FormKey", "dbo.Forms", "UId");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.EmailSends", "FormKey", "dbo.Forms");
            DropIndex("dbo.EmailSends", new[] { "FormKey" });
            DropColumn("dbo.EmailSends", "Type");
            DropColumn("dbo.EmailSends", "FormKey");
        }
    }
}
