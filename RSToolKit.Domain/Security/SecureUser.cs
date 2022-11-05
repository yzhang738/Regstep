using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Principal;
using RSToolKit.Domain.Entities.Clients;
using RSToolKit.Domain.Identity;
using RSToolKit.Domain.Data;
using Microsoft.AspNet.Identity;


namespace RSToolKit.Domain.Security
{
    /// <summary>
    /// A class that holds information about the user requesting records.
    /// </summary>
    public class SecureUser
    {
        /// <summary>
        /// The Principal in use.
        /// </summary>
        public IPrincipal Principal { get; protected set; }
        /// <summary>
        /// The User is use for permission checking.
        /// </summary>
        public User AppUser { get; protected set; }
        /// <summary>
        /// Whether or not the User has global permissions.
        /// </summary>
        public bool GlobalPermissions { get; protected set; }
        /// <summary>
        /// Whether or not the User is a company administrator.
        /// </summary>
        public bool CompanyAdministrator { get; protected set; }

        /// <summary>
        /// Initializes the class with the principal and the context.
        /// </summary>
        /// <param name="principal">The principal of the authenticated user.</param>
        /// <param name="context">The context being used.</param>
        public SecureUser(IPrincipal principal, EFDbContext context)
        {
            AppUser = null;
            Principal = null;
            GlobalPermissions = false;
            CompanyAdministrator = false;
            SetUser(principal, context);
        }


        /// <summary>
        /// Initializes the class with a user and it's associated principal.
        /// </summary>
        /// <param name="user">The user to use for permission checking.</param>
        /// <param name="principal">The associated principal with that user.</param>
        public SecureUser(User user, IPrincipal principal)
        {
            AppUser = null;
            Principal = null;
            GlobalPermissions = false;
            CompanyAdministrator = false;
            SetUser(user, principal);
        }

        /// <summary>
        /// Sets the user and principal to use in permissions checks.
        /// </summary>
        /// <param name="user">The user to be set.</param>
        /// <param name="principal">The principal associated with the logged in user.</param>
        /// <exception cref="ArgumentNullException">Thrown if either the user or principal parameters are null.</exception>
        public void SetUser(User user, IPrincipal principal)
        {
            if (user == null)
                throw new ArgumentNullException("user", "The user supplied cannot be null.");
            if (principal == null)
                throw new ArgumentNullException("principal", "The principal supplied cannot be null.");
            AppUser = user;
            Principal = principal;
            GlobalPermissions = Principal.HasGlobalPermissions();
            CompanyAdministrator = Principal.IsInRole("Company Administrators");
        }

        public void SetUser(IPrincipal principal, EFDbContext context)
        {
            if (principal == null)
                throw new ArgumentNullException("principal", "The principal supplied cannot be null.");
            if (principal == null)
                throw new ArgumentNullException("context", "The context supplied cannot be null.");
            GlobalPermissions = Principal.HasGlobalPermissions();
            CompanyAdministrator = Principal.IsInRole("Company Administrators");
            using (var um = new AppUserManager(new Microsoft.AspNet.Identity.EntityFramework.UserStore<User>(context)))
            {
                AppUser = um.FindByName(principal.Identity.Name);
                context.Entry(AppUser).Collection(u => u.CustomGroups).Load();
            }
        }

    }
}
