using Microsoft.AspNet.Identity;
using Newtonsoft.Json;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using RSToolKit.Domain;
using RSToolKit.Domain.Data;
using RSToolKit.Domain.Engines;
using RSToolKit.Domain.Entities;
using RSToolKit.Domain.Entities.Clients;
using RSToolKit.Domain.Entities;
using RSToolKit.Domain.Entities.Components;
using RSToolKit.Domain.Entities.Email;
using RSToolKit.Domain.Entities.MerchantAccount;
using RSToolKit.Domain.JItems;
using RSToolKit.Domain.Security;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Security.Principal;

namespace RSToolKit.Domain.Identity
{
    /// <summary>
    /// Contains extensions for the <code>IPrincipal</code> class.
    /// </summary>
    public static class IdentityExtensions
    {
        public static readonly string[] _globalPermissionRoles = new string[] { "Super Administrators", "Programmer", "Administrators" };

        /// <summary>
        /// Determines if the principal is authenticated and a member of a role that has global rwx permissions.
        /// </summary>
        /// <param name="principal">The principal in question.</param>
        /// <returns>True if principal has global permissions, false otherwise.</returns>
        public static bool HasGlobalPermissions(this IPrincipal principal)
        {
            if (principal.Identity.IsAuthenticated)
            {
                foreach (var role in _globalPermissionRoles)
                {
                    if (principal.IsInRole(role))
                        return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Checks to see if a princip
        /// </summary>
        /// <param name="principal"></param>
        /// <param name="roles"></param>
        /// <returns></returns>
        public static bool IsInRole(this IPrincipal principal, IEnumerable<string> roles)
        {
            if (principal.HasGlobalPermissions())
                return true;
            foreach (var role in roles)
            {
                if (principal.IsInRole(role))
                    return true;
            }
            return false;
        }
    }
}