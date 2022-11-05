using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using RSToolKit.Domain.Data;
using RSToolKit.Domain.Entities;
using RSToolKit.Domain.JItems;

namespace RSToolKit.Domain.Engines
{
    public class RegScriptTableEngine
    {
        // Here we define all the regular expressions used to parse the script.
        protected IEnumerable<string> _mustStartWith = new List<string>()
        {
            "var",
            "header"
        };

        protected char[] _wordDeliminations = new char[]
        {
            ' ',
            ';'
        };

        protected string[] _selectionDeliminations = new string[]
        {
            ";",
            "where"
        };

        protected string[] _whereDeliminations = new string[]
        {
            ";",
            "select"
        };

        protected string[] _filterLinks = new string[]
        {
            "and",
            "or"
        };

        protected string[] _filterTests = new string[]
        {
            "==",
            "=",
            "!=",
            "<>",
            "*=",
            "!*=",
            "^=",
            "!^=",
            "$=",
            "!$=",
            ">",
            "<",
            ">=",
            "<="
        };

        protected string[] _operands = new string[]
        {
            "+",
            "-",
            "*",
            "/",
            "%"
        };

        protected string _script;
        protected bool _live;
        protected bool _test;
        protected bool _runnable;

        protected Dictionary<string, object> _variables;
        protected Dictionary<string, object> _headers;
        protected int _cCharNumber;
        protected char? _cChar;
        protected string _cWord;
        protected int _rCharNumber;
        protected int _tCharNumber;
        protected char? _rChar;
        protected string _rWord;
        protected int _lineNumber;
        protected IEnumerable<string> _lines;
        protected int _phraseIndex = 0;

        protected List<Registrant> _registrants;

        protected long _active;
        protected long _submitted;
        protected long _incomplete;
        protected long _cancelled;
        protected long _deleted;
        protected long? _rsvpYes;
        protected long? _rsvpNo;
        protected Form _form;
        protected TableInformation _table;

        protected ProgressInfo _info;

        public RegScriptTableEngine(Form form, string script, TableInformation table)
            : this()
        {
            this._script = script;
            this._script = this._script.Replace("\r\n", " ");
            this._script = this._script.Replace('\r', ' ');
            this._script = this._script.Replace('\n', ' ');
            this._script = this._script.Replace('\t', ' ');
            this._script = this._script.Trim();
            this._form = form;
            if (this._form != null)
            {
                var regType = (form.Status == FormStatus.Developement) ? RegistrationType.Test : RegistrationType.Live;
                var countingRegistrants = form.Registrants.Where(r => r.Type == regType).ToList();
                this._registrants = countingRegistrants.Where(r => r.Status == RegistrationStatus.Submitted).ToList();
                this._active = this._registrants.Where(r => r.Status != RegistrationStatus.Deleted && r.Status != RegistrationStatus.CanceledByCompany && r.Status != RegistrationStatus.CanceledByAdministrator && r.Status != RegistrationStatus.Canceled).Count();
                this._submitted = this._registrants.Where(r => r.Status == RegistrationStatus.Submitted).Count();
                this._incomplete = this._registrants.Where(r => r.Status == RegistrationStatus.Incomplete).Count();
                this._cancelled = this._registrants.Where(r => r.Status == RegistrationStatus.Canceled || r.Status == RegistrationStatus.CanceledByAdministrator || r.Status == RegistrationStatus.CanceledByCompany).Count();
                this._deleted = this._registrants.Where(r => r.Status == RegistrationStatus.Deleted).Count();
                this._rsvpYes = null;
                this._rsvpNo = null;
                if (form.Pages.Where(p => p.Type == PageType.RSVP && p.Enabled).FirstOrDefault() != null)
                {
                    this._rsvpYes = this._registrants.Where(r => r.RSVP).Count();
                    this._rsvpNo = this._registrants.Where(r => !r.RSVP).Count();
                }
                this._runnable = true;
            }
            if (script.Length < 1)
                this._runnable = false;
            this._table = table;
        }

        public RegScriptTableEngine()
        {
            this._script = "";
            this._variables = new Dictionary<string, object>();
            this._headers = new Dictionary<string, object>();
            this._cCharNumber = 0;
            this._cChar = null;
            this._cWord = null;
            this._rCharNumber = 0;
            this._tCharNumber = 0;
            this._rChar = null;
            this._rWord = null;
            this._runnable = false;
        }

        public Dictionary<string, object> Run()
        {
            // No we need to grab each line as we run through the information.
            this._table.UpdateMessage("Parsing Script", "Formatting script to specification.");
            var inQuote = false;
            var lines = new List<String>();
            this._rChar = this._script[0];
            this._rCharNumber = 0;
            this._tCharNumber = 0;
            char quoteType = '"';
            char? prevChar = null;
            while (this._rChar != null)
            {
                if (!inQuote)
                {
                    switch (this._rChar)
                    {
                        case ';':
                            var t_line = this._script.Substring(this._tCharNumber, this._rCharNumber - this._tCharNumber + 1).Replace('\n', ' ').Trim();
                            lines.Add(t_line);
                            this._tCharNumber = this._rCharNumber + 1;
                            break;
                        case '"':
                            inQuote = true;
                            quoteType = '"';
                            break;
                        case '\'':
                            inQuote = true;
                            quoteType = '\'';
                            break;
                    }
                }
                else
                {
                    if (this._rChar == quoteType && prevChar != '\\')
                        inQuote = false;
                }
                if (this._rCharNumber < this._script.Length - 1)
                {
                    this._rCharNumber++;
                    prevChar = this._rChar;
                    this._rChar = this._script[this._rCharNumber];
                }
                else
                {
                    prevChar = this._rChar;
                    this._rChar = null;
                }
            }

            this._lines = lines;
            var lineCount = this._lines.Count();
            this._table.SetTickFraction(lineCount);
            if (prevChar != ';' || inQuote)
            {
                lines.Add(this._script.Substring(this._tCharNumber));
                this._lines = lines;
                string message = null;
                if (inQuote && prevChar == ';')
                    message = "Unexpected termination of line. The quote was not terminated.";
                else
                    message = "Unexpected termination of line. Expected ';' but found " + prevChar + " instead.";
                throw new RegScriptTableException(message, this._rCharNumber, this._lines.Count() - 1, this._lines);
            }

            // Now we have all the lines.
            for (this._lineNumber = 0; this._lineNumber < this._lines.Count(); this._lineNumber++)
            {
                this._table.UpdateMessage("Executing Script", "Executing script line " + this._lineNumber + 1 + ".");
                var line = lines[this._lineNumber];
                string t_phrase = null;
                var t_index = 0;
                t_index = line.IndexOf(' ');
                if (t_index == -1)
                    t_index = line.Length;
                var declaration = FindPhrase(line, 0, out t_index);
                this._rCharNumber = this._tCharNumber = t_index;
                var identifier = "";
                var operand = "";
                if (!this._mustStartWith.Contains(declaration.ToLower()))
                    throw new RegScriptTableException("Invalid starting token.", 0, this._lineNumber, lines);
                prevChar = null;
                this._rChar = null;
                identifier = FindPhrase(line, this._rCharNumber, out t_index);
                this._rCharNumber = this._tCharNumber = t_index;
                this._rChar = line[this._rCharNumber];
                operand = FindPhrase(line, this._rCharNumber, out t_index);
                this._rCharNumber = this._tCharNumber = t_index;
                var phrases = new List<string>();
                do
                {
                    t_phrase = FindPhrase(line, this._rCharNumber, out t_index);
                    this._rCharNumber = this._tCharNumber = t_index;
                    if (t_phrase != null)
                        phrases.Add(t_phrase);
                } while (t_phrase != null);

                IEnumerable<string> selections = new List<string>();
                IEnumerable<JTableFilter> filters = new List<JTableFilter>();
                IEnumerable<string> operations = new List<string>();

                for (this._phraseIndex = 0; this._phraseIndex < phrases.Count; this._phraseIndex++)
                {
                    var phrase = phrases[this._phraseIndex];
                    switch (phrase)
                    {
                        case "select":
                            selections = FindSelections(phrases, ++this._phraseIndex, out this._phraseIndex);
                            break;
                        case "where":
                            filters = FindFilters(phrases, ++this._phraseIndex, out this._phraseIndex);
                            break;
                        default:
                            operations = phrases.Skip(this._phraseIndex);
                            this._phraseIndex = phrases.Count;
                            break;
                    }
                }
                object obj = null;
                if (selections.Count() > 0)
                {
                    this._table.UpdateMessage("Executing Script", "Running select statement.");
                    var filteredRegistrants = new List<Registrant>();
                    if (filters.Count() > 0)
                    {
                        for (var i = 0; i < this._registrants.Count; i++)
                        {
                            var take = false;
                            var grouping = true;
                            var tests = new List<Tuple<bool, string>>();
                            var registrant = this._registrants[i];
                            for (var j = 0; j < filters.Count(); j++)
                            {
                                var filter = filters.ElementAt(j);
                                var groupTest = true;
                                var first = true;
                                grouping = filter.GroupNext;
                                var test = false;
                                do
                                {
                                    if (!filter.GroupNext)
                                        grouping = false;
                                    var field = filter.ActingOn;
                                    if (field.StartsWith("["))
                                        field = field.Substring(1);
                                    if (field.EndsWith("]"))
                                        field = field.Substring(0, field.Length - 1);
                                    test = registrant.TestValue(field, filter.Value, filter.Test);
                                    switch (filter.Link)
                                    {
                                        case "and":
                                            groupTest = groupTest && test;
                                            break;
                                        case "or":
                                            if (first)
                                                groupTest = test;
                                            else
                                                groupTest = groupTest || test;
                                            break;
                                        case "none":
                                        default:
                                            groupTest = test;
                                            break;
                                    }
                                    first = false;
                                    if (!grouping)
                                        break;
                                    j++;
                                    if (j < filters.Count())
                                        filter = filters.ElementAt(j);
                                    else
                                        break;
                                } while (grouping);
                                tests.Add(new Tuple<bool, string>(groupTest, j < (filters.Count() - 1) ? filters.ElementAt(j + 1).Link : "none"));
                            }
                            take = tests.Count > 0 ? tests[0].Item1 : true;
                            for (var j = 1; j < tests.Count; j++)
                            {
                                switch (tests[j - 1].Item2)
                                {
                                    case "and":
                                        take = take && tests[j].Item1;
                                        break;
                                    case "or":
                                        take = take || tests[j].Item1;
                                        break;
                                    case "none":
                                    default:
                                        take = tests[j].Item1;
                                        break;
                                }
                            }
                            if (take)
                            {
                                filteredRegistrants.Add(registrant);
                            }
                            this._table.UpdateMessage("Executing Script", "Filtering registrants by where clause.");
                        }
                    }
                    else
                    {
                        filteredRegistrants = this._registrants;
                    }
                    var select = selections.First();
                    this._table.UpdateMessage("Executing Script", "Processing select data.");
                    if (select.ToLower().StartsWith("count"))
                    {
                        this._table.UpdateMessage("Executing Script", "Counting select data.");
                        obj = (float)filteredRegistrants.Count;
                    }
                    else
                    {
                        this._table.UpdateMessage("Executing Script", "Parsing select data.");
                        var built = "";
                        var items = new List<string>();
                        var selects = new List<string>();
                        var operandNext = false;
                        foreach (var selectItem in selections)
                        {
                            if (operandNext && selectItem.ToLower() != "+")
                                throw new RegScriptTableException("Expected a '+' and got " + selectItem + " instead.", -1, this._lineNumber, this._lines);
                            built += selectItem;
                            if (selectItem.StartsWith("[") && selectItem.EndsWith("]"))
                                selects.Add(selectItem);
                        }
                        foreach (var registrant in filteredRegistrants)
                        {
                            var t_value = built;
                            foreach (var selected in selects)
                            {
                                var field = selected.Substring(1, selected.Length - 2);
                                var value = registrant.SearchPrettyValue(field) ?? "";
                                t_value = t_value.Replace(field, value);
                            }
                            items.Add(t_value);
                        }
                    }
                }
                else
                {
                    this._table.UpdateMessage("Executing Script", "Manipulating script variables.");
                    float finalValue = 0;
                    string t_operand = "+";
                    for (var t_i = 0; t_i < phrases.Count(); t_i++)
                    {
                        var phrase = phrases.ElementAt(t_i);
                        float t_numbValue = 0;
                        object t_variable = null;
                        if (this._variables.ContainsKey(phrase))
                            t_variable = this._variables[phrase];
                        if (t_variable == null)
                        {
                            // We need to check the phrase.
                            if ((phrase.StartsWith("\"") && phrase.EndsWith("\"")) || (phrase.StartsWith("'") && phrase.EndsWith("'")))
                                // We have a string.
                                t_variable = phrase.Substring(1, phrase.Length - 1);
                            else
                                t_variable = phrase;
                        }
                        if (t_variable is string)
                        {
                            if (!float.TryParse(t_variable as string, out t_numbValue))
                                throw new RegScriptTableException("Expected a number but found " + t_variable.ToString() + " instead.", -1, this._lineNumber, this._lines);
                        }
                        else if (t_variable is float)
                        {
                            t_numbValue = (float)t_variable;
                        }
                        else
                        {
                            throw new RegScriptTableException("Expected a number but found " + t_variable.ToString() + " instead.", -1, this._lineNumber, this._lines);
                        }
                        if (!this._operands.Contains(t_operand.ToLower()))
                            throw new RegScriptTableException("Expected an operand but found " + t_operand + " instead.", -1, this._lineNumber, this._lines);
                        switch (t_operand)
                        {
                            case "+":
                                finalValue += t_numbValue;
                                break;
                            case "-":
                                finalValue -= t_numbValue;
                                break;
                            case "*":
                                finalValue *= t_numbValue;
                                break;
                            case "/":
                                finalValue /= t_numbValue;
                                break;
                            case "%":
                                finalValue %= t_numbValue;
                                break;
                        }
                        t_operand = phrases.ElementAtOrDefault(++t_i);
                    }
                    obj = finalValue;
                }

                switch (declaration)
                {
                    case "var":
                        #region Variable
                        this._variables.Add(identifier, obj);
                        break;
                    #endregion
                    case "header":
                        #region Header
                        this._headers.Add(identifier, obj);
                        break;
                        #endregion
                }
                this._table.Tick("Executing Script", "Line completed.");
            }

            return this._headers;
        }

        protected IEnumerable<string> FindSelections(IEnumerable<string> phrases, int start, out int t_index)
        {
            var phrase = phrases.ElementAtOrDefault(start);
            var selections = new List<string>();
            if (phrase == null)
                phrase = ";";
            if (!this._selectionDeliminations.Contains(phrase.ToLower()))
            {
                selections.Add(phrase);
                phrase = phrases.ElementAtOrDefault(++start);
                if (phrase == null)
                    phrase = ";";
            }
            t_index = --start;
            return selections;
        }

        protected IEnumerable<JTableFilter> FindFilters(IEnumerable<string> phrases, int start, out int t_index)
        {
            var phrase = phrases.ElementAtOrDefault(start);
            var filters = new List<JTableFilter>();
            if (phrase == null)
                phrase = ";";
            var grouping = false;
            while (!this._whereDeliminations.Contains(phrase.ToLower()))
            {
                var t_filter = new JTableFilter();
                if (filters.Count != 0)
                {
                    if (!this._filterLinks.Contains(phrase.ToLower()))
                        throw new RegScriptTableException("Expected a link value, instead got " + phrase + ".", -1, this._lineNumber, this._lines);
                    t_filter.Link = phrase.ToLower();
                    phrase = phrases.ElementAtOrDefault(++start);
                    if (phrase == null)
                        throw new RegScriptTableException("Expected a value, instead got " + phrase + ".", -1, this._lineNumber, this._lines);
                }
                filters.Add(t_filter);
                if (phrase.StartsWith("("))
                    grouping = true;
                t_filter.ActingOn = phrase;
                phrase = phrases.ElementAtOrDefault(++start);
                if (phrase == null)
                {
                    t_filter.Test = "==";
                    t_filter.Value = "true";
                    break;
                }
                t_filter.Test = phrase.ToLower();
                t_filter.Test = t_filter.Test == "=" ? "==" : t_filter.Test;
                phrase = phrases.ElementAtOrDefault(++start);
                if (phrase == null)
                {
                    t_filter.Value = "true";
                    break;
                }
                t_filter.Value = phrase;
                phrase = phrases.ElementAtOrDefault(++start);
                if (phrase == null)
                    phrase = ";";
                if (phrase.EndsWith(")"))
                    grouping = false;
                t_filter.GroupNext = grouping;
            }
            t_index = --start;
            var t_order = 0;
            filters.ForEach(f => f.Order = ++t_order);
            return filters;
        }

        protected string FindPhrase(string subject, int start, out int t_index)
        {
            t_index = start + 1;
            if (start >= subject.Length)
                return null;
            string t_phrase = null;
            char? t_rChar = subject[start];
            char? t_prevChar = null;
            int t_position = start;
            if (t_rChar == '"' || t_rChar == '\'')
            {
                char quoteType = t_rChar.Value;
                do
                {
                    if (t_position + 1 >= subject.Length)
                        throw new RegScriptTableException("Invalid string token.  '\"' expected before end of line.", subject.Length, this._lineNumber, this._lines);
                    t_prevChar = t_rChar;
                    t_rChar = subject[++t_position];
                } while (t_rChar != quoteType && t_prevChar != '\\');
                t_phrase = subject.Substring(start + 1, t_position - start - 1);
            }
            else if (t_rChar == '[')
            {
                char quoteType = ']';
                do
                {
                    if (t_position + 1 >= subject.Length)
                        throw new RegScriptTableException("Invalid string token.  '\"' expected before end of line.", subject.Length, this._lineNumber, this._lines);
                    t_prevChar = t_rChar;
                    t_rChar = subject[++t_position];
                } while (t_rChar != quoteType && t_prevChar != '\\');
                t_phrase = subject.Substring(start, ++t_position - start);
            }
            else
            {
                t_position = subject.IndexOfAny(this._wordDeliminations, start);
                if (t_position == -1)
                    throw new RegScriptTableException("Invalid token. ' ' or ';' Expected before end of line.", subject.Length, this._lineNumber, this._lines);
                t_phrase = subject.Substring(start, t_position - start);
            }
            t_index = t_position + 1;
            while (t_index < subject.Length && subject[t_index] == ' ')
                t_index++;
            t_phrase = t_phrase.Trim();
            if (String.IsNullOrEmpty(t_phrase))
                return null;
            return t_phrase.Trim();
        }

    }

    public class RegScriptTableHeader
    {
        public IEnumerable<JTableFilter> Filters { get; set; }
        public IEnumerable<string> Selects { get; set; }
        public RegScriptTableHeader(IEnumerable<JTableFilter> filters, IEnumerable<string> selects)
        {
            Filters = filters;
            Selects = selects;
        }
    }

    public class RegScriptTableException
        : Exception
    {
        protected int p_lineNumber = 0;
        protected int p_charNumber = 0;
        protected IEnumerable<string> p_lines;
        public int LineNumber { get { return p_lineNumber; } }
        public int CharNumber { get { return p_charNumber; } }
        public IEnumerable<string> Lines { get { return p_lines; } }

        public RegScriptTableException(string message, int charNumber, int lineNumber, IEnumerable<string> lines)
            : base(message)
        {
            p_charNumber = charNumber;
            p_lineNumber = lineNumber;
            p_lines = lines;
        }
    }
}
