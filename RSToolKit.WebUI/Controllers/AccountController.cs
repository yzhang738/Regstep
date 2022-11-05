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
using System.Threading.Tasks;
using System.Net.Mail;

namespace RSToolKit.WebUI.Controllers
{
    [Authorize]
    public class AccountController : RSController
    {
        public AccountController(EFDbContext context)
            : base(context)
        {
            if (context == null)
                throw new ArgumentException("Database context cannot be null.", "context");
            Context = context;
            iLog.LoggingMethod = "AccountController";
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Context.Dispose();
            }
            base.Dispose(disposing);
        }

        [HttpGet]
        public ActionResult ChangeCompany()
        {
            var list = new List<Company>();
            foreach (var company in Repository.Search<Company>(c => 1 == 1).OrderBy(c => c.Name))
                list.Add(company);
            ViewBag.User = user;
            return View(list);
        }

        public ActionResult ChangeCompany(Guid Company)
        {
            var company = Context.Companies.Where(c => c.UId == Company).FirstOrDefault();
            if (company == null)
                return RedirectToAction("ChangeCompany");
            if (!Repository.CanAccess(company, SecurityAccessType.Read))
                return RedirectToAction("Error403", "Error");
            user.WorkingCompany = company;
            UserManager.Update(user);
            return RedirectToAction("ChangeCompany");
        }

        [HttpGet]
        public ActionResult MyAccount(Guid? id)
        {
            User user;
            if (id.HasValue)
                user = UserManager.FindById(id.Value.ToString());
            else
                user = UserManager.FindById(User.Identity.GetUserId());
            return View(user);
        }

        [HttpPut]
        [ValidateAntiForgeryToken]
        public async Task<JsonNetResult> MyAccount(User model)
        {
            var l_user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            if (l_user.Id != user.Id)
                if (!User.IsInRole("Super Administrators") && !User.IsInRole("System Administrators") && !User.IsInRole("Administrators"))
                    if (!(User.IsInRole("Company Administrators") && l_user.CompanyKey == user.CompanyKey))
                        return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = false, Message = "Insufficient Permissions" } };
            l_user.FirstName = model.FirstName;
            l_user.LastName = model.LastName;
            l_user.PhoneNumber = model.PhoneNumber;
            l_user.Birthdate = model.Birthdate;
            l_user.UTCOffset = model.UTCOffset;
            if (!String.IsNullOrWhiteSpace(Request.Form["nPassword"]) && Request.Form["nPassword"] == Request.Form["cnPassword"])
            {
                var p_result = UserManager.ChangePassword(l_user.Id, Request.Form["oPassword"], Request.Form["nPassword"]);
                if (!p_result.Succeeded)
                    return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = false, Message = String.Join(",", p_result.Errors.ToArray()), Location = "refresh" } };
            }
            if (user.Email != model.Email)
            {
                if (User.IsInRole("Super Administrators"))
                {
                    l_user.Email = model.Email;
                    l_user.EmailConfirmed = true;
                }
                else
                {
                    l_user.Email = model.Email;
                    l_user.ValidationToken = Guid.NewGuid();
                    l_user.EmailConfirmed = false;
                    SendEmailValidateMessage(l_user);
                }
            }
            UserManager.Update(l_user);
            return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = true } };
        }     

        [AllowAnonymous]
        public ActionResult LogOff()
        {
            AuthManager.SignOut();
            return RedirectToAction("Index", "Cloud");
        }

        [AllowAnonymous]
        [HttpGet]
        public ActionResult Login(string returnUrl = "/Home/Index")
        {
            ViewBag.ReturnUrl = returnUrl;
            return View(new LoginModel());
        }

        [AllowAnonymous]
        [HttpPost]
        public ActionResult Login(LoginModel model, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                var user = UserManager.Find(model.Username, model.Password);
                if (user == null)
                {
                    ModelState.AddModelError("", "Invalid name or password.");
                    user = UserManager.FindByName(model.Username);
                    if (user != null)
                    {
                        user.LastPasswordFailureDate = DateTimeOffset.UtcNow;
                        if (user.LastPasswordFailureDate.AddMinutes(5) > DateTimeOffset.UtcNow)
                        {
                            user.PasswordFailuresSinceLastSuccess++;
                            if (user.PasswordFailuresSinceLastSuccess >= 5)
                            {
                                user.IsLocked = true;
                            }
                        }
                        else
                        {
                            user.PasswordFailuresSinceLastSuccess = 1;
                        }
                        UserManager.Update(user);
                    }
                }
                else
                {
                    user.PasswordFailuresSinceLastSuccess = 0;
                    UserManager.Update(user);
                    if (!user.IsConfirmed)
                    {
                        ModelState.AddModelError("", "The user is not confirmed.  Plesase check your email for a confirmation link.");
                    }
                    else if (user.IsLocked)
                    {
                        ModelState.AddModelError("", "The user is currently locked out.  Please contact an administrator.");
                    }
                    else
                    {
                        var ident = UserManager.CreateIdentity(user, DefaultAuthenticationTypes.ApplicationCookie);
                        AuthManager.SignOut();
                        AuthManager.SignIn(new AuthenticationProperties { IsPersistent = model.RememberMe }, ident);
                        if (String.IsNullOrWhiteSpace(returnUrl) || returnUrl == "/")
                            returnUrl = "/Account/MyAccount";
                        return Redirect(returnUrl);
                    }
                }
            }
            ViewBag.ReturnUrl = returnUrl;
            return View(new LoginModel());
        }

        [AllowAnonymous]
        [HttpGet]
        public ActionResult Register()
        {
            return View(new RegisterModel());
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<ActionResult> Register(RegisterModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new User()
                {
                    UserName = model.Username,
                    Email = model.Email
                };
                var result = await UserManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    user = UserManager.Find(model.Username, model.Password);
                    var ident = await UserManager.CreateIdentityAsync(user, DefaultAuthenticationTypes.ApplicationCookie);
                    AuthManager.SignOut();
                    AuthManager.SignIn(new AuthenticationProperties { IsPersistent = false }, ident);
                    return Redirect("/Home/Index");
                }
                else
                {
                    foreach (var e in result.Errors)
                    {
                        ModelState.AddModelError("", e);
                    }
                }
            }
            return View(new RegisterModel());
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult ConfirmEmail(string id, Guid validationToken)
        {
            var u = UserManager.FindById(id);
            if (u == null)
                return RedirectToAction("AccountError", "Error", new string[] { "User Id is invalid." });
            if (u.ValidationToken != validationToken)
                return View("AccountError", new string[] { "validation tokens do not match or is expired." });
            var ident = UserManager.CreateIdentity(u, DefaultAuthenticationTypes.ApplicationCookie);
            u.EmailConfirmed = true;
            u.IsConfirmed = true;
            u.ValidationToken = Guid.NewGuid();
            u.PasswordResetToken = Guid.NewGuid();
            u.PasswordResetTokenExpiration = DateTimeOffset.UtcNow.AddMinutes(30);
            UserManager.Update(u);
            AuthManager.SignOut();
            AuthManager.SignIn(ident);
            TempData.Add("Message", "Email Verrified.  We advise you to change your password now.");
            return RedirectToAction("PasswordReset", new { id = u.Id, token = u.PasswordResetToken });
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult> ConfirmAccount(string id, Guid validationToken)
        {
            var token = await UserManager.ConfirmAccountAsync(id, validationToken);
            if (token == null)
                throw new InvalidDataException("The validation token is invalid.");
            return View("SetPassword", new PasswordResetModel() { Token = token.ToString(), UserId = id });
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult PasswordReset(string id, Guid token)
        {
            if (TempData.ContainsKey("Message"))
                ViewBag.Message = TempData["Message"];
            var u = UserManager.FindById(id);
            if (u == null)
                return RedirectToAction("AccountError", "Error", new string[] { "User Id is invalid." });
            if (u.PasswordResetToken != token || u.PasswordResetTokenExpiration > DateTime.UtcNow)
                return View("AccountError", new string[] { "Password reset tokens do not match or is expired." });
            return View(u);
        }

        [HttpPut]
        [AllowAnonymous]
        [JsonValidateAntiForgeryToken]
        public async Task<JsonNetResult> PasswordReset(PasswordResetModel model)
        {
            if (model.Password != model.ConfirmPassword)
                return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = false, Message = "Passwords do not match." } };
            var result = await UserManager.ResetPasswordAsync(model.UserId, model.Token, model.Password);
            if (result.Succeeded)
            {
                var l_user = UserManager.FindById(model.UserId);
                var ident = UserManager.CreateIdentity(l_user, DefaultAuthenticationTypes.ApplicationCookie);
                AuthManager.SignOut();
                AuthManager.SignIn(ident);
                return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = true, Location = Url.Action("MyAccount", "Account") } };
            }
            return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = result.Succeeded, Message = String.Join(",", result.Errors.ToArray()) } };
        }

        [HttpPost]
        [JsonValidateAntiForgeryToken]
        public async Task<JsonNetResult> ChangePassword(string id, string oPassword, string nPassword, string cnPassword)
        {
            if (nPassword != cnPassword)
                return new JsonNetResult() { JsonRequestBehavior = System.Web.Mvc.JsonRequestBehavior.AllowGet, Data = new { Success = false, Message = "Passwords do not match." } };
            var result = await UserManager.ChangePasswordAsync(id, oPassword, nPassword);
            return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = result.Succeeded, Location = "nothing", Message = String.Join(",", result.Errors.ToArray()) } };
        }

        [HttpGet]
        public ActionResult Index()
        {
            return View();
        }

        private void SendEmailValidateMessage(User newUser)
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
                htmlMessage = rgxConfirm.Replace(htmlMessage, Url.Action("ConfirmEmail", "Account", new { id = newUser.Id, validationToken = newUser.ValidationToken }, Request.Url.Scheme));
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