using RSToolKit.Domain.Data;
using RSToolKit.Domain.Entities.Clients;
using RSToolKit.Domain.Entities;
using RSToolKit.Domain.Entities.Components;
using RSToolKit.Domain.Entities.Email;
using RSToolKit.Domain.Entities.MerchantAccount;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;


namespace RSToolKit.Domain.Entities.Components
{
    public class RatingSelect : Component, IVariableHolder
    {
        [ForeignKey("MappedComponentKey")]
        [ClearRelationship("MappedComponentKey")]
        public virtual Component MappedComponent { get; set; }
        public Guid? MappedComponentKey { get; set; }

        public int MaxRating { get; set; }
        public int MinRating { get; set; }
        public RatingSelectType RatingSelectType { get; set; }
        public RatingStep Step { get; set; }
        public string Color { get; set; }

        public RatingSelect()
            : base()
        {
            MaxRating = 5;
            MinRating = 0;
            Step = RatingStep.Half;
            RatingSelectType = RatingSelectType.Default;
            Color = "#FFE303";
        }

        public static RatingSelect New(FormsRepository repository, Panel panel, Company company, User user, Guid? owner = null, Guid? group = null, string name = null, string description = "", int row = int.MaxValue, int order = int.MaxValue, string permission = "770")
        {
            var node = new RatingSelect()
            {
                UId = Guid.NewGuid(),
                Name = name == null ? "New free text - " + DateTime.UtcNow.ToString("MM/dd/yyyy h:mm tt") + "." : name,
                Description = description,
                Row = row,
                Order = order,
                Company = company,
                CompanyKey = company.UId
            };
            node.Variable = new Variable()
            {
                Form = panel.Page.Form,
                FormKey = panel.Page.FormKey,
                Value = node.UId.ToString()
            };
            panel.Components.Add(node);
            repository.Commit();
            return node;
        }

        public override IComponent DeepCopy(Panel panel, IEnumerable<Audience> audiences = null, IEnumerable<Seating> seatings = null)
        {
            var comp = new RatingSelect();
            DeepCopyStuff(comp, panel, audiences, seatings);
            comp.MaxRating = MaxRating;
            comp.MinRating = MinRating;
            comp.Step = Step;
            comp.RatingSelectType = RatingSelectType;
            comp.Color = Color;
            return comp;
        }
    }

    public enum RatingSelectType
    {
        [StringValue("Star")]
        Default = 0,
    }

    public enum RatingStep
    {
        [StringValue("Whole Numbers")]
        [FloatValue(1f)]
        WholeNumber = 0,
        [StringValue("Half")]
        [FloatValue(0.5f)]
        Half = 1,
        [FloatValue(0.1f)]
        [StringValue("Tenths")]
        Tenths = 2
    }
}
