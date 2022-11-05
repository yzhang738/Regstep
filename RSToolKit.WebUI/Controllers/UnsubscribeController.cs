using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RSToolKit.Domain.Entities.Email;
using RSToolKit.Domain.Data;
using RSToolKit.WebUI.Models.Views.Unsubscribe;

namespace RSToolKit.WebUI.Controllers
{
    [AllowAnonymous]
    public class UnsubscribeController : Controller
    {
        [HttpGet]
        public ActionResult Company(long id, long emailSend)
        {
            using (var context = new EFDbContext())
            {
                var company = context.Companies.First(c => c.SortingId == id);
                var send = context.EmailSends.First(s => s.SortingId == emailSend);
                // First lets see if the user has already unsubscribed with that email.
                var unsub = context.Unsubscribes.FirstOrDefault(s => s.UnsubscribeFrom == company.UId && s.Email == send.Recipient);
                if (unsub != null)
                    // previously unsubscribed.
                    return View("AlreadyUnsubscribed");
                unsub = new Unsubscribe()
                {
                    UId = Guid.NewGuid(),
                    DateSubmitted = DateTimeOffset.Now,
                    Email = send.Recipient.Trim().ToLower(),
                    UnsubscribeFrom = company.UId
                };
                context.Unsubscribes.Add(unsub);
                context.SaveChanges();
            }
            return View("Unsubscribed");
        }

        [HttpGet]
        public ActionResult Form(long id, long emailSend)
        {
            using (var context = new EFDbContext())
            {
                var form = context.Forms.First(c => c.SortingId == id);
                var send = context.EmailSends.First(s => s.SortingId == emailSend);
                // First lets see if the user has already unsubscribed with that email.
                var unsub = context.Unsubscribes.FirstOrDefault(s => s.UnsubscribeFrom == form.UId && s.Email == send.Recipient);
                if (unsub != null)
                    // previously unsubscribed.
                    return View("AlreadyUnsubscribed");
                unsub = new Unsubscribe()
                {
                    UId = Guid.NewGuid(),
                    DateSubmitted = DateTimeOffset.Now,
                    Email = send.Recipient.Trim().ToLower(),
                    UnsubscribeFrom = form.UId
                };
                context.Unsubscribes.Add(unsub);
                context.SaveChanges();
            }
            return View("Unsubscribed");
        }

        [HttpGet]
        public ActionResult Campaign(long id, long emailSend)
        {
            using (var context = new EFDbContext())
            {
                var campaign = context.EmailCampaigns.First(c => c.SortingId == id);
                var send = context.EmailSends.First(s => s.SortingId == emailSend);
                // First lets see if the user has already unsubscribed with that email.
                var unsub = context.Unsubscribes.FirstOrDefault(s => s.UnsubscribeFrom == campaign.UId && s.Email == send.Recipient);
                if (unsub != null)
                    // previously unsubscribed.
                    return View("AlreadyUnsubscribed");
                unsub = new Unsubscribe()
                {
                    UId = Guid.NewGuid(),
                    DateSubmitted = DateTimeOffset.Now,
                    Email = send.Recipient.Trim().ToLower(),
                    UnsubscribeFrom = campaign.UId
                };
                context.Unsubscribes.Add(unsub);
                context.SaveChanges();
            }
            return View("Unsubscribed");
        }

    }
}