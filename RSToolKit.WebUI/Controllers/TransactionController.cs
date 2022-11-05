using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RSToolKit.WebUI.Infrastructure;
using RSToolKit.Domain.Exceptions;
using RSToolKit.Domain.Data;
using RSToolKit.Domain.Entities.MerchantAccount;
using RSToolKit.Domain;

namespace RSToolKit.WebUI.Controllers
{
    [PopoutView]
    [MinimalView]
    [RegStepHandleError(typeof(InvalidIdException), "InvalidIdException")]
    [RegStepHandleError(typeof(TransactionNotFoundException), "RegistrantNotFound")]
    [RegStepHandleError(typeof(MerchantNotFoundException), "MerchantNotFoundException")]
    [RegStepHandleError(typeof(RefundAmountExceededException), "RefundAmountExceededException")]
    [RegStepHandleError(typeof(InsufficientPermissionsException), "InsufficientPermissionsException")]
    [Authorize(Roles = "Super Administrators,Administrators,Cloud Users,Cloud+ Users,Programmers")]
    public class TransactionController
        : RegStepController
    {
        protected TransactionRepository _tranRepo;

        /// <summary>
        /// Initializes the controller with a supplied context.
        /// </summary>
        public TransactionController(EFDbContext context)
            : base(context)
        {
            _tranRepo = new TransactionRepository(Context);
            Log.LoggingMethod = "TransactionController";
            Repositories.Add(_tranRepo);
        }

        /// <summary>
        /// Initializes the controller with a new context.
        /// </summary>
        public TransactionController()
            : base()
        {
            _tranRepo = new TransactionRepository(Context);
            Log.LoggingMethod = "TransactionController";
            Repositories.Add(_tranRepo);
        }

        [ComplexId("id")]
        [HttpGet]
        public ActionResult Index(object id)
        {
            var transaction = GetTransaction(id);
            return View("Index", transaction);
        }

        [ComplexId("id")]
        [HttpGet]
        public ActionResult Get(object id)
        {
            return Index(id);
        }

        protected TransactionRequest GetTransaction(object id)
        {
            TransactionRequest transaction = null;
            if (id is long)
                transaction = _tranRepo.First(t => t.SortingId == (long)id);
            else if (id is Guid)
                transaction = _tranRepo.Find((Guid)id);
            else
                throw new InvalidIdException("The id does not match any transaction in the database.");
            return transaction;
        }

        [HttpPut]
        [ComplexId("id")]
        [JsonValidateAntiForgeryToken]
        public JsonNetResult RefundAmount(object id, decimal amount)
        {
            var transaction = GetTransaction(id);
            var total = transaction.TotalAmount();
            if (amount > total)
                throw new RefundAmountExceededException("The refund amount of " + amount.ToString("c", transaction.Currency.GetCulture()) + " exceeds the captured amount of " + amount.ToString("c", transaction.Currency.GetCulture()));
            var captures = transaction.Details.Where(d => (d.TransactionType == TransactionType.Capture || d.TransactionType == TransactionType.AuthorizeCapture) && d.Approved).OrderByDescending(d => d.DateCreated).ToList();
            if (captures.Count == 0)
                throw new RefundAmountExceededException("The transaction has captured no funds.");
            var merchant = transaction.MerchantAccount;
            if (merchant == null)
                throw new MerchantNotFoundException();
            var gateway = merchant.GetGateway();
            var amountLeft = amount;
            foreach (var capture in captures)
            {
                if (amountLeft <= 0)
                    break;
                var r_ammount = amountLeft;
                if (r_ammount > capture.Ammount)
                    r_ammount = capture.Ammount;
                TransactionDetail td = null;
                if (transaction.Mode == ServiceMode.Test)
                {
                    td = new TransactionDetail()
                    {
                        TransactionType = TransactionType.Credit,
                        Ammount = r_ammount,
                        Approved = true,
                        TransactionRequestKey = transaction.UId,
                        DateCreated = DateTimeOffset.UtcNow
                    };
                }
                else
                {
                    td = gateway.Credit(r_ammount, capture, false);
                    if (td.Approved)
                    {
                        if (td.voided)
                        {
                            td.Ammount = r_ammount;
                        }
                        amountLeft -= r_ammount;
                    }

                }
                capture.Credits.Add(td);
                transaction.Details.Add(td);
            }
            transaction.Registrant.UpdateAccounts();
            _tranRepo.Commit();
            if (amountLeft > 0)
                throw new RefundAmountExceededException("The refund of " + amount + " was over the captured amount of " + total);
            return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { success = true } };
        }

        [HttpPut]
        [ComplexId("id")]
        [JsonValidateAntiForgeryToken]
        public JsonNetResult RefundBalance(object id)
        {
            var transaction = GetTransaction(id);
            var amount = transaction.TotalAmount();
            if (!amount.HasValue)
                throw new RefundAmountExceededException("The transaction has captured no funds.");
            var captures = transaction.Details.Where(d => (d.TransactionType == TransactionType.Capture || d.TransactionType == TransactionType.AuthorizeCapture) && d.Approved).OrderByDescending(d => d.DateCreated).ToList();
            if (captures.Count == 0)
                throw new RefundAmountExceededException("The transaction has captured no funds.");
            var merchant = transaction.MerchantAccount;
            if (merchant == null)
                throw new MerchantNotFoundException();
            var gateway = merchant.GetGateway();
            var amountLeft = amount.Value;
            foreach (var capture in captures.Where(c => c.Approved))
            {
                if (amountLeft <= 0)
                    break;
                var r_amount = amountLeft;
                if (r_amount > capture.Ammount)
                    r_amount = capture.Ammount;
                TransactionDetail td = null;
                if (transaction.Mode == ServiceMode.Test)
                {
                    td = new TransactionDetail()
                    {
                        TransactionType = TransactionType.Credit,
                        Ammount = amount.Value,
                        Approved = true,
                        TransactionRequestKey = transaction.UId,
                        DateCreated = DateTimeOffset.UtcNow
                    };
                }
                else
                {
                    td = gateway.Credit(r_amount, capture, false);
                    if (td.Approved)
                    {
                        if (td.voided)
                        {
                            td.Ammount = r_amount = capture.Ammount;
                        }
                        amountLeft -= r_amount;
                    }
                }
                if (td.Approved)
                    amountLeft -= r_amount;
                capture.Credits.Add(td);
                transaction.Details.Add(td);
            }
            transaction.Registrant.UpdateAccounts();
            _tranRepo.Commit();
            if (amountLeft > 0)
                throw new RefundAmountExceededException("There was a problem refunding the full amount.");
            return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { success = true } };
        }

    }
}
