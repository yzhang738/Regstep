using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin.Security;
using RSToolKit.Domain;
using RSToolKit.Domain.Data;
using RSToolKit.Domain.Entities.Clients;
using RSToolKit.Domain.Entities.Navigation;
using RSToolKit.Domain.Exceptions;
using RSToolKit.Domain.Logging;
using RSToolKit.WebUI.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace RSToolKit.WebUI.Infrastructure
{
    /// <summary>
    /// A base controller to use in the application.
    /// </summary>
    public class RegStepController
        : Controller, IRegStepController
    {

        #region Properties and Variables
        
        protected static Regex _invoiceName = new Regex(@"\[inv\s*=>\s*invoicedisplayname\]", RegexOptions.IgnoreCase);
        protected static Regex _invoiceCharge = new Regex(@"\[inv\s*=>\s*charge\]", RegexOptions.IgnoreCase);
        protected static Regex _invoiceLastFour = new Regex(@"\[inv\s*=>\s*lastfour\]", RegexOptions.IgnoreCase);

        private AppUserManager _userManager;
        private AppRoleManager _roleManager;

        protected EFDbContext _trailContext;
        protected UserTrail _userTrail;

        /// <summary>
        /// Is going to create a breadcrumb.
        /// </summary>
        public bool NoTrail { get; set; }
        /// <summary>
        /// Is being rendered as a popout window.
        /// </summary>
        public bool IsPopout { get; set; }
        /// <summary>
        /// Is being rendered as a minimal view.
        /// </summary>
        public bool IsMinimal { get; set; }
        /// <summary>
        /// The Logging method used in the controller.
        /// </summary>
        public ILogger Log = null;
        /// <summary>
        /// The current user logged in.
        /// </summary>
        public User AppUser { get; protected set; }
        /// <summary>
        /// The context being used.
        /// </summary>
        public EFDbContext Context;
        /// <summary>
        /// The company the authenticated user is working under.
        /// </summary>
        public Company WorkingCompany { get; set; }
        /// <summary>
        /// A list of repositories being used.
        /// </summary>
        public List<IRepository> Repositories { get; set; }
        /// <summary>
        /// The User Manager for the application.
        /// </summary>
        public AppUserManager UserManager
        {
            get
            {
                if (_userManager == null)
                    _userManager = new AppUserManager(new UserStore<User>(Context));
                return _userManager;
            }
        }
        /// <summary>
        /// The Authentication Manager for the application.
        /// </summary>
        protected IAuthenticationManager AuthManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }
        /// <summary>
        /// The Role Manager for the application.
        /// </summary>
        public AppRoleManager RoleManager
        {
            get
            {
                if (_roleManager == null)
                    _roleManager = new AppRoleManager(new RoleStore<AppRole>(Context));
                return _roleManager;
            }
        }
        /// <summary>
        /// The trail being used.
        /// </summary>
        public Trail<Pheromone> Trail { get; set; }
        /// <summary>
        /// The <code>Pheromone</code> that is being used.
        /// </summary>
        public Pheromone Pheromone { get; set; }
        #endregion

        #region Constructors

        /// <summary>
        /// Initializes the controller with a context.
        /// </summary>
        /// <param name="context">The context the controller should use.</param>
        public RegStepController(EFDbContext context)
        {
            Context = context;
            Log = new DataLogger()
            {
                LoggingMethod = "RegStepController",
                Thread = "Main"
            };
            this._userManager = null;
            this._roleManager = null;
            Repositories = new List<IRepository>();
            IsMinimal = false;
            IsPopout = false;
            WorkingCompany = null;
            NoTrail = false;
            this._trailContext = new EFDbContext();
            this._userTrail = null;
            Pheromone = null;
            ViewBag.Context = Context;
        }

        /// <summary>
        /// Initializes the controller.
        /// </summary>
        public RegStepController()
        {
            Context = new EFDbContext();
            Log = new DataLogger()
            {
                LoggingMethod = "RegStepController",
                Thread = "Main"
            };
            this._userManager = null;
            this._roleManager = null;
            Repositories = new List<IRepository>();
            IsMinimal = false;
            IsPopout = false;
            WorkingCompany = null;
            NoTrail = false;
            this._trailContext = new EFDbContext();
            this._userTrail = null;
            Pheromone = null;
            ViewBag.Context = Context;
        }

        #endregion

        #region Supporting Methods

        /// <summary>
        /// Performs some task before the execution of the action happens.
        /// The AppUser is set here.
        /// Sets the following <code>ViewBag</code> properties.
        /// <code>ViewBag.CompanyId</code> holds the <code>long</code> SortingId of the working company for the user, or -1 if no user is authenticated.
        /// <code>ViewBag.Company</code> holds the <code>Company</code> class of the working company for the user, or null if no user is authenticated.
        /// <code>ViewBag.User</code> holds the <code>User</code> of the authenticated user or null if no user is authenticated.
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
                {
                    // We add the user to the context so permissions will execute properly.
                    Context.SecuritySettings.SetUser(AppUser, User);
                    ViewBag.Company = AppUser.WorkingCompany;
                    ViewBag.CompanyId = AppUser.WorkingCompany != null ? AppUser.WorkingCompany.SortingId : -1;
                    WorkingCompany = AppUser.WorkingCompany ?? AppUser.Company;
                }
                this._userTrail = this._trailContext.UserTrails.FirstOrDefault(t => t.UserId == AppUser.Id);
                if (this._userTrail == null)
                {
                    this._userTrail = new UserTrail() { UserId = AppUser.Id };
                    this._trailContext.UserTrails.Add(this._userTrail);
                    this._trailContext.SaveChanges();
                }
                Trail = new Trail<Pheromone>(this._userTrail.GetTrail<Pheromone>(), 10);
                ViewBag.Trail = Trail;
            }

        }

        /// <summary>
        /// Sets the current crumb.
        /// </summary>
        /// <param name="filterContext">The filter context to be used.</param>
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);
            if (User.Identity.IsAuthenticated && AppUser != null)
            {
                // Now we run through the bread crumbs.
                #region Trail
                if (!NoTrail)
                {
                    // We need to grab the controller and action.
                    var controller = filterContext.RouteData.Values["controller"].ToString().ToLower();
                    var action = filterContext.RouteData.Values["action"].ToString().ToLower();
                    var parameters = new Dictionary<string, string>();
                    foreach (var kvp in filterContext.RouteData.Values.Where(k => !k.Key.ToLower().In("controller", "action")))
                        parameters.Add(kvp.Key, kvp.Value.ToString());
                    foreach (var key in filterContext.HttpContext.Request.QueryString.AllKeys)
                        if (!parameters.ContainsKey(key))
                            parameters.Add(key, filterContext.HttpContext.Request.QueryString[key].ToString());
                    if (IsPopout && parameters.ContainsKey("popout"))
                        parameters.Add("popout", "True");
                    if (IsMinimal && parameters.ContainsKey("minimal"))
                        parameters.Add("minimal", "True");
                    // First we check to see if we are tracking this as a breadcrumb.
                    var ignoreTrail = NoTrail ||
                        filterContext.HttpContext.Request.IsAjaxRequest() ||
                        Request.HttpMethod.ToUpper() != "GET" ||
                        IsPopout ||
                        (filterContext.HttpContext.Request.QueryString.AllKeys.Contains("ignoretrail") && filterContext.HttpContext.Request.QueryString["ignoretrail"].ToLower() == "true") ||
                        filterContext.HttpContext.Request.QueryString.AllKeys.Contains("trailid");
                    var lastTrailItem = Trail.PeekTail();
                    var sameTrailItem = true;
                    // This section of code ignores the trail if it was the exact same as the last one.
                    if (lastTrailItem != null && lastTrailItem.Controller == controller && lastTrailItem.Action == action)
                    {
                        if (lastTrailItem.Parameters.Count != parameters.Count)
                        {
                            sameTrailItem = false;
                        }
                        else
                        {
                            foreach (var kvp in lastTrailItem.Parameters)
                            {
                                if (!parameters.ContainsKey(kvp.Key))
                                {
                                    sameTrailItem = false;
                                    break;
                                }
                                if (parameters[kvp.Key] != lastTrailItem.Parameters[kvp.Key])
                                {
                                    sameTrailItem = false;
                                    break;
                                }
                            }
                        }
                        if (sameTrailItem)
                        {
                            Pheromone = lastTrailItem;
                            ignoreTrail = true;
                        }
                    }
                    if (!ignoreTrail)
                    {
                        var pheromone = new Pheromone()
                        {
                            Action = action,
                            Controller = controller,
                            ActionDate = DateTimeOffset.Now,
                            Id = Guid.NewGuid(),
                            Label = action,
                            Parameters = parameters
                        };
                        Trail.Push(pheromone);
                        Pheromone = pheromone;
                        this._userTrail.SetTrail(Trail);
                        this._trailContext.SaveChanges();
                    }
                    ViewBag.Pheromone = Pheromone;
                }
                #endregion
            }
        }

        /// <summary>
        /// Sets the IsPopout and IsMinimal in ViewBag.
        /// </summary>
        /// <param name="filterContext">The context.</param>
        protected override void OnResultExecuting(ResultExecutingContext filterContext)
        {
            base.OnResultExecuting(filterContext);
            ViewBag.IsPopout = IsPopout;
            ViewBag.IsMinimal = IsMinimal;
        }

        /// <summary>
        /// Closing actions.
        /// </summary>
        /// <param name="filterContext"></param>
        protected override void OnResultExecuted(ResultExecutedContext filterContext)
        {
            // Here we need to make sure the top 5 used forms are still loaded into memory.
            Task.Factory.StartNew(() =>
            {
                LoadTop5FormReports();
            });
        }

        /// <summary>
        /// Disposes of items that are in need of disposing.
        /// </summary>
        /// <param name="disposing">True if disposing is taking place.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (Context != null)
                    Context.Dispose();
                if (this._trailContext != null)
                    this._trailContext.Dispose();
            }
            base.Dispose(disposing);
        }

        /// <summary>
        /// Catches exceptions that where not handled by exception filters and logs them to the database.
        /// </summary>
        /// <param name="filterContext">The filter context that contains the exception.</param>
        protected override void OnException(ExceptionContext filterContext)
        {
            if (Pheromone != null)
            {
                Trail.Rip();
                UpdateTrail();
            }
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

        /// <summary>
        /// Encodes the string for html.  This is mainly used for encoding a string for an html that is not used in a view.
        /// </summary>
        /// <param name="html">The html that needs to be encoded.</param>
        /// <returns>Returns a string with html encoded escapse characters.</returns>
        protected string EncodeHtml(string html)
        {
            return HttpUtility.HtmlEncode(html);
        }

        /// <summary>
        /// Loads the top 5 most used form reports and attach them to <code>FormReport</code>.
        /// </summary>
        protected void LoadTop5FormReports()
        {
            /*
            List<Guid> top5 = new List<Guid>();
            using (var context = new EFDbContext())
            {
                var ttop5 = context.Counters.Where(c => c.Type == CountType.FormReport).OrderBy(c => c.Count).Take(5).Select(c => c.Key).ToList();
                foreach (var t in ttop5)
                {
                    if (context.TokenData.Where(d => d.DataKey == t).Count() > 0)
                        continue;
                    top5.Add(t);
                }
            }
            foreach (var key in top5)
                ThreadPool.QueueUserWorkItem(FormReportController.LoadDataCallback, key);
            //*/
        }

        /// <summary>
        /// Updates the trail for the user.
        /// </summary>
        public void UpdateTrail()
        {
            this._userTrail.SetTrail(Trail);
            this._trailContext.SaveChanges();
        }

        /// <summary>
        /// Updates the <code>Pheromone</code> label.
        /// </summary>
        /// <param name="label">The label to use.</param>
        public void UpdateTrailLabel(string label)
        {
            if (Pheromone != null)
            {
                Pheromone.Label = label;
                UpdateTrail();
            }
        }

        #endregion

    }
}