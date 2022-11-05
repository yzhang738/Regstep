using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RSToolKit.Domain.Entities.Email;

namespace RSToolKit.Domain.Data
{
    public interface IEmailRecipient
        : IRSData
    {
        string Email { get; set; }
        List<EmailSend> EmailSends { get; set; }
    }
}
