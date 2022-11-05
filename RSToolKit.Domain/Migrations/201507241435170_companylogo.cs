namespace RSToolKit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class companylogo : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.CompanyLogos",
                c => new
                    {
                        SortingId = c.Long(nullable: false, identity: true),
                        UId = c.Guid(nullable: false),
                        DateCreated = c.DateTimeOffset(nullable: false, precision: 7),
                        DateModified = c.DateTimeOffset(nullable: false, precision: 7),
                        ModificationToken = c.Guid(nullable: false),
                        ModifiedBy = c.Guid(nullable: false),
                        BinaryData = c.Binary(),
                        SizeInBytes = c.Long(nullable: false),
                        Width = c.Int(nullable: false),
                        Length = c.Int(nullable: false),
                        Company_UId = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.SortingId)
                .ForeignKey("dbo.Companies", t => t.Company_UId)
                .Index(t => t.UId, unique: true)
                .Index(t => t.Company_UId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.CompanyLogos", "Company_UId", "dbo.Companies");
            DropIndex("dbo.CompanyLogos", new[] { "Company_UId" });
            DropIndex("dbo.CompanyLogos", new[] { "UId" });
            DropTable("dbo.CompanyLogos");
        }
    }
}
