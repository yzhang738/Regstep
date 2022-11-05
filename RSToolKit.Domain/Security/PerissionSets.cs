using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using RSToolKit.Domain.Data;

namespace RSToolKit.Domain.Security
{
    public class PermissionSet
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long SortingId { get; set; }
        public Guid Target { get; set; }
        public Guid Owner { get; set; }
        public bool Read { get; set; }
        public bool Execute { get; set; }
        public bool Write { get; set; }
        public PermissionSet()
        {
            Read = false;
            Execute = false;
            Write = false;
        }

        public static PermissionSet New(FormsRepository repository, Guid target, Guid owner, char p)
        {
            bool read, write, execute;
            read = write = execute = false;
            switch (p)
            {
                case '1':
                    execute = true;
                    break;
                case '2':
                    write = true;
                    break;
                case '3':
                    write = true;
                    execute = true;
                    break;
                case '4':
                    read = true;
                    break;
                case '5':
                    read = true;
                    execute = true;
                    break;
                case '6':
                    read = true;
                    write = true;
                    break;
                case '7':
                    read = true;
                    write = true;
                    execute = true;
                    break;
            }
            return New(repository, target, owner, read, write, execute);
        }

        public static void CreateDefaultPermissions(FormsRepository repository, INode node, Guid target, bool read = true, bool write = true, bool execute = true)
        {
            var permission = new PermissionSet()
            {
                Target = node.UId,
                Owner = target,
                Read = read,
                Write = write,
                Execute = execute
            };
            if (target != Guid.Empty)
            {
                var permissionAnony = new PermissionSet()
                {
                    Target = node.UId,
                    Owner = Guid.Empty,
                    Read = false,
                    Write = false,
                    Execute = false
                };
                repository.Add(permissionAnony);
            }
            repository.Add(permission);
        }

        public static PermissionSet New(FormsRepository repository, Guid target, Guid owner, bool read, bool write, bool execute)
        {
            var permission = new PermissionSet()
            {
                Target = target,
                Owner = owner,
                Read = read,
                Write = write,
                Execute = execute
            };
            repository.Add(permission);
            repository.Commit();
            return permission;
        }

        public static PermissionSet New(Guid target, Guid owner, bool read, bool write, bool execute)
        {
            var permission = new PermissionSet()
            {
                Target = target,
                Owner = owner,
                Read = read,
                Write = write,
                Execute = execute
            };
            return permission;
        }

        public static PermissionSet New(Guid target, Guid owner, char p)
        {
            bool read, write, execute;
            read = write = execute = false;
            switch (p)
            {
                case '1':
                    execute = true;
                    break;
                case '2':
                    write = true;
                    break;
                case '3':
                    write = true;
                    execute = true;
                    break;
                case '4':
                    read = true;
                    break;
                case '5':
                    read = true;
                    execute = true;
                    break;
                case '6':
                    read = true;
                    write = true;
                    break;
                case '7':
                    read = true;
                    write = true;
                    execute = true;
                    break;
            }
            var permission = new PermissionSet()
            {
                Target = target,
                Owner = owner,
                Read = read,
                Write = write,
                Execute = execute
            };
            return permission;
        }

        public bool Access(SecurityAccessType type)
        {
            switch (type)
            {
                case SecurityAccessType.Read:
                    return Read;
                case SecurityAccessType.Write:
                    return Write;
                case SecurityAccessType.Execute:
                    return Execute;
                default:
                    return false;
            }
        }
    }
}
