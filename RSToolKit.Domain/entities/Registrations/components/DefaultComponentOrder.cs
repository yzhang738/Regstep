using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using RSToolKit.Domain.Data;
using RSToolKit.Domain.Entities.Clients;

namespace RSToolKit.Domain.Entities.Components
{
    public class DefaultComponentOrder : IFormItem
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long SortingId { get; set; }
        [Key]
        public Guid UId { get; set; }

        [ForeignKey("FormKey")]
        public virtual Form Form { get; set; }
        public Guid FormKey { get; set; }

        public string ComponentType { get; set; }
        public string Order { get; set; }

        public DefaultComponentOrder(Type component)
        {
            if (component == typeof(Input))
                Order = "label,description,input";
            else if (component == typeof(CheckboxGroup))
                Order = "label,description,agenda,price,input";
            else if (component == typeof(CheckboxItem))
                Order = "label,description,agenda,price";
            else if (component == typeof(RadioGroup))
                Order = "label,description,agenda,price,input";
            else if (component == typeof(RadioItem))
                Order = "label,description,agenda,price,input";
            else if (component == typeof(DropdownGroup))
                Order = "label,description,agenda,price,input";
            else if (component == typeof(RatingSelect))
                Order = "label,description,agenda,price,input";
            ComponentType = component.ToString();
            UId = Guid.NewGuid();
        }

        public DefaultComponentOrder()
        { }

        public DefaultComponentOrder DeepCopy(Form form)
        {
            var dco = new DefaultComponentOrder();
            dco.Order = Order;
            dco.ComponentType = ComponentType;
            dco.UId = Guid.NewGuid();
            dco.Form = form;
            return dco;
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
