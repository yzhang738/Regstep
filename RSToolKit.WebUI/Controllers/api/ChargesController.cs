using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RSToolKit.WebUI.Infrastructure;
using RSToolKit.WebUI.Infrastructure.Attributes.Authorization;
using RSToolKit.Domain.Data.Repositories;
using RSToolKit.Domain.Entities.Finances;
using RSToolKit.Domain.Entities;

namespace RSToolKit.WebUI.Controllers.API
{
    [ApiAuthorization(Roles = "Super Administrators")]
    public class ChargesController
        : RegStepApiController
    {

        /// <summary>
        /// The current charge being worked with.
        /// </summary>
        protected Form _form;

        public ChargesController()
            : base()
        { }

        // GET: Charge
        public JsonNetResult Get(long id)
        {
            var charges = Context.Charges.Where(c => c.FormKey == id).OrderBy(c => c.DateCreated).ToList();
            return JsonNetResult.Success(Url.Link("getCharges", new { controller = "charges", id = id }), data: charges);
        }
    }
}