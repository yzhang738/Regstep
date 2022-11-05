using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using RSToolKit.Domain.Entities.Components;
using RSToolKit.Domain.Entities;
using RSToolKit.Domain.Data;
using Newtonsoft.Json;

namespace RSToolKit.WebUI.Infrastructure
{
    public static class HtmlComponentBuilder
    {

        private static readonly string ItemOpen = "<div class=\"form-component-row\">";
        private static readonly string ItemClose = "</div>";

        #region Component Rendering

        public static string Render(Component component, Registrant reg)
        {
            if (component is Input)
                return Render(component as Input, reg);
            else if (component is RadioGroup)
                return Render(component as RadioGroup, reg);
            else if (component is CheckboxGroup)
                return Render(component as CheckboxGroup, reg);
            else if (component is DropdownGroup)
                return Render(component as DropdownGroup, reg);
            else if (component is RatingSelect)
                return Render(component as RatingSelect, reg);
            else if (component is NumberSelector)
                return Render(component as NumberSelector, reg);
            return "";
        }

        public static string Render(Input component, Registrant reg)
        {
            var html = "<div class=\"col-sm-12\">";
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
                    html += "<textarea id=\"" + component.SortingId + "\" class=\"form-control input-sm form-text-area\">" + reg.SearchPrettyValue(component.UId.ToString()) + "</textarea>";
                    break;
                case Domain.Entities.Components.InputType.Password:
                    html += "<input id=\"" + component.SortingId + "\" class=\"form-control input-sm form-text-input\" type=\"password\" value=\"\" />";
                    break;
                case Domain.Entities.Components.InputType.UniversalCreditCard:
                    Guid cardId;
                    var peek = "";
                    var datapoint = reg.Data.Where(dp => dp.VariableUId == component.UId).FirstOrDefault();
                    if (datapoint != null && Guid.TryParse(datapoint.Value, out cardId))
                    {
                        using (var repo = new FormsRepository())
                        {
                            var card = repo.Search<RSToolKit.Domain.Entities.MerchantAccount.CreditCard>(c => c.UId == cardId).FirstOrDefault();
                            if (card != null)
                                peek = card.Number;
                        }
                    }
                    html += "<input id=\"" + component.SortingId + "\" class=\"form-control input-sm form-text-input\" type=\"text\" value=\"" + peek + "\" />";
                    break;
                case Domain.Entities.Components.InputType.File:
                    var inputData = reg.Data.Where(d => d.VariableUId == component.UId).FirstOrDefault();
                    if (inputData != null && inputData.File != null)
                    {
                        if (inputData.File.FileType.ToLower().StartsWith("image"))
                        {
                            html += "<div class=\"form-file\">";
                            html += "<a href=\"/Registrant/File/" + inputData.UId + "\">";
                            html += "<img class=\"form-file\" src=\"/Registrant/Thumbnail/" + reg.UId + "?component=" + component.UId + "\" />";
                            html += "</a>";
                            html += "</div>";
                        }
                        else
                        {
                            html += "<div class=\"form-file\">";
                            html += "<a href=\"/Registrant/File/" + inputData.UId + "\">";
                            html += "<span class=\"glyphicon glyphicon-cloud-download\"></span> Download";
                            html += "</a>";
                            html += "</div>";
                        }
                    }
                    html += "<input id=\"" + component.SortingId + "\" class=\"form-text-input\" type=\"file\" name=\"" + component.UId + "\"/></div>";
                    break;
                case Domain.Entities.Components.InputType.Date:
                    html += "<input id=\"" + component.SortingId + "\"  value=\"" + reg[component.UId] + "\" class=\"form-control input-sm form-text-input\" type=\"text\" data-component-type=\"datetime\" data-input-type=\"date\" name=\"" + component.UId + "\"" + (component.MinDate.HasValue ? "  data-date-startDate=\"" + component.MinDate.Value.ToShortDateString() + "\"" : "") + (component.MaxDate.HasValue ? " data-date-endDate=\"" + component.MaxDate.Value.ToShortDateString() + "\"" : "") + " data-min-view=\"2\" data-date-format=\"m/d/yyyy\"/>";
                    break;
                case Domain.Entities.Components.InputType.DateTime:
                    html += "<input id=\"" + component.SortingId + "\" value=\"" + reg[component.UId] + "\" class=\"form-control input-sm form-text-input\" type=\"text\"data-component-type=\"datetime\" name=\"" + component.UId + "\" " + (component.MinDate.HasValue ? " data-date-startDate=\"" + component.MinDate.Value.ToString("M/d/yyy h/mm tt") + "\"" : "") + (component.MaxDate.HasValue ? " data-date-endDate=\"" + component.MaxDate.Value.ToString("M/d/yyy h/mm tt") + "\"" : "") + " data-date-showmeridian=\"true\" data-date-format=\"m/d/yyyy H:ii P\"/>";
                    break;
                case Domain.Entities.Components.InputType.Time:
                    html += "<input id=\"" + component.SortingId + "\"  value=\"" + reg[component.UId] + "\" class=\"form-control input-sm form-text-input\" type=\"text\" data-date-pickDate=\"false\" name=\"" + component.UId + "\" data-component-type=\"datetime\" data-date-startView=\"0\" data-date-maxView=\"0\" data-date-showmeridian=\"true\" data-date-format=\"H:ii P\"/>";
                    break;
                default:
                    html += "<input id=\"" + component.SortingId + "\" class=\"form-control input-sm form-text-input\" type=\"text\"  value=\"" + reg[component.UId] + "\" />";
                    break;
            }
            return html;
        }

        public static string Render(RadioGroup component, Registrant reg)
        {
            var data = reg.Data.FirstOrDefault(d => d.Component.UId == component.UId);
            var html = "<div class=\"col-sm-12 form-component\" data-component-type=\"radiogroup\">";
            var price = PriceGroup.DisplayPrice(component, reg);
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
                            displayDate = component.AgendaStart.ToString(reg.Form.Culture) + " - " + component.AgendaEnd.ToString(reg.Form.Culture);
                        else
                            displayDate = component.AgendaStart.DateTime.ToShortTimeString() + " - " + component.AgendaEnd.DateTime.ToShortTimeString();
                        html += ItemOpen + "<span class=\"form-agenda\">" + displayDate + "</span>" + ItemClose;
                        break;
                    case "price":
                        if (!price.HasValue)
                            break;
                        html += "<div class=\"form-item-row\"><span class=\"form-price\">" + price.Value.ToString("c", reg.Form.Culture) + "</span></div>";
                        break;
                }
            }
            var itemHtml = "";
            foreach (var item in component.Items.OrderBy(i => i.Order))
            {
                itemHtml += RenderRadioItem(item, data);
            }
            html += itemHtml;
            html += "<input type=\"hidden\" id=\"" + component.SortingId + "\" value=\"" + component.UId + "\" />";
            component.Items.Sort((a, b) => a.Order.CompareTo(b.Order));
            html += "</div></div>";

            return html;
        }

        public static string RenderRadioItem(RadioItem item, RegistrantData data)
        {
            var reg = data.Registrant;
            Guid selected = Guid.Empty;
            if (data != null)
                Guid.TryParse(data.Value, out selected);
            var seating = item.Seating;
            var seated = true;
            var waitlisted = false;
            var full = false;
            var html = "";
            html += "<div class=\"form-item" + (item.Required ? " required-item" : "") + "\"><div class=\"form-radio radio\">";
            html += "<label>";
            var price = PriceGroup.DisplayPrice(item, reg);
            // We need to check seating.
            html += "<input id=\"" + item.UId + "\" class=\"input-outside\" type=\"radio\" name=\"radio_reg\" value=\"" + item.UId + "\"" + (selected == item.UId ? " checked=\"checked\"" : "") + " data-parent=\"" + item.RadioGroup.SortingId + "\" />";
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
                            displayDate = item.AgendaStart.ToString(reg.Form.Culture) + " - " + item.AgendaEnd.ToString(reg.Form.Culture);
                        else
                            displayDate = item.AgendaStart.DateTime.ToShortTimeString() + " - " + item.AgendaEnd.DateTime.ToShortTimeString();
                        html += "<div class=\"form-item-row\"><span class=\"form-agenda\">" + displayDate + "</span></div>";
                        break;
                    case "price":
                        if (!price.HasValue)
                            break;
                        html += "<div class=\"form-item-row\"><span class=\"form-price\">" + price.Value.ToString("c", reg.Form.Culture) + "</span></div>";
                        break;
                }
            }
            html += "</label>";
            html += "</div></div>";
            return html;
        }

        public static string Render(CheckboxGroup component, Registrant reg)
        {
            var data = reg.Data.FirstOrDefault(d => d.Component.UId == component.UId);
            if (data == null)
                data = new RegistrantData();
            data.Component = component;
            var html = "";
            html += "<div class=\"col-sm-12 form-component\" data-component-type=\"checkbox\" >";
            var price = PriceGroup.DisplayPrice(component, reg);
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
                            displayDate = component.AgendaStart.ToString(reg.Form.Culture) + " - " + component.AgendaEnd.ToString(reg.Form.Culture);
                        else
                            displayDate = component.AgendaStart.DateTime.ToShortTimeString() + " - " + component.AgendaEnd.DateTime.ToShortTimeString();
                        html += ItemOpen + "<span class=\"form-agenda\">" + displayDate + "</span>" + ItemClose;
                        break;
                    case "price":
                        if (!price.HasValue)
                            break;
                        html += "<div class=\"form-item-row\"><span class=\"form-price\">" + price.Value.ToString("c", reg.Form.Culture) + "</span></div>";
                        break;
                }
            }
            var itemHtml = "";
            foreach (var item in component.Items.OrderBy(i => i.Order))
            {
                itemHtml += RenderCheckboxItem(item, data);
            }
            html += itemHtml;
            html += "</div><input id=\"" + component.SortingId + "\" type=\"hidden\" value='" + data.Value + "'></div>";
            return html;
        }

        public static string RenderCheckboxItem(CheckboxItem item, RegistrantData data)
        {
            var reg = data.Registrant;
            var selected = false;
            var selections = new List<Guid>();
            if (data != null)
                selections = JsonConvert.DeserializeObject<List<Guid>>(data.Value);
            selected = selections.Contains(item.UId);
            var html = "";
            html += "<div class=\"form-item\"><div class=\"form-checkbox checkbox\">";
            html += "<label>";
            var price = PriceGroup.DisplayPrice(item, reg);
            html += "<input id=\"" + item.UId + "\"" + (selected ? " checked=\"checked\"" : "") + " data-parent=\"" + item.CheckboxGroup.SortingId + "\" type=\"checkbox\" />";
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
                            displayDate = item.AgendaStart.ToString(reg.Form.Culture) + " - " + item.AgendaEnd.ToString(reg.Form.Culture);
                        else
                            displayDate = item.AgendaStart.DateTime.ToShortTimeString() + " - " + item.AgendaEnd.DateTime.ToShortTimeString();
                        html += "<div class=\"form-item-row\"><span class=\"form-agenda\">" + displayDate + "</span></div>";
                        break;
                    case "price":
                        if (!price.HasValue)
                            break;
                        html += "<div class=\"form-item-row\"><span class=\"form-price\">" + price.Value.ToString("c", reg.Form.Culture) + "</span></div>";
                        break;
                }
            }
            html += "</label>";
            html += "</div></div>";
            return html;
        }

        public static string Render(DropdownGroup component, Registrant reg)
        {
            var data = reg.Data.FirstOrDefault(d => d.Component.UId == component.UId);
            data = data ?? new RegistrantData()
            {
                Registrant = reg,
                Component = component,
                UId = Guid.NewGuid()
            };
            var html = "";
            html += "<div class=\"col-sm-6 col-md-4 col-lg-3 form-component\">";
            var price = PriceGroup.DisplayPrice(component, reg);
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
                            displayDate = component.AgendaStart.ToString(reg.Form.Culture) + " - " + component.AgendaEnd.ToString(reg.Form.Culture);
                        else
                            displayDate = component.AgendaStart.DateTime.ToShortTimeString() + " - " + component.AgendaEnd.DateTime.ToShortTimeString();
                        html += ItemOpen + "<span class=\"form-agenda\">" + displayDate + "</span>" + ItemClose;
                        break;
                    case "price":
                        if (!price.HasValue)
                            break;
                        html += "<div class=\"form-item-row\"><span class=\"form-price\">" + price.Value.ToString("c", reg.Form.Culture) + "</span></div>";
                        break;
                }
            }
            var itemHtml = "";
            foreach (var item in component.Items.OrderBy(i => i.Order))
                itemHtml += RenderDropdownItem(item, data);
            html += "<div class=\"form-items\"><select id=\"" + component.SortingId + "\" class=\"form-control input-sm form-select\">";
            html += "<option value=\"\"></option>";
            html += itemHtml;
            html += "</select></div></div>";

            return html;
        }

        public static string RenderDropdownItem(DropdownItem item, RegistrantData data)
        {
            var reg = data.Registrant;
            var seating = item.Seating;
            var full = false;
            Guid selected = Guid.Empty;
            if (data != null)
                Guid.TryParse(data.Value, out selected);
            return "<option value=\"" + item.UId + "\"" + (selected == item.UId ? " selected=true" : "") + (full ? " disabled=\"true\"" : "") + ">" + item.LabelText + (full ? " FULL" : "") + "</option>";
        }

        public static string Render(RatingSelect component, Registrant reg)
        {
            var html = "";
            if (reg.Form.ParentForm == null || component.MappedComponent == null)
            {
                var dataPoint = reg.Data.FirstOrDefault(d => d.VariableUId == component.UId);
                html += "<div class=\"col-sm-6 col-md-4 col-lg-3 form-component\" >";
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
                html += "<input class=\"rating form-control input-sm form-text-input\" type=\"password\" id=\"" + component.SortingId + "\" value=\"" + (dataPoint != null ? dataPoint.Value : "") + "\" />";
                html += "</div>";
            }
            else
            {
                var formRegistrant = reg.Form.ParentForm.Registrants.Where(r => r.Type == RegistrationType.Live && r.Status == RegistrationStatus.Submitted && r.Email.ToLower() == reg.Email.ToLower()).FirstOrDefault();
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
                        var dataPoint = reg.Data.FirstOrDefault(d => d.VariableUId == selection);
                        var t_item = (mappedComponent as IComponentItemParent).Children.FirstOrDefault(i => i.UId == selection);
                        if (t_item != null)
                        {
                            html += "<div class=\"col-sm-12 form-component\" >";
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
                            html += "<input id=\"" + component.SortingId + "\" class=\"rating form-control input-sm form-text-input\" type=\"text\" data-showClear=\"false\" data-showCaption=\"false\" data-size=\"sm\" value=\"" + (dataPoint != null ? dataPoint.Value : "") + "\" />";
                            html += "</div>";
                        }
                    }
                }
            }
            return html;
        }

        public static string Render(NumberSelector component, Registrant reg)
        {
            var data = reg.Data.FirstOrDefault(d => d.Component.UId == component.UId);
            var html = "";
            html += "<div class=\"col-sm-12 form-component\" data-component-type=\"dropdown\" >";
            var price = PriceGroup.DisplayPrice(component, reg);
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
                            displayDate = component.AgendaStart.ToString(reg.Form.Culture) + " - " + component.AgendaEnd.ToString(reg.Form.Culture);
                        else
                            displayDate = component.AgendaStart.DateTime.ToShortTimeString() + " - " + component.AgendaEnd.DateTime.ToShortTimeString();
                        html += ItemOpen + "<span class=\"form-agenda\">" + displayDate + "</span>" + ItemClose;
                        break;
                    case "price":
                        if (!price.HasValue)
                            break;
                        html += "<div class=\"form-item-row\"><span class=\"form-price\">" + price.Value.ToString("c", reg.Form.Culture) + "</span></div>";
                        break;
                }
            }
            var itemHtml = "";
            var t_value = (data != null ? data.Value : "0");
            for (var i = component.Min; i <= component.Max; i++)
                itemHtml += "<option value=" + i + (t_value == i.ToString() ? " selected=\"selected\"" : "") + ">" + i + "</option>";
            html += "<div class=\"form-items\"><select id=\"" + component.SortingId + "\" class=\"form-control input-sm form-select\">";
            html += "<option value=\"\"></option>";
            html += itemHtml;
            html += "</select></div></div>";

            return html;
        }


        #endregion

    }
}