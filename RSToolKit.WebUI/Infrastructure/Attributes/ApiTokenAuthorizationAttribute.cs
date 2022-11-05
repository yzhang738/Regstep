using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Helpers;
using System.Web.ApplicationServices;
using RSToolKit.Domain.Entities.Clients;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Security.Principal;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using System.Web.Http.Properties;
using Newtonsoft.Json;
using RSToolKit.Domain.Data;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity;
using RSToolKit.WebUI.Infrastructure;

namespace RSToolKit.WebUI.Infrastructure
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited = true, AllowMultiple = true)]
    public class ApiTokenAttendeeAuthorizationAttribute
        : AuthorizationFilterAttribute
    {
        private readonly object _typeId = new object();
        
        /// <summary>
        /// Gets a unique identifier for this <see cref="T:System.Attribute"/>.
        /// </summary>
        /// <returns>The unique identifier for the attribute.</returns>
        public override object TypeId
        {
            get { return _typeId; }
        }

        /// <summary>
        /// Determines whether access for this particular request is authorized.
        /// </summary>
        /// <param name="actionContext">The context.</param>
        /// <returns><c>true</c> if access is authorized; otherwise <c>false</c>.</returns>
        protected virtual bool IsAuthorized(HttpActionContext actionContext)
        {
            // Lets try to authenticate
            var token = actionContext.ControllerContext.Request.Headers.GetValues("RegStepApiToken").FirstOrDefault();
            if (token == null)
                return false;
            using (var context = new EFDbContext())
            using (var repository = new FormsRepository(context))
            using (var um = new AppUserManager(new UserStore<User>(context)))
            {
                repository.DiscardSecurity();
                var user = context.Users.FirstOrDefault(u => u.ApiToken == token);
                (actionContext.ControllerContext.Controller as AuthApiController).User = user;
                if (user != null)
                {
                    if (user.ApiTokenExpiration < DateTimeOffset.UtcNow)
                        return false;
                    var ident = um.CreateIdentity(user, DefaultAuthenticationTypes.ApplicationCookie);
                    var roles = new List<string>();
                    foreach (var role in user.Roles)
                    {
                        var appRole = repository.Search<AppRole>(r => r.Id == role.RoleId).FirstOrDefault();
                        if (appRole == null)
                            continue;
                        roles.Add(appRole.Name);
                    }
                    (actionContext.ControllerContext.Controller as AuthApiController).Principal = new GenericPrincipal(ident, roles.ToArray());
                    if (user.ApiTokenExpiration.Value.AddMinutes(5) < DateTimeOffset.UtcNow)
                    {
                        user.ApiTokenExpiration = user.ApiTokenExpiration.Value.AddMinutes(5);
                        um.Update(user);
                    }
                    return true;
                }
                else
                {
                    // Not a user so now we check if it is a registrant.
                    var registrant = context.Registrants.FirstOrDefault(r => r.ApiToken == token);
                    (actionContext.ControllerContext.Controller as AuthApiController).Registrant = registrant;
                    if (registrant == null)
                        return false;
                    if (registrant.ApiTokenExpiration < DateTimeOffset.UtcNow)
                        return false;
                    if (registrant.ApiTokenExpiration.Value.AddMinutes(5) < DateTimeOffset.UtcNow)
                    {
                        registrant.ApiTokenExpiration = registrant.ApiTokenExpiration.Value.AddMinutes(5);
                        repository.Commit();
                    }
                    return true;
                }
            }
        }

        /// <summary>
        /// Called when an action is being authorized. This method uses the user <see cref="IPrincipal"/>
        /// returned via <see cref="HttpRequestContext.Principal"/>. Authorization is denied if
        /// - the request is not associated with any user.
        /// - the user is not authenticated,
        /// - the user is authenticated but is not in the authorized group of <see cref="Users"/> (if defined), or if the user
        /// is not in any of the authorized <see cref="Roles"/> (if defined).
        /// 
        /// If authorization is denied then this method will invoke <see cref="HandleUnauthorizedRequest(HttpActionContext)"/> to process the unauthorized request.
        /// </summary>
        /// <remarks>You can use <see cref="AllowAnonymousAttribute"/> to cause authorization checks to be skipped for a particular
        /// action or controller.</remarks>
        /// <seealso cref="IsAuthorized(HttpActionContext)" />
        /// <param name="actionContext">The context.</param>
        /// <exception cref="ArgumentNullException">The context parameter is null.</exception>
        public override void OnAuthorization(HttpActionContext actionContext)
        {
            if (!IsAuthorized(actionContext))
            {
                HandleUnauthorizedRequest(actionContext);
            }
        }

        /// <summary>
        /// Processes requests that fail authorization. This default implementation creates a new response with the
        /// Unauthorized status code. Override this method to provide your own handling for unauthorized requests.
        /// </summary>
        /// <param name="actionContext">The context.</param>
        protected virtual void HandleUnauthorizedRequest(HttpActionContext actionContext)
        {
            if (actionContext == null)
            {
                throw new Exception("Non Action");
            }

            actionContext.Response = actionContext.ControllerContext.Request.CreateErrorResponse(HttpStatusCode.Unauthorized, "You must provide a valid RegStepApiToken in the header.");
        }

    }

    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited = true, AllowMultiple = true)]
    public class ApiTokenUserAuthorizationAttribute : AuthorizationFilterAttribute
    {
        private readonly object _typeId = new object();

        /// <summary>
        /// Gets a unique identifier for this <see cref="T:System.Attribute"/>.
        /// </summary>
        /// <returns>The unique identifier for the attribute.</returns>
        public override object TypeId
        {
            get { return _typeId; }
        }

        /// <summary>
        /// Determines whether access for this particular request is authorized.
        /// </summary>
        /// <param name="actionContext">The context.</param>
        /// <returns><c>true</c> if access is authorized; otherwise <c>false</c>.</returns>
        protected virtual bool IsAuthorized(HttpActionContext actionContext)
        {
            // Lets try to authenticate
            string token = null;
            try
            {
                token = actionContext.ControllerContext.Request.Headers.GetValues("RegStepApiToken").FirstOrDefault();
            }
            catch (Exception)
            {
                return false;
            }
            if (token == null)
                return false;
            using (var context = new EFDbContext())
            using (var repository = new FormsRepository(context))
            using (var um = new AppUserManager(new UserStore<User>(context)))
            {
                repository.DiscardSecurity();
                var user = context.Users.FirstOrDefault(u => u.ApiToken == token);
                if (user == null)
                    return false;
                if (user.ApiTokenExpiration < DateTimeOffset.UtcNow)
                    return false;
                (actionContext.ControllerContext.Controller as AuthApiController).User = user;
                var ident = um.CreateIdentity(user, DefaultAuthenticationTypes.ApplicationCookie);
                var roles = new List<string>();
                foreach (var role in user.Roles)
                {
                    var appRole = repository.Search<AppRole>(r => r.Id == role.RoleId).FirstOrDefault();
                    if (appRole == null)
                        continue;
                    roles.Add(appRole.Name);
                }
                (actionContext.ControllerContext.Controller as AuthApiController).Principal = new GenericPrincipal(ident, roles.ToArray());
                if (user.ApiTokenExpiration.Value.AddMinutes(5) < DateTimeOffset.UtcNow)
                {
                    user.ApiTokenExpiration = user.ApiTokenExpiration.Value.AddMinutes(5);
                    um.Update(user);
                }
                return true;
            }
        }

        /// <summary>
        /// Called when an action is being authorized. This method uses the user <see cref="IPrincipal"/>
        /// returned via <see cref="HttpRequestContext.Principal"/>. Authorization is denied if
        /// - the request is not associated with any user.
        /// - the user is not authenticated,
        /// - the user is authenticated but is not in the authorized group of <see cref="Users"/> (if defined), or if the user
        /// is not in any of the authorized <see cref="Roles"/> (if defined).
        /// 
        /// If authorization is denied then this method will invoke <see cref="HandleUnauthorizedRequest(HttpActionContext)"/> to process the unauthorized request.
        /// </summary>
        /// <remarks>You can use <see cref="AllowAnonymousAttribute"/> to cause authorization checks to be skipped for a particular
        /// action or controller.</remarks>
        /// <seealso cref="IsAuthorized(HttpActionContext)" />
        /// <param name="actionContext">The context.</param>
        /// <exception cref="ArgumentNullException">The context parameter is null.</exception>
        public override void OnAuthorization(HttpActionContext actionContext)
        {
            if (!IsAuthorized(actionContext))
            {
                HandleUnauthorizedRequest(actionContext);
            }
        }

        /// <summary>
        /// Processes requests that fail authorization. This default implementation creates a new response with the
        /// Unauthorized status code. Override this method to provide your own handling for unauthorized requests.
        /// </summary>
        /// <param name="actionContext">The context.</param>
        protected virtual void HandleUnauthorizedRequest(HttpActionContext actionContext)
        {
            if (actionContext == null)
            {
                throw new Exception("Non Action");
            }

            actionContext.Response = actionContext.ControllerContext.Request.CreateErrorResponse(HttpStatusCode.Unauthorized, new Exception("Not Authorized"));
        }

    }

}