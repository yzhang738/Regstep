using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Mail;
using RSToolKit.Domain.Entities;
using RSToolKit.Domain.Entities.Email;
using RSToolKit.Domain.Entities;
using RSToolKit.Domain.Entities.Clients;

namespace RSToolKit.Domain.Data
{
    public interface IEmail
        : ICompanyHolder, ILogicHolder, INodeItem
    {
        string To { get; set; }
        string From { get; set; }
        string CC { get; set; }
        string BCC { get; set; }
        string Subject { get; set; }
        string Description { get; set; }
        string Name { get; set; }
        Form Form { get; set; }
        Guid? FormKey { get; set; }
        EmailCampaign EmailCampaign { get; set; }
        Guid? EmailCampaignKey { get; set; }
        SavedList SavedList { get; set; }
        Guid? SavedListKey { get; set; }
        ContactReport ContactReport { get; set; }
        Guid? ContactReportKey { get; set; }
        string PlainText { get; set; }
        IEmailList EmailList { get; }
        Guid? EmailListKey { get; set; }
        EmailType EmailType { get; set; }
        DateTimeOffset? SendTime { get; set; }
        long IntervalTicks { get;set; }
        int MaxSends { get; set; }
        bool RepeatSending { get; set; }
        TimeSpan? SendInterval { get; set; }
        double IntervalSeconds { get; set; }
        EmailData GenerateEmail(RSParser parser);
        IEnumerable<Logic> DeepCopyLogics(Form form, Form oldForm);
    }
}
