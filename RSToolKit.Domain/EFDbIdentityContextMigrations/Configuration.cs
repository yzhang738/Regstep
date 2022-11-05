namespace RSToolKit.Domain.EFDbIdentityContextMigrations
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;
    using RSToolKit.Domain.Entities.Clients;
    using Microsoft.Owin.Security;
    using Microsoft.AspNet.Identity;
    using Microsoft.AspNet.Identity.EntityFramework;

    internal sealed class Configuration : DbMigrationsConfiguration<RSToolKit.Domain.Entities.Clients.EFDbIdentityContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
            MigrationsDirectory = @"EFDbIdentityContextMigrations";
            ContextKey = "RSToolKit.Domain.Entities.Clients.EFDbIdentityContext";
        }

        protected override void Seed(RSToolKit.Domain.Entities.Clients.EFDbIdentityContext context)
        {
            //  This method will be called after migrating to the latest version.

            //  You can use the DbSet<T>.AddOrUpdate() helper extension method 
            //  to avoid creating duplicate seed data. E.g.
            //
            //    context.People.AddOrUpdate(
            //      p => p.FullName,
            //      new Person { FullName = "Andrew Peters" },
            //      new Person { FullName = "Brice Lambson" },
            //      new Person { FullName = "Rowan Miller" }
            //    );
            //

            var cUId = Guid.Parse("{27e170e9-80bf-4292-82e3-b8fa348352e3}");

            var user1 = new User()
            {
                UserName = "andrew.jackson",
                Birthdate = new DateTime(1987, 1, 31),
                FirstName = "Andrew",
                LastName = "Jackson",
                LastIPA = "127.0.0.1",
                Comment = "Super Admin",
                Email = "andrew.jackson@lamir.net",
                IsConfirmed = true,
                EmailConfirmed = true,
                Company = cUId,
                CurrentCompany = cUId,
                CurrentCompanyName = "RegStep Technologies"
            };
            
            var user2 = new User()
            {
                UserName = "jason.white",
                Birthdate = new DateTime(1987, 1, 31),
                FirstName = "Jason",
                LastName = "White",
                LastIPA = "127.0.0.1",
                Comment = "Super Admin",
                Email = "jason@regstep.com",
                IsConfirmed = true,
                EmailConfirmed = true,
                Company = cUId,
                CurrentCompany = cUId,
                CurrentCompanyName = "RegStep Technologies"
            };


            var UserManager = new AppUserManager(new UserStore<User>(context));
            var RoleManager = new AppRoleManager(new RoleStore<AppRole>(context));

            if (!RoleManager.RoleExists("Super Administrators"))
                RoleManager.Create(new AppRole("Super Administrators"));
            if (!RoleManager.RoleExists("Administrators"))
                RoleManager.Create(new AppRole("Administrators"));
            if (!RoleManager.RoleExists("Cloud Users"))
                RoleManager.Create(new AppRole("Cloud Users"));
            if (!RoleManager.RoleExists("FormBuilder Users"))
                RoleManager.Create(new AppRole("FormBuilder Users"));
            if (!RoleManager.RoleExists("EmailBuilder Users"))
                RoleManager.Create(new AppRole("EmailBuilder Users"));

            var result_userCreate = UserManager.Create(user1, "Elise315#!%");
            if (result_userCreate.Succeeded)
            {
                UserManager.AddToRole(user1.Id, "Super Administrators");
                UserManager.AddToRole(user1.Id, "Administrators");
            }
            result_userCreate = UserManager.Create(user2, "RS1q2w3e4r");
            if (result_userCreate.Succeeded)
            {
                UserManager.AddToRole(user2.Id, "Super Administrators");
                UserManager.AddToRole(user2.Id, "Administrators");
            }
        }
    }
}
