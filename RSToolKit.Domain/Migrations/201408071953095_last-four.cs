namespace RSToolKit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class lastfour : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.TransactionRequests", "LastFour", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.TransactionRequests", "LastFour");
        }
    }
}
