using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using RSToolKit.WebUI.Infrastructure;
using RSToolKit.WebUI.Controllers;

namespace RSToolKit.WebUI.Infrastructure.Register
{
    /// <summary>
    /// Creates html helpers for registration.
    /// </summary>
    public static class RegisterHtmlHelper
    {
        /// <summary>
        /// Creates the desired html helper.
        /// </summary>
        /// <param name="controller">The controller being used.</param>
        /// <returns>An html helper.</returns>
        public static IRegisterHtmlHelper Create(RegStepController controller)
        {
            IRegisterHtmlHelper helper = null;
            if (controller is AdminRegisterController)
            {
                var aController = controller as AdminRegisterController;
                // We need to return an AdminRegisterHtmlHelper.
                if (aController.Registrant == null)
                    helper = new AdminRegisterHtmlHelper(aController.Form, aController.Context);
                else
                    helper = new AdminRegisterHtmlHelper(aController.Registrant, aController.CurrentPageNumber, aController.Context);
                return helper;
            }
            return null;
        }
    }
}