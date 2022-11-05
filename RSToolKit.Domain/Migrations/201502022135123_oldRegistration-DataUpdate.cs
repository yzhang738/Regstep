namespace RSToolKit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class oldRegistrationDataUpdate : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.OldRegistrantData", "Value", c => c.String(maxLength: 1000));
            AlterColumn("dbo.OldRegistrantData", "VariableUId", c => c.Guid());
            DropColumn("dbo.OldRegistrantData", "Variable");
            DropColumn("dbo.OldRegistrantData", "Descriminator");
        }
        
        public override void Down()
        {
            AddColumn("dbo.OldRegistrantData", "Descriminator", c => c.String(nullable: false, maxLength: 250));
            AddColumn("dbo.OldRegistrantData", "Variable", c => c.String(nullable: false, maxLength: 250));
            AlterColumn("dbo.OldRegistrantData", "VariableUId", c => c.Guid(nullable: false));
            AlterColumn("dbo.OldRegistrantData", "Value", c => c.String(nullable: false, maxLength: 1000));
        }
    }
}
