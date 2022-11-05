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
using RSToolKit.Domain.Entities.Email;
using RSToolKit.Domain.Exceptions;
using System.Reflection;

namespace RSToolKit.WebUI.Controllers
{
    [Authorize(Roles = "Super Administrators,Administrators,Programmers")]
    public class PermissionController
        : RegStepController
    {

        protected IProtected _item;

        public PermissionController()
            : base()
        {
            Log.LoggingMethod = "PermissionController";
        }

        // GET: Security
        public ActionResult Index()
        {
            return View();
        }

        [Authorize(Roles = "Cloud Users,Cloud+ Users,Email Builders,Form Builders,Super Administrators,Administrators,Programmers,Company Administrators")]
        [HttpGet]
        public ActionResult Get(Guid id, string type)
        {
            var location = this._SetItem(id, type);
            var permissions = Context.PermissionsSets.Where(p => p.Target == id).ToList();
            var model = new PermissionsModel()
            {
                Name = this._item.Name,
                UId = this._item.UId,
                Type = type
            };
            foreach (var permission in permissions.ToList())
            {
                var set = new PermissionSetModel()
                {
                    Target = permission.Target,
                    Owner = permission.Owner,
                    Read = permission.Read,
                    Write = permission.Write,
                    Execute = permission.Execute,
                    Type = type
                };
                if (set.Owner == Guid.Empty)
                {
                    set.OwnerName = "Anonymous";
                    model.Permissions.Add(set);
                }
                else if (set.Owner == WorkingCompany.UId)
                {
                    set.OwnerName = WorkingCompany.Name + "[company]";
                    model.Permissions.Add(set);
                }
                else
                {
                    var group = Context.CustomGroups.Where(g => g.UId == set.Owner).FirstOrDefault();
                    if (group == null)
                        continue;
                    set.OwnerName = group.Name;
                    model.Permissions.Add(set);
                }
            }
            ViewBag.Name = this._item.Name;
            ViewBag.UId = id;
            #region Grab Location

            model.Location = location;

            #endregion
            ViewBag.CompanyObj = WorkingCompany;
            return View("EditPermissions", model);
        }

        [Authorize(Roles = "Cloud Users,Cloud+ Users,Email Builders,Form Builders,Super Administrators,Administrators,Programmers,Company Administrators")]
        [HttpPut]
        [JsonValidateAntiForgeryToken]
        public JsonNetResult Set(PermissionSetModel model)
        {
            this._SetItem(model.Target, model.Type);
            var pSet = Context.PermissionsSets.Where(p => p.Target == model.Target && p.Owner == model.Owner).FirstOrDefault();
            if (pSet == null)
            {
                pSet = new PermissionSet()
                {
                    Target = model.Target,
                    Owner = model.Owner,
                    Read = model.Read,
                    Write = model.Write,
                    Execute = model.Execute
                };
                Context.PermissionsSets.Add(pSet);
            }
            else
            {
                pSet.Read = model.Read;
                pSet.Write = model.Write;
                pSet.Execute = model.Execute;
            }
            Context.SaveChanges();
            return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = true, Location = Url.Action("Permissions", new { id = model.Target }) } };
        }

        [Authorize(Roles = "Cloud Users,Cloud+ Users,Email Builders,Form Builders,Super Administrators,Administrators,Programmers,Company Administrators")]
        [HttpDelete]
        [JsonValidateAntiForgeryToken]
        public JsonNetResult Delete(PermissionSetModel model)
        {
            this._SetItem(model.Target, model.Type);
            var pSet = Context.PermissionsSets.Where(p => p.Target == model.Target && p.Owner == model.Owner).FirstOrDefault();
            if (pSet == null)
                return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = false, Message = "Invalid Object" } };
            Context.PermissionsSets.Remove(pSet);
            Context.SaveChanges();
            return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = true, Location = Url.Action("Permissions", new { id = model.Target }) } };
        }

        protected string _SetItem(Guid id, string type)
        {
            var location = "";
            this._item = null;
            switch (type.ToLower())
            {
                case "globalreport":
                    var gReport = Context.GlobalReports.FirstOrDefault(r => r.UId == id);
                    this._item = gReport;
                    if (gReport != null)
                        location = Url.Action("Get", "GlobalReport", new { id = gReport.Id }, Request.Url.Scheme);
                    break;
            }
            if (this._item == null)
                throw new RegStepException("IProtected item not found.");
            if (!this._item.GetPermission(Context.SecuritySettings, SecurityAccessType.Read).HasAccess(SecurityAccessType.Read))
                throw new InsufficientPermissionsException();

            return location;
        }


    }
}