using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using RSToolKit.Domain.Entities.Clients;

namespace RSToolKit.WebUI.Infrastructure
{
    public interface IRegStepController
        : IController
    {
        /// <summary>
        /// The User Manager for the application.
        /// </summary>
        AppUserManager UserManager { get; }
        /// <summary>
        /// The Role Manager for the application.
        /// </summary>
        AppRoleManager RoleManager { get; }
        /// <summary>
        /// The current user logged in.
        /// </summary>
        User AppUser { get; }
    }
}