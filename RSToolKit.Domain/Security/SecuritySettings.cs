using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Principal;
using RSToolKit.Domain.Entities.Clients;
using RSToolKit.Domain.Identity;
using RSToolKit.Domain.Data;

namespace RSToolKit.Domain.Security
{
    /// <summary>
    /// A class that holds the information to determine if permissions are
    /// </summary>
    public class SecuritySettings
    {
        protected bool _useSecurity;

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
        /// Whether or not security should be used.
        /// </summary>
        public bool UseSecurity
        {
            get
            {
                if (_useSecurity)
                    return AppUser != null && Principal != null;
                return _useSecurity;
            }
            set
            {
                _useSecurity = value;
            }
        }

        /// <summary>
        /// Initializes the class with default parameters.
        /// </summary>
        public SecuritySettings()
        {
            _useSecurity = false;
            AppUser = null;
            Principal = null;
            GlobalPermissions = false;
            CompanyAdministrator = false;
        }

        /// <summary>
        /// Initializes the class with a user and it's associated principal.
        /// </summary>
        /// <param name="user">The user to use for permission checking.</param>
        /// <param name="principal">The associated principal with that user.</param>
        public SecuritySettings(User user, IPrincipal principal)
            : this()
        {
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
            _useSecurity = true;
        }

        /// <summary>
        /// Creates the default permissions for a database object that requires permissions.
        /// </summary>
        /// <typeparam name="T">The type of item being used. Must inherit either <code>IRequirePermissions</code> or <code>IPermissionHolder</code></typeparam>
        /// <param name="node">The database object to use for the permission generation.</param>
        /// <returns>An <code>IEnumerable</code> colection containing <code>PermissionSet</code>.</returns>
        public static IEnumerable<PermissionSet> CreateDefaultPermissions(IPermissionHolder permissionHolder)
        {
            var permissions = new List<PermissionSet>();
            permissions.Add(new PermissionSet() { Owner = permissionHolder.CompanyKey, Read = true, Write = true, Execute = true, Target = permissionHolder.UId });
            return permissions;
        }

    }
}
