using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using RSToolKit.Domain.Data;

namespace RSToolKit.Domain.Entities.Clients
{
    public class AvailableRoles
    {
        [Key]
        public Guid UId { get; set; }
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long SortingId { get; set; }

        [ForeignKey("RoleKey")]
        public virtual AppRole Role { get; set; }
        public string RoleKey { get; set; }

        [ForeignKey("CompanyKey")]
        public virtual Company Company { get; set; }
        public Guid CompanyKey { get; set; }

        public int TotalAvailable { get; set; }

        public AvailableRoles()
        {
            TotalAvailable = -1;
        }

        public static AvailableRoles New(FormsRepository repository, User user, Company company, AppRole role)
        {
            var aRole = new AvailableRoles();
            aRole.UId = Guid.NewGuid();
            aRole.Role = role;
            aRole.RoleKey = role.Id;
            aRole.Company = company;
            aRole.CompanyKey = company.UId;
            repository.Add(aRole);
            repository.Commit();
            return aRole;
        }

        public static AvailableRoles New(Company company, AppRole role)
        {
            var aRole = new AvailableRoles();
            aRole.UId = Guid.NewGuid();
            aRole.Role = role;
            aRole.RoleKey = role.Id;
            aRole.Company = company;
            aRole.CompanyKey = company.UId;
            return aRole;
        }

    }
}
