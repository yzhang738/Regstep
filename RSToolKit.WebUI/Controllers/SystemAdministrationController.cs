using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using System.Web.Routing;
using RSToolKit.Domain.Entities.Clients;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity;
using System.Data.Entity;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using System.Security;
using RSToolKit.Domain.Data;
using Microsoft.AspNet.Identity.Owin;
using RSToolKit.Domain.Entities;
using RSToolKit.Domain.Security;
using RSToolKit.Domain.Entities.Email;
using RSToolKit.WebUI.Infrastructure;
using System.Threading;
using System.IO;
using System.Text.RegularExpressions;
using RSToolKit.Domain;
using RSToolKit.Domain.Logging;
using RSToolKit.Domain.JItems;
using System.Data.SqlClient;
using RSToolKit.Domain.Entities.MerchantAccount;

namespace RSToolKit.WebUI.Controllers
{
    [Authorize(Roles="Super Administrators,System Administrators")]
    public class SystemAdministrationController : RSController
    {

        public SystemAdministrationController(EFDbContext context)
            : base(context)
        {
            iLog.LoggingMethod = "SystemAdministrationController";
        }

        // GET: SystemAdministration
        public ActionResult Index()
        {
            return View();
        }

        #region SmtpServers

        [HttpGet]
        public ActionResult SmtpServers()
        {
            var servers = Repository.Search<SmtpServer>(s => s.CompanyKey == company.UId).ToList();
            return View(servers);
        }

        [HttpGet]
        public ActionResult EditSmtpServer(Guid Company, Guid UId)
        {
            var connString = SqlHelper.GetConnectionString(Company);
            if (connString == null)
                return RedirectToAction("SmtpServers");
            SmtpServer server = null;
            using (var context = new EFDbContext(connString.ConnectionString))
            {
                server = context.SmtpServers.FirstOrDefault(s => s.UId == UId);
                if (server == null)
                    return RedirectToAction("SmtpServers", new { Company = Company });
            }
            return View(server);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditSmtpServer(SmtpServer server, string Password)
        {
            var connString = SqlHelper.GetConnectionString(server.CompanyKey);
            if (connString == null)
                ModelState.AddModelError("", "The company was invalid.");
            if (ModelState.IsValid)
            {
                using (var context = new EFDbContext(connString.ConnectionString))
                {
                    if (server.Primary)
                    {
                        var primaries = context.SmtpServers.Where(s => s.Primary);
                        foreach (var s in primaries)
                        {
                            s.Primary = false;
                        }
                        context.SaveChanges();
                    }
                    var serv = context.SmtpServers.FirstOrDefault(s => s.UId == server.UId);
                    if (serv == null)
                    {
                        ModelState.AddModelError("", "The UId was invalid.");
                        return View(server);
                    }
                    serv.Name = server.Name;
                    serv.Address = server.Address;
                    serv.DateModified = DateTimeOffset.UtcNow;
                    serv.ModifiedBy = user.UId;
                    serv.ModificationToken = Guid.NewGuid();
                    serv.Port = server.Port;
                    serv.SSL = server.SSL;
                    serv.Username = server.Username;
                    serv.Primary = server.Primary;
                    if (!String.IsNullOrEmpty(Password))
                    {
                        serv.EncryptPassword(Password);
                    }
                    var count = context.SaveChanges();
                    if (count != 1)
                    {
                        ModelState.AddModelError("", "SmtpServer was not saved.");
                        return View(server);
                    }
                }
                return RedirectToAction("SmtpServers");
            }
            return View(server);
        }

        [HttpPost]
        [ActionName("SmtpServer")]
        [JsonValidateAntiForgeryToken]
        public ActionResult AddSmtpServer(string address, int port, string username, string key)
        {
            if (!UserManager.IsInRole(user.Id, "Super Administrators"))
                return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = false, Message = "Insufficient Permisions" } };
            var server = SmtpServer.New(Repository, company, user, address, port, username, key);
            return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = true } };
        }

        [HttpDelete]
        [JsonValidateAntiForgeryToken]
        public JsonNetResult DeleteSmtpServer(Guid id)
        {
            var server = Repository.Search<SmtpServer>(s => s.UId == id).FirstOrDefault();
            if (server == null)
                return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = false, Message = "Invalid Object" } };
            Repository.Remove(server);
            Repository.Commit();
            return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Succes = true } };
        }

        #endregion

        #region Company

        #region CRUD

        [HttpGet]
        public ActionResult Companies()
        {
            return View(Context.Companies.ToList());
        }

        [HttpGet]
        [ActionName("Company")]
        public ActionResult EditCompany(long id)
        {
            var company = Repository.Search<Company>(c => c.SortingId == id).FirstOrDefault();
            if (company == null)
                return RedirectToAction("Index");
            ViewBag.Roles = RoleManager.Roles.ToList();
            return View("Company", company);
        }

        [HttpPost]
        [ActionName("Company")]
        [JsonValidateAntiForgeryToken]
        public JsonNetResult NewCompany(string name, string description = null, string database = null, int contactLimit = 100)
        {
            var company = Company.New(Repository, user, name, contactLimit, database, description);
            Repository.Commit();
            return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = true, Location = Url.Action("Company", "SystemAdministration", new { id = company.UId }, Request.Url.Scheme) } };
        }

        [HttpPut]
        [ActionName("Company")]
        [JsonValidateAntiForgeryToken]
        public JsonNetResult EditCompany(Company model)
        {
            var company = Repository.Search<Company>(c => c.UId == model.UId).FirstOrDefault();
            if (company == null)
                return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = false, Message = "Invalid Object" } };
            company.Name = model.Name;
            company.Description = model.Description;
            company.ContactLimit = model.ContactLimit;
            Repository.Commit();
            return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = true } };
        }

        [HttpPut]
        [ActionName("CompanyRole")]
        [JsonValidateAntiForgeryToken]
        public JsonNetResult EditCompanyRole(string id, string roleKey, int amount)
        {
            Company company = null;
            Guid uid;
            long lid;
            if (Guid.TryParse(id, out uid))
                company = Repository.Search<Company>(c => c.UId == uid).FirstOrDefault();
            else if (long.TryParse(id, out lid))
                company = Repository.Search<Company>(c => c.SortingId == lid).FirstOrDefault();
            if (company == null)
                return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = false, Message = "Invalid Object" } };
            var role = RoleManager.Roles.Where(r => r.Id == roleKey).FirstOrDefault();
            if (role == null)
                return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = false, Message = "Invalid Role Object" } };
            var a_role = company.AvailableRoles.Where(r => r.RoleKey == roleKey).FirstOrDefault();
            if (a_role == null)
                a_role = AvailableRoles.New(Repository, user, company, role);
            a_role.TotalAvailable = amount;
            Repository.Commit();
            return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = true, Location = "refresh" } };
        }

        [HttpDelete]
        [ActionName("Company")]
        [JsonValidateAntiForgeryToken]
        public JsonNetResult DeleteCompany(Guid id)
        {
            var company = Repository.Search<Company>(c => c.UId == id).FirstOrDefault();
            if (company == null)
                return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = false, Message = "Invalid Object" } };
            Repository.Remove(company);
            Repository.Commit();
            return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = true } };
        }

        #endregion

        [HttpGet]
        public ActionResult FixCompanies()
        {
            var styles = Repository.Search<DefaultFormStyle>(s => s.CompanyKey == null).ToList();
            var parentUId = Guid.Parse("{27e170e9-80bf-4292-82e3-b8fa348352e3}");
            var companies = Repository.Search<Company>(c => c.UId != parentUId).ToList();
            foreach (var company in companies)
            {
                Context.DefaultFormStyles.RemoveRange(company.FormStyles);
                if (company.FormStyles.Count == 0)
                {
                    foreach (var style in styles)
                    {
                        var n_style = new DefaultFormStyle()
                        {
                            GroupName = style.GroupName,
                            Name = style.Name,
                            Sort = style.Sort,
                            SubSort = style.SubSort,
                            Type = style.Type,
                            Value = style.Value,
                            Variable = style.Variable,
                            CompanyKey = company.UId
                        };
                        company.FormStyles.Add(n_style);
                    }
                }
            }
            Repository.Commit();
            return View();
        }

        [HttpGet]
        public ActionResult UpdateCompanyStyles()
        {
            var parentUId = Guid.Parse("{27e170e9-80bf-4292-82e3-b8fa348352e3}");
            var styles = Repository.Search<DefaultFormStyle>(s => s.CompanyKey == null).ToList();
            var companies = Repository.Search<Company>(c => c.UId != parentUId).ToList();
            foreach (var company in companies)
            {
                foreach (var style in styles)
                {
                    var n_style = new DefaultFormStyle()
                    {
                        GroupName = style.GroupName,
                        Name = style.Name,
                        Sort = style.Sort,
                        SubSort = style.SubSort,
                        Type = style.Type,
                        Value = style.Value,
                        Variable = style.Variable,
                        CompanyKey = company.UId
                    };
                    var c_style = company.FormStyles.Where(s => s.GroupName == n_style.GroupName && s.Name == n_style.Name && s.Variable == n_style.Variable).FirstOrDefault();
                    if (c_style == null)
                        company.FormStyles.Add(n_style);
                    else
                    {
                        c_style.SubSort = n_style.SubSort;
                        c_style.Sort = n_style.Sort;
                    }
                }
                foreach (var form in company.Forms)
                {
                    foreach (var style in company.FormStyles)
                    {
                        var n_style = new FormStyle() { Owner = user.UId, Group = company.UId, GroupName = style.GroupName, Name = style.Name, Variable = style.Variable, Value = style.Value, Sort = style.Sort, SubSort = style.SubSort, Type = style.Type };
                        var f_style = company.FormStyles.Where(s => s.GroupName == n_style.GroupName && s.Name == n_style.Name && s.Variable == n_style.Variable).FirstOrDefault();
                        if (f_style == null)
                            form.FormStyles.Add(n_style);
                        else
                        {
                            f_style.SubSort = n_style.SubSort;
                            f_style.Sort = n_style.Sort;
                        }
                    }
                }
            }
            Repository.Commit();
            return View("FixCompanies");
        }

        #endregion

        #region Email Templates

        public ActionResult EmailTemplates()
        {
            return View(Repository.Search<EmailTemplate>(t => 1 == 1).ToList());
        }

        [HttpDelete]
        [ActionName("EmailTemplate")]
        [JsonValidateAntiForgeryToken]
        public JsonNetResult DeleteEmailTemplate(Guid id)
        {
            var template = Repository.Search<EmailTemplate>(t => t.UId == id).FirstOrDefault();
            if (template == null)
                return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = false, Message = "Invalid Object" } };
            var usedEmails = Repository.Search<RSEmail>(e => e.EmailTemplateKey == template.UId, checkAccess: false);
            var usedCount = usedEmails.Count();
            if (usedCount > 0)
                return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = false, Message = "The template is being used by " + usedCount + " RSEmails." } };
            Repository.Remove(template);
            Repository.Commit();
            return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = true, Location = Url.Action("EmailTemplates", "SystemAdministration", null, Request.Url.Scheme) } };
        }

        [HttpPost]
        [ActionName("EmailTemplate")]
        [JsonValidateAntiForgeryToken]
        public JsonNetResult NewEmailTemplate(string name, string description = "")
        {
            var file = Request.Files[0];
            if (file.ContentType != "text/html")
                return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = false, Message = "Invalid file type" } };
            var template = EmailTemplate.New(name, description);
            using (var s_reader = new StreamReader(file.InputStream))
            {
                var t_contents = s_reader.ReadToEnd();
                t_contents = Regex.Replace(t_contents, @"\r?\n", "", RegexOptions.Singleline);
                var t_ripContent = t_contents;
                var rgx_area = new Regex(@"<!--editorarea_(?<name>[^-]+)-->(?<content>.*?)<!--endeditorarea-->", RegexOptions.IgnoreCase);
                var rgx_variable = new Regex(@"@render_var_(?<variable>[\w\d-]+)(?<params>\[[^\]]*\])?", RegexOptions.IgnoreCase);
                var rgx_varParams = new Regex(@"(?<key>[^:;\[]*):(?<value>[^;\]]*)", RegexOptions.IgnoreCase);
                // First thing we do is go through all the defined email areas.
                t_ripContent = rgx_area.Replace(t_ripContent, @"@render_${name}");
                var html_area = EmailTemplateArea.New(template, "html", t_ripContent, "");
                foreach (Match t_variable in rgx_variable.Matches(t_ripContent))
                {
                    var t_var = t_variable.Groups["variable"].Value;
                    if (template.Variables.Where(v => v.Variable == t_var).Count() != 0)
                        continue;
                    var t_params = t_variable.Groups["params"].Value;
                    string t_default = null;
                    var t_type = "text";
                    var t_name = t_var;
                    var t_description = "";
                    foreach (Match t_param in rgx_varParams.Matches(t_params))
                    {
                        switch (t_param.Groups["key"].Value.ToLower())
                        {
                            case "default":
                                t_default = t_param.Groups["value"].Value;
                                break;
                            case "type":
                                t_type = t_param.Groups["value"].Value;
                                break;
                            case "name":
                                t_name = t_param.Groups["value"].Value;
                                break;
                            case "description":
                                t_description = t_param.Groups["value"].Value;
                                break;
                        }
                    }
                    EmailTemplateVariable.New(template, t_var, t_name, t_description, t_default, t_type);
                }
                foreach (Match m_area in rgx_area.Matches(t_contents))
                {
                    var t_name = m_area.Groups["name"].Value;
                    var t_content = m_area.Groups["content"].Value;
                    var t_area = EmailTemplateArea.New(template, t_name, t_content);
                    foreach (Match t_variable in rgx_variable.Matches(t_content))
                    {
                        var v_var = t_variable.Groups["variable"].Value;
                        if (template.Variables.Where(v => v.Variable == v_var).Count() != 0)
                            continue;
                        var v_params = t_variable.Groups["params"].Value;
                        string v_default = null;
                        var v_type = "text";
                        var v_name = v_var;
                        var v_description = "";
                        foreach (Match t_param in rgx_varParams.Matches(v_params))
                        {
                            switch (t_param.Groups["key"].Value.ToLower())
                            {
                                case "default":
                                    v_default = t_param.Groups["value"].Value;
                                    break;
                                case "type":
                                    v_type = t_param.Groups["value"].Value;
                                    break;
                                case "name":
                                    v_name = t_param.Groups["value"].Value;
                                    break;
                                case "description":
                                    v_description = t_param.Groups["value"].Value;
                                    break;
                            }
                        }
                        TemplateEmailAreaVariable.New(t_area, v_var, v_name, v_description, v_default, v_type);
                    }
                }
                // Before we save, we need to scrub the variables of all parameters
                foreach (var area in template.EmailAreas)
                {
                    area.Html = rgx_variable.Replace(area.Html, @"@render_var_${variable}");
                }
            }
            Repository.Add(template);
            Repository.Commit();
            return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = true } };
        }

        #endregion

        #region Logs

        [HttpGet]
        [ActionName("Logs")]
        [CrumbLabel("Logs")]
        public ActionResult Logs(DateTimeOffset? date = null)
        {
            if (Request.IsAjaxRequest())
            {
                if (date == null)
                    date = DateTimeOffset.Now.AddYears(-1);
                var logs = Repository.Search<Log>(l => l.DateCreated > date.Value).OrderBy(l => l.SortingId).ToList();
                var table = new JTable()
                {
                    Name = "Logs",
                    Parent = "System Logging Data",
                    Id = ""
                };
                var userKeys = logs.Distinct(new LogUserKeyComparer()).ToList();
                var userValues = new List<JTableHeaderPossibleValue>();
                foreach (var e_id in userKeys)
                {
                    var t_user = UserManager.FindById(e_id.UserKey);
                    if (t_user != null)
                        userValues.Add(new JTableHeaderPossibleValue() { Id = t_user.Id, Label = t_user.UserName });
                }
                table.Headers.Add(new JTableHeader() { Label = "Id", Id = "Id", Type = "number" });
                table.Headers.Add(new JTableHeader() { Label = "Thread", Id = "Thread", Type = "itemParent", PossibleValues = logs.Distinct(new LogThreadComparer()).Select(l => new JTableHeaderPossibleValue() { Id = l.Thread, Label = l.Thread }).ToList() });
                table.Headers.Add(new JTableHeader() { Label = "Level", Id = "Level", Type = "number" });
                table.Headers.Add(new JTableHeader() { Label = "Logger", Id = "Logger", Type = "itemParent", PossibleValues = logs.Distinct(new LogLoggerComparer()).Select(l => new JTableHeaderPossibleValue() { Id = l.Thread, Label = l.Thread }).ToList() });
                table.Headers.Add(new JTableHeader() { Label = "Message", Id = "Message" });
                table.Headers.Add(new JTableHeader() { Label = "User", Id = "User", Type = "itemParent", PossibleValues = userValues });
                table.Headers.Add(new JTableHeader() { Label = "Date", Id = "Date", Type = "date" });
                table.Headers.Add(new JTableHeader() { Label = "Modification Token", Id = "modificationToken", Removed = true });
                foreach (var log in logs)
                {
                    var t_row = new JTableRow()
                    {
                        Id = log.SortingId.ToString()
                    };
                    var t_user = userValues.FirstOrDefault(pv => pv.Id == log.UserKey);
                    string t_userName = null;
                    if (t_user != null)
                        t_userName = t_user.Label;
                    t_row.Columns.Add(new JTableColumn() { HeaderId = "Id", Value = log.SortingId.ToString(), PrettyValue = "<a href='#' data-action='Log' data-controller='SystemAdministration' data-options='{\"key\":\"" + log.SortingId + "\"}'>" + log.SortingId.ToString() + "</a>", Id = log.SortingId + "_Id" });
                    t_row.Columns.Add(new JTableColumn() { HeaderId = "Thread", Value = log.Thread, PrettyValue = log.Thread, Id = log.SortingId + "_Thread" });
                    t_row.Columns.Add(new JTableColumn() { HeaderId = "Level", Value = log.Level, PrettyValue = log.Level, Id = log.SortingId + "_Level" });
                    t_row.Columns.Add(new JTableColumn() { HeaderId = "Logger", Value = log.Logger, PrettyValue = log.Logger, Id = log.SortingId + "_Logger" });
                    t_row.Columns.Add(new JTableColumn() { HeaderId = "Message", Value = log.Message, PrettyValue = log.Message, Id = log.SortingId + "_Message" });
                    t_row.Columns.Add(new JTableColumn() { HeaderId = "User", Value = String.IsNullOrWhiteSpace(log.UserKey) ? "System" : log.UserKey, PrettyValue = t_userName, Id = log.SortingId + "_UserKey" });
                    t_row.Columns.Add(new JTableColumn() { HeaderId = "Date", Value = log.DateCreated.ToString("s"), PrettyValue = log.DateCreated.ToString("s"), Id = log.SortingId + "_Date" });
                    t_row.Columns.Add(new JTableColumn() { HeaderId = "modificationToken", Value = log.ModificationToken.ToString(), PrettyValue = log.ModificationToken.ToString() });
                    table.AddRow(t_row);
                }
                return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = true, Table = table } };
            }
            else
            {
                return View();
            }
        }

        [HttpPost]
        [JsonValidateAntiForgeryToken]
        public JsonNetResult LogUpdates(ModificationCheck check)
        {
            var date = DateTimeOffset.Now.AddYears(-1);
            var logs = Repository.Search<Log>(l => l.DateCreated > date).OrderBy(l => l.SortingId).ToList();
            var table = new JTable()
            {
                Name = "Logs",
                Parent = "System Logging Data",
                Id = ""
            };
            var userKeys = logs.Distinct(new LogUserKeyComparer()).ToList();
            var userValues = new List<JTableHeaderPossibleValue>();
            foreach (var e_id in userKeys)
            {
                var t_user = UserManager.FindById(e_id.UserKey);
                if (t_user != null)
                    userValues.Add(new JTableHeaderPossibleValue() { Id = t_user.Id, Label = t_user.UserName });
            }
            table.Headers.Add(new JTableHeader() { Label = "Id", Id = "Id", Type = "number" });
            table.Headers.Add(new JTableHeader() { Label = "Thread", Id = "Thread", Type = "itemParent", PossibleValues = logs.Distinct(new LogThreadComparer()).Select(l => new JTableHeaderPossibleValue() { Id = l.Thread, Label = l.Thread }).ToList() });
            table.Headers.Add(new JTableHeader() { Label = "Level", Id = "Level", Type = "number" });
            table.Headers.Add(new JTableHeader() { Label = "Logger", Id = "Logger", Type = "itemParent", PossibleValues = logs.Distinct(new LogLoggerComparer()).Select(l => new JTableHeaderPossibleValue() { Id = l.Thread, Label = l.Thread }).ToList() });
            table.Headers.Add(new JTableHeader() { Label = "Message", Id = "Message" });
            table.Headers.Add(new JTableHeader() { Label = "User", Id = "User", Type = "itemParent", PossibleValues = userValues });
            table.Headers.Add(new JTableHeader() { Label = "Date", Id = "Date", Type = "date" });
            table.Headers.Add(new JTableHeader() { Label = "Modification Token", Id = "modificationToken", Removed = true });
            foreach (var log in logs)
            {
                var t_row = new JTableRow()
                {
                    Id = log.SortingId.ToString()
                };
                var t_user = userValues.FirstOrDefault(pv => pv.Id == log.UserKey);
                string t_userName = null;
                if (t_user != null)
                    t_userName = t_user.Label;
                t_row.Columns.Add(new JTableColumn() { HeaderId = "Id", Value = log.SortingId.ToString(), PrettyValue = "<a href='#' data-action='Log' data-controller='SystemAdministration' data-options='{\"key\":\"" + log.SortingId + "\"}'>" + log.SortingId.ToString() + "</a>", Id = log.SortingId + "_Id" });
                t_row.Columns.Add(new JTableColumn() { HeaderId = "Thread", Value = log.Thread, PrettyValue = log.Thread, Id = log.SortingId + "_Thread" });
                t_row.Columns.Add(new JTableColumn() { HeaderId = "Level", Value = log.Level, PrettyValue = log.Level, Id = log.SortingId + "_Level" });
                t_row.Columns.Add(new JTableColumn() { HeaderId = "Logger", Value = log.Logger, PrettyValue = log.Logger, Id = log.SortingId + "_Logger" });
                t_row.Columns.Add(new JTableColumn() { HeaderId = "Message", Value = log.Message, PrettyValue = log.Message, Id = log.SortingId + "_Message" });
                t_row.Columns.Add(new JTableColumn() { HeaderId = "User", Value = String.IsNullOrWhiteSpace(log.UserKey) ? "System" : log.UserKey, PrettyValue = t_userName, Id = log.SortingId + "_UserKey" });
                t_row.Columns.Add(new JTableColumn() { HeaderId = "Date", Value = log.DateCreated.ToString("s"), PrettyValue = log.DateCreated.ToString("s"), Id = log.SortingId + "_Date" });
                t_row.Columns.Add(new JTableColumn() { HeaderId = "modificationToken", Value = log.ModificationToken.ToString(), PrettyValue = log.ModificationToken.ToString() });
                table.AddRow(t_row);
            }
            if (check.items.Count == 0)
                return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = true, rows = table.Rows, action = "rows" } };
            if (check.lastFullCheck.AddHours(2) < DateTimeOffset.Now)
                return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = true, Action = "expired" } };
            var rows = new List<JTableRow>();
            foreach (var row in table.Rows)
            {
                var needsUpdate = true;
                var match = check.items.FirstOrDefault(i => i.id == row.Id);
                var modHeader = row.Columns.FirstOrDefault(h => h.HeaderId == "modificationToken");
                if (modHeader == null)
                    continue;
                if (match != null && modHeader.Value == match.token)
                    needsUpdate = false;
                if (needsUpdate)
                    rows.Add(row);
            }
            return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = true, rows = rows, action = "rows" } };
        }


        [HttpGet]
        [ActionName("Log")]
        public ActionResult Log(int key)
        {
            var log = Repository.Search<Log>(l => l.SortingId == key).FirstOrDefault();
            if (log == null)
                return RedirectToAction("Error404", "Error");
            ViewBag.LogUser = UserManager.FindById(log.UserKey ?? "");
            return View(log);
        }

        #endregion

        public JsonNetResult DeleteRegistrant(Guid id)
        {
            var registrant = Repository.Search<Registrant>(r => r.UId == id).FirstOrDefault();
            if (registrant == null)
                return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = false };
            Repository.Remove(registrant);
            Repository.Commit();
            return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = true };
        }

        public JsonNetResult DeleteMarkedRegistrants()
        {
            var registrants = Repository.Search<Registrant>(r => r.Status == RegistrationStatus.Deleted).ToList();
            Repository.Remove(registrants);
            Repository.Commit();
            return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = true };
        }

        [HttpPost]
        [JsonValidateAntiForgeryToken]
        public JsonNetResult UpdateAccounts()
        {
            foreach (var form in Repository.Context.Forms.ToList())
            {
                foreach (var registrant in form.Registrants.ToList())
                {
                    registrant.UpdateAccounts();
                }
            }
            Repository.Commit();
            return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = true } };
        }

        [HttpGet]
        public ActionResult Styles()
        {
            return View(Repository.Search<DefaultFormStyle>(c => c.CompanyKey == null).ToList());
        }

        [HttpPut]
        [ActionName("Styles")]
        [JsonValidateAntiForgeryToken]
        public JsonNetResult EditStyles()
        {
            var formData = Request.Form;
            var formStyles = Repository.Search<DefaultFormStyle>(c => c.CompanyKey == null).ToList();
            var styles = formData.AllKeys.Where(k => k != "UId");
            foreach (var style in styles)
            {
                Guid styleId;
                if (!Guid.TryParse(style, out styleId))
                    continue;
                var oldStyle = formStyles.Where(f => f.UId == styleId).FirstOrDefault();
                oldStyle.Value = String.IsNullOrWhiteSpace(formData[style].Trim()) || formData[style] == "null" ? null : formData[style].Trim();
            }
            Repository.Commit();
            return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = true } };
        }

        [HttpGet]
        public JsonNetResult UpdateInvoiceNumbers(long id)
        {
            var form = Repository.Search<Form>(f => f.SortingId == id).FirstOrDefault();
            if (form == null)
                return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Message = "Invalid Form" } };
            var count = 0;
            using (var conn = new SqlConnection("Server=regstep.com;User Id=web-qna_user;Password=AustinOak98;Initial Catalog=regstep-old"))
            using (var cmd = new SqlCommand("SELECT [Confirmation] FROM [Registrants] WHERE [UId] = @UId;", conn))
            {
                foreach (var registrant in form.Registrants)
                {
                    var oReg = registrant.OldRegistrations.OrderBy(o => o.DateCreated).FirstOrDefault();
                    var useOld = true;
                    conn.Open();
                    cmd.Parameters.Clear();
                    cmd.Parameters.Add("@UId", System.Data.SqlDbType.UniqueIdentifier).Value = registrant.UId;
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            reader.Read();
                            registrant.InvoiceNumber = reader.GetString(0);
                        }
                        else
                        {
                            useOld = true;
                        }
                    }
                    conn.Close();
                    if (useOld)
                    {
                        if (oReg == null)
                            registrant.InvoiceNumber = registrant.Confirmation;
                        else
                            registrant.InvoiceNumber = oReg.Confirmation;
                    }
                    count++;
                }
            }
            var affected = Repository.Commit(skipFormatting: true);

            return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Message = "Success with " + affected + " records affected." } };
        }

        [HttpGet]
        public JsonNetResult UpdateAllInvoiceNumbers()
        {
            foreach (var form in Repository.Search<Form>(f => f.Status == FormStatus.Open || f.Status == FormStatus.Closed))
            {
                var count = 0;
                using (var conn = new SqlConnection("Server=regstep.com;User Id=web-qna_user;Password=AustinOak98;Initial Catalog=regstep-old"))
                using (var cmd = new SqlCommand("SELECT [Confirmation] FROM [Registrants] WHERE [UId] = @UId;", conn))
                {
                    foreach (var registrant in form.Registrants)
                    {
                        var oReg = registrant.OldRegistrations.OrderBy(o => o.DateCreated).FirstOrDefault();
                        var useOld = true;
                        conn.Open();
                        cmd.Parameters.Clear();
                        cmd.Parameters.Add("@UId", System.Data.SqlDbType.UniqueIdentifier).Value = registrant.UId;
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                reader.Read();
                                registrant.InvoiceNumber = reader.GetString(0);
                            }
                            else
                            {
                                useOld = true;
                            }
                        }
                        conn.Close();
                        if (useOld)
                        {
                            if (oReg == null)
                                registrant.InvoiceNumber = registrant.Confirmation;
                            else
                                registrant.InvoiceNumber = oReg.Confirmation;
                        }
                        count++;
                    }
                }
            }
            var affected = Repository.Commit(skipFormatting: true);
            return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Message = "Success with " + affected + " records affected." } };
        }

        [HttpGet]
        public JsonNetResult UpdateRegistrant(string id)
        {
            var rid = ParseId(id);
            Registrant reg = null;
            if (rid is long)
                reg = Repository.Search<Registrant>(r => r.SortingId == (long)rid).FirstOrDefault();
            else if (rid is Guid)
                reg = Repository.Search<Registrant>(r => r.UId == (Guid)rid).FirstOrDefault();
            if (reg == null)
                return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = false, Message = "No Registrant" } };
            reg.UpdateAccounts();
            Repository.Commit();
            return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = true } };
        }

        [HttpGet]
        public ActionResult RemoveSeats(long id)
        {
            var form = Context.Forms.FirstOrDefault(f => f.SortingId == id);
            var toRemove = new List<Seater>();
            foreach (var seating in form.Seatings.ToList())
            {
                foreach (var seater in seating.Seaters.Where(s => s.Seated && s.Registrant.Type == RegistrationType.Live).ToList())
                {
                    var component = seater.Component as IComponentItem;
                    if (component == null)
                        continue;
                    var data = seater.Registrant.Data.FirstOrDefault(d => d.VariableUId == component.ParentKey);
                    if (!seater.Registrant.IsCurrentSelection(component, data))
                        toRemove.Add(seater);
                }
            }
            return View(toRemove);
        }

        public string RemoveAllSeats()
        {
            var toRemove = new List<Seater>();
            var count = 0;
            foreach (var seating in Context.Seatings.ToList())
            {
                foreach (var seater in seating.Seaters.Where(s => s.Seated && s.Registrant.Type == RegistrationType.Live).ToList())
                {
                    count++;
                    var component = seater.Component as IComponentItem;
                    if (component == null)
                        continue;
                    var data = seater.Registrant.Data.FirstOrDefault(d => d.VariableUId == component.ParentKey);
                    if (!seater.Registrant.IsCurrentSelection(component, data))
                    {
                        toRemove.Add(seater);
                    }
                }
            }
            Context.Seaters.RemoveRange(toRemove);
            Context.SaveChanges();
            return toRemove.Count + " / " + count;
            
        }

        [HttpGet]
        [ActionName("DeleteAdjustment")]
        public JsonNetResult DeleteAdjustment(string id)
        {
            var rid = ParseId(id);
            Adjustment adjustment = null;
            if (rid is long)
                adjustment = Repository.Search<Adjustment>(a => a.SortingId == (long)rid).FirstOrDefault();
            else if (rid is Guid)
                adjustment = Repository.Search<Adjustment>(a => a.UId == (Guid)rid).FirstOrDefault();
            if (adjustment == null)
                return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = false, Message = "Invalid object." } };
            Repository.Remove(adjustment);
            return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = true } };
        }
    }
}