namespace RSToolKit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class MerchantAccountInfoParameters : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.MerchantAccountInfo", "RawParameters", c => c.String(defaultValue: "[]"));
        }
        
        public override void Down()
        {
            DropColumn("dbo.MerchantAccountInfo", "RawParameters");
        }
    }
}
