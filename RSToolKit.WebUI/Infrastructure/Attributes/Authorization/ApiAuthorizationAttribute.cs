using System;
using System.Linq;
using System.Web;
using System.Web.Http.Controllers;
using System.Net.Http;
using System.Net;
using System.Web.Http.Filters;
using Microsoft.AspNet.Identity;
using RSToolKit.Domain.Identity;
using System.Collections.Generic;

namespace RSToolKit.WebUI.Infrastructure.Attributes.Authorization
{
    /// <summary>
    /// An attribute that uses both token authorization and ASP.NET authorization.
    /// </summary>
    public class ApiAuthorizationAttribute
        : AuthorizationFilterAttribute
    {
        /// <summary>
        /// The roles to use for the authorization.
        /// </summary>
        public string Roles
        {
            get { return _roles ?? String.Empty; }
            set
            {
                _roles = value;
                _rolesSplit = this._splitString(value);
            }

        }
        protected string[] _rolesSplit { get; set; }
        protected string _roles { get; set; }

        private class KeyAndToken
        {
            public Guid token { get; set; }
            public String key { get; set; }
        }

        protected virtual bool _IsAuthorized(HttpActionContext actionContext)
        {
            // First we see if the user is already authenticated through the ASP.NET system.
            var principal = actionContext.ControllerContext.RequestContext.Principal;
            if (principal != null && principal.Identity != null && principal.Identity.IsAuthenticated)
                return principal.IsInRole(this._rolesSplit);

            // The user was not authenticated by the system. We need to authenticate api token credentials.

            IEnumerable<string> authValues = null;
            if (!actionContext.ControllerContext.Request.Headers.TryGetValues("RegStepApiToken", out authValues))
                return false;
            if (authValues.Count() == 0)
                return false;
            var token = authValues.First();

            // Now we have the token. We need to authenticate the user.
            using (var context = new Domain.Data.EFDbContext())
            using (var um = new Domain.Entities.Clients.AppUserManager(new Microsoft.AspNet.Identity.EntityFramework.UserStore<Domain.Entities.Clients.User>(context)))
            {
                var user = context.Users.Where(u => u.ApiToken == token).FirstOrDefault();
                if (user == null)
                    return false;
                if (!user.ApiTokenExpiration.HasValue || user.ApiTokenExpiration < DateTimeOffset.Now)
                    return false;

                // Now let's create the identity and sign in.
                var ident = um.CreateIdentity(user, DefaultAuthenticationTypes.ApplicationCookie);
                actionContext.Request.GetOwinContext().Authentication.SignOut();
                actionContext.Request.GetOwinContext().Authentication.SignIn(new Microsoft.Owin.Security.AuthenticationProperties { IsPersistent = true }, ident);
                return actionContext.RequestContext.Principal.IsInRole(this._rolesSplit);
            }
        }

        public override void OnAuthorization(HttpActionContext actionContext)
        {
            if (!this._IsAuthorized(actionContext))
                this._HandleUnathorizedRequest(actionContext);

            // The user is authorizes, but that doesn't mean they have permissions.  We need to check against roles.
            if (this._rolesSplit.Length == 0)
                // No roles.
                return;
            foreach (var role in this._rolesSplit)
                if (actionContext.RequestContext.Principal.IsInRole(role))
                    return;
            // They are not authorized in the group.
            this._HandleUnathorizedRequest(actionContext);
        }

        protected virtual void _HandleUnathorizedRequest(HttpActionContext actionContext)
        {
            if (actionContext == null)
                throw new Exception("No action.");
            actionContext.Response = actionContext.ControllerContext.Request.CreateErrorResponse(HttpStatusCode.Unauthorized, new UnauthorizedAccessException());
        }

        /// <summary>
        /// Splits the string on commas and removes any leading/trailing whitespace from each result item.
        /// </summary>
        /// <param name="original">The input string.</param>
        /// <returns>An array of strings parsed from the input <paramref name="original"/> string.</returns>
        protected string[] _splitString(string original)
        {
            if (String.IsNullOrEmpty(original))
            {
                return new string[0];
            }

            var split = from piece in original.Split(',')
                        let trimmed = piece.Trim()
                        where !String.IsNullOrEmpty(trimmed)
                        select trimmed;
            return split.ToArray();
        }
    }
}