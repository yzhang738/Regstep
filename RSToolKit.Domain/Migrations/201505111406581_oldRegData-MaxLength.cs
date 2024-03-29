namespace RSToolKit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class oldRegDataMaxLength : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.OldRegistrantData", "Value", c => c.String());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.OldRegistrantData", "Value", c => c.String(maxLength: 1000));
        }
    }
}
