using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using RSToolKit.Domain.Entities;
using System.Web.Mvc;
using RSToolKit.Domain.Entities.Clients;
using RSToolKit.Domain.Data;
using System.Text.RegularExpressions;
using RSToolKit.Domain.Entities.Components;

namespace RSToolKit.WebUI.Infrastructure
{
    public static class RSHtml
    {
        private static readonly Regex RegexBetweenTags = new Regex(@">(?! )\s+", RegexOptions.Compiled);
        private static readonly Regex RegexLineBreaks = new Regex(@"([\n\s])+?(?<= {2,})<", RegexOptions.Compiled);
        private static readonly Regex RegexRemoveTabs = new Regex(@"([\t])+", RegexOptions.Compiled);

        /// <summary>
        /// Renders a folder select tree.
        /// </summary>
        /// <param name="company">The company for the folder tree.</param>
        /// <returns>A MvcHtmlString that contains the tree in a bootstrap formatted div.</returns>
        public static MvcHtmlString RenderFolderSelect(Guid company, INode node = null, string returnAfterDelete = null)
        {
            var Url = new UrlHelper(HttpContext.Current.Request.RequestContext);
            if (String.IsNullOrEmpty(returnAfterDelete))
            {
                returnAfterDelete = Url.Action("Index", "FormBuilder", null, "https");
            }
            var html = @"<div class=""folder-list"">";
            using (var context = new EFDbContext())
            {
                Guid? currentFolder = null;
                if (node != null)
                {
                    var pointer = context.Pointers.Where(p => p.Target == node.UId).FirstOrDefault();
                    if (pointer != null)
                        currentFolder = pointer.FolderKey;
                }
                var folders = context.Folders.Where(f => f.CompanyKey == company && f.ParentKey == null).ToList();
                foreach (var folder in folders)
                {
                    html += RenderFolderSelect(folder, returnAfterDelete, currentFolder);
                }
            }
            html += @"
</div>";
            return new MvcHtmlString(html);
        }
        /// <summary>
        /// Renders a folder select tree for a folder.
        /// </summary>
        /// <param name="folder">The folder for the folder tree.</param>
        /// <returns>A MvcHtmlString that contains the tree in a bootstrap formatted div.</returns>

        private static MvcHtmlString RenderFolderSelect(Folder folder, string returnAfterDelete, Guid? currentFolder = null)
        {
            var selected = folder.UId == currentFolder;
            var display = @"<a href=""#"" class=""folder-link" + (selected ? " folder-selected" : "") + @""" data-id=""" + folder.UId.ToString() + @"""><span class=""glyphicon glyphicon-link""></span> Link</a><span class=""folder-selected-text" + (selected ? "" : " folder-not-selected") + @""">Current</span>";
            var html = @"<div class=""row folder-row"" id=""folderSelect_" + folder.UId.ToString() + @""">
    <div class=""row folder"">
        <div class=""col-xs-5 folder-label""><span class=""glyphicon glyphicon-folder-close""></span> " + folder.Name + @"</div>
        <div class=""col-xs-7 folder-action"">" + display + @"</div>
    </div>
    <div class=""row folder-contents collapse"">";
            foreach (var child in folder.Children)
            {
                html += RenderFolderSelect(child, returnAfterDelete);
            }
            html += @"
    </div>
</div>";
            return new MvcHtmlString(html);
        }

        /// <summary>
        /// Renders a folder tree to view files in the folder with no permission restrictions.
        /// </summary>
        /// <param name="folder">The folder to render down.</param>
        /// <param name="descriminators">The descriminators for picking pointers.</param>
        /// <returns>An MvcHtmlString of the formatted folder tree.</returns>
        public static MvcHtmlString RenderFolder(Folder folder, string returnAfterDelete, params string[] descriminators)
        {
            var Request = HttpContext.Current.Request;
            var Url = new UrlHelper(Request.RequestContext);
            descriminators = descriminators ?? new string[0];
            if (String.IsNullOrEmpty(returnAfterDelete))
            {
                returnAfterDelete = Url.Action("Index", "FormBuilder", null, "https");
            }
            foreach (string descriminator in descriminators)
            {
                descriminator.Trim();
                descriminator.ToLower();
            }
            var html = @"
<div class=""row folder-row"" id=""folder_" + folder.UId.ToString() + @""">
    <div class=""row folder"">
        <div class=""col-xs-5 folder-label""><span class=""glyphicon glyphicon-folder-close""></span> " + folder.Name + @"</div>
        <div class=""folder-action col-xs-3""><a href=""" + Url.Action("EditFolder", "FormBuilder", new { id = folder.UId }) + @"""><span class=""glyphicon glyphicon-edit""></span> Edit</a></div>
        <div class=""folder-action col-xs-3"">
            <a data-http-method=""delete"" href=""" + Url.Action("Index", "FormBuilder", null, Request.Url.Scheme) + @""" data-redirect=""" + Url.Action("Index", "FormBuilder", null, Request.Url.Scheme) + @""" data-id=""" + folder.UId + @"""><span class=""glyphicon glyphicon-trash""></span> Delete</a>
        </div>
    </div>
    <div class=""row folder-contents collapse"">";
            foreach (var f in folder.Children)
                html += RenderFolder(f, returnAfterDelete).ToString();
            using (var context = new RSToolKit.Domain.Data.EFDbContext())
            {
                foreach (var p in folder.Pointers)
                {
                    INamedNode node = null;
                    var deleteAction = "";
                    var controller = "";
                    var editAction = "";
                    var glyphicon = "glyphicon glyphicon";
                    switch ("form".ToLower().Trim())
                    {
                        case "form":
                            node = context.Forms.Where(f => f.UId == p.Target).FirstOrDefault();
                            deleteAction = "Form";
                            editAction = "Form";
                            controller = "FormBuilder";
                            glyphicon += "-file";
                            break;
                        case "email campaign":
                            node = context.EmailCampaigns.Where(e => e.UId == p.Target).FirstOrDefault();
                            deleteAction = "DeleteEmailCampaign";
                            editAction = "EditEmailCampaign";
                            controller = "Email";
                            glyphicon += "-book";
                            break;
                        case "email":
                            node = context.RSEmails.Where(e => e.UId == p.Target).FirstOrDefault();
                            deleteAction = "DeleteEmail";
                            editAction = "EditEmail";
                            controller = "Email";
                            glyphicon += "-envelope";
                            break;
                        case "survey":
                            node = context.Forms.Where(f => f.UId == p.Target).FirstOrDefault();
                            deleteAction = "DeleteSurvey";
                            editAction = "EditSurvey";
                            controller = "FormBuilder";
                            glyphicon += "-star-empty";
                            break;
                    }
                    if (node == null)
                        continue;
                    html += @"
        <div class=""row folder-item"">
            <div class=""col-xs-5""><span class=""" + glyphicon + @"""></span> " + node.Name + @"</div>
            <div class=""folder-item-action col-xs-3""><a href=""" + Url.Action(editAction, controller, new { id = p.Target }) + @"""><span class=""glyphicon glyphicon-edit""></span> Edit</a></div>
            <div class=""folder-item-action col-xs-3"">
                <a data-http-method=""delete"" href=""" + Url.Action(deleteAction, controller, null, Request.Url.Scheme) + @""" data-redirect=""" + returnAfterDelete + @""" data-object='{""id"":""" + node.UId + @"""}'><span class=""glyphicon glyphicon-trash""></span> Delete</a>
            </div>
        </div>";
                }
                html += @"
    </div>";
            }
            html += @"
</div>
";
            return new MvcHtmlString(html);
        }

        /// <summary>
        /// Gets the name of a folder by id.
        /// </summary>
        /// <param name="id">The id of the folder.</param>
        /// <returns>The folder name.</returns>
        public static string GetFolderName(Guid id)
        {
            using (var context = new EFDbContext())
            {
                var folder = context.Folders.Where(f => f.UId == id).FirstOrDefault();
                return folder == null ? "No Folder Found" : folder.Name;
            }
        }

        /// <summary>
        /// Gets the name of a folder for a form.
        /// </summary>
        /// <param name="form">The form to be used.</param>
        /// <returns>The folder name.</returns>
        public static string GetFolderName(Form form)
        {
            using (var context = new EFDbContext())
            {
                var pointer = context.Pointers.Where(p => p.Target == form.UId).FirstOrDefault();
                if (pointer == null)
                    return "No Pointer";
                var id = pointer.FolderKey;
                var folder = context.Folders.Where(f => f.UId == id).FirstOrDefault();
                return folder == null ? "No Folder Found" : folder.Name;
            }
        }

        /// <summary>
        /// Converts a datetimeoffset to a string based on users timezone preference.
        /// </summary>
        /// <param name="date">The data that is being converted.</param>
        /// <param name="user">The user to grab time zone from.</param>
        /// <returns>The string format.</returns>
        public static string UserDateString(this DateTimeOffset date, User user)
        {
            if (date == null)
                return "";
            return UserDateString(user, date, "rs-s");
        }

        /// <summary>
        /// Converts a datetime to the users specified TimeZone.
        /// </summary>
        /// <param name="user">The user to update the date for.</param>
        /// <param name="date">The date needing updating.</param>
        /// <param name="format">The format to use for the date.</param>
        /// <returns>Returns an MvcHtmlString of the modified and formatted date.</returns>
        public static string UserDateString(User user, DateTimeOffset date, string format = "")
        {
            var userDate = UserDate(user, date);
            if (format == "rs-s")
                format = "yyyy-M-d h:mm:ss tt";
            if (String.IsNullOrWhiteSpace(format))
                return userDate.ToString();
            return userDate.ToString(format);
        }


        /// <summary>
        /// Converts a datetime to the users specified TimeZone.
        /// </summary>
        /// <param name="user">The user to update the date for.</param>
        /// <param name="date">The date needing updating.</param>
        /// <param name="format">The format to use for the date.</param>
        /// <returns>Returns an MvcHtmlString of the modified and formatted date.</returns>
        public static string UserDateString(User user, DateTimeOffset date, IFormatProvider format)
        {
            var userDate = UserDate(user, date);
            return userDate.ToString(format);
        }
        /// <summary>
        /// Converts a datetime to the users specified TimeZone.
        /// </summary>
        /// <param name="user">The user to update the date for.</param>
        /// <param name="date">The date needing updating.</param>
        /// <returns>Returns an MvcHtmlString of the modified date.</returns>
        public static DateTimeOffset UserDate(User user, DateTimeOffset date)
        {
            if (user != null)
                return TimeZoneInfo.ConvertTime(date, user.TimeZone);
            return date;
        }

        /// <summary>
        /// Gets the common name for a form when only the UId is available.
        /// </summary>
        /// <param name="id">The global unique identifier of a form.</param>
        /// <returns>Returns a string of the form name.</returns>
        public static string GetCommonName(Guid id)
        {
            using (var context = new EFDbContext())
            {
                var form = context.Forms.FirstOrDefault(f => f.UId == id);
                if (form == null)
                    return null;
                else
                    return form.Name;
            }
        }

        public static string GetFamiliarName(string value)
        {
            using (var context = new EFDbContext())
            {
                Guid id;
                if (!Guid.TryParse(value, out id))
                    return value;
                //First we see if it a form.
                var form = context.Forms.Where(f => f.UId == id).FirstOrDefault();
                if (form != null)
                    return form.Name;
                var component = context.Components.Where(f => f.UId == id).FirstOrDefault();
                if (component != null)
                    return component.Name;
                return value;
            }
        }

        #region Registration


        #endregion

    }
}