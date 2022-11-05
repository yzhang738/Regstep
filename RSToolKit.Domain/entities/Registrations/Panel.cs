using System;
using System.Collections.Generic;
using System.Linq;
using RSToolKit.Domain.Entities.Components;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using RSToolKit.Domain.Data;
using Newtonsoft.Json;
using RSToolKit.Domain.Entities.Clients;
using RSToolKit.Domain.JItems;

namespace RSToolKit.Domain.Entities
{
    public class Panel
        : ILogicHolder, IComparable<Panel>, IFormItem, IFormComponent
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

        public virtual List<Audience> Audiences { get; set; }
        [CascadeDelete]
        public virtual List<Component> Components { get; set; }
        [CascadeDelete]
        public virtual List<Logic> Logics { get; set; }

        [ForeignKey("PageKey")]
        public virtual Page Page { get; set; }
        public Guid PageKey { get; set; }

        [MaxLength(1000)]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string Description { get; set; }

        public RSVPType RSVP { get; set; }

        public bool AdminOnly { get; set; }
        public bool Display { get; set; }
        public bool Enabled { get; set; }
        public bool Locked { get; set; }

        public int Order { get; set; }

        [NotMapped]
        public List<JLogic> JLogics { get; set; }
        public string RawJLogics
        {
            get
            {
                return JsonConvert.SerializeObject(JLogics);
            }
            set
            {
                if (String.IsNullOrEmpty(value))
                {
                    JLogics = new List<JLogic>();
                }
                else
                {
                    try
                    {
                        JLogics = JsonConvert.DeserializeObject<List<JLogic>>(value);
                    }
                    catch (Exception)
                    {
                        JLogics = new List<JLogic>();
                    }
                }
            }
        }


        public Panel()
            : base()
        {
            Name = "New Panel";
            UId = Guid.Empty;
            DateCreated = DateModified = DateTimeOffset.Now;
            Display = true;
            Enabled = true;
            Order = 1;
            RSVP = RSVPType.None;
            Components = new List<Component>();
            Audiences = new List<Audience>();
            Description = "";
            Locked = false;
            Logics = new List<Logic>();
            JLogics = new List<JLogic>();
        }

        public static Panel New(FormsRepository repository, Page page, Company company, User user, string name, string description = "", Guid? owner = null, Guid? group = null, string permission = "770")
        {
            var node = new Panel()
            {
                UId = Guid.NewGuid(),
                Name = name ?? "New Panel - " + DateTimeOffset.UtcNow.ToString("MM/dd/yyyy h:mm tt") + ".",
                Description = description,
            };
            if (page.Panels.Count > 0)
                node.Order = page.Panels.Last().Order + 1;
            page.Panels.Add(node);
            repository.Commit();
            return node;
        }

        public Form GetForm()
        {
            return Page.GetForm();
        }

        public INode GetNode()
        {
            return Page.GetNode();
        }

        public int CompareTo(Panel other)
        {
            return Order.CompareTo(other.Order);
        }
    }
}
