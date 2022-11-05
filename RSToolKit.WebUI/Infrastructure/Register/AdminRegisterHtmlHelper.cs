using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text.RegularExpressions;
using RSToolKit.Domain.Entities;
using RSToolKit.Domain.Data;
using RSToolKit.Domain.JItems;
using RSToolKit.Domain.Engines;
using Newtonsoft.Json;
using RSToolKit.Domain.Entities.Components;
using RSToolKit.Domain;

namespace RSToolKit.WebUI.Infrastructure.Register
{
    public class AdminRegisterHtmlHelper
        : IDisposable, IRegisterHtmlHelper
    {
        #region Fields and Properties
        /// <summary>
        /// The opening tag for a component.
        /// </summary>
        protected static readonly string st_itemOpen = "<div class=\"form-component-row\">";
        /// <summary>
        /// The closing tag for a component
        /// </summary>
        protected static readonly string st_itemClose = "</div>";
        /// <summary>
        /// The error message holder.
        /// </summary>
        protected static readonly string st_error = "<div class=\"form-messagebox form-error-message\" style=\"display: none;\"><div class=\"form-messagebox-message\"><div id=\"{0}\"><span class=\"glyphicon glyphicon-warning\"></span></div></div></div>";
        /// <summary>
        /// The value styles for regex.
        /// </summary>
        protected static readonly Regex st_valueToStyle = new Regex(@"^(?<group>[^_]*)_(?<variable>.*)$");
        /// <summary>
        /// The index of the component.
        /// </summary>
        protected int _index;
        /// <summary>
        /// The index of the waitlist.
        /// </summary>
        protected int _waitlistIndex;
        /// <summary>
        /// The registrant used for rendering.
        /// </summary>
        public Registrant Registrant { get; protected set; }
        /// <summary>
        /// The form being rendered.
        /// </summary>
        public Form Form { get; protected set; }
        /// <summary>
        /// The page being rendered.
        /// </summary>
        public Page Page { get; protected set; }
        /// <summary>
        /// The data errors.
        /// </summary>
        public List<SetDataError> Errors { get; protected set; }
        /// <summary>
        /// The context to use.
        /// </summary>
        public EFDbContext Context { get; protected set; }
        /// <summary>
        /// The dictionary of synchronized logic.
        /// </summary>
        public Dictionary<string, IEnumerable<JLogicCommand>> Commands { get; protected set; }
        /// <summary>
        /// Determines if this is for admin registration.
        /// </summary>
        public bool AdminRegistration { get; protected set; }
        /// <summary>
        /// Whether the helper is being used to edit single components.
        /// </summary>
        public bool Editing { get; protected set; }
        /// <summary>
        /// Whether the sorting id should be used for the id of the html input.
        /// </summary>
        public bool UseSortingId { get; protected set; }
        #endregion

        #region Contsructors
        /// <summary>
        /// Initializes the class with minimal access.
        /// </summary>
        /// <param name="form">The form to use.</param>
        public AdminRegisterHtmlHelper(Form form, EFDbContext context, bool adminRegistration = true, bool editing = false, bool useSortingId = false)
        {
            AdminRegistration = adminRegistration;
            UseSortingId = useSortingId;
            Editing = editing;
            Registrant = null;
            Form = form;
            Errors = null;
            Context = context;
            this._index = -1;
            this._waitlistIndex = -1;
            Commands = new Dictionary<string, IEnumerable<JLogicCommand>>();
        }

        /// <summary>
        /// Initializes the class.
        /// </summary>
        /// <param name="registrant">The registrant being used.</param>
        /// <param name="page">The page being rendered.</param>
        /// <param name="commands">The synchronized logic commands.</param>
        /// <param name="errors">The errors if any are present.</param>
        public AdminRegisterHtmlHelper(Registrant registrant, int page, EFDbContext context, List<SetDataError> errors = null, bool adminRegistration = false, bool editing = false, bool useSortingId = false)
        {
            AdminRegistration = adminRegistration;
            UseSortingId = useSortingId;
            Editing = editing;
            Registrant = registrant;
            Form = registrant.Form;
            Errors = errors ?? new List<SetDataError>();
            Context = context;
            this._index = -1;
            this._waitlistIndex = -1;
            Page = Form.Pages.FirstOrDefault(p => p.PageNumber == page);
            Commands = new Dictionary<string, IEnumerable<JLogicCommand>>();
        }

        /// <summary>
        /// Sets the current page.
        /// </summary>
        /// <param name="page"></param>
        public void SetPage(int page)
        {
            Page = Form.Pages.FirstOrDefault(p => p.PageNumber == page);
        }
        #endregion

        #region Form Methods
        /// <summary>
        /// Renders the page numbers.
        /// </summary>
        /// <returns>The html string.</returns>
        public HtmlString RenderPageNumbers()
        {
            if (Page != null)
                return new HtmlString(Page.PageNumber + " of " + Page.Form.Pages.OrderBy(p => p.PageNumber).Last().PageNumber);
            return new HtmlString("");
        }

        /// <summary>
        /// Renders the form styles.
        /// </summary>
        /// <returns>The html string.</returns>
        public HtmlString RenderFormStyle()
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
                    if (st_valueToStyle.IsMatch(fs.Value))
                    {
                        var match = st_valueToStyle.Match(fs.Value);
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
            return new HtmlString(style);
        }

        /// <summary>
        /// Renders the shopping cart.
        /// </summary>
        /// <returns>The shopping cart to render.</returns>
        public HtmlString RenderShoppingCart()
        {
            if (Registrant == null)
                return new HtmlString("");
            var html = "";
            if (Registrant == null)
                return new HtmlString(html);
            if (Form.DisableShoppingCart || Form.Survey)
                return new HtmlString(html);
            var total = Registrant.Fees + Registrant.Taxes + Registrant.Adjustings - Registrant.Transactions;
            html = "Shopping Cart: " + total.ToString("c", Form.Culture);
            return new HtmlString(html);
        }

        /// <summary>
        /// Renders hidden inputs for processing Post and Put Http requests.
        /// </summary>
        /// <returns>The html string.</returns>
        public HtmlString RenderHiddens()
        {
            var html = "";
            if (Page != null)
                html += @"<input type=""hidden"" id=""PageKey"" name=""PageKey"" value=""" + Page.UId + @""" /><input type=""hidden"" id=""PageNumber"" name=""PageNumber"" value=""" + Page.PageNumber + @""" />";
            if (Registrant != null)
                html += @"<input type=""hidden"" id=""RegistrantKey"" name=""RegistrantKey"" value=""" + Registrant.UId + @""" />";
            if (Form != null)
                html += @"<input type=""hidden"" id=""FormKey"" name=""FormKey"" value=""" + Form.UId + @""" />";
            return new HtmlString(html);
        }

        /// <summary>
        /// Renders the footer for the form.
        /// </summary>
        /// <returns>The html string.</returns>
        public HtmlString RenderFooter()
        {
            return new HtmlString(this._Parse(Form.Footer));
        }

        /// <summary>
        /// Renders the header for the form.
        /// </summary>
        /// <returns>The html string.</returns>
        public HtmlString RenderHeader()
        {
            return new HtmlString(this._Parse(Form.Header));
        }

        #endregion

        #region IsBlank Methods

        /// <summary>
        /// Checks to see if the form item is blank.
        /// </summary>
        /// <typeparam name="T">The type of form item.</typeparam>
        /// <param name="item">The form item.</param>
        /// <returns>True if blank, false otherwise.</returns>
        public bool IsBlank<T>(T item)
            where T : IFormComponent
        {
            if (item is Page)
                return this._PageIsBlank(item as Page);
            else if (item is Panel)
                return this._PanelIsBlank(item as Panel);
            else if (item is IComponent)
                return this._ComponentIsBlank(item as IComponent);
            else
                return false;
        }

        /// <summary>
        /// Checks to see if the page is blank for the registrant.
        /// </summary>
        /// <param name="page">The page to check. Defaults to the current page.</param>
        /// <returns>True if blank, false otherwise.</returns>
        protected bool _PageIsBlank(Page page = null)
        {
            page = page ?? Page;
            if (!page.Enabled)
                return true;
            if ((Registrant.RSVP && page.RSVP == RSVPType.Decline) || (!Registrant.RSVP && page.RSVP == RSVPType.Accept))
                return true;
            if (page.Audiences.Count > 0 && (Registrant.Audience == null || !page.Audiences.Contains(Registrant.Audience)))
                return true;
            var commands = this._ExecuteLogic(page);
            foreach (var command in commands)
            {
                switch (command.Command)
                {
                    case JLogicWork.PageSkip:
                    case JLogicWork.Hide:
                        return true;
                }
            }
            var blank = true;
            foreach (var panel in page.Panels)
                blank &= this._PanelIsBlank(panel);
            return blank;
        }

        /// <summary>
        /// Checks to see if a panel is blank.
        /// </summary>
        /// <param name="panel">The panel to check for being blank.</param>
        /// <returns>True if blank, false otherwise.</returns>
        protected bool _PanelIsBlank(Panel panel)
        {
            if (!panel.Enabled)
                return true;
            if ((Registrant.RSVP && panel.RSVP == RSVPType.Decline) || (!Registrant.RSVP && panel.RSVP == RSVPType.Accept))
                return true;
            if (panel.Audiences.Count > 0 && (Registrant.Audience == null || !panel.Audiences.Contains(Registrant.Audience)))
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
                blank &= this._ComponentIsBlank(component);
            return blank;
        }

        /// <summary>
        /// Checks to see if a component is blank for the registrant.
        /// </summary>
        /// <param name="component">The component to check that is blank.</param>
        /// <returns>True if blank, false otherwise.</returns>
        protected bool _ComponentIsBlank(IComponent component)
        {
            if ((Registrant.RSVP && component.RSVP == RSVPType.Decline) || (!Registrant.RSVP && component.RSVP == RSVPType.Accept))
                return true;
            if (component.Audiences.Count > 0 && (Registrant.Audience == null || !component.Audiences.Contains(Registrant.Audience)))
                return true;
            if (!component.Enabled)
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
                    blank &= this._ComponentItemIsBlank(item);
                return blank;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Checks to see if a component item is blank.
        /// </summary>
        /// <param name="component"></param>
        /// <returns>True if blank, false otherwise.</returns>
        protected bool _ComponentItemIsBlank(IComponentItem component)
        {
            if ((Registrant.RSVP && component.RSVP == RSVPType.Decline) || (!Registrant.RSVP && component.RSVP == RSVPType.Accept))
                return true;
            if (component.Audiences.Count > 0 && (Registrant.Audience == null || !component.Audiences.Contains(Registrant.Audience)))
                return true;
            if (!component.Enabled)
            {
                var dp = Registrant.Data.FirstOrDefault(d => d.Component.UId == component.ParentKey);
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

        #endregion

        #region Component Rendering

        /// <summary>
        /// Checks to see if the component is being skipped.
        /// </summary>
        /// <param name="component"></param>
        /// <param name="comparer"></param>
        /// <returns></returns>
        protected ComponentSkipResult _TestSkip(IComponent component, List<Guid> comparer = null)
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
            return skip;
        }

        /// <summary>
        /// Renders form errors.
        /// </summary>
        /// <param name="id">The id of the component to render errors for.</param>
        /// <returns>The errors.</returns>
        protected HtmlString RenderErrors(string id)
        {
            return new HtmlString(String.Format(st_error, id + "Error"));
        }

        /// <summary>
        /// Renders the html for a component.
        /// </summary>
        /// <param name="component"></param>
        /// <param name="admin"></param>
        /// <param name="editing"></param>
        /// <param name="sortingId"></param>
        /// <returns></returns>
        public HtmlString Render(IComponent component)
        {
            // Check to see if we skip the component.
            var skip = this._TestSkip(component);
            if (skip.Skip && !Editing)
            {
                // We are skipping so we null out values or return nothing in the case of an item.
                if (component is FreeText)
                    return new HtmlString("");
                if (component is IComponentItem)
                    return new HtmlString("");
                if (skip.NullOut)
                {
                    this._index++;
                    var html = @"<input type=""hidden"" name=""Components[" + this._index + @"].Key"" value=""" + component.UId + @""" />";
                    html += @"<input type=""hidden"" name=""Components[" + this._index + @"].Value"" value=""__skipped"" />";
                    return new HtmlString(html);
                }
                return new HtmlString("");
            }
            // Now we need to render the components individually.
            if (component is Input)
                return this._RenderInput(component as Input);
            else if (component is RadioGroup)
                return this._RenderRadioGroup(component as RadioGroup);
            else if (component is CheckboxGroup)
                return this._RenderCheckboxGroup(component as CheckboxGroup);
            else if (component is DropdownGroup)
                return this._RenderDropdownGroup(component as DropdownGroup);
            else if (component is RatingSelect)
                return this._RenderRatingSelect(component as RatingSelect);
            else if (component is FreeText)
                return this._RenderFreeText(component as FreeText);
            else if (component is NumberSelector)
                return this._RenderNumberSelector(component as NumberSelector);
            return new HtmlString("");
        }

        /// <summary>
        /// Renders the Input
        /// </summary>
        /// <param name="component">The input component.</param>
        /// <returns>The html string.</returns>
        protected HtmlString _RenderInput(Input component)
        {
            var t_id = UseSortingId ? component.SortingId.ToString() : component.UId.ToString();
            this._index++;
            var html = "<div class=\"" + (component.Type == RSToolKit.Domain.Entities.Components.InputType.Multiline ? "col-md-6" : "col-sm-6 col-md-4 col-lg-3") + " form-component" + (component.Required && !AdminRegistration ? " required-component" : "") + "\">";
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
                        html += st_itemOpen + "<label class=\"control-label\"><span class=\"form-component-label\"" + (String.IsNullOrEmpty(labelStyle) ? "" : " style=\"" + labelStyle + "\"") + ">" + component.LabelText + "</span></label>" + st_itemClose;
                        break;
                    case "description":
                    case "alttext":
                        var descriptionStyleList = component.Styles.Where(s => s.GroupName == ".form-component-label").ToList();
                        var descriptionStyle = "";
                        foreach (var style in descriptionStyleList)
                            descriptionStyle += style.Variable + ":" + style.Value + ";";
                        html += st_itemOpen + "<span class=\"form-component-description\"" + (String.IsNullOrEmpty(descriptionStyle) ? "" : " style=\"" + descriptionStyle + "\"") + ">" + component.AltText + "</span>" + st_itemClose;
                        break;
                }
            }
            // Render the input.
            switch (component.Type)
            {
                case Domain.Entities.Components.InputType.Multiline:
                    html += "<textarea" + (component.Required && !AdminRegistration ? " data-form-required=\"true\"" : "") + (component.Length.HasValue ? " data-form-max=\"" + component.Length + "\"" : "") + " id=\"" + t_id + "\" name=\"Components[" + this._index + "].Value\" class=\"form-control input-sm form-text-area\">" + Registrant[component.UId] + "</textarea>";
                    break;
                case Domain.Entities.Components.InputType.Password:
                    html += "<input" + (component.Required && !AdminRegistration ? " data-form-required=\"true\"" : "") + (component.Length.HasValue ? " data-form-max=\"" + component.Length + "\"" : "") + " id=\"" + t_id + "\" class=\"form-control input-sm form-text-input\" type=\"password\" name=\"Components[" + this._index + "].Value\" value=\"\" />";
                    break;
                case Domain.Entities.Components.InputType.UniversalCreditCard:
                    Guid cardId;
                    var peek = "";
                    var datapoint = Registrant.Data.Where(dp => dp.VariableUId == component.UId).FirstOrDefault();
                    if (datapoint != null && Guid.TryParse(datapoint.Value, out cardId))
                    {
                        var card = Context.CreditCards.Find(cardId);
                        if (card != null)
                        {
                            if (Editing)
                            {
                                peek = card.Number;
                            }
                            else
                            {
                                peek = card.Number.GetLast(4);
                            }
                        }
                        if (String.IsNullOrEmpty(peek))
                            peek = "";
                    }
                    html += "<input" + (component.Required && !AdminRegistration ? " data-form-required=\"true\"" : "") + " id=\"" + t_id + "\" class=\"form-control input-sm form-text-input\" type=\"text\" name=\"Components[" + this._index + "].Value\" value=\"" + peek + "\" />";
                    break;
                case Domain.Entities.Components.InputType.File:
                    var inputData = Registrant.Data.Where(d => d.VariableUId == component.UId).FirstOrDefault();
                    if (inputData != null && inputData.File != null)
                    {
                        if (inputData.File.FileType.ToLower().StartsWith("image"))
                        {
                            html += "<div class=\"form-file\">";
                            if (Editing)
                                html += "<a href=\"\\Registrant\\File\\" + inputData.UId + "\">";
                            html += "<img class=\"form-file\" src=\"\\Registrant\\Image\\Thumbnail\\" + Registrant.SortingId + "\\" + component.SortingId + "\" />";
                            if (Editing)
                                html += "</a>";
                            html += "</div>";
                        }
                        else
                        {
                            html += "<div class=\"form-file\">";
                            if (Editing)
                                html += "<a href=\"\\Registrant\\File\\" + inputData.UId + "\">";
                            html += "<span class=\"glyphicon glyphicon-cloud-download\"></span> Download";
                            if (Editing)
                                html += "</a>";
                            html += "</div>";
                        }
                    }
                    html += "<input" + (component.Required && !AdminRegistration ? " data-form-required=\"true\"" : "") + " id=\"" + t_id + "\" class=\"form-text-input\" type=\"file\" name=\"" + component.UId + "\"/></div>";
                    this._index--;
                    return new HtmlString(html);
                case Domain.Entities.Components.InputType.Date:
                    html += "<input" + (component.Required && !AdminRegistration ? " data-form-required=\"true\"" : "") + " id=\"" + t_id + "\" name=\"Components[" + this._index + "].Value\" value=\"" + Registrant[component.UId] + "\" class=\"form-control input-sm form-text-input\" type=\"text\" data-component-type=\"datetime\" data-input-type=\"date\" name=\"" + component.UId + "\"" + (component.MinDate.HasValue ? "  data-date-startDate=\"" + component.MinDate.Value.ToShortDateString() + "\"" : "") + (component.MaxDate.HasValue ? " data-date-endDate=\"" + component.MaxDate.Value.ToShortDateString() + "\"" : "") + " data-min-view=\"2\" data-date-format=\"m/d/yyyy\"/>";
                    break;
                case Domain.Entities.Components.InputType.DateTime:
                    html += "<input" + (component.Required && !AdminRegistration ? " data-form-required=\"true\"" : "") + " id=\"" + t_id + "\" name=\"Components[" + this._index + "].Value\" value=\"" + Registrant[component.UId] + "\" class=\"form-control input-sm form-text-input\" type=\"text\"data-component-type=\"datetime\" name=\"" + component.UId + "\" " + (component.MinDate.HasValue ? " data-date-startDate=\"" + component.MinDate.Value.ToString("M/d/yyy h/mm tt") + "\"" : "") + (component.MaxDate.HasValue ? " data-date-endDate=\"" + component.MaxDate.Value.ToString("M/d/yyy h/mm tt") + "\"" : "") + " data-date-showmeridian=\"true\" data-date-format=\"m/d/yyyy H:ii P\"/>";
                    break;
                case Domain.Entities.Components.InputType.Time:
                    html += "<input" + (component.Required && !AdminRegistration ? " data-form-required=\"true\"" : "") + " id=\"" + t_id + "\" name=\"Components[" + this._index + "].Value\" value=\"" + Registrant[component.UId] + "\" class=\"form-control input-sm form-text-input\" type=\"text\" data-date-pickDate=\"false\" name=\"" + component.UId + "\" data-component-type=\"datetime\" data-date-startView=\"0\" data-date-maxView=\"0\" data-date-showmeridian=\"true\" data-date-format=\"H:ii P\"/>";
                    break;
                default:
                    html += "<input" + (component.Required && !AdminRegistration ? " data-form-required=\"true\"" : "") + (component.Type != Domain.Entities.Components.InputType.Default ? "" : " data-form-validation=\"" + component.ValueType.GetTagValue() + "\"") + (component.Length.HasValue ? " data-form-max=\"" + component.Length + "\"" : "") + " id=\"" + t_id + "\" class=\"form-control input-sm form-text-input\" type=\"text\" name=\"Components[" + this._index + "].Value\" value=\"" + Registrant[component.UId] + "\" data-component-type=\"" + component.Type.GetTagValue() + "\" " + (component.Required && !AdminRegistration ? "data-component-required=\"true\" " : "") + "data-component-regex=\"" + component.RegexPattern.GetRgxValue() + "\" />";
                    break;
            }
            html += "<input type=\"hidden\" name=\"Components[" + this._index + "].Key\" value=\"" + component.UId + "\" /></div>";
            return new HtmlString(html);
        }

        /// <summary>
        /// Renders the component.
        /// </summary>
        /// <param name="component">The component to render.</param>
        /// <returns>The html string.</returns>
        protected HtmlString _RenderRadioGroup(RadioGroup component)
        {
            var t_id = UseSortingId ? component.SortingId.ToString() : component.UId.ToString();
            var data = Registrant.Data.FirstOrDefault(d => d.Component.UId == component.UId);
            var html = "<div class=\"col-md-8 form-component" + (component.Required && !AdminRegistration ? " required-component" : "") + "\" data-component-type=\"radiogroup\" data-component-index=\"" + this._index + "\">";
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
                        html += st_itemOpen + "<label class=\"control-label\"><span class=\"form-component-label\"" + (String.IsNullOrEmpty(labelStyle) ? "" : " style=\"" + labelStyle + "\"") + ">" + component.LabelText + "</span></label>" + st_itemClose;
                        break;
                    case "description":
                    case "alttext":
                        var descriptionStyleList = component.Styles.Where(s => s.GroupName == ".form-component-label").ToList();
                        var descriptionStyle = "";
                        foreach (var style in descriptionStyleList)
                            descriptionStyle += style.Variable + ":" + style.Value + ";";
                        html += st_itemOpen + "<span class=\"form-component-description\"" + (String.IsNullOrEmpty(descriptionStyle) ? "" : " style=\"" + descriptionStyle + "\"") + ">" + component.AltText + "</span>" + st_itemClose;
                        break;
                    case "date":
                        var displayDate = "";
                        var multipleDays = component.AgendaEnd.Date != component.AgendaStart.Date;
                        var showDate = component.DisplayAgendaDate | multipleDays;
                        if (showDate)
                            displayDate = component.AgendaStart.ToString(Form.Culture) + " - " + component.AgendaEnd.ToString(Form.Culture);
                        else
                            displayDate = component.AgendaStart.DateTime.ToShortTimeString() + " - " + component.AgendaEnd.DateTime.ToShortTimeString();
                        html += st_itemOpen + "<span class=\"form-agenda\">" + displayDate + "</span>" + st_itemClose;
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
                itemHtml += this._RenderRadioItem(item, data);
            }
            if (String.IsNullOrEmpty(itemHtml))
            {
                var skipHtml = @"<input type=""hidden"" name=""Components[" + this._index + @"].Key"" value=""" + component.UId + @""" />";
                skipHtml += @"<input type=""hidden"" name=""Components[" + this._index + @"].Value"" value=""__skipped"" />";
                return new HtmlString(skipHtml);
            }
            html += itemHtml;
            this._index++;
            html += "<input type=\"hidden\" name=\"Components[" + this._index + "].Key\" value=\"" + component.UId + "\" />";
            html += "<input" + (component.Required && !AdminRegistration ? " data-form-required=\"true\"" : "") + " data-component-type=\"radiogroup\" id=\"" + t_id + "\" type=\"hidden\" name=\"Components[" + this._index + "].Value\" value=\"" + Registrant.GetRawValue(component.UId.ToString()) + "\"><div class=\"form-items\">";
            component.Items.Sort((a, b) => a.Order.CompareTo(b.Order));
            html += "</div></div>";

            return new HtmlString(html);
        }

        /// <summary>
        /// Renders the component.
        /// </summary>
        /// <param name="component">The component to render.</param>
        /// <returns>The html string.</returns>
        protected HtmlString _RenderRadioItem(RadioItem item, RegistrantData data)
        {
            Guid selected = Guid.Empty;
            if (data != null)
                Guid.TryParse(data.Value, out selected);
            var skip = this._TestSkip(item, new List<Guid>() { selected });
            if (skip.NullOut)
                if (selected == item.UId)
                    data.Value = null;
            if (skip.Skip && !Editing)
                return new HtmlString("");
            var disabled = false;
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
                html += "<input type=\"radio\" disabled=\"true\" name=\"radio" + this._index + "\" value=\"" + item.UId + "\" data-parent=\"" + item.RadioGroupKey + "\" />";
                if (full)
                    html += "<div class=\"form-at-capacity\">" + seating.FullLabel + "</div>";
            }
            else
            {
                html += "<input id=\"" + item.UId + "\" class=\"input-outside\" type=\"radio\" name=\"radio" + this._index + "\" value=\"" + item.UId + "\"" + (selected == item.UId ? " checked=\"checked\"" : "") + " data-parent=\"" + item.RadioGroupKey + "\" />";
                if (full)
                    html += "<div class=\"form-at-capacity\">" + seating.FullLabel + (seated ? " <i>previously seated</i>" : "") + "</div>";
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
                        html += "<div class=\"form-item-row\"><span class=\"form-item-label\"" + (String.IsNullOrEmpty(labelStyle) ? "" : " style=\"" + labelStyle + "\"") + ">" + item.LabelText + "</span>" + (full ? " <span class=\"form-at-capacity\">" + seating.FullLabel + "</span>" : "") + "</div>";
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
                this._waitlistIndex++;
                html += "<label><input type=\"hidden\" name=\"Waitlistings[" + this._waitlistIndex + "].Key\" value=\"" + item.UId + "\" /><input type=\"checkbox\"" + (waitlisted ? " checked=\"true\"" : "") + " name=\"Waitlistings[" + this._waitlistIndex + "].Value\" value=\"true\"><input type=\"hidden\" name=\"Waitlistings[" + this._waitlistIndex + "].Value\" value=\"false\" /> " + seating.WaitlistLabel + "</label>";
            }
            html += "</div></div>";
            return new HtmlString(html);
        }

        /// <summary>
        /// Renders the component.
        /// </summary>
        /// <param name="component">The component to render.</param>
        /// <returns>The html string.</returns>
        protected HtmlString _RenderCheckboxGroup(CheckboxGroup component)
        {
            var t_id = UseSortingId ? component.SortingId.ToString() : component.UId.ToString();
            var data = Registrant.Data.FirstOrDefault(d => d.Component.UId == component.UId);
            if (data == null)
                data = new RegistrantData();
            data.Component = component;
            var html = "";
            html += "<div class=\"col-md-8 form-component" + (component.Required && !AdminRegistration ? " required-component" : "") + "\" data-component-type=\"checkbox\" data-component-index=\"" + this._index + "\" data-component-timeexclusion=\"" + component.TimeExclusion + "\">";
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
                        html += st_itemOpen + "<label class=\"control-label\"><span class=\"form-component-label\"" + (String.IsNullOrEmpty(labelStyle) ? "" : " style=\"" + labelStyle + "\"") + ">" + component.LabelText + "</span></label>" + st_itemClose;
                        break;
                    case "description":
                    case "alttext":
                        var descriptionStyleList = component.Styles.Where(s => s.GroupName == ".form-component-label").ToList();
                        var descriptionStyle = "";
                        foreach (var style in descriptionStyleList)
                            descriptionStyle += style.Variable + ":" + style.Value + ";";
                        html += st_itemOpen + "<span class=\"form-component-description\"" + (String.IsNullOrEmpty(descriptionStyle) ? "" : " style=\"" + descriptionStyle + "\"") + ">" + component.AltText + "</span>" + st_itemClose;
                        break;
                    case "date":
                        var displayDate = "";
                        var multipleDays = component.AgendaEnd.Date != component.AgendaStart.Date;
                        var showDate = component.DisplayAgendaDate | multipleDays;
                        if (showDate)
                            displayDate = component.AgendaStart.ToString(Form.Culture) + " - " + component.AgendaEnd.ToString(Form.Culture);
                        else
                            displayDate = component.AgendaStart.DateTime.ToShortTimeString() + " - " + component.AgendaEnd.DateTime.ToShortTimeString();
                        html += st_itemOpen + "<span class=\"form-agenda\">" + displayDate + "</span>" + st_itemClose;
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
                itemHtml += this._RenderCheckboxItem(item, data);
            }
            if (String.IsNullOrEmpty(itemHtml))
            {
                var skipHtml = @"<input type=""hidden"" name=""Components[" + this._index + @"].Key"" value=""" + t_id + @""" />";
                skipHtml += @"<input type=""hidden"" name=""Components[" + this._index + @"].Value"" value=""__skipped"" />";
                return new HtmlString(skipHtml);
            }
            html += itemHtml;
            this._index++;
            html += "<input type=\"hidden\" name=\"Components[" + this._index + "].Key\" value=\"" + component.UId + "\" /><input " + (component.Required && !AdminRegistration ? " data-form-required=\"true\"" : "") + " data-component-timeexclusion-message=\"" + component.DialogText + "\" data-component-type=\"checkboxgroup\" data-component-timeexclusion=\"" + component.TimeExclusion + "\" id=\"" + component.UId + "\" type=\"hidden\" name=\"Components[" + this._index + "].Value\" value='" + data.Value + "'><div class=\"form-items\">";
            html += "</div></div>";
            return new HtmlString(html);
        }

        /// <summary>
        /// Renders the component.
        /// </summary>
        /// <param name="component">The component to render.</param>
        /// <returns>The html string.</returns>
        protected HtmlString _RenderCheckboxItem(CheckboxItem item, RegistrantData data)
        {
            var selected = false;
            var selections = new List<Guid>();
            if (data != null)
                selections = JsonConvert.DeserializeObject<List<Guid>>(data.Value);
            var skip = this._TestSkip(item, selections);
            if (skip.NullOut)
            {
                selections.Remove(item.UId);
                data.Value = JsonConvert.SerializeObject(selections);
            }
            if (skip.Skip && !Editing)
                return new HtmlString("");
            var disabled = false;
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
            var html = "";
            html += "<div class=\"form-item" + (item.Required && !AdminRegistration ? " required-item" : "") + "\"><div class=\"form-checkbox checkbox\">";
            if (disabled)
                html += "<fieldset disabled>";
            html += "<label>";
            html += RenderErrors(item.UId.ToString());
            var price = PriceGroup.DisplayPrice(item, Registrant);
            if (disabled)
            {
                html += "<input" + (item.Required && !AdminRegistration ? " data-form-required=\"true\"" : "") + " type=\"checkbox\" disabled=\"true\" " + (item.CheckboxGroup.TimeExclusion ? "data-agenda-start=\"" + item.AgendaStart.ToString() + "\" data-agenda-end=\"" + item.AgendaEnd.ToString() + "\"" : "") + " data-parent=\"" + item.ParentKey.ToString() + "\" value=\"" + item.UId + "\" data-component-required=\"" + (item.Required ? "true" : "false") + "\" />";
                if (full)
                    html += "<div class=\"form-at-capacity\">" + seating.FullLabel + "</div>";
            }
            else
            {
                html += "<input id=\"" + item.UId + "\"" + (selected ? " checked=\"checked\"" : "") + " type=\"checkbox\"" + (item.CheckboxGroup.TimeExclusion ? " data-agenda-start=\"" + item.AgendaStart.ToString() + "\" data-agenda-end=\"" + item.AgendaEnd.ToString() + "\"" : "") + " data-parent=\"" + item.ParentKey.ToString() + "\" value=\"" + item.UId + "\" data-component-required=\"" + (item.Required ? "true" : "false") + "\" />";
                if (full)
                    html += "<div class=\"form-at-capacity\">" + seating.FullLabel + (seated ? " <i>previously seated</i>" : "") + "</div>";
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
                        html += "<div class=\"form-item-row\"><span class=\"form-item-label\"" + (String.IsNullOrEmpty(labelStyle) ? "" : " style=\"" + labelStyle + "\"") + ">" + item.LabelText + "</span>" + (full ? " <span class=\"form-at-capacity\">" + seating.FullLabel + "</span>" : "") + "</div>";
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
                this._waitlistIndex++;
                html += "<label class=\"waitlist\"><input type=\"hidden\" name=\"Waitlistings[" + this._waitlistIndex + "].Key\" value=\"" + item.UId + "\" /><input type=\"checkbox\"" + (waitlisted ? " checked=\"checked\"" : "") + " name=\"Waitlistings[" + this._waitlistIndex + "].Value\" value=\"true\"><input type=\"hidden\" name=\"Waitlistings[" + this._waitlistIndex + "].Value\" value=\"false\" /> " + seating.WaitlistLabel + "</label>";
            }
            html += "</div></div>";
            return new HtmlString(html);
        }

        /// <summary>
        /// Renders the component.
        /// </summary>
        /// <param name="component">The component to render.</param>
        /// <returns>The html string.</returns>
        protected HtmlString _RenderDropdownGroup(DropdownGroup component)
        {
            var t_id = UseSortingId ? component.SortingId.ToString() : component.UId.ToString();
            var data = Registrant.Data.FirstOrDefault(d => d.Component.UId == component.UId);
            var html = "";
            html += "<div class=\"col-sm-6 col-md-4 col-lg-3 form-component" + (component.Required && !AdminRegistration ? " required-component" : "") + "\" data-component-type=\"dropdown\" data-component-index=\"" + this._index + "\">";
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
                        html += st_itemOpen + "<label class=\"control-label\"><span class=\"form-component-label\"" + (String.IsNullOrEmpty(labelStyle) ? "" : " style=\"" + labelStyle + "\"") + ">" + component.LabelText + "</span></label>" + st_itemClose;
                        break;
                    case "description":
                    case "alttext":
                        var descriptionStyleList = component.Styles.Where(s => s.GroupName == ".form-component-label").ToList();
                        var descriptionStyle = "";
                        foreach (var style in descriptionStyleList)
                            descriptionStyle += style.Variable + ":" + style.Value + ";";
                        html += st_itemOpen + "<span class=\"form-component-description\"" + (String.IsNullOrEmpty(descriptionStyle) ? "" : " style=\"" + descriptionStyle + "\"") + ">" + component.AltText + "</span>" + st_itemClose;
                        break;
                    case "date":
                        var displayDate = "";
                        var multipleDays = component.AgendaEnd.Date != component.AgendaStart.Date;
                        var showDate = component.DisplayAgendaDate | multipleDays;
                        if (showDate)
                            displayDate = component.AgendaStart.ToString(Form.Culture) + " - " + component.AgendaEnd.ToString(Form.Culture);
                        else
                            displayDate = component.AgendaStart.DateTime.ToShortTimeString() + " - " + component.AgendaEnd.DateTime.ToShortTimeString();
                        html += st_itemOpen + "<span class=\"form-agenda\">" + displayDate + "</span>" + st_itemClose;
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
                itemHtml += this._RenderDropdownItem(item, data);
            if (String.IsNullOrEmpty(itemHtml))
            {
                var skipHtml = @"<input type=""hidden"" name=""Components[" + this._index + @"].Key"" value=""" + t_id + @""" />";
                skipHtml += @"<input type=""hidden"" name=""Components[" + this._index + @"].Value"" value=""__skipped"" />";
                return new HtmlString(skipHtml);
            }
            this._index++;
            html += "<input type=\"hidden\" name=\"Components[" + this._index + "].Key\" value=\"" + component.UId + "\" /><div class=\"form-items\"><select" + (component.Required && !AdminRegistration ? " data-form-required=\"true\"" : "") + " id=\"" + component.UId.ToString() + "\" name=\"Components[" + this._index + "].Value\" class=\"form-control input-sm form-select\">";
            html += "<option value=\"\"></option>";
            html += itemHtml;
            html += "</select></div></div>";

            return new HtmlString(html);
        }

        /// <summary>
        /// Renders the component.
        /// </summary>
        /// <param name="component">The component to render.</param>
        /// <returns>The html string.</returns>
        protected HtmlString _RenderDropdownItem(DropdownItem item, RegistrantData data)
        {
            var seating = item.Seating;
            var full = false;
            Guid selected = Guid.Empty;
            if (data != null)
                Guid.TryParse(data.Value, out selected);
            var skip = this._TestSkip(item, new List<Guid>() { selected });
            if (skip.NullOut)
                if (selected == item.UId)
                    data.Value = null;
            if (skip.Skip && !Editing)
                return new HtmlString("");
            if (seating != null && seating.AvailableSeats < 1 && !AdminRegistration)
                return new HtmlString("");
            return new HtmlString("<option value=\"" + item.UId + "\"" + (selected == item.UId ? " selected=true" : "") + (full ? " disabled=\"true\"" : "") + ">" + item.LabelText + (full ? " - FULL" : "") + "</option>");
        }

        /// <summary>
        /// Renders the component.
        /// </summary>
        /// <param name="component">The component to render.</param>
        /// <returns>The html string.</returns>
        protected HtmlString _RenderRatingSelect(RatingSelect component)
        {
            var t_id = UseSortingId ? component.SortingId.ToString() : component.UId.ToString();
            var html = "";
            if (Form.ParentForm == null || component.MappedComponent == null)
            {
                var dataPoint = Registrant.Data.FirstOrDefault(d => d.VariableUId == component.UId);
                html += "<div class=\"col-sm-6 col-md-4 col-lg-3 form-component\" data-component-index=\"" + this._index + "\">";
                foreach (var item in component.DisplayComponentOrder.Items.OrderBy(i => i.Order))
                {
                    switch (item.Item.ToLower())
                    {
                        case "label":
                            var labelStyleList = component.Styles.Where(s => s.GroupName == ".form-component-label").ToList();
                            var labelStyle = "";
                            foreach (var style in labelStyleList)
                                labelStyle += style.Variable + ":" + style.Value + ";";
                            html += st_itemOpen + "<label class=\"control-label\"><span class=\"form-component-label\"" + (String.IsNullOrEmpty(labelStyle) ? "" : " style=\"" + labelStyle + "\"") + ">" + component.LabelText + "</span></label>" + st_itemClose;
                            break;
                        case "description":
                        case "alttext":
                            var descriptionStyleList = component.Styles.Where(s => s.GroupName == ".form-component-label").ToList();
                            var descriptionStyle = "";
                            foreach (var style in descriptionStyleList)
                                descriptionStyle += style.Variable + ":" + style.Value + ";";
                            html += st_itemOpen + "<span class=\"form-component-description\"" + (String.IsNullOrEmpty(descriptionStyle) ? "" : " style=\"" + descriptionStyle + "\"") + ">" + component.AltText + "</span>" + st_itemClose;
                            break;
                    }
                }
                this._index++;
                html += "<input class=\"rating form-control input-sm form-text-input\" type=\"password\" id=\"" + t_id + "\" name=\"Components[" + this._index + "].Value\" value=\"" + (dataPoint != null ? dataPoint.Value : "") + "\" />";
                html += "<input type=\"hidden\" name=\"Components[" + this._index + "].Key\" value=\"" + component.UId + "\" /></div>";
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
                            this._index++;
                            html += "<div class=\"col-sm-6 col-md-4 col-lg-3 form-component\" data-component-index=\"" + this._index + "\">";
                            foreach (var item in component.DisplayComponentOrder.Items.OrderBy(i => i.Order))
                            {
                                switch (item.Item.ToLower())
                                {
                                    case "label":
                                        var labelStyleList = component.Styles.Where(s => s.GroupName == ".form-component-label").ToList();
                                        var labelStyle = "";
                                        foreach (var style in labelStyleList)
                                            labelStyle += style.Variable + ":" + style.Value + ";";
                                        html += st_itemOpen + "<label class=\"control-label\"><span class=\"form-component-label\"" + (String.IsNullOrEmpty(labelStyle) ? "" : " style=\"" + labelStyle + "\"") + ">" + mappedComponent.LabelText + ": " + t_item.LabelText + "</span></label>" + st_itemClose;
                                        break;
                                    case "description":
                                    case "alttext":
                                        var descriptionStyleList = component.Styles.Where(s => s.GroupName == ".form-component-label").ToList();
                                        var descriptionStyle = "";
                                        foreach (var style in descriptionStyleList)
                                            descriptionStyle += style.Variable + ":" + style.Value + ";";
                                        html += st_itemOpen + "<span class=\"form-component-description\"" + (String.IsNullOrEmpty(descriptionStyle) ? "" : " style=\"" + descriptionStyle + "\"") + ">" + t_item.AltText + "</span>" + st_itemClose;
                                        break;
                                }
                            }
                            html += "<input id=\"" + t_id + "\" class=\"rating form-control input-sm form-text-input\" type=\"text\" data-showClear=\"false\" data-showCaption=\"false\" data-size=\"sm\" name=\"Components[" + this._index + "].Value\" value=\"" + (dataPoint != null ? dataPoint.Value : "") + "\" />";
                            html += "<input type=\"hidden\" name=\"Components[" + this._index + "].Key\" value=\"" + t_item.UId + "\" />";
                            html += "</div>";
                        }
                    }
                }
            }
            return new HtmlString(html);
        }

        /// <summary>
        /// Renders the component.
        /// </summary>
        /// <param name="component">The component to render.</param>
        /// <returns>The html string.</returns>
        protected HtmlString _RenderFreeText(FreeText component)
        {
            var html = "";
            html = "<div class=\"col-xs-12 form-component\">" + this._Parse(component.Html) + "</div>";
            return new HtmlString(html);
        }

        /// <summary>
        /// Renders the component.
        /// </summary>
        /// <param name="component">The component to render.</param>
        /// <returns>The html string.</returns>
        protected HtmlString _RenderNumberSelector(NumberSelector component)
        {
            var t_id = UseSortingId ? component.SortingId.ToString() : component.UId.ToString();
            var data = Registrant.Data.FirstOrDefault(d => d.Component.UId == component.UId);
            var html = "";
            html += "<div class=\"col-sm-6 col-md-4 col-lg-3 form-component" + (component.Required && !AdminRegistration ? " required-component" : "") + "\" data-component-type=\"dropdown\" data-component-index=\"" + this._index + "\">";
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
                        html += st_itemOpen + "<label class=\"control-label\"><span class=\"form-component-label\"" + (String.IsNullOrEmpty(labelStyle) ? "" : " style=\"" + labelStyle + "\"") + ">" + component.LabelText + "</span></label>" + st_itemClose;
                        break;
                    case "description":
                    case "alttext":
                        var descriptionStyleList = component.Styles.Where(s => s.GroupName == ".form-component-label").ToList();
                        var descriptionStyle = "";
                        foreach (var style in descriptionStyleList)
                            descriptionStyle += style.Variable + ":" + style.Value + ";";
                        html += st_itemOpen + "<span class=\"form-component-description\"" + (String.IsNullOrEmpty(descriptionStyle) ? "" : " style=\"" + descriptionStyle + "\"") + ">" + component.AltText + "</span>" + st_itemClose;
                        break;
                    case "date":
                        var displayDate = "";
                        var multipleDays = component.AgendaEnd.Date != component.AgendaStart.Date;
                        var showDate = component.DisplayAgendaDate | multipleDays;
                        if (showDate)
                            displayDate = component.AgendaStart.ToString(Form.Culture) + " - " + component.AgendaEnd.ToString(Form.Culture);
                        else
                            displayDate = component.AgendaStart.DateTime.ToShortTimeString() + " - " + component.AgendaEnd.DateTime.ToShortTimeString();
                        html += st_itemOpen + "<span class=\"form-agenda\">" + displayDate + "</span>" + st_itemClose;
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
                var skipHtml = @"<input type=""hidden"" name=""Components[" + this._index + @"].Key"" value=""" + component.UId + @""" />";
                skipHtml += @"<input type=""hidden"" name=""Components[" + this._index + @"].Value"" value=""__skipped"" />";
                return new HtmlString(skipHtml);
            }
            this._index++;
            html += "<input type=\"hidden\" name=\"Components[" + this._index + "].Key\" value=\"" + component.UId + "\" /><div class=\"form-items\"><select" + (component.Required && !AdminRegistration ? " data-form-required=\"true\"" : "") + " id=\"" + t_id + "\" name=\"Components[" + this._index + "].Value\" class=\"form-control input-sm form-select\">";
            html += "<option value=\"\"></option>";
            html += itemHtml;
            html += "</select></div></div>";

            return new HtmlString(html);
        }


        #endregion

        #region RSVP
        /// <summary>
        /// Renders the rsvps.
        /// </summary>
        /// <param name="first">If this is the first time rendering rsvps.</param>
        /// <returns>The html string.</returns>
        public HtmlString RenderRsvp(bool first = false)
        {
            var html = "<div class=\"row form-btn-row\"><div class=\"col-xs-12\"><div class=\"row\"><input type=\"hidden\" name=\"RSVP\" id=\"RSVP\" value=\"\" />";
            html += "<div class=\"col-sm-6 col-md-4 col-lg-3 form-btn\"><a href=\"#\" class=\"btn-rs btn-rsvp" + (Registrant.RSVP == true && !first ? " btn-selected" : "") + "\" id=\"rsvpAccept\">" + Form.RSVPAccept + @"</a></div>";
            html += "<div class=\"col-sm-6 col-md-4 col-lg-3 form-btn\"><a href=\"#\" class=\"btn-rs btn-rsvp" + (Registrant.RSVP == false && !first ? " btn-selected" : "") + "\" id=\"rsvpDecline\">" + Form.RSVPDecline + @"</a></div>";
            html += "</div></div></div>";
            return new HtmlString(this._Parse(html));
        }
        #endregion

        #region Audience
        /// <summary>
        /// Renders the audiences.
        /// </summary>
        /// <returns>The html string.</returns>
        public HtmlString RenderAudiences()
        {
            var html = "<div class=\"row form-btn-panel\"><div class=\"col-xs-12\"><div class=\"row\">";
            foreach (var audience in Form.Audiences.OrderBy(a => a.Order))
            {
                html += "<div class=\"col-sm-6 col-md-4 col-lg-3 form-btn\"><a href=\"#\" class=\"btn-rs btn-audience" + (Registrant.AudienceKey.HasValue ? (Registrant.AudienceKey == audience.UId ? " btn-selected" : "") : "") + "\" data-value=\"" + audience.UId + "\">" + audience.Label + @"</a></div>";
            }
            html += "<input type=\"hidden\" name=\"Audience\" id=\"Audience\" value=\"" + Registrant.AudienceKey + "\"/></div></div></div>";
            return new HtmlString(this._Parse(html));
        }
        #endregion

        #region Panel
        /// <summary>
        /// Renders the panel.
        /// </summary>
        /// <param name="panel">The panel to render</param>
        /// <returns>The html string.</returns>
        public HtmlString RenderPanel(Panel panel)
        {
            var skipped = false;
            if (!panel.Enabled)
                return new HtmlString("");
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
                    this._index++;
                    html += @"<input type=""hidden"" name=""Components[" + this._index + @"].Key"" value=""" + component.UId + @""" /><input type=""hidden"" name=""Components[" + this._index + @"].Value"" value=""__skipped"" />";
                }
                return new HtmlString(html);
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
                        compHtml += Render(component);
                    }
                }
                html += compHtml;
                html += "</div></div></div>";
                return new HtmlString(String.IsNullOrWhiteSpace(compHtml) ? "" : this._Parse(html));
            }
        }
        #endregion

        #region Confirmation
        public HtmlString RenderConfirmationComponent(IComponent component)
        {
            if (component.AdminOnly && !AdminRegistration)
                return new HtmlString("");
            var commands = this._ExecuteLogic(component);
            //var commands = component.RunLogic(Registrant, true, Repository);
            if (!component.Enabled)
                return new HtmlString("");
            if ((component.RSVP == RSVPType.Decline && Registrant.RSVP) || (component.RSVP == RSVPType.Accept && !Registrant.RSVP))
                return new HtmlString("");
            if (component.Audiences.Count > 0 && (Registrant.Audience == null || !component.Audiences.Contains(Registrant.Audience)))
                return new HtmlString("");
            foreach (var command in commands)
            {
                switch (command.Command)
                {
                    case JLogicWork.Hide:
                        return new HtmlString("");
                }
            }
            if (component is FreeText)
            {
                if ((component as FreeText).HideReview)
                    return new HtmlString("");
                return new HtmlString(@"<tr><td colspan=""2"" class=""confirmation-freetext"">" + ((FreeText)component).Html + "</td></tr>");
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
                    Registrant = Registrant,
                    RegistrantKey = Registrant.UId,
                    Value = null
                };
                Registrant.Data.Add(dp);
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
                var selections = JsonConvert.DeserializeObject<List<Guid>>(dp.Value ?? "") ?? new List<Guid>();
                if (selections.Contains(cbg.Items[0].UId))
                    labelValue = "Checked";
                else
                    labelValue = "Not Checked";
            }
            if (component is Input && ((Input)component).Type == Domain.Entities.Components.InputType.UniversalCreditCard)
            {
                Guid cardId;
                if (dp != null && Guid.TryParse(dp.Value, out cardId))
                {
                    var card = Context.CreditCards.Find(cardId);
                    if (card != null)
                        f_value = card.Number.GetLast(4);
                }
            }
            if (String.IsNullOrEmpty(f_value))
                f_value = "<i>No Selection</i>";
            html += @"<td>" + labelValue + @"</td>";
            html += @"<td>" + f_value + @"</td>";
            html += @"</tr>";
            return new HtmlString(html);
        }

        /// <summary>
        /// Renderst the panel.
        /// </summary>
        /// <param name="panel">The panel to render</param>
        /// <returns>The html string.</returns>
        public HtmlString RenderConfirmationPanel(Panel panel)
        {
            if (panel.AdminOnly && !AdminRegistration)
                return new HtmlString("");
            var commands = this._ExecuteLogic(panel);
            //var commands = panel.RunLogic(Registrant, true, Repository);
            if (!panel.Enabled)
                return new HtmlString("");
            if ((panel.RSVP == RSVPType.Decline && Registrant.RSVP) || (panel.RSVP == RSVPType.Accept && !Registrant.RSVP))
                return new HtmlString("");
            if (panel.Audiences.Count > 0 && (Registrant.Audience == null || !panel.Audiences.Contains(Registrant.Audience)))
                return new HtmlString("");
            foreach (var command in commands)
            {
                switch (command.Command)
                {
                    case JLogicWork.Hide:
                        return new HtmlString("");
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
                return new HtmlString("");
            html += itemHtml + @"";
            return new HtmlString(html);
        }

        /// <summary>
        /// Renders the confirmation page.
        /// </summary>
        /// <param name="page">The page to render.</param>
        /// <returns>The html string.</returns>
        public HtmlString RenderConfirmationPage(Page page)
        {
            var commands = this._ExecuteLogic(page);
            //var commands = page.RunLogic(Registrant, true, Repository);
            if (!page.Enabled)
                return new HtmlString("");
            if (page.AdminOnly && !AdminRegistration)
                return new HtmlString("");
            if ((page.RSVP == RSVPType.Decline && Registrant.RSVP) || (page.RSVP == RSVPType.Accept && !Registrant.RSVP))
                return new HtmlString("");
            if (page.Audiences.Count > 0 && (Registrant.Audience == null || !page.Audiences.Contains(Registrant.Audience)))
                return new HtmlString("");
            foreach (var command in commands)
            {
                switch (command.Command)
                {
                    case JLogicWork.PageSkip:
                        return new HtmlString("");
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
                return new HtmlString("");
            html += panelHtml;
            return new HtmlString(this._Parse(html));
        }
        #endregion

        #region Helpers
        /// <summary>
        /// Gets the commands in memory that have previously been run or runs them.
        /// </summary>
        /// <param name="holder">The holder of the logic.</param>
        /// <param name="onLoad">If it should run on load logic or advancing logic. True by default.</param>
        /// <returns>The logic commands of the execution.</returns>
        protected IEnumerable<JLogicCommand> _ExecuteLogic(ILogicHolder holder, bool onLoad = true)
        {
            return LogicEngine.RunLogic(holder, registrant: Registrant, onLoad: onLoad, runCommands: false, allCommands: Commands);
        }

        /// <summary>
        /// Parses the html string of reg syntax
        /// </summary>
        /// <param name="toParse">The string to parse.</param>
        /// <returns>The parsed html string.</returns>
        protected string _Parse(string toParse)
        {
            Domain.Entities.Clients.User t_user = null;
            if (Context.SecuritySettings != null && Context.SecuritySettings.AppUser != null)
                t_user = Context.SecuritySettings.AppUser;
            if (Registrant != null)
                return Registrant.Parse(toParse, t_user);
            return Form.Parse(toParse, t_user);
        }

        /// <summary>
        /// Parses the string of reg syntax
        /// </summary>
        /// <param name="toParse">The string to parse.</param>
        /// <returns>The parsed string.</returns>
        protected HtmlString _Parse(HtmlString toParse)
        {
            return new HtmlString(this._Parse(toParse.ToString()));
        }


        #endregion

        public void Dispose()
        {
        }
    }

    /// <summary>
    /// Holds information about a component being skipped.
    /// </summary>
    public class RegisterComponentSkipResult
    {
        /// <summary>
        /// If the component is skipped.
        /// </summary>
        protected bool skip;
        /// <summary>
        /// If the component should be nulled out.
        /// </summary>
        protected bool nullOut;

        /// <summary>
        /// Wether the component should be skipped
        /// </summary>
        public bool Skip
        {
            get
            {
                return skip;
            }
            set
            {
                skip = value;
            }
        }
        /// <summary>
        /// Wether the component should be skipped or not.
        /// </summary>
        public bool NullOut
        {
            get
            {
                return nullOut;
            }
            set
            {
                if (value)
                    skip = true;
                nullOut = value;
            }
        }
        /// <summary>
        /// Should the component be hidden.
        /// </summary>
        public bool Hide { get; set; }

        /// <summary>
        /// Initializes the class.
        /// </summary>
        public RegisterComponentSkipResult()
        {
            skip = false;
            nullOut = false;
            Hide = false;
        }
    }
}