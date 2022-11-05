namespace RSToolKit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class seatingholdinterval : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Seatings", "HoldInterval", c => c.Time(precision: 7));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Seatings", "HoldInterval");
        }
    }
}
