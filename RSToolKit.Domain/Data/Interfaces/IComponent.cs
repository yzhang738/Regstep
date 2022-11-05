using System;
using System.Collections.Generic;
using RSToolKit.Domain.Entities;
using RSToolKit.Domain.Entities.Clients;

namespace RSToolKit.Domain.Data
{
    public interface IComponent
        : INode, ILogicHolder, IFormItem, IFormComponent
    {
        #region IComponent

        List<Audience> Audiences { get; set; }

        Seating Seating { get; set; }
        Guid? SeatingKey { get; set; }
        PriceGroup PriceGroup { get; set; }
        string Description { get; set; }
        DateTimeOffset AgendaStart { get; set; }
        DateTimeOffset AgendaEnd { get; set; }
        DateTimeOffset Open { get; set; }
        DateTimeOffset Close { get; set; }
        RSVPType RSVP { get; set; }
        bool AdminOnly { get; set; }
        bool Display { get; set; }
        bool Enabled { get; set; }
        string AltText { get; set; }
        string LabelText { get; set; }
        string Name { get; set; }
        List<ComponentStyle> Styles { get; set; }
        Guid? PanelKey { get; set; }
        Panel Panel { get; set; }
        ContactHeader MappedTo { get; set; }
        Guid? MappedToKey { get; set; }
        Variable Variable { get; set; }
        Guid CompanyKey { get; set; }
        Company Company { get; set; }

        int Row { get; set; }
        int Order { get; set; }
        string DisplayOrder { get; set; }
        bool DisplayAgendaDate { get; set; }
        bool Required { get; set; }
        bool Locked { get; set; }
       
        #endregion

    }

    public interface IVariableHolder
    { }

    public interface IComponentSurveyMappable
    { }
}

/*

        #region IComponent

        private Guid _cssUId;
        private Guid _labelCssUId;
        private Guid _altTextCssUId;
        private Guid _panelUId;
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
        public Panel Panel { get; set; }
        [NotMapped]
        public PriceGroup Price { get; set; }
        [NotMapped]
        protected List<Logic> Logics { get; set; }


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
        public Guid PanelUId
        {
            get
            {
                return Panel == null ? _panelUId : Panel.UId;
            }
            set
            {
                _panelUId = value;
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

        public int Line { get; set; }
        public int Order { get; set; }
        public string DisplayOrder { get; set; }
        public bool DisplayAgendaDate { get; set; }
        public bool Required { get; set; }

        public void Initiate(EFDbContext context)
        { }

        #endregion


            _cssUId = Guid.Empty;
            _labelCssUId = Guid.Empty;
            _panelUId = Guid.Empty;
            _seatingUId = Guid.Empty;
            _altTextCssUId = Guid.Empty;
            _agendaCssUId = Guid.Empty;
            Description = "";
            AgendaStart = DateTimeOffset.UtcNow;
            AgendaEnd = AgendaStart.AddMinutes(30);
            AgendaCss = null;
            Open = DateTimeOffset.UtcNow;
            Close = DateTimeOffset.UtcNow.AddHours(24);
            RSVP = RSVPType.None;
            RawAudiences = "";
            Css = null;
            AdminOnly = false;
            Display = true;
            Enabled = true;
            AltText = "";
            AltTextCss = null;
            LabelText = "";
            LabelCss = null;
            Panel = new Panel() { UId = Guid.Empty };
            PanelUId = Guid.Empty;
            Line = -1;
            Order = -1;
            DisplayOrder = "!lbl!itm";
            DisplayAgendaDate = false;
            Seating = null;
            Logics = new List<Logic>();



*/
