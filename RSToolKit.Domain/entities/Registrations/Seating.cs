using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using RSToolKit.Domain.Data;
using Newtonsoft.Json;
using RSToolKit.Domain.Entities.Components;
using RSToolKit.Domain.Entities.Clients;

namespace RSToolKit.Domain.Entities
{
    public class Seating
        : IFormItem, IRSData, IRequirePermissions
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

        [NotMapped]
        public List<Audience> Audiences { get; set; }
        [NotMapped]
        public FormsRepository Repository { get; set; }

        [CascadeDelete]
        public virtual List<Seater> Seaters { get; set; }
        public virtual List<Component> Components { get; set; }
        [CascadeDelete]
        public virtual List<SeatingStyle> Styles { get; set; }

        [ForeignKey("FormKey")]
        public virtual Form Form { get; set; }
        public Guid FormKey{ get; set; }

        [MaxLength(1000)]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string FullLabel { get; set; }

        [MaxLength(1000)]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string WaitlistLabel{ get; set; }

        [MaxLength(1000)]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string WaitlistItemLabel { get; set; }

        public SeatingType SeatingType { get; set; }

        public bool Waitlistable { get; set; }
        public bool MultipleSeats { get; set; }

        public int MaxSeats { get; set; }

        public DateTimeOffset Start { get; set; }
        public DateTimeOffset End { get; set; }

        public TimeSpan? HoldInterval { get; set; }

        [NotMapped]
        public RegistrationType RegType
        {
            get
            {
                if (Form == null)
                    return RegistrationType.Live;
                return (Form.Status == FormStatus.Developement || Form.Status == FormStatus.Ready) ? RegistrationType.Test : RegistrationType.Live;
            }
        }

        [NotMapped]
        public int AvailableSeats
        {
            get
            {

                if (Seaters.Where(s => !s.Seated && s.Registrant.RSVP && s.Registrant.Type == RegType && s.Registrant.Status != RegistrationStatus.Canceled && s.Registrant.Status != RegistrationStatus.CanceledByAdministrator && s.Registrant.Status != RegistrationStatus.CanceledByCompany && s.Registrant.Status != RegistrationStatus.Deleted).Count() > 0)
                    return 0;
                var taken = Seaters.Where(s => s.Seated && s.Registrant.RSVP && s.Registrant.Type == RegType && s.Registrant.Status != RegistrationStatus.Canceled && s.Registrant.Status != RegistrationStatus.CanceledByAdministrator && s.Registrant.Status != RegistrationStatus.CanceledByCompany && s.Registrant.Status != RegistrationStatus.Deleted).Count();
                return MaxSeats - taken;
            }
        }

        [NotMapped]
        public int SeatsTaken
        {
            get
            {
                return Seaters.Where(s => s.Seated && s.Registrant.RSVP && s.Registrant.Type == RegType && s.Registrant.Status != RegistrationStatus.Canceled && s.Registrant.Status != RegistrationStatus.CanceledByAdministrator && s.Registrant.Status != RegistrationStatus.CanceledByCompany && s.Registrant.Status != RegistrationStatus.Deleted).Count();
            }
        }

        [NotMapped]
        public IEnumerable<Seater> Seated
        {
            get
            {
                return Seaters.Where(s => s.Seated && s.Registrant.RSVP && s.Registrant.Type == RegType && s.Registrant.Status != RegistrationStatus.Incomplete && s.Registrant.Status != RegistrationStatus.Canceled && s.Registrant.Status != RegistrationStatus.CanceledByAdministrator && s.Registrant.Status != RegistrationStatus.CanceledByCompany && s.Registrant.Status != RegistrationStatus.Deleted).OrderBy(s => s.DateSeated);
            }
        }

        [NotMapped]
        public IEnumerable<Seater> Waited
        {
            get
            {
                return Seaters.Where(s => !s.Seated && s.Registrant.RSVP && s.Registrant.Type == RegType && s.Registrant.Status != RegistrationStatus.Incomplete && s.Registrant.Status != RegistrationStatus.Canceled && s.Registrant.Status != RegistrationStatus.CanceledByAdministrator && s.Registrant.Status != RegistrationStatus.CanceledByCompany && s.Registrant.Status != RegistrationStatus.Deleted).OrderBy(s => s.Date);
            }
        }

        [NotMapped]
        public int Waiters
        {
            get
            {
                return Seaters.Where(s => !s.Seated && s.Registrant.RSVP && s.Registrant.Type == RegType && s.Registrant.Status != RegistrationStatus.Canceled && s.Registrant.Status != RegistrationStatus.CanceledByAdministrator && s.Registrant.Status != RegistrationStatus.CanceledByCompany && s.Registrant.Status != RegistrationStatus.Deleted).Count();
            }
        }

        [NotMapped]
        public int RawSeats
        {
            get
            {
                var taken = Seaters.Where(s => s.Seated && s.Registrant.RSVP && s.Registrant.Type == RegType && s.Registrant.Status != RegistrationStatus.Canceled && s.Registrant.Status != RegistrationStatus.CanceledByAdministrator && s.Registrant.Status != RegistrationStatus.CanceledByCompany && s.Registrant.Status != RegistrationStatus.Deleted).Count();
                return MaxSeats - taken;
            }
        }


        [NotMapped]
        public bool HasWaiters
        {
            get
            {
                return Seaters.Where(s => !s.Seated && s.Registrant.RSVP && s.Registrant.Type == RegType && s.Registrant.Status != RegistrationStatus.Canceled && s.Registrant.Status != RegistrationStatus.CanceledByAdministrator && s.Registrant.Status != RegistrationStatus.CanceledByCompany && s.Registrant.Status != RegistrationStatus.Deleted).Count() > 0;
            }
        }

        public Seating()
        {
            Form = null;
            SeatingType = SeatingType.NoSeating;
            Start = DateTimeOffset.UtcNow;
            End = DateTimeOffset.UtcNow;
            MaxSeats = -1;
            Seaters = new List<Seater>();
            Waitlistable = false;
            MultipleSeats = false;
            FullLabel = "No Seats Available";
            WaitlistLabel = "Waitlisting.";
            WaitlistItemLabel = "Item Waitlisting.";
            Audiences = new List<Audience>();
            Components = new List<Component>();
        }

        public static Seating New(FormsRepository repository, Form form, User user, SeatingType type = SeatingType.NoSeating, int maxSeats = -1, bool waitlistable = false, Guid? owner = null, Guid? group = null, string permission = "770")
        {
            var node = new Seating()
            {
                UId = Guid.NewGuid(),
                SeatingType = type,
                MaxSeats = maxSeats,
                Waitlistable = waitlistable,
            };
            form.Seatings.Add(node);
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

        protected void Update()
        {
            if (Repository == null)
                return;
            Seaters.Where(s => (DateTimeOffset.UtcNow - s.Registrant.DateModified) > HoldInterval).ToList().ForEach(s => Repository.Remove(s));
            Repository.Commit();
        }


        public IPermissionHolder GetPermissionHolder()
        {
            return Form;
        }
    }

    public class Seater
        : IRegistrantItem, IRequirePermissions
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long SortingId { get; set; }
        [Key]
        public Guid UId { get; set; }

        [ForeignKey("SeatingKey")]
        public virtual Seating Seating { get; set; }
        public Guid SeatingKey { get; set; }

        [ForeignKey("ComponentKey")]
        public virtual Component Component { get; set; }
        public Guid ComponentKey { get; set; }

        [ForeignKey("RegistrantKey")]
        public virtual Registrant Registrant { get; set; }
        public Guid? RegistrantKey { get; set; }

        public bool Seated { get; set; }

        public DateTimeOffset Date { get; set; }
        public DateTimeOffset DateSeated { get; set; }

        public Seater ()
        {
        }

        public static Seater New(Seating seating, Registrant registrant, Component component, bool? seated = null)
        {
            var date = DateTimeOffset.UtcNow;
            var full = seating.AvailableSeats < 1;
            var seater = new Seater()
            {
                UId = Guid.NewGuid(),
                Registrant = registrant,
                RegistrantKey = registrant.UId,
                Seating = seating,
                SeatingKey = seating.UId,
                Component = component,
                ComponentKey = component.UId,
                Date = date,
                DateSeated = date
            };
            registrant.Seatings.Add(seater);
            seating.Seaters.Add(seater);
            if (seated == null)
            {
                if (!full)
                    seated = true;
                else
                    seated = false;
            }
            seater.Seated = seated.Value;
            return seater;
        }

        public Form GetForm()
        {
            return (Registrant != null ? Registrant.GetForm() : null);
        }

        public INode GetNode()
        {
            return (Registrant != null ? Registrant.GetNode() : null);
        }

        public IPermissionHolder GetPermissionHolder()
        {
            return Seating.Form;
        }
    }

    public enum SeatingType
    {
        [StringValue("Open Seating")]
        NoSeating = 0,
        [StringValue("Reserved Price Charged")]
        ReservedPrice = 1,
        [StringValue("Highest Price Charged")]
        HighestPrice = 2
    }

}
