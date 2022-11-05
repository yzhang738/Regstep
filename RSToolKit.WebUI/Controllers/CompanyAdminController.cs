using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RSToolKit.WebUI.Infrastructure;
using RSToolKit.Domain.Entities;
using RSToolKit.Domain.Entities.Clients;
using RSToolKit.Domain.Entities;
using RSToolKit.Domain.Data;
using OfficeOpenXml;
using System.IO;
using RSToolKit.Domain.Entities.Components;
using System.Text.RegularExpressions;
using System.Data.Entity;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using RSToolKit.Domain.Security;
using RSToolKit.Domain.Entities.MerchantAccount;
using RSToolKit.WebUI.Models;
using RSToolKit.Domain.Entities.Email;
using Newtonsoft.Json;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity;
using RSToolKit.Domain;
using System.Net.Mail;

namespace RSToolKit.WebUI.Controllers
{
    [Authorize(Roles="Company Administrators,Super Administrators,System Administrators, Adminstrators")]
    public class CompanyAdminController : RSController
    {

        public CompanyAdminController(EFDbContext context)
            : base(context)
        {
            iLog.LoggingMethod = "CompanyAdminController";

        }

        [HttpGet]
        public ActionResult Index()
        {
            return View(company);
        }

        [HttpGet]
        public ActionResult Users()
        {
            ViewBag.UM = UserManager;
            ViewBag.RM = RoleManager;
            return View(company);
        }

        [HttpPost]
        [ActionName("CompanyAdmin")]
        [JsonValidateAntiForgeryToken]
        public JsonNetResult SetCompanyAdmin(Guid id)
        {
            var userEdit = UserManager.FindById(id.ToString());
            if (userEdit == null)
                return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = false, Message = "Invalid Object" } };
            if (user.CompanyKey != userEdit.CompanyKey)
                if (!User.IsInRole("Super Administrators") && !User.IsInRole("System Administrators") && !User.IsInRole("Administrators"))
                    return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = false, Message = "Insufficient Permissions" } };
            if (UserManager.IsInRole(id.ToString(), "Company Administrators"))
                return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = false, Message = "Already in Role" } };
            else
            {
                var result = UserManager.AddToRole(id.ToString(), "Company Administrators");
                return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = result.Succeeded, Message = String.Join(",", result.Errors.ToArray()), Location = "refresh" } };
            }
        }

        [HttpDelete]
        [ActionName("CompanyAdmin")]
        [JsonValidateAntiForgeryToken]
        public JsonNetResult DeleteCompanyAdmin(Guid id)
        {
            var userEdit = UserManager.FindById(id.ToString());
            if (userEdit == null)
                return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = false, Message = "Invalid Object" } };
            if (user.CompanyKey != userEdit.CompanyKey)
                if (!User.IsInRole("Super Administrators") && !User.IsInRole("System Administrators") && !User.IsInRole("Administrators"))
                    return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = false, Message = "Insufficient Permissions" } };
            if (!UserManager.IsInRole(id.ToString(), "Company Administrators"))
                return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = false, Message = "User not in Role" } };
            else
            {
                var result = UserManager.RemoveFromRole(id.ToString(), "Company Administrators");
                return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = result.Succeeded, Message = String.Join(",", result.Errors.ToArray()) } };
            }
        }

        [HttpPost]
        [ActionName("SetCompanyGroup")]
        [JsonValidateAntiForgeryToken]
        public JsonNetResult SetCompanyGroup(Guid id, Guid groupKey)
        {
            var group = Repository.Search<CustomGroup>(c => c.UId == groupKey).FirstOrDefault();
            if (group == null)
                return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = false, Message = "Invalid Object" } };
            var userEdit = UserManager.FindById(id.ToString());
            if (userEdit == null)
                return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = false, Message = "Invalid Object" } };
            if (user.CompanyKey != userEdit.CompanyKey)
                if (!User.IsInRole("Super Administrators") && !User.IsInRole("System Administrators") && !User.IsInRole("Administrators"))
                    return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = false, Message = "Insufficient Permissions" } };
            if (user.CustomGroups.Where(g => g.UId == groupKey).Count() != 0)
                return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = false, Message = "Already in Group" } };
            else
            {
                userEdit.CustomGroups.Add(group);
                UserManager.Update(userEdit);
                return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = true, Location = "refresh" } };
            }
        }

        [HttpDelete]
        [ActionName("SetCompanyGroup")]
        [JsonValidateAntiForgeryToken]
        public JsonNetResult DeleteSetCompanyGroup(Guid id, Guid groupKey)
        {
            var group = Repository.Search<CustomGroup>(c => c.UId == groupKey).FirstOrDefault();
            if (group == null)
                return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = false, Message = "Invalid Object" } };
            var userEdit = UserManager.FindById(id.ToString());
            if (userEdit == null)
                return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = false, Message = "Invalid Object" } };
            if (user.CompanyKey != userEdit.CompanyKey)
                if (!User.IsInRole("Super Administrators") && !User.IsInRole("System Administrators") && !User.IsInRole("Administrators"))
                    return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = false, Message = "Insufficient Permissions" } };
            if (user.CustomGroups.Where(g => g.UId == groupKey).Count() == 0)
                return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = false, Message = "User not in Group" } };
            else
            {
                userEdit.CustomGroups.Remove(group);
                UserManager.Update(userEdit);
                return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = true } };
            }
        }

        [HttpPost]
        [ActionName("CompanyGroup")]
        [JsonValidateAntiForgeryToken]
        public JsonNetResult NewCompanyGroup(string name)
        {
            if (Repository.Search<CustomGroup>(g => g.CompanyKey == company.UId && g.Name == name).Count() > 0)
                return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = false, Message = "Object Already Exists" } };
            var group = CustomGroup.New(company);
            group.Name = name;
            Repository.Add(group);
            Repository.Commit();
            return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = true, Location = "refresh" } };
        }

        [HttpDelete]
        [ActionName("CompanyGroup")]
        [JsonValidateAntiForgeryToken]
        public JsonNetResult DeleteCompanyGroup(Guid id)
        {
            var group = Repository.Search<CustomGroup>(g => g.CompanyKey == company.UId && g.UId == id).FirstOrDefault();
            if (group == null)
                return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = false, Message = "Invalid Object" } };
            Repository.Remove(group);
            Repository.Commit();
            return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = true } };
        }

        [HttpPost]
        [ActionName("Role")]
        [JsonValidateAntiForgeryToken]
        public JsonNetResult SetRole(Guid id, string role)
        {
            var userEdit = UserManager.FindById(id.ToString());
            if (userEdit == null)
                return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = false, Message = "Invalid Object" } };
            if (user.CompanyKey != userEdit.CompanyKey)
                if (!User.IsInRole("Super Administrators") && !User.IsInRole("System Administrators") && !User.IsInRole("Administrators"))
                    return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = false, Message = "Insufficient Permissions" } };
            var count = 0;
            var appRole = RoleManager.FindByName(role);
            if (appRole == null)
            {
                return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = false, Message = "Invalid Object" } };
            }
            var availableRoles = company.AvailableRoles.Where(a => a.RoleKey == appRole.Id).FirstOrDefault();
            if (availableRoles == null)
                return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = false, Message = "Invalid Object" } };
            if (availableRoles.TotalAvailable != -1)
            {
                foreach (var count_user in company.Users)
                {
                    if (count_user.Roles.Where(r => r.RoleId == appRole.Id).Count() == 1)
                        count++;
                }
                if (count >= availableRoles.TotalAvailable)
                    return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = false, Message = "Not enough role spots." } };
            }
            if (UserManager.IsInRole(userEdit.Id, role))
                return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = false, Message = "Already in Role" } };
            else
            {
                var result = UserManager.AddToRole(userEdit.Id, role);
                return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = result.Succeeded, Message = String.Join(",", result.Errors.ToArray()), Location = "refresh" } };
            }
        }

        [HttpDelete]
        [ActionName("Role")]
        [JsonValidateAntiForgeryToken]
        public JsonNetResult DeleteRole(Guid id, string role)
        {
            var userEdit = UserManager.FindById(id.ToString());
            if (userEdit == null)
                return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = false, Message = "Invalid Object" } };
            if (user.CompanyKey != userEdit.CompanyKey)
                if (!User.IsInRole("Super Administrators") && !User.IsInRole("System Administrators") && !User.IsInRole("Administrators"))
                    return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = false, Message = "Insufficient Permissions" } };
            if (!UserManager.IsInRole(userEdit.Id, role))
                return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = false, Message = "User not in Role" } };
            else
            {
                var result = UserManager.RemoveFromRole(userEdit.Id, role);
                return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = result.Succeeded, Message = String.Join(",", result.Errors.ToArray()), Location = "refresh" } };
            }
        }

        [HttpGet]
        public ActionResult CompanyGroups()
        {
            return View(company);
        }

        [HttpGet]
        public ActionResult Company()
        {
            return View(company);
        }

        [HttpPut]
        [JsonValidateAntiForgeryToken]
        public JsonNetResult Company(Company model)
        {
            var companyEdit = Repository.Search<Company>(s => s.UId == model.UId).FirstOrDefault();
            if (companyEdit == null)
                return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = false, Message = "Invalid Object" } };
            if (user.CompanyKey != companyEdit.UId)
                if (!User.IsInRole("Super Administrators") && !User.IsInRole("System Administrators") && !User.IsInRole("Administrators"))
                    return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = false, Message = "Insufficient Permissions" } };
            companyEdit.Name = model.Name;
            companyEdit.BillingAddressLine1 = model.BillingAddressLine1;
            companyEdit.BillingAddressLine2 = model.BillingAddressLine2;
            companyEdit.BillingCity = model.BillingCity;
            companyEdit.BillingCountry = model.BillingCountry;
            companyEdit.BillingState = model.BillingState;
            companyEdit.BillingZip = model.BillingZip;
            companyEdit.ShippingAddressLine1 = model.ShippingAddressLine1;
            companyEdit.ShippingAddressLine2 = model.ShippingAddressLine2;
            companyEdit.ShippingCity = model.ShippingCity;
            companyEdit.ShippingCountry = model.ShippingCountry;
            companyEdit.ShippingState = model.ShippingState;
            companyEdit.ShippingZip = model.ShippingZip;
            companyEdit.RegistrationAddressLine1 = model.RegistrationAddressLine1;
            companyEdit.RegistrationAddressLine2 = model.RegistrationAddressLine2;
            companyEdit.RegistrationCity = model.RegistrationCity;
            companyEdit.RegistrationCountry = model.RegistrationCountry;
            companyEdit.RegistrationEmail = model.RegistrationEmail;
            companyEdit.RegistrationFax = model.RegistrationFax;
            companyEdit.RegistrationPhone = model.RegistrationPhone;
            companyEdit.RegistrationState = model.RegistrationState;
            companyEdit.RegistrationZip = model.RegistrationZip;
            Repository.Commit();
            return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = true } };
        }

        [HttpPost]
        [ActionName("User")]
        [JsonValidateAntiForgeryToken]
        public JsonNetResult NewUser(string username, string email, string firstname, string lastname)
        {
            var n_user = new User()
            {
                Email = email,
                FirstName = firstname,
                LastName = lastname,
                CompanyKey = company.UId,
                UserName = username,
                ValidationToken = Guid.NewGuid()
            };
            var password = System.Web.Security.Membership.GeneratePassword(8, 2);
            var result = UserManager.Create(n_user, password);
            if (result.Succeeded)
            {
                SendValidateEmail(n_user);
                return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = true } };
            }
            return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = false, Message = String.Join(",", result.Errors.ToArray()), Location = "refresh" } };
        }

        [HttpDelete]
        [ActionName("User")]
        [JsonValidateAntiForgeryToken]
        public JsonNetResult DeleteUser(string id)
        {
            var d_user = UserManager.FindById(id);
            if (d_user == null)
                return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = false, Message = "Invalid Object" } };
            if (user.CompanyKey != d_user.CompanyKey)
                if (!User.IsInRole("Super Administrators") && !User.IsInRole("System Administrators") && !User.IsInRole("Administrators"))
                    return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = false, Message = "Insufficient Permissions" } };
            var result = UserManager.Delete(d_user, Repository);
            if (result.Succeeded)
                return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = true } };
            return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = false, Message = String.Join(",", result.Errors.ToArray()), Location = "refresh" } };
        }

        [HttpGet]
        [ActionName("AccessLog")]
        public ActionResult GetAccessLog()
        {
            var r_logs = Repository.Search<AccessLog>(al => al.CompanyKey == company.UId);
            var logs = AccessLog.SortAndCombine(r_logs);
            return View("AccessLog", logs);
        }

        #region TinyUrls

        [HttpGet]
        public ActionResult TinyUrls()
        {
            var forms = company.Forms.ToList();
            foreach (var tu in company.TinyUrls)
                forms.Remove(tu.Form);
            ViewBag.Forms = forms;
            return View(company.TinyUrls.AsEnumerable());
        }

        [HttpGet]
        [ActionName("TinyUrl")]
        public ActionResult GetTinyUrl(Guid id)
        {
            var tinyUrl = company.TinyUrls.FirstOrDefault(t => t.UId == id);
            if (tinyUrl == null)
                return RedirectToAction("Error404", "Error");
            var forms = company.Forms.ToList();
            foreach (var tu in company.TinyUrls)
            {
                if (tu.Form != tinyUrl.Form)
                    continue;
                forms.Remove(tu.Form);
            }
            return View(tinyUrl);
        }

        [HttpPut]
        [ActionName("TinyUrl")]
        public JsonNetResult EditTinyUrl(TinyUrl url)
        {
            var tinyUrl = company.TinyUrls.FirstOrDefault(t => t.UId == url.UId);
            if (tinyUrl == null)
                return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = false, Message = "Invalid object." } };
            var forms = company.Forms.ToList();
            foreach (var tu in company.TinyUrls)
            {
                forms.Remove(tu.Form);
            }
            var formKeys = forms.Select(f => f.UId);
            if (!url.UId.In(formKeys.ToArray()))
                return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = false, Message = "The form is currently in use." } };
            if (Repository.Search<TinyUrl>(t => t.Url.ToLower() == url.Url.ToLower() && t.UId == url.UId).Count() > 0)
                return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = false, Message = "The url is currently in use." } };
            var form = company.Forms.FirstOrDefault(f => f.UId == url.UId);
            if (form == null)
                return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = false, Message = "The form is invalid." } };
            tinyUrl.Url = url.Url;
            tinyUrl.Form = form;
            tinyUrl.Company = company;
            tinyUrl.UId = form.UId;
            Repository.Commit();
            return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = true } };
        }

        [HttpPost]
        [ActionName("TinyUrl")]
        [JsonValidateAntiForgeryToken]
        public JsonNetResult NewTinyUrl(Guid formKey, string url)
        {
            var tinyUrl = new TinyUrl();
            var forms = company.Forms.ToList();
            foreach (var tu in company.TinyUrls)
            {
                forms.Remove(tu.Form);
            }
            var formKeys = forms.Select(f => f.UId);
            if (!formKey.In(formKeys.ToArray()))
                return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = false, Message = "The form is currently in use." } };
            if (Repository.Search<TinyUrl>(t => t.Url.ToLower() == url.ToLower()).Count() > 0)
                return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = false, Message = "The url is currently in use." } };
            var form = company.Forms.FirstOrDefault(f => f.UId == formKey);
            if (form == null)
                return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = false, Message = "The form is invalid." } };
            tinyUrl.Company = company;
            tinyUrl.Form = form;
            tinyUrl.Url = url;
            tinyUrl.UId = form.UId;
            Repository.Add(tinyUrl);
            Repository.Commit();
            return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = true, Location = Url.Action("TiyUrl", "CompanyAdmin", new { id = tinyUrl.UId }, Request.Url.Scheme) } };
        }


        [HttpDelete]
        [ActionName("TinyUrl")]
        [JsonValidateAntiForgeryToken]
        public JsonNetResult DeleteTinyUrl(Guid id)
        {
            var tinyUrl = Repository.Search<TinyUrl>(t => t.UId == id);
            if (tinyUrl == null)
                return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = false, Message = "Invalid object." } };
            Repository.Remove(tinyUrl);
            Repository.Commit();
            return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = true, Location = Url.Action("TinyUrls", "CompanyAdmin", null, Request.Url.Scheme) } };
        }

        #endregion

        protected void SendValidateEmail(User newUser)
        {
            var server = Context.SmtpServers.Where(s => s.Name == "Primary").FirstOrDefault();
            if (server != null)
            {
                var htmlMessage = "";
                if (System.IO.File.Exists(Server.MapPath("~/App_Data/Emails/validateEmail.html")))
                {
                    using (var fileReader = new StreamReader(Server.MapPath("~/App_Data/Emails/validateEmail.html")))
                    {
                        htmlMessage = fileReader.ReadToEnd();
                    }
                }
                var rgxConfirm = new Regex(@"\[ValidateEmail\]", RegexOptions.IgnoreCase);
                htmlMessage = rgxConfirm.Replace(htmlMessage, Url.Action("ConfirmAccount", "Account", new { id = newUser.Id, validationToken = newUser.ValidationToken }, Request.Url.Scheme));
                var message = new MailMessage();
                message.From = new MailAddress("no_reply@regstep.com", "RegStep Technologies");
                message.Subject = "Validate Email";
                message.Body = htmlMessage;
                message.IsBodyHtml = true;
                server.SendAdminEmail(message, new string[] { newUser.Email }, Context);
            }

        }
    }
}