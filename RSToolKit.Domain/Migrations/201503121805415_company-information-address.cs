namespace RSToolKit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class companyinformationaddress : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Companies", "BillingAddressLine1", c => c.String());
            AddColumn("dbo.Companies", "BillingAddressLine2", c => c.String());
            AddColumn("dbo.Companies", "BillingCity", c => c.String());
            AddColumn("dbo.Companies", "BillingState", c => c.String());
            AddColumn("dbo.Companies", "BillingCountry", c => c.String());
            AddColumn("dbo.Companies", "BillingZip", c => c.String());
            AddColumn("dbo.Companies", "ShippingAddressLine1", c => c.String());
            AddColumn("dbo.Companies", "ShippingAddressLine2", c => c.String());
            AddColumn("dbo.Companies", "ShippingCity", c => c.String());
            AddColumn("dbo.Companies", "ShippingState", c => c.String());
            AddColumn("dbo.Companies", "ShippingCountry", c => c.String());
            AddColumn("dbo.Companies", "ShippingZip", c => c.String());
            AddColumn("dbo.Companies", "RegistrationAddressLine1", c => c.String());
            AddColumn("dbo.Companies", "RegistrationAddressLine2", c => c.String());
            AddColumn("dbo.Companies", "RegistrationCity", c => c.String());
            AddColumn("dbo.Companies", "RegistrationState", c => c.String());
            AddColumn("dbo.Companies", "RegistrationCountry", c => c.String());
            AddColumn("dbo.Companies", "RegistrationZip", c => c.String());
            AddColumn("dbo.Companies", "RegistrationPhone", c => c.String());
            AddColumn("dbo.Companies", "RegistrationFax", c => c.String());
            AddColumn("dbo.Companies", "RegistrationEmail", c => c.String());
            DropColumn("dbo.Companies", "Database");
            DropColumn("dbo.Companies", "Key");
            DropColumn("dbo.Companies", "Token");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Companies", "Token", c => c.Guid());
            AddColumn("dbo.Companies", "Key", c => c.String());
            AddColumn("dbo.Companies", "Database", c => c.String(maxLength: 50));
            DropColumn("dbo.Companies", "RegistrationEmail");
            DropColumn("dbo.Companies", "RegistrationFax");
            DropColumn("dbo.Companies", "RegistrationPhone");
            DropColumn("dbo.Companies", "RegistrationZip");
            DropColumn("dbo.Companies", "RegistrationCountry");
            DropColumn("dbo.Companies", "RegistrationState");
            DropColumn("dbo.Companies", "RegistrationCity");
            DropColumn("dbo.Companies", "RegistrationAddressLine2");
            DropColumn("dbo.Companies", "RegistrationAddressLine1");
            DropColumn("dbo.Companies", "ShippingZip");
            DropColumn("dbo.Companies", "ShippingCountry");
            DropColumn("dbo.Companies", "ShippingState");
            DropColumn("dbo.Companies", "ShippingCity");
            DropColumn("dbo.Companies", "ShippingAddressLine2");
            DropColumn("dbo.Companies", "ShippingAddressLine1");
            DropColumn("dbo.Companies", "BillingZip");
            DropColumn("dbo.Companies", "BillingCountry");
            DropColumn("dbo.Companies", "BillingState");
            DropColumn("dbo.Companies", "BillingCity");
            DropColumn("dbo.Companies", "BillingAddressLine2");
            DropColumn("dbo.Companies", "BillingAddressLine1");
        }
    }
}
