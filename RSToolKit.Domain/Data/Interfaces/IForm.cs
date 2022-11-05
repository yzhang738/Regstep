using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RSToolKit.Domain.Entities.Email;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.SqlClient;
using RSToolKit.Domain.Data;
using System.Globalization;
using RSToolKit.Domain.Entities.MerchantAccount;
using RSToolKit.Domain.Entities.Components;
using RSToolKit.Domain.Entities.Clients;
using System.Web.Mvc;
using RSToolKit.Domain.Entities;
using RSToolKit.Domain.Entities;

namespace RSToolKit.Domain.Data
{
    [Obsolete]
    public interface IForm
        : IBaseItem, ICustomTextHolder, IPointerTarget, IPersonHolder, IEmailHolder
    {
        List<LogicBlock> LogicBlocks { get; set; }
        [CascadeDelete]
        List<Page> Pages { get; set; }
        List<Audience> Audiences { get; set; }
        List<Variable> Variables { get; set; }
        List<Tag> Tags { get; set; }
        List<Seating> Seatings { get; set; }
        List<FormStyle> FormStyles { get; set; }
        List<DefaultComponentOrder> DefaultComponentOrders { get; set; }
        List<PromotionCode> PromotionalCodes { get; set; }
        List<Registrant> Registrants { get; set; }
        List<SingleFormReport> CustomReports { get; set; }
    }
}
