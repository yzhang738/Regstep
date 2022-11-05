using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RSToolKit.Domain.Entities;
using RSToolKit.Domain.Data;
using RSToolKit.Domain.JItems;

namespace RSToolKit.Domain.Engines
{
    public static class LogicEngine
    {
        /// <summary>
        /// This will run logic and return the commands it cannot complete itself.
        /// Ensure the items are attached to a repository supplied so data can be saved.
        /// </summary>
        /// <typeparam name="T">The type of logicHolder.</typeparam>
        /// <param name="logicHolder">The logic holder.</param>
        /// <param name="repository">The repository being used with the data.</param>
        /// <param name="registrant">The registrant. default: null</param>
        /// <param name="contact">The contact. default: null</param>
        /// <param name="onLoad">If we are running onload logic or onadvance. default: true</param>
        /// <returns>Returns the logic commands it cannot complete itself.</returns>
        public static IEnumerable<JLogicCommand> RunLogic<T>(T logicHolder, FormsRepository repository, Registrant registrant = null, Contact contact = null, bool onLoad = true, bool runCommands = true)
            where T : ILogicHolder
        {
            var commands = new List<JLogicCommand>();
            IPerson person = registrant;
            if (person == null)
                person = contact;
            if (person == null)
                return new List<JLogicCommand>();
            // Now we run the logics.
            if (logicHolder.Logics.Count > 0)
            {
                // This holder is still using the legacy system.  We will use that system instead of the new one.
                commands = Logic.RunLogics(logicHolder.Logics, registrant, onLoad, repository).Select(l => new JLogicCommand() { Command = Convert(l.Command), CommandType = Convert(l.CommandType), Order = l.Order, Parameters = l.Parameters }).ToList();
            }
            else if (logicHolder.JLogics != null && logicHolder.JLogics.Count > 0)
            {
                foreach (var logic in logicHolder.JLogics.Where(l => l.OnLoad == onLoad))
                    commands.AddRange(logic.RunLogics(person));
            }
            // Now we run through commands.
            if (logicHolder is IEmail && commands.Count == 0)
                commands.Add(new JLogicCommand() { Command = JLogicWork.SendEmail, Order = 1 });
            var returnCommands = new List<JLogicCommand>();
            foreach (var command in commands)
            {
                switch (command.Command)
                {
                    case JLogicWork.SetVar:
                        if (!runCommands)
                            continue;
                        if (!command.Parameters.Keys.Contains("Variable") || String.IsNullOrWhiteSpace(command.Parameters["Variable"]))
                            continue;
                        if (!command.Parameters.Keys.Contains("Value") || String.IsNullOrWhiteSpace(command.Parameters["Value"]))
                            continue;
                        person.SetData(command.Parameters["Variable"], command.Parameters["Value"], ignoreValidation: true);
                        repository.Commit();
                        break;
                    default:
                        returnCommands.Add(command);
                        break;

                }
            }
            return returnCommands;
        }

        /// <summary>
        /// This will run logic and return the commands it cannot complete itself.
        /// Ensure the items are attached to a repository supplied so data can be saved.
        /// </summary>
        /// <typeparam name="T">The type of logicHolder.</typeparam>
        /// <param name="logicHolder">The logic holder.</param>
        /// <param name="context">The context being used with the data.</param>
        /// <param name="registrant">The registrant. default: null</param>
        /// <param name="contact">The contact. default: null</param>
        /// <param name="onLoad">If we are running onload logic or onadvance. default: true</param>
        /// <returns>Returns the logic commands it cannot complete itself.</returns>
        public static IEnumerable<JLogicCommand> RunLogic<T>(T logicHolder, Registrant registrant = null, Contact contact = null, bool onLoad = true, bool runCommands = true, Dictionary<string, IEnumerable<JLogicCommand>> allCommands = null)
            where T : ILogicHolder
        {
            allCommands = allCommands ?? new Dictionary<string, IEnumerable<JLogicCommand>>();
            if (allCommands.ContainsKey(logicHolder.UId.ToString()))
                return allCommands[logicHolder.UId.ToString()];
            var commands = new List<JLogicCommand>();
            IPerson person = registrant;
            if (person == null)
                person = contact;
            if (person == null)
                return new List<JLogicCommand>();
            // Now we run the logics.
            if (logicHolder.Logics.Count > 0)
            {
                // This holder is still using the legacy system.  We will use that system instead of the new one.
                commands = Logic.RunLogics(logicHolder.Logics, registrant, onLoad).Select(l => new JLogicCommand() { Command = Convert(l.Command), CommandType = Convert(l.CommandType), Order = l.Order, Parameters = l.Parameters }).ToList();
            }
            else if (logicHolder.JLogics != null && logicHolder.JLogics.Count > 0)
            {
                foreach (var logic in logicHolder.JLogics.Where(l => l.OnLoad == onLoad))
                    commands.AddRange(logic.RunLogics(person));
            }
            // Now we run through commands.
            if (logicHolder is IEmail && commands.Count == 0)
                commands.Add(new JLogicCommand() { Command = JLogicWork.SendEmail, Order = 1 });
            var returnCommands = new List<JLogicCommand>();
            foreach (var command in commands)
            {
                switch (command.Command)
                {
                    case JLogicWork.SetVar:
                        if (!runCommands)
                        {
                            returnCommands.Add(command);
                            continue;
                        }
                        if (!command.Parameters.Keys.Contains("Variable") || String.IsNullOrWhiteSpace(command.Parameters["Variable"]))
                            continue;
                        if (!command.Parameters.Keys.Contains("Value") || String.IsNullOrWhiteSpace(command.Parameters["Value"]))
                            continue;
                        person.SetData(command.Parameters["Variable"], command.Parameters["Value"], ignoreValidation: true);
                        returnCommands.Add(command);
                        break;
                    default:
                        returnCommands.Add(command);
                        break;

                }
            }
            allCommands.Add(logicHolder.UId.ToString(), returnCommands);
            return returnCommands;
        }


        public static JLogicWork Convert(LogicWork command)
        {
            switch (command)
            {
                case LogicWork.DisplayText:
                    return JLogicWork.DisplayText;
                case LogicWork.DontSendEmail:
                    return JLogicWork.DontSendEmail;
                case LogicWork.FormHalt:
                    return JLogicWork.FormHalt;
                case LogicWork.Hide:
                    return JLogicWork.Hide;
                case LogicWork.PageHalt:
                    return JLogicWork.PageHalt;
                case LogicWork.PageSkip:
                    return JLogicWork.PageSkip;
                case LogicWork.SendEmail:
                    return JLogicWork.SendEmail;
                case LogicWork.SetVar:
                    return JLogicWork.SetVar;
                case LogicWork.Show:
                    return JLogicWork.Show;
                default:
                    return JLogicWork.DisplayText;
            }
        }

        public static JLogicCommandType Convert(LogicCommandType command)
        {
            switch (command)
            {
                case LogicCommandType.Else:
                    return JLogicCommandType.Else;
                default:
                    return JLogicCommandType.Then;
            }
        }
    }
}
