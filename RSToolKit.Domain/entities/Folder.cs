using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RSToolKit.Domain.Data;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using RSToolKit.Domain.Entities.Clients;
using RSToolKit.Domain.Security;

namespace RSToolKit.Domain.Entities
{
    public class Folder
        : IPointerTarget, ICompanyHolder, INode
    {

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long SortingId { get; set; }

        [Key]
        public Guid UId { get; set; }

        [MaxLength(250)]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string Name { get; set; }

        [MaxLength(3)]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string Permission { get; set; }

        public DateTimeOffset DateCreated { get; set; }
        public DateTimeOffset DateModified { get; set; }

        public Guid ModificationToken { get; set; }
        public Guid ModifiedBy { get; set; }

        [ForeignKey("CompanyKey")]
        public virtual Company Company { get; set; }
        public Guid CompanyKey { get; set; }

        public virtual List<Folder> Children { get; set; }
        public virtual List<Pointer> Pointers { get; set; }

        [ForeignKey("ParentKey")]
        public virtual Folder Parent { get; set; }
        public Guid? ParentKey { get; set; }

        #region Constructors

        public Folder()
        {
            Pointers = new List<Pointer>();
            Children = new List<Folder>();
            Name = "New Folder";
        }

        public static Folder New(FormsRepository repository, Company company, User user, string name, Folder parent = null, Guid? owner = null, Guid? group = null, string permission = "770")
        {
            var node = new Folder()
            {
                UId = Guid.NewGuid(),
                Name = name == null ? "New Folder" : name,
                Parent = parent,
                Permission = permission,
                CompanyKey = company.UId,
                Company = company
            };
            PermissionSet.CreateDefaultPermissions(repository, node, company.UId);
            if (parent != null)
                parent.Children.Add(node);
            else
                repository.Add(node);
            repository.Commit();
            return node;
        }

        #endregion

        #region Icomparable

        public int CompareTo(Folder other)
        {
            return UId.CompareTo(other.UId);
        }

        #endregion
    }
}
