using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using RSToolKit.Domain.Data;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;
using RSToolKit.Domain.JItems;
using RSToolKit.Domain.Entities.Email;

// Complete
namespace RSToolKit.Domain.Entities
{
    public class LogicBlock : ILogicHolder, IContentItem, IFormItem
    {

        #region INode

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long SortingId { get; set; }

        [Key]
        public Guid UId { get; set; }

        [MaxLength(250)]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string Name { get; set; }

        [MaxLength(3)]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string Permission { get; set; }

        public DateTimeOffset DateCreated { get; set; }
        public DateTimeOffset DateModified { get; set; }

        public Guid Owner { get; set; }
        public Guid Group { get; set; }
        public Guid ModificationToken { get; set; }
        public Guid ModifiedBy { get; set; }

        #endregion

        [ForeignKey("FormKey")]
        public virtual Form Form { get; set; }
        public Guid? FormKey { get; set; }

        [ForeignKey("EmailCampaignKey")]
        public virtual EmailCampaign EmailCampaign { get; set; }
        public Guid? EmailCampaignKey { get; set; }

        [CascadeDelete]
        public virtual List<Logic> Logics { get; set; }

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

        public INode GetNode()
        {
            return Form as INode ?? EmailCampaign as INode;
        }

        public Form GetForm()
        {
            return Form;
        }
        
        public LogicBlock()
        {
            DateModified = DateCreated = DateTimeOffset.Now;
            Name = "New Logic";
            UId = Guid.NewGuid();
            Logics = new List<Logic>();
            JLogics = new List<JLogic>();
        }

        public LogicBlock DeepCopy(Form form)
        {
            var lb = new LogicBlock();
            form.LogicBlocks.Add(lb);
            lb.UId = Guid.NewGuid();
            lb.DateCreated = DateTimeOffset.UtcNow;
            lb.DateModified = lb.DateCreated;
            lb.Form = form;
            lb.Name = Name;
            foreach (var logic in Logics)
            {
                logic.DeepCopy(lb, form, Form);
            }
            return lb;
        }

    }
}
