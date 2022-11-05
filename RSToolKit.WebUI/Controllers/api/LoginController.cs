using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity;
using RSToolKit.Domain.Entities.Clients;
using RSToolKit.Domain.Data;
using RSToolKit.Domain;
using RSToolKit.Domain.Entities;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Web.Http.Cors;
using System.IO;

namespace RSToolKit.WebUI.Controllers.API
{
    [EnableCors("*", "*", "*")]
    [AllowAnonymous]
    public class LoginController : ApiController
    {
        public IHttpActionResult Post(APILoginInformation data)
        {
            //var type = Request.Content.Headers.ContentType;
            //var bodyText = Request.Content.ReadAsStringAsync();
            if (!ModelState.IsValid)
                return BadRequest("The username and password must be supplied.");
            if (data.username == null || data.password == null)
                return BadRequest("The username and password must be provided.");
            User user = null;
            if (!data.eventId.HasValue)
            {
                using (var context = new EFDbContext())
                using (var um = new AppUserManager(new UserStore<User>(context)))
                {
                    user = um.Find(data.username, data.password);
                    if (user != null)
                    {
                        user.ApiToken = Guid.NewGuid().ToString();
                        user.ApiTokenExpiration = DateTimeOffset.UtcNow.AddHours(72);
                        if (data.rememberMe)
                            user.ApiTokenExpiration = DateTimeOffset.UtcNow.AddDays(60);
                        um.Update(user);
                        return Ok(new APILoginData(true, "Success", user.ApiToken, user.ApiTokenExpiration));
                    }
                }
                return Ok(new APILoginData(false, "Invalid login or password.", null, null));
            }
            else
            {
                using (var repository = new FormsRepository())
                {
                    var form = repository.Search<Form>(f => f.SortingId == data.eventId.Value).FirstOrDefault();
                    if (form == null)
                        return Ok(new APILoginData(false, "Invalid event.", null, null));
                    if (String.IsNullOrEmpty(data.password))
                        data.password = "0";
                    var sortingId = -1L;
                    if (!Int64.TryParse(data.password, System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture, out sortingId))
                        sortingId = -1L;
                    var registrant = form.Registrants.FirstOrDefault(r => r.Email.ToLower() == data.username.ToLower());
                    if (registrant == null)
                        return Ok(new APILoginData(false, "Invalid login.", null, null));
                    if (registrant.SortingId != sortingId)
                        if (registrant.InvoiceNumber != data.password)
                            return Ok(new APILoginData(false, "Invalid password.", null, null));
                    registrant.ApiToken = Guid.NewGuid().ToString();
                    registrant.ApiTokenExpiration = DateTimeOffset.UtcNow.AddHours(72);
                    if (data.rememberMe)
                        registrant.ApiTokenExpiration = DateTimeOffset.UtcNow.AddDays(60);
                    repository.Commit();
                    return Ok(new APILoginData(true, "Success", registrant.ApiToken, registrant.ApiTokenExpiration));
                }
            }
        }

        public IHttpActionResult Options()
        {
            return Ok();
        }

        public IHttpActionResult Get()
        {
            return Ok("Good");
        }

    }

    public class APILoginInformation
    {
        public long? eventId { get; set; }
        [Required]
        public string username { get; set; }
        [Required]
        public string password { get; set; }
        public bool rememberMe { get; set; }

        public APILoginInformation()
        {
            eventId = null;
            username = null;
            password = null;
            rememberMe = false;
        }
    }

    public class APILoginData
    {
        public bool success { get; set; }
        public string message { get; set; }
        public string apiToken { get; set; }
        public DateTimeOffset apiExpiration { get; set; }

        public APILoginData()
        {
            success = false;
            message = "Not Initialized";
            apiToken = null;
            apiExpiration = DateTimeOffset.Now;
        }

        public APILoginData(bool Success, string Message, string APIToken, DateTimeOffset? APIExpiration)
            : this()
        {
            success = Success;
            message = Message;
            apiToken = APIToken;
            apiExpiration = APIExpiration.HasValue ? APIExpiration.Value : apiExpiration;
        }
    }
}
