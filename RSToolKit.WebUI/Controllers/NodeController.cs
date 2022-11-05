using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RSToolKit.WebUI.Infrastructure;
using RSToolKit.Domain.Data;
using RSToolKit.Domain.Entities;
using RSToolKit.Domain.Entities;
using RSToolKit.Domain.Entities.Email;
using RSToolKit.Domain.Entities.Clients;

namespace RSToolKit.WebUI.Controllers
{
    public class NodeController : RSController
    {
        private static Dictionary<Type, ACS> p_editors = new Dictionary<Type, ACS>()
        {
            { typeof(Form), new ACS() { Action = "Form", Controller = "FormBuilder" } },
            { typeof(RSEmail), new ACS() { Action = "Email", Controller = "EmailBuilder" } },
            { typeof(ContactReport), new ACS() { Action = "ContactList", Controller = "Cloud" } },
            { typeof(EmailCampaign), new ACS() { Action = "EmailCampaign", Controller = "EmailBuilder" } },
            { typeof(SavedList), new ACS() { Action = "SavedList", Controller = "Cloud" } },
            { typeof(Folder), new ACS() { Action = "Folder", Controller = "Folder" } }
        };


        public NodeController(EFDbContext context)
            : base(context)
        {

        }

        // GET: Node
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public ActionResult Node(Guid id)
        {
            var node = Repository.Search<INode>(n => n.UId == id).FirstOrDefault();
            if (node == null)
                return RedirectToAction("Error404", "Error");
            if (!p_editors.ContainsKey(node.GetType().BaseType))
                return RedirectToAction("Error404", "Error");
            var editor = p_editors[node.GetType().BaseType];
            return RedirectToAction(editor.Action, editor.Controller, new { id = node.UId });
        }

        [HttpPost]
        [ActionName("Copy")]
        [JsonValidateAntiForgeryToken]
        public JsonNetResult Copy(Guid key, Guid? companyKey = null, string name = null)
        {
            var node = Repository.Search<ICopyable>(f => f.UId == key).FirstOrDefault();
            var company = node.Company;
            if (companyKey.HasValue)
                company = Repository.Search<Company>(c => c.UId == companyKey).FirstOrDefault();
            if (company == null)
                company = node.Company;
            if (node == null)
                return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = false, Message = "Invalid object." } };
            var newNode = node.DeepCopy(name, company, user, Repository);
            Repository.Commit();
            return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = true, Location = Url.Action("Node", "Node", new { id = newNode.UId }, Request.Url.Scheme) } };
        }

        [HttpDelete]
        [ActionName("Node")]
        [JsonValidateAntiForgeryToken]
        public ActionResult DeleteNode(Guid id)
        {
            var node = Repository.Search<INode>(n => n.UId == id).FirstOrDefault();
            if (node == null)
                return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = false, Message = "Invalid Object" } };
            if (!p_editors.ContainsKey(node.GetType().BaseType))
                return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { Success = false, Message = "Invalid Object" } };
            var editor = p_editors[node.GetType().BaseType];
            return RedirectToAction(editor.Action, editor.Controller, new { id = node.UId });
        }

        private class ACS
        {
            public string Action { get; set; }
            public string Controller { get; set; }
        }
    }
}