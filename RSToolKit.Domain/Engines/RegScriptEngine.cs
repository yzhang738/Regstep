using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using RSToolKit.Domain.Data;
using RSToolKit.Domain.Entities;

namespace RSToolKit.Domain.RegScript
{
    public class RegScriptEngine
    {
        // Here we define all the regular expressions used to parse the script.
        protected static Regex rgx_variable = new Regex(@"^var\s+(?<var>[a-zA-Z0-9]*)\s*(?<operand>[+=%/|&^\*!]*)\s*(?<value>.*)", RegexOptions.IgnoreCase);
        protected static Regex rgx_header = new Regex(@"header\s+""(?<header>.*?)""\s+is\s+(?<data>.*)", RegexOptions.IgnoreCase);
        protected static Regex rgx_operands = new Regex(@"(?<operand>[+=%/|&^\*!-]+)\s*(?<leftover>.*)", RegexOptions.IgnoreCase);
        protected static Regex rgx_math = new Regex(@"(?<value>[^-+=%/|&^\*!]+)\s*(?<operand>[-+=%/|&^\*!]*)\s*", RegexOptions.IgnoreCase);
        protected static Regex rgx_var = new Regex(@"^var=>(?<variable>.*)$", RegexOptions.IgnoreCase);
        protected static Regex rgx_paren = new Regex(@"\((?<group>[^\)]*?(?:(?:('|"")[^'""]*?\2)[^\)]*?)*)\)\s*(?<link>AND|OR|)?\s*", RegexOptions.IgnoreCase);
        protected static Regex rgx_where = new Regex(@"where\s+(?<group>.*?)(?=\s*select|$)", RegexOptions.IgnoreCase);
        protected static Regex rgx_select = new Regex(@"select\s+(?<group>.*?)(?=\s*where|$)", RegexOptions.IgnoreCase);
        protected static Regex rgx_clause = new Regex(@"\s*(?<link>AND|OR|)?\s*(?<var>.*?(?=\s*(?:=|==|!=|<>|\^=|!\^=|\$=|!\$=|>|<|>=|<=|=rgx=|!=rgx=|\*=|!\*=)))\s*(?<test>=|==|!=|<>|\^=|!\^=|\$=|!\$=|>|<|>=|<=|=rgx=|!=rgx=|\*=|!\*=)\s*(?<quote>""|')(?<val>.*?)\k<quote>", RegexOptions.IgnoreCase);
        protected static Regex rgx_dobject = new Regex(@"\[(?<object>[^\]]*)\]", RegexOptions.IgnoreCase);
        protected static Regex rgx_count = new Regex(@"COUNT\([^\)]*\)", RegexOptions.IgnoreCase);
        protected static Regex rgx_useOn = new Regex(@"using\s+(?<value>.*)", RegexOptions.IgnoreCase);
        protected static Regex rgx_useOff = new Regex(@"using off\s+(?<value>.*)", RegexOptions.IgnoreCase);

        protected string Script;
        protected Form Form;
        protected FormsRepository Repository;
        protected Dictionary<string, object> Variables;
        protected Dictionary<string, object> Headers;
        protected int LineNumber;
        protected int CharNumber;
        protected bool live;
        protected bool test;
        protected bool submitted;
        protected bool incomplete;
        protected bool cancelled;
        protected bool deleted;
        protected bool? rsvp;
        protected Form CurrentForm;

        public RegScriptEngine(Form form, string script, FormsRepository repository)
            : this()
        {
            Script = script;
            Form = form;
            CurrentForm = form;
            Repository = repository;
        }

        public RegScriptEngine()
        {
            Script = "";
            Variables = new Dictionary<string, object>();
            Headers = new Dictionary<string, object>();
            LineNumber = 0;
            CharNumber = -1;
            live = submitted = true;
            cancelled = deleted = incomplete = test = false;
            rsvp = null;
        }

        public Dictionary<string, object> Run()
        {
            foreach (Match match in Regex.Matches(Script, @"(?<script>[^;]*?);"))
            {
                LineNumber++;
                var script = match.Groups["script"].Value.Trim();
                if (script.StartsWith(Environment.NewLine))
                    script = script.Substring(Environment.NewLine.Length).Trim();
                script = script.Replace(Environment.NewLine, " ").Trim();
                if (rgx_useOff.IsMatch(script))
                {
                    var t_val = rgx_useOff.Match(script).Groups["value"].Value.Trim().ToLower();
                    switch (t_val)
                    {
                        case "live":
                            live = false;
                            break;
                        case "submitted":
                            submitted = false;
                            break;
                        case "incomplete":
                            incomplete = false;
                            break;
                        case "cancelled":
                            cancelled = false;
                            break;
                        case "deleted":
                            deleted = false;
                            break;
                        case "test":
                            test = false;
                            break;
                        default:
                            if (t_val.StartsWith("rsvp"))
                                rsvp = null;
                            else if (t_val.StartsWith("form"))
                                CurrentForm = Form;
                            break;
                    }
                }
                else if (rgx_useOn.IsMatch(script))
                {
                    var t_val = rgx_useOn.Match(script).Groups["value"].Value.Trim().ToLower();
                    switch (t_val)
                    {
                        case "live":
                            live = true;
                            break;
                        case "submitted":
                            submitted = true;
                            break;
                        case "incomplete":
                            incomplete = true;
                            break;
                        case "cancelled":
                            cancelled = true;
                            break;
                        case "deleted":
                            deleted = true;
                            break;
                        case "test":
                            test = true;
                            break;
                        default:
                            if (t_val.StartsWith("rsvp"))
                            {
                                var t_match = Regex.Match(t_val, @"rsvp\s*=(?:>|(?:&gt;))\s*(?<rsvp>.*)");
                                bool t_rsvp;
                                if (bool.TryParse(t_match.Groups["rsvp"].Value ?? "false", out t_rsvp))
                                    rsvp = t_rsvp;
                                else
                                    rsvp = null;
                            }
                            else if (t_val.StartsWith("form"))
                            {
                                var t_match = Regex.Match(t_val, @"form\s*=(?:>|(?:&gt;))\s*(?<form>.*)");
                                Guid t_id;
                                if (Guid.TryParse(t_match.Groups["form"].Value, out t_id))
                                {
                                    CurrentForm = Repository.Search<Form>(f => f.UId == t_id, checkAccess: false).FirstOrDefault();
                                    if (CurrentForm == null)
                                        CurrentForm = Form;
                                    else if (CurrentForm.CompanyKey != Form.CompanyKey)
                                        CurrentForm = Form;
                                }
                                else
                                {
                                    CurrentForm = Form;
                                }
                            }
                            break;
                    }
                }
                else if (script.ToLower().StartsWith("var"))
                    runVariable(script);
                else if (script.ToLower().StartsWith("header"))
                    RunHeader(script);
                else
                    RunOperation(script);
            }

            return Headers;
        }

        public void runVariable(string script)
        {
            var match = rgx_variable.Match(script);
            var vari = match.Groups["var"].Value.Trim();
            var operand = match.Groups["operand"].Value.Trim();
            var value = match.Groups["value"].Value.Trim();
            object result = null;
            if (value.ToLower().StartsWith("select") || value.ToLower().StartsWith("where"))
                result = RunClause(value);
            else
                result = RunOperation(value);
            Variables[vari] = VariableMath("var=>" + vari, operand, result);
        }

        public void RunHeader(string script)
        {
            var m_header = rgx_header.Match(script);
            var header = m_header.Groups["header"].Value;
            var data = m_header.Groups["data"].Value;
            if (data.ToLower().StartsWith("select") || data.ToLower().StartsWith("where"))
                Headers[header] = RunClause(data);
            else
            {
                object result = RunOperation(data);
                Headers[header] = result;
            }
        }

        public object RunClause(string script)
        {
            List<Guid> matchedContacts = null;
            string where = null;
            string select = null;
            if (rgx_where.IsMatch(script))
                where = rgx_where.Match(script).Groups["group"].Value;
            if (rgx_select.IsMatch(script))
                select = rgx_select.Match(script).Groups["group"].Value;
            var filters = GetFilters(where);
            var selects = RunOperation(select);

            // Now we grab what we from the form

            var list = new List<Registrant>();

            // Now we must filter out the contacts
            IEnumerable<Registrant> f_registrants = CurrentForm.Registrants;
            if (live)
                f_registrants = f_registrants.Where(r => r.Type == RegistrationType.Live);
            if (submitted)
                f_registrants = f_registrants.Where(r => r.Status == RegistrationStatus.Submitted);
            if (cancelled)
                f_registrants = f_registrants.Where(r => r.Status == RegistrationStatus.Canceled || r.Status == RegistrationStatus.CanceledByAdministrator || r.Status == RegistrationStatus.CanceledByCompany);
            if (test)
                f_registrants = f_registrants.Where(r => r.Type == RegistrationType.Test);
            if (deleted)
                f_registrants = f_registrants.Where(r => r.Status == RegistrationStatus.Deleted);
            if (incomplete)
                f_registrants = f_registrants.Where(r => r.Status == RegistrationStatus.Incomplete);
            if (rsvp.HasValue)
                f_registrants = f_registrants.Where(r => r.RSVP == rsvp.Value);
            foreach (var registrant in f_registrants.OrderBy(r => r.DateCreated).ToList())
            {
                var take = true;
                var tests = new List<Tuple<bool, FilterLink>>();
                for (var i = 0; i < filters.Count; i++)
                {
                    var filter = filters[i];
                    var groupTest = true;
                    var first = true;
                    var grouping = filter.GroupNext;
                    var t_test = false;
                    do
                    {
                        if (!filter.GroupNext)
                        {
                            grouping = false;
                        }
                        t_test = registrant.CompareData(filter.ActingOn, filter.Value, filter.Test.GetTestValue(), ref matchedContacts);
                        switch (filter.Link)
                        {
                            case FilterLink.And:
                                groupTest = groupTest && t_test;
                                break;
                            case FilterLink.Or:
                                if (first)
                                    groupTest = t_test;
                                else
                                    groupTest = groupTest || t_test;
                                break;
                            case FilterLink.None:
                            default:
                                groupTest = t_test;
                                break;
                        }
                        first = false;
                        if (!grouping)
                            break;
                        i++;
                        if (i < filters.Count)
                            filter = filters[i];
                        else
                            break;
                    } while (grouping);
                    tests.Add(new Tuple<bool, FilterLink>(groupTest, (i + 1) < filters.Count ? filters[(i + 1)].Link : FilterLink.None));
                }
                take = tests.Count > 0 ? tests[0].Item1 : true;
                for (int i = 1; i < tests.Count; i++)
                {
                    switch (tests[i - 1].Item2)
                    {
                        case FilterLink.And:
                            take = take && tests[i].Item1;
                            break;
                        case FilterLink.Or:
                            take = take || tests[i].Item1;
                            break;
                        case FilterLink.None:
                        default:
                            take = tests[i].Item1;
                            break;
                    }
                }
                if (take)
                {
                    list.Add(registrant);
                }
            }

            // Now we compile the selects.
            if (rgx_count.IsMatch(select))
            {
                return (float)list.Count;
            }
            else
            {
                var stuff = new List<string>();
                foreach (var registrant in list)
                {
                    if (selects is List<String>)
                    { }
                    else if (selects is string)
                    {
                        var info = selects as string;
                    foreach (Match obj in rgx_dobject.Matches(select))
                    {
                        Guid t_id;
                        var o_val = obj.Groups["object"].Value;
                            string dp;
                            dp = registrant.SearchPrettyValue(o_val);
                            if (dp != null)
                                info = info.Replace("[" + o_val + "]", dp);
                    }
                    stuff.Add(info);
                }
                }
                return stuff;
            }
        }

        public object GetSelects(string select)
        {
            if (String.IsNullOrWhiteSpace(select))
                throw new RegScriptException("select clause is empty.", LineNumber);
            return RunOperation(select);
        }

        public List<QueryFilter> GetFilters(string where)
        {
            if (where == null)
                return new List<QueryFilter>();
            var filters = new List<QueryFilter>();
            var groupLink = FilterLink.None;
            if (rgx_paren.IsMatch(where))
            {
                foreach (Match match in rgx_paren.Matches(where))
                {
                    var group = match.Groups["group"].Value;
                    foreach (Match clause in rgx_clause.Matches(group))
                    {
                        var filter = new QueryFilter()
                        {
                            GroupNext = true,
                        };
                        filters.Add(filter);
                        filter.ActingOn = clause.Groups["var"].Value.Replace("[", "").Replace("]", "");
                        #region filter test
                        switch (clause.Groups["test"].Value.ToLower())
                        {
                            case "=":
                            case "==":
                            case "is":
                            case "equals":
                            case "equal":
                                filter.Test = LogicTest.Equal;
                                break;
                            case "!=":
                            case "<>":
                            case "not equal":
                            case "not equals":
                            case "is not":
                                filter.Test = LogicTest.NotEqual;
                                break;
                            case ">":
                            case "greater than":
                                filter.Test = LogicTest.GreaterThan;
                                break;
                            case ">=":
                            case "greater than or equal":
                            case "greater than or equals":
                                filter.Test = LogicTest.GreaterThanOrEqual;
                                break;
                            case "<":
                            case "less than":
                                filter.Test = LogicTest.LessThan;
                                break;
                            case "<=":
                            case "less than or equal":
                            case "less than or equals":
                                filter.Test = LogicTest.LessThanOrEqual;
                                break;
                            case "starts with":
                            case "^=":
                                filter.Test = LogicTest.StartsWith;
                                break;
                            case "!^=":
                                filter.Test = LogicTest.NotStartsWith;
                                break;
                            case "ends with":
                            case "$=":
                                filter.Test = LogicTest.EndsWith;
                                break;
                            case "!$=":
                                filter.Test = LogicTest.NotEndsWith;
                                break;
                            case "=rgx=":
                            case "is match":
                                filter.Test = LogicTest.RegexMatch;
                                break;
                            case "!=rgx=":
                            case "not match":
                            case "no match":
                                filter.Test = LogicTest.RegexNotMatch;
                                break;
                            case "*=":
                                filter.Test = LogicTest.Contains;
                                break;
                            case "!*=":
                                filter.Test = LogicTest.DoesNotContain;
                                break;
                            default:
                                filter.Test = LogicTest.Equal;
                                break;
                        }
                        #endregion
                        filter.Value = clause.Groups["val"].Value;
                        switch (clause.Groups["link"].Value.ToLower())
                        {
                            case "and":
                                filter.Link = FilterLink.And;
                                break;
                            case "or":
                                filter.Link = FilterLink.Or;
                                break;
                            default:
                                filter.Link = FilterLink.None;
                                break;
                        }
                        if (groupLink != FilterLink.None)
                        {
                            filters.Last().Link = groupLink;
                            groupLink = FilterLink.None;
                        }
                    }
                    switch (match.Groups["link"].Value.ToLower())
                    {
                        case "and":
                            groupLink = FilterLink.And;
                            break;
                        case "or":
                            groupLink = FilterLink.Or;
                            break;
                        default:
                            groupLink = FilterLink.None;
                            break;
                    }
                    filters.Last().GroupNext = false;
                }
            }
            else
            {
                foreach (Match clause in rgx_clause.Matches(where))
                {
                    var filter = new QueryFilter()
                    {
                        GroupNext = true,
                    };
                    filters.Add(filter);
                    filter.ActingOn = clause.Groups["var"].Value.Replace("[", "").Replace("]", "");
                    #region filter test
                    switch (clause.Groups["test"].Value.ToLower())
                    {
                        case "=":
                        case "==":
                        case "is":
                        case "equals":
                        case "equal":
                            filter.Test = LogicTest.Equal;
                            break;
                        case "!=":
                        case "<>":
                        case "not equal":
                        case "not equals":
                        case "is not":
                            filter.Test = LogicTest.NotEqual;
                            break;
                        case ">":
                        case "greater than":
                            filter.Test = LogicTest.GreaterThan;
                            break;
                        case ">=":
                        case "greater than or equal":
                        case "greater than or equals":
                            filter.Test = LogicTest.GreaterThanOrEqual;
                            break;
                        case "<":
                        case "less than":
                            filter.Test = LogicTest.LessThan;
                            break;
                        case "<=":
                        case "less than or equal":
                        case "less than or equals":
                            filter.Test = LogicTest.LessThanOrEqual;
                            break;
                        case "starts with":
                        case "^=":
                            filter.Test = LogicTest.StartsWith;
                            break;
                        case "ends with":
                        case "$=":
                            filter.Test = LogicTest.EndsWith;
                            break;
                        case "=rgx=":
                        case "is match":
                            filter.Test = LogicTest.RegexMatch;
                            break;
                        case "!=rgx=":
                        case "not match":
                        case "no match":
                            filter.Test = LogicTest.RegexNotMatch;
                            break;
                        case "*=":
                            filter.Test = LogicTest.Contains;
                            break;
                        case "!*=":
                            filter.Test = LogicTest.DoesNotContain;
                            break;
                        default:
                            filter.Test = LogicTest.Equal;
                            break;
                    }
                    #endregion
                    filter.Value = clause.Groups["val"].Value;
                    switch (clause.Groups["link"].Value.ToLower())
                    {
                        case "and":
                            filter.Link = FilterLink.And;
                            break;
                        case "or":
                            filter.Link = FilterLink.Or;
                            break;
                        default:
                            filter.Link = FilterLink.None;
                            break;
                    }
                }
                filters.Last().GroupNext = false;
            }

            return filters;
        }

        public object RunOperation(string value)
        {
            value = value.Trim();
            var values = new List<object>();
            var operands = new List<string>();
            while (value.Length > 0)
            {
                if (value.StartsWith("\"") || value.StartsWith("'"))
                {
                    var t_index = 0;
                    values.Add(GetString(value, out t_index));
                    value = value.Substring(t_index + 1).Trim();
                }
                else
                {
                    var t_index = value.IndexOf(' ');
                    string t_value = null;
                    if (t_index != -1)
                    {
                        t_value = value.Substring(0, t_index).Trim();
                    }
                    else
                    {
                        t_value = value;
                        value = "";
                    }
                    var fl_value = 0f;
                    if (float.TryParse(t_value, out fl_value))
                        values.Add(fl_value);
                    else if (rgx_dobject.IsMatch(t_value))
                        values.Add(t_value);
                    else if (rgx_count.IsMatch(t_value))
                    {
                        values = new List<object>() { t_value };
                        break;
                    }
                    else if (Variables.Keys.Contains(t_value))
                        values.Add("var=>" + t_value);
                    else
                        throw new RegScriptException("Syntax Error: Invalid object.", LineNumber);
                    value = value.Substring(t_index + 1).Trim();
                }
                if (rgx_operands.IsMatch(value))
                {
                    var o_match = rgx_operands.Match(value);
                    operands.Add(o_match.Groups["operand"].Value.Trim());
                    value = o_match.Groups["leftover"].Value.Trim();
                }
            }

            return RunMath(values, operands);
        }

        public object RunMath(List<object> values, List<string> operands)
        {
            object result = null;

            if (values.Count < 1)
                return result;

            result = VariableMath(values[0], null, null);

            for (var i = 0; i < operands.Count; i++)
            {
                if (i + 1 >= values.Count)
                    return VariableMath(result, null, null);
                result = VariableMath(result, operands[i], values[i + 1]);
            }

            return result;
        }

        public string GetString(string value, out int index)
        {
            var open = '"';
            if (value.StartsWith("'"))
                open = '\'';
            // We have a string literal and need to find out where it ends.
            char peek = '\\';
            var t_index = 0;
            do
            {
                t_index++;
                t_index = value.IndexOf(open, t_index);
                if (t_index < 1)
                    break;
                peek = value[t_index - 1];
            } while (peek == '\\');
            index = t_index;
            return value.Substring(1, t_index - 1).Replace("\\", "");
        }

        public object VariableMath(object val1, string operand, object val2)
        {
            if (val1 != null && rgx_var.IsMatch(val1.ToString()))
            {
                var match = rgx_var.Match(val1.ToString());
                var key = match.Groups["variable"].Value;
                if (Variables.ContainsKey(key))
                    val1 = Variables[key];
                else
                    val1 = "";
            }
            if (val2 != null && rgx_var.IsMatch(val2.ToString()))
            {
                var match = rgx_var.Match(val2.ToString());
                var key = match.Groups["variable"].Value;
                if (Variables.ContainsKey(key))
                    val2 = Variables[key];
                else
                    val2 = "";
            }
            if (operand == null || val2 == null)
                return val1;
            switch (operand)
            {
                case "+":
                case "+=":
                    if (val1 is float && val2 is float)
                        return (float)val1 + (float)val2;
                    else if (val1 is List<string> && val2 is List<String>)
                    {
                        ((List<string>)val1).AddRange((List<string>)val2);
                        return val1;
                    }
                    else
                        return val1.ToString() + val2.ToString();
                case "-=":
                case "-":
                    if (val1 is float && val2 is float)
                        return (float)val1 - (float)val2;
                    else
                        return null;
                case "*=":
                case "*":
                    if (val1 is float && val2 is float)
                        return (float)val1 * (float)val2;
                    else
                        return null;
                case "/=":
                case "/":
                    if (val1 is float && val2 is float)
                        return (float)val1 / (float)val2;
                    else
                        return null;
                case "%=":
                case "%":
                    if (val1 is float && val2 is float)
                        return (float)val1 % (float)val2;
                    else
                        return null;
                case "=":
                    return val2;
            }
            return null;
        }
    }

    public class RegScriptException : Exception
    {
        protected int linenumber;
        public int LineNumber { get { return linenumber; } }

        public RegScriptException(string message, int line)
            : base(message)
        {
            linenumber = line;
        }
    }
}
