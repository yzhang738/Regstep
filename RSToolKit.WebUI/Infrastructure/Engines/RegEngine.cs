using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using RSToolKit.Domain.Data;
using RSToolKit.Domain.Entities;
using RSToolKit.Domain.JItems;
using RSToolKit.WebUI.Infrastructure.Json;
using RSToolKit.Domain.Entities.Reports;
using System.Collections.Concurrent;
using RSToolKit.WebUI.Infrastructure.Extensions;
using RSToolKit.Domain;

namespace RSToolKit.WebUI.Infrastructure.Engines
{
    public static class RegEngine
    {
        /// <summary>
        /// The allowable prefixes used for each line.
        /// </summary>
        static readonly string[] Prefixes;

        /// <summary>
        /// The characters that end a word
        /// </summary>
        static readonly char[] LineDeliminations;

        /// <summary>
        /// The strings that can end a selection list.
        /// </summary>
        static readonly string[] SelectionDeliminations;

        /// <summary>
        /// The strings that can end a where list;
        /// </summary>
        static readonly string[] WhereDeliminations;

        /// <summary>
        /// The characters that end a word.
        /// </summary>
        static readonly char[] WordDeliminations;

        /// <summary>
        /// The links to be used in filtering.
        /// </summary>
        static readonly string[] FilterLinks;

        /// <summary>
        /// The tests that can be used in a filter.
        /// </summary>
        static readonly string[] FilterTests;

        /// <summary>
        /// The operands allowed for variable manipulations.
        /// </summary>
        static readonly string[] Operands;

        /// <summary>
        /// Holds strings that preface a command action.
        /// </summary>
        static readonly string[] Commands;

        /// <summary>
        /// Holds caches of previously executed scripts.
        /// </summary>
        static ConcurrentDictionary<long, _RegScriptCache> Cache;

        static RegEngine()
        {
            Cache = new ConcurrentDictionary<long, _RegScriptCache>();

            Prefixes = new string[]
            {
                "var",
                "using",
                "//",
                "table",
                "join",
                "order"
            };

            SelectionDeliminations = new string[]
            {
                String.Empty,
                "where"
            };

            WhereDeliminations = new string[]
            {
                String.Empty,
                "select"
            };


            WordDeliminations = new char[]
            {
                ' ',
                ';'
            };

            LineDeliminations = new char[]
            {
                ';'
            };

            FilterLinks = new string[]
            {
                "and",
                "or"
            };

            FilterTests = new string[]
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

            Operands = new string[]
            {
                "+",
                "-",
                "*",
                "/",
                "%"
            };

            Commands = new string[]
            {
                "select",
                "where",
                "join",
                "as",
                "distinct",
                "order",
                "=",
                "*=",
                "+=",
                "/=",
                "-="
            };

        }

        /// <summary>
        /// Clears the cache for the RegScript.
        /// </summary>
        /// <param name="item">The reg script to remove.</param>
        public static void ClearCache(this IRegScript item)
        {
            _RegScriptCache cache;
            Cache.TryRemove(item.Id, out cache);
        }

        /// <summary>
        /// Executes the script and 
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public static IEnumerable<RegTable> Execute(this IRegScript item, ref Domain.Data.Info.Progress progress)
        {
            var formIDs = new List<long>();

            // First we need to check if the data has been run recently.
            var tables = new List<RegTable>();
            _RegScriptCache t_cache;
            if (WebConstants.ProgressStati.TryGetValue(item.Id + "_globalreport", out progress))
                if (!progress.Complete)
                    // It is currently under progress.
                    return null;

            if (Cache.TryGetValue(item.Id, out t_cache))
            {
                // We have the item already processed.
                if (t_cache.DateCreated.AddHours(3) > DateTimeOffset.Now)
                    // The cache is current. We use it.
                    return Cache[item.Id].Tables;
            }

            if (t_cache == null)
            {
                t_cache = new _RegScriptCache()
                {
                    Id = item.Id,
                    Tables = new List<RegTable>()
                };
            }
            t_cache.DateCreated = DateTimeOffset.Now;

            progress = new Domain.Data.Info.Progress(item.Id + "_globalreport");
            WebConstants.ProgressStati.TryAdd(item.Id + "_globalreport", progress);

            // Now we process lines.
            var script = new _RegScript()
            {
                RawScript = item.Script,
                Lines = new List<_RegScriptLine>()
            };
            var currentChar = script.RawScript[0];
            var currentPosition = 0;
            var searchPosition = 0;
            var searchCharacter = currentChar;
            var quoteType = char.MinValue;
            var quotePosition = -1;
            var previousCharacter = char.MinValue;

            progress.Update(1, "Parsing Script", "Seperating script into lines.");

            // We are going to grab all the lines.
            while (searchCharacter != char.MinValue)
            {
                if (quoteType == char.MinValue)
                {
                    switch (searchCharacter)
                    {
                        case ';':
                            script.Lines.Add(new _RegScriptLine(script.RawScript.Substring(currentPosition, searchPosition - currentPosition + 1).Replace(Environment.NewLine, "").Trim(), currentPosition, searchPosition));
                            currentPosition = searchPosition + 1;
                            progress.UpdateMessage(details: "Line " + script.Lines.Count + " found.");
                            break;
                        case '"':
                        case '\'':
                            quoteType = searchCharacter;
                            quotePosition = searchPosition;
                            break;
                    }
                }
                else
                {
                    if (searchCharacter == quoteType && previousCharacter != '\\')
                        quoteType = char.MinValue;
                }
                if (searchPosition < script.RawScript.Length - 1)
                {
                    previousCharacter = searchCharacter;
                    searchCharacter = script.RawScript[++searchPosition];
                }
                else
                {
                    previousCharacter = searchCharacter;
                    searchCharacter = char.MinValue;
                }
            }

            // Now we see if the script terminated correctly.
            if (previousCharacter != ';')
                throw new RegScriptException("The script must end in a ';'.", script.RawScript.Length - 1, script.RawScript.Length);

            if (quoteType != char.MinValue)
                throw new RegScriptException("Unexpected termination of quote.", quotePosition, script.RawScript.Length);

            // Now we have all the lines.
            progress.SetTickFraction(script.Lines.Count);

            var linePosition = -1;
            foreach (var line in script.Lines)
            {
                linePosition++;

                // First we determine if it is a valid start prefix.
                // We find the first space.
                searchPosition = line.Line.IndexOf(' ');
                var prefix = FindPhrase(line, 0);

                if (!Prefixes.Contains(prefix.Phrase))
                    throw new RegScriptException("'" + prefix.Phrase + "' is not a valid start declaration. You must use " + String.Join(", ", Prefixes) + ".", line.RawScriptLocationStart, line.RawScriptLocationStart + prefix.PhraseLineLocationEnd);
                searchPosition = prefix.PhraseLineLocationEnd;
                var phrases = new List<_RegScriptPhrase>();
                _RegScriptPhrase phrase;
                do
                {
                    phrase = FindPhrase(line, searchPosition);
                    if (phrase != null)
                    {
                        phrases.Add(phrase);
                        searchPosition = phrase.PhraseLineLocationEnd;
                    }
                } while (phrase != null);

                switch (prefix.Phrase)
                {
                    case "//":
                        continue;
                    case "using":
                        if (phrases.Count < 1)
                            throw new RegScriptException("You must supply an event id after a using.", line.RawScriptLocationStart, line.RawScriptLocationStart);
                        formIDs.Clear();
                        foreach (var u_phrase in phrases)
                        {
                            long s_formId;
                            if (!long.TryParse(u_phrase.Phrase, out s_formId))
                                throw new RegScriptException("You must supply an event id after a using that is an number.", line.RawScriptLocationStart, line.RawScriptLocationStart);
                            if (formIDs.Contains(s_formId))
                                throw new RegScriptException("You are already using that form.", line.RawScriptLocationStart, line.RawScriptLocationStart);
                            formIDs.Add(s_formId);
                        }
                        break;
                    case "table":
                        if (phrases.Count < 1)
                            throw new RegScriptException("You must supply a name after a table declaration.", line.RawScriptLocationStart, line.RawScriptLocationStart);
                        var type = phrases.ElementAtOrDefault(1);
                        var t_table = new RegTable() { Name = phrases[0].Phrase };
                        if (type != null && type.Phrase == "count")
                        {
                            t_table.Vertical = true;
                            t_table.Rows.Add(new RegTableRow() { Order = 1, Values = new List<RegTableRowValue>() });
                        }
                        tables.Add(t_table);
                        break;
                    case "join":
                        if (phrases.Count < 1)
                            throw new RegScriptException("You must supply a table after the join declaration.", line.RawScriptLocationStart, line.RawScriptLocationStart);
                        var table = tables.FirstOrDefault(t => t.Name == phrases[0].Phrase);
                        if (table == null)
                            throw new RegScriptException("You must supply a valid table after the join declaration. '" + phrases[0].Phrase + "' is not in " + string.Join(", ", tables.Select(t => t.Name)) + ".", line.RawScriptLocationStart, line.RawScriptLocationStart);
                        var j_phrase = phrases.ElementAtOrDefault(1);
                        if (j_phrase == null || j_phrase.Phrase != "with")
                            throw new RegScriptException("The phrase after the table declaration must be where.", line.RawScriptLocationStart, line.RawScriptLocationStart);
                        var selections = new List<_RegScriptSelection>();
                        var filters = new List<JTableFilter>();
                        var operations = new List<_RegScriptPhrase>();
                        for (var phraseIndex = 2; phraseIndex < phrases.Count; phraseIndex++)
                        {
                            j_phrase = phrases[phraseIndex];
                            switch (j_phrase.Phrase)
                            {
                                case "select":
                                    selections = FindSelections(phrases, ++phraseIndex, out phraseIndex);
                                    break;
                                case "where":
                                    filters = FindFilters(phrases, ++phraseIndex, out phraseIndex);
                                    break;
                                default:
                                    operations = phrases.Skip(++phraseIndex).ToList();
                                    phraseIndex = phrases.Count();
                                    break;
                            }
                        }
                        if (selections.Count == 0)
                            throw new RegScriptException("You must have selections for a join clause.", line.RawScriptLocationStart, line.RawScriptLocationEnd);
                        var filteredRegistrants = new List<Registrant>();
                        // First we must find out what forms can be used that are supplied in the using.
                        using (var context = new EFDbContext())
                        {
                            foreach (var formID in formIDs)
                            {
                                var form = context.Forms.Include("Registrants").Where(f => f.SortingId == formID && f.CompanyKey == item.CompanyKey).FirstOrDefault();
                                if (form == null)
                                    continue;
                                if (filters.Count == 0)
                                {
                                    filteredRegistrants.AddRange(form.Registrants.ToList());
                                }
                                else
                                {
                                    for (var i = 0; i < form.Registrants.Count; i++)
                                    {
                                        var take = false;
                                        var grouping = true;
                                        var tests = new List<Tuple<bool, string>>();
                                        var registrant = form.Registrants[i];
                                        for (var j = 0; j < filters.Count; j++)
                                        {
                                            var filter = filters[j];
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
                                                        groupTest = test;
                                                        break;
                                                }
                                                first = false;
                                                if (!grouping)
                                                    break;
                                                j++;
                                                if (j < filters.Count)
                                                    filter = filters[j];
                                                else
                                                    break;
                                            } while (grouping);
                                            tests.Add(new Tuple<bool, string>(groupTest, j < (filters.Count - 1) ? filters[j + 1].Link : "none"));
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
                                            filteredRegistrants.Add(registrant);
                                    }
                                }
                            }

                            // Now we need to start making our selections.
                            if (table.Vertical)
                            {
                                var row = table.Rows.First();
                                foreach (var select in selections)
                                {
                                    var value = row.Values.FirstOrDefault(r => r.HeaderValue == select.Label);
                                    if (value == null)
                                    {
                                        var header = new RegTableHeader() { Order = table.Headers.Count + 1, Value = select.Label };
                                        table.Headers.Add(header);
                                        value = new RegTableRowValue() { Value = "0", Header = header };
                                        row.Values.Add(value);
                                    }
                                    value.Value = (int.Parse(value.Value) + filteredRegistrants.Count).ToString();
                                }
                            }
                            else
                            {
                                foreach (var registrant in filteredRegistrants)
                                {
                                    var row = new RegTableRow()
                                    {
                                        Order = table.Rows.Count + 1,
                                    };
                                    table.Rows.Add(row);
                                    foreach (var select in selections)
                                    {
                                        if (select.Header == null)
                                            select.Header = table.Headers.FirstOrDefault(h => h.Value == select.Label);
                                        if (select.Header == null)
                                        {
                                            select.Header = new RegTableHeader() { Order = table.Headers.Count + 1, Value = select.Label };
                                            table.Headers.Add(select.Header);
                                        }
                                        var value = new RegTableRowValue() { Header = select.Header };
                                        row.Values.Add(value);
                                        if (select.DbIdentifier.First() == "count")
                                        {
                                            value.Value = filteredRegistrants.Count.ToString();
                                        }
                                        else
                                        {
                                            var val = "";
                                            foreach (var ident in select.DbIdentifier)
                                                val += registrant.GetScriptValue(ident);
                                            value.Value = val;
                                        }
                                    }
                                }
                            }
                        }
                        break;
                }
            }
            t_cache.Tables = tables;
            Cache.TryAdd(t_cache.Id, t_cache);
            progress.Complete = true;
            return tables;
        }

        static List<_RegScriptSelection> FindSelections(IEnumerable<_RegScriptPhrase> phrases, int start, out int t_index)
        {
            var phrase = phrases.ElementAtOrDefault(start);
            if (phrase == null)
                throw new RegScriptException("The selection must end in either ';' or 'where'.", phrases.First().Start, phrases.First().End);
            var selections = new List<_RegScriptSelection>();
            while (!SelectionDeliminations.Contains(phrase.Phrase.ToLower()))
            {
                var dbIdentifiers = new List<string>();
                dbIdentifiers.Add(phrase.Phrase);
                var label = phrase.Phrase;
                if (label.StartsWith("[") && label.EndsWith("]"))
                    label = label.Substring(1, label.Length - 2);

                var peek = phrases.ElementAtOrDefault(start + 1);
                while (peek != null && peek.Phrase == "+")
                {
                    start++;
                    peek = phrases.ElementAtOrDefault(++start);
                    if (peek == null)
                        throw new RegScriptException("You must supply another selection after as.", phrase.Line.RawScriptLocationStart, phrase.Line.RawScriptLocationEnd);
                    dbIdentifiers.Add(peek.Phrase);
                    peek = phrases.ElementAtOrDefault(start + 1);
                    if (peek == null)
                        break;
                }
                var peek1 = phrases.ElementAtOrDefault(start + 1);
                var peek2 = phrases.ElementAtOrDefault(start + 2);
                if (peek1 != null && peek1.Phrase == "as")
                {
                    if (peek2 == null)
                        throw new RegScriptException("You must supply a valid string after as statement.", phrase.Start, phrase.End);
                    label = peek2.Phrase;
                    start += 2;
                }
                selections.Add(new _RegScriptSelection(label, dbIdentifiers.ToArray()));
                phrase = phrases.ElementAtOrDefault(++start);
                if (phrase == null)
                    phrase = new _RegScriptPhrase(String.Empty, null, -1, -1);
                else if (phrase.Phrase == ",")
                    phrase = phrases.ElementAtOrDefault(++start) ?? new _RegScriptPhrase(String.Empty, null, -1, -1);
            }
            t_index = --start;
            return selections;
        }

        static List<JTableFilter> FindFilters(IEnumerable<_RegScriptPhrase> phrases, int start, out int t_index)
        {
            var phrase = phrases.ElementAtOrDefault(start);
            var filters = new List<JTableFilter>();
            if (phrase == null)
                throw new RegScriptException("The selection must end in either ';' or 'where'.", phrases.First().Start, phrases.First().End);
            var grouping = false;
            while (!WhereDeliminations.Contains(phrase.Phrase))
            {
                var t_filter = new JTableFilter();
                if (filters.Count != 0)
                {
                    if (!FilterLinks.Contains(phrase.Phrase))
                        throw new RegScriptException("Expected a link value, instead got " + phrase.Phrase + ".", phrase.Start, phrase.End);
                    t_filter.Link = phrase.Phrase;
                    phrase = phrases.ElementAtOrDefault(++start);
                    if (phrase == null)
                        throw new RegScriptException("Expected a value, instead got " + phrase.Phrase + ".", phrase.Start, phrase.End);
                }
                filters.Add(t_filter);
                if (phrase.Phrase.StartsWith("("))
                    grouping = true;
                t_filter.ActingOn = phrase.Phrase;
                phrase = phrases.ElementAtOrDefault(++start);
                if (phrase == null)
                    throw new RegScriptException("Unexpected termination of filter.", phrases.First().PhraseLineLocationStart, phrases.First().PhraseLineLocationEnd);
                t_filter.Test = phrase.Phrase;
                t_filter.Test = t_filter.Test == "=" ? "==" : t_filter.Test;
                phrase = phrases.ElementAtOrDefault(++start);
                if (phrase == null)
                    throw new RegScriptException("Unexpected termination of filter.", phrases.First().PhraseLineLocationStart, phrases.First().PhraseLineLocationEnd);
                t_filter.Value = phrase.Phrase;
                phrase = phrases.ElementAtOrDefault(++start);
                if (phrase == null)
                    phrase = new _RegScriptPhrase(String.Empty, null, -1, -1);
                if (phrase.Phrase.EndsWith(")"))
                    grouping = false;
                t_filter.GroupNext = grouping;
            }
            t_index = --start;
            var t_order = 0;
            filters.ForEach(f => f.Order = ++t_order);
            return filters;
        }

        static _RegScriptPhrase FindPhrase(_RegScriptLine line, int start)
        {

            var index = start + 1;
            if (start >= line.Line.Length)
                return null;
            string phrase = null;
            char currentCharacter = line.Line[start];
            char previousCharacter = char.MinValue;
            int currentPosition = start;
            bool quotedPhrase = false;
            if (currentCharacter == '"' || currentCharacter == '\'')
            {
                quotedPhrase = true;
                char quoteType = currentCharacter;
                do
                {
                    if (currentPosition + 1 >= line.Line.Length)
                        throw new RegScriptException("Invalid string token. '\"' expected before end of line.", line.RawScriptLocationStart, line.RawScriptLocationStart + index);
                    previousCharacter = currentCharacter;
                    currentCharacter = line.Line[++currentPosition];
                } while (currentCharacter != quoteType && previousCharacter != '\\');
                phrase = line.Line.Substring(start + 1, currentPosition - start - 1);
            }
            else if (currentCharacter == '[')
            {
                char quoteType = ']';
                do
                {
                    if (currentPosition + 1 >= line.Line.Length)
                        throw new RegScriptException("Invalid string token.  ']' expected before end of line.", line.RawScriptLocationStart, line.RawScriptLocationStart + index);
                    previousCharacter = currentCharacter;
                    currentCharacter = line.Line[++currentPosition];
                } while (currentCharacter != quoteType && previousCharacter != '\\');
                phrase = line.Line.Substring(start, ++currentPosition - start);
            }
            else
            {
                currentPosition = line.Line.IndexOfAny(WordDeliminations, start);
                if (currentPosition == -1)
                    throw new RegScriptException("Invalid token. ' ' or ';' Expected before end of line.", line.RawScriptLocationStart, line.RawScriptLocationStart + index);
                phrase = line.Line.Substring(start, currentPosition - start);
            }
            index = currentPosition + 1;
            while (index < line.Line.Length && line.Line[index] == ' ')
                index++;
            if (!quotedPhrase)
                phrase = phrase.Trim();
            if (String.IsNullOrEmpty(phrase))
                return null;
            return new _RegScriptPhrase(phrase, line, start, index);
        }

        public static string GetScriptValue(this Registrant registrant, string key)
        {
            RegistrantData data = null;

            if (key.StartsWith("[") && key.EndsWith("]"))
            {
                // We are asking for a variable.
                key = key.Substring(1, key.Length - 2);
                switch (key.ToLower())
                {
                    case "dateregistered":
                    case "date":
                    case "registrationdate":
                        return registrant.DateCreated.ToString("yyyy-MM-dd hh:mm tt");
                    case "lastedit":
                    case "modificationdate":
                    case "datemodified":
                        return registrant.DateModified.ToString("yyyy-MM-dd hh:mm tt");
                    case "form":
                        return registrant.Form.Name;
                    case "rsvp":
                        return registrant.RSVP ? registrant.Form.RSVPAccept : registrant.Form.RSVPDecline;
                    case "status":
                        return registrant.Status.GetStringValue();
                    case "confirmation":
                        return registrant.Confirmation;
                    case "email":
                        return registrant.Email.ToLower();
                    case "attended":
                        return registrant.Attended ? "Yes" : "No";
                    case "audience":
                        return registrant.Audience != null ? registrant.Audience.Label : "no audience";
                }
                data = registrant.Data.FirstOrDefault(d => d.Component.Variable.Value == key);
            }
            else
            {
                // It is supplied as either an long or uid.
                long lid;
                Guid uid;
                if (long.TryParse(key, out lid))
                    data = registrant.Data.FirstOrDefault(d => d.Component.SortingId == lid);
                else if (Guid.TryParse(key, out uid))
                    data = registrant.Data.FirstOrDefault(d => d.Component.UId == uid);
                else
                    return key;
            }
            if (data == null)
                return "No Data";

            return data.GetFormattedValue();
        }


        protected class _RegScriptSelection
        {
            public string Label { get; protected set; }
            public List<string> DbIdentifier { get; protected set; }
            public RegTableHeader Header { get; set; }

            public _RegScriptSelection(string label, params string[] dbIdentifiers)
            {
                Label = label;
                DbIdentifier = dbIdentifiers.ToList();
            }
        }

        protected class _RegScript
        {
            public string RawScript { get; set; }
            public List<_RegScriptLine> Lines { get; set; }
        }
        
        protected class _RegScriptLine
        {
            public string Line { get; protected set; }
            public int RawScriptLocationStart { get; protected set; }
            public int RawScriptLocationEnd { get; protected set; }

            public _RegScriptLine(string line, int start, int end)
            {
                Line = line;
                RawScriptLocationEnd = end;
                RawScriptLocationStart = start;
            }
        }

        protected class _RegScriptPhrase
        {
            public string Phrase { get; protected set; }
            public _RegScriptLine Line { get; protected set; }
            public int PhraseLineLocationStart { get; protected set; }
            public int PhraseLineLocationEnd { get; protected set; }

            public int Start
            {
                get
                {
                    return Line.RawScriptLocationStart + PhraseLineLocationStart;
                }
            }

            public int End
            {
                get
                {
                    return Line.RawScriptLocationEnd + PhraseLineLocationEnd;
                }
            }

            public _RegScriptPhrase(string phrase, _RegScriptLine line, int start, int end)
            {
                Phrase = phrase;
                PhraseLineLocationStart = start;
                PhraseLineLocationEnd = end;
                Line = line;
            }
        }

        protected class _RegScriptCache
        {
            public DateTimeOffset DateCreated { get; set; }
            public long Id { get; set; }
            public List<RegTable> Tables { get; set; }
        }
    }

    public class RegScriptException
        : Exception
    {
        public int Start { get; protected set; }
        public int End { get; protected set; }

        public RegScriptException()
            : base("Unknown RegScript exception.")
        {
            Start = End = -1;
        }

        public RegScriptException(string message, int start = -1, int end = -1)
            : base(message)
        {
            Start = start;
            End = end;
        }
    }
}
