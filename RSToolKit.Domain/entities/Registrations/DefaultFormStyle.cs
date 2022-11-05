using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RSToolKit.Domain.Data;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using RSToolKit.Domain.Entities.Clients;

namespace RSToolKit.Domain.Entities
{
    public class DefaultFormStyle : INodeItem, IRSData
    {

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long SortingId { get; set; }

        [Key]
        public Guid UId { get; set; }

        [MaxLength(250)]
        public string Name { get; set; }

        public DateTimeOffset DateCreated { get; set; }
        public DateTimeOffset DateModified { get; set; }

        public Guid ModificationToken { get; set; }
        public Guid ModifiedBy { get; set; }

        [ForeignKey("CompanyKey")]
        public virtual Company Company { get; set; }
        public Guid? CompanyKey { get; set; }

        public string Variable { get; set; }
        public string Value { get; set; }
        public string GroupName { get; set; }
        public string Sort { get; set; }
        public string Type { get; set; }
        public string SubSort { get; set; }


        public DefaultFormStyle()
        {
            UId = Guid.NewGuid();
            Sort = "";
            Name = "";
            Type = "";
            SubSort = "";
        }

        public INode GetNode()
        {
            return Company as INode;
        }

    }

    public class FormStyle : IRSData, IFormItem
    {

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long SortingId { get; set; }

        [Key]
        public Guid UId { get; set; }

        [MaxLength(250)]
        public string Name { get; set; }

        [MaxLength(3)]
        public string Permission { get; set; }

        public DateTimeOffset DateCreated { get; set; }
        public DateTimeOffset DateModified { get; set; }

        public Guid Owner { get; set; }
        public Guid Group { get; set; }
        public Guid ModificationToken { get; set; }
        public Guid ModifiedBy { get; set; }

        [ForeignKey("FormKey")]
        public Form Form { get; set; }
        public Guid FormKey { get; set; }

        public string Variable { get; set; }
        public string Value { get; set; }
        public string GroupName { get; set; }
        public string Sort { get; set; }
        public string Type { get; set; }
        public string SubSort { get; set; }

        public FormStyle()
        {
            UId = Guid.NewGuid();
        }

        public FormStyle DeepCopy(Form form)
        {
            var fs = new FormStyle();
            fs.Form = form;
            form.FormStyles.Add(fs);
            fs.Variable = Variable;
            fs.Value = Value;
            fs.GroupName = GroupName;
            fs.Sort = Sort;
            fs.Type = Type;
            fs.SubSort = SubSort;
            return fs;
        }

        public Form GetForm()
        {
            return Form;
        }

        public INode GetNode()
        {
            return Form as INode;
        }

    }
}
