using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RSToolKit.WebUI.Infrastructure;
using RSToolKit.Domain.Data;
using RSToolKit.Domain.Entities.MerchantAccount;

namespace RSToolKit.WebUI.Controllers
{
    public class MerchantAccountController
        : RegStepController
    {

        protected MerchantAccountRepository _mercRepo;

        #region Constructors
        protected void _Initialize()
        {
            Log.LoggingMethod = "MerchantAccountController";
            this._mercRepo = new MerchantAccountRepository(Context);
            Repositories.Add(this._mercRepo);
        }

        public MerchantAccountController()
            : base()
        {
            this._Initialize();
        }

        public MerchantAccountController(EFDbContext context)
            : base(context)
        {
            this._Initialize();
        }
        #endregion

        #region Actions
        [HttpGet]
        [ComplexId("id")]
        public ActionResult Index(object id)
        {
            var merchantAccount = this._GetMerchantAccount(id);
            UpdateTrailLabel("Merchant Account: " + merchantAccount.Name);
            return View("Index", merchantAccount);
        }

        [HttpGet]
        [ComplexId("id")]
        public ActionResult Get(object id)
        {
            if (id is long && (long)id == -1)
                return List();
            return Index(id);
        }

        [HttpGet]
        public ActionResult List()
        {
            UpdateTrailLabel(WorkingCompany.Name + ": Merchant Accounts");
            return View("List", WorkingCompany.MerchantAccounts);
        }

        [HttpPut]
        [JsonValidateAntiForgeryToken]
        [IsAjax]
        public JsonNetResult Put(MerchantAccountInfo model)
        {
            var merchant = this._GetMerchantAccount(model.UId);
            merchant.Name = model.Name;
            merchant.Key = model.Key;
            merchant.UserName = model.UserName;
            merchant.Descriminator = model.Descriminator;
            merchant.Parameters = model.Parameters;
            this._mercRepo.Commit();
            return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { success = true } };
        }

        [HttpDelete]
        [ComplexId("id")]
        [JsonValidateAntiForgeryToken]
        [IsAjax]
        public JsonNetResult Delete(object id)
        {
            var merchant = this._GetMerchantAccount(id);
            this._mercRepo.Remove(merchant);
            this._mercRepo.Commit();
            return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { success = true } };
        }

        [HttpGet]
        [NoTrail]
        public ActionResult New()
        {
            var merchant = new MerchantAccountInfo()
            {
                Company = WorkingCompany,
                CompanyKey = WorkingCompany.UId,
                UId = Guid.NewGuid()
            };
            this._mercRepo.Add(merchant);
            this._mercRepo.Commit();
            return RedirectToAction("Get", new { id = merchant.SortingId });
        }
        #endregion

        #region Suporting
        /// <summary>
        /// Gets the merchant account by the id.
        /// </summary>
        /// <param name="id">The id of the <code>MerchantAccountInfo</code> as either <code>Guid</code> or <code>long</code>.</param>
        /// <returns></returns>
        protected MerchantAccountInfo _GetMerchantAccount(object id)
        {
            if (id is Guid)
                return this._mercRepo.Find((Guid)id);
            else if (id is long)
                return this._mercRepo.First(m => m.SortingId == (long)id);
            throw new InvalidIdException();
        }
        #endregion
    }
}