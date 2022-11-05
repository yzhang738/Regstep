using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using RSToolKit.Domain.Data;
using System.Text.RegularExpressions;
using RSToolKit.Domain.Entities.Email;
using System.Web.Mvc;
using RSToolKit.Domain.Entities.Clients;

// Complete
namespace RSToolKit.Domain.Entities
{
    public class CustomText
        : IContentItem, IRequirePermissions
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

        [ForeignKey("FormKey")]
        public virtual Form Form { get; set; }
        public Guid? FormKey { get; set; }

        [ForeignKey("EmailCampaignKey")]
        public virtual EmailCampaign EmailCampaign { get; set; }
        public Guid? EmailCampaignKey { get; set; }

        [MaxLength(100)]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string Variable { get; set; }

        [AllowHtml]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string Text { get; set; }

        public CustomText()
        {
            Variable = "";
            Text = "";
        }

        public static CustomText New(FormsRepository repository, Company company, User user, Form form, Guid? owner = null, Guid? group = null, string permission = "770")
        {
            var node = new CustomText()
            {
                UId = Guid.NewGuid(),
                Form = form,
                FormKey = form.UId,
            };
            form.CustomTexts.Add(node);
            repository.Commit();
            return node;
        }

        public CustomText DeepCopy(Form form = null, EmailCampaign campaign = null)
        {
            var ct = new CustomText();
            ct.Form = form;
            ct.EmailCampaign = campaign;
            ct.Variable = Variable;
            ct.Text = Text;
            ct.DateCreated = DateTimeOffset.UtcNow;
            ct.DateModified = ct.DateCreated;
            ct.Name = Name;
            ct.UId = Guid.NewGuid();
            if (ct.Form != null)
                ct.Form.CustomTexts.Add(ct);
            if (ct.EmailCampaign != null)
                ct.EmailCampaign.CustomTexts.Add(ct);
            return ct;
        }

        public IPermissionHolder GetPermissionHolder()
        {
            return Form as IPermissionHolder ?? EmailCampaign as IPermissionHolder;
        }

        public INode GetNode()
        {
            return ((Form as INode) ?? (EmailCampaign as INode));
        }

        #region Static

        public static string Render(RSEmail email, string str, EFDbContext context)
        {
            var type = "";
            if (email.FormKey.HasValue)
                type = "form";
            if (email.EmailCampaignKey.HasValue)
                type = "emailcampaign";
            List<CustomText> customTexts = null;
            switch (type)
            {
                case "form":
                    context.CustomTexts.Where(ct => ct.FormKey == email.FormKey).ToList();
                    break;
                case "emailcampaign":
                    context.CustomTexts.Where(ct => ct.FormKey == email.EmailCampaignKey).ToList();
                    break;
            }
            foreach (var c in customTexts)
            {
                var text = c.Text;
                if (HasCustomText(text, customTexts))
                    text = Render(text, customTexts);
                str = Regex.Replace(str, @"\[" + c.Name + @"\]", text, RegexOptions.IgnoreCase);
            }
            return str;
        }

        public static string Render(string str, IEnumerable<CustomText> customTexts)
        {
            foreach (var c in customTexts)
            {
                var text = c.Text;
                if (HasCustomText(text, customTexts))
                    text = Render(text, customTexts);
                str = Regex.Replace(str, @"\[" + c.Name + @"\]", text, RegexOptions.IgnoreCase);
            }
            return str;
        }

        public static bool HasCustomText(string str, IEnumerable<CustomText> CustomTexts)
        {
            var matches = Regex.Matches(str, @"\[(?<CustomText>[a-zA-Z0-9]+)\]", RegexOptions.IgnoreCase);
            foreach (Match match in matches)
            {
                if (CustomTexts.FirstOrDefault(ct => ct.Name == match.Groups["CustomText"].Value) != null)
                    return true;
            }
            return false;
        }

        #endregion
    }
}
