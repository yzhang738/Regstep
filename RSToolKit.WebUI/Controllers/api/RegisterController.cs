using System.Net;
using System.Net.Http;
using System.Web.Http;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity;
using RSToolKit.Domain.Entities.Clients;
using RSToolKit.Domain.Data;
using RSToolKit.Domain;
using RSToolKit.Domain.Entities;
using RSToolKit.WebUI.Infrastructure;
using RSToolKit.Domain.Engines;
using RSToolKit.Domain.JItems;
using RSToolKit.Domain.Entities.Components;
using System.Collections.Generic;
using System.Linq;

namespace RSToolKit.WebUI.Controllers.API
{
    [ApiTokenAttendeeAuthorization]
    public class RegisterController : AuthApiController
    {
        public IHttpActionResult Get(long eventId, long registrantId, int page = 0)
        {
            using (var context = new EFDbContext())
            using (var repository = new FormsRepository(context, User, Principal))
            {
                var form = repository.Search<Form>(f => f.SortingId == eventId).FirstOrDefault();
                if (form == null)
                    return BadRequest("eventId invalid");
                var registrant = form.Registrants.FirstOrDefault(r => r.SortingId == registrantId);
                if (registrant == null)
                    return BadRequest("registrantId invalid");
                var t_page = form.Pages.FirstOrDefault(p => p.PageNumber == page);
                var r_page = new JPage();
                if (t_page == null)
                {
                    if (!form.Survey)
                    {
                        var apiResult = new ApiData() { success = true, message = "review", data = ToRegistrationInfo(registrant) };
                        // Review Page
                    }
                    else
                    {
                        var t_data = new { };
                        var apiResult = new ApiData() { success = true, message = "confirmation", data = t_data };
                        // Confirmation Page
                    }
                }
            }
            return Ok();
        }
    }

    public class JPage
    {
        public IEnumerable<JPanel> panels { get; set; }
        public int number { get; set; }
        public IEnumerable<JComponentError> errors { get; set; }
        public long eventId { get; set; }
        public long registrantId { get; set; }
        public bool lastPage { get; set; }

        public JPage()
        {
            panels = new List<JPanel>();
            errors = new List<JComponentError>();
            number = 0;
            eventId = registrantId = 0L;

        }
    }

    public class JPanel
    {
        public IEnumerable<JComponent> components { get; set; }
        public int order { get; set; }

        public JPanel()
        {
            components = new List<JComponent>();
            order = 0;
        }
    }

    public class JComponentError
    {
        public string message { get; set; }
        public long componentId { get; set; }

        public JComponentError()
        {
            message = "There was an error.";
            componentId = 0L;
        }
    }
}
