using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.Design;
using System.ComponentModel.DataAnnotations.Schema;
using RSToolKit.Domain.Entities.Components;
using RSToolKit.Domain.Data;

namespace RSToolKit.Domain.Entities
{
    public class StyleBase
    {
        [Key]
        public Guid UId { get; set; }
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long SortingId { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
        public string Variable { get; set; }
        public string GroupName { get; set; }
        public string Sort { get; set; }
        public string SubSort { get; set; }
        public string Type { get; set; }

        public StyleBase()
        {
            UId = Guid.NewGuid();
            Name = "New Style";
            Value = null;
            Variable = null;
            GroupName = null;
            Sort = null;
            SubSort = null;
            Type = "string";
        }
    }

    [Table("ComponentStyle")]
    public class ComponentStyle : StyleBase, IFormItem
    {
        [ForeignKey("ComponentKey")]
        public virtual Component Component { get; set; }
        public Guid ComponentKey { get; set; }

        public ComponentStyle() : base()
        { }

        public Form GetForm()
        {
            return Component.GetForm();
        }

        public INode GetNode()
        {
            return Component.GetNode();
        }
    }

    [Table("PriceStyle")]
    public class PriceStyle : StyleBase, IFormItem
    {
        [ForeignKey("PriceGroupKey")]
        public virtual PriceGroup PriceGroup { get; set; }
        public Guid PriceGroupKey { get; set; }

        public PriceStyle() : base()
        { }

        public Form GetForm()
        {
            return PriceGroup.GetForm();
        }

        public INode GetNode()
        {
            return PriceGroup.GetNode();
        }
    }

    [Table("SeatingStyle")]
    public class SeatingStyle : StyleBase, INodeItem
    {
        [ForeignKey("SeatingKey")]
        public virtual Seating Seating { get; set; }
        public Guid SeatingKey { get; set; }
        public SeatingStyle() : base()
        { }

        public Form GetForm()
        {
            return Seating.GetForm();
        }

        public INode GetNode()
        {
            return Seating.GetNode();
        }
    }
}
