namespace RSToolKit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class maxLengthDataValue : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.RegistrantData", "Value", c => c.String());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.RegistrantData", "Value", c => c.String(maxLength: 1000));
        }
    }
}
