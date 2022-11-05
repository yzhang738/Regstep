namespace RSToolKit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class seatingregistrant : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Seaters", "RegistrantKey", c => c.Guid());
            CreateIndex("dbo.Seaters", "RegistrantKey");
            AddForeignKey("dbo.Seaters", "RegistrantKey", "dbo.Registrants", "UId");
            DropColumn("dbo.Tags", "CompanyUId");
            DropColumn("dbo.Pointers", "Descriminator");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Pointers", "Descriminator", c => c.String(maxLength: 250));
            AddColumn("dbo.Tags", "CompanyUId", c => c.Guid(nullable: false));
            DropForeignKey("dbo.Seaters", "RegistrantKey", "dbo.Registrants");
            DropIndex("dbo.Seaters", new[] { "RegistrantKey" });
            DropColumn("dbo.Seaters", "RegistrantKey");
        }
    }
}
