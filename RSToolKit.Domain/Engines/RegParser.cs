using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RSToolKit.Domain.Entities;
using System.Text.RegularExpressions;
using RSToolKit.Domain.Entities.Clients;
using System.Web;

namespace RSToolKit.Domain.Engines
{
    /// <summary>
    /// A class used for parsing RegStep syntax in the system.
    /// </summary>
    public static class RegParser
    {

        /// <summary>
        /// The syntax to look for.
        /// </summary>
        public static Regex Syntax = new Regex(@"\[([^\]]*)\]", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        /// <summary>
        /// Parses the registrant and form for any syntax.
        /// </summary>
        /// <param name="registrant">The registrant.</param>
        /// <param name="toParse">The html string to parse.</param>
        /// <param name="user">The user.</param>
        /// <param name="noHtml">If html should be avoided.</param>
        /// <returns>The parsed html string.</returns>
        public static HtmlString Parse(this Registrant registrant, HtmlString toParse, User user = null, bool noHtml = false)
        {
            return new HtmlString(registrant.Parse(toParse.ToString(), user, noHtml));
        }

        /// <summary>
        /// Parses the registrant and form for any syntax.
        /// </summary>
        /// <param name="registrant">The registrant.</param>
        /// <param name="toParse">The string to parse.</param>
        /// <param name="user">The user.</param>
        /// <param name="noHtml">If html should be avoided.</param>
        /// <returns>The parsed string.</returns>
        public static string Parse(this Registrant registrant, string toParse, User user = null, bool noHtml = false)
        {
            var matches = Syntax.Matches(toParse).Cast<Match>();
            var badMatches = new List<string>();
            foreach (Match match in matches.Reverse())
            {
                toParse = toParse.Remove(match.Index, match.Groups[0].Value.Length);
                var cap = match.Groups[1].Value;
                cap = cap.Replace("&gt;", ">");
                if (cap.Contains("=>"))
                {
                    var prefix = cap.Substring(0, cap.IndexOf("=>")).ToLower().Trim();
                    var suffix = cap.Substring(cap.IndexOf("=>") + 2).ToLower().Trim();
                    switch (prefix)
                    {
                        case "reg":
                        case "registrant":
                        case "inv":
                        case "credit":
                            if (registrant == null)
                                toParse = toParse.Insert(match.Index, noHtml ? "no registrant supplied" : "<i>no registrant supplied</i>");
                            else
                                toParse = toParse.Insert(match.Index, registrant.GetStringValue(suffix, user, noHtml));
                            break;
                        case "contact":
                            if (registrant.Contact == null)
                                toParse = toParse.Insert(match.Index, noHtml ? "no contact supplied" : "<i>no contact supplied</i>");
                            else
                                toParse = toParse.Insert(match.Index, registrant.Contact.GetStringValue(suffix, user, noHtml));
                            break;
                        case "form":
                            if (registrant.Form == null)
                                toParse = toParse.Insert(match.Index, noHtml ? "no form supplied" : "<i>no form supplied</i>");
                            else
                                toParse = toParse.Insert(match.Index, registrant.Form.GetStringValue(suffix, registrant, user, noHtml));
                            break;
                    }
                }
                else
                {
                    if (registrant.Form == null)
                    {
                        toParse = toParse.Insert(match.Index, noHtml ? "no form supplied" : "<i>no form supplied</i>");
                    }
                    else
                    {
                        var customText = registrant.Form.CustomTexts.FirstOrDefault(c => c.Name == cap);
                        var logicText = registrant.Form.LogicBlocks.FirstOrDefault(c => c.Name == cap);
                        if (customText != null || logicText != null)
                        {
                            Data.IContentItem contentItem = customText;
                            if (contentItem == null)
                                contentItem = logicText;
                            toParse = toParse.Insert(match.Index, contentItem.GetStringValue(registrant, noHtml));
                        }
                        else
                        {
                            toParse = toParse.Insert(match.Index, match.Groups[0].Value);
                            badMatches.Add(match.Groups[0].Value);
                        }
                    }
                }
            }
            // Now we see if there are any other matches that haven't already been checked.
            if (Syntax.Matches(toParse).Cast<Match>().Select(m => m.Groups[0].Value).Where(m => !badMatches.Any(b => b == m)).Count() > 0)
                return registrant.Parse(toParse, user, noHtml);
            return toParse;
        }

        /// <summary>
        /// Parses the form for any syntax.
        /// </summary>
        /// <param name="form">The form.</param>
        /// <param name="toParse">The html string to parse.</param>
        /// <param name="user">The user.</param>
        /// <param name="noHtml">If html should be avoided.</param>
        /// <returns>The parsed html string.</returns>
        public static HtmlString Parse(this Form form, HtmlString toParse, User user = null, bool noHtml = false)
        {
            return new HtmlString(Parse(form, toParse.ToString(), user, noHtml));
        }

        /// <summary>
        /// Parses the form for any syntax.
        /// </summary>
        /// <param name="form">The form.</param>
        /// <param name="toParse">The string to parse.</param>
        /// <param name="user">The user.</param>
        /// <param name="noHtml">If html should be avoided.</param>
        /// <returns>The parsed string.</returns>
        public static string Parse(this Form form, string toParse, User user = null, bool noHtml = false)
        {
            var matches = Syntax.Matches(toParse).Cast<Match>();
            var badMatches = new List<string>();
            foreach (Match match in matches.Reverse())
            {
                toParse = toParse.Remove(match.Index, match.Groups[0].Value.Length);
                var cap = match.Groups[1].Value;
                cap = cap.Replace("&gt;", ">");
                if (cap.Contains("=>"))
                {
                    var prefix = cap.Substring(0, cap.IndexOf("=>")).ToLower().Trim();
                    var suffix = cap.Substring(cap.IndexOf("=>") + 2).ToLower().Trim();
                    switch (prefix)
                    {
                        case "form":
                            if (form == null)
                                toParse = toParse.Insert(match.Index, noHtml ? "no form supplied" : "<i>no form supplied</i>");
                            else
                                toParse = toParse.Insert(match.Index, form.GetStringValue(suffix, user, noHtml));
                            break;
                    }
                }
                else
                {
                    if (form == null)
                    {
                        toParse = toParse.Insert(match.Index, noHtml ? "no form supplied" : "<i>no form supplied</i>");
                    }
                    else
                    {
                        var customText = form.CustomTexts.FirstOrDefault(c => c.Name == cap);
                        if (customText != null)
                        {
                            toParse = toParse.Insert(match.Index, customText.Text);
                        }
                        else
                        {
                            toParse = toParse.Insert(match.Index, match.Groups[0].Value);
                            badMatches.Add(match.Groups[0].Value);
                        }
                    }
                }
            }
            // Now we see if there are any other matches that haven't already been checked.
            if (Syntax.Matches(toParse).Cast<Match>().Select(m => m.Groups[0].Value).Where(m => !badMatches.Any(b => b == m)).Count() > 0)
                return form.Parse(toParse, user, noHtml);
            return toParse;
        }


        /// <summary>
        /// Get the string value of a contentitem.
        /// </summary>
        /// <param name="contentItem">The content item to use.</param>
        /// <param name="registrant">The registrant to use.</param>
        /// <param name="noHtml">If html should be used or not.</param>
        /// <returns>The string value.</returns>
        public static string GetStringValue(this Data.IContentItem contentItem, Data.IPerson person, bool noHtml = false)
        {
            if (contentItem is CustomText)
                return (contentItem as CustomText).Text;
            else if (contentItem is LogicBlock)
            {
                if (person == null)
                    return noHtml ? "no person supplied" : "<i>no person supplied</i>";
                var t_lb = contentItem as LogicBlock;
                var registrant = person as Registrant;
                var contact = person as Contact;
                if (registrant != null)
                    contact = registrant.Contact;
                var commands = LogicEngine.RunLogic(t_lb, registrant, contact);
                var display = commands.FirstOrDefault(c => c.Command == JItems.JLogicWork.DisplayText);
                if (display == null)
                    return null;
                return display.Parameters["Text"];
            }
            return noHtml ? "Error" : "<b><u>Error</u></b>";
        }

        /// <summary>
        /// Gets the string value of reg syntax.
        /// </summary>
        /// <param name="registrant">The registrant being used.</param>
        /// <param name="property">The property to use.</param>
        /// <param name="registrant">The registrant to use</param>
        /// <param name="user">The user to use.</param>
        /// <param name="noHtml">If html should be used.</param>
        /// <returns>The string value.</returns>
        public static string GetStringValue(this Registrant registrant, string property, User user = null, bool noHtml = false)
        {
            RSToolKit.Domain.Entities.MerchantAccount.TransactionRequest request = null;
            switch (property)
            {
                case "rsvp":
                    return registrant.RSVP ? registrant.Form.RSVPAccept ?? "Yes" : registrant.Form.RSVPDecline ?? "No";
                case "creditcharge":
                    request = registrant.TransactionRequests.OrderBy(t => t.DateCreated).LastOrDefault();
                    if (request == null)
                        return noHtml ? "no charges" : "<i>no charges</i>";
                    return Math.Round(request.Ammount, 2).ToString();
                case "charge":
                    return (registrant.Fees + registrant.Adjustings - registrant.Transactions).ToString();
                case "lastfour":
                    request = registrant.TransactionRequests.OrderBy(t => t.DateCreated).LastOrDefault();
                    if (request == null)
                        return noHtml ? "no charges" : "<i>no charges</i>";
                    return request.LastFour;
                case "invoicedisplayname":
                    request = registrant.TransactionRequests.OrderBy(t => t.DateCreated).LastOrDefault();
                    if (request == null)
                        return noHtml ? "no charges" : "<i>no charges</i>";
                    var invoiceName = request.MerchantAccount.Company.Name;
                    if (request.MerchantAccount.Company != null && request.MerchantAccount.Company.UId == Guid.Parse("{27e170e9-80bf-4292-82e3-b8fa348352e3}"))
                        invoiceName = "either RegStep Technologies or Registration Assistant";
                    return invoiceName;
                case "invoicedetails":
                case "details":
                    #region Invoice Details
                    var invdet_html = @"<table width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""border-collapse: collapse; font-family: arial; font-size: 12px; color: black;""><thead></thead>";
                    invdet_html += @"<tbody>";
                    invdet_html += @"<tr><td colspan=""4"" style=""padding: 2px; font-family: arial; font-size: 12px;""><b>REGISTRATION FEES</b></td><tr>";
                    invdet_html += @"<tr>";
                    invdet_html += @"<td width=""40%"" style=""padding: 2px; font-family: arial; font-size: 12px;""><b>Item</b></td>";
                    invdet_html += @"<td wdith=""20%"" style=""padding: 2px; font-family: arial; font-size: 12px;"" align=""right""><b>Price</b></td>";
                    invdet_html += @"<td width=""20%"" style=""padding: 2px; font-family: arial; font-size: 12px;"" align=""right""><b>Quantity</b></td>";
                    invdet_html += @"<td width=""20%"" style=""padding: 2px; font-family: arial; font-size: 12px;"" align=""right""><b>Total</b></td>";
                    invdet_html += @"</tr>";
                    var feeTotal = 0.00m;
                    var running_total = 0.00m;
                    foreach (var item in registrant.GetShoppingCartItems().Items)
                    {
                        var item_total = Math.Round(item.Ammount * item.Quanity);
                        feeTotal += Math.Round(item_total, 2);
                        invdet_html += "<tr>";
                        invdet_html += @"<td style=""padding: 2px; font-family: arial; font-size: 12px;"">" + item.Name + "</td>";
                        invdet_html += @"<td style=""padding: 2px; font-family: arial; font-size: 12px;"" align=""right"">" + Math.Round(item.Ammount, 2).ToString("F2", registrant.Form.Culture) + @"</td>";
                        invdet_html += @"<td style=""padding: 2px; font-family: arial; font-size: 12px;"" align=""right"">" + item.Quanity + "</td>";
                        invdet_html += @"<td style=""padding: 2px; font-family: arial; font-size: 12px;"" align=""right"">" + item_total.ToString("F2", registrant.Form.Culture) + "</td>";
                        invdet_html += @"</tr>";
                    }
                    invdet_html += "<tr>";
                    invdet_html += @"<td colspan=""2"" align=""right"" style=""padding: 2px 2px 10px 2px; font-family: arial; font-size: 12px;""><b>TOTAL FEES:</b></td>"; ;
                    invdet_html += @"<td colspan=""2"" align=""right"" style=""padding: 2px 2px 10px 2px; font-family: arial; font-size: 12px;""><b>" + feeTotal.ToString("F2", registrant.Form.Culture) + "</td>";
                    invdet_html += "</tr>";
                    running_total = feeTotal;
                    if (registrant.PromotionalCodes.Count != 0)
                    {
                        // There are promotional codes so we need to include this section.
                        invdet_html += "<tr>";
                        invdet_html += @"<td colspan=""4"" style=""padding: 10px 2px 2px 2px; border-top: 1px solid #dddddd; font-family: arial; font-size: 12px;""><b>DISCOUNTS</b></td>";
                        invdet_html += "</tr>";
                        invdet_html += "<tr>";
                        invdet_html += @"<td colspan=""2"" style=""padding: 2px; font-family: arial; font-size: 12px;""><b>Discount</b></td>";
                        invdet_html += @"<td align=""right"" style=""padding: 2px; font-family: arial; font-size: 12px;""><b>Code Entered</b></td>";
                        invdet_html += @"<td align=""right"" style=""padding: 2px; font-family: arial; font-size: 12px;""><b>Amount</b></td>";
                        invdet_html += "</tr>";
                        var code_runningTotal = 0.00m;
                        foreach (var code in registrant.PromotionalCodes.Where(c => c.Code.Action == ShoppingCartAction.Add))
                        {
                            var code_total = Math.Round(code.Code.Amount, 2) * -1;
                            code_runningTotal += Math.Round(code_total, 2);
                            running_total -= Math.Round(code_total, 2);
                            invdet_html += "<tr>";
                            invdet_html += @"<td colspan=""2"" style=""padding: 2px; font-family: arial; font-size: 12px;"">" + code.Code.Description + "</td>";
                            invdet_html += @"<td align=""right"" style=""padding: 2px; font-family: arial; font-size: 12px;"">" + code.Code.Code + "</td>";
                            invdet_html += @"<td align=""right"" style=""padding: 2px; font-family: arial; font-size: 12px;"">" + (code_total * -1).ToString("F2", registrant.Form.Culture) + "</td>";
                            invdet_html += "</tr>";
                        }
                        foreach (var code in registrant.PromotionalCodes.Where(c => c.Code.Action == ShoppingCartAction.Subtract))
                        {
                            var code_total = Math.Round(code.Code.Amount, 2);
                            code_runningTotal += Math.Round(code_total, 2);
                            running_total -= Math.Round(code_total, 2);
                            invdet_html += "<tr>";
                            invdet_html += @"<td colspan=""2"" style=""padding: 2px; font-family: arial; font-size: 12px;"">" + code.Code.Description + "</td>";
                            invdet_html += @"<td align=""right"" style=""padding: 2px; font-family: arial; font-size: 12px;"">" + code.Code.Code + "</td>";
                            invdet_html += @"<td align=""right"" style=""padding: 2px; font-family: arial; font-size: 12px;"">" + (code_total * -1).ToString("F2", registrant.Form.Culture) + "</td>";
                            invdet_html += "</tr>";
                        }
                        foreach (var code in registrant.PromotionalCodes.Where(c => c.Code.Action == ShoppingCartAction.Multiply))
                        {
                            var code_total = Math.Round((running_total * (1 - code.Code.Amount)), 2);
                            code_runningTotal += Math.Round(code_total, 2);
                            running_total -= Math.Round(code_total, 2);
                            invdet_html += "<tr>";
                            invdet_html += @"<td colspan=""2"" style=""padding: 2px; font-family: arial; font-size: 12px;"">" + code.Code.Description + "</td>";
                            invdet_html += @"<td align=""right"" style=""padding: 2px; font-family: arial; font-size: 12px;"">" + code.Code.Code + "</td>";
                            invdet_html += @"<td align=""right"" style=""padding: 2px; font-family: arial; font-size: 12px;"">" + (code_total * -1).ToString("F2", registrant.Form.Culture) + "</td>";
                            invdet_html += "</tr>";
                        }
                        invdet_html += "<tr>";
                        invdet_html += @"<td colspan=""2"" align=""right"" style=""padding: 2px 2px 10px 2px; font-family: arial; font-size: 12px;""><b>TOTAL DISCOUNTS:</b></td>";
                        invdet_html += @"<td colspan=""2"" align=""right"" style=""padding: 2px 2px 10px 2px; font-family: arial; font-size: 12px;""><b>" + (code_runningTotal * -1).ToString("F2", registrant.Form.Culture) + "</b></td>";
                        invdet_html += "</tr>";
                    }
                    if (registrant.PaysTaxes)
                    {
                        // There is tax, so we must apply the tax.
                        invdet_html += "<tr>";
                        invdet_html += @"<td colspan=""4"" style=""padding: 10px 2px 2px 2px; border-top: 1px solid #dddddd; font-family: arial; font-size: 12px;""><b>TAX</b></td>";
                        invdet_html += "</tr>";
                        var inv_tax = Math.Round(running_total * registrant.Form.Tax.Value, 2);
                        running_total += Math.Round(inv_tax, 2);
                        invdet_html += "<tr>";
                        invdet_html += @"<td colspan=""2"" align=""right"" style=""padding: 2px 2px 10px 2px; font-family: arial; font-size: 12px;""><b>" + registrant.Form.TaxDescription + "</b></td>";
                        invdet_html += @"<td colspan=""2"" align=""right"" style=""padding: 2px 2px 10px 2px; font-family: arial; font-size: 12px;""><b>" + inv_tax.ToString("F2", registrant.Form.Culture) + "</b></td>";
                        invdet_html += "</tr>";
                    }
                    invdet_html += "<tr>";
                    invdet_html += @"<td colspan=""2"" align=""right"" style=""background: #eeeeee; padding: 10px 2px 10px 2px; border-top: 1px solid #dddddd; font-family: arial; font-size: 12px;""><b>REGISTRATION TOTAL:</b></td>";
                    invdet_html += @"<td colspan=""2"" align=""right"" style=""background: #eeeeee; padding: 10px 2px 10px 2px; border-top: 1px solid #dddddd; font-family: arial; font-size: 12px;""><b>" + registrant.Fees.ToString("F2", registrant.Form.Culture) + "</b></td>";
                    invdet_html += "</tr>";
                    if (registrant.Adjustments.Count > 0)
                    {
                        // There are adjustments so we need to set those
                        invdet_html += "<tr>";
                        invdet_html += @"<td colspan=""4"" style=""padding: 10px 2px 2px 2px; border-top: 1px solid #dddddd; font-family: arial; font-size: 12px;""><b>ADJUSTMENTS</b></td>";
                        invdet_html += "</tr>";
                        invdet_html += "<tr>";
                        invdet_html += @"<td colspan=""2"" style=""padding: 2px; font-family: arial; font-size: 12px;""><b>Description</b></td>";
                        invdet_html += @"<td align=""right"" style=""padding: 2px; font-family: arial; font-size: 12px;""><b>Date</b></td>";
                        invdet_html += @"<td align=""right"" style=""padding: 2px; font-family: arial; font-size: 12px;""><b>Amount</b></td>";
                        invdet_html += "</tr>";
                        var adjustments_total = 0.00m;
                        foreach (var adjustment in registrant.Adjustments.Where(a => a.AdjustmentType == "Adjustment").OrderBy(a => a.Date).ToList())
                        {
                            var t_adjAmount = Math.Round(adjustment.Amount, 2);
                            adjustments_total += t_adjAmount;
                            invdet_html += "<tr>";
                            invdet_html += @"<td colspan=""2"" style=""padding: 2px; font-family: arial; font-size: 12px;"">" + adjustment.Name + "</td>";
                            invdet_html += @"<td align=""right"" style=""padding: 2px; font-family: arial; font-size: 12px;"">" + adjustment.Date.LocalDateTime.ToString(registrant.Form.Culture) + "</td>";
                            invdet_html += @"<td align=""right"" style=""padding: 2px; font-family: arial; font-size: 12px;"">" + t_adjAmount.ToString("F2", registrant.Form.Culture) + "</td>";
                            invdet_html += "</tr>";
                        }
                        invdet_html += "<tr>";
                        invdet_html += @"<td colspan=""2"" align=""right"" style=""padding: 2px 2px 10px 2px; font-family: arial; font-size: 12px;""><b>TOTAL ADJUSTMENTS:</b></td>";
                        invdet_html += @"<td colspan=""2"" align=""right"" style=""padding: 2px 2px 10px 2px; font-family: arial; font-size: 12px;""><b>" + adjustments_total.ToString("F2", registrant.Form.Culture) + "</b></td>";
                        invdet_html += "</tr>";
                    }
                    var inv_trans = new List<Data.IFinanceAmmount>();
                    inv_trans.AddRange(registrant.TransactionRequests.ToList());
                    inv_trans.AddRange(registrant.Adjustments.Where(a => a.AdjustmentType != "Adjustment").ToList());
                    if (inv_trans.Count > 0)
                    {
                        // There are valid transaction so we include this section.
                        invdet_html += "<tr>";
                        invdet_html += @"<td colspan=""4"" style=""padding: 10px 2px 2px 2px; border-top: 1px solid #dddddd; font-family: arial; font-size: 12px;""><b>PAYMENTS</b></td>";
                        invdet_html += "</tr>";
                        invdet_html += "<tr>";
                        invdet_html += @"<td colspan=""2"" style=""padding: 2px; font-family: arial; font-size: 12px;""><b>Type</b></td>";
                        invdet_html += @"<td align=""right"" style=""padding: 2px; font-family: arial; font-size: 12px;""><b>Date</b></td>";
                        invdet_html += @"<td align=""right"" style=""padding: 2px; font-family: arial; font-size: 12px;""><b>Amount</b></td>";
                        invdet_html += "</tr>";
                        var payment_total = 0.00m;
                        foreach (var trans in inv_trans.OrderBy(t => t.DateCreated))
                        {
                            var t_name = trans.Name;
                            if (trans is Entities.MerchantAccount.Adjustment)
                                t_name = (trans as Entities.MerchantAccount.Adjustment).AdjustmentType + " Recorded";
                            var t_total_n = trans.TotalAmount();
                            var t_total = Math.Round((t_total_n.HasValue ? t_total_n.Value : 0.00m), 2);
                            payment_total += t_total;
                            invdet_html += "<tr>";
                            invdet_html += @"<td colspan=""2"" style=""padding: 2px; font-family: arial; font-size: 12px;"">" + t_name + "</td>";
                            invdet_html += @"<td align=""right"" style=""padding: 2px; font-family: arial; font-size: 12px;"">" + trans.DateCreated.LocalDateTime.ToString(registrant.Form.Culture) + "</td>";
                            invdet_html += @"<td align=""right"" style=""padding: 2px; font-family: arial; font-size: 12px;"">" + (t_total * -1).ToString("F2", registrant.Form.Culture) + "</td>";
                            invdet_html += "</tr>";
                        }
                        invdet_html += "<tr>";
                        invdet_html += @"<td colspan=""2"" align=""right"" style=""padding: 2px 2px 10px 2px; font-family: arial; font-size: 12px;""><b>TOTAL PAYMENTS:</b></td>";
                        invdet_html += @"<td colspan=""2"" align=""right"" style=""padding: 2px 2px 10px 2px; font-family: arial; font-size: 12px;""><b>" + (payment_total * -1).ToString("F2", registrant.Form.Culture) + "</b></td>";
                        invdet_html += "</tr>";
                    }
                    invdet_html += "<tr>";
                    invdet_html += @"<td colspan=""2"" align=""right"" style=""background: #eeeeee; padding: 10px 2px 10px 2px; border-top: 1px solid #dddddd; font-family: arial; font-size: 12px;""><b>BALANCE:</b></td>";
                    invdet_html += @"<td colspan=""2"" align=""right"" style=""background: #eeeeee; padding: 10px 2px 10px 2px; border-top: 1px solid #dddddd; font-family: arial; font-size: 12px;""><b>" + registrant.TotalOwed.ToString("F2", registrant.Form.Culture) + "</b></td>";
                    invdet_html += "</tr>";
                    invdet_html += "</tbody>";
                    invdet_html += "</table>";
                    return invdet_html;
                #endregion
                case "date":
                    return registrant.DateCreated.ToString(user);
                case "email":
                    return registrant.Email;
                case "invoicenumber":
                    return registrant.InvoiceNumber;
                case "confirmation":
                    return registrant.Confirmation;
                case "key":
                    return registrant.UId.ToString();
                case "payingagentnumber":
                    return registrant.PayingAgentNumber;
                case "payingagentname":
                    return registrant.PayingAgentName;
                case "audience":
                    if (registrant.Audience == null)
                        return noHtml ? "no audience" : "<i>no audience</i>";
                    return registrant.Audience.Name;
                case "tax":
                    return Math.Round(registrant.Taxes, 2).ToString("c", registrant.Form.Culture);
                case "fees":
                    return Math.Round(registrant.Fees, 2).ToString("c", registrant.Form.Culture);
                case "adjustments":
                case "adjustings":
                    return Math.Round(registrant.Adjustings, 2).ToString("c", registrant.Form.Culture);
                case "totalowed":
                    return Math.Round(registrant.TotalOwed, 2).ToString("c", registrant.Form.Culture);
                case "attended":
                    return registrant.Attended ? "Yes" : "No";
                default:
                    using (var context = new Data.EFDbContext())
                    {
                        Data.IComponent component;
                        long lid;
                        Guid uid;
                        if (long.TryParse(property, out lid))
                            component = context.Components.Where(c => c.SortingId == lid).FirstOrDefault();
                        else if (Guid.TryParse(property, out uid))
                            component = context.Components.Where(c => c.UId == uid).FirstOrDefault();
                        else
                            component = context.Components.Where(c => c.Variable != null && c.Variable.Value == property && c.Panel.Page.FormKey == registrant.FormKey).FirstOrDefault();
                        if (component == null)
                            return noHtml ? "component does not exist" : "<i>component does not exist</i>";
                        var data = registrant.Data.FirstOrDefault(d => d.VariableUId == component.UId);
                        if (data != null)
                            return data.GetPretty(!noHtml);
                    }
                    return noHtml ? "no data" : "</i>no data</i>";
            }
        }

        /// <summary>
        /// Gets the string value of reg syntax.
        /// </summary>
        /// <param name="contact">The contact being used.</param>
        /// <param name="property">The property to use.</param>
        /// <param name="user">The user to use.</param>
        /// <param name="noHtml">If html should be used.</param>
        /// <returns>The string value.</returns>
        public static string GetStringValue(this Contact contact, string property, User user = null, bool noHtml = false)
        {
            switch (property)
            {
                case "email":
                    return contact.Email;
                case "date":
                case "datecreated":
                    return contact.DateCreated.ToString(user, "rs_s");
                case "datemodified":
                case "lastupdate":
                    return contact.DateModified.ToString(user, "rs_s");
                default:
                    using (var context = new Data.EFDbContext())
                    {
                        ContactHeader header;
                        long lid;
                        Guid uid;
                        if (long.TryParse(property, out lid))
                            header = context.ContactHeaders.Where(c => c.SortingId == lid).FirstOrDefault();
                        else if (Guid.TryParse(property, out uid))
                            header = context.ContactHeaders.Where(c => c.UId == uid).FirstOrDefault();
                        else
                            header = context.ContactHeaders.Where(c => c.Name == property).FirstOrDefault();
                        if (header == null)
                            return noHtml ? "contact header does not exist" : "<i>contact header does not exist</i>";
                        var data = contact.Data.FirstOrDefault(d => d.HeaderKey == header.UId);
                        if (data != null)
                            return data.PrettyValue;
                    }
                    return noHtml ? "no data" : "</i>no data</i>";
            }
        }

        /// <summary>
        /// Gets the string value of reg syntax.
        /// </summary>
        /// <param name="form">The form being used.</param>
        /// <param name="property">The property to use.</param>
        /// <param name="registrant">The registrant to use.</param>
        /// <param name="user">The user to use.</param>
        /// <param name="noHtml">If html should be used.</param>
        /// <returns>The string value.</returns>
        public static string GetStringValue(this Form form, string property, Registrant registrant, User user = null, bool noHtml = false)
        {
            if (registrant == null)
                return noHtml ? "no registrant" : "<i>no registrant</i>";
            switch (property)
            {
                case "continueregistrationurl":
                    if (registrant != null)
                        return "https://toolkit.regstep.com/Register/continueregistration/" + registrant.UId;
                    return noHtml ? "no registrant" : "<i>no registrant</i>";
                case "editregistrationurl":
                    if (registrant != null)
                        return "https://toolkit.regstep.com/Register/Edit/" + registrant.UId;
                    return noHtml ? "no registrant" : "<i>no registrant</i>";
                case "cancelregistrationurl":
                    if (registrant != null)
                        return "https://toolkit.regstep.com/Register/Cancel/" + registrant.UId;
                    return noHtml ? "no registrant" : "<i>no registrant</i>";
                case "confirmationurl":
                    if (registrant != null)
                        return "https://toolkit.regstep.com/Register/ShowConfirmation/" + registrant.UId;
                    return noHtml ? "no registrant" : "<i>no registrant</i>";
                case "continueregistrationlink":
                    if (registrant != null)
                        return "toolkit.regstep.com/Register/continueregistration/" + registrant.UId;
                    return noHtml ? "no registrant" : "<i>no registrant</i>";
                case "editregistrationlink":
                    if (registrant != null)
                        return "toolkit.regstep.com/Register/Edit/" + registrant.UId;
                    return noHtml ? "no registrant" : "<i>no registrant</i>";
                case "cancelregistrationlink":
                    if (registrant != null)
                        return "toolkit.regstep.com/Register/Cancel/" + registrant.UId;
                    return noHtml ? "no registrant" : "<i>no registrant</i>";
                case "confirmationlink":
                    if (registrant != null)
                        return "toolkit.regstep.com/Register/ShowConfirmation/" + registrant.UId;
                    return noHtml ? "no registrant" : "<i>no registrant</i>";
                case "smartlink":
                    if (registrant != null && registrant.Contact == null)
                        return "https://toolkit.regstep.com/Register/SmartLink/" + form.SortingId + "/" + registrant.Confirmation;
                    else if (registrant.Contact != null)
                        return "https://toolkit.regstep.com/Register/SmartLink/" + form.SortingId + "/" + registrant.Contact.Email;
                    else
                        return "https://toolkit.regstep.com/Register/SmartLink/" + form.SortingId + "/Enter Email";
            }
            return form.GetStringValue(property, user, noHtml);
        }

        /// <summary>
        /// Gets the string value of reg syntax.
        /// </summary>
        /// <param name="form">The form being used.</param>
        /// <param name="property">The property to use.</param>
        /// <param name="contact">The contact to use.</param>
        /// <param name="user">The user to use.</param>
        /// <param name="noHtml">If html should be used.</param>
        /// <returns>The string value.</returns>
        public static string GetStringValue(this Form form, string property, Contact contact, User user = null, bool noHtml = false)
        {
            if (contact == null)
                return noHtml ? "no contact" : "<i>no contact</i>";
            switch (property)
            {
                case "smartlink":
                    return "https://toolkit.regstep.com/Register/SmartLink/" + form.SortingId + "/" + contact.Email;
            }
            return form.GetStringValue(property, user, noHtml);
        }


        /// <summary>
        /// Gets the string value for properties of a form that do not need a registrant.
        /// </summary>
        /// <param name="form">The form being used.</param>
        /// <param name="property">The property.</param>
        /// <param name="user">The user.</param>
        /// <param name="noHtml">Flag to ignore html.</param>
        /// <returns>The parsed string</returns>
        public static string GetStringValue(this Form form, string property, User user = null, bool noHtml = false)
        {
            switch (property.ToLower())
            {
                case "coordinatorname":
                    return form.CoordinatorName;
                case "coordinatoremail":
                    return form.CoordinatorEmail;
                case "coordinatorphone":
                    return form.CoordinatorPhone;
                case "closedate":
                    return form.Close.ToString(user);
                case "eventdata":
                    return form.Open.ToString(user);
                case "name":
                    return form.Name;
            }
            return noHtml ? "invalid property" : "<i>invalid property</i>";
        }
    }
}
