using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RSToolKit.WebUI.Models
{
    public class LogicEditModel
    {
        public Guid UId { get; set; }
        public String Name { get; set; }
        public bool Incoming { get; set; }
        public List<LogicGroupModel> LogicGroups { get; set; }
        public List<LogicCommandModel> Thens { get; set; }
        public List<LogicCommandModel> Elses { get; set; }
    }

    public class LogicGroupModel
    {
        public List<LogicStatementModel> Statements { get; set; }
        public int Order { get; set; }
        public int Link { get; set; }
    }

    public class LogicStatementModel
    {
        public string CommonVariable { get; set; }
        public string Variable { get; set; }
        public Guid Form { get; set; }
        public string CommonValue { get; set; }
        public string Value { get; set; }
        public int Test { get; set; }
        public int Link { get; set; }
    }

    public class LogicCommandModel
    {
        public List<string> Params { get; set; }
        public int Command { get; set; }
    }

    public class LogicFormModel
    {
        public Guid UId { get; set; }
        public Guid CompanyKey { get; set; }
        public string Name { get; set; }

        public LogicFormModel()
        {
        }
    }
}