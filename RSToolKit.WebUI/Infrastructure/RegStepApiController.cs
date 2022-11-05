using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using RSToolKit.Domain.Entities.Clients;
using RSToolKit.Domain.Entities;
using System.Security.Principal;
using RSToolKit.WebUI.Controllers.API;
using RSToolKit.Domain;
using Newtonsoft.Json;
using RSToolKit.Domain.Data;
using RSToolKit.Domain.Engines;
using RSToolKit.Domain.JItems;
using RSToolKit.Domain.Entities.Components;
using RSToolKit.WebUI.Infrastructure.Attributes.Authorization;

namespace RSToolKit.WebUI.Infrastructure
{
    /// <summary>
    /// An api controller that uses authentication.
    /// </summary>
    public class RegStepApiController
        : ApiController, IDisposable
    {
        /// <summary>
        /// The authenticated principal.
        /// </summary>
        public IPrincipal Principal
        {
            get
            {
                return User;
            }
        }

        /// <summary>
        /// The context being used.
        /// </summary>
        public EFDbContext Context { get; protected set; }

        /// <summary>
        /// Creates the controller.
        /// </summary>
        public RegStepApiController()
        {
            Context = new EFDbContext();
        }

         public new void Dispose()
        {
            Context.Dispose();
            base.Dispose();
        }

    }
}