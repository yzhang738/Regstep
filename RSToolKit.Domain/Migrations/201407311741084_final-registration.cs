namespace RSToolKit.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class finalregistration : DbMigration
    {
        public override void Up()
        {
            DropIndex("dbo.OldRegistrants", new[] { "CurrentRegistrantKey" });
            DropIndex("dbo.TransactionRequests", new[] { "CreditCardUId" });
            DropIndex("dbo.TransactionDetails", new[] { "RequestUId" });
            DropIndex("dbo.TransactionDetails", new[] { "CreditCardUId" });
            RenameColumn(table: "dbo.TransactionRequests", name: "RegistrantUId", newName: "RegistrantKey");
            RenameColumn(table: "dbo.TransactionRequests", name: "CreditCardUId", newName: "CreditCardKey");
            RenameColumn(table: "dbo.TransactionDetails", name: "RequestUId", newName: "TransactionRequestKey");
            RenameColumn(table: "dbo.TransactionRequests", name: "MerchantAccount_UId", newName: "MerchantAccountKey");
            RenameColumn(table: "dbo.CreditCardAccess", name: "CreditCardUId", newName: "CreditCardKey");
            RenameColumn(table: "dbo.TransactionDetails", name: "CreditCardUId", newName: "CreditCardKey");
            RenameIndex(table: "dbo.TransactionRequests", name: "IX_RegistrantUId", newName: "IX_RegistrantKey");
            RenameIndex(table: "dbo.TransactionRequests", name: "IX_MerchantAccount_UId", newName: "IX_MerchantAccountKey");
            RenameIndex(table: "dbo.CreditCardAccess", name: "IX_CreditCardUId", newName: "IX_CreditCardKey");
            AddColumn("dbo.ComponentBases", "ValueType", c => c.Int());
            AddColumn("dbo.Forms", "Badge", c => c.String());
            AddColumn("dbo.OldRegistrants", "FormKey", c => c.Guid(nullable: false));
            AddColumn("dbo.TransactionRequests", "FormKey", c => c.Guid());
            AddColumn("dbo.TransactionRequests", "CompanyKey", c => c.Guid(nullable: false));
            AddColumn("dbo.TransactionRequests", "Permission", c => c.String());
            AddColumn("dbo.TransactionRequests", "Date", c => c.DateTimeOffset(nullable: false, precision: 7));
            AddColumn("dbo.TransactionRequests", "Form_UId", c => c.Guid());
            AddColumn("dbo.CreditCards", "RegistrantKey", c => c.Guid());
            AddColumn("dbo.CreditCards", "FormKey", c => c.Guid(nullable: false));
            AddColumn("dbo.CreditCards", "CompanyKey", c => c.Guid(nullable: false));
            AddColumn("dbo.CreditCards", "Delete", c => c.Boolean(nullable: false));
            AddColumn("dbo.TransactionDetails", "FormKey", c => c.Guid());
            AddColumn("dbo.TransactionDetails", "CompanyKey", c => c.Guid());
            AlterColumn("dbo.ComponentBases", "MinDate", c => c.DateTime());
            AlterColumn("dbo.ComponentBases", "MaxDate", c => c.DateTime());
            AlterColumn("dbo.OldRegistrants", "CurrentRegistrantKey", c => c.Guid());
            AlterColumn("dbo.TransactionRequests", "CreditCardKey", c => c.Guid());
            AlterColumn("dbo.TransactionDetails", "TransactionRequestKey", c => c.Guid(nullable: false));
            AlterColumn("dbo.TransactionDetails", "CreditCardKey", c => c.Guid());
            CreateIndex("dbo.OldRegistrants", "FormKey");
            CreateIndex("dbo.OldRegistrants", "CurrentRegistrantKey");
            CreateIndex("dbo.TransactionRequests", "CreditCardKey");
            CreateIndex("dbo.TransactionRequests", "Form_UId");
            CreateIndex("dbo.TransactionDetails", "CreditCardKey");
            CreateIndex("dbo.TransactionDetails", "TransactionRequestKey");
            AddForeignKey("dbo.OldRegistrants", "FormKey", "dbo.Forms", "UId");
            AddForeignKey("dbo.TransactionRequests", "Form_UId", "dbo.Forms", "UId");
            DropColumn("dbo.TransactionRequests", "FormUId");
            DropColumn("dbo.TransactionRequests", "CompanyUId");
            DropColumn("dbo.CreditCards", "FormUId");
            DropColumn("dbo.CreditCards", "CompanyUId");
            DropColumn("dbo.TransactionDetails", "FormUId");
            DropColumn("dbo.TransactionDetails", "CompanyUId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.TransactionDetails", "CompanyUId", c => c.Guid());
            AddColumn("dbo.TransactionDetails", "FormUId", c => c.Guid());
            AddColumn("dbo.CreditCards", "CompanyUId", c => c.Guid(nullable: false));
            AddColumn("dbo.CreditCards", "FormUId", c => c.Guid(nullable: false));
            AddColumn("dbo.TransactionRequests", "CompanyUId", c => c.Guid(nullable: false));
            AddColumn("dbo.TransactionRequests", "FormUId", c => c.Guid(nullable: false));
            DropForeignKey("dbo.TransactionRequests", "Form_UId", "dbo.Forms");
            DropForeignKey("dbo.OldRegistrants", "FormKey", "dbo.Forms");
            DropIndex("dbo.TransactionDetails", new[] { "TransactionRequestKey" });
            DropIndex("dbo.TransactionDetails", new[] { "CreditCardKey" });
            DropIndex("dbo.TransactionRequests", new[] { "Form_UId" });
            DropIndex("dbo.TransactionRequests", new[] { "CreditCardKey" });
            DropIndex("dbo.OldRegistrants", new[] { "CurrentRegistrantKey" });
            DropIndex("dbo.OldRegistrants", new[] { "FormKey" });
            AlterColumn("dbo.TransactionDetails", "CreditCardKey", c => c.Guid(nullable: false));
            AlterColumn("dbo.TransactionDetails", "TransactionRequestKey", c => c.Guid());
            AlterColumn("dbo.TransactionRequests", "CreditCardKey", c => c.Guid(nullable: false));
            AlterColumn("dbo.OldRegistrants", "CurrentRegistrantKey", c => c.Guid(nullable: false));
            AlterColumn("dbo.ComponentBases", "MaxDate", c => c.DateTimeOffset(precision: 7));
            AlterColumn("dbo.ComponentBases", "MinDate", c => c.DateTimeOffset(precision: 7));
            DropColumn("dbo.TransactionDetails", "CompanyKey");
            DropColumn("dbo.TransactionDetails", "FormKey");
            DropColumn("dbo.CreditCards", "Delete");
            DropColumn("dbo.CreditCards", "CompanyKey");
            DropColumn("dbo.CreditCards", "FormKey");
            DropColumn("dbo.CreditCards", "RegistrantKey");
            DropColumn("dbo.TransactionRequests", "Form_UId");
            DropColumn("dbo.TransactionRequests", "Date");
            DropColumn("dbo.TransactionRequests", "Permission");
            DropColumn("dbo.TransactionRequests", "CompanyKey");
            DropColumn("dbo.TransactionRequests", "FormKey");
            DropColumn("dbo.OldRegistrants", "FormKey");
            DropColumn("dbo.Forms", "Badge");
            DropColumn("dbo.ComponentBases", "ValueType");
            RenameIndex(table: "dbo.CreditCardAccess", name: "IX_CreditCardKey", newName: "IX_CreditCardUId");
            RenameIndex(table: "dbo.TransactionRequests", name: "IX_MerchantAccountKey", newName: "IX_MerchantAccount_UId");
            RenameIndex(table: "dbo.TransactionRequests", name: "IX_RegistrantKey", newName: "IX_RegistrantUId");
            RenameColumn(table: "dbo.TransactionDetails", name: "CreditCardKey", newName: "CreditCardUId");
            RenameColumn(table: "dbo.CreditCardAccess", name: "CreditCardKey", newName: "CreditCardUId");
            RenameColumn(table: "dbo.TransactionRequests", name: "MerchantAccountKey", newName: "MerchantAccount_UId");
            RenameColumn(table: "dbo.TransactionDetails", name: "TransactionRequestKey", newName: "RequestUId");
            RenameColumn(table: "dbo.TransactionRequests", name: "CreditCardKey", newName: "CreditCardUId");
            RenameColumn(table: "dbo.TransactionRequests", name: "RegistrantKey", newName: "RegistrantUId");
            CreateIndex("dbo.TransactionDetails", "CreditCardUId");
            CreateIndex("dbo.TransactionDetails", "RequestUId");
            CreateIndex("dbo.TransactionRequests", "CreditCardUId");
            CreateIndex("dbo.OldRegistrants", "CurrentRegistrantKey");
        }
    }
}
