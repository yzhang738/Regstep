using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Newtonsoft.Json;
using RSToolKit.Domain;
using RSToolKit.Domain.Data;
using RSToolKit.Domain.Entities;
using RSToolKit.Domain.Entities.Clients;
using RSToolKit.Domain.Entities.MerchantAccount;
using RSToolKit.WebUI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;


namespace RSToolKit.WebUI.Controllers
{
    [System.Web.Http.Authorize(Roles = "Super Administrators,Administrators,Cloud Users,Cloud+ Users")]
    public class FormGatewayController : ApiController
    {
        private FormsRepository Repository;
        private EFDbContext Context;

        public FormGatewayController(FormsRepository repository)
        {
            Context = repository.Context;
            Repository = repository;
        }

        public FormGatewayController()
        {
            Context = new EFDbContext();
            Repository = new FormsRepository(Context);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Repository.Dispose();
                Context.Dispose();
            }
            base.Dispose(disposing);
        }

        public IHttpActionResult Get()
        {
            return NotFound();
        }

        public async Task<IHttpActionResult> Post(TransactionModel model)
        {
            var user = UserManager.FindByIdAsync(User.Identity.GetUserId());
            #region Initialization
            var errors = new Dictionary<Guid, string>();
            var form = Repository.Search<Form>(f => f.UId == model.FormKey).FirstOrDefault();
            if (form == null)
                return BadRequest();
            var registrant = form.Registrants.Where(r => r.UId == model.RegistrantKey).FirstOrDefault();
            if (registrant == null)
                return BadRequest();
            #endregion
            registrant.UpdateAccounts();
            var ammount = Math.Round(model.Amount, 2);
            if (ammount > registrant.TotalOwed)
                ammount = registrant.TotalOwed;

            if (form.MerchantAccount == null)
                return BadRequest("Error 500.M1: No Merchant Account Supplied");
            IMerchantAccount<TransactionRequest> gateway = form.MerchantAccount.GetGateway();
            switch (form.MerchantAccount.Descriminator)
            {
                case "paypal":
                    if (String.IsNullOrWhiteSpace(model.State))
                        return BadRequest("Error 500.P3: State must be supplied.");
                    if (String.IsNullOrWhiteSpace(model.Address1))
                        return BadRequest("Error 500.P1: Address must be supplied.");
                    if (String.IsNullOrWhiteSpace(model.ZipCode))
                        return BadRequest("Error 500.P4: Postal code must be supplied.");
                    if (String.IsNullOrWhiteSpace(model.City))
                        return BadRequest("Error 500.P2: City must be supplied.");
                    if (String.IsNullOrWhiteSpace(model.Phone))
                        return BadRequest("Error 500.P5: Phone must be supplied.");
                    break;
            }

            //Now we build the transaction Request.
            var request = new TransactionRequest()
            {
                Zip = model.ZipCode,
                Ammount = ammount,
                CVV = model.CardCode,
                CardNumber = model.CardNumber,
                NameOnCard = model.NameOnCard,
                ExpMonthAndYear = model.ExpMonth + model.ExpYear,
                Cart = JsonConvert.SerializeObject(registrant.GetShoppingCartItems().ToCart()),
                CompanyKey = form.CompanyKey,
                MerchantAccountKey = form.MerchantAccountKey,
                FormKey = form.UId,
                Form = form,
                RegistrantKey = registrant.UId,
                Registrant = registrant,
                TransactionType = TransactionType.AuthorizeCapture,
                Mode = (registrant.Type == RegistrationType.Live ? ServiceMode.Live : ServiceMode.Test),
                LastFour = model.CardNumber.GetLast(4),
                DateCreated = DateTimeOffset.UtcNow,
                DateModified = DateTimeOffset.UtcNow,
                Currency = form.Currency,
                Address = model.Address1,
                Address2 = model.Address2,
                City = model.City,
                Country = model.Country,
                State = model.State,
                Phone = model.Phone
            };
            if (form.MerchantAccount.Descriminator == "paypal")
            {
                request.CardType = model.CardType;
            }
            var names = model.NameOnCard.Split(' ');
            if (names.Length < 2)
            {
                request.FirstName = "";
                request.LastName = model.NameOnCard;
            }
            else
            {
                request.FirstName = names[0];
                request.LastName = names[names.Length - 1];
            }
            TransactionDetail td = null;
            if (registrant.Type == RegistrationType.Live)
            {
                request.Mode = ServiceMode.Live;
                if (!CCHelper.ValidateCard(model.CardNumber))
                    return BadRequest("Error 500.M3: Invalid credit card number.");
                if (String.IsNullOrWhiteSpace(model.NameOnCard))
                    return BadRequest("Error 500.M5: Must supply the name on the card.");
                if (String.IsNullOrWhiteSpace(model.ZipCode))
                    return BadRequest("Error 500.M6: Must supply zip code.");
                td = gateway.AuthorizeCapture(request);
            }
            else
            {
                td = new TransactionDetail()
                {
                    Approved = true,
                    Ammount = request.Ammount,
                    AuthorizationCode = "TEST",
                    FormKey = request.FormKey,
                    Message = "Success",
                    TransactionType = TransactionType.AuthorizeCapture,
                    TransactionID = "__TEST__" + Guid.NewGuid().ToString()
                };
            }
            td.Registrant = registrant;
            request.Details.Add(td);
            Repository.Add(request);
            registrant.UpdateAccounts();
            Repository.Commit();

            if (td.Approved)
            {
                return Ok(new { success = true });
            }
            else
            {
                return BadRequest("Error 500.M?: Unknown gateway error.");
            }
        }

        protected AppUserManager UserManager
        {
            get
            {
                return HttpContext.Current.GetOwinContext().GetUserManager<AppUserManager>();
            }
        }
    }
}
