using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using RSToolKit.Domain.Entities.Clients;
using RSToolKit.Domain.Entities.Components;
using RSToolKit.Domain.Entities;
using RSToolKit.Domain.Entities.Email;
using RSToolKit.Domain.Entities.MerchantAccount;
using System.Text.RegularExpressions;
using System.Reflection;
using RSToolKit.Domain.Engines;
using RSToolKit.Domain.JItems;

namespace RSToolKit.Domain.Data
{
    /*
    /// <summary>
    /// Holds the information to parse the string for regstep syntax.
    /// </summary>
    public static class RegStepParser
    {
        private static Regex SyntaxFinder;

        private static Dictionary<string, string> RegistrantValues;

        private static Dictionary<string, string> FormValues;

        private static Dictionary<string, string> ContactValues;

        private static Dictionary<string, string> CompanyValues;

        static RegStepParser()
        {
            SyntaxFinder = new Regex(@"\[([^\]]*)\]", RegexOptions.IgnoreCase | RegexOptions.Compiled);

            ContactValues = new Dictionary<string, string>()
            {
                { "name", "Name" }
            };

            ContactValues = new Dictionary<string, string>()
            {
                { "email", "Email" }
            };

            RegistrantValues = new Dictionary<string, string>()
            {
                { "date", "DateCreated" },
                { "email", "Email" },
                { "invoicenumber", "InvoiceNumber" },
                { "confirmation", "Confirmation" },
                { "key", "UId" },
                { "payingagentnumber", "PayingAgentNumber" },
                { "payingagentname", "PayingAgentName" },
                { "rsvp", "nr_rsvp" },
                { "audience", "Audience" },
                { "charge", "nr_charge" },
                { "invoicedisplayname", "nr_invoicedisplayname" },
                { "details", "nr_invoicedetails" },
                { "lastfour", "nr_lastfour" }
            };

            FormValues = new Dictionary<string, string>()
            {
                { "name", "Name" },
                { "continueregistrationurl", "nr_continueregistrationurl" },
                { "cancelregistrationurl", "nr_cancelregistrationurl" },
                { "confirmationurl", "nr_confirmationurl" },
                { "editregistrationurl", "nr_editregistrationurl" },
                { "continueregistrationlink", "nr_continueregistrationlink" },
                { "cancelregistrationlink", "nr_cancelregistrationlink" },
                { "editregistrationlink", "nr_editregistrationlink" },
                { "confirmationlink", "nr_confirmationlink" },
                { "coordinatorname", "CoordinatorName" },
                { "coordinatoremail", "CoordinatorEmail" },
                { "coordinatorphone", "CoordinatorPhone" },
                { "closedate", "Close" },
                { "eventdate", "Open" },
                { "smartlink", "nr_smartlink" }
            };
        }
    }
    //*/

    public class RSParser
    {
        #region Variables
        public Registrant Registrant { get; set; }
        public EFDbContext Context { get; set; }
        public Form Form { get; set; }
        public EmailCampaign EmailCampaign { get; set; }
        public Contact Contact { get; set; }
        public TransactionRequest Request { get; set; }

        private int pr_iterations = 0;
        private readonly Regex pr_rgx_content = new Regex(@"\[(?<name>[^\]]+)\]", RegexOptions.IgnoreCase);
        private readonly List<RSParserInfo> pr_rgx_contact = new List<RSParserInfo>()
        {
            new RSParserInfo() { RGX = new Regex(@"\[contact\s*=(?:>|(?:&gt;))\s*email\]", RegexOptions.IgnoreCase), Method = "Email" }
        };
        private readonly List<RSParserInfo> pr_rgx_company = new List<RSParserInfo>()
        {
            new RSParserInfo() { RGX = new Regex(@"\[company\s*=(?:>|(?:&gt;))\s*name\]", RegexOptions.IgnoreCase), Method = "Name" }
        };
        private readonly List<RSParserInfo> pr_rgx_form = new List<RSParserInfo>()
        {
            new RSParserInfo() { RGX = new Regex(@"\[form\s*=(?:>|(?:&gt;))\s*name\]", RegexOptions.IgnoreCase), Method = "Name" },
            new RSParserInfo() { RGX = new Regex(@"\[form\s*=(?:>|(?:&gt;))\s*continueregistrationurl\]", RegexOptions.IgnoreCase), Method = "nr_continueregistrationurl" },
            new RSParserInfo() { RGX = new Regex(@"\[form\s*=(?:>|(?:&gt;))\s*cancelregistrationurl\]", RegexOptions.IgnoreCase), Method = "nr_cancelregistrationurl" },
            new RSParserInfo() { RGX = new Regex(@"\[form\s*=(?:>|(?:&gt;))\s*confirmationurl\]", RegexOptions.IgnoreCase), Method = "nr_confirmationurl" },
            new RSParserInfo() { RGX = new Regex(@"\[form\s*=(?:>|(?:&gt;))\s*editregistrationurl\]", RegexOptions.IgnoreCase), Method = "nr_editregistrationurl" },
            new RSParserInfo() { RGX = new Regex(@"\[form\s*=(?:>|(?:&gt;))\s*continueregistrationlink\]", RegexOptions.IgnoreCase), Method = "nr_continueregistrationlink" },
            new RSParserInfo() { RGX = new Regex(@"\[form\s*=(?:>|(?:&gt;))\s*cancelregistrationlink\]", RegexOptions.IgnoreCase), Method = "nr_cancelregistrationlink" },
            new RSParserInfo() { RGX = new Regex(@"\[form\s*=(?:>|(?:&gt;))\s*editregistrationlink\]", RegexOptions.IgnoreCase), Method = "nr_editregistrationlink" },
            new RSParserInfo() { RGX = new Regex(@"\[form\s*=(?:>|(?:&gt;))\s*confirmationlink\]", RegexOptions.IgnoreCase), Method = "nr_confirmationlink" },
            new RSParserInfo() { RGX = new Regex(@"\[form\s*=(?:>|(?:&gt;))\s*coordinatorname\]", RegexOptions.IgnoreCase), Method = "CoordinatorName" },
            new RSParserInfo() { RGX = new Regex(@"\[form\s*=(?:>|(?:&gt;))\s*coordinatoremail\]", RegexOptions.IgnoreCase), Method = "CoordinatorEmail" },
            new RSParserInfo() { RGX = new Regex(@"\[form\s*=(?:>|(?:&gt;))\s*coordinatorphone\]", RegexOptions.IgnoreCase), Method = "CoordinatorPhone" },
            new RSParserInfo() { RGX = new Regex(@"\[form\s*=(?:>|(?:&gt;))\s*closedate\]", RegexOptions.IgnoreCase), Method = "Close" },
            new RSParserInfo() { RGX = new Regex(@"\[form\s*=(?:>|(?:&gt;))\s*eventdate\]", RegexOptions.IgnoreCase), Method = "Open" },
            new RSParserInfo() { RGX = new Regex(@"\[form\s*=(?:>|(?:&gt;))\s*smartlink\]", RegexOptions.IgnoreCase), Method = "nr_smartlink" }
        };
        private readonly List<RSParserInfo> pr_rgx_registrant = new List<RSParserInfo>()
        {
            new RSParserInfo() { RGX = new Regex(@"\[reg\s*=(?:>|(?:&gt;))\s*date\]", RegexOptions.IgnoreCase), Method = "DateCreated" },
            new RSParserInfo() { RGX = new Regex(@"\[reg\s*=(?:>|(?:&gt;))\s*email\]", RegexOptions.IgnoreCase), Method = "Email" },
            new RSParserInfo() { RGX = new Regex(@"\[reg\s*=(?:>|(?:&gt;))\s*invoicenumber\]", RegexOptions.IgnoreCase), Method = "InvoiceNumber" },
            new RSParserInfo() { RGX = new Regex(@"\[reg\s*=(?:>|(?:&gt;))\s*confirmation\]", RegexOptions.IgnoreCase), Method = "Confirmation" },
            new RSParserInfo() { RGX = new Regex(@"\[reg\s*=(?:>|(?:&gt;))\s*key\]", RegexOptions.IgnoreCase), Method = "UId" },
            new RSParserInfo() { RGX = new Regex(@"\[reg\s*=(?:>|(?:&gt;))\s*payingagentnumber\]", RegexOptions.IgnoreCase), Method = "PayingAgentNumber" },
            new RSParserInfo() { RGX = new Regex(@"\[reg\s*=(?:>|(?:&gt;))\s*payingagentname\]", RegexOptions.IgnoreCase), Method = "PayingAgentName" },
            new RSParserInfo() { RGX = new Regex(@"\[reg\s*=(?:>|(?:&gt;))\s*rsvp\]", RegexOptions.IgnoreCase), Method = "nr_rsvp" },
            new RSParserInfo() { RGX = new Regex(@"\[reg\s*=(?:>|(?:&gt;))\s*audience\]", RegexOptions.IgnoreCase), Method = "Audience" },
            new RSParserInfo() { RGX = new Regex(@"\[reg\s*=(?:>|(?:&gt;))\s*charge\]", RegexOptions.IgnoreCase), Method = "nr_charge" },
            new RSParserInfo() { RGX = new Regex(@"\[inv\s*=(?:>|(?:&gt;))\s*invoicedisplayname\]", RegexOptions.IgnoreCase), Method = "nr_invoicedisplayname" },
            new RSParserInfo() { RGX = new Regex(@"\[inv\s*=(?:>|(?:&gt;))\s*details\]", RegexOptions.IgnoreCase), Method = "nr_invoicedetails" },
            new RSParserInfo() { RGX = new Regex(@"\[inv\s*=(?:>|(?:&gt;))\s*charge\]", RegexOptions.IgnoreCase), Method = "nr_charge" },
            new RSParserInfo() { RGX = new Regex(@"\[credit\s*=(?:>|(?:&gt;))\s*invoicedisplayname\]", RegexOptions.IgnoreCase), Method = "nr_invoicedisplayname" },
            new RSParserInfo() { RGX = new Regex(@"\[credit\s*=(?:>|(?:&gt;))\s*charge\]", RegexOptions.IgnoreCase), Method = "nr_creditcharge" },
            new RSParserInfo() { RGX = new Regex(@"\[credit\s*=(?:>|(?:&gt;))\s*lastfour\]", RegexOptions.IgnoreCase), Method = "nr_lastfour" }
        };
        private readonly Regex pr_rgx_reg = new Regex(@"\[reg\s*=(?:>|(?:&gt;))\s*(?<name>[^\]]*)\]", RegexOptions.IgnoreCase);

        public readonly Dictionary<string, string> RegSyn = new Dictionary<string, string>()
            {
                { "date", "DateCreated" },
                { "email", "Email" },
                { "invoicenumber", "InvoiceNumber" },
                { "confirmation", "Confirmation" },
                { "key", "UId" },
                { "payingagentnumber", "PayingAgentNumber" },
                { "payingagentname", "PayingAgentName" },
                { "rsvp", "nr_rsvp" },
                { "audience", "Audience" },
                { "charge", "nr_charge" },
                { "invoicedisplayname", "nr_invoicedisplayname" },
                { "details", "nr_invoicedetails" },
                { "lastfour", "nr_lastfour" }
            };

        #endregion


        public RSParser(Registrant registrant = null, FormsRepository repository = null, Form form = null, EmailCampaign campaign = null, Contact contact = null)
        {
            Registrant = registrant;
            Context = repository.Context;
            Form = form;
            EmailCampaign = campaign;
            Contact = contact;
        }

        public RSParser(Registrant registrant = null, EFDbContext context = null, Form form = null, EmailCampaign campaign = null, Contact contact = null)
        {
            Registrant = registrant;
            Context = context;
            Form = form;
            EmailCampaign = campaign;
            Contact = contact;
        }

        public string ParseEmailCampaign(string toParse)
        {
            if (toParse == null)
                return null;
            if (EmailCampaign != null)
                throw new ArgumentNullException("The class was initialized with no email campaign.");
            toParse = ParseCompany(EmailCampaign.Company, toParse);
            return toParse;
        }

        protected string ParseCompany(Company company, string toParse)
        {
            if (toParse == null)
                return null;
            foreach (var rgx in pr_rgx_company)
                toParse = rgx.RGX.Replace(toParse, GetProperty(company, rgx.Method));
            return toParse;
        }

        public string ParseForm(string toParse)
        {
            if (toParse == null)
                return null;
            if (Form == null)
                throw new ArgumentNullException("The class was initialized with no form.");
            toParse = ParseCompany(Form.Company, toParse);
            foreach (var rgx in pr_rgx_form)
                toParse = rgx.RGX.Replace(toParse, GetProperty(Form, rgx.Method));
            return toParse;
        }

        public string ParseAllAvailable(string toParse)
        {
            if (toParse == null)
                return toParse;
            #region Registrant
            foreach (Match match in Regex.Matches(toParse, @"\[([^\]]*)\]"))
            {
                var cap = match.Groups[1].Value;
                cap = cap.Replace("&gt;", ">");
                if (cap.Contains("=>"))
                {
                    var prefix = cap.Substring(0, cap.IndexOf("=>")).ToLower().Trim();
                    var suffix = cap.Substring(cap.IndexOf("=>") + 2).ToLower().Trim();
                    switch (prefix)
                    {
                        case "reg":
                            if (Registrant == null)
                                continue;
                            if (RegSyn.ContainsKey(suffix))
                            {
                                toParse = toParse.Remove(match.Index, match.Groups[0].Value.Length);
                                toParse = toParse.Insert(match.Index, GetProperty(Registrant, RegSyn[suffix]));
                            }
                            continue;
                    }
                }
                else
                {
                    if (Form == null)
                        continue;
                    var customText = Form.CustomTexts.FirstOrDefault(c => c.Name == cap);
                    var logicText = Form.LogicBlocks.FirstOrDefault(c => c.Name == cap);
                    String replaceText = null;
                    if (customText != null)
                        replaceText = ParseContentItem(customText);
                    else if (logicText != null)
                        replaceText = ParseContentItem(logicText);
                    if (replaceText != null)
                    {
                        toParse = toParse.Remove(match.Index, match.Groups[0].Value.Length);
                        toParse = toParse.Insert(match.Index, replaceText);
                    }
                }
            }
            return toParse;
            #endregion
        }

        public string ParseRegistrant(string toParse)
        {
            if (toParse == null)
                return null;
            if (Registrant == null)
                throw new ArgumentNullException("The class was initialized with no registrant.  Parse all requires a registrant.");
            if (Context == null)
                throw new ArgumentNullException("The class was initialized with no repository.  Parse all requires a repository.");
            // Lets see if a contact is available
            if (Contact == null)
            {
                var contact = Context.Contacts.FirstOrDefault(c => c.CompanyKey == Registrant.Form.CompanyKey && c.Name.ToLower() == Registrant.Email.ToLower());
                if (contact != null)
                    Contact = contact;
            }
            //Parse registrant fields
            foreach (var rgx in pr_rgx_registrant)
            {
                if (rgx.RGX.IsMatch(toParse))
                {
                    var prev = toParse;
                    toParse = rgx.RGX.Replace(toParse, GetProperty(Registrant, rgx.Method));
                    var now = toParse;
                }
            }
            foreach (var dp in Registrant.Data)
            {
                if (dp.Component == null)
                    continue;
                if (dp.Component.Variable == null)
                    continue;
                string insertValue = dp.GetFormattedValue();
                if (dp.Empty())
                    insertValue = "";
                var variable = dp.Component.Variable.Value.ToLower();
                toParse = Regex.Replace(toParse, @"\[reg\s*=(?:>|(?:&gt;))\s*" + dp.Component.Variable.Value.ToLower() + @"\]", insertValue ?? "", RegexOptions.IgnoreCase);
            }
            toParse = pr_rgx_reg.Replace(toParse, "");
            return toParse;
        }

        public string ParseAll(string toParse)
        {
            if (toParse == null)
                return null;
            var parsed = toParse;
            if (Registrant == null)
                throw new ArgumentNullException("The class was initialized with no registrant.  Parse all requires a registrant.");
            if (Form == null)
                throw new ArgumentNullException("The class was initialized with no form.  Parse all requires a form.");
            if (Context == null)
                throw new ArgumentNullException("The class was initialized with no repository.  Parse all requires a repository.");

            parsed = ParseForm(parsed);
            parsed = ParseRegistrant(parsed);
            parsed = ParseContent(parsed);


            return parsed;
        }

        public string ParseAvailable(string toParse)
        {
            string parse = toParse;
            if (toParse == null)
                return null;
            parse = ParseContent(parse);
            if (Form != null)
                parse = ParseForm(parse);
            if (EmailCampaign != null)
                parse = ParseEmailCampaign(parse);
            if (Registrant != null)
                parse = ParseRegistrant(parse);
            if (Contact != null)
                parse = ParseContact(parse);
            return parse;
        }

        public string ParseContact(string toParse)
        {
            if (toParse == null)
                return null;
            var parsed = toParse;
            if (Contact == null)
                throw new ArgumentNullException("The class was initialized with no registrant.  Parse all requires a registrant.");
            if (Context == null)
                throw new ArgumentNullException("The class was initialized with no repository.  Parse all requires a repository.");
            //Parse registrant fields
            foreach (var rgx in pr_rgx_contact)
                parsed = rgx.RGX.Replace(parsed, rgx.Method);
            foreach (var dp in Contact.Data)
            {
                string insertValue = dp.GetFormattedValue();
                if (dp.Value == null)
                    insertValue = "";
                var variable = dp.Header.Name;
                parsed = Regex.Replace(parsed, @"\[contact\s*=(?:>|(?:&gt;))\s*" + variable + @"\]", insertValue, RegexOptions.IgnoreCase);
            }
            return parsed;
        }

        public string ParseContent(string toParse, int iterations = 0)
        {
            if (toParse == null)
                return null;
            if (iterations > 25)
                return toParse;
            var t_contents = new List<IContentItem>();
            if (Form != null)
                t_contents.AddRange(Form.ContentItems);
            if (EmailCampaign != null)
                t_contents.AddRange(EmailCampaign.ContentItems);
            var r_string = toParse;
            var replacements = 0;
            foreach (Match match in pr_rgx_content.Matches(r_string))
            {
                var t_content = t_contents.FirstOrDefault(c => c.Name != null &&  c.Name.ToLower() == match.Groups["name"].Value.ToLower());
                if (t_content == null)
                    continue;
                var replaceWith = ParseContentItem(t_content);
                if (replaceWith == null)
                    continue;
                replacements++;
                r_string = Regex.Replace(r_string, @"\[" + match.Groups["name"].Value + @"\]", ParseContentItem(t_content), RegexOptions.IgnoreCase);
            }
            if (replacements == 0)
                return r_string;
            else
                return ParseContent(r_string, ++iterations);
        }

        protected string ParseContentItem(IContentItem contentItem)
        {
            if (contentItem is CustomText)
                return (contentItem as CustomText).Text;
            else if (contentItem is LogicBlock)
            {
                var t_lb = contentItem as LogicBlock;
                var commands = LogicEngine.RunLogic(t_lb, Registrant, Contact);
                var display = commands.FirstOrDefault(c => c.Command == JLogicWork.DisplayText);
                if (display == null)
                    return null;
                return display.Parameters["Text"];
                    
            }
            return "[" + contentItem.Name + "]";
        }

        public string GetProperty(object obj, string property)
        {
            var html = "";
            if (property.StartsWith("nr_"))
            {
                var info = property.Substring(3);
                switch (info)
                {
                    #region Form
                    case "continueregistrationurl":
                        if (Registrant != null)
                            html = "https://toolkit.regstep.com/Register/continueregistration/" + Registrant.UId;
                        break;
                    case "editregistrationurl":
                        if (Registrant != null)
                            html = "https://toolkit.regstep.com/Register/Edit/" + Registrant.UId;
                        break;
                    case "cancelregistrationurl":
                        if (Registrant != null)
                            html = "https://toolkit.regstep.com/Register/Cancel/" + Registrant.UId;
                        break;
                    case "confirmationurl":
                        if (Registrant != null)
                            html = "https://toolkit.regstep.com/Register/ShowConfirmation/" + Registrant.UId;
                        break;
                    case "continueregistrationlink":
                        if (Registrant != null)
                            html = "toolkit.regstep.com/Register/continueregistration/" + Registrant.UId;
                        break;
                    case "editregistrationlink":
                        if (Registrant != null)
                            html = "toolkit.regstep.com/Register/Edit/" + Registrant.UId;
                        break;
                    case "cancelregistrationlink":
                        if (Registrant != null)
                            html = "toolkit.regstep.com/Register/Cancel/" + Registrant.UId;
                        break;
                    case "confirmationlink":
                        if (Registrant != null)
                            html = "toolkit.regstep.com/Register/ShowConfirmation/" + Registrant.UId;
                        break;
                    case "smartlink":
                        if (Registrant != null)
                            html += "https://toolkit.regstep.com/Register/SmartLink/" + Form.SortingId + "/" + Registrant.Confirmation;
                        else if (Contact != null)
                            html += "https://toolkit.regstep.com/Register/SmartLink/" + Form.SortingId + "/" + Contact.Email;
                        else
                            html += "https://toolkit.regstep.com/Register/SmartLink/" + Form.SortingId + "/Enter Email";
                        break;
                    #endregion
                    #region Registrant
                    case "rsvp":
                        if (Registrant != null)
                            html = Registrant.RSVP ? Registrant.Form.RSVPAccept ?? "Yes" : Registrant.Form.RSVPDecline ?? "No";
                        break;
                    case "creditcharge":
                        if (Registrant != null)
                        {
                            if (Request == null)
                                Request = Registrant.TransactionRequests.OrderBy(t => t.DateCreated).LastOrDefault();
                            if (Request != null)
                                html = Math.Round(Request.Ammount, 2).ToString();

                        }
                        break;
                    case "charge":
                        if (Registrant != null)
                            html = (Registrant.Fees + Registrant.Adjustings - Registrant.Transactions).ToString();
                        break;
                    case "lastfour":
                        if (Registrant != null)
                        {
                            if (Request == null)
                                Request = Registrant.TransactionRequests.OrderBy(t => t.DateCreated).LastOrDefault();
                            if (Request != null)
                                html = Request.LastFour;
                        }
                        break;
                    case "invoicedisplayname":
                        if (Registrant != null)
                        {
                            if (Request == null)
                                Request = Registrant.TransactionRequests.OrderBy(t => t.DateCreated).LastOrDefault();
                            if (Request != null)
                            {
                                var invoiceName = Request.MerchantAccount.Company.Name;
                                if (Request.MerchantAccount.Company != null && Request.MerchantAccount.Company.UId == Guid.Parse("{27e170e9-80bf-4292-82e3-b8fa348352e3}"))
                                    invoiceName = "either RegStep Technologies or Registration Assistant";
                                html = invoiceName;
                            }
                        }
                        break;
                    case "invoicedetails":
                        if (Registrant != null)
                        {
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
                            foreach (var item in Registrant.GetShoppingCartItems().Items)
                            {
                                var item_total = Math.Round(item.Ammount * item.Quanity);
                                feeTotal += Math.Round(item_total, 2);
                                invdet_html += "<tr>";
                                invdet_html += @"<td style=""padding: 2px; font-family: arial; font-size: 12px;"">" + item.Name + "</td>";
                                invdet_html += @"<td style=""padding: 2px; font-family: arial; font-size: 12px;"" align=""right"">" + Math.Round(item.Ammount, 2).ToString("F2", Form.Culture) + @"</td>";
                                invdet_html += @"<td style=""padding: 2px; font-family: arial; font-size: 12px;"" align=""right"">" + item.Quanity + "</td>";
                                invdet_html += @"<td style=""padding: 2px; font-family: arial; font-size: 12px;"" align=""right"">" + item_total.ToString("F2", Form.Culture) + "</td>";
                                invdet_html += @"</tr>";
                            }
                            invdet_html += "<tr>";
                            invdet_html += @"<td colspan=""2"" align=""right"" style=""padding: 2px 2px 10px 2px; font-family: arial; font-size: 12px;""><b>TOTAL FEES:</b></td>"; ;
                            invdet_html += @"<td colspan=""2"" align=""right"" style=""padding: 2px 2px 10px 2px; font-family: arial; font-size: 12px;""><b>" + feeTotal.ToString("F2", Form.Culture) + "</td>";
                            invdet_html += "</tr>";
                            running_total = feeTotal;
                            if (Registrant.PromotionalCodes.Count != 0)
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
                                foreach (var code in Registrant.PromotionalCodes.Where(c => c.Code.Action == ShoppingCartAction.Add))
                                {
                                    var code_total = Math.Round(code.Code.Amount, 2) * -1;
                                    code_runningTotal += Math.Round(code_total, 2);
                                    running_total -= Math.Round(code_total, 2);
                                    invdet_html += "<tr>";
                                    invdet_html += @"<td colspan=""2"" style=""padding: 2px; font-family: arial; font-size: 12px;"">" + code.Code.Description + "</td>";
                                    invdet_html += @"<td align=""right"" style=""padding: 2px; font-family: arial; font-size: 12px;"">" + code.Code.Code + "</td>";
                                    invdet_html += @"<td align=""right"" style=""padding: 2px; font-family: arial; font-size: 12px;"">" + (code_total * -1).ToString("F2", Form.Culture) + "</td>";
                                    invdet_html += "</tr>";
                                }
                                foreach (var code in Registrant.PromotionalCodes.Where(c => c.Code.Action == ShoppingCartAction.Subtract))
                                {
                                    var code_total = Math.Round(code.Code.Amount, 2);
                                    code_runningTotal += Math.Round(code_total, 2);
                                    running_total -= Math.Round(code_total, 2);
                                    invdet_html += "<tr>";
                                    invdet_html += @"<td colspan=""2"" style=""padding: 2px; font-family: arial; font-size: 12px;"">" + code.Code.Description + "</td>";
                                    invdet_html += @"<td align=""right"" style=""padding: 2px; font-family: arial; font-size: 12px;"">" + code.Code.Code + "</td>";
                                    invdet_html += @"<td align=""right"" style=""padding: 2px; font-family: arial; font-size: 12px;"">" + (code_total * -1).ToString("F2", Form.Culture) + "</td>";
                                    invdet_html += "</tr>";
                                }
                                foreach (var code in Registrant.PromotionalCodes.Where(c => c.Code.Action == ShoppingCartAction.Multiply))
                                {
                                    var code_total = Math.Round((running_total * (1 - code.Code.Amount)), 2);
                                    code_runningTotal += Math.Round(code_total, 2);
                                    running_total -= Math.Round(code_total, 2);
                                    invdet_html += "<tr>";
                                    invdet_html += @"<td colspan=""2"" style=""padding: 2px; font-family: arial; font-size: 12px;"">" + code.Code.Description + "</td>";
                                    invdet_html += @"<td align=""right"" style=""padding: 2px; font-family: arial; font-size: 12px;"">" + code.Code.Code + "</td>";
                                    invdet_html += @"<td align=""right"" style=""padding: 2px; font-family: arial; font-size: 12px;"">" + (code_total * -1).ToString("F2", Form.Culture) + "</td>";
                                    invdet_html += "</tr>";
                                }
                                invdet_html += "<tr>";
                                invdet_html += @"<td colspan=""2"" align=""right"" style=""padding: 2px 2px 10px 2px; font-family: arial; font-size: 12px;""><b>TOTAL DISCOUNTS:</b></td>";
                                invdet_html += @"<td colspan=""2"" align=""right"" style=""padding: 2px 2px 10px 2px; font-family: arial; font-size: 12px;""><b>" + (code_runningTotal * -1).ToString("F2", Form.Culture) + "</b></td>";
                                invdet_html += "</tr>";
                            }
                            if (Registrant.PaysTaxes)
                            {
                                // There is tax, so we must apply the tax.
                                invdet_html += "<tr>";
                                invdet_html += @"<td colspan=""4"" style=""padding: 10px 2px 2px 2px; border-top: 1px solid #dddddd; font-family: arial; font-size: 12px;""><b>TAX</b></td>";
                                invdet_html += "</tr>";
                                var inv_tax = Math.Round(running_total * Form.Tax.Value, 2);
                                running_total += Math.Round(inv_tax, 2);
                                invdet_html += "<tr>";
                                invdet_html += @"<td colspan=""2"" align=""right"" style=""padding: 2px 2px 10px 2px; font-family: arial; font-size: 12px;""><b>" + Form.TaxDescription + "</b></td>";
                                invdet_html += @"<td colspan=""2"" align=""right"" style=""padding: 2px 2px 10px 2px; font-family: arial; font-size: 12px;""><b>" + inv_tax.ToString("F2", Form.Culture) + "</b></td>";
                                invdet_html += "</tr>";
                            }
                            invdet_html += "<tr>";
                            invdet_html += @"<td colspan=""2"" align=""right"" style=""background: #eeeeee; padding: 10px 2px 10px 2px; border-top: 1px solid #dddddd; font-family: arial; font-size: 12px;""><b>REGISTRATION TOTAL:</b></td>";
                            invdet_html += @"<td colspan=""2"" align=""right"" style=""background: #eeeeee; padding: 10px 2px 10px 2px; border-top: 1px solid #dddddd; font-family: arial; font-size: 12px;""><b>" + Registrant.Fees.ToString("F2", Form.Culture) + "</b></td>";
                            invdet_html += "</tr>";
                            if (Registrant.Adjustments.Count > 0)
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
                                foreach (var adjustment in Registrant.Adjustments.Where(a => a.AdjustmentType == "Adjustment").OrderBy(a => a.Date).ToList())
                                {
                                    var t_adjAmount = Math.Round(adjustment.Amount, 2);
                                    adjustments_total += t_adjAmount;
                                    invdet_html += "<tr>";
                                    invdet_html += @"<td colspan=""2"" style=""padding: 2px; font-family: arial; font-size: 12px;"">" + adjustment.Name + "</td>";
                                    invdet_html += @"<td align=""right"" style=""padding: 2px; font-family: arial; font-size: 12px;"">" + adjustment.Date.LocalDateTime.ToString(Form.Culture) + "</td>";
                                    invdet_html += @"<td align=""right"" style=""padding: 2px; font-family: arial; font-size: 12px;"">" + t_adjAmount.ToString("F2", Form.Culture) + "</td>";
                                    invdet_html += "</tr>";
                                }
                                invdet_html += "<tr>";
                                invdet_html += @"<td colspan=""2"" align=""right"" style=""padding: 2px 2px 10px 2px; font-family: arial; font-size: 12px;""><b>TOTAL ADJUSTMENTS:</b></td>";
                                invdet_html += @"<td colspan=""2"" align=""right"" style=""padding: 2px 2px 10px 2px; font-family: arial; font-size: 12px;""><b>" + adjustments_total.ToString("F2", Form.Culture) + "</b></td>";
                                invdet_html += "</tr>";
                            }
                            var inv_trans = new List<IFinanceAmmount>();
                            inv_trans.AddRange(Registrant.TransactionRequests.ToList());
                            inv_trans.AddRange(Registrant.Adjustments.Where(a => a.AdjustmentType != "Adjustment").ToList());
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
                                    if (trans is Adjustment)
                                        t_name = (trans as Adjustment).AdjustmentType + " Recorded";
                                    var t_total_n = trans.TotalAmount();
                                    var t_total = Math.Round((t_total_n.HasValue ? t_total_n.Value : 0.00m), 2);
                                    payment_total += t_total;
                                    invdet_html += "<tr>";
                                    invdet_html += @"<td colspan=""2"" style=""padding: 2px; font-family: arial; font-size: 12px;"">" + t_name + "</td>";
                                    invdet_html += @"<td align=""right"" style=""padding: 2px; font-family: arial; font-size: 12px;"">" + trans.DateCreated.LocalDateTime.ToString(Form.Culture) + "</td>";
                                    invdet_html += @"<td align=""right"" style=""padding: 2px; font-family: arial; font-size: 12px;"">" + (t_total * -1).ToString("F2", Form.Culture) + "</td>";
                                    invdet_html += "</tr>";
                                }
                                invdet_html += "<tr>";
                                invdet_html += @"<td colspan=""2"" align=""right"" style=""padding: 2px 2px 10px 2px; font-family: arial; font-size: 12px;""><b>TOTAL PAYMENTS:</b></td>";
                                invdet_html += @"<td colspan=""2"" align=""right"" style=""padding: 2px 2px 10px 2px; font-family: arial; font-size: 12px;""><b>" + (payment_total * -1).ToString("F2", Form.Culture) + "</b></td>";
                                invdet_html += "</tr>";
                            }
                            invdet_html += "<tr>";
                            invdet_html += @"<td colspan=""2"" align=""right"" style=""background: #eeeeee; padding: 10px 2px 10px 2px; border-top: 1px solid #dddddd; font-family: arial; font-size: 12px;""><b>BALANCE:</b></td>";
                            invdet_html += @"<td colspan=""2"" align=""right"" style=""background: #eeeeee; padding: 10px 2px 10px 2px; border-top: 1px solid #dddddd; font-family: arial; font-size: 12px;""><b>" + Registrant.TotalOwed.ToString("F2", Form.Culture) + "</b></td>";
                            invdet_html += "</tr>";
                            invdet_html += "</tbody>";
                            invdet_html += "</table>";
                            html = invdet_html;
                        }
                        break;
                    #endregion
                    default:
                        break;
                }
            }
            else
            {
                PropertyInfo prop = null;
                try
                {
                    prop = obj.GetType().GetProperty(property);
                }
                catch (Exception) { }
                if (prop != null)
                {
                    var val = prop.GetValue(obj);
                    if (val == null)
                        html = "";
                    if (val is DateTimeOffset)
                        html = ((DateTimeOffset)val).LocalDateTime.ToString();
                    else if (val is Audience)
                        html = ((Audience)val).Name;
                    else if (val is decimal)
                        html = Math.Round((decimal)val, 2).ToString();
                    html = val != null ? val.ToString() : "";
                }
            }
            html = Regex.Replace(html, @"\$([0-9]*)", "$$$0", RegexOptions.Multiline);
            return html;
        }


        public class RSParserInfo
        {
            public Regex RGX { get; set; }
            public string Method { get; set; }
        }
    }
}