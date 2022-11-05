using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RSToolKit.WebUI.Infrastructure;
using RSToolKit.Domain.Data;
using RSToolKit.Domain.Security;
using RSToolKit.Domain.Entities;
using RSToolKit.WebUI.Models;
using Microsoft.AspNet.Identity;
using RSToolKit.Domain.Entities.Components;
using RSToolKit.Domain.Entities.Clients;
using RSToolKit.Domain.Entities;
using RSToolKit.Domain.Entities.Email;
using System.Reflection;

namespace RSToolKit.WebUI.Controllers
{
    [Authorize(Roles = "Super Administrators,Administrators,Programmers")]
    public class SecurityController : RSController
    {

        protected IProtected _item;

        public SecurityController(EFDbContext context)
            : base(context)
        {
            iLog.LoggingMethod = "SecurityController";
        }

        public SecurityController()
            : base()
        {
            iLog.LoggingMethod = "SecurityController";
        }

        // GET: Security
        public ActionResult Index()
        {
            return View();
        }

        [Authorize(Roles = "Cloud Users,Cloud+ Users,Email Builders,Form Builders,Super Administrators,Administrators,Programmers,Company Administrators")]
        [HttpGet]
        [ActionName("Permissions")]
        public ActionResult EditPermissions(Guid id)
        {
            var node = Repository.Search<INamedNode>(n => n.UId == id).FirstOrDefault();
            if (node == null)
                return RedirectToAction("Error404", "Error");
            var permissions = Repository.Search<PermissionSet>(p => p.Target == id);
            var model = new PermissionsModel();
            model.Name = node.Name;
            model.UId = id;
            foreach (var permission in permissions.ToList())
            {
                var set = new PermissionSetModel()
                {
                    Target = permission.Target,
                    Owner = permission.Owner,
                    Read = permission.Read,
                    Write = permission.Write,
                    Execute = permission.Execute
                };
                if (set.Owner == Guid.Empty)
                {
                    set.OwnerName = "Anonymous";
                    model.Permissions.Add(set);
                }
                else if (set.Owner == company.UId)
                {
                    set.OwnerName = company.Name + "[company]";
                    model.Permissions.Add(set);
                }
                else
                {
                    var group = Repository.Search<CustomGroup>(g => g.UId == set.Owner).FirstOrDefault();
                    if (group == null)
                        continue;
                    set.OwnerName = group.Name;
                    model.Permissions.Add(set);
                }
            }
            ViewBag.Name = node.Name;
            ViewBag.UId = id;
            string location = "";
            #region Grab Location

            //FORMBUILDER
            if (node is Form)
                location = Url.Action("Form", "FormBuilder", new { id = id }, Request.Url.Scheme);
            else if (node is Page)
                location = Url.Action("Page", "FormBuilder", new { id = id }, Request.Url.Scheme);
            else if (node is Panel)
                location = Url.Action("Panel", "FormBuilder", new { id = id }, Request.Url.Scheme);
            else if (node is Component)
                location = Url.Action("Component", "FormBuilder", new { id = id }, Request.Url.Scheme);
            else if (node is Logic)
                location = Url.Action("Logic", "FormBuilder", new { id = id }, Request.Url.Scheme);
            else if (node is CustomText)
                location = Url.Action("ContentBlock", "FormBuilder", new { id = id }, Request.Url.Scheme);
            else if (node is LogicBlock)
                location = Url.Action("ContentLogic", "FormBuilder", new { id = id }, Request.Url.Scheme);
            else if (node is PriceGroup)
                location = Url.Action("Prices", "FormBuilder", new { id = id }, Request.Url.Scheme);
            //CLOUD
            else if (node is Contact)
                location = Url.Action("Contact", "FormBuilder", new { id = id }, Request.Url.Scheme);
            else if (node is SavedList)
                location = Url.Action("SavedList", "FormBuilder", new { id = id }, Request.Url.Scheme);
            else if (node is ContactReport)
                location = Url.Action("ContactReport", "FormBuilder", new { id = id }, Request.Url.Scheme);
            else if (node is SingleFormReport)
                location = Url.Action("SingleFormReport", "FormBuilder", new { id = id }, Request.Url.Scheme);
            //EMAIL
            else if (node is RSEmail)
                location = Url.Action("Email", "Email", new { id = id }, Request.Url.Scheme);
            else if (node is EmailCampaign)
                location = Url.Action("EmailCampaign", "Email", new { id = id }, Request.Url.Scheme);
            model.Location = location;

            #endregion
            ViewBag.CompanyObj = company;
            return View("EditPermissions", model);
        }

        [Authorize(Roles = "Cloud Users,Cloud+ Users,Email Builders,Form Builders,Super Administrators,Administrators,Programmers,Company Administrators")]
        [HttpPut]
        [ActionName("PermissionSet")]
        [JsonValidateAntiForgeryToken]
        public JsonNetResult PutPermissionSet(PermissionSetModel model)
        {
            var node = Repository.Search<INode>(n => n.UId == model.Target).FirstOrDefault();
            if (node == null)
                return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = false, Message = "Invalid Object" } };
            var pSet = Repository.Search<PermissionSet>(p => p.Target == model.Target && p.Owner == model.Owner).FirstOrDefault();
            if (pSet == null)
                pSet = PermissionSet.New(Repository, model.Target, model.Owner, model.Read, model.Write, model.Execute);
            else
            {
                pSet.Read = model.Read;
                pSet.Write = model.Write;
                pSet.Execute = model.Execute;
                Repository.Commit();
            }
            return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = true, Location = Url.Action("Permissions", new { id = model.Target }) } };
        }

        [Authorize(Roles = "Cloud Users,Cloud+ Users,Email Builders,Form Builders,Super Administrators,Administrators,Programmers,Company Administrators")]
        [HttpDelete]
        [ActionName("PermissionSet")]
        [JsonValidateAntiForgeryToken]
        public JsonNetResult DeletePermissionSet(PermissionSetModel model)
        {
            var node = Repository.Search<INode>(n => n.UId == model.Target).FirstOrDefault();
            var pSet = Repository.Search<PermissionSet>(p => p.Target == model.Target && p.Owner == model.Owner).FirstOrDefault();
            if (pSet == null)
                return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = false, Message = "Invalid Object" } };
            Repository.Remove(pSet);
            Repository.Commit();
            return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = true, Location = Url.Action("Permissions", new { id = model.Target }) } };
        }

    }
}