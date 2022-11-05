using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Principal;
using RSToolKit.Domain.Entities.Clients;
using RSToolKit.Domain.Identity;
using RSToolKit.Domain.Data;
using RSToolKit.Domain.Exceptions;

namespace RSToolKit.Domain.Security
{
    public static class Security
    {
        /// <summary>
        /// The type for an item that is protected.
        /// </summary>
        public static Type ProtectedItem = typeof(IProtected);
        /// <summary>
        /// The type for an item that is globally protected.
        /// </summary>
        public static Type GloballyProtected = typeof(IGloballyProtected);
        /// <summary>
        /// The type for an item that inherits protection.
        /// </summary>
        public static Type ProtectionInherited = typeof(IProtectionInherited);

        /// <summary>
        /// Gets the permission with the highest level of access for the specified secure user.
        /// </summary>
        /// <param name="item">The item to check permissions for.</param>
        /// <param name="user">The secure user requesting access.</param>
        /// <param name="action">The type of action wishing to be performed.</param>
        /// <param name="ignoreException">Flag to ignore insufficient permissions exceptions.</param>
        /// <returns>The permission set that has the highest level of access for the specified secure user.</returns>
        /// <exception cref="InsufficientPermissionsException">Thrown if the <paramref name="ignoreException"/> is not set and sufficient permissions where not found.</exception>
        public static PermissionSet GetPermission(this IProtected item, SecuritySettings user, SecurityAccessType action, bool ignoreException = false)
        {
            // First we need to check if it is an globally protected item.
            if (item is IGloballyProtected)
            {
                // It is, if the user does not have global permissions, the check automatically fails.
                if (user.GlobalPermissions)
                    return new PermissionSet() { Execute = true, Read = true, Write = true, Target = item.UId, Owner = Guid.Empty, SortingId = -1 };

                if (ignoreException)
                    return new PermissionSet() { Execute = false, Read = false, Write = false, Target = item.UId, Owner = Guid.Empty, SortingId = -1 };
                throw new InsufficientPermissionsException();
            }
            PermissionSet set;
            if (user.GlobalPermissions || (user.CompanyAdministrator && user.AppUser.WorkingCompanyKey == item.CompanyKey))
                return new PermissionSet() { Execute = true, Read = true, Write = true, Target = item.UId, Owner = Guid.Empty, SortingId = -1 };
            IEnumerable<PermissionSet> permissions;
            using (var context = new EFDbContext())
                // We query the databse for the permissions sets.
                permissions = context.PermissionsSets.Where(p => p.Target == item.UId).ToList();
            if (permissions.Count() == 0 && !ignoreException)
                // No permissions so we return no access;
                throw new InsufficientPermissionsException();
            // First we check anonymous permissions.
            set = permissions.FirstOrDefault(p => p.Owner == Guid.Empty);
            if (set != null && set.HasAccess(action))
                return set;
            set = permissions.FirstOrDefault(p => p.Owner == item.CompanyKey);
            if (set != null && set.HasAccess(action))
                return set;
            // Company permissions failed. Now we check custom group permissions.
            foreach (var group in user.AppUser.CustomGroups)
            {
                set = permissions.FirstOrDefault(p => p.Owner == group.UId);
                if (set != null && set.HasAccess(action))
                    return set;
                // Permissions for this group failed. Lets move on to the next one.
            }
            // All permissions failed. We either now throw an exception or return false depending on the ignoreException flag.
            if (!ignoreException)
                throw new InsufficientPermissionsException();
            return new PermissionSet() { Execute = false, Read = false, Write = false, Target = item.UId, Owner = Guid.Empty, SortingId = -1 };
        }

        /// <summary>
        /// Gets the permission with the highest level of access for the specified secure user.
        /// </summary>
        /// <param name="item">The item to check permissions for.</param>
        /// <param name="user">The secure user requesting access.</param>
        /// <param name="action">The type of action wishing to be performed.</param>
        /// <param name="ignoreException">Flag to ignore insufficient permissions exceptions.</param>
        /// <returns>The permission set that has the highest level of access for the specified secure user.</returns>
        /// <exception cref="InsufficientPermissionsException">Thrown if the <paramref name="ignoreException"/> is not set and sufficient permissions where not found.</exception>
        public static PermissionSet GetPermission(this IProtectionInherited item, SecuritySettings user, SecurityAccessType action, bool ignoreException = false)
        {
            return item.ProtectedItem.GetPermission(user, action, ignoreException);
        }


        /// <summary>
        /// Gets the permission with the highest level of access for the specified secure user.
        /// </summary>
        /// <param name="location">The location of the item to check permissions for.</param>
        /// <param name="user">The secure user requesting access.</param>
        /// <param name="action">The type of action wishing to be performed.</param>
        /// <param name="ignoreException">Flag to ignore insufficient permissions exceptions.</param>
        /// <returns>The permission set that has the highest level of access for the specified secure user.</returns>
        /// <exception cref="InsufficientPermissionsException">Thrown if the <paramref name="ignoreException"/> is not set and sufficient permissions where not found.</exception>
        public static PermissionSet GetPermission<Titem>(this Domain.Data.Repositories.ItemLocation<Titem> location, SecureUser user, SecurityAccessType action, bool ignoreException = false)
            where Titem : class, IDataItem
        {
            var itemType = typeof(Titem);
            if (!itemType.IsAssignableFrom(ProtectedItem) || itemType.IsAssignableFrom(ProtectionInherited))
                // No protection is being used.
                return new PermissionSet() { Execute = true, Read = true, Write = true, Target = location.UId, Owner = Guid.Empty, SortingId = -1 };

            // First we need to check if it is an globally protected item.
            if (typeof(Titem).IsAssignableFrom(typeof(IGloballyProtected)))
            {
                // It is, if the user does not have global permissions, the check automatically fails.
                if (user.GlobalPermissions)
                    return new PermissionSet() { Execute = true, Read = true, Write = true, Target = location.UId, Owner = Guid.Empty, SortingId = -1 };

                if (ignoreException)
                    return new PermissionSet() { Execute = false, Read = false, Write = false, Target = location.UId, Owner = Guid.Empty, SortingId = -1 };
                throw new InsufficientPermissionsException();
            }
            List<PermissionSet> permissions;

            using (var context = new EFDbContext())
            {
                var permissionTarget = location.UId;
                if (itemType.IsAssignableFrom(ProtectionInherited))
                {
                    var item = location.DbSet.Find(location.Id);
                    if (item == null)
                        throw new Data.Repositories.RecordNotFoundException("Can't run permissions on a record that does not exist.");
                    permissionTarget = (item as IProtectionInherited).ProtectionTarget;
                }
                // We query the databse for the permissions sets.
                permissions = context.PermissionsSets.Where(p => p.Target == permissionTarget).ToList();
            }
            PermissionSet set;
            if (user.GlobalPermissions || (user.CompanyAdministrator && user.AppUser.CompanyKey == location.CompanyKey))
                return new PermissionSet() { Execute = true, Read = true, Write = true, Target = location.UId, Owner = Guid.Empty, SortingId = -1 };
            if (permissions.Count() == 0 && !ignoreException)
                // No permissions so we return no access;
                throw new InsufficientPermissionsException();
            // First we check anonymous permissions.
            set = permissions.FirstOrDefault(p => p.Owner == Guid.Empty);
            if (set != null && set.HasAccess(action))
                return set;
            set = permissions.FirstOrDefault(p => p.Owner == location.CompanyKey);
            if (set != null && set.HasAccess(action))
                return set;
            // Company permissions failed. Now we check custom group permissions.
            foreach (var group in user.AppUser.CustomGroups)
            {
                set = permissions.FirstOrDefault(p => p.Owner == group.UId);
                if (set != null && set.HasAccess(action))
                    return set;
                // Permissions for this group failed. Lets move on to the next one.
            }
            // All permissions failed. We either now throw an exception or return false depending on the ignoreException flag.
            if (!ignoreException)
                throw new InsufficientPermissionsException();
            return new PermissionSet() { Execute = false, Read = false, Write = false, Target = location.UId, Owner = Guid.Empty, SortingId = -1 };
        }

        /// <summary>
        /// Checks to see if a permissions set has permissions for a specified action.
        /// </summary>
        /// <param name="set">The permission set.</param>
        /// <param name="action">The action.</param>
        /// <returns>Flag for possitive permissions.</returns>
        public static bool HasAccess(this PermissionSet set, SecurityAccessType action)
        {
            switch (action)
            {
                case SecurityAccessType.Read:
                    return set.Read;
                case SecurityAccessType.Write:
                    return set.Write;
                case SecurityAccessType.Execute:
                    return set.Execute;
            }
            return false;
        }
    }
}
