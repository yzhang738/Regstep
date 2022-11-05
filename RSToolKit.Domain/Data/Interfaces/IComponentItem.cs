using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RSToolKit.Domain.Entities.Components;
using RSToolKit.Domain.Entities;

namespace RSToolKit.Domain.Data
{
    public interface IComponentItem : IComponent
    {
        Guid ParentKey { get; }
        Component Parent { get; }
        bool AgendaItem { get; set; }
        string GetVariable();
    }
}

/*


        #region IComponentItem

        private Guid _cssUId;
        private Guid _labelCssUId;
        private Guid _altTextCssUId;
        private Guid _parentUId;
        private Guid _seatingUId;
        private Guid _agendaCssUId;

        [NotMapped]
        public Seating Seating { get; set; }
        [NotMapped]
        public CSS AgendaCss { get; set; }
        [NotMapped]
        public CSS Css { get; set; }
        [NotMapped]
        public CSS LabelCss { get; set; }
        [NotMapped]
        public CSS AltTextCss { get; set; }
        [NotMapped]
        public PriceGroup Price { get; set; }
        [NotMapped]
        protected List<Logic> Logics { get; set; }
        [NotMapped]
        public virtual IComponent Parent { get; set; }

        [MaxLength(500)]
        public string Description { get; set; }
        public DateTimeOffset AgendaStart { get; set; }
        public DateTimeOffset AgendaEnd { get; set; }
        public DateTimeOffset Open { get; set; }
        public DateTimeOffset Close { get; set; }
        public RSVPType RSVP { get; set; }
        public string RawAudiences { get; set; }
        public bool AdminOnly { get; set; }
        public bool Display { get; set; }
        public bool Enabled { get; set; }
        public string AltText { get; set; }
        public string LabelText { get; set; }
        public int Line { get; set; }
        public int Order { get; set; }
        public string DisplayOrder { get; set; }
        public bool DisplayAgendaDate { get; set; }
        public bool Required { get; set; }

        public Guid CssUId
        {
            get
            {
                return Css == null ? _cssUId : Css.UId;
            }
            set
            {
                _cssUId = value;
            }
        }
        public Guid LabelCssUId
        {
            get
            {
                return LabelCss == null ? _labelCssUId : LabelCss.UId;
            }
            set
            {
                _labelCssUId = value;
            }
        }
        public Guid AltTextCssUId
        {
            get
            {
                return AltTextCss == null ? _altTextCssUId : AltTextCss.UId;
            }
            set
            {
                _altTextCssUId = value;
            }
        }
        public Guid ParentUId
        {
            get
            {
                return Parent == null ? _parentUId : Parent.UId;
            }
            set
            {
                _parentUId = value;
            }
        }
        public Guid SeatingUId
        {
            get
            {
                return Seating == null ? _seatingUId : Seating.UId;
            }
            set
            {
                _seatingUId = value;
            }
        }
        public Guid AgendaCssUId
        {
            get
            {
                return AgendaCss == null ? _agendaCssUId : AgendaCss.UId;
            }
            set
            {
                _agendaCssUId = value;
            }
        }

        public void Initiate(EFDbContext context)
        { }

        #endregion


            _cssUId = Guid.Empty;
            _labelCssUId = Guid.Empty;
            _parentUId = Guid.Empty;
            _seatingUId = Guid.Empty;
            _altTextCssUId = Guid.Empty;
            _agendaCssUId = Guid.Empty;
            Description = "";
            AgendaStart = DateTimeOffset.UtcNow;
            AgendaEnd = AgendaStart.AddMinutes(30);
            AgendaCss = new CSS() { UId = UId };
            Open = DateTimeOffset.UtcNow;
            Close = DateTimeOffset.UtcNow.AddHours(24);
            RSVP = RSVPType.None;
            RawAudiences = "";
            Css = new CSS() { UId = UId };
            AdminOnly = false;
            Display = true;
            Enabled = true;
            AltText = "";
            AltTextCss = new CSS() { UId = UId };
            LabelText = "";
            LabelCss = new CSS() { UId = UId };
            Parent = null;
            ParentUId = Guid.Empty;
            Line = -1;
            Order = -1;
            DisplayOrder = "!lbl!itm";
            DisplayAgendaDate = false;
            Seating = null;
            SeatingUId = Guid.Empty;
            Logics = new List<Logic>();
            Price = new PriceGroup();




*/