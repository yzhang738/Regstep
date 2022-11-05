using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RSToolKit.Domain.Data;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using RSToolKit.Domain.Entities.Clients;
using Newtonsoft.Json;
using RSToolKit.Domain.JItems;
using RSToolKit.Domain.Security;

namespace RSToolKit.Domain.Entities
{
    public class SavedTable : IReport
    {

        protected JTable pr_table;

        [Key]
        public Guid UId { get; set; }

        [NotMapped]
        public JTable Table
        {
            get
            {
                return pr_table;
            }
            set
            {
                pr_table = value;
                Favorite = value.Favorite;
                pr_table.SavedId = UId.ToString();
            }
        }

        public Guid ParentKey { get; set; }

        [ForeignKey("CompanyKey")]
        public virtual Company Company { get; set; }
        public Guid CompanyKey { get; set; }

        public string Name { get; set; }

        public DateTimeOffset DateCreated { get; set; }

        public DateTimeOffset DateModified { get; set; }

        public Guid ModificationToken { get; set; }

        public Guid ModifiedBy { get; set; }

        [Index(IsClustered = true)]
        public long SortingId { get; set; }

        [MaxLength]
        public string RawJtable
        {
            get
            {
                return JsonConvert.SerializeObject(Table);
            }
            set
            {
                Table = JsonConvert.DeserializeObject<JTable>(value);
            }
        }

        [NotMapped]
        public Form Form { get; set; }

        public bool Favorite { get; set; }

        public SavedTable()
        {
            pr_table = new JTable();
            Form = null;
        }

        public static SavedTable New(FormsRepository repository, Company company, Guid parentKey)
        {
            var node = new SavedTable()
            {
                ParentKey = parentKey,
                Company = company
            };
            PermissionSet.CreateDefaultPermissions(repository, node, company.UId);
            repository.Add(node);
            return node;
        }
    }
}
