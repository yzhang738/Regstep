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
using RSToolKit.Domain.JItems;
using RSToolKit.Domain.Engines;

namespace RSToolKit.WebUI.Infrastructure
{
    public class AdminRegistrationHtml
    {
        public Registrant Registrant { get; set; }
        public Form Form { get; set; }
        public Page Page { get; set; }
        public Dictionary<Guid, string> Errors { get; set; }
        public bool Live { get; set; }
        public FormsRepository Repository { get; set; }
        public List<Guid> CapacitiesUsed { get; set; }
        public RSParser Parser { get; set; }

        private int Index;
        private int WaitlistIndex;

        private static readonly string ItemOpen = "<div class=\"form-component-row\">";
        private static readonly string ItemClose = "</div>";
        private static readonly Regex valueToStyle = new Regex(@"^(?<group>[^_]*)_(?<variable>.*)$");


        public AdminRegistrationHtml(Form form, Registrant registrant, Page page, FormsRepository repository, Dictionary<Guid, string> errors = null)
        {
            Registrant = registrant;
            Form = form;
            Page = page;
            Errors = errors ?? new Dictionary<Guid, string>();
            Index = -1;
            WaitlistIndex = -1;
            Repository = repository;
            Form.Pages.Sort();
            CapacitiesUsed = new List<Guid>();
            if (page != null)
            {
                Page.Panels.Sort();
                Page.Panels.ForEach(p => p.Components.Sort());
            }
            Form.Audiences.Sort();
            Parser = new RSParser(Registrant, Repository, Form);
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
            if (Form.DisableShoppingCart)
                return new MvcHtmlString(html);
            var total = Registrant.Fees + Registrant.Adjustings - Registrant.Transactions;
            html = "Shopping Cart: " + total.ToString("c", Form.Culture);
            return new MvcHtmlString(html);
        }

        public MvcHtmlString RenderHiddens()
        {
            var html = @"<input type=""hidden"" name=""FormKey"" value=""" + Form.UId + @""" /><input type=""hidden"" name=""PageNumber"" value=""" + (Page == null ? "0" : Page.PageNumber.ToString()) + @""" /><input type=""hidden"" name=""Live"" value=""" + Live + @""" />";
            if (Registrant != null)
                html += @"<input type=""hidden"" name=""RegistrantKey"" value=""" + Registrant.UId + @""" /><input type=""hidden"" name=""Confirmation"" value=""" + Registrant.Confirmation + @""" />";
            return new MvcHtmlString(html);
        }

        public MvcHtmlString RenderComponent(Component component, bool editing = false)
        {
            bool skip = false;
            if ((Registrant.RSVP && component.RSVP == RSVPType.Decline) || (!Registrant.RSVP && component.RSVP == RSVPType.Accept))
                skip = true;
            if (component.Audiences.Count > 0 && (Registrant.Audience == null || !component.Audiences.Contains(Registrant.Audience)))
                skip = true;
            var commands = LogicEngine.RunLogic(component, Repository, registrant: Registrant);
            //var commands = component.RunLogic(Registrant, true, Repository);
            foreach (var command in commands)
            {
                switch (command.Command)
                {
                    case JLogicWork.Hide:
                        skip = true;
                        break;
                }
            }
            if (skip && !editing)
            {
                if (!(component is FreeText))
                {
                    if (!(component is IComponentItem))
                    {
                        Index++;
                        var html = @"<input type=""hidden"" name=""Components[" + Index + @"].Key"" value=""" + component.UId + @""" />";
                        html += @"<input type=""hidden"" name=""Components[" + Index + @"].Value"" value=""__skipped"" />";
                        return new MvcHtmlString(html);
                    }
                    else
                    {
                        var item = component as IComponentItem;
                        if (item == null)
                            return new MvcHtmlString("");
                        var dataPoint = Registrant.Data.FirstOrDefault(d => d.VariableUId == item.ParentKey);
                        if (dataPoint == null)
                            return new MvcHtmlString("");
                        if (dataPoint.Empty())
                            return new MvcHtmlString("");
                        if (item.Parent is IComponentMultipleSelection)
                        {
                            var jArray = JsonConvert.DeserializeObject<List<Guid>>(dataPoint.Value);
                            jArray.Remove(item.UId);
                            dataPoint.Value = JsonConvert.SerializeObject(jArray);
                        }
                        else
                        {
                            if (dataPoint.Value.ToLower() == item.UId.ToString().ToLower())
                                dataPoint.Value = null;
                        }
                        return new MvcHtmlString("");
                    }
                }
                else
                {
                    return new MvcHtmlString("");
                }
            }

            if (component is Input)
                return RenderComponent((Input)component, editing);
            else if (component is FreeText)
                return RenderComponent((FreeText)component, editing);
            else if (component is RadioGroup)
                return RenderComponent((RadioGroup)component, editing);
            else if (component is CheckboxGroup)
                return RenderComponent((CheckboxGroup)component, editing);
            else if (component is DropdownGroup)
                return RenderComponent((DropdownGroup)component, editing);
            else if (component is RadioItem)
                return RenderComponent((RadioItem)component, editing);
            else if (component is CheckboxItem)
                return RenderComponent((CheckboxItem)component, editing);
            else if (component is DropdownItem)
                return RenderComponent((DropdownItem)component, editing);
            else if (component is RatingSelect)
                return RenderComponent((RatingSelect)component, editing);
            return new MvcHtmlString("");
        }

        public MvcHtmlString RenderComponent(Input component, bool editing = false)
        {
            if (!component.Enabled)
                return new MvcHtmlString("");
            Index++;
            var html = "";
            html += "<div class=\"" + (component.Type == RSToolKit.Domain.Entities.Components.InputType.Multiline ? "col-md-6" : "col-sm-6 col-md-4 col-lg-3") + " form-component" + (component.Required ? " required-component" : "") + "\">";
            if (Errors.ContainsKey(component.UId))
            {
                html += "<div class=\"form-messagebox form-error-message\"><span class=\"glyphicon glyphicon-warning-sign\"></span>" + Errors[component.UId] + "</div>";
            }
            else
            {
                html += "<div class=\"form-messagebox form-error-message\" style=\"display: none;\"><span class=\"glyphicon glyphicon-warning-sign\"></span><span class=\"form-messagebox-message\"></span></div>";
            }
            component.DisplayComponentOrder.Items.Sort((a, b) => a.Order.CompareTo(b.Order));
            foreach (var item in component.DisplayComponentOrder.Items)
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
            switch (component.Type)
            {
                case Domain.Entities.Components.InputType.Multiline:
                    html += "<textarea" + (component.Required ? " data-form-required=\"true\"" : "") + (component.Length.HasValue ? " data-form-max=\"" + component.Length + "\"" : "") + " id=\"" + component.UId.ToString() + "\" name=\"Components[" + Index + "].Value\" class=\"form-control input-sm form-text-area\">" + Registrant[component.UId] + "</textarea>";
                    break;
                case Domain.Entities.Components.InputType.Password:
                    html += "<input" + (component.Required ? " data-form-required=\"true\"" : "") + (component.Length.HasValue ? " data-form-max=\"" + component.Length + "\"" : "") + " id=\"" + component.UId.ToString() + "\" class=\"form-control input-sm form-text-input\" type=\"password\" name=\"Components[" + Index + "].Value\" value=\"\" />";
                    break;
                case Domain.Entities.Components.InputType.UniversalCreditCard:
                    Guid cardId;
                    var peek = "";
                    var datapoint = Registrant.Data.Where(dp => dp.VariableUId == component.UId).FirstOrDefault();
                    if (datapoint != null && Guid.TryParse(datapoint.Value, out cardId))
                    {
                        if (editing)
                        {
                            var card = Repository.Search<CreditCard>(c => c.UId == cardId).FirstOrDefault();
                            if (card != null)
                                peek = card.Number;
                        }
                        else
                        {
                            peek = Repository.SecurePeek<CreditCard>(c => c.UId == cardId).FirstOrDefault();
                        }
                        if (String.IsNullOrEmpty(peek))
                            peek = "";
                    }
                    html += "<input" + (component.Required ? " data-form-required=\"true\"" : "") + " id=\"" + component.UId.ToString() + "\" class=\"form-control input-sm form-text-input\" type=\"text\" name=\"Components[" + Index + "].Value\" value=\"" + peek + "\" />";
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
                    html += "<input" + (component.Required ? " data-form-required=\"true\"" : "") + " id=\"" + component.UId.ToString() + "\" class=\"form-text-input\" type=\"file\" name=\"" + component.UId + "\"/></div>";
                    Index--;
                    return new MvcHtmlString(html);
                case Domain.Entities.Components.InputType.Date:
                    html += "<input" + (component.Required ? " data-form-required=\"true\"" : "") + " id=\"" + component.UId.ToString() + "\" name=\"Components[" + Index + "].Value\" value=\"" + Registrant[component.UId] + "\" class=\"form-control input-sm form-text-input\" type=\"text\" data-component-type=\"datetime\" data-input-type=\"date\" name=\"" + component.UId + "\"" + (component.MinDate.HasValue ? "  data-date-startDate=\"" + component.MinDate.Value.ToShortDateString() + "\"" : "") + (component.MaxDate.HasValue ? " data-date-endDate=\"" + component.MaxDate.Value.ToShortDateString() + "\"" : "") + " data-min-view=\"2\" data-date-format=\"m/d/yyyy\"/>";
                    break;
                case Domain.Entities.Components.InputType.DateTime:
                    html += "<input" + (component.Required ? " data-form-required=\"true\"" : "") + " id=\"" + component.UId.ToString() + "\" name=\"Components[" + Index + "].Value\" value=\"" + Registrant[component.UId] + "\" class=\"form-control input-sm form-text-input\" type=\"text\"data-component-type=\"datetime\" name=\"" + component.UId + "\" " + (component.MinDate.HasValue ? " data-date-startDate=\"" + component.MinDate.Value.ToString("M/d/yyy h/mm tt") + "\"" : "") + (component.MaxDate.HasValue ? " data-date-endDate=\"" + component.MaxDate.Value.ToString("M/d/yyy h/mm tt") + "\"" : "") + " data-date-showmeridian=\"true\" data-date-format=\"m/d/yyyy H:ii P\"/>";
                    break;
                case Domain.Entities.Components.InputType.Time:
                    html += "<input" + (component.Required ? " data-form-required=\"true\"" : "") + " id=\"" + component.UId.ToString() + "\" name=\"Components[" + Index + "].Value\" value=\"" + Registrant[component.UId] + "\" class=\"form-control input-sm form-text-input\" type=\"text\" data-date-pickDate=\"false\" name=\"" + component.UId + "\" data-component-type=\"datetime\" data-date-startView=\"0\" data-date-maxView=\"0\" data-date-showmeridian=\"true\" data-date-format=\"H:ii P\"/>";
                    break;
                default:
                    html += "<input" + (component.Required ? " data-form-required=\"true\"" : "") + (component.Type != Domain.Entities.Components.InputType.Default ? "" : " data-form-validation=\"" + component.ValueType.GetTagValue() + "\"") + (component.Length.HasValue ? " data-form-max=\"" + component.Length + "\"" : "") + " id=\"" + component.UId.ToString() + "\" class=\"form-control input-sm form-text-input\" type=\"text\" name=\"Components[" + Index + "].Value\" value=\"" + Registrant[component.UId] + "\" data-component-type=\"" + component.Type.GetTagValue() + "\" data-component-required=\"" + component.Required + "\" data-component-regex=\"" + component.RegexPattern.GetRgxValue() + "\" />";
                    break;
            }
            html += "<input type=\"hidden\" name=\"Components[" + Index + "].Key\" value=\"" + component.UId + "\" /></div>";
            return new MvcHtmlString(html);
        }

        public MvcHtmlString RenderComponent(FreeText component, bool editing = false)
        {
            if (!component.Enabled)
                return new MvcHtmlString("");
            var html = "";
            html = "<div class=\"col-xs-12 form-component\">" + Parser.ParseAvailable(component.Html) + "</div>";
            return new MvcHtmlString(html);
        }

        public MvcHtmlString RenderComponent(RadioGroup component, bool editing = false)
        {
            if (!component.Enabled)
                return new MvcHtmlString("");
            Index++;
            var html = "";
            html += "<div class=\"col-md-6 form-component" + (component.Required ? " required-component" : "") + "\" data-component-type=\"radiogroup\" data-component-index=\"" + Index + "\">";
            if (Errors.ContainsKey(component.UId))
            {
                html += "<div class=\"form-messagebox form-error-message\"><span class=\"glyphicon glyphicon-warning-sign\"></span>" + Errors[component.UId] + "</div>";
            }
            else
            {
                html += "<div class=\"form-messagebox form-error-message\" style=\"display: none;\"><span class=\"glyphicon glyphicon-warning-sign\"></span><span class=\"form-messagebox-message\"></span></div>";
            }
            component.DisplayComponentOrder.Items.Sort((a, b) => a.Order.CompareTo(b.Order));
            var price = PriceGroup.DisplayPrice(component, Registrant);
            foreach (var item in component.DisplayComponentOrder.Items)
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
            html += "<input type=\"hidden\" name=\"Components[" + Index + "].Key\" value=\"" + component.UId + "\" />";
            html += "<input" + (component.Required ? " data-form-required=\"true\"" : "") + " data-component-type=\"radiogroup\" id=\"" + component.UId + "\" type=\"hidden\" name=\"Components[" + Index + "].Value\" value=\"" + Registrant[component.UId] + "\"><div class=\"form-items\">";
            component.Items.Sort((a, b) => a.Order.CompareTo(b.Order));
            foreach (var item in component.Items)
            {
                html += RenderComponent((Component)item, editing);
            }
            html += "</div></div>";

            return new MvcHtmlString(html);
        }

        public MvcHtmlString RenderComponent(RadioItem component, bool editing = false)
        {
            Guid selected = Guid.Empty;
            Guid.TryParse(Registrant[component.ParentKey], out selected);
            var disabled = false;
            if (!component.Enabled && selected != component.UId)
            {
                if (!component.Display)
                    return new MvcHtmlString("");
                disabled = true;
            }
            var seating = component.Seating;
            var seated = true;
            var waitlisted = false;
            var full = false;
            if (seating != null)
            {
                // There is seating so we grab the seat for the user if available.
                var seater = seating.Seaters.Where(s => s.RegistrantKey == Registrant.UId && s.ComponentKey == component.UId).FirstOrDefault();
                full = seating.AvailableSeats < 1;
                seated = seater != null && seater.Seated;
                waitlisted = seater != null && !seater.Seated;
            }
            var html = "";
            html += "<div class=\"form-item" + (component.Required ? " required-item" : "") + "\"><div class=\"form-radio radio\">";
            if (disabled)
                html += "<fieldset disabled>";
            html += "<label>";
            if (Errors.ContainsKey(component.UId))
            {
                html += "<div class=\"form-messagebox form-error-message\"><span class=\"glyphicon glyphicon-warning-sign\"></span>" + Errors[component.UId] + "</div>";
            }
            else
            {
                html += "<div class=\"form-messagebox form-error-message\" style=\"display: none;\"><span class=\"glyphicon glyphicon-warning-sign\"></span><span class=\"form-messagebox-message\"></span></div>";
            }
            component.DisplayComponentOrder.Items.Sort((a, b) => a.Order.CompareTo(b.Order));
            var price = PriceGroup.DisplayPrice(component, Registrant);
            // We need to check seating.
            if (disabled)
            {
                html += "<input type=\"radio\" disabled=\"true\" name=\"radio" + Index + "\" value=\"" + component.UId + "\" data-parent=\"" + component.RadioGroupKey + "\" />";
                if (full)
                    html += "<div class=\"form-at-capacity\">" + seating.FullLabel + "</div>";
            }
            else
            {
                html += "<input id=\"" + component.UId + "\" class=\"input-outside\" type=\"radio\" name=\"radio" + Index + "\" value=\"" + component.UId + "\"" + (selected == component.UId ? " checked=\"checked\"" : "") + " data-parent=\"" + component.RadioGroupKey + "\" />";
                if (full)
                    html += "<div class=\"form-at-capacity\">" + seating.FullLabel + (seated ? " <i>previously seated</i>" : "") + "</div>";
            }
            foreach (var item in component.DisplayComponentOrder.Items)
            {
                switch (item.Item.ToLower())
                {
                    case "label":
                        var labelStyleList = component.Styles.Where(s => s.GroupName == ".form-component-label").ToList();
                        var labelStyle = "";
                        foreach (var style in labelStyleList)
                            labelStyle += style.Variable + ":" + style.Value + ";";
                        html += "<div class=\"form-item-row\"><span class=\"form-item-label\"" + (String.IsNullOrEmpty(labelStyle) ? "" : " style=\"" + labelStyle + "\"") + ">" + component.LabelText + "</span></div>";
                        break;
                    case "description":
                    case "alttext":
                        var descriptionStyleList = component.Styles.Where(s => s.GroupName == ".form-component-label").ToList();
                        var descriptionStyle = "";
                        foreach (var style in descriptionStyleList)
                            descriptionStyle += style.Variable + ":" + style.Value + ";";
                        html += "<div class=\"form-item-row\"><span class=\"form-item-description\"" + (String.IsNullOrEmpty(descriptionStyle) ? "" : " style=\"" + descriptionStyle + "\"") + ">" + component.AltText + "</span></div>";
                        break;
                    case "date":
                        var displayDate = "";
                        var multipleDays = component.AgendaEnd.Date != component.AgendaStart.Date;
                        var showDate = component.DisplayAgendaDate | multipleDays;
                        if (showDate)
                            displayDate = component.AgendaStart.ToString(Form.Culture) + " - " + component.AgendaEnd.ToString(Form.Culture);
                        else
                            displayDate = component.AgendaStart.DateTime.ToShortTimeString() + " - " + component.AgendaEnd.DateTime.ToShortTimeString();
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
                html += "<label><input type=\"hidden\" name=\"Waitlistings[" + WaitlistIndex + "].Key\" value=\"" + component.UId + "\" /><input type=\"checkbox\"" + (waitlisted ? " checked=\"true\"" : "") + " name=\"Waitlistings[" + WaitlistIndex + "].Value\" value=\"true\"><input type=\"hidden\" name=\"Waitlistings[" + WaitlistIndex + "].Value\" value=\"false\" /> " + seating.WaitlistLabel + "</label>";
            }
            html += "</div></div>";
            return new MvcHtmlString(html);
        }

        public MvcHtmlString RenderComponent(DropdownGroup component, bool editing = false)
        {
            Index++;
            var html = "";
            html += "<div class=\"col-sm-6 col-md-4 col-lg-3 form-component" + (component.Required ? " required-component" : "") + "\" data-component-type=\"dropdown\" data-component-index=\"" + Index + "\">";
            if (Errors.ContainsKey(component.UId))
            {
                html += "<div class=\"form-messagebox form-error-message\"><span class=\"glyphicon glyphicon-warning-sign\"></span>" + Errors[component.UId] + "</div>";
            }
            else
            {
                html += "<div class=\"form-messagebox form-error-message\" style=\"display: none;\"><span class=\"glyphicon glyphicon-warning-sign\"></span><span class=\"form-messagebox-message\"></span></div>";
            }
            component.DisplayComponentOrder.Items.Sort((a, b) => a.Order.CompareTo(b.Order));
            var price = PriceGroup.DisplayPrice(component, Registrant);
            foreach (var item in component.DisplayComponentOrder.Items)
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
            html += "<input type=\"hidden\" name=\"Components[" + Index + "].Key\" value=\"" + component.UId + "\" /><div class=\"form-items\"><select" + (component.Required ? " data-form-required=\"true\"" : "") + " id=\"" + component.UId.ToString() + "\" name=\"Components[" + Index + "].Value\" class=\"form-control input-sm form-select\">";
            component.Items.Sort((a, b) => a.Order.CompareTo(b.Order));
            var selected = Guid.Empty;
            bool hasValue = Guid.TryParse(Registrant[component.UId], out selected);
            html += "<option value=\"\"" + (hasValue ? "" : " selected=true") + "></option>";
            foreach (var item in component.Items)
            {
                html += RenderComponent((Component)item, editing);
            }
            html += "</select></div></div>";

            return new MvcHtmlString(html);
        }

        public MvcHtmlString RenderComponent(DropdownItem component, bool editing = false)
        {
            var seating = component.Seating;
            var full = false;
            var selected = Guid.Empty;
            bool hasValue = Guid.TryParse(Registrant[component.ParentKey], out selected);
            if (!component.Enabled && selected != component.UId)
                return new MvcHtmlString("");
            return new MvcHtmlString("<option value=\"" + component.UId + "\"" + (selected == component.UId ? " selected=true" : "") + (full ? " disabled=\"true\"" : "") + ">" + component.LabelText + "</option>");
        }

        public MvcHtmlString RenderComponent(CheckboxGroup component, bool editing = false)
        {
            Index++;
            var html = "";
            html += "<div class=\"col-md-6 form-component" + (component.Required ? " required-component" : "") + "\" data-component-type=\"checkbox\" data-component-index=\"" + Index + "\" data-component-timeexclusion=\"" + component.TimeExclusion + "\">";
            if (Errors.ContainsKey(component.UId))
            {
                html += "<div class=\"form-messagebox form-error-message\"><span class=\"glyphicon glyphicon-warning-sign\"></span>" + Errors[component.UId] + "</div>";
            }
            else
            {
                html += "<div class=\"form-messagebox form-error-message\" style=\"display: none;\"><span class=\"glyphicon glyphicon-warning-sign\"></span><span class=\"form-messagebox-message\"></span></div>";
            }
            component.DisplayComponentOrder.Items.Sort((a, b) => a.Order.CompareTo(b.Order));
            var price = PriceGroup.DisplayPrice(component, Registrant);
            foreach (var item in component.DisplayComponentOrder.Items)
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
            html += "<input type=\"hidden\" name=\"Components[" + Index + "].Key\" value=\"" + component.UId + "\" /><input " + (component.Required ? " data-form-required=\"true\"" : "") + " data-component-timeexclusion-message=\"" + component.DialogText + "\" data-component-type=\"checkboxgroup\" data-component-timeexclusion=\"" + component.TimeExclusion + "\" id=\"" + component.UId + "\" type=\"hidden\" name=\"Components[" + Index + "].Value\" value='" + Registrant[component.UId] + "'><div class=\"form-items\">";
            component.Items.Sort((a, b) => a.Order.CompareTo(b.Order));
            foreach (var item in component.Items)
            {
                html += RenderComponent((Component)item, editing);
            }
            html += "</div></div>";

            return new MvcHtmlString(html);
        }

        public MvcHtmlString RenderComponent(CheckboxItem component, bool editing = false)
        {
            var selected = false;
            var dataPoint = Registrant.Data.Where(d => d.VariableUId == component.ParentKey).FirstOrDefault();
            if (dataPoint != null)
            {
                var selections = JsonConvert.DeserializeObject<List<Guid>>(dataPoint.Value);
                selected = component.UId.In(selections.ToArray());
            }
            var disabled = false;
            if (!component.Enabled && !selected)
            {
                if (!component.Display)
                    return new MvcHtmlString("");
                disabled = true;
            }
            var seating = component.Seating;
            var seated = true;
            var waitlisted = false;
            var full = false;
            if (seating != null)
            {
                // There is seating so we grab the seat for the user if available.
                var seater = seating.Seaters.Where(s => s.RegistrantKey == Registrant.UId && s.ComponentKey == component.UId).FirstOrDefault();
                full = seating.AvailableSeats < 1;
                seated = seater != null && seater.Seated;
                waitlisted = seater != null && !seater.Seated;
            }
            var html = "";
            html += "<div class=\"form-item" + (component.Required ? " required-item" : "") + "\"><div class=\"form-checkbox checkbox\">";
            if (disabled)
                html += "<fieldset disabled>";
            html += "<label>";
            if (Errors.ContainsKey(component.UId))
            {
                html += "<div class=\"form-messagebox form-error-message\"><span class=\"glyphicon glyphicon-warning-sign\"></span>" + Errors[component.UId] + "</div>";
            }
            else
            {
                html += "<div class=\"form-messagebox form-error-message\" style=\"display: none;\"><span class=\"glyphicon glyphicon-warning-sign\"></span><span class=\"form-messagebox-message\"></span></div>";
            }
            component.DisplayComponentOrder.Items.Sort((a, b) => a.Order.CompareTo(b.Order));
            var price = PriceGroup.DisplayPrice(component, Registrant);
            if (disabled)
            {
                html += "<input" + (component.Required ? " data-form-required=\"true\"" : "") + " type=\"checkbox\" disabled=\"true\" " + (component.CheckboxGroup.TimeExclusion ? "data-agenda-start=\"" + component.AgendaStart.ToString() + "\" data-agenda-end=\"" + component.AgendaEnd.ToString() + "\"" : "") + " data-parent=\"" + component.ParentKey.ToString() + "\" value=\"" + component.UId + "\" data-component-required=\"" + (component.Required ? "true" : "false") + "\" />";
                if (full)
                    html += "<div class=\"form-at-capacity\">" + seating.FullLabel + "</div>";
            }
            else
            {
                html += "<input id=\"" + component.UId + "\"" + (selected ? " checked=\"checked\"" : "") + " type=\"checkbox\"" + (component.CheckboxGroup.TimeExclusion ? " data-agenda-start=\"" + component.AgendaStart.ToString() + "\" data-agenda-end=\"" + component.AgendaEnd.ToString() + "\"" : "") + " data-parent=\"" + component.ParentKey.ToString() + "\" value=\"" + component.UId + "\" data-component-required=\"" + (component.Required ? "true" : "false") + "\" />";
                if (full)
                    html += "<div class=\"form-at-capacity\">" + seating.FullLabel + (seated ? " <i>previously seated</i>" : "") + "</div>";
            }
            foreach (var item in component.DisplayComponentOrder.Items)
            {
                switch (item.Item.ToLower())
                {
                    case "label":
                        var labelStyleList = component.Styles.Where(s => s.GroupName == ".form-component-label").ToList();
                        var labelStyle = "";
                        foreach (var style in labelStyleList)
                            labelStyle += style.Variable + ":" + style.Value + ";";
                        html += "<div class=\"form-item-row\"><span class=\"form-item-label\"" + (String.IsNullOrEmpty(labelStyle) ? "" : " style=\"" + labelStyle + "\"") + ">" + component.LabelText + "</span></div>";
                        break;
                    case "description":
                    case "alttext":
                        var descriptionStyleList = component.Styles.Where(s => s.GroupName == ".form-component-label").ToList();
                        var descriptionStyle = "";
                        foreach (var style in descriptionStyleList)
                            descriptionStyle += style.Variable + ":" + style.Value + ";";
                        html += "<div class=\"form-item-row\"><span class=\"form-item-description\"" + (String.IsNullOrEmpty(descriptionStyle) ? "" : " style=\"" + descriptionStyle + "\"") + ">" + component.AltText + "</span></div>";
                        break;
                    case "date":
                        var displayDate = "";
                        var multipleDays = component.AgendaEnd.Date != component.AgendaStart.Date;
                        var showDate = component.DisplayAgendaDate | multipleDays;
                        if (showDate)
                            displayDate = component.AgendaStart.ToString(Form.Culture) + " - " + component.AgendaEnd.ToString(Form.Culture);
                        else
                            displayDate = component.AgendaStart.DateTime.ToShortTimeString() + " - " + component.AgendaEnd.DateTime.ToShortTimeString();
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
                html += "<label class=\"waitlist\"><input type=\"hidden\" name=\"Waitlistings[" + WaitlistIndex + "].Key\" value=\"" + component.UId + "\" /><input type=\"checkbox\"" + (waitlisted ? " checked=\"checked\"" : "") + " name=\"Waitlistings[" + WaitlistIndex + "].Value\" value=\"true\"><input type=\"hidden\" name=\"Waitlistings[" + WaitlistIndex + "].Value\" value=\"false\" /> " + seating.WaitlistLabel + "</label>";
            }
            html += "</div></div>";
            return new MvcHtmlString(html);

        }

        public MvcHtmlString RenderComponent(RatingSelect component, bool editing = false)
        {
            var html = "";
            if (Form.ParentForm == null || component.MappedComponent == null)
            {
                var dataPoint = Registrant.Data.FirstOrDefault(d => d.VariableUId == component.UId);
                html += "<div class=\"col-sm-6 col-md-4 col-lg-3 form-component\" data-component-index=\"" + Index + "\">";
                component.DisplayComponentOrder.Items.Sort((a, b) => a.Order.CompareTo(b.Order));
                foreach (var item in component.DisplayComponentOrder.Items)
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
                html += "<input class=\"rating form-control input-sm form-text-input\" type=\"password\" name=\"Components[" + Index + "].Value\" value=\"" + (dataPoint != null ? dataPoint.Value : "") + "\" />";
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
                            Guid t_id;
                            if (Guid.TryParse(t_dataPoint.Value, out t_id))
                            {
                                selections.Add(t_id);
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
                            component.DisplayComponentOrder.Items.Sort((a, b) => a.Order.CompareTo(b.Order));
                            foreach (var item in component.DisplayComponentOrder.Items)
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
                            html += "<input class=\"rating form-control input-sm form-text-input\" type=\"text\" data-showClear=\"false\" data-showCaption=\"false\" data-size=\"sm\" name=\"Components[" + Index + "].Value\" value=\"" + (dataPoint != null ? dataPoint.Value : "") + "\" />";
                            html += "<input type=\"hidden\" name=\"Components[" + Index + "].Key\" value=\"" + t_item.UId + "\" />";
                            html += "</div>";
                        }
                    }
                }
            }
            return new MvcHtmlString(html);
        }

        public MvcHtmlString RenderFooter()
        {
            return new MvcHtmlString(Parser.ParseAvailable(Form.Footer));
        }

        public MvcHtmlString RenderHeader()
        {
            return new MvcHtmlString(Parser.ParseAvailable(Form.Header));
        }

        public MvcHtmlString RenderRsvp(bool first = false)
        {
            var html = "<div class=\"row form-btn-row\"><div class=\"col-xs-12\"><div class=\"row\"><input type=\"hidden\" name=\"RSVP\" id=\"RSVP\" value=\"\" />";
            html += "<div class=\"col-sm-4 col-md-3 col-lg-2 form-btn\"><a href=\"#\" class=\"btn-rs btn-rsvp" + (Registrant.RSVP == true && !first ? " btn-selected" : "") + "\" id=\"rsvpAccept\">" + Form.RSVPAccept + @"</a></div>";
            html += "<div class=\"col-sm-4 col-md-3 col-lg-2 form-btn\"><a href=\"#\" class=\"btn-rs btn-rsvp" + (Registrant.RSVP == false && !first ? " btn-selected" : "") + "\" id=\"rsvpDecline\">" + Form.RSVPDecline + @"</a></div>";
            html += "</div></div></div>";
            return new MvcHtmlString(Parser.ParseAvailable(html));
        }

        public MvcHtmlString RenderAudiences()
        {
            var html = "<div class=\"row form-btn-panel\"><div class=\"col-xs-12\"><div class=\"row\">";
            foreach (var audience in Form.Audiences)
            {
                html += "<div class=\"col-sm-4 col-md-3 col-lg-2 form-btn\"><a href=\"#\" class=\"btn-rs btn-audience" + (Registrant.AudienceKey.HasValue ? (Registrant.AudienceKey == audience.UId ? " btn-selected" : "") : "") + "\" data-value=\"" + audience.UId + "\">" + audience.Label + @"</a></div>";
            }
            html += "<input type=\"hidden\" name=\"AudienceKey\" id=\"AudienceKey\" value=\"" + Registrant.AudienceKey + "\"/></div></div></div>";
            return new MvcHtmlString(Parser.ParseAvailable(html));
        }

        public MvcHtmlString RenderPanel(Panel panel)
        {
            var skipped = false;
            if (!panel.Enabled)
                return new MvcHtmlString("");
            if (panel.AdminOnly && Live)
                return new MvcHtmlString("");
            if ((panel.RSVP == RSVPType.Accept && !Registrant.RSVP) || (panel.RSVP == RSVPType.Decline && Registrant.RSVP))
                skipped = true;
            else if (panel.Audiences.Count > 0 && (Registrant.Audience != null && !panel.Audiences.Contains(Registrant.Audience)))
                skipped = true;
            var commands = LogicEngine.RunLogic(panel, Repository, registrant: Registrant);
            //var commands = panel.RunLogic(Registrant, true, Repository);
            foreach (var command in commands)
            {
                switch (command.Command)
                {
                    case JLogicWork.SetVar:
                        #region SetVar
                        if (Guid.Parse(command.Parameters["Form"]) == Form.UId)
                        {
                            Guid variableUId;
                            var variable = command.Parameters["Variable"];
                            var value = command.Parameters["Value"];
                            RegistrantData dataPoint;
                            var exit = false;
                            switch (variable.ToLower())
                            {
                                case "email":
                                    Registrant.Email = value;
                                    exit = true;
                                    break;
                                case "RSVP":
                                    Registrant.RSVP = bool.Parse(value);
                                    exit = true;
                                    break;
                                case "Audience":
                                    Registrant.Audience = Form.Audiences.Where(a => a.UId == Guid.Parse(value)).First();
                                    exit = true;
                                    break;
                                case "Status":
                                    Registrant.Status = (RegistrationStatus)Int32.Parse(variable);
                                    exit = true;
                                    break;
                            }
                            if (exit)
                                break;
                            variableUId = Guid.Parse(variable);
                            dataPoint = Registrant.Data.Where(d => d.VariableUId == variableUId).FirstOrDefault();
                            if (dataPoint == null)
                            {
                                dataPoint = new RegistrantData()
                                {
                                    VariableUId = variableUId,
                                    Value = value
                                };
                                dataPoint.Component = Repository.Search<Component>(c => c.UId == variableUId).First();
                                Registrant.Data.Add(dataPoint);
                            }
                        }
                        else
                        {
                            Guid variableUId;
                            var c_form = Repository.Search<Form>(f => f.UId == Guid.Parse(command.Parameters["Form"])).First();
                            var c_registrant = Repository.Search<Registrant>(r => r.FormKey == c_form.UId && r.Email == Registrant.Email).FirstOrDefault();
                            if (c_registrant == null)
                                break;
                            var variable = command.Parameters["Variable"];
                            var value = command.Parameters["Value"];
                            RegistrantData dataPoint;
                            bool exit = false;
                            switch (variable.ToLower())
                            {
                                case "email":
                                    c_registrant.Email = value;
                                    exit = true;
                                    break;
                                case "RSVP":
                                    c_registrant.RSVP = bool.Parse(value);
                                    exit = true;
                                    break;
                                case "Audience":
                                    c_registrant.Audience = c_form.Audiences.Where(a => a.UId == Guid.Parse(value)).First();
                                    exit = true;
                                    break;
                                case "Status":
                                    c_registrant.Status = (RegistrationStatus)Int32.Parse(variable);
                                    exit = true;
                                    break;
                            }
                            if (exit)
                                break;
                            variableUId = Guid.Parse(variable);
                            dataPoint = c_registrant.Data.Where(d => d.VariableUId == variableUId).FirstOrDefault();
                            if (dataPoint == null)
                            {
                                dataPoint = new RegistrantData()
                                {
                                    VariableUId = variableUId,
                                    Value = value
                                };
                                dataPoint.Component = Repository.Search<Component>(c => c.UId == variableUId).First();
                                Registrant.Data.Add(dataPoint);
                            }
                        }
                        #endregion
                        break;
                    case JLogicWork.Hide:
                        skipped = true;
                        break;
                }
            }
            var html = @"";
            if (skipped)
            {
                foreach (var component in panel.Components.Where(c => !(c is FreeText) && !(c is IComponentItem) && !c.AdminOnly))
                {
                    Index++;
                    html += @"<input type=""hidden"" name=""Components[" + Index + @"].Key"" value=""" + component.UId + @""" /><input type=""hidden"" name=""Components[" + Index + @"].Value"" value=""__skipped"" />";
                }
                return new MvcHtmlString(html);
            }
            else
            {
                html = @"<div class=""row form-panel""><div class=""col-xs-12""><div class=""row form-panel-row"">";
                var prevRow = 0;
                var lastRow = panel.Components.Select(c => c.Row).Distinct().OrderBy(r => r).Last();
                while (prevRow < lastRow)
                {
                    var currentRow = ++prevRow;
                    foreach (var component in panel.Components.Where(c => c.Row == currentRow).OrderBy(c => c.Order))
                    {
                        html += RenderComponent(component);
                    }
                }
                html += "</div></div></div>";
                return new MvcHtmlString(Parser.ParseAvailable(html));
            }
        }

        public MvcHtmlString RenderConfirmationComponent(Component component)
        {
            if (component.AdminOnly)
                return new MvcHtmlString("");
            var commands = LogicEngine.RunLogic(component, Repository, registrant: Registrant);
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
                return new MvcHtmlString(@"<tr><td colspan=""2"" class=""confirmation-freetext"">" + ((FreeText)component).Html + "</td></tr>");
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
                if (JsonConvert.DeserializeObject<List<Guid>>(dp.Value).Contains(cbg.Items[0].UId))
                    labelValue = "Checked";
                else
                    labelValue = "Not Checked";
            }
            if (component is Input && ((Input)component).Type == Domain.Entities.Components.InputType.UniversalCreditCard)
            {
                Guid cardId;
                if (dp != null && Guid.TryParse(dp.Value, out cardId))
                {
                    f_value = Repository.SecurePeek<CreditCard>(c => c.UId == cardId).FirstOrDefault();
                }
            }
            if (String.IsNullOrEmpty(f_value))
                f_value = "<i>No Selection</i>";
            html += @"<td>" + labelValue + @"</td>";
            html += @"<td>" + f_value + @"</td>";
            html += @"</tr>";
            var waitlisted = false;
            var waitlistedHtml = @"<tr><td><span class=""confirmation-waitlist-label"">Waitlistings:</td>";
            if (component is CheckboxGroup)
            {
                foreach (var item in ((CheckboxGroup)component).Items)
                {
                    var seat = item.Seating != null ? item.Seating.Seaters.FirstOrDefault(c => c.ComponentKey == item.UId && c.RegistrantKey == Registrant.UId && !c.Seated) : null;
                    if (seat != null)
                    {
                        waitlistedHtml += @"<td>" + seat.Component.LabelText + "</td>";
                        waitlisted = true;
                    }
                }
            }
            else if (component is RadioGroup)
            {
                foreach (var item in ((RadioGroup)component).Items)
                {
                    var seat = item.Seating != null ? item.Seating.Seaters.FirstOrDefault(c => c.ComponentKey == item.UId && c.RegistrantKey == Registrant.UId && !c.Seated) : null;
                    if (seat != null)
                    {
                        waitlistedHtml += @"<td>" + seat.Component.LabelText + "</td>";
                        waitlisted = true;
                    }
                }
            }
            waitlistedHtml += @"</tr>";
            if (waitlisted)
                html += waitlistedHtml;
            return new MvcHtmlString(html);
        }

        public MvcHtmlString RenderConfirmationPanel(Panel panel)
        {
            if (panel.AdminOnly)
                return new MvcHtmlString("");
            var commands = LogicEngine.RunLogic(panel, Repository, registrant: Registrant);
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
            var commands = LogicEngine.RunLogic(page, Repository, registrant: Registrant);
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
            return new MvcHtmlString(Parser.ParseAvailable(html));
        }

        #region EditComponents

        public MvcHtmlString RenderEditComponent(Component component, int? width)
        {
            MvcHtmlString content = null;
            if (component is Input)
                content = RenderEditComponent((Input)component, width);
            else if (component is FreeText)
                content = RenderEditComponent((FreeText)component);
            else if (component is RadioGroup)
                content = RenderEditComponent((RadioGroup)component);
            else if (component is CheckboxGroup)
                content = RenderEditComponent((CheckboxGroup)component);
            else if (component is DropdownGroup)
                content = RenderEditComponent((DropdownGroup)component);
            else if (component is RadioItem)
                content = RenderEditComponent((RadioItem)component);
            else if (component is CheckboxItem)
                content = RenderEditComponent((CheckboxItem)component);
            else if (component is DropdownItem)
                content = RenderEditComponent((DropdownItem)component);
            return new MvcHtmlString(Parser.ParseAvailable(content.ToString()));
        }

        public MvcHtmlString RenderEditComponent(Input component, int? width)
        {
            Index++;
            var html = "";
            html += "<div  class=\"" + (component.Type == RSToolKit.Domain.Entities.Components.InputType.Multiline ? "col-md-6" : "col-sm-6 col-md-4 col-lg-3") + " form-component" + (component.Required ? " required-component" : "") + "\">";
            if (Errors.ContainsKey(component.UId))
            {
                html += "<div class=\"form-component-error\"><span class=\"glyphicon glyphicon-warning-sign\"></span>" + Errors[component.UId] + "</div>";
            }
            component.DisplayComponentOrder.Items.Sort((a, b) => a.Order.CompareTo(b.Order));
            foreach (var item in component.DisplayComponentOrder.Items)
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
            switch (component.Type)
            {
                case Domain.Entities.Components.InputType.Multiline:
                    html += "<textarea name=\"value\" id=\"value\" class=\"form-control input-sm form-text-area\">" + Registrant[component.UId] + "</textarea>";
                    break;
                case Domain.Entities.Components.InputType.Password:
                    html += "<input class=\"form-control input-sm form-text-input\" type=\"password\" name=\"value\" id=\"value\" value=\"\" />";
                    break;
                case Domain.Entities.Components.InputType.UniversalCreditCard:
                    bool creditCardSaved = Registrant.Data.Where(d => d.VariableUId == component.UId).FirstOrDefault() != null;
                    html += "<input class=\"form-control input-sm form-text-input\" type=\"text\" name=\"value\" id=\"value\" value=\"" + (creditCardSaved ? "Credit Card Captured" : "") + "\" />";
                    break;
                case Domain.Entities.Components.InputType.File:
                    var inputData = Registrant.Data.Where(d => d.VariableUId == component.UId).FirstOrDefault();
                    if (inputData != null && inputData.File != null && ((Input)component).FileType == "picture")
                    {
                        html += "<div class=\"form-file\"><img class=\"form-file\" src=\"..\\..\\Cloud\\RegistrantImageThumbnail\\" + Registrant.UId + "?component=" + component.UId + (width.HasValue ? "&width=" + width.Value : "") + "\" /></div>";
                    }
                    if (inputData != null)
                        html += "<div><a href=\"..\\..\\Cloud\\RegistrantFile\\" + inputData.UId + "\">Download File</a></div>";
                    Index--;
                    return new MvcHtmlString(html);
                case Domain.Entities.Components.InputType.Date:
                    html += "<input name=\"value\" id=\"value\" value=\"" + Registrant[component.UId] + "\" class=\"form-control input-sm form-text-input\" type=\"text\" data-component-type=\"datetime\" data-date-pickTime=\"false\" name=\"" + component.UId + "\"" + (component.MinDate.HasValue ? "  data-date-minDate=\"" + component.MinDate.Value.ToShortDateString() + "\"" : "") + (component.MaxDate.HasValue ? " data-date-maxDate=\"" + component.MaxDate.Value.ToShortDateString() + "\"" : "") + " />";
                    break;
                case Domain.Entities.Components.InputType.DateTime:
                    html += "<input name=\"value\" id=\"value\" value=\"" + Registrant[component.UId] + "\" class=\"form-control input-sm form-text-input\" type=\"text\"data-component-type=\"datetime\" name=\"" + component.UId + "\"" + (component.MinDate.HasValue ? " data-date-minDate=\"" + component.MinDate.Value.ToShortDateString() + "\"" : "") + (component.MaxDate.HasValue ? " data-date-maxDate=\"" + component.MaxDate.Value.ToShortDateString() + "\"" : "") + " />";
                    break;
                case Domain.Entities.Components.InputType.Time:
                    html += "<input name=\"value\" id=\"value\" value=\"" + Registrant[component.UId] + "\" class=\"form-control input-sm form-text-input\" type=\"text\" data-date-pickDate=\"false\" name=\"" + component.UId + "\" data-component-type=\"datetime\" />";
                    break;
                default:
                    html += "<input class=\"form-control input-sm form-text-input\" type=\"text\" name=\"value\" id=\"value\" value=\"" + Registrant[component.UId] + "\" data-component-type=\"" + component.Type.GetTagValue() + "\" data-component-required=\"" + component.Required + "\" data-component-regex=\"" + component.RegexPattern.GetRgxValue() + "\" />";
                    break;
            }
            html += "</div>";
            return new MvcHtmlString(html);
        }

        public MvcHtmlString RenderEditComponent(FreeText component)
        {
            var html = "";
            html = "<div class=\"col-xs-12 form-component\">" + component.Html + "</div>";
            return new MvcHtmlString(html);
        }

        public MvcHtmlString RenderEditComponent(RadioGroup component)
        {
            Index++;
            var html = "";
            html += "<div class=\"col-md-6 form-component" + (component.Required ? " required-component" : "") + "\" data-component-type=\"radiogroup\" data-component-index=\"" + Index + "\">";
            if (Errors.ContainsKey(component.UId))
            {
                html += "<div class=\"form-component-error\"><span class=\"glyphicon glyphicon-warning-sign\"></span>" + Errors[component.UId] + "</div>";
            }
            component.DisplayComponentOrder.Items.Sort((a, b) => a.Order.CompareTo(b.Order));
            var price = PriceGroup.DisplayPrice(component, Registrant);
            foreach (var item in component.DisplayComponentOrder.Items)
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
            component.Items.Sort((a, b) => a.Order.CompareTo(b.Order));
            foreach (var item in component.Items)
            {
                html += RenderEditComponent((Component)item, null);
            }
            html += "</div></div>";

            return new MvcHtmlString(html);
        }

        public MvcHtmlString RenderEditComponent(RadioItem component)
        {
            var html = "";
            html += "<div class=\"form-item" + (component.Required ? " required-item" : "") + "\"><div class=\"form-radio radio\"><label>";
            if (Errors.ContainsKey(component.UId))
            {
                html += "<div class=\"form-component-error\"><span class=\"glyphicon glyphicon-warning-sign\"></span>" + Errors[component.UId] + "</div>";
            }
            component.DisplayComponentOrder.Items.Sort((a, b) => a.Order.CompareTo(b.Order));
            var price = PriceGroup.DisplayPrice(component, Registrant);
            Guid selected = Guid.Empty;
            Guid.TryParse(Registrant[component.ParentKey], out selected);
            html += "<input type=\"radio\" name=\"value\" value=\"" + component.UId + "\"" + (selected == component.UId ? " checked=true" : "") + " data-parent=\"" + component.RadioGroupKey + "\" />";
            foreach (var item in component.DisplayComponentOrder.Items)
            {
                switch (item.Item.ToLower())
                {
                    case "label":
                        var labelStyleList = component.Styles.Where(s => s.GroupName == ".form-component-label").ToList();
                        var labelStyle = "";
                        foreach (var style in labelStyleList)
                            labelStyle += style.Variable + ":" + style.Value + ";";
                        html += "<div class=\"form-item-row\"><span class=\"form-item-label\"" + (String.IsNullOrEmpty(labelStyle) ? "" : " style=\"" + labelStyle + "\"") + ">" + component.LabelText + "</span></div>";
                        break;
                    case "description":
                    case "alttext":
                        var descriptionStyleList = component.Styles.Where(s => s.GroupName == ".form-component-label").ToList();
                        var descriptionStyle = "";
                        foreach (var style in descriptionStyleList)
                            descriptionStyle += style.Variable + ":" + style.Value + ";";
                        html += "<div class=\"form-item-row\"><span class=\"form-item-description\"" + (String.IsNullOrEmpty(descriptionStyle) ? "" : " style=\"" + descriptionStyle + "\"") + ">" + component.AltText + "</span></div>";
                        break;
                    case "date":
                        var displayDate = "";
                        var multipleDays = component.AgendaEnd.Date != component.AgendaStart.Date;
                        var showDate = component.DisplayAgendaDate | multipleDays;
                        if (showDate)
                            displayDate = component.AgendaStart.ToString(Form.Culture) + " - " + component.AgendaEnd.ToString(Form.Culture);
                        else
                            displayDate = component.AgendaStart.DateTime.ToShortTimeString() + " - " + component.AgendaEnd.DateTime.ToShortTimeString();
                        html += "<div class=\"form-item-row\"><span class=\"form-agenda\">" + displayDate + "</span></div>";
                        break;
                    case "price":
                        if (!price.HasValue)
                            break;
                        html += "<div class=\"form-item-row\"><span class=\"form-price\">" + price.Value.ToString("c", Form.Culture) + "</span></div>";
                        break;
                }
            }
            html += "</div></div>";
            return new MvcHtmlString(html);

        }

        public MvcHtmlString RenderEditComponent(DropdownGroup component)
        {
            Index++;
            var html = "";
            html += "<div class=\"col-sm-6 col-md-4 col-lg-3 form-component" + (component.Required ? " required-component" : "") + "\" data-component-type=\"dropdown\" data-component-index=\"" + Index + "\">";
            if (Errors.ContainsKey(component.UId))
            {
                html += "<div class=\"form-component-error\"><span class=\"glyphicon glyphicon-warning-sign\"></span>" + Errors[component.UId] + "</div>";
            }
            component.DisplayComponentOrder.Items.Sort((a, b) => a.Order.CompareTo(b.Order));
            var price = PriceGroup.DisplayPrice(component, Registrant);
            foreach (var item in component.DisplayComponentOrder.Items)
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
            html += "<div class=\"form-items\"><select name=\"value\" id=\"value\" class=\"form-control input-sm form-select\">";
            component.Items.Sort((a, b) => a.Order.CompareTo(b.Order));
            var selected = Guid.Empty;
            bool hasValue = Guid.TryParse(Registrant[component.UId], out selected);
            html += "<option value=\"\"" + (hasValue ? "" : " selected=true") + "></option>";
            foreach (var item in component.Items)
            {
                html += RenderEditComponent((Component)item, null);
            }
            html += "</select></div></div>";

            return new MvcHtmlString(html);
        }

        public MvcHtmlString RenderEditComponent(DropdownItem component)
        {
            var selected = Guid.Empty;
            bool hasValue = Guid.TryParse(Registrant[component.ParentKey], out selected);
            return new MvcHtmlString("<option value=\"" + component.UId + "\"" + (selected == component.UId ? " selected=true" : "") + ">" + component.LabelText + "</option>");
        }

        public MvcHtmlString RenderEditComponent(CheckboxGroup component)
        {
            Index++;
            var html = "";
            html += "<div class=\"col-md-6 form-component" + (component.Required ? " required-component" : "") + "\" data-component-type=\"checkbox\" data-component-index=\"" + Index + "\" data-component-timeexclusion=\"" + component.TimeExclusion + "\">";
            if (Errors.ContainsKey(component.UId))
            {
                html += "<div class=\"form-component-error\"><span class=\"glyphicon glyphicon-warning-sign\"></span>" + Errors[component.UId] + "</div>";
            }
            component.DisplayComponentOrder.Items.Sort((a, b) => a.Order.CompareTo(b.Order));
            var price = PriceGroup.DisplayPrice(component, Registrant);
            foreach (var item in component.DisplayComponentOrder.Items)
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
            html += "<input data-component-type=\"checkboxgroup\" data-component-timeexclusion=\"" + component.TimeExclusion + "\" id=\"" + component.UId + "\" type=\"hidden\" name=\"value\" id=\"value\" value='" + Registrant[component.UId] + "'><div class=\"form-items\">";
            component.Items.Sort((a, b) => a.Order.CompareTo(b.Order));
            foreach (var item in component.Items)
            {
                html += RenderEditComponent((Component)item, null);
            }
            html += "</div></div>";

            return new MvcHtmlString(html);
        }

        public MvcHtmlString RenderEditComponent(CheckboxItem component)
        {
            var html = "";
            html += "<div class=\"form-item" + (component.Required ? " required-item" : "") + "\"><div class=\"form-checkbox checkbox\"><label>";
            if (Errors.ContainsKey(component.UId))
            {
                html += "<div class=\"form-component-error\"><span class=\"glyphicon glyphicon-warning-sign\"></span>" + Errors[component.UId] + "</div>";
            }
            component.DisplayComponentOrder.Items.Sort((a, b) => a.Order.CompareTo(b.Order));
            var price = PriceGroup.DisplayPrice(component, Registrant);
            html += "<input type=\"checkbox\" " + (component.CheckboxGroup.TimeExclusion ? "data-agenda-start=\"" + component.AgendaStart.ToString() + "\" data-agenda-end=\"" + component.AgendaEnd.ToString() + "\"" : "") + " data-parent=\"" + component.ParentKey.ToString() + "\" value=\"" + component.UId + "\"" + (JsonConvert.DeserializeObject<List<Guid>>(Registrant[component.ParentKey] ?? "[]").Contains(component.UId) ? " checked=true" : "") + " data-component-required=\"" + (component.Required ? "true" : "false") + "\" />";
            foreach (var item in component.DisplayComponentOrder.Items)
            {
                switch (item.Item.ToLower())
                {
                    case "label":
                        var labelStyleList = component.Styles.Where(s => s.GroupName == ".form-component-label").ToList();
                        var labelStyle = "";
                        foreach (var style in labelStyleList)
                            labelStyle += style.Variable + ":" + style.Value + ";";
                        html += "<div class=\"form-item-row\"><span class=\"form-item-label\"" + (String.IsNullOrEmpty(labelStyle) ? "" : " style=\"" + labelStyle + "\"") + ">" + component.LabelText + "</span></div>";
                        break;
                    case "description":
                    case "alttext":
                        var descriptionStyleList = component.Styles.Where(s => s.GroupName == ".form-component-label").ToList();
                        var descriptionStyle = "";
                        foreach (var style in descriptionStyleList)
                            descriptionStyle += style.Variable + ":" + style.Value + ";";
                        html += "<div class=\"form-item-row\"><span class=\"form-item-description\"" + (String.IsNullOrEmpty(descriptionStyle) ? "" : " style=\"" + descriptionStyle + "\"") + ">" + component.AltText + "</span></div>";
                        break;
                    case "date":
                        var displayDate = "";
                        var multipleDays = component.AgendaEnd.Date != component.AgendaStart.Date;
                        var showDate = component.DisplayAgendaDate | multipleDays;
                        if (showDate)
                            displayDate = component.AgendaStart.ToString(Form.Culture) + " - " + component.AgendaEnd.ToString(Form.Culture);
                        else
                            displayDate = component.AgendaStart.DateTime.ToShortTimeString() + " - " + component.AgendaEnd.DateTime.ToShortTimeString();
                        html += "<div class=\"form-item-row\"><span class=\"form-agenda\">" + displayDate + "</span></div>";
                        break;
                    case "price":
                        if (!price.HasValue)
                            break;
                        html += "<div class=\"form-item-row\"><span class=\"form-price\">" + price.Value.ToString("c", Form.Culture) + "</span></div>";
                        break;
                }
            }
            html += "</div></div>";
            return new MvcHtmlString(html);

        }



        #endregion
    }
}