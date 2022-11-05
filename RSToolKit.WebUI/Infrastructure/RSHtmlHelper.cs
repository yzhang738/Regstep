using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RSToolKit.Domain.Entities;
using System.Text.RegularExpressions;
using RSToolKit.Domain.Entities.Clients;
using Microsoft.AspNet.Identity;

namespace RSToolKit.WebUI.Infrastructure
{
    public static class RSHtmlHelper
    {
        /// <summary>
        /// A list of glyphicons to use from the bootstrap 3.0 css for various c# classes.
        /// </summary>
        private static Dictionary<string, string> p_icons = new Dictionary<string, string>()
        {
            { "Form", "glyphicon glyphicon-list-alt" },
            { "EmailCampaign", "glyphicon glyphicon-inbox" },
            { "MerchantAccountInfo", "glyphicon glyphicon-credit-card" },
            { "RSEmail", "glyphicon glyphicon-envelope" },
            { "ContactReport", "glyphicon glyphicon-list" },
            { "SavedList", "glyphicon glyphicon-list" }
        };

        /// <summary>
        /// Regex to parse out the type from a C# class type.
        /// </summary>
        private static Regex rgx_type = new Regex(@"\.?(?<type>[^\.]+)$");

        /// <summary>
        /// Formats a datetime offset to the time zone that the logged in user specified.
        /// </summary>
        /// <param name="htmlHelper">The html helper fromt he view context.</param>
        /// <param name="date">The date wanting to convert.</param>
        /// <param name="stringFormat">The string format for the date.</param>
        /// <param name="format">The IFormatProvider to be used.</param>
        /// <returns>Returns an MvcHtmlString of the formatted date.</returns>
        public static IHtmlString UserDate(this HtmlHelper htmlHelper, DateTimeOffset date, string stringFormat = "rs_s", IFormatProvider format = null)
        {
            var rsController = (IRegStepController)htmlHelper.ViewContext.Controller;
            var user = rsController.AppUser;
            switch (stringFormat)
            {
                case "rs_s":
                    stringFormat = "yyyy-M-d h:mm:ss tt";
                    break;
            }
            if (user == null)
            {
                if (format != null && !String.IsNullOrEmpty(stringFormat))
                    return new MvcHtmlString(date.ToString(stringFormat, format));
                if (format != null)
                    return new MvcHtmlString(date.ToString(format));
                if (!String.IsNullOrEmpty(stringFormat))
                    return new MvcHtmlString(date.ToString(stringFormat));
                else
                    return new MvcHtmlString(date.ToString());
            }
            var userDate = TimeZoneInfo.ConvertTime(date, user.TimeZone);
            if (format != null && !String.IsNullOrEmpty(stringFormat))
                return new MvcHtmlString(userDate.ToString(stringFormat, format));
            if (format != null)
                return new MvcHtmlString(userDate.ToString(format));
            if (!String.IsNullOrEmpty(stringFormat))
                return new MvcHtmlString(userDate.ToString(stringFormat));
            else
                return new MvcHtmlString(userDate.ToString());
        }

        /// <summary>
        /// Renders a folder select tree.
        /// </summary>
        /// <param name="htmlHelper">The html helper that is being extended.</param>
        /// <param name="start">The folder to start rendering from.</param>
        /// <param name="types">The type of items to display in the folder tree.</param>
        /// <param name="id">The id of the folder tree.</param>
        /// <returns>An html string to display on an html page.</returns>
        public static IHtmlString FolderSelect(this HtmlHelper htmlHelper, Guid? start = null, string[] types = null, string id = null)
        {
            if (!(htmlHelper.ViewContext.Controller is RSController))
                throw new InvalidOperationException("The view must be part of a RSController.");
            var html = "<div class=\"tree\"" + (id == null ? "" : " id=\"" + id + "\"") + "><ul>";
            var rs_controller = (RSController)htmlHelper.ViewContext.Controller;
            var repo = rs_controller.Repository;
            var company = rs_controller.company;
            types = types ?? new string[0];
            // Render folders
            var base_folders = new List<Folder>();
            if (start.HasValue)
            {
                var base_folder = repo.Search<Folder>(f => f.CompanyKey == company.UId && f.UId == start.Value).FirstOrDefault();
                if (base_folder == null)
                    return new MvcHtmlString("");
                base_folders.Add(base_folder);
            }
            else
            {
                base_folders.AddRange(repo.Search<Folder>(f => f.CompanyKey == company.UId && !f.ParentKey.HasValue).ToList());
            }
            base_folders.OrderBy(f => f.Name);
            foreach (var folder in base_folders)
            {
                html += RenderFolder(rs_controller, folder, types);
            }
            html += "</ul></div>";
            return new MvcHtmlString(html);
        }

        /// <summary>
        /// Renders a folder with it's children.
        /// </summary>
        /// <param name="controller">The controller that is used to do the rendering. This is used to get the Repository.</param>
        /// <param name="folder">The folder to render.</param>
        /// <param name="types">The type of items to display</param>
        /// <returns>A string representing unencoded raw html.</returns>
        private static string RenderFolder(RSController controller, Folder folder, string[] types)
        {
            var html = "<li>";
            html += "<span id=\"" + folder.UId + "\" class=\"tree-folder tree-item tree-haschildren\"><span class=\"glyphicon glyphicon-folder-close\"></span> <span class=\"tree-item-name\">" + folder.Name + "</span></span>";
            html += "<ul>";

            foreach (var c_folder in folder.Children)
            {
                html += RenderFolder(controller, c_folder, types);
            }

            foreach (var pointer in folder.Pointers)
            {
                var node = controller.Repository.Search<RSToolKit.Domain.Data.INamedNode>(c => c.UId == pointer.Target).FirstOrDefault();
                if (node == null)
                    continue;
                var t_str = node.GetType().BaseType.Name;
                if (types.Length > 0 && !types.Contains(t_str))
                    continue;
                html += "<li><span id=\"" + node.UId + "\" class=\"tree-node tree-item\"><span class=\"" + p_icons[t_str] + "\"></span> <span class=\"tree-item-name\">" + node.Name + "</span></span></li>";
            }

            html += "</ul>";
            html += "</li>";

            return html;
        }

    }
}