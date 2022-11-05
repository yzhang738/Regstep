using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using RSToolKit.Domain.Data;
using Newtonsoft.Json;
using RSToolKit.Domain.Entities.Clients;
using RSToolKit.Domain.JItems;
using RSToolKit.Domain.Engines;

namespace RSToolKit.Domain.Entities
{
    public class Page
        : ILogicHolder, IRSData, IComparable<Page>, IFormItem, IFormComponent
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

        [CascadeDelete]
        public virtual List<Logic> Logics { get; set; }
        public virtual List<Audience> Audiences { get; set; }
        [CascadeDelete]
        public virtual List<Panel> Panels { get; set; }

        [ForeignKey("FormKey")]
        public virtual Form Form { get; set; }
        public Guid FormKey { get; set; }
        
        [MaxLength(1000)]
        public string Description { get; set; }

        public RSVPType RSVP { get; set; }
        public PageType Type { get; set; }

        public bool AdminOnly { get; set; }
        public bool Display { get; set; }
        public bool Enabled { get; set; }
        public bool Locked { get; set; }

        public int PageNumber { get; set; }

        //*
        [NotMapped]
        public List<JLogic> JLogics { get; set; }
        public string RawJLogics
        {
            get
            {
                return JsonConvert.SerializeObject(JLogics ?? new List<JLogic>());
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
                        JLogics = JsonConvert.DeserializeObject<List<JLogic>>(value ?? "[]");
                        if (JLogics == null)
                            JLogics = new List<JLogic>();
                    }
                    catch (Exception)
                    {
                        JLogics = new List<JLogic>();
                    }
                }
            }
        }
        //*/

        public Page()
            : base()
        {
            Description = "";
            RSVP = RSVPType.None;
            AdminOnly = false;
            PageNumber = 1;
            Display = true;
            Enabled = true;
            Panels = new List<Panel>();
            Audiences = new List<Audience>();
            Logics = new List<Logic>();
            Locked = false;
            Type = PageType.UserDefined;
            JLogics = new List<JLogic>();
        }

        public static Page New(FormsRepository repository, Form form, Company comany, User user, string name, string description = "", PageType type = PageType.UserDefined, Guid? owner = null, Guid? group = null, string permission = "770")
        {
            var node = new Page()
            {
                UId = Guid.NewGuid(),
                Description = description,
                Name = name == null ? "New Page - " + DateTimeOffset.UtcNow.ToString("dd/mm/yyyy h:mm tt") : name,
                Type = type
            };
            form.Pages.Sort();
            if (form.Pages.Count > 0)
                node.PageNumber = form.Pages.Last().PageNumber + 1;
            else
            {
                if (form.Survey)
                    node.PageNumber = 1;
                else
                    node.PageNumber = 3;
            }
            form.Pages.Add(node);
            repository.Commit();
            return node;
        }

        public Form GetForm()
        {
            return Form;
        }

        public INode GetNode()
        {
            return Form as INode;
        }

        public bool IsBlank(Registrant registrant, FormsRepository repository, bool showingAdminOnly = false, Dictionary<string, IEnumerable<JLogicCommand>> allCommands = null)
        {
            // We run through each panel.
            var rsvpPage = Form.Pages.FirstOrDefault(p => p.Type == PageType.RSVP);
            var rsvpEnabled = rsvpPage != null ? rsvpPage.Enabled : false;
            var shownComponentsCount = 0;
            foreach (var panel in Panels)
            {
                if ((!showingAdminOnly && panel.AdminOnly) || !panel.Enabled)
                    continue;
                if (rsvpEnabled)
                    if (panel.RSVP != RSVPType.None)
                        if ((registrant.RSVP && panel.RSVP == RSVPType.Decline) || (!registrant.RSVP && panel.RSVP == RSVPType.Accept))
                            continue;
                if (panel.Audiences.Count > 0 && (registrant.Audience == null || !panel.Audiences.Contains(registrant.Audience)))
                    continue;
                if (LogicEngine.RunLogic(panel, repository, registrant: registrant).Count(c => c.Command == JLogicWork.Hide) > 0)
                    continue;
                /*
                if (panel.RunLogic(registrant, true, repository).Count(c => c.Command == LogicWork.Hide) > 0)
                    continue;
                //*/
                foreach (var component in panel.Components)
                {
                    var logicCommands = LogicEngine.RunLogic(component, repository, registrant: registrant);
                    //var logicCommands = component.RunLogic(registrant, true, repository);
                    if (!showingAdminOnly && component.AdminOnly)
                        continue;
                    if (!component.Enabled && logicCommands.Count(c => c.Command == JLogicWork.Show) == 0)
                        continue;
                    if (rsvpEnabled)
                        if ((registrant.RSVP && component.RSVP == RSVPType.Decline) || (!registrant.RSVP && component.RSVP == RSVPType.Accept))
                            continue;
                    if (component.Audiences.Count > 0 && (registrant.Audience == null || !component.Audiences.Contains(registrant.Audience)))
                        continue;
                    if (logicCommands.Count(c => c.Command == JLogicWork.Hide) > 0)
                        continue;
                    shownComponentsCount++;
                }
            }
            return shownComponentsCount == 0;
        }

        public bool IsBlank(Registrant registrant, bool showingAdminOnly = false, Dictionary<string, IEnumerable<JLogicCommand>> allCommands = null)
        {
            allCommands = allCommands ?? new Dictionary<string, IEnumerable<JLogicCommand>>();
            // We run through each panel.
            var rsvpPage = Form.Pages.FirstOrDefault(p => p.Type == PageType.RSVP);
            var rsvpEnabled = rsvpPage != null ? rsvpPage.Enabled : false;
            var shownComponentsCount = 0;
            foreach (var panel in Panels)
            {
                if ((!showingAdminOnly && panel.AdminOnly) || !panel.Enabled)
                    continue;
                if (rsvpEnabled)
                    if (panel.RSVP != RSVPType.None)
                        if ((registrant.RSVP && panel.RSVP == RSVPType.Decline) || (!registrant.RSVP && panel.RSVP == RSVPType.Accept))
                            continue;
                if (panel.Audiences.Count > 0 && (registrant.Audience == null || !panel.Audiences.Contains(registrant.Audience)))
                    continue;
                if (LogicEngine.RunLogic(panel, registrant: registrant, allCommands: allCommands).Count(c => c.Command == JLogicWork.Hide) > 0)
                    continue;
                /*
                if (panel.RunLogic(registrant, true, repository).Count(c => c.Command == LogicWork.Hide) > 0)
                    continue;
                //*/
                foreach (var component in panel.Components)
                {
                    var logicCommands = LogicEngine.RunLogic(component, registrant: registrant, allCommands: allCommands);
                    //var logicCommands = component.RunLogic(registrant, true, repository);
                    if (!showingAdminOnly && component.AdminOnly)
                        continue;
                    if (!component.Enabled && logicCommands.Count(c => c.Command == JLogicWork.Show) == 0)
                        continue;
                    if (rsvpEnabled)
                        if ((registrant.RSVP && component.RSVP == RSVPType.Decline) || (!registrant.RSVP && component.RSVP == RSVPType.Accept))
                            continue;
                    if (component.Audiences.Count > 0 && (registrant.Audience == null || !component.Audiences.Contains(registrant.Audience)))
                        continue;
                    if (logicCommands.Count(c => c.Command == JLogicWork.Hide) > 0)
                        continue;
                    shownComponentsCount++;
                }
            }
            return shownComponentsCount == 0;
        }


        public int CompareTo(Page other)
        {
            return PageNumber.CompareTo(other.PageNumber);
        }

    }

    public enum PageType
    {
        [StringValue("User Defined")]
        UserDefined = -1,
        [StringValue("RSVP Page")]
        RSVP = 0,
        [StringValue("Audience Page")]
        Audience = 1,
        [StringValue("Confirmaiton Page")]
        Confirmation = 2,
        [StringValue("Billing Page")]
        Billing = 3,
        [StringValue("Billing Confirmation Page")]
        BillingComfirnmation = 4
    }
}
