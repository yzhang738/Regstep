namespace RSToolKit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class def_from_styles : DbMigration
    {
        public override void Up()
        {
            DropIndex("dbo.DefaultFormStyles", new[] { "CompanyKey" });
            AlterColumn("dbo.DefaultFormStyles", "CompanyKey", c => c.Guid());
            CreateIndex("dbo.DefaultFormStyles", "CompanyKey");
        }
        
        public override void Down()
        {
            DropIndex("dbo.DefaultFormStyles", new[] { "CompanyKey" });
            AlterColumn("dbo.DefaultFormStyles", "CompanyKey", c => c.Guid(nullable: false));
            CreateIndex("dbo.DefaultFormStyles", "CompanyKey");
        }
    }
}
