using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Data.Entity;
using Microsoft.AspNet.Identity;
using RSToolKit.Domain.Data;
using RSToolKit.Domain.Security;

namespace RSToolKit.Domain.Entities.Clients
{
    public class EFDbIdentityContext : IdentityDbContext<User>
    {

        public DbSet<DatabaseAccess> DatabaseAccess { get; set; }
        public DbSet<CustomGroup> CustomGroups { get; set; }

        public EFDbIdentityContext() : base ("EFDbIdentityContext")
        { }

        public EFDbIdentityContext(string connection) : base (connection)
        { }

        static EFDbIdentityContext()
        {
        }

        public static EFDbIdentityContext Create()
        {
            return new EFDbIdentityContext();
        }

        public static EFDbIdentityContext Create(string connection)
        {
            return new EFDbIdentityContext(connection);
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                .HasMany(u => u.CustomGroups)
                .WithMany(g => g.Users);
            base.OnModelCreating(modelBuilder);
        }
    }

    public class EFDbIdentityInitializer : DropCreateDatabaseIfModelChanges<EFDbIdentityContext>
    {
        protected override void Seed(EFDbIdentityContext context)
        {
            PerformInitialSetup(context);
            base.Seed(context);
        }

        public void PerformInitialSetup(EFDbIdentityContext context)
        {
        }
    }
}
