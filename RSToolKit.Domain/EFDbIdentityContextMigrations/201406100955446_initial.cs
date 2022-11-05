namespace RSToolKit.Domain.EFDbIdentityContextMigrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class initial : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.CustomGroups",
                c => new
                    {
                        UId = c.Guid(nullable: false),
                        SortingId = c.Long(nullable: false, identity: true),
                        Company = c.Guid(nullable: false),
                        Name = c.String(),
                        Owner = c.Guid(nullable: false),
                        Group = c.Guid(nullable: false),
                        Permission = c.String(maxLength: 3),
                    })
                .PrimaryKey(t => t.UId);
            
            CreateTable(
                "dbo.AspNetUsers",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        DateCreated = c.DateTimeOffset(nullable: false, precision: 7),
                        DateModified = c.DateTimeOffset(nullable: false, precision: 7),
                        Owner = c.Guid(nullable: false),
                        Group = c.Guid(nullable: false),
                        Permission = c.String(maxLength: 3),
                        ModificationToken = c.Guid(nullable: false),
                        ModifiedBy = c.Guid(nullable: false),
                        Company = c.Guid(nullable: false),
                        ValidationToken = c.Guid(nullable: false),
                        PasswordResetToken = c.Guid(nullable: false),
                        PasswordResetTokenExpiration = c.DateTimeOffset(nullable: false, precision: 7),
                        LastPasswordFailureDate = c.DateTimeOffset(nullable: false, precision: 7),
                        PasswordChangeDate = c.DateTimeOffset(nullable: false, precision: 7),
                        Crumbs = c.String(),
                        Birthdate = c.DateTime(nullable: false),
                        IsLocked = c.Boolean(nullable: false),
                        LockedDate = c.DateTimeOffset(nullable: false, precision: 7),
                        LockReason = c.String(),
                        UTCOffset = c.Single(nullable: false),
                        Comment = c.String(),
                        PasswordQuestion = c.String(),
                        PasswordAnswer = c.String(),
                        FirstName = c.String(),
                        LastName = c.String(),
                        PasswordFailuresSinceLastSuccess = c.Int(nullable: false),
                        IsConfirmed = c.Boolean(nullable: false),
                        LastIPA = c.String(),
                        CurrentCompany = c.Guid(nullable: false),
                        CurrentCompanyName = c.String(),
                        Email = c.String(maxLength: 256),
                        EmailConfirmed = c.Boolean(nullable: false),
                        PasswordHash = c.String(),
                        SecurityStamp = c.String(),
                        PhoneNumber = c.String(),
                        PhoneNumberConfirmed = c.Boolean(nullable: false),
                        TwoFactorEnabled = c.Boolean(nullable: false),
                        LockoutEndDateUtc = c.DateTime(),
                        LockoutEnabled = c.Boolean(nullable: false),
                        AccessFailedCount = c.Int(nullable: false),
                        UserName = c.String(nullable: false, maxLength: 256),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.UserName, unique: true, name: "UserNameIndex");
            
            CreateTable(
                "dbo.AspNetUserClaims",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.String(nullable: false, maxLength: 128),
                        ClaimType = c.String(),
                        ClaimValue = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.AspNetUserLogins",
                c => new
                    {
                        LoginProvider = c.String(nullable: false, maxLength: 128),
                        ProviderKey = c.String(nullable: false, maxLength: 128),
                        UserId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.LoginProvider, t.ProviderKey, t.UserId })
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.AspNetUserRoles",
                c => new
                    {
                        UserId = c.String(nullable: false, maxLength: 128),
                        RoleId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.UserId, t.RoleId })
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .ForeignKey("dbo.AspNetRoles", t => t.RoleId, cascadeDelete: true)
                .Index(t => t.UserId)
                .Index(t => t.RoleId);
            
            CreateTable(
                "dbo.DatabaseAccess",
                c => new
                    {
                        SortingId = c.Long(nullable: false, identity: true),
                        Company = c.Guid(nullable: false),
                        User = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.SortingId);
            
            CreateTable(
                "dbo.AspNetRoles",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Name = c.String(nullable: false, maxLength: 256),
                        Discriminator = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.Name, unique: true, name: "RoleNameIndex");
            
            CreateTable(
                "dbo.UserCustomGroups",
                c => new
                    {
                        User_Id = c.String(nullable: false, maxLength: 128),
                        CustomGroup_UId = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => new { t.User_Id, t.CustomGroup_UId })
                .ForeignKey("dbo.AspNetUsers", t => t.User_Id, cascadeDelete: true)
                .ForeignKey("dbo.CustomGroups", t => t.CustomGroup_UId, cascadeDelete: true)
                .Index(t => t.User_Id)
                .Index(t => t.CustomGroup_UId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.AspNetUserRoles", "RoleId", "dbo.AspNetRoles");
            DropForeignKey("dbo.AspNetUserRoles", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserLogins", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.UserCustomGroups", "CustomGroup_UId", "dbo.CustomGroups");
            DropForeignKey("dbo.UserCustomGroups", "User_Id", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserClaims", "UserId", "dbo.AspNetUsers");
            DropIndex("dbo.UserCustomGroups", new[] { "CustomGroup_UId" });
            DropIndex("dbo.UserCustomGroups", new[] { "User_Id" });
            DropIndex("dbo.AspNetRoles", "RoleNameIndex");
            DropIndex("dbo.AspNetUserRoles", new[] { "RoleId" });
            DropIndex("dbo.AspNetUserRoles", new[] { "UserId" });
            DropIndex("dbo.AspNetUserLogins", new[] { "UserId" });
            DropIndex("dbo.AspNetUserClaims", new[] { "UserId" });
            DropIndex("dbo.AspNetUsers", "UserNameIndex");
            DropTable("dbo.UserCustomGroups");
            DropTable("dbo.AspNetRoles");
            DropTable("dbo.DatabaseAccess");
            DropTable("dbo.AspNetUserRoles");
            DropTable("dbo.AspNetUserLogins");
            DropTable("dbo.AspNetUserClaims");
            DropTable("dbo.AspNetUsers");
            DropTable("dbo.CustomGroups");
        }
    }
}
