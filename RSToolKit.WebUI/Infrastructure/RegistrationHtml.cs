using RSToolKit.Domain;
using RSToolKit.Domain.Data;
using RSToolKit.Domain.Entities;
using RSToolKit.Domain.Entities.Clients;
using RSToolKit.Domain.Entities.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;
using RSToolKit.Domain.Entities.MerchantAccount;
using RSToolKit.Domain.Engines;
using RSToolKit.Domain.JItems;
using RSToolKit.WebUI.Controllers;

namespace RSToolKit.WebUI.Infrastructure
{
    public class RegistrationHtml
    {
        private static readonly string ItemOpen = "<div class=\"form-component-row\">";
        private static readonly string ItemClose = "</div>";
        private static readonly Regex valueToStyle = new Regex(@"^(?<group>[^_]*)_(?<variable>.*)$");

        public Registrant Registrant { get; set; }
        public Form Form { get; set; }
        public Page Page { get; set; }
        public List<SetDataError> Errors { get; set; }
        public EFDbContext Context { get; set; }
        public RegisterController Controller { get; set; }
        public Dictionary<string, IEnumerable<JLogicCommand>> Commands { get; protected set; }

        private int Index;
        private int WaitlistIndex;

        public RegistrationHtml(Form form, Registrant registrant, Page page, EFDbContext context, RegisterController controller, List<SetDataError> errors = null)
        {
            Registrant = registrant;
            Form = form;
            Page = page;
            Errors = errors ?? new List<SetDataError>();
            Index = -1;
            WaitlistIndex = -1;
            Context = context;
            Form.Pages.Sort();
            Controller = controller;
            if (page != null)
            {
                Page.Panels.Sort();
                Page.Panels.ForEach(p => p.Components.Sort());
            }
            Form.Audiences.Sort();
            Commands = new Dictionary<string, IEnumerable<JLogicCommand>>();
        }

        public RegistrationHtml(RegisterController controller, List<SetDataError> errors = null)
        {
            Controller = controller;
            Form = controller.Form;
            Registrant = controller.Reg;
            Page = Controller.Page;
            Context = Controller.Context;
            Errors = errors ?? new List<SetDataError>();
            Index = -1;
            WaitlistIndex = -1;
            if (Form != null)
                Form.Pages.Sort();
            if (Page != null)
            {
                Page.Panels.Sort();
                Page.Panels.ForEach(p => p.Components.Sort());
            }
            if (Form != null)
                Form.Audiences.Sort();
            Commands = controller.LogicCommands;
        }

        public MvcHtmlString RenderPageNumbers()
        {
            return new MvcHtmlString(Page.PageNumber + " of " + Form.Pages.Last().PageNumber);
        }

        public MvcHtmlString RenderFormStyle()
        {
            var style = "";
            Form.FormStyles.Sort((a, b) => a.GroupName.CompareTo(b.GroupName));
            var groups = Form.FormStyles.Select(s => s.GroupName).Distinct();
            foreach (var group in groups)
            {
                style += group + "{" + Environment.NewLine;
                foreach (var fs in Form.FormStyles.Where(s => s.GroupName == group).ToList())
                {
                    if (fs.Value == null)
                        continue;
                    if (valueToStyle.IsMatch(fs.Value))
                    {
                        var match = valueToStyle.Match(fs.Value);
                        var groupTarget = match.Groups["group"].Value;
                        var variableTarget = match.Groups["variable"].Value;
                        var target = Form.FormStyles.Where(s => s.GroupName == groupTarget && s.Variable == variableTarget).FirstOrDefault();
                        if (target != null)
                            style += "\t" + fs.Variable + ": " + target.Value + ";" + Environment.NewLine;
                    }
                    else
                    {
                        style += "\t" + fs.Variable + ": " + fs.Value + ";" + Environment.NewLine;
                    }
                }
                style += "}";
            }
            return new MvcHtmlString(style);
        }

        public MvcHtmlString RenderShoppingCart()
        {
            var html = "";
            if (Registrant == null)
                return new MvcHtmlString(html);
            if (Form.DisableShoppingCart || Form.Survey)
                return new MvcHtmlString(html);
            Registrant.UpdateAccounts();
            var total = Registrant.Fees + Registrant.Taxes + Registrant.Adjustings - Registrant.Transactions;
            html = "Shopping Cart: " + total.ToString("c", Form.Culture);
            return new MvcHtmlString(html);
        }

        public MvcHtmlString RenderHiddens()
        {
            var html = @"<input type=""hidden"" name=""PageNumber"" value=""" + (Page == null ? "0" : Page.PageNumber.ToString()) + @""" />";
            if (Registrant != null)
                html += @"<input type=""hidden"" name=""RegistrantKey"" value=""" + Registrant.UId + @""" />";
            return new MvcHtmlString(html);
        }

        public bool IsBlank<T>(T item, Registrant registrant, bool admin = false)
            where T : IFormItem
        {
            if (item is Page)
                return PageIsBlank(item as Page, registrant, admin);
            else if (item is Panel)
                return PanelIsBlank(item as Panel, registrant, admin);
            else if (item is IComponent)
                return ComponentIsBlank(item as IComponent, registrant, admin);
            else
                return false;
        }

        protected bool PageIsBlank(Page page, Registrant registrant, bool admin)
        {
            if (!page.Enabled)
                return true;
            if ((Registrant.RSVP && page.RSVP == RSVPType.Decline) || (!Registrant.RSVP && page.RSVP == RSVPType.Accept))
                return true;
            if (page.Audiences.Count > 0 && (Registrant.Audience == null || !page.Audiences.Contains(Registrant.Audience)))
                return true;
            if (page.AdminOnly && !admin)
                return true;
            var commands = this._ExecuteLogic(page);
            foreach (var command in commands)
            {
                switch (command.Command)
                {
                    case JLogicWork.Hide:
                        return true;
                }
            }
            var blank = true;
            foreach (var panel in page.Panels)
                blank &= PanelIsBlank(panel, registrant, admin);
            return blank;
        }

        protected bool PanelIsBlank(Panel panel, Registrant registrant, bool admin)
        {
            if (!panel.Enabled)
                return true;
            if ((Registrant.RSVP && panel.RSVP == RSVPType.Decline) || (!Registrant.RSVP && panel.RSVP == RSVPType.Accept))
                return true;
            if (panel.Audiences.Count > 0 && (Registrant.Audience == null || !panel.Audiences.Contains(Registrant.Audience)))
                return true;
            if (panel.AdminOnly && !admin)
                return true;
            var commands = this._ExecuteLogic(panel);
            foreach (var command in commands)
            {
                switch (command.Command)
                {
                    case JLogicWork.Hide:
                        return true;
                }
            }
            var blank = true;
            foreach (var component in panel.Components)
                blank &= ComponentIsBlank(component, registrant, admin);
            return blank;
        }

        protected bool ComponentIsBlank(IComponent component, Registrant registrant, bool admin)
        {
            if ((Registrant.RSVP && component.RSVP == RSVPType.Decline) || (!Registrant.RSVP && component.RSVP == RSVPType.Accept))
                return true;
            if (component.Audiences.Count > 0 && (Registrant.Audience == null || !component.Audiences.Contains(Registrant.Audience)))
                return true;
            if (!component.Enabled)
                return true;
            if (component.AdminOnly && !admin)
                return true;
            var commands = this._ExecuteLogic(component);
            foreach (var command in commands)
            {
                switch (command.Command)
                {
                    case JLogicWork.Hide:
                        return true;
                }
            }
            if (component is IComponentItemParent)
            {
                var blank = true;
                foreach (var item in (component as IComponentItemParent).Children)
                    blank &= ComponentIsBlank(item, registrant, admin);
                return blank;
            }
            else
            {
                return false;
            }
        }

        protected bool ComponentItemIsBlank(IComponentItem component, Registrant registrant, bool admin)
        {
            if ((Registrant.RSVP && component.RSVP == RSVPType.Decline) || (!Registrant.RSVP && component.RSVP == RSVPType.Accept))
                return true;
            if (component.Audiences.Count > 0 && (Registrant.Audience == null || !component.Audiences.Contains(Registrant.Audience)))
                return true;
            if (!component.Enabled)
            {
                var dp = registrant.Data.FirstOrDefault(d => d.Component.UId == component.ParentKey);
                if (dp == null)
                    return true;
                else
                {
                    var selections = new List<Guid>();
                    if (component.Parent is IComponentMultipleSelection)
                    {
                        selections = JsonConvert.DeserializeObject<List<Guid>>(dp.Value);
                    }
                    else
                    {
                        Guid selection;
                        if (Guid.TryParse(dp.Value, out selection))
                            selections.Add(selection);
                    }
                    if (!selections.Contains(component.UId))
                        return true;
                }
                if (!component.Display)
                    return true;
            }
            if (component.AdminOnly && !admin)
                return true;
            var commands = this._ExecuteLogic(component);
            foreach (var command in commands)
            {
                switch (command.Command)
                {
                    case JLogicWork.Hide:
                        return true;
                }
            }
            return false;
        }

        #region Component Rendering

        protected ComponentSkipResult TestSkip(IComponent component, bool admin = false, List<Guid> comparer = null)
        {
            var skip = new ComponentSkipResult();
            if ((Registrant.RSVP && component.RSVP == RSVPType.Decline) || (!Registrant.RSVP && component.RSVP == RSVPType.Accept))
                skip.NullOut = true;
            if (component.Audiences.Count > 0 && (Registrant.Audience == null || !component.Audiences.Contains(Registrant.Audience)))
                skip.NullOut = true;
            var commands = this._ExecuteLogic(component);
            foreach (var command in commands)
            {
                switch (command.Command)
                {
                    case JLogicWork.Hide:
                        skip.NullOut = true;
                        break;
                }
            }
            if (!component.Enabled)
                if (comparer == null || !comparer.Contains(component.UId))
                    skip.Hide = true;
            if (component.AdminOnly && !admin)
                skip.Hide = true;
            return skip;
        }

        protected MvcHtmlString RenderErrors(string id)
        {
            var hasErrors = Errors.Count(e => e.Id == id) > 0;
            var html = "<div class=\"form-messagebox form-error-message\"" + (!hasErrors ? " style=\"display: none;\"" : "") + "><div class=\"form-messagebox-message\">";
            foreach (var error in Errors.Where(e => e.Id == id))
                html += "<div><span class=\"glyphicon glyphicon-warning\"></span>" + error.Message + "</div>";
            html += "</div></div>";
            return new MvcHtmlString(html);
        }

        public MvcHtmlString Render(IComponent component, bool admin = false, bool editing = false, bool sortingId = false)
        {
            // Check to see if we skip the component.
            var skip = TestSkip(component, admin);
            if (skip.Skip && !editing)
            {
                // We are skipping so we null out values or return nothing in the case of an item.
                if (component is FreeText)
                    return new MvcHtmlString("");
                if (component is IComponentItem)
                    return new MvcHtmlString("");
                if (skip.NullOut)
                {
                    Index++;
                    var html = @"<input type=""hidden"" name=""Components[" + Index + @"].Key"" value=""" + component.UId + @""" />";
                    html += @"<input type=""hidden"" name=""Components[" + Index + @"].Value"" value=""__skipped"" />";
                    return new MvcHtmlString(html);
                }
                return new MvcHtmlString("");
            }
            // Check for admin only.
            if (skip.Hide && !admin && !editing)
                return new MvcHtmlString(@"<input data-component-adminonly=""true"" type=""hidden"" name=""Components[" + Index + @"].Key"" value=""" + component.UId + @""" /><input type=""hidden"" name=""Components[" + Index + @"].Value"" value=""" + Registrant[component.UId] + @""" />");
            // Now we need to render the components individually.
            if (component is Input)
                return RenderInput(component as Input, admin, editing, sortingId);
            else if (component is RadioGroup)
                return RenderRadioGroup(component as RadioGroup, admin, editing, sortingId);
            else if (component is CheckboxGroup)
                return RenderCheckboxGroup(component as CheckboxGroup, admin, editing, sortingId);
            else if (component is DropdownGroup)
                return RenderDropdownGroup(component as DropdownGroup, admin, editing, sortingId);
            else if (component is RatingSelect)
                return RenderRatingSelect(component as RatingSelect, admin, editing, sortingId);
            else if (component is FreeText)
                return RenderFreeText(component as FreeText, admin, editing);
            else if (component is NumberSelector)
                return RenderNumberSelector(component as NumberSelector, admin, editing, sortingId);
            return new MvcHtmlString("");
        }

        protected MvcHtmlString RenderInput(Input component, bool admin, bool editing, bool sortingId)
        {
            var t_id = sortingId ? component.SortingId.ToString() : component.UId.ToString();
            Index++;
            var html = "<div class=\"" + (component.Type == RSToolKit.Domain.Entities.Components.InputType.Multiline ? "col-md-6" : "col-md-4 col-lg-3") + " form-component" + (component.Required && !admin ? " required-component" : "") + "\">";
            html += RenderErrors(component.UId.ToString());
            foreach (var item in component.DisplayComponentOrder.Items.OrderBy(i => i.Order))
            {
                switch (item.Item.ToLower())
                {
                    case "label":
                        var labelStyleList = component.Styles.Where(s => s.GroupName == ".form-component-label").ToList();
                        var labelStyle = "";
                        foreach (var style in labelStyleList)
                            labelStyle += style.Variable + ":" + style.Value + ";";
                        html += ItemOpen + "<label class=\"control-label\"><span class=\"form-component-label\"" + (String.IsNullOrEmpty(labelStyle) ? "" : " style=\"" + labelStyle + "\"") + ">" + component.LabelText + "</span></label>" + ItemClose;
                        break;
                    case "description":
                    case "alttext":
                        var descriptionStyleList = component.Styles.Where(s => s.GroupName == ".form-component-label").ToList();
                        var descriptionStyle = "";
                        foreach (var style in descriptionStyleList)
                            descriptionStyle += style.Variable + ":" + style.Value + ";";
                        html += ItemOpen + "<span class=\"form-component-description\"" + (String.IsNullOrEmpty(descriptionStyle) ? "" : " style=\"" + descriptionStyle + "\"") + ">" + component.AltText + "</span>" + ItemClose;
                        break;
                }
            }
            // Render the input.
            switch (component.Type)
            {
                case Domain.Entities.Components.InputType.Multiline:
                    html += "<textarea" + (component.Required && !admin ? " data-form-required=\"true\"" : "") + (component.Length.HasValue ? " data-form-max=\"" + component.Length + "\"" : "") + " id=\"" + t_id + "\" name=\"Components[" + Index + "].Value\" class=\"form-control input-sm form-text-area\">" + Registrant[component.UId] + "</textarea>";
                    break;
                case Domain.Entities.Components.InputType.Password:
                    html += "<input" + (component.Required && !admin ? " data-form-required=\"true\"" : "") + (component.Length.HasValue ? " data-form-max=\"" + component.Length + "\"" : "") + " id=\"" + t_id + "\" class=\"form-control input-sm form-text-input\" type=\"password\" name=\"Components[" + Index + "].Value\" value=\"\" />";
                    break;
                case Domain.Entities.Components.InputType.UniversalCreditCard:
                    Guid cardId;
                    var peek = "";
                    var datapoint = Registrant.Data.Where(dp => dp.VariableUId == component.UId).FirstOrDefault();
                    if (datapoint != null && Guid.TryParse(datapoint.Value, out cardId))
                    {
                        if (editing)
                        {
                            var card = Context.CreditCards.Where(c => c.UId == cardId).FirstOrDefault();
                            if (card != null)
                                peek = card.Number;
                        }
                        else
                        {
                            var card = Context.CreditCards.Where(c => c.UId == cardId).FirstOrDefault();
                            peek = card == null ? "" : card.Number.GetLast(4);
                        }
                        if (String.IsNullOrEmpty(peek))
                            peek = "";
                    }
                    html += "<input" + (component.Required && !admin ? " data-form-required=\"true\"" : "") + " id=\"" + t_id + "\" class=\"form-control input-sm form-text-input\" type=\"text\" name=\"Components[" + Index + "].Value\" value=\"" + peek + "\" />";
                    break;
                case Domain.Entities.Components.InputType.File:
                    var inputData = Registrant.Data.Where(d => d.VariableUId == component.UId).FirstOrDefault();
                    if (inputData != null && inputData.File != null)
                    {
                        if (inputData.File.FileType.ToLower().StartsWith("image"))
                        {
                            html += "<div class=\"form-file\">";
                            if (editing)
                                html += "<a href=\"..\\..\\Cloud\\RegistrantFile\\" + inputData.UId + "\">";
                            html += "<img class=\"form-file\" src=\"..\\..\\Cloud\\RegistrantImageThumbnail\\" + Registrant.UId + "?component=" + component.UId + "\" />";
                            if (editing)
                                html += "</a>";
                            html += "</div>";
                        }
                        else
                        {
                            html += "<div class=\"form-file\">";
                            if (editing)
                                html += "<a href=\"..\\..\\Cloud\\RegistrantFile\\" + inputData.UId + "\">";
                            html += "<span class=\"glyphicon glyphicon-cloud-download\"></span> Download";
                            if (editing)
                                html += "</a>";
                            html += "</div>";
                        }
                    }
                    html += "<input" + (component.Required && !admin ? " data-form-required=\"true\"" : "") + " id=\"" + t_id + "\" class=\"form-text-input\" type=\"file\" name=\"" + component.UId + "\"/></div>";
                    Index--;
                    return new MvcHtmlString(html);
                case Domain.Entities.Components.InputType.Date:
                    html += "<input" + (component.Required && !admin ? " data-form-required=\"true\"" : "") + " id=\"" + t_id + "\" name=\"Components[" + Index + "].Value\" value=\"" + Registrant[component.UId] + "\" class=\"form-control input-sm form-text-input\" type=\"text\" data-component-type=\"datetime\" data-input-type=\"date\" name=\"" + component.UId + "\"" + (component.MinDate.HasValue ? "  data-date-startDate=\"" + component.MinDate.Value.ToShortDateString() + "\"" : "") + (component.MaxDate.HasValue ? " data-date-endDate=\"" + component.MaxDate.Value.ToShortDateString() + "\"" : "") + " data-min-view=\"2\" data-date-format=\"m/d/yyyy\"/>";
                    break;
                case Domain.Entities.Components.InputType.DateTime:
                    html += "<input" + (component.Required && !admin ? " data-form-required=\"true\"" : "") + " id=\"" + t_id + "\" name=\"Components[" + Index + "].Value\" value=\"" + Registrant[component.UId] + "\" class=\"form-control input-sm form-text-input\" type=\"text\"data-component-type=\"datetime\" name=\"" + component.UId + "\" " + (component.MinDate.HasValue ? " data-date-startDate=\"" + component.MinDate.Value.ToString("M/d/yyy h/mm tt") + "\"" : "") + (component.MaxDate.HasValue ? " data-date-endDate=\"" + component.MaxDate.Value.ToString("M/d/yyy h/mm tt") + "\"" : "") + " data-date-showmeridian=\"true\" data-date-format=\"m/d/yyyy H:ii P\"/>";
                    break;
                case Domain.Entities.Components.InputType.Time:
                    html += "<input" + (component.Required && !admin ? " data-form-required=\"true\"" : "") + " id=\"" + t_id + "\" name=\"Components[" + Index + "].Value\" value=\"" + Registrant[component.UId] + "\" class=\"form-control input-sm form-text-input\" type=\"text\" data-date-pickDate=\"false\" name=\"" + component.UId + "\" data-component-type=\"datetime\" data-date-startView=\"0\" data-date-maxView=\"0\" data-date-showmeridian=\"true\" data-date-format=\"H:ii P\"/>";
                    break;
                default:
                    html += "<input" + (component.Required && !admin ? " data-form-required=\"true\"" : "") + (component.Type != Domain.Entities.Components.InputType.Default ? "" : " data-form-validation=\"" + component.ValueType.GetTagValue() + "\"") + (component.Length.HasValue ? " data-form-max=\"" + component.Length + "\"" : "") + " id=\"" + t_id + "\" class=\"form-control input-sm form-text-input\" type=\"text\" name=\"Components[" + Index + "].Value\" value=\"" + Registrant[component.UId] + "\" data-component-type=\"" + component.Type.GetTagValue() + "\" " + (component.Required && !admin ? "data-component-required=\"true\" " : "") + "data-component-regex=\"" + component.RegexPattern.GetRgxValue() + "\" />";
                    break;
            }
            html += "<input type=\"hidden\" name=\"Components[" + Index + "].Key\" value=\"" + component.UId + "\" /></div>";
            return new MvcHtmlString(html);
        }

        protected MvcHtmlString RenderRadioGroup(RadioGroup component, bool admin, bool editing, bool sortingId)
        {
            var t_id = sortingId ? component.SortingId.ToString() : component.UId.ToString();
            var data = Registrant.Data.FirstOrDefault(d => d.Component.UId == component.UId);
            var html = "<div class=\"col-xs-12 form-component" + (component.Required && !admin ? " required-component" : "") + "\" data-component-type=\"radiogroup\" data-component-index=\"" + Index + "\">";
            html += RenderErrors(component.UId.ToString());
            var price = PriceGroup.DisplayPrice(component, Registrant);
            foreach (var item in component.DisplayComponentOrder.Items.OrderBy(i => i.Order))
            {
                switch (item.Item.ToLower())
                {
                    case "label":
                        var labelStyleList = component.Styles.Where(s => s.GroupName == ".form-component-label").ToList();
                        var labelStyle = "";
                        foreach (var style in labelStyleList)
                            labelStyle += style.Variable + ":" + style.Value + ";";
                        html += ItemOpen + "<label class=\"control-label\"><span class=\"form-component-label\"" + (String.IsNullOrEmpty(labelStyle) ? "" : " style=\"" + labelStyle + "\"") + ">" + component.LabelText + "</span></label>" + ItemClose;
                        break;
                    case "description":
                    case "alttext":
                        var descriptionStyleList = component.Styles.Where(s => s.GroupName == ".form-component-label").ToList();
                        var descriptionStyle = "";
                        foreach (var style in descriptionStyleList)
                            descriptionStyle += style.Variable + ":" + style.Value + ";";
                        html += ItemOpen + "<span class=\"form-component-description\"" + (String.IsNullOrEmpty(descriptionStyle) ? "" : " style=\"" + descriptionStyle + "\"") + ">" + component.AltText + "</span>" + ItemClose;
                        break;
                    case "date":
                        var displayDate = "";
                        var multipleDays = component.AgendaEnd.Date != component.AgendaStart.Date;
                        var showDate = component.DisplayAgendaDate | multipleDays;
                        if (showDate)
                            displayDate = component.AgendaStart.ToString(Form.Culture) + " - " + component.AgendaEnd.ToString(Form.Culture);
                        else
                            displayDate = component.AgendaStart.DateTime.ToShortTimeString() + " - " + component.AgendaEnd.DateTime.ToShortTimeString();
                        html += ItemOpen + "<span class=\"form-agenda\">" + displayDate + "</span>" + ItemClose;
                        break;
                    case "price":
                        if (!price.HasValue)
                            break;
                        html += "<div class=\"form-item-row\"><span class=\"form-price\">" + price.Value.ToString("c", Form.Culture) + "</span></div>";
                        break;
                }
            }
            var itemHtml = "";
            foreach (var item in component.Items.OrderBy(i => i.Order))
            {
                itemHtml += RenderRadioItem(item, data, admin, editing);
            }
            if (String.IsNullOrEmpty(itemHtml))
            {
                var skipHtml = @"<input type=""hidden"" name=""Components[" + Index + @"].Key"" value=""" + component.UId + @""" />";
                skipHtml += @"<input type=""hidden"" name=""Components[" + Index + @"].Value"" value=""__skipped"" />";
                return new MvcHtmlString(skipHtml);
            }
            html += itemHtml;
            Index++;
            html += "<input type=\"hidden\" name=\"Components[" + Index + "].Key\" value=\"" + component.UId + "\" />";
            html += "<input" + (component.Required && !admin ? " data-form-required=\"true\"" : "") + " data-component-type=\"radiogroup\" id=\"" + t_id + "\" type=\"hidden\" name=\"Components[" + Index + "].Value\" value=\"" + Registrant.GetRawValue(component.UId.ToString()) + "\"><div class=\"form-items\">";
            component.Items.Sort((a, b) => a.Order.CompareTo(b.Order));
            html += "</div></div>";

            return new MvcHtmlString(html);
        }

        protected MvcHtmlString RenderRadioItem(RadioItem item, RegistrantData data, bool admin, bool editing)
        {
            Guid selected = Guid.Empty;
            if (data != null)
                Guid.TryParse(data.Value, out selected);
            var skip = TestSkip(item, admin, new List<Guid>() { selected });
            if (skip.NullOut)
                if (selected == item.UId)
                    data.Value = null;
            if (skip.Skip && !editing)
                return new MvcHtmlString("");
            var disabled = false;
            if (skip.Hide && !admin)
            {
                if (!item.Display)
                    return new MvcHtmlString("");
                disabled = true;
            }
            var seating = item.Seating;
            var seated = true;
            var waitlisted = false;
            var full = false;
            if (seating != null)
            {
                // There is seating so we grab the seat for the user if available.
                var seater = seating.Seaters.Where(s => s.RegistrantKey == Registrant.UId && s.ComponentKey == item.UId).FirstOrDefault();
                full = seating.AvailableSeats < 1;
                seated = seater != null && seater.Seated;
                waitlisted = seater != null && !seater.Seated;
            }
            if (full && !seated && !admin)
                disabled = true;
            var html = "";
            html += "<div class=\"form-item" + (item.Required ? " required-item" : "") + "\"><div class=\"form-radio radio\">";
            if (disabled)
                html += "<fieldset disabled>";
            html += "<label>";
            html += RenderErrors(item.UId.ToString());
            var price = PriceGroup.DisplayPrice(item, Registrant);
            // We need to check seating.
            if (disabled)
            {
                html += "<input type=\"radio\" disabled=\"true\" name=\"radio" + Index + "\" value=\"" + item.UId + "\" data-parent=\"" + item.RadioGroupKey + "\" />";
                if (full)
                    html += "<div class=\"form-at-capacity\">" + seating.FullLabel + "</div>";
            }
            else
            {
                html += "<input id=\"" + item.UId + "\" class=\"input-outside\" type=\"radio\" name=\"radio" + Index + "\" value=\"" + item.UId + "\"" + (selected == item.UId ? " checked=\"checked\"" : "") + " data-parent=\"" + item.RadioGroupKey + "\" />";
            }
            foreach (var displayItem in item.DisplayComponentOrder.Items.OrderBy(i => i.Order))
            {
                switch (displayItem.Item.ToLower())
                {
                    case "label":
                        var labelStyleList = item.Styles.Where(s => s.GroupName == ".form-component-label").ToList();
                        var labelStyle = "";
                        foreach (var style in labelStyleList)
                            labelStyle += style.Variable + ":" + style.Value + ";";
                        html += "<div class=\"form-item-row\"><span class=\"form-item-label\"" + (String.IsNullOrEmpty(labelStyle) ? "" : " style=\"" + labelStyle + "\"") + ">" + item.LabelText + "</span></div>";
                        break;
                    case "description":
                    case "alttext":
                        var descriptionStyleList = item.Styles.Where(s => s.GroupName == ".form-component-label").ToList();
                        var descriptionStyle = "";
                        foreach (var style in descriptionStyleList)
                            descriptionStyle += style.Variable + ":" + style.Value + ";";
                        html += "<div class=\"form-item-row\"><span class=\"form-item-description\"" + (String.IsNullOrEmpty(descriptionStyle) ? "" : " style=\"" + descriptionStyle + "\"") + ">" + item.AltText + "</span></div>";
                        break;
                    case "date":
                        var displayDate = "";
                        var multipleDays = item.AgendaEnd.Date != item.AgendaStart.Date;
                        var showDate = item.DisplayAgendaDate | multipleDays;
                        if (showDate)
                            displayDate = item.AgendaStart.ToString(Form.Culture) + " - " + item.AgendaEnd.ToString(Form.Culture);
                        else
                            displayDate = item.AgendaStart.DateTime.ToShortTimeString() + " - " + item.AgendaEnd.DateTime.ToShortTimeString();
                        html += "<div class=\"form-item-row\"><span class=\"form-agenda\">" + displayDate + "</span></div>";
                        break;
                    case "price":
                        if (!price.HasValue)
                            break;
                        html += "<div class=\"form-item-row\"><span class=\"form-price\">" + price.Value.ToString("c", Form.Culture) + "</span></div>";
                        break;
                }
            }
            html += "</label>";
            if (disabled)
                html += "</fieldset>";
            if (full && seating.Waitlistable && !seated)
            {
                WaitlistIndex++;
                html += "<label><input type=\"hidden\" name=\"Waitlistings[" + WaitlistIndex + "].Key\" value=\"" + item.UId + "\" /><input type=\"checkbox\"" + (waitlisted ? " checked=\"true\"" : "") + " name=\"Waitlistings[" + WaitlistIndex + "].Value\" value=\"true\"><input type=\"hidden\" name=\"Waitlistings[" + WaitlistIndex + "].Value\" value=\"false\" /> " + seating.WaitlistLabel + "</label>";
            }
            html += "</div></div>";
            return new MvcHtmlString(html);
        }

        protected MvcHtmlString RenderCheckboxGroup(CheckboxGroup component, bool admin, bool editing, bool sortingId)
        {
            var t_id = sortingId ? component.SortingId.ToString() : component.UId.ToString();
            var data = Registrant.Data.FirstOrDefault(d => d.Component.UId == component.UId);
            if (data == null)
                data = new RegistrantData();
            data.Component = component;
            var html = "";
            html += "<div class=\"col-xs-12 form-component" + (component.Required && !admin ? " required-component" : "") + "\" data-component-type=\"checkbox\" data-component-index=\"" + Index + "\" data-component-timeexclusion=\"" + component.TimeExclusion + "\">";
            html += RenderErrors(component.UId.ToString());
            var price = PriceGroup.DisplayPrice(component, Registrant);
            foreach (var item in component.DisplayComponentOrder.Items.OrderBy(i => i.Order))
            {
                switch (item.Item.ToLower())
                {
                    case "label":
                        var labelStyleList = component.Styles.Where(s => s.GroupName == ".form-component-label").ToList();
                        var labelStyle = "";
                        foreach (var style in labelStyleList)
                            labelStyle += style.Variable + ":" + style.Value + ";";
                        html += ItemOpen + "<label class=\"control-label\"><span class=\"form-component-label\"" + (String.IsNullOrEmpty(labelStyle) ? "" : " style=\"" + labelStyle + "\"") + ">" + component.LabelText + "</span></label>" + ItemClose;
                        break;
                    case "description":
                    case "alttext":
                        var descriptionStyleList = component.Styles.Where(s => s.GroupName == ".form-component-label").ToList();
                        var descriptionStyle = "";
                        foreach (var style in descriptionStyleList)
                            descriptionStyle += style.Variable + ":" + style.Value + ";";
                        html += ItemOpen + "<span class=\"form-component-description\"" + (String.IsNullOrEmpty(descriptionStyle) ? "" : " style=\"" + descriptionStyle + "\"") + ">" + component.AltText + "</span>" + ItemClose;
                        break;
                    case "date":
                        var displayDate = "";
                        var multipleDays = component.AgendaEnd.Date != component.AgendaStart.Date;
                        var showDate = component.DisplayAgendaDate | multipleDays;
                        if (showDate)
                            displayDate = component.AgendaStart.ToString(Form.Culture) + " - " + component.AgendaEnd.ToString(Form.Culture);
                        else
                            displayDate = component.AgendaStart.DateTime.ToShortTimeString() + " - " + component.AgendaEnd.DateTime.ToShortTimeString();
                        html += ItemOpen + "<span class=\"form-agenda\">" + displayDate + "</span>" + ItemClose;
                        break;
                    case "price":
                        if (!price.HasValue)
                            break;
                        html += "<div class=\"form-item-row\"><span class=\"form-price\">" + price.Value.ToString("c", Form.Culture) + "</span></div>";
                        break;
                }
            }
            var itemHtml = "";
            foreach (var item in component.Items.OrderBy(i => i.Order))
            {
                itemHtml += RenderCheckboxItem(item, data, admin, editing);
            }
            if (String.IsNullOrEmpty(itemHtml))
            {
                var skipHtml = @"<input type=""hidden"" name=""Components[" + Index + @"].Key"" value=""" + t_id + @""" />";
                skipHtml += @"<input type=""hidden"" name=""Components[" + Index + @"].Value"" value=""__skipped"" />";
                return new MvcHtmlString(skipHtml);
            }
            html += itemHtml;
            Index++;
            html += "<input type=\"hidden\" name=\"Components[" + Index + "].Key\" value=\"" + component.UId + "\" /><input " + (component.Required && !admin ? " data-form-required=\"true\"" : "") + " data-component-timeexclusion-message=\"" + component.DialogText + "\" data-component-type=\"checkboxgroup\" data-component-timeexclusion=\"" + component.TimeExclusion + "\" id=\"" + component.UId + "\" type=\"hidden\" name=\"Components[" + Index + "].Value\" value='" + data.Value + "'><div class=\"form-items\">";
            html += "</div></div>";
            return new MvcHtmlString(html);
        }

        protected MvcHtmlString RenderCheckboxItem(CheckboxItem item, RegistrantData data, bool admin, bool editing)
        {
            var selected = false;
            var selections = new List<Guid>();
            if (data != null)
                selections = JsonConvert.DeserializeObject<List<Guid>>(data.Value);
            var skip = TestSkip(item, admin, selections);
            if (skip.NullOut)
            {
                selections.Remove(item.UId);
                data.Value = JsonConvert.SerializeObject(selections);
            }
            if (skip.Skip && !editing)
                return new MvcHtmlString("");
            var disabled = false;
            if (skip.Hide && !admin)
            {
                if (!item.Display)
                    return new MvcHtmlString("");
                disabled = true;
            }
            selected = selections.Contains(item.UId);
            var seating = item.Seating;
            var seated = true;
            var waitlisted = false;
            var full = false;
            if (seating != null)
            {
                // There is seating so we grab the seat for the user if available.
                var seater = seating.Seaters.Where(s => s.RegistrantKey == Registrant.UId && s.ComponentKey == item.UId).FirstOrDefault();
                full = seating.AvailableSeats < 1;
                seated = seater != null && seater.Seated;
                waitlisted = seater != null && !seater.Seated;
            }
            if (full && !seated && !admin)
                disabled = true;
            var html = "";
            html += "<div class=\"form-item" + (item.Required && !admin ? " required-item" : "") + "\"><div class=\"form-checkbox checkbox\">";
            if (disabled)
                html += "<fieldset disabled>";
            html += "<label>";
            html += RenderErrors(item.UId.ToString());
            var price = PriceGroup.DisplayPrice(item, Registrant);
            if (disabled)
            {
                html += "<input" + (item.Required && !admin ? " data-form-required=\"true\"" : "") + " type=\"checkbox\" disabled=\"true\" " + (item.CheckboxGroup.TimeExclusion ? "data-agenda-start=\"" + item.AgendaStart.ToString() + "\" data-agenda-end=\"" + item.AgendaEnd.ToString() + "\"" : "") + " data-parent=\"" + item.ParentKey.ToString() + "\" value=\"" + item.UId + "\" data-component-required=\"" + (item.Required ? "true" : "false") + "\" />";
                if (full)
                    html += "<div class=\"form-at-capacity\">" + seating.FullLabel + "</div>";
            }
            else
            {
                html += "<input id=\"" + item.UId + "\"" + (selected ? " checked=\"checked\"" : "") + " type=\"checkbox\"" + (item.CheckboxGroup.TimeExclusion ? " data-agenda-start=\"" + item.AgendaStart.ToString() + "\" data-agenda-end=\"" + item.AgendaEnd.ToString() + "\"" : "") + " data-parent=\"" + item.ParentKey.ToString() + "\" value=\"" + item.UId + "\" data-component-required=\"" + (item.Required ? "true" : "false") + "\" />";
            }
            foreach (var itemDisplay in item.DisplayComponentOrder.Items.OrderBy(i => i.Order))
            {
                switch (itemDisplay.Item.ToLower())
                {
                    case "label":
                        var labelStyleList = item.Styles.Where(s => s.GroupName == ".form-component-label").ToList();
                        var labelStyle = "";
                        foreach (var style in labelStyleList)
                            labelStyle += style.Variable + ":" + style.Value + ";";
                        html += "<div class=\"form-item-row\"><span class=\"form-item-label\"" + (String.IsNullOrEmpty(labelStyle) ? "" : " style=\"" + labelStyle + "\"") + ">" + item.LabelText + "</span></div>";
                        break;
                    case "description":
                    case "alttext":
                        var descriptionStyleList = item.Styles.Where(s => s.GroupName == ".form-component-label").ToList();
                        var descriptionStyle = "";
                        foreach (var style in descriptionStyleList)
                            descriptionStyle += style.Variable + ":" + style.Value + ";";
                        html += "<div class=\"form-item-row\"><span class=\"form-item-description\"" + (String.IsNullOrEmpty(descriptionStyle) ? "" : " style=\"" + descriptionStyle + "\"") + ">" + item.AltText + "</span></div>";
                        break;
                    case "date":
                        var displayDate = "";
                        var multipleDays = item.AgendaEnd.Date != item.AgendaStart.Date;
                        var showDate = item.DisplayAgendaDate | multipleDays;
                        if (showDate)
                            displayDate = item.AgendaStart.ToString(Form.Culture) + " - " + item.AgendaEnd.ToString(Form.Culture);
                        else
                            displayDate = item.AgendaStart.DateTime.ToShortTimeString() + " - " + item.AgendaEnd.DateTime.ToShortTimeString();
                        html += "<div class=\"form-item-row\"><span class=\"form-agenda\">" + displayDate + "</span></div>";
                        break;
                    case "price":
                        if (!price.HasValue)
                            break;
                        html += "<div class=\"form-item-row\"><span class=\"form-price\">" + price.Value.ToString("c", Form.Culture) + "</span></div>";
                        break;
                }
            }
            html += "</label>";
            if (disabled)
                html += "</fieldset>";
            if (full && seating.Waitlistable && !seated)
            {
                WaitlistIndex++;
                html += "<label class=\"waitlist\"><input type=\"hidden\" name=\"Waitlistings[" + WaitlistIndex + "].Key\" value=\"" + item.UId + "\" /><input type=\"checkbox\"" + (waitlisted ? " checked=\"checked\"" : "") + " name=\"Waitlistings[" + WaitlistIndex + "].Value\" value=\"true\"><input type=\"hidden\" name=\"Waitlistings[" + WaitlistIndex + "].Value\" value=\"false\" /> " + seating.WaitlistLabel + "</label>";
            }
            html += "</div></div>";
            return new MvcHtmlString(html);
        }

        protected MvcHtmlString RenderDropdownGroup(DropdownGroup component, bool admin, bool editing, bool sortingId)
        {
            var t_id = sortingId ? component.SortingId.ToString() : component.UId.ToString();
            var data = Registrant.Data.FirstOrDefault(d => d.Component.UId == component.UId);
            var html = "";
            html += "<div class=\"col-md-4 col-lg-3 form-component" + (component.Required && !admin ? " required-component" : "") + "\" data-component-type=\"dropdown\" data-component-index=\"" + Index + "\">";
            html += RenderErrors(component.UId.ToString());
            var price = PriceGroup.DisplayPrice(component, Registrant);
            foreach (var item in component.DisplayComponentOrder.Items.OrderBy(i => i.Order))
            {
                switch (item.Item.ToLower())
                {
                    case "label":
                        var labelStyleList = component.Styles.Where(s => s.GroupName == ".form-component-label").ToList();
                        var labelStyle = "";
                        foreach (var style in labelStyleList)
                            labelStyle += style.Variable + ":" + style.Value + ";";
                        html += ItemOpen + "<label class=\"control-label\"><span class=\"form-component-label\"" + (String.IsNullOrEmpty(labelStyle) ? "" : " style=\"" + labelStyle + "\"") + ">" + component.LabelText + "</span></label>" + ItemClose;
                        break;
                    case "description":
                    case "alttext":
                        var descriptionStyleList = component.Styles.Where(s => s.GroupName == ".form-component-label").ToList();
                        var descriptionStyle = "";
                        foreach (var style in descriptionStyleList)
                            descriptionStyle += style.Variable + ":" + style.Value + ";";
                        html += ItemOpen + "<span class=\"form-component-description\"" + (String.IsNullOrEmpty(descriptionStyle) ? "" : " style=\"" + descriptionStyle + "\"") + ">" + component.AltText + "</span>" + ItemClose;
                        break;
                    case "date":
                        var displayDate = "";
                        var multipleDays = component.AgendaEnd.Date != component.AgendaStart.Date;
                        var showDate = component.DisplayAgendaDate | multipleDays;
                        if (showDate)
                            displayDate = component.AgendaStart.ToString(Form.Culture) + " - " + component.AgendaEnd.ToString(Form.Culture);
                        else
                            displayDate = component.AgendaStart.DateTime.ToShortTimeString() + " - " + component.AgendaEnd.DateTime.ToShortTimeString();
                        html += ItemOpen + "<span class=\"form-agenda\">" + displayDate + "</span>" + ItemClose;
                        break;
                    case "price":
                        if (!price.HasValue)
                            break;
                        html += "<div class=\"form-item-row\"><span class=\"form-price\">" + price.Value.ToString("c", Form.Culture) + "</span></div>";
                        break;
                }
            }
            var itemHtml = "";
            foreach (var item in component.Items.OrderBy(i => i.Order))
                itemHtml += RenderDropdownItem(item, data, admin, editing);
            if (String.IsNullOrEmpty(itemHtml))
            {
                var skipHtml = @"<input type=""hidden"" name=""Components[" + Index + @"].Key"" value=""" + t_id + @""" />";
                skipHtml += @"<input type=""hidden"" name=""Components[" + Index + @"].Value"" value=""__skipped"" />";
                return new MvcHtmlString(skipHtml);
            }
            Index++;
            html += "<input type=\"hidden\" name=\"Components[" + Index + "].Key\" value=\"" + component.UId + "\" /><div class=\"form-items\"><select" + (component.Required && !admin ? " data-form-required=\"true\"" : "") + " id=\"" + component.UId.ToString() + "\" name=\"Components[" + Index + "].Value\" class=\"form-control input-sm form-select\">";
            html += "<option value=\"\"></option>";
            html += itemHtml;
            html += "</select></div></div>";

            return new MvcHtmlString(html);
        }

        protected MvcHtmlString RenderDropdownItem(DropdownItem item, RegistrantData data, bool admin, bool editing)
        {
            var seating = item.Seating;
            var full = false;
            Guid selected = Guid.Empty;
            if (data != null)
                Guid.TryParse(data.Value, out selected);
            var skip = TestSkip(item, admin, new List<Guid>() { selected });
            if (skip.NullOut)
                if (selected == item.UId)
                    data.Value = null;
            if (skip.Skip && !editing)
                return new MvcHtmlString("");
            if (seating != null && seating.AvailableSeats < 1 && !admin)
                return new MvcHtmlString("");
            if (skip.Hide && !admin)
                return new MvcHtmlString("");
            return new MvcHtmlString("<option value=\"" + item.UId + "\"" + (selected == item.UId ? " selected=true" : "") + (full ? " disabled=\"true\"" : "") + ">" + item.LabelText + (full ? " FULL" : "") + "</option>");
        }

        protected MvcHtmlString RenderRatingSelect(RatingSelect component, bool admin, bool editing, bool sortingId)
        {
            var t_id = sortingId ? component.SortingId.ToString() : component.UId.ToString();
            var html = "";
            if (Form.ParentForm == null || component.MappedComponent == null)
            {
                var dataPoint = Registrant.Data.FirstOrDefault(d => d.VariableUId == component.UId);
                html += "<div class=\"col-sm-6 col-md-4 col-lg-3 form-component\" data-component-index=\"" + Index + "\">";
                foreach (var item in component.DisplayComponentOrder.Items.OrderBy(i => i.Order))
                {
                    switch (item.Item.ToLower())
                    {
                        case "label":
                            var labelStyleList = component.Styles.Where(s => s.GroupName == ".form-component-label").ToList();
                            var labelStyle = "";
                            foreach (var style in labelStyleList)
                                labelStyle += style.Variable + ":" + style.Value + ";";
                            html += ItemOpen + "<label class=\"control-label\"><span class=\"form-component-label\"" + (String.IsNullOrEmpty(labelStyle) ? "" : " style=\"" + labelStyle + "\"") + ">" + component.LabelText + "</span></label>" + ItemClose;
                            break;
                        case "description":
                        case "alttext":
                            var descriptionStyleList = component.Styles.Where(s => s.GroupName == ".form-component-label").ToList();
                            var descriptionStyle = "";
                            foreach (var style in descriptionStyleList)
                                descriptionStyle += style.Variable + ":" + style.Value + ";";
                            html += ItemOpen + "<span class=\"form-component-description\"" + (String.IsNullOrEmpty(descriptionStyle) ? "" : " style=\"" + descriptionStyle + "\"") + ">" + component.AltText + "</span>" + ItemClose;
                            break;
                    }
                }
                Index++;
                html += "<input class=\"rating form-control input-sm form-text-input\" type=\"password\" id=\"" + t_id + "\" name=\"Components[" + Index + "].Value\" value=\"" + (dataPoint != null ? dataPoint.Value : "") + "\" />";
                html += "<input type=\"hidden\" name=\"Components[" + Index + "].Key\" value=\"" + component.UId + "\" /></div>";
                html += "</div>";
            }
            else
            {
                var formRegistrant = Form.ParentForm.Registrants.Where(r => r.Type == RegistrationType.Live && r.Status == RegistrationStatus.Submitted && r.Email.ToLower() == Registrant.Email.ToLower()).FirstOrDefault();
                if (formRegistrant != null)
                {
                    var selections = new List<Guid>();
                    var t_dataPoint = formRegistrant.SearchData(component.MappedComponentKey.ToString());
                    if (t_dataPoint != null && !t_dataPoint.Empty())
                    {
                        if (component.MappedComponent is IComponentMultipleSelection)
                        {
                            selections = JsonConvert.DeserializeObject<List<Guid>>(t_dataPoint.Value);
                        }
                        else if (component.MappedComponent is IComponentItemParent)
                        {
                            Guid tt_id;
                            if (Guid.TryParse(t_dataPoint.Value, out tt_id))
                            {
                                selections.Add(tt_id);
                            }
                        }
                    }
                    var mappedComponent = component.MappedComponent;
                    foreach (var selection in selections)
                    {
                        var dataPoint = Registrant.Data.FirstOrDefault(d => d.VariableUId == selection);
                        var t_item = (mappedComponent as IComponentItemParent).Children.FirstOrDefault(i => i.UId == selection);
                        if (t_item != null)
                        {
                            Index++;
                            html += "<div class=\"col-sm-6 col-md-4 col-lg-3 form-component\" data-component-index=\"" + Index + "\">";
                            foreach (var item in component.DisplayComponentOrder.Items.OrderBy(i => i.Order))
                            {
                                switch (item.Item.ToLower())
                                {
                                    case "label":
                                        var labelStyleList = component.Styles.Where(s => s.GroupName == ".form-component-label").ToList();
                                        var labelStyle = "";
                                        foreach (var style in labelStyleList)
                                            labelStyle += style.Variable + ":" + style.Value + ";";
                                        html += ItemOpen + "<label class=\"control-label\"><span class=\"form-component-label\"" + (String.IsNullOrEmpty(labelStyle) ? "" : " style=\"" + labelStyle + "\"") + ">" + mappedComponent.LabelText + ": " + t_item.LabelText + "</span></label>" + ItemClose;
                                        break;
                                    case "description":
                                    case "alttext":
                                        var descriptionStyleList = component.Styles.Where(s => s.GroupName == ".form-component-label").ToList();
                                        var descriptionStyle = "";
                                        foreach (var style in descriptionStyleList)
                                            descriptionStyle += style.Variable + ":" + style.Value + ";";
                                        html += ItemOpen + "<span class=\"form-component-description\"" + (String.IsNullOrEmpty(descriptionStyle) ? "" : " style=\"" + descriptionStyle + "\"") + ">" + t_item.AltText + "</span>" + ItemClose;
                                        break;
                                }
                            }
                            html += "<input id=\"" + t_id + "\" class=\"rating form-control input-sm form-text-input\" type=\"text\" data-showClear=\"false\" data-showCaption=\"false\" data-size=\"sm\" name=\"Components[" + Index + "].Value\" value=\"" + (dataPoint != null ? dataPoint.Value : "") + "\" />";
                            html += "<input type=\"hidden\" name=\"Components[" + Index + "].Key\" value=\"" + t_item.UId + "\" />";
                            html += "</div>";
                        }
                    }
                }
            }
            return new MvcHtmlString(html);
        }

        protected MvcHtmlString RenderFreeText(FreeText component, bool admin, bool editing)
        {
            var html = "";
            html = "<div class=\"col-xs-12 form-component\">" + this._Parse(component.Html) + "</div>";
            return new MvcHtmlString(html);
        }

        protected MvcHtmlString RenderNumberSelector(NumberSelector component, bool admin, bool editing, bool sortingId)
        {
            var t_id = sortingId ? component.SortingId.ToString() : component.UId.ToString();
            var data = Registrant.Data.FirstOrDefault(d => d.Component.UId == component.UId);
            var html = "";
            html += "<div class=\"col-sm-6 col-md-4 col-lg-3 form-component" + (component.Required && !admin ? " required-component" : "") + "\" data-component-type=\"dropdown\" data-component-index=\"" + Index + "\">";
            html += RenderErrors(component.UId.ToString());
            var price = PriceGroup.DisplayPrice(component, Registrant);
            foreach (var item in component.DisplayComponentOrder.Items.OrderBy(i => i.Order))
            {
                switch (item.Item.ToLower())
                {
                    case "label":
                        var labelStyleList = component.Styles.Where(s => s.GroupName == ".form-component-label").ToList();
                        var labelStyle = "";
                        foreach (var style in labelStyleList)
                            labelStyle += style.Variable + ":" + style.Value + ";";
                        html += ItemOpen + "<label class=\"control-label\"><span class=\"form-component-label\"" + (String.IsNullOrEmpty(labelStyle) ? "" : " style=\"" + labelStyle + "\"") + ">" + component.LabelText + "</span></label>" + ItemClose;
                        break;
                    case "description":
                    case "alttext":
                        var descriptionStyleList = component.Styles.Where(s => s.GroupName == ".form-component-label").ToList();
                        var descriptionStyle = "";
                        foreach (var style in descriptionStyleList)
                            descriptionStyle += style.Variable + ":" + style.Value + ";";
                        html += ItemOpen + "<span class=\"form-component-description\"" + (String.IsNullOrEmpty(descriptionStyle) ? "" : " style=\"" + descriptionStyle + "\"") + ">" + component.AltText + "</span>" + ItemClose;
                        break;
                    case "date":
                        var displayDate = "";
                        var multipleDays = component.AgendaEnd.Date != component.AgendaStart.Date;
                        var showDate = component.DisplayAgendaDate | multipleDays;
                        if (showDate)
                            displayDate = component.AgendaStart.ToString(Form.Culture) + " - " + component.AgendaEnd.ToString(Form.Culture);
                        else
                            displayDate = component.AgendaStart.DateTime.ToShortTimeString() + " - " + component.AgendaEnd.DateTime.ToShortTimeString();
                        html += ItemOpen + "<span class=\"form-agenda\">" + displayDate + "</span>" + ItemClose;
                        break;
                    case "price":
                        if (!price.HasValue)
                            break;
                        html += "<div class=\"form-item-row\"><span class=\"form-price\">" + price.Value.ToString("c", Form.Culture) + "</span></div>";
                        break;
                }
            }
            var itemHtml = "";
            var t_value = (data != null ? data.Value : "0");
            for (var i = component.Min; i <= component.Max; i++)
                itemHtml += "<option value=" + i + (t_value == i.ToString() ? " selected=\"selected\"" : "") + ">" + i + "</option>";
            if (String.IsNullOrEmpty(itemHtml))
            {
                var skipHtml = @"<input type=""hidden"" name=""Components[" + Index + @"].Key"" value=""" + component.UId + @""" />";
                skipHtml += @"<input type=""hidden"" name=""Components[" + Index + @"].Value"" value=""__skipped"" />";
                return new MvcHtmlString(skipHtml);
            }
            Index++;
            html += "<input type=\"hidden\" name=\"Components[" + Index + "].Key\" value=\"" + component.UId + "\" /><div class=\"form-items\"><select" + (component.Required && !admin ? " data-form-required=\"true\"" : "") + " id=\"" + t_id + "\" name=\"Components[" + Index + "].Value\" class=\"form-control input-sm form-select\">";
            html += "<option value=\"\"></option>";
            html += itemHtml;
            html += "</select></div></div>";

            return new MvcHtmlString(html);
        }


        #endregion

        public MvcHtmlString RenderFooter()
        {
            return new MvcHtmlString(this._Parse(Form.Footer));
        }

        public MvcHtmlString RenderHeader()
        {
            return new MvcHtmlString(this._Parse(Form.Header));
        }

        public MvcHtmlString RenderRsvp(bool first = false)
        {
            var html = "<div class=\"row form-btn-row\"><div class=\"col-xs-12\"><div class=\"row\"><input type=\"hidden\" name=\"RSVP\" id=\"RSVP\" value=\"\" />";
            html += "<div class=\"col-md-4 col-lg-3 form-btn\"><a href=\"#\" class=\"btn-rs btn-rsvp" + (Registrant.RSVP == true && !first ? " btn-selected" : "") + "\" id=\"rsvpAccept\">" + Form.RSVPAccept + @"</a></div>";
            html += "<div class=\"col-md-4 col-lg-3 form-btn\"><a href=\"#\" class=\"btn-rs btn-rsvp" + (Registrant.RSVP == false && !first ? " btn-selected" : "") + "\" id=\"rsvpDecline\">" + Form.RSVPDecline + @"</a></div>";
            html += "</div></div></div>";
            return new MvcHtmlString(this._Parse(html));
        }

        public MvcHtmlString RenderAudiences()
        {
            var html = "<div class=\"row form-btn-panel\"><div class=\"col-xs-12\"><div class=\"row\">";
            foreach (var audience in Form.Audiences.OrderBy(a => a.Order))
            {
                html += "<div class=\"col-md-4 col-lg-3 form-btn\"><a href=\"#\" class=\"btn-rs btn-audience" + (Registrant.AudienceKey.HasValue ? (Registrant.AudienceKey == audience.UId ? " btn-selected" : "") : "") + "\" data-value=\"" + audience.UId + "\">" + audience.Label + @"</a></div>";
            }
            html += "<input type=\"hidden\" name=\"Audience\" id=\"Audience\" value=\"" + Registrant.AudienceKey + "\"/></div></div></div>";
            return new MvcHtmlString(this._Parse(html));
        }

        public MvcHtmlString RenderPanel(Panel panel, bool admin = false, bool editing = false)
        {
            var skipped = false;
            if (!panel.Enabled)
                return new MvcHtmlString("");
            if (panel.AdminOnly && !admin)
                return new MvcHtmlString("");
            if ((panel.RSVP == RSVPType.Accept && !Registrant.RSVP) || (panel.RSVP == RSVPType.Decline && Registrant.RSVP))
                skipped = true;
            else if (panel.Audiences.Count > 0 && (Registrant.Audience != null && !panel.Audiences.Contains(Registrant.Audience)))
                skipped = true;
            var commands = this._ExecuteLogic(panel);
            //var commands = panel.RunLogic(Registrant, true, Repository);
            foreach (var command in commands)
            {
                switch (command.Command)
                {
                    case JLogicWork.Hide:
                        skipped = true;
                        break;
                }
            }
            if (skipped)
            {
                var html = @"";
                foreach (var component in panel.Components.Where(c => !(c is FreeText) && !(c is IComponentItem) && !c.AdminOnly))
                {
                    Index++;
                    html += @"<input type=""hidden"" name=""Components[" + Index + @"].Key"" value=""" + component.UId + @""" /><input type=""hidden"" name=""Components[" + Index + @"].Value"" value=""__skipped"" />";
                }
                return new MvcHtmlString(html);
            }
            else
            {
                var html = @"<div class=""row form-panel""><div class=""col-xs-12""><div class=""row form-panel-row"">";
                var prevRow = 0;
                var lastRow = panel.Components.Select(c => c.Row).Distinct().OrderBy(r => r).Last();
                var compHtml = "";
                while (prevRow < lastRow)
                {
                    var currentRow = ++prevRow;
                    foreach (var component in panel.Components.Where(c => c.Row == currentRow).OrderBy(c => c.Order))
                    {
                        compHtml += Render(component, admin, editing);
                    }
                }
                html += compHtml;
                html += "</div></div></div>";
                return new MvcHtmlString(String.IsNullOrWhiteSpace(compHtml) ? "" : this._Parse(html));
            }
        }

        public MvcHtmlString RenderConfirmationComponent(IComponent component)
        {
            if (component.AdminOnly)
                return new MvcHtmlString("");
            var commands = this._ExecuteLogic(component);
            //var commands = component.RunLogic(Registrant, true, Repository);
            if (!component.Enabled)
                return new MvcHtmlString("");
            if ((component.RSVP == RSVPType.Decline && Registrant.RSVP) || (component.RSVP == RSVPType.Accept && !Registrant.RSVP))
                return new MvcHtmlString("");
            if (component.Audiences.Count > 0 && (Registrant.Audience == null || !component.Audiences.Contains(Registrant.Audience)))
                return new MvcHtmlString("");
            foreach (var command in commands)
            {
                switch (command.Command)
                {
                    case JLogicWork.Hide:
                        return new MvcHtmlString("");
                }
            }
            if (component is FreeText)
            {
                if ((component as FreeText).HideReview)
                    return new MvcHtmlString("");
                return new MvcHtmlString(@"<tr><td colspan=""2"" class=""confirmation-freetext"">" + ((FreeText)component).Html + "</td></tr>");
            }
            var dp = Registrant.Data.Where(d => d.VariableUId == component.UId).FirstOrDefault();
            var f_value = "";
            if (dp != null)
            {
                f_value = dp.GetFormattedValue();
                if (dp.File != null)
                {
                    if (Regex.IsMatch(dp.File.FileType, @"^image", RegexOptions.IgnoreCase))
                    {
                        f_value = "<img src=\"../../Cloud/RegistrantImageThumbnail/" + Registrant.UId.ToString() + "?component=" + component.UId.ToString() + "&width=200\" />";
                    }
                }
            }
            else
            {
                dp = new RegistrantData()
                {
                    UId = Guid.NewGuid(),
                    Component = component as Component,
                    VariableUId = component.UId,
                    RegistrantKey = Registrant.UId,
                    Registrant = Registrant,
                    Value = null
                };
            }
            var html = @"<tr>";
            var labelValue = component.LabelText;
            if (!String.IsNullOrEmpty(labelValue))
            {
                if (!Regex.IsMatch(labelValue, @":$"))
                    labelValue += ":";
            }
            if (component is CheckboxGroup && String.IsNullOrEmpty(component.LabelText) && ((CheckboxGroup)component).Items.Count == 1)
            {
                var cbg = (CheckboxGroup)component;
                if (JsonConvert.DeserializeObject<List<Guid>>(dp.Value ?? "").Contains(cbg.Items[0].UId))
                    labelValue = "Checked";
                else
                    labelValue = "Not Checked";
            }
            if (component is Input && ((Input)component).Type == Domain.Entities.Components.InputType.UniversalCreditCard)
            {
                Guid cardId;
                if (dp != null && Guid.TryParse(dp.Value, out cardId))
                {
                    var card = Context.CreditCards.Where(c => c.UId == cardId).FirstOrDefault();
                    f_value = card == null ? "" : card.Number.GetLast(4);
                }
            }
            if (String.IsNullOrEmpty(f_value))
                f_value = "<i>No Selection</i>";
            html += @"<td>" + labelValue + @"</td>";
            html += @"<td>" + f_value + @"</td>";
            html += @"</tr>";
            return new MvcHtmlString(html);
        }

        public MvcHtmlString RenderConfirmationPanel(Panel panel)
        {
            if (panel.AdminOnly)
                return new MvcHtmlString("");
            var commands = this._ExecuteLogic(panel);
            //var commands = panel.RunLogic(Registrant, true, Repository);
            if (!panel.Enabled)
                return new MvcHtmlString("");
            if ((panel.RSVP == RSVPType.Decline && Registrant.RSVP) || (panel.RSVP == RSVPType.Accept && !Registrant.RSVP))
                return new MvcHtmlString("");
            if (panel.Audiences.Count > 0 && (Registrant.Audience == null || !panel.Audiences.Contains(Registrant.Audience)))
                return new MvcHtmlString("");
            foreach (var command in commands)
            {
                switch (command.Command)
                {
                    case JLogicWork.Hide:
                        return new MvcHtmlString("");
                }
            }
            var html = @"";
            var itemHtml = "";
            panel.Components.Sort();
            foreach (var component in panel.Components)
            {
                itemHtml += RenderConfirmationComponent(component);
            }
            if (String.IsNullOrWhiteSpace(itemHtml))
                return new MvcHtmlString("");
            html += itemHtml + @"";
            return new MvcHtmlString(html);
        }

        public MvcHtmlString RenderConfirmationPage(Page page)
        {
            var commands = this._ExecuteLogic(page);
            //var commands = page.RunLogic(Registrant, true, Repository);
            if (!page.Enabled || page.AdminOnly)
                return new MvcHtmlString("");
            if ((page.RSVP == RSVPType.Decline && Registrant.RSVP) || (page.RSVP == RSVPType.Accept && !Registrant.RSVP))
                return new MvcHtmlString("");
            if (page.Audiences.Count > 0 && (Registrant.Audience == null || !page.Audiences.Contains(Registrant.Audience)))
                return new MvcHtmlString("");
            foreach (var command in commands)
            {
                switch (command.Command)
                {
                    case JLogicWork.PageSkip:
                        return new MvcHtmlString("");
                }
            }

            var html = @"<tr><td colspan=""2"" class=""confirmation-page-header"">Page " + page.PageNumber + @"</td></tr>";
            var panelHtml = "";
            if (page.Type == PageType.RSVP)
            {
                panelHtml = @"<tr><td>RSVP:</td><td>" + (Registrant.RSVP ? Registrant.Form.RSVPAccept : Registrant.Form.RSVPDecline) + "</td></tr>";
            }
            else if (page.Type == PageType.Audience)
            {
                if (Form.Audiences.Count > 0)
                    panelHtml = @"<tr><td>Audience:</td><td>" + (Registrant.Audience != null ? Registrant.Audience.Label : "<i>Not Selected</i>") + "</td></tr>";
            }
            else
            {
                page.Panels.Sort();
                foreach (var panel in page.Panels)
                    panelHtml += RenderConfirmationPanel(panel);
            }
            if (String.IsNullOrWhiteSpace(panelHtml))
                return new MvcHtmlString("");
            html += panelHtml;
            return new MvcHtmlString(this._Parse(html));
        }

        protected IEnumerable<JLogicCommand> _ExecuteLogic(ILogicHolder holder, bool onLoad = true)
        {
            return LogicEngine.RunLogic(holder, registrant: Registrant, onLoad: onLoad, runCommands: false, allCommands: Commands);
        }


        protected string _Parse(string toParse)
        {
            if (Registrant != null)
                return Registrant.Parse(toParse);
            return Form.Parse(toParse);
        }

        protected MvcHtmlString _Parse(MvcHtmlString toParse)
        {
            if (Registrant == null)
                return new MvcHtmlString(Registrant.Parse(toParse.ToString()));
            return new MvcHtmlString(Form.Parse(toParse.ToString()));
        }

    }
}