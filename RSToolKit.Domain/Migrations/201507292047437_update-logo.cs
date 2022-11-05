namespace RSToolKit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class updatelogo : DbMigration
    {
        public override void Up()
        {
            RenameColumn(table: "dbo.CompanyLogos", name: "Company_UId", newName: "CompanyKey");
            RenameIndex(table: "dbo.CompanyLogos", name: "IX_Company_UId", newName: "IX_CompanyKey");
            AddColumn("dbo.CompanyLogos", "MIME", c => c.String());
            AddColumn("dbo.CompanyLogos", "Height", c => c.Int(nullable: false));
            DropColumn("dbo.CompanyLogos", "Length");
        }
        
        public override void Down()
        {
            AddColumn("dbo.CompanyLogos", "Length", c => c.Int(nullable: false));
            DropColumn("dbo.CompanyLogos", "Height");
            DropColumn("dbo.CompanyLogos", "MIME");
            RenameIndex(table: "dbo.CompanyLogos", name: "IX_CompanyKey", newName: "IX_Company_UId");
            RenameColumn(table: "dbo.CompanyLogos", name: "CompanyKey", newName: "Company_UId");
        }
    }
}
