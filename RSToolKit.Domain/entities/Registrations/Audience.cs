using RSToolKit.Domain.Data;
using RSToolKit.Domain.Entities.Components;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;
using RSToolKit.Domain.Entities.Clients;

// Complete
namespace RSToolKit.Domain.Entities
{
    public class Audience
        : IFormItem, IComparable<Audience>, IRequirePermissions, IFormComponent
    {

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long SortingId { get; set; }

        [Key]
        public Guid UId { get; set; }

        [MaxLength(250)]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string Name { get; set; }

        public DateTimeOffset DateCreated { get; set; }
        public DateTimeOffset DateModified { get; set; }

        public Guid ModificationToken { get; set; }
        public Guid ModifiedBy { get; set; }

        public virtual List<Page> Pages { get; set; }
        public virtual List<Panel> Panels { get; set; }
        public virtual List<Component> Components { get; set; }
        public virtual List<Registrant> Registrants { get; set; }
        public virtual List<Prices> Prices { get; set; }

        [JsonIgnore]
        [ForeignKey("FormKey")] 
        public virtual Form Form { get; set; }
        public Guid FormKey { get; set; }

        [MaxLength(100)]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string Label { get; set; }

        public int Order { get; set; }

        public Audience()
        {
            Order = -1;
            Label = "New Audience";
            Pages = new List<Page>();
            Panels = new List<Panel>();
            Components = new List<Component>();
            Registrants = new List<Registrant>();
            Prices = new List<Prices>();
        }

        public static Audience New(FormsRepository repository, Company company, User user, Form form, Guid? owner = null, Guid? group = null, string name = null, string permission = "770")
        {
            name = name ?? "New Audience - " + DateTimeOffset.UtcNow.ToString("MM/dd/yyyy h:mm tt") + ".";
            var node = new Audience()
            {
                UId = Guid.NewGuid(),
                Label = name,
                Name = name,
                Form = form,
                FormKey = form.UId,
            };
            form.Audiences.Add(node);
            repository.Commit();
            return node;
        }

        public IPermissionHolder GetPermissionHolder()
        {
            return Form;
        }

        public Form GetForm()
        {
            return Form;
        }

        public INode GetNode()
        {
            return Form as INode;
        }

        #region Comparings

        public override bool Equals(object obj)
        {
            if (obj is Audience)
            {
                return ((Audience)obj).UId == UId;
            }
            else
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public int CompareTo(Audience other)
        {
            return Order - other.Order;
        }

        #endregion

    }
}
