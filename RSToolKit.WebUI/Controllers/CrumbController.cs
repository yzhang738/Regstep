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
using System.IO;
using System.Net.Mail;
using System.Text.RegularExpressions;
using RSToolKit.Domain;
using RSToolKit.Domain.Entities;
using RSToolKit.Domain.Entities.Components;
using RSToolKit.Domain.Logging;
using RSToolKit.Domain.JItems;
using RSToolKit.Domain.Engines;
using System.Threading.Tasks;
using RSToolKit.Domain.Exceptions;
using RSToolKit.WebUI.Infrastructure;
using Newtonsoft.Json;

namespace RSToolKit.WebUI.Controllers
{
    /// <summary>
    /// Handles crumb attributes.
    /// </summary>
    [Authorize]
    public class CrumbController
        : Controller
    {
        private AppUserManager _userManager;

        /// <summary>
        /// The <code>User</code> that is authenticated.
        /// </summary>
        public User AppUser { get; set; }
        /// <summary>
        /// The <code>EFDbContext</code> to be used.
        /// </summary>
        public EFDbContext Context { get; set; }
        /// <summary>
        /// Initializes the controller with a new <code>EFDbContext</code>.
        /// </summary>
        public CrumbController()
        {
            Context = new EFDbContext();
        }
        /// <summary>
        /// The User Manager for the application.
        /// </summary>
        public AppUserManager UserManager
        {
            get
            {
                if (this._userManager == null)
                    this._userManager = new AppUserManager(new UserStore<User>((EFDbContext)Context));
                return this._userManager;
            }
        }

        /// <summary>
        /// Disposes of the <code>Context</code>;
        /// </summary>
        /// <param name="disposing">If it is disposing.</param>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            Context.Dispose();
        }

        [HttpGet]
        [IsAjax]
        public JsonNetResult Index(Guid id)
        {
            var crumb = AppUser.BreadCrumbs.FirstOrDefault(c => c.Id == id);
            if (crumb == null)
                throw new InvalidIdException("The id does no pertain to a current crumb.");
            return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { success = true, crumb = JsonConvert.SerializeObject(crumb) } };
        }

        [HttpGet]
        [IsAjax]
        public JsonNetResult Get(Guid id)
        {
            return Index(id);
        }

        [HttpPost]
        [IsAjax]
        public JsonNetResult Post(Crumb crumb)
        {
            crumb.Id = Guid.NewGuid();
            AppUser.BreadCrumbs.Enqueue(crumb);
            return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { success = true, crumb = JsonConvert.SerializeObject(crumb) } };
        }

        [HttpPut]
        [IsAjax]
        public JsonNetResult Put(Crumb crumb)
        {
            var t_crumb = AppUser.BreadCrumbs.FirstOrDefault(c => c.Id == crumb.Id);
            if (t_crumb == null)
                throw new InvalidIdException("The crumb was not found in the user memory.");
            t_crumb.Label = crumb.Label;
            t_crumb.Parameters = crumb.Parameters;
            t_crumb.ActionDate = crumb.ActionDate;
            t_crumb.Action = crumb.Action;
            t_crumb.Controller = crumb.Controller;
            Context.SaveChanges();
            return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { success = true, crumb = t_crumb } };
        }

        #region Suporting Methods

        /// <summary>
        /// Performs some task before the execution of the action happens.
        /// The AppUser is set here.
        /// </summary>
        /// <param name="filterContext">the context used when authenticating.</param>
        protected override void OnAuthorization(AuthorizationContext filterContext)
        {
            base.OnAuthorization(filterContext);
            if (User.Identity.IsAuthenticated)
            {
                // The user is authenticated. We need to set the AppUser property with the authenticated user.
                AppUser = UserManager.FindById(User.Identity.GetUserId());
                ViewBag.User = AppUser;
                if (AppUser != null)
                    // We add the user to the context so permissions will execute properly.
                    Context.SecuritySettings.SetUser(AppUser, User);
            }
        }

        /// <summary>
        /// Saves the <code>Context</code> changes into the database.
        /// </summary>
        /// <param name="filterContext">The filter context.</param>
        protected override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            base.OnActionExecuted(filterContext);
            Context.SaveChanges();
        }

        /// <summary>
        /// Catches exceptions that where not handled by exception filters and logs them to the database.
        /// </summary>
        /// <param name="filterContext">The filter context that contains the exception.</param>
        protected override void OnException(ExceptionContext filterContext)
        {
            if (filterContext.ExceptionHandled)
                return;
            filterContext.ExceptionHandled = true;
            var logger = new DataLogger();
            var exc = filterContext.Exception;
            if (filterContext.HttpContext.User.Identity.IsAuthenticated)
                logger.UserId = filterContext.HttpContext.User.Identity.GetUserId();
            var log = logger.Error(exc);



            //exc.LoggedId = log.SortingId;
            // if the request is AJAX return JSON else view.
            if (filterContext.HttpContext.Request.IsAjaxRequest())
            {
                //Because its a exception raised after ajax invocation
                //Lets return a Json.Net object
                filterContext.HttpContext.Response.Clear();
                filterContext.Result = new JsonNetResult()
                {
                    Data = filterContext.Exception.Message,
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                };
                filterContext.HttpContext.Response.StatusCode = 500;
                filterContext.ExceptionHandled = true;
            }
            else
            {
                var result = CreateErrorActionResult(filterContext);
                filterContext.Result = result;

                // Prepare the response code.
                filterContext.ExceptionHandled = true;
                filterContext.HttpContext.Response.Clear();
                filterContext.HttpContext.Response.StatusCode = 500;
                filterContext.HttpContext.Response.TrySkipIisCustomErrors = true;
            }
        }

        /// <summary>
        /// Creates the <code>ActionResult</code> for an error.
        /// </summary>
        /// <param name="filterContext">The filter context.</param>
        /// <returns>An <code>ActionResult</code> with the error view.</returns>
        protected virtual ActionResult CreateErrorActionResult(ExceptionContext filterContext)
        {
            var view = "~/Views/Error/Exception.cshtml";
            if (filterContext.Exception is RegStepException)
                view = "~/Views/Error/RegStepException.cshtml";
            var ctx = new ControllerContext(filterContext.RequestContext, filterContext.Controller);
            var viewName = SelectFirstView(ctx,
                                            view);

            var controllerName = (string)filterContext.RouteData.Values["controller"];
            var actionName = (string)filterContext.RouteData.Values["action"];
            var model = new HandleErrorInfo(filterContext.Exception, controllerName, actionName);
            var result = new ViewResult
            {
                ViewName = viewName,
                ViewData = new ViewDataDictionary<HandleErrorInfo>(model),
            };
            return result;
        }

        /// <summary>
        /// Selects the first view it can find from the desired view names.
        /// </summary>
        /// <param name="ctx">The controller context.</param>
        /// <param name="viewNames">The view names to look through.</param>
        /// <returns>The string of the view.</returns>
        protected string SelectFirstView(ControllerContext ctx, params string[] viewNames)
        {
            return viewNames.First(view => ViewExists(ctx, view));
        }

        /// <summary>
        /// Checks to see if the view exists.
        /// </summary>
        /// <param name="ctx">The controller context.</param>
        /// <param name="name">The name of the view.</param>
        /// <returns><code>True</code> if the view exists, <code>False</code> otherwise.</returns>
        protected bool ViewExists(ControllerContext ctx, string name)
        {
            var result = ViewEngines.Engines.FindView(ctx, name, null);
            return result.View != null;
        }

        #endregion
    }
}