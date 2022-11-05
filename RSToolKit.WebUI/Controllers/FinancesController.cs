using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RSToolKit.WebUI.Infrastructure;

namespace RSToolKit.WebUI.Controllers
{
    public class FinancesController
        : RegStepController
    {
        // GET: Finances
        public ActionResult Index()
        {
            return View();
        }
    }
}