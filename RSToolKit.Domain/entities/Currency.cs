using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RSToolKit.Domain.Entities
{
    public enum Currency
    {
        [StringValue("Default")]
        [CurrencySymbolValue("$")]
        [CurrencyFormat("en-US")]
        [PayPalValue("USD")]
        Default = 0,
        [StringValue("USD")]
        [PayPalValue("USD")]
        [CurrencySymbolValue("$")]
        [CurrencyFormat("en-US")]
        [iPayValue("840")]
        USD = 1,
        [CurrencySymbolValue("€")]
        [StringValue("Euro")]
        [PayPalValue("Euro")]
        [CurrencyFormat("fr-FR")]
        [iPayValue("978")]
        Euro = 2,
        [CurrencySymbolValue("C$")]
        [StringValue("CAD")]
        [PayPalValue("CAD")]
        [CurrencyFormat("en-US")]
        [iPayValue("124")]
        CAD = 3,
        [CurrencySymbolValue("$")]
        [StringValue("NZD")]
        [iPayValue("554")]
        [CurrencyFormat("en-US")]
        NZD = 4
    }
}
