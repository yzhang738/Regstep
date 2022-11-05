using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text.RegularExpressions;
using RSToolKit.Domain.Entities;
using RSToolKit.Domain.Entities.Components;
using RSToolKit.Domain.Data;

namespace RSToolKit.WebUI.Infrastructure
{
    public class RegDataHtmlHelper
    {

        #region Variables

        protected string _nl = Environment.NewLine;
        protected string _tb = "\t";
        protected Form _form;
        protected Registrant _reg;
        protected EFDbContext _context;

        #endregion

        #region Constructor

        public RegDataHtmlHelper(EFDbContext context, Form form, Registrant reg)
        {
            _form = form;
            _reg = reg;
            _context = context;
        }

        public RegDataHtmlHelper(EFDbContext context, Guid form, string reg)
        {
            _context = context;
            _form = context.Forms.FirstOrDefault(f => f.UId == form);
            _reg = context.Registrants.FirstOrDefault(r => r.Confirmation == reg && r.FormKey == form);

        }

        #endregion

        #region Universal Methods

        /// <summary>
        /// Returns html string that represents start and end times for the checkbox item.
        /// </summary>
        /// <param name="time">The agenda time to get html for.</param>
        /// <returns>The html string.</returns>
        public string ToHtml(DateTimeOffset start, DateTimeOffset end, bool displayDate, CSS css)
        {
            if (displayDate)
            {
                if (start.Day == end.Day && start.Month == end.Month && start.Year == end.Year)
                {
                    return @"<span class=""Time"" style=""" + css.InlineCss + @""">" + start.Month + "/" + start.Day + "/" + start.Year + " " + start.DateTime.ToShortTimeString() + " - " + end.DateTime.ToShortTimeString() + "</span>";
                }
                else
                {
                    return @"<span  class=""Time"" style=""" + css.InlineCss + @""">" + start.Month + "/" + start.Day + "/" + start.Year + " " + start.DateTime.ToShortTimeString() + " - " + end.Month + "/" + end.Day + "/" + end.Year + " " + end.DateTime.ToShortTimeString() + "</span>";
                }
            }
            else
            {
                return @"<span  class=""Time"" style=""" + css.InlineCss + @""">" + start.DateTime.ToShortTimeString() + " - " + end.DateTime.ToShortTimeString() + "</span>";
            }
        }

        /// <summary>
        /// Returns the html representing the label.
        /// </summary>
        /// <param name="label">The label to get the html for.</param>
        /// <returns>The string representing the html.</returns>
        public string ToHtml(string label, CSS css)
        {
            css["vertical-align"] = "top";
            if (String.IsNullOrWhiteSpace(label)) return "";
            else return @"<span class=""ComponentLabel"" style=""" + css.InlineCss + @""">" + Parse(label) + @"</span>";
        }

        /// <summary>
        /// Parses the given text for RegStep syntax and replaces it with the appropriate text.
        /// </summary>
        /// <param name="text">The text to parse through.</param>
        /// <param name="form">The form to pull the RegStep syntax from.</param>
        /// <returns>Returns a string representing the desired value in the RegStep syntax.</returns>
        public string Parse(string text)
        {
            string returnText = text;
            returnText = CustomText.Render(returnText, _form.CustomTexts);
            Regex rgxVariable = new Regex(@"(?<!\\)\[([^\[\]]*)\]");
            Match match = rgxVariable.Match(text);
            while (match.Success)
            {
                Regex replaceCloseC = new Regex(@"&gt;");
                string inner = match.Groups[1].Value;
                inner = replaceCloseC.Replace(inner, ">");
                string replace = "";
                Regex rgxVariablesItem = new Regex(@"([^=]*)=>(.*)");
                Regex rgxVariablesItem2 = new Regex(@"([^=]*)=>(.*)");
                Regex rgxCustomText = new Regex(@"(?<customText>[^(?:=>)]*?)");
                Match matchItems = rgxVariablesItem.Match(inner);
                Match matchCTItems = rgxCustomText.Match(inner);
                if (matchItems.Success)
                {
                    switch (matchItems.Groups[1].Value)
                    {
                        case "var":
                            string value = "<i>Undefined</i>";
                            var data = _reg.Data.FirstOrDefault(d => d.Component.Variable.Value == matchItems.Groups[2].Value);
                            if (data != null) value = data.Value;
                            if (!String.IsNullOrEmpty(value))
                            {
                                replace = value;
                            }
                            else replace = "<span style=\"color: red; font-weight: bold;\" title =\"The variable " + matchItems.Groups[2].Value + " does not exists for this form. The incorrect syntax is [" + match.Groups[1].Value + "]\">!RegStepSyntaxError!</span>";
                            break;
                    }
                }
            }
            return returnText;
        }

        #endregion

        #region CheckboxGroup

        /// <summary>
        /// Gets the html that is needed to make the checkbox group.
        /// </summary>
        /// <param name="group">The checkbox group.</param>
        /// <returns>Returns the html as a string.</returns>
        public RegDataReturn GetDataCbg(Guid id)
        {
            var group = _context.Components.OfType<CheckboxGroup>().FirstOrDefault(c => c.UId == id);
            //NEWEST CODE
            var ret = new RegDataReturn();
            //This grabs what checkbox items are selected.
            List<Guid> selected = new List<Guid>();
            Regex selectedItems = new Regex(@"!([^!]*)");
            string selections = "";
            var data = _reg.Data.FirstOrDefault(d => d.VariableUId == group.UId);
            if (data != null)
                selections = data.Value;
            var selectedVal = "";
            foreach (Match m in selectedItems.Matches(selections))
            {
                var sUid = Guid.Parse(m.Groups[1].Value);
                selectedVal += group.Items.Find(i => i.UId == sUid).LabelText + ", ";
                selected.Add(sUid);
            }
            if (!String.IsNullOrEmpty(selectedVal))
            {
                selectedVal = selectedVal.Remove(selectedVal.Length - 3, 2);
            }

            //This holds a row of items in the component.  Every component starts with one.
            //A "nl" item closes the previous one and opens the next one.
            string js = "";
            string html = @"<div class=""row"">";
            html += @"<div class=""col-sm-12"">" + group.LabelText + @"</div>";
            html += @"</div>";
            foreach (CheckboxItem item in group.Items)
            {
                item.DisplayAgendaDate = (group.DisplayAgendaDate || item.DisplayAgendaDate);
                string disable = "";
                string selectedText = "";
                if (selected.Contains(item.UId))
                {
                    selectedText = @" checked=""checked"" ";
                }
                bool disabled = false;
                bool waitlisting = false;
                if (item.Seating != null && item.Seating.UId != Guid.Empty)
                {
                    bool alreadySeated = false;
                    if (item.Seating.MaxSeats - item.Seating.Seaters.Count(s => s.Seated) < 1)
                    {
                        if (data != null && !String.IsNullOrWhiteSpace(data.Value) && selected.Contains(item.UId))
                            alreadySeated = true;
                        if (!alreadySeated)
                        {
                            disabled = true;
                            if (disabled) disable = @" disabled=""disabled"" ";
                        }
                    }
                    if (item.Seating.MaxSeats - item.Seating.Seaters.Count(s => s.Seated) < 1)
                    {
                        if (data != null && !String.IsNullOrWhiteSpace(data.Value) && selected.Contains(item.UId))
                            alreadySeated = true;
                        if (!alreadySeated && item.Seating.Waitlistable)
                        {
                            waitlisting = true;
                        }
                    }
                }
                string itemHtml = @"<div class=""row"">";
                string soldOut = "";
                if (disabled)
                {
                    if (!waitlisting)
                    {
                        soldOut = @"<span class=""SoldOut"">" + Parse(item.Seating.FullLabel) + @"</span>";
                    }
                    else
                    {
                        soldOut = @"<span class=""Waitlisting"" onclick=""WAITOpen" + item.UId.ToString("N") + @"()"">" + Parse(item.Seating.WaitlistLabel) + @"</span>";
                    }
                }
                itemHtml += @"<div class=""col-sm-1"">" + @"<input class=""cbi"" type=""checkbox"" data-component-id=""" + item.UId.ToString("N") + @""" data-component-label=""" + item.LabelText + @""" id=""cbi" + item.UId.ToString("N") + @""" value=""" + item.LabelText + @""" onchange=""EditCBG('cbi" + item.UId.ToString("N") + @"', '" + item.AgendaStart.ToString() + @"', '" + item.AgendaEnd.ToString() + @"')""" + selectedText + disable + @" /></div>";
                itemHtml += @"<div class=""col-sm-11"">" + ToHtml(item.LabelText, new CSS()) + " " + soldOut + @"</div>";
                if (waitlisting)
                {
                    string alreadyWaitlisting = "";
                    if (item.Seating.Seaters.FirstOrDefault(s => s.SeatingKey == _reg.UId && s.ComponentKey == item.UId && !s.Seated) != null) alreadyWaitlisting = @" checked=""true"" ";
                    itemHtml += @"<div class=""WaitlistingDiv Hidden"" id=""WAIT" + item.UId.ToString("N") + @""">";
                    itemHtml += @"<div style=""text-align: center;"">Waitlisting for " + Parse(item.LabelText) + @"</div>";
                    itemHtml += @"<input type=""checkbox"" id=""WAITCB" + item.UId.ToString("N") + @""" onclick=""WAITCLICK" + item.UId.ToString("N") + @"()"" " + alreadyWaitlisting + @"/> Add to Waitlist";
                    itemHtml += @"<input type=""hidden"" id=""WAITH" + item.UId.ToString("N") + @""" value=""" + (item.Seating.Seaters.FirstOrDefault(s => s.SeatingKey == _reg.UId && s.ComponentKey == item.UId && !s.Seated) != null).ToString() + @""" name=""WAIT" + item.UId.ToString("N") + @""" />";
                    js += @"<script type=""text/javascript"">
                            function WAITCLICK" + item.UId.ToString("N") + @"(){
                                if ($('#WAITCB" + item.UId.ToString("N") + @"').is(':checked')) {
                                    $('#WAITH" + item.UId.ToString("N") + @"').val('true');
                                } else {
                                    $('#WAITH" + item.UId.ToString("N") + @"').val('false');
                                }
                            } 
                        </script>";
                    itemHtml += @"</div>";
                }
                itemHtml += @"</div>";
                html += itemHtml;
            }
            html += @"</div>";
            string currentVal = "";
            if (data != null)
            {
                currentVal = data.Value;
            }
            html += @"<input type=""hidden"" id=""editVal"" value=""" + currentVal + @""" />";
            html += @"<input type=""hidden"" id=""viewVal"" value=""" + selectedVal + @""" />";
            html += @"</div>";
            ret.Html = html;

            ret.JavaScript = js;


            string jsHeader = "";
            jsHeader += @"
                    function EditCBG(cbi, start, end) {
                        var valueField = $('#editVal');
                        $(valueField).val('');";
            for (int i = 0; i < group.Items.Count; i++)
            {
                if (group.Items[i].Seating == null || (group.Items[i].Seating.MaxSeats - group.Items[i].Seating.Seaters.Count(s => s.Seated) > 0))
                {
                    jsHeader += @"
                        if (cbi == 'cbi" + group.Items[i].UId.ToString("N") + @"' && $('#cbi" + group.Items[i].UId.ToString("N") + @"').is(':checked')) {";
                    for (int j = 0; j < group.Items.Count; j++)
                    {
                        if (group.Items[i] == group.Items[j]) continue;
                        if (group.Items[j].Seating == null || (group.Items[j].Seating.MaxSeats - group.Items[j].Seating.Seaters.Count(s => s.Seated) > 0))
                        {
                            if (group.TimeExclusion && group.Items[i].Open.CompareTo(group.Items[j].Close) < 0 && group.Items[i].Open.CompareTo(group.Items[j].Open) > 0)
                            {
                                jsHeader += @"
                            if ($('#cbi" + group.Items[j].UId.ToString("N") + @"').is(':checked'); {
                                $('#cbi" + group.Items[j].UId.ToString("N") + @"').prop('checked') = false;
                            }";
                            }

                        }
                    }
                    jsHeader += @"
                        }";
                }
            }
            jsHeader += @"
                    var cbgValue = '';
                    var cbgViewVal = '';
                    $('.cbi:checked').each(function () {
                        cbgValue += '!' + $(this).attr('data-component-id');
                        cbgViewVal += $(this).attr('data-component-label') + ', ';
                    });
                    if (cbgViewVal.length > 3 && cbgViewVal.match(', $')) {
                        cbgViewVal = cbgViewVal.substring(0, cbgViewVal.length - 2);
                    }
                    $(valueField).val(cbgValue);
                    $('#viewVal').val(cbgViewVal);
                }";

            if (group.TimeExclusion)
            {
            }

            ret.JavaScript += jsHeader;

            return ret;
        }

        #endregion

    }

    public class RegDataReturn
    {
        public string JavaScript { get; set; }
        public string Html { get; set; }
    }
}