using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RSToolKit.Domain.Data;
using RSToolKit.Domain.Entities;
using RSToolKit.Domain.Security;
using RSToolKit.WebUI.Infrastructure;
using System.Text.RegularExpressions;

namespace RSToolKit.WebUI.Controllers
{
    public class FolderController : RSController
    {
        // GET: Folder
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ActionName("Folder")]
        [JsonValidateAntiForgeryToken]
        public JsonNetResult NewFolder(Guid? parent = null, string name = "New Folder")
        {
            Folder parentFolder = null;
            if (parent.HasValue)
                parentFolder = Repository.Search<Folder>(f => f.CompanyKey == company.UId && f.UId == parent.Value).FirstOrDefault();
            var folder = Folder.New(Repository, company, user, name, parentFolder);
            if (folder == null)
                return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = false, Message = "Unhandled Error" } };
            return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = true, Location = Url.Action("Folder", "Folder", new { id = folder.UId }, Request.Url.Scheme), Id = folder.UId } };
        }

        [HttpDelete]
        [JsonValidateAntiForgeryToken]
        [ActionName("Folder")]
        public JsonNetResult DeleteFolder(Guid id)
        {
            var folder = Repository.Search<Folder>(f => f.UId == id && f.CompanyKey == company.UId).FirstOrDefault();
            if (folder == null)
                return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = false, Message = "Invalid Object" } };
            // Now we need to iterate through all the items and folders and see if the user has permission to delete each item
            Repository.Remove(folder);
            Repository.Commit();
            return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = true } };
        }

        [HttpPut]
        [JsonValidateAntiForgeryToken]
        [ActionName("Folder")]
        public JsonNetResult EditFolder(Guid id, string name)
        {
            var folder = Repository.Search<Folder>(f => f.UId == id && f.CompanyKey == company.UId).FirstOrDefault();
            if (folder == null)
                return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = false, Message = "Invalid Object" } };
            if (String.IsNullOrWhiteSpace(name) || Regex.IsMatch(name, @"[^a-zA-Z0-9-_ :]"))
                return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = false, Message = "Invalid Name" } };
            folder.Name = name;
            Repository.Commit();
            return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = true } };
        }

        [HttpPut]
        [ActionName("Move")]
        [JsonValidateAntiForgeryToken]
        public JsonNetResult Move(Guid id, Guid targetFolder)
        {
            var pointer = Repository.Search<Pointer>(p => p.Target == id).FirstOrDefault();
            var node = Repository.Search<IPointerTarget>(n => n.UId == id).FirstOrDefault();
            var n_folder = Repository.Search<Folder>(f => f.UId == targetFolder).FirstOrDefault();
            if (pointer == null  && !(node is Folder))
                return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = false, Messasge = "Pointer does not exist." } };
            if (n_folder == null)
                return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = false, Message = "Folder does not exist." } };
            if (id == targetFolder)
                return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = false, Message = "Circle reference found." } };
            if (pointer == null)
            {
                var c_folder = (Folder)node;
                c_folder.Parent = n_folder;
                c_folder.ParentKey = n_folder.UId;
            }
            else
            {
                pointer.Folder = n_folder;
            }
            Repository.Commit();
            return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = true, Message = "Success", FolderName = n_folder.Name } };
        }
    }
}