using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RSToolKit.Domain.Data;
using RSToolKit.Domain.Entities;
using RSToolKit.Domain.Entities;

namespace RSToolKit.Domain.JItems
{
    public class JLogic
    {
        public List<JTableFilter> Filters { get; set; }
        public List<JLogicCommand> Commands { get; set; }
        public List<JTableHeader> Headers { get; set; }
        public string Name { get; set; }
        public int Order { get; set; }
        public bool OnLoad { get; set; }
        public string Id { get; set; }

        public JLogic()
        {
            Filters = new List<JTableFilter>();
            Commands = new List<JLogicCommand>();
            Order = 1;
            OnLoad = true;
            Id = Guid.NewGuid().ToString();
            Name = "New Logic";
        }

        public void AddCommand(JLogicCommand command)
        {
            command.Order = Commands.Count + 1;
            Commands.Add(command);
        }

        public void AddFilter(JTableFilter filter)
        {
            filter.Order = Filters.Count + 1;
            Filters.Add(filter);
        }

        public IEnumerable<JLogicCommand> RunLogics(IPerson person)
        {
            var registrant = person as Registrant;

            // Now we need to run through the logic filters.
            var filters = Filters.OrderBy(f => f.Order).ToList();
            var tests = new List<Tuple<bool, string>>();
            var grouping = true;
            List<Guid> matchedContacts = null;
            using (var context = new EFDbContext())
            {
                for (var i = 0; i < filters.Count; i++)
                {
                    var filter = filters[i];
                    var groupTest = true;
                    var first = true;
                    var test = false;
                    grouping = true;
                    do
                    {
                        var filterValue = filter.Value;
                        if (!filter.GroupNext)
                            grouping = false;
                        test = person.CompareData(filter.ActingOn, filterValue, filter.Test, ref matchedContacts, filter.CaseSensitive);
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
                        i++;
                        if (i < Filters.Count)
                            filter = Filters[i];
                        else
                            break;
                    } while (grouping);
                    tests.Add(new Tuple<bool, string>(groupTest, i < (Filters.Count - 1) ? Filters[i + 1].Link : "none"));
                }
            }
            var take = tests.Count > 0 ? tests[0].Item1 : true;
            for (var i = 1; i < tests.Count; i++)
            {
                switch (tests[i - 1].Item2)
                {
                    case "and":
                        take = take && tests[i].Item1;
                        break;
                    case "or":
                        take = take || tests[i].Item1;
                        break;
                    case "none":
                    default:
                        take = tests[i].Item1;
                        break;
                }
            }
            if (take)
                return Commands.Where(c => c.CommandType == JLogicCommandType.Then).OrderBy(l => l.Order).ToList();
            else
                return Commands.Where(c => c.CommandType == JLogicCommandType.Else).OrderBy(l => l.Order).ToList();
        }
    }

    public class JLogicCommand
    {
        public JLogicWork Command { get; set; }
        public JLogicCommandType CommandType { get; set; }
        public int Order { get; set; }
        public Dictionary<string, string> Parameters { get; set; }

        public JLogicCommand()
        {
            Command = JLogicWork.Hide;
            Order = -1;
            CommandType = JLogicCommandType.Then;
            Parameters = new Dictionary<string, string>();
        }
    }

    public enum JLogicWork
    {
        [StringValue("Page Skip")]
        PageSkip = 0,
        [StringValue("Set Var")]
        SetVar = 1,
        [StringValue("Display Text")]
        DisplayText = 2,
        [StringValue("Hide")]
        Hide = 3,
        [StringValue("Show")]
        Show = 4,
        [StringValue("Form Halt")]
        FormHalt = 5,
        [StringValue("Send Email")]
        SendEmail = 6,
        [StringValue("Page Halt")]
        PageHalt = 7,
        [StringValue("Don't Send Email")]
        DontSendEmail = 8,
    }

    public enum JLogicCommandType
    {
        Then = 0,
        Else = 1
    }
}