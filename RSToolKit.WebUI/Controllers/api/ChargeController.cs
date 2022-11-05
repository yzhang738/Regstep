using System;
using RSToolKit.WebUI.Infrastructure;
using RSToolKit.WebUI.Infrastructure.Attributes.Authorization;
using RSToolKit.Domain.Data.Repositories;
using RSToolKit.Domain.Entities.Finances;

namespace RSToolKit.WebUI.Controllers.API
{
    [ApiAuthorization(Roles = "Super Administrators")]
    public class ChargeController
        : RegStepApiController
    {

        /// <summary>
        /// The charge repository.
        /// </summary>
        protected ChargeRepository _repo;
        /// <summary>
        /// The current charge being worked with.
        /// </summary>
        protected Charge _charge;

        public ChargeController()
            : base()
        {
            this._repo = new ChargeRepository(Context, new Domain.Security.SecureUser(Principal, Context));
        }

        // GET: Charge
        public JsonNetResult Get(long id)
        {
            this._charge = this._repo.Find(id);
            return JsonNetResult.Success(Url.Link("getCharge", new { controller = "charge", id = id }), data: this._charge);
        }

        public JsonNetResult Put(Charge input)
        {
            this._charge = this._repo.Find(input.Id, ignoreException: true);
            if (this._charge == null)
            {
                // The charge did not exist. We need to create it.
                this._charge = input;
                this._charge.UId = Guid.NewGuid();
                Context.Charges.Add(this._charge);
            }
            var affected = Context.SaveChanges();
            if (affected > 0)
                return JsonNetResult.Success(location: Url.Link("getCharge", new { controller = "charge", id = this._charge.Id }));
            return JsonNetResult.Failure();
        }

        public JsonNetResult Post(Charge input)
        {
            input.UId = Guid.NewGuid();
            Context.Charges.Add(input);
            var affected = Context.SaveChanges();
            if (affected > 0)
                return JsonNetResult.Success(location: Url.Link("getCharge", new { controller = "charge", id = this._charge.Id }));
            return JsonNetResult.Failure();
        }

        public JsonNetResult Delete(long id)
        {
            this._charge = this._repo.Find(id);
            this._repo.Delete(this._charge);
            var affected = Context.SaveChanges();
            if (affected > 0)
                return JsonNetResult.Success(location: Url.Link("getCharge", new { controller = "charge", id = this._charge.Id }));
            return JsonNetResult.Failure();
        }
    }
}