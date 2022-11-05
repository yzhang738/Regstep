using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Web.Helpers.AntiXsrf;
using System.Web.Helpers.Claims;
using System.Web.Mvc;
using System.Web.WebPages.Resources;
using System.Web.Helpers;
using System.Text.RegularExpressions;

namespace System.Web.Helpers
{
    public static class JsonAntiForgery
    {
        public static HtmlString JsonAntiforgeryToken(this HtmlHelper antiforgery)
        {
            var tokenHtml = AntiForgery.GetHtml();
            var token = Regex.Match(tokenHtml.ToString(), @"value=""(?<token>[^""]*)""").Groups["token"].Value;
            return new HtmlString(token);
        }
    }

}

