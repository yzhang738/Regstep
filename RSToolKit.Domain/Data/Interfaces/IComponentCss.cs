using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RSToolKit.Domain.Entities;

namespace RSToolKit.Domain.Data
{
    public interface IComponentCss
    {
        CSS Css { get; set; }
        CSS LabelCss { get; set; }
        CSS AgendaCss { get; set; }
        CSS AltTextCss { get; set; }
        Guid CssUId { get; set; }
        Guid LabelCssUId { get; set; }
        Guid AgendaCssUId { get; set; }
        Guid AltTextCssUId { get; set; }
    }
}
