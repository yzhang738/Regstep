using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using RSToolKit.Domain.Data;
using System.Text.RegularExpressions;
using RSToolKit.Domain.Errors;
using RSToolKit.Domain.Entities.Components;
using Newtonsoft.Json;
using RSToolKit.Domain.Entities.Email;
using RSToolKit.Domain.Entities.Clients;
using RSToolKit.Domain.Entities;

namespace RSToolKit.Domain.Entities
{
    public class Logic : IRSData, IComparable<Logic>, INodeItem
    {

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long SortingId { get; set; }

        [Key]
        public Guid UId { get; set; }

        [MaxLength(250)]
        public string Name { get; set; }

        public DateTimeOffset DateCreated { get; set; }
        public DateTimeOffset DateModified { get; set; }

        public Guid ModificationToken { get; set; }
        public Guid ModifiedBy { get; set; }

        [CascadeDelete]
        public virtual List<LogicGroup> LogicGroups { get; set; }
        [CascadeDelete]
        public virtual List<LogicCommand> Commands { get; set; }

        [DisplayFormat(ConvertEmptyStringToNull = false)]
        [MaxLength(1000)]
        public string Description { get; set; }

        public bool Incoming { get; set; }

        public int Order { get; set; }

        [ForeignKey("PageKey")]
        public virtual Page Page { get; set; }
        public Guid? PageKey { get; set; }

        [ForeignKey("PanelKey")]
        public virtual Panel Panel { get; set; }
        public Guid? PanelKey { get; set; }

        [ForeignKey("ComponentKey")]
        public virtual Component Component { get; set; }
        public Guid? ComponentKey { get; set; }

        [ForeignKey("LogicBlockKey")]
        public virtual LogicBlock LogicBlock { get; set; }
        public Guid? LogicBlockKey { get; set; }

        [NotMapped]
        [JsonIgnore]
        public IEmail TheEmail
        {
            get
            {
                if (Email != null)
                    return Email;
                else
                    return HtmlEmail;
            }
        }

        [ForeignKey("HtmlEmailKey")]
        public virtual RSHtmlEmail HtmlEmail { get; set; }
        public Guid? HtmlEmailKey { get; set; }

        [ForeignKey("EmailKey")]
        public virtual RSEmail Email { get; set; }
        public Guid? EmailKey { get; set; }

        public Logic()
            : base()
        {
            Name = "New Logic";
            DateCreated = DateModified = DateTimeOffset.Now;
            LogicGroups = new List<LogicGroup>();
            Commands = new List<LogicCommand>();
            Incoming = false;
            Description = "";
            Order = 1;
        }

        public static Logic New(FormsRepository repository, ILogicHolder holder, Company company, User user, Guid? owner = null, Guid? group = null, string description = null, bool incoming = true)
        {
            var node = new Logic()
            {
                UId = Guid.NewGuid(),
                Incoming = incoming,
                Name = "New Logic - " + DateTimeOffset.UtcNow.ToString("MM/dd/yyyy h:mm tt"),
            };
            holder.Logics.Sort((a, b) => a.Order.CompareTo(b.Order));
            if (holder.Logics.Count > 0)
                node.Order = holder.Logics.Last().Order + 1;
            holder.Logics.Add(node);
            repository.Commit();
            return node;
        }

        #region IComparable, Equals, ToString

        public int CompareTo(Logic other)
        {
            return Order - other.Order;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Logic)) return false;
            return ((Logic)obj).UId == UId;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        #endregion

        public INode GetNode()
        {
            INode node = null;
            if (Component != null)
            {
                if (Component is IComponentItem)
                {
                    node = ((IComponentItem)Component).Parent.Panel.Page.Form;
                }
                else
                {
                    node = Component.Panel.Page.Form;
                }
            }
            else if (Panel != null)
            {
                node = Panel.Page.Form;
            }
            else if (Page != null)
            {
                node = Page.Form;
            }
            else if (Email != null)
            {
                node = Email.Form as INode ?? Email.EmailCampaign as INode;
            }
            else if (HtmlEmail != null)
            {
                node = HtmlEmail.Form as INode ?? HtmlEmail.EmailCampaign as INode;
            }
            else if (LogicBlock != null)
            {
                node = LogicBlock.Form as INode ?? LogicBlock.EmailCampaign as INode;
            }
            return node;
        }

        #region Methods

        public Dictionary<LogicWork, Dictionary<string, string>> GetWork(Registrant registrant, FormsRepository repository)
        {
            var list = new Dictionary<LogicWork, Dictionary<string, string>>();
            bool eval = RunLogic(registrant);
            var commands = eval ? Commands.Where(c => c.CommandType == LogicCommandType.Then).ToList() : Commands.Where(c => c.CommandType == LogicCommandType.Else).ToList();
            foreach (var command in commands)
            {
                list.Add(command.Command, command.Parameters);
            }
            return list;
        }

        [Obsolete]
        public bool RunLogic(IPerson registrant, FormsRepository repository)
        {
            if (LogicGroups.Count == 0)
                return false;
            LinkedList<LogicEvaluation> evaluations = new LinkedList<LogicEvaluation>();
            foreach (LogicGroup group in LogicGroups)
            {
                evaluations.AddLast(group.Evaluate(registrant));
            }
            bool eval = true;
            LinkedListNode<LogicEvaluation> position = evaluations.First;
            LinkedListNode<LogicEvaluation> last = evaluations.Last;
            //CHECK THE AND OPERANDS FIRST
            while (position != last)
            {
                if (position.Value.Link == LogicLink.And)
                {
                    eval = position.Value.Eval & position.Next.Value.Eval;
                    LogicEvaluation replace = new LogicEvaluation(position.Next.Value.Link, eval);
                    evaluations.Remove(position.Next);
                    evaluations.AddAfter(position, replace);
                    evaluations.Remove(position);
                    position = evaluations.Find(replace);
                    last = evaluations.Last;
                }
                else
                {
                    position = position.Next;
                }
            }
            //CHECK THE Xor (^) OPERANDS SECOND
            position = evaluations.First;
            last = evaluations.Last;
            while (position != last)
            {
                if (position.Value.Link == LogicLink.Xor)
                {
                    eval = position.Value.Eval ^ position.Next.Value.Eval;
                    LogicEvaluation replace = new LogicEvaluation(position.Next.Value.Link, eval);
                    evaluations.Remove(position.Next);
                    evaluations.AddAfter(position, replace);
                    evaluations.Remove(position);
                    position = evaluations.Find(replace);
                    last = evaluations.Last;
                }
                else
                {
                    position = position.Next;
                }
            }
            //CHECK OR OPERANDS THIRD
            position = evaluations.First;
            last = evaluations.Last;
            while (position != last)
            {
                if (position.Value.Link == LogicLink.Or)
                {
                    eval = position.Value.Eval | position.Next.Value.Eval;
                    LogicEvaluation replace = new LogicEvaluation(position.Next.Value.Link, eval);
                    evaluations.Remove(position.Next);
                    evaluations.AddAfter(position, replace);
                    evaluations.Remove(position);
                    position = evaluations.Find(replace);
                    last = evaluations.Last;
                }
                else
                {
                    position = position.Next;
                }
            }
            return evaluations.First.Value.Eval;
        }

        public bool RunLogic(IPerson registrant)
        {
            if (LogicGroups.Count == 0)
                return false;
            LinkedList<LogicEvaluation> evaluations = new LinkedList<LogicEvaluation>();
            foreach (LogicGroup group in LogicGroups)
            {
                evaluations.AddLast(group.Evaluate(registrant));
            }
            bool eval = true;
            LinkedListNode<LogicEvaluation> position = evaluations.First;
            LinkedListNode<LogicEvaluation> last = evaluations.Last;
            //CHECK THE AND OPERANDS FIRST
            while (position != last)
            {
                if (position.Value.Link == LogicLink.And)
                {
                    eval = position.Value.Eval & position.Next.Value.Eval;
                    LogicEvaluation replace = new LogicEvaluation(position.Next.Value.Link, eval);
                    evaluations.Remove(position.Next);
                    evaluations.AddAfter(position, replace);
                    evaluations.Remove(position);
                    position = evaluations.Find(replace);
                    last = evaluations.Last;
                }
                else
                {
                    position = position.Next;
                }
            }
            //CHECK THE Xor (^) OPERANDS SECOND
            position = evaluations.First;
            last = evaluations.Last;
            while (position != last)
            {
                if (position.Value.Link == LogicLink.Xor)
                {
                    eval = position.Value.Eval ^ position.Next.Value.Eval;
                    LogicEvaluation replace = new LogicEvaluation(position.Next.Value.Link, eval);
                    evaluations.Remove(position.Next);
                    evaluations.AddAfter(position, replace);
                    evaluations.Remove(position);
                    position = evaluations.Find(replace);
                    last = evaluations.Last;
                }
                else
                {
                    position = position.Next;
                }
            }
            //CHECK OR OPERANDS THIRD
            position = evaluations.First;
            last = evaluations.Last;
            while (position != last)
            {
                if (position.Value.Link == LogicLink.Or)
                {
                    eval = position.Value.Eval | position.Next.Value.Eval;
                    LogicEvaluation replace = new LogicEvaluation(position.Next.Value.Link, eval);
                    evaluations.Remove(position.Next);
                    evaluations.AddAfter(position, replace);
                    evaluations.Remove(position);
                    position = evaluations.Find(replace);
                    last = evaluations.Last;
                }
                else
                {
                    position = position.Next;
                }
            }
            return evaluations.First.Value.Eval;
        }


        #endregion

        [Obsolete]
        public static List<LogicCommand> RunLogics(List<Logic> logics, IPerson registrant, bool onLoad, FormsRepository repository)
        {
            var list = new List<LogicCommand>();
            foreach (var logic in logics.Where(l => l.Incoming == onLoad).OrderBy(l => l.Order).ToList())
            {
                var result = logic.RunLogic(registrant, repository);
                List<LogicCommand> commands;
                List<LogicCommand> otherCommands;
                var thenCommands = logic.Commands.Where(c => c.CommandType == LogicCommandType.Then).ToList();
                var elseCommands = logic.Commands.Where(c => c.CommandType == LogicCommandType.Else).ToList();
                if (result)
                {
                    commands = thenCommands;
                    otherCommands = elseCommands;
                }
                else
                {
                    commands = elseCommands;
                    otherCommands = thenCommands;
                }
                if (commands.Count == 0 && otherCommands.Count > 0 && otherCommands.Count(c => c.Command == LogicWork.SendEmail) > 0)
                    commands.Add(new LogicCommand() { Command = LogicWork.DontSendEmail });
                if (commands.Where(c => c.Command == LogicWork.PageSkip).Count() > 0)
                    return new List<LogicCommand>() { commands.Where(c => c.Command == LogicWork.PageSkip).FirstOrDefault() };
                list.AddRange(commands);
            }
            return list;
        }

        public static List<LogicCommand> RunLogics(List<Logic> logics, IPerson registrant, bool onLoad)
        {
            var list = new List<LogicCommand>();
            foreach (var logic in logics.Where(l => l.Incoming == onLoad).OrderBy(l => l.Order).ToList())
            {
                var result = logic.RunLogic(registrant);
                List<LogicCommand> commands;
                List<LogicCommand> otherCommands;
                var thenCommands = logic.Commands.Where(c => c.CommandType == LogicCommandType.Then).ToList();
                var elseCommands = logic.Commands.Where(c => c.CommandType == LogicCommandType.Else).ToList();
                if (result)
                {
                    commands = thenCommands;
                    otherCommands = elseCommands;
                }
                else
                {
                    commands = elseCommands;
                    otherCommands = thenCommands;
                }
                if (commands.Count == 0 && otherCommands.Count > 0 && otherCommands.Count(c => c.Command == LogicWork.SendEmail) > 0)
                    commands.Add(new LogicCommand() { Command = LogicWork.DontSendEmail });
                if (commands.Where(c => c.Command == LogicWork.PageSkip).Count() > 0)
                    return new List<LogicCommand>() { commands.Where(c => c.Command == LogicWork.PageSkip).FirstOrDefault() };
                list.AddRange(commands);
            }
            return list;
        }


        public Logic DeepCopy(ILogicHolder logicHold, Form form, Form oldForm)
        {
            var logic = new Logic();
            logic.UId = Guid.NewGuid();
            logicHold.Logics.Add(logic);
            if (logicHold is IComponent)
            {
                logic.Component = logicHold as Component;
                logic.ComponentKey = logicHold.UId;
            }
            else if (logicHold is Page)
            {
                logic.Page = logicHold as Page;
                logic.PageKey = logicHold.UId;
            }
            else if (logicHold is Panel)
            {
                logic.Panel = logicHold as Panel;
                logic.PanelKey = logicHold.UId;
            }
            else if (logicHold is RSEmail)
            {
                logic.Email = logicHold as RSEmail;
                logic.EmailKey = logicHold.UId;
            }
            else if (logicHold is RSHtmlEmail)
            {
                logic.HtmlEmail = logicHold as RSHtmlEmail;
                logic.HtmlEmailKey = logicHold.UId;
            }
            else if (logicHold is LogicBlock)
            {
                logic.LogicBlock = logicHold as LogicBlock;
                logic.LogicBlockKey = logicHold.UId;
            }
            logic.UId = Guid.NewGuid();
            logic.DateCreated = DateTimeOffset.UtcNow;
            logic.DateModified = logic.DateCreated;
            logic.Description = Description;
            logic.Incoming = Incoming;
            logic.Order = Order;
            logic.Name = Name;
            foreach (var command in Commands)
            {
                command.DeepCopy(logic, form, oldForm);
            }
            foreach (var group in LogicGroups)
            {
                group.DeepCopy(logic, form, oldForm);
            }
            return logic;
        }

    }

    public class LogicCommand
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [JsonIgnore]
        public long SortingId { get; set; }

        [Key]
        [JsonIgnore]
        public Guid UId { get; set; }

        [ForeignKey("LogicKey")]
        [JsonIgnore]
        public virtual Logic Logic { get; set; }
        [JsonIgnore]
        public Guid LogicKey { get; set; }

        [JsonIgnore]
        [ForeignKey("FormKey")]
        public Form Form { get; set; }
        [JsonIgnore]
        public Guid? FormKey { get; set; }

        public LogicWork Command { get; set; }
        public LogicCommandType CommandType { get; set; }

        [JsonIgnore]
        public string Params
        {
            get
            {
                return JsonConvert.SerializeObject(Parameters);
            }
            set
            {
                Parameters = JsonConvert.DeserializeObject<Dictionary<string, string>>(value);
                if (Parameters == null)
                    Parameters = new Dictionary<string, string>();
            }
        }

        public int Order { get; set; }

        //Not Mapped but affected by mapped data
        [NotMapped]
        public Dictionary<string, string> Parameters { get; set; }

        public LogicCommand()
        {
            UId = Guid.NewGuid();
            Command = LogicWork.Hide;
            Order = -1;
            CommandType = LogicCommandType.Then;
            Parameters = new Dictionary<string, string>();
        }

        public LogicCommand DeepCopy(Logic logic, Form form, Form oldForm)
        {
            var oldFormComponents = oldForm.GetComponents();
            var newFormComponents = form.GetComponents();
            var command = new LogicCommand();
            logic.Commands.Add(command);
            command.UId = Guid.NewGuid();
            command.CommandType = CommandType;
            command.Command = Command;
            command.Parameters = Parameters;
            command.Logic = logic;
            command.LogicKey = logic.UId;
            if (command.Parameters.Keys.Contains("Form"))
            {
                if (command.Parameters["Form"] == oldForm.UId.ToString())
                {
                    command.Parameters["Form"] = form.UId.ToString();
                    Guid c_id;
                    if (command.Parameters.Keys.Contains("Variable") && Guid.TryParse(command.Parameters["Variable"], out c_id))
                    {
                        var o_comp = oldFormComponents.FirstOrDefault(c => c.UId == c_id);
                        if (o_comp != null && o_comp.Variable != null)
                        {
                            var n_comp = newFormComponents.FirstOrDefault(c => c.Variable != null && c.Variable.Value == o_comp.Variable.Value);
                            if (n_comp != null)
                            {
                                command.Parameters["Variable"] = n_comp.UId.ToString();
                                Guid i_id;
                                if (o_comp is IComponentItemParent && n_comp is IComponentItemParent && Guid.TryParse(command.Parameters["Value"], out i_id))
                                {
                                    var o_item = (o_comp as IComponentItemParent).Children.FirstOrDefault(i => i.UId == i_id);
                                    if (o_item != null)
                                    {
                                        var n_item = (n_comp as IComponentItemParent).Children.FirstOrDefault(i => i.LabelText == o_item.LabelText);
                                        command.Parameters["Value"] = n_item.UId.ToString();
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return command;
        }
    }

    public class LogicGroup
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long SortingId { get; set; }

        [Key]
        public Guid UId { get; set; }

        [ForeignKey("LogicKey")]
        public virtual Logic Logic { get; set; }
        public Guid LogicKey { get; set; }

        [CascadeDelete]
        public virtual List<LogicStatement> LogicStatements { get; set; }

        public LogicLink Link { get; set; }

        public int Order { get; set; }

        public LogicGroup()
        {
            Link = LogicLink.None;
            LogicStatements = new List<LogicStatement>();
            UId = Guid.NewGuid();
            Order = -1;
        }

        public LogicGroup DeepCopy(Logic logic, Form form, Form oldForm)
        {
            var logicGroup = new LogicGroup();
            logic.LogicGroups.Add(logicGroup);
            logicGroup.UId = Guid.NewGuid();
            logicGroup.Logic = logic;
            logicGroup.LogicKey = logic.UId;
            logicGroup.Order = Order;
            logicGroup.Link = Link;
            foreach (var statement in LogicStatements)
            {
                statement.DeepCopy(logicGroup, form, oldForm);
            }
            return logicGroup;
        }

        public LogicEvaluation Evaluate(IPerson registrant)
        {
            LinkedList<LogicEvaluation> evaluations = new LinkedList<LogicEvaluation>();
            foreach (LogicStatement statement in LogicStatements.OrderBy(a => a.Order).ToList())
            {
                evaluations.AddLast(statement.Evaluate(registrant));
            }
            bool eval = true;
            LinkedListNode<LogicEvaluation> position = evaluations.First;
            LinkedListNode<LogicEvaluation> last = evaluations.Last;
            //CHECK THE AND OPERANDS FIRST
            while (position != last)
            {
                if (position.Value.Link == LogicLink.And)
                {
                    eval = position.Value.Eval & position.Next.Value.Eval;
                    LogicEvaluation replace = new LogicEvaluation(position.Next.Value.Link, eval);
                    evaluations.Remove(position.Next);
                    evaluations.AddAfter(position, replace);
                    evaluations.Remove(position);
                    position = evaluations.Find(replace);
                    last = evaluations.Last;
                }
                else
                {
                    position = position.Next;
                }
            }
            //CHECK THE Xor (^) OPERANDS SECOND
            position = evaluations.First;
            last = evaluations.Last;
            while (position != last)
            {
                if (position.Value.Link == LogicLink.Xor)
                {
                    eval = position.Value.Eval ^ position.Next.Value.Eval;
                    LogicEvaluation replace = new LogicEvaluation(position.Next.Value.Link, eval);
                    evaluations.Remove(position.Next);
                    evaluations.AddAfter(position, replace);
                    evaluations.Remove(position);
                    position = evaluations.Find(replace);
                    last = evaluations.Last;
                }
                else
                {
                    position = position.Next;
                }
            }
            //CHECK OR OPERANDS THIRD
            position = evaluations.First;
            last = evaluations.Last;
            while (position != last)
            {
                if (position.Value.Link == LogicLink.Or)
                {
                    eval = position.Value.Eval | position.Next.Value.Eval;
                    LogicEvaluation replace = new LogicEvaluation(position.Next.Value.Link, eval);
                    evaluations.Remove(position.Next);
                    evaluations.AddAfter(position, replace);
                    evaluations.Remove(position);
                    position = evaluations.Find(replace);
                    last = evaluations.Last;
                }
                else
                {
                    position = position.Next;
                }
            }
            return new LogicEvaluation(Link, evaluations.First.Value.Eval);
        }

    }

    public class LogicStatement
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long SortingId { get; set; }

        [Key]
        public Guid UId { get; set; }

        [ForeignKey("LogicGroupKey")]
        public virtual LogicGroup LogicGroup { get; set; }
        public Guid LogicGroupKey { get; set; }

        [ForeignKey("FormKey")]
        public virtual Form Form { get; set; }
        public Guid FormKey { get; set; }

        public string Variable { get; set; }
        public string Value { get; set; }

        public LogicLink Link { get; set; }
        public LogicTest Test { get; set; }

        public int Order { get; set; }

        [NotMapped]
        public string RegistrantConfirmation { get; set; }
        [NotMapped]
        public string ValueName { get; set; }
        [NotMapped]
        public string VariableName { get; set; }

        public LogicStatement()
        {
            Variable = "";
            Value = "true";
            Test = LogicTest.Equal;
        }

        #region Methods

        public LogicStatement DeepCopy(LogicGroup group, Form form, Form oldForm)
        {
            var oldFormComponents = oldForm.GetComponents();
            var newFormComponents = form.GetComponents();
            var statement = new LogicStatement();
            statement.LogicGroup = group;
            statement.LogicGroupKey = group.UId;
            group.LogicStatements.Add(statement);
            statement.Link = Link;
            statement.Order = Order;
            statement.Test = Test;
            statement.Value = Value;
            statement.Variable = Variable;
            statement.UId = Guid.NewGuid();
            if (Form.UId == oldForm.UId)
            {
                statement.Form = form;
                statement.FormKey = form.UId;
                Guid c_id;
                if (Guid.TryParse(statement.Variable, out c_id))
                {
                    var o_comp = oldFormComponents.FirstOrDefault(c => c.UId == c_id);
                    if (o_comp != null)
                    {
                        var n_comp = newFormComponents.FirstOrDefault(c => c.Variable != null && c.Variable.Value == o_comp.Variable.Value);
                        if (n_comp != null)
                        {
                            statement.Variable = n_comp.UId.ToString();
                            Guid i_id;
                            if (o_comp is IComponentItemParent && n_comp is IComponentItemParent && Guid.TryParse(statement.Value, out i_id))
                            {
                                var o_item = (o_comp as IComponentItemParent).Children.FirstOrDefault(i => i.UId == i_id);
                                if (o_item != null)
                                {
                                    var n_item = (n_comp as IComponentItemParent).Children.FirstOrDefault(i => i.LabelText == o_item.LabelText);
                                    statement.Value = n_item.UId.ToString();
                                }
                            }
                        }
                    }
                }
            }
            return statement;
        }

        /// <summary>
        /// Evaluates the logic statement and returns the result.
        /// </summary>
        /// <returns>The Logic Evaluation of the statement.</returns>
        public LogicEvaluation Evaluate(IPerson person)
        {
            List<Guid> matchedContacts = null;
            return new LogicEvaluation(Link, person.CompareData(Variable, Value, Test.GetTestValue(), ref matchedContacts));
        }

        #endregion
    }

    public class LogicEvaluation
    {
        /// <summary>
        /// Gets and sets the type of link for the list.
        /// </summary>
        public LogicLink Link { get; set; }
        /// <summary>
        /// Gets and sets the statements evaluation logic.
        /// </summary>
        public bool Eval { get; set; }

        /// <summary>
        /// Creates the logic evaluation class.
        /// </summary>
        /// <param name="link">The logic link for the statement resolved.</param>
        /// <param name="eval">The bool of the evaluated statement.</param>
        public LogicEvaluation(LogicLink link, bool eval)
        {
            Link = link;
            Eval = eval;
        }
    }

    #region Enumerations

    public enum LogicTest
    {
        [StringValue("equals")]
        [TestValue("==")]
        Equal = 0,
        [StringValue("greater than")]
        [TestValue(">")]
        GreaterThan = 1,
        [StringValue("greater than or equal to")]
        [TestValue(">=")]
        GreaterThanOrEqual = 2,
        [StringValue("less than")]
        [TestValue("<")]
        LessThan = 3,
        [StringValue("less than or equal to")]
        [TestValue("<=")]
        LessThanOrEqual = 4,
        [StringValue("not equal to")]
        [TestValue("!=")]
        NotEqual = 5,
        [StringValue("starts with")]
        [TestValue("^=")]
        StartsWith = 6,
        [StringValue("does not start with")]
        [TestValue("!^=")]
        NotStartsWith = 7,
        [StringValue("ends with")]
        [TestValue("$=")]
        EndsWith = 8,
        [TestValue("!$=")]
        [StringValue("does not end with")]
        NotEndsWith = 9,
        [StringValue("contains")]
        [TestValue("*=")]
        Contains = 10,
        [StringValue("does not contain")]
        [TestValue("!*=")]
        DoesNotContain = 11,
        [StringValue("=rgx=")]
        [TestValue("=rgx=")]
        RegexMatch = 12,
        [StringValue("!=rgx=")]
        [TestValue("!=rgx=")]
        RegexNotMatch = 13,
        [StringValue("in")]
        [TestValue("in")]
        In = 14,
        [StringValue("not in")]
        [TestValue("not in")]
        NotIn = 15
    }

    public enum LogicLink
    {
        [StringValue("None")]
        None = 0,
        [StringValue("&")]
        And = 1,
        [StringValue("|")]
        Or = 2,
        [StringValue("^")]
        Xor = 3
    }

    public enum LogicWork
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

    public enum LogicCommandType
    {
        Then = 0,
        Else = 1
    }

    #endregion
}
