using Microsoft.AspNet.Identity.EntityFramework;

namespace RSToolKit.Domain.Entities.Clients
{
    public class AppRole : IdentityRole
    {
        public AppRole() : base() { }

        public AppRole(string name) : base(name) { }
    }
}