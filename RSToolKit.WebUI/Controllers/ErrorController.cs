using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RSToolKit.WebUI.Infrastructure;

namespace RSToolKit.WebUI.Controllers
{
    [AllowAnonymous]
    public class ErrorController : RSController
    {
        // GET: Error
        public ActionResult Error403()
        {
            return View();
        }

        public ActionResult Error404()
        {
            return View();
        }

        public ActionResult Error500(long id = 0)
        {
            return View(id);
        }

        public ActionResult Error(long id = 0)
        {
            return View("Error500", id);
        }
    }
}