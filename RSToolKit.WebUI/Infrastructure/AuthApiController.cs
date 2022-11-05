using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using RSToolKit.Domain.Entities.Clients;
using RSToolKit.Domain.Entities;
using System.Security.Principal;
using RSToolKit.WebUI.Controllers.API;
using RSToolKit.Domain;
using Newtonsoft.Json;
using RSToolKit.Domain.Data;
using RSToolKit.Domain.Engines;
using RSToolKit.Domain.JItems;
using RSToolKit.Domain.Entities.Components;

namespace RSToolKit.WebUI.Infrastructure
{
    public class AuthApiController : ApiController
    {
        public new User User { get; set; }
        public Registrant Registrant { get; set; }
        public IPrincipal Principal { get; set; }

        protected static JRegistrant ToRegistrationInfo(Registrant registrant)
        {
            var t_reg = new JRegistrant();
            // first name
            t_reg.firstName = registrant.SearchPrettyValue("firstname");
            t_reg.firstName = t_reg.firstName ?? registrant.SearchPrettyValue("FirstName");
            t_reg.firstName = t_reg.firstName ?? registrant.SearchPrettyValue("firstName");
            t_reg.firstName = t_reg.firstName ?? "";
            // last name
            t_reg.lastName = registrant.SearchPrettyValue("lastname");
            t_reg.lastName = t_reg.lastName ?? registrant.SearchPrettyValue("LastName");
            t_reg.lastName = t_reg.lastName ?? registrant.SearchPrettyValue("lastName");
            t_reg.lastName = t_reg.lastName ?? "";
            // address1
            t_reg.address1 = registrant.SearchPrettyValue("address");
            t_reg.address1 = t_reg.address1 ?? registrant.SearchPrettyValue("Address");
            t_reg.address1 = t_reg.address1 ?? registrant.SearchPrettyValue("address1");
            t_reg.address1 = t_reg.address1 ?? registrant.SearchPrettyValue("Address1");
            t_reg.address1 = t_reg.address1 ?? "";
            // address2
            t_reg.address2 = registrant.SearchPrettyValue("address2");
            t_reg.address2 = t_reg.address2 ?? registrant.SearchPrettyValue("Address2");
            t_reg.address2 = t_reg.address2 ?? "";
            // city
            t_reg.city = registrant.SearchPrettyValue("address2");
            t_reg.city = t_reg.city ?? registrant.SearchPrettyValue("Address2");
            t_reg.city = t_reg.city ?? "";
            // postalcode
            t_reg.postalCode = registrant.SearchPrettyValue("zip");
            t_reg.postalCode = t_reg.postalCode ?? registrant.SearchPrettyValue("Zip");
            t_reg.postalCode = t_reg.postalCode ?? registrant.SearchPrettyValue("zipcode");
            t_reg.postalCode = t_reg.postalCode ?? registrant.SearchPrettyValue("zipCode");
            t_reg.postalCode = t_reg.postalCode ?? registrant.SearchPrettyValue("ZipCode");
            t_reg.postalCode = t_reg.postalCode ?? registrant.SearchPrettyValue("postalcode");
            t_reg.postalCode = t_reg.postalCode ?? registrant.SearchPrettyValue("postalCode");
            t_reg.postalCode = t_reg.postalCode ?? registrant.SearchPrettyValue("PostalCode");
            t_reg.postalCode = t_reg.postalCode ?? "";
            // stateorprovince
            t_reg.stateOrProvince = t_reg.stateOrProvince ?? registrant.SearchPrettyValue("stateOrProvince");
            t_reg.stateOrProvince = t_reg.stateOrProvince ?? registrant.SearchPrettyValue("StateOrProvince");
            t_reg.stateOrProvince = t_reg.stateOrProvince ?? registrant.SearchPrettyValue("stateorprovince");
            t_reg.stateOrProvince = t_reg.stateOrProvince ?? registrant.SearchPrettyValue("state");
            t_reg.stateOrProvince = t_reg.stateOrProvince ?? registrant.SearchPrettyValue("State");
            t_reg.stateOrProvince = t_reg.stateOrProvince ?? registrant.SearchPrettyValue("province");
            t_reg.stateOrProvince = t_reg.stateOrProvince ?? registrant.SearchPrettyValue("Province");
            t_reg.stateOrProvince = t_reg.stateOrProvince ?? "";
            // country
            t_reg.country = registrant.SearchPrettyValue("country");
            t_reg.country = t_reg.country ?? registrant.SearchPrettyValue("Country");
            t_reg.country = t_reg.country ?? "";
            // phone number
            t_reg.phone = registrant.SearchPrettyValue("phone");
            t_reg.phone = t_reg.phone ?? registrant.SearchPrettyValue("Phone");
            t_reg.phone = t_reg.phone ?? registrant.SearchPrettyValue("phonenumber");
            t_reg.phone = t_reg.phone ?? registrant.SearchPrettyValue("phoneNumber");
            t_reg.phone = t_reg.phone ?? registrant.SearchPrettyValue("PhoneNumber");
            t_reg.phone = t_reg.phone ?? "";

            t_reg.audience = registrant.Audience != null ? registrant.Audience.Name : "";
            t_reg.attended = registrant.Attended;
            t_reg.status = registrant.Status.ToString();
            t_reg.type = registrant.Type.GetStringValue();
            t_reg.id = registrant.SortingId;
            t_reg.email = registrant.Email.ToLower();
            t_reg.rsvp = registrant.RSVP;
            t_reg.dateRegistered = registrant.DateCreated;
            t_reg.dateModified = registrant.DateModified;
            t_reg.confirmation = registrant.Confirmation;
            foreach (var comp in registrant.Form.GetComponents())
            {
                var data = registrant.Data.FirstOrDefault(d => d.VariableUId == comp.UId);
                if (data == null || data.Value == null)
                    continue;
                var t_data = new JRegistrantData();
                t_data.id = data.SortingId;
                t_data.label = data.Component.LabelText;
                t_data.value = data.GetFormattedValue();
                t_data.raw = data.GetRaw();
                t_data.componentId = data.Component.SortingId;
                if (data.Component is Input && (data.Component as Input).Type == InputType.File)
                {
                    t_data.value = "https://toolkit.regstep.com/api/registrant/file/" + data.GetRaw();
                }
                t_reg.data.Add(t_data);
            }
            return t_reg;
        }

        protected static IEnumerable<JRegistrantAgenda> GetAgenda(Registrant registrant)
        {
            var items = new List<JRegistrantAgenda>();
            var itemParents = new Dictionary<long, List<Guid>>();
            foreach (var item in registrant.Form.GetComponentItems().Where(i => i.AgendaItem))
            {
                var id = item.Parent.SortingId;
                var data = registrant.Data.Where(d => d.Component.SortingId == id).FirstOrDefault();
                if (data == null)
                    continue;
                if (!itemParents.Keys.Contains(id))
                    itemParents[id] = JsonConvert.DeserializeObject<List<Guid>>(data.Value);
                var selections = itemParents[id];
                if (selections.Contains(item.UId))
                    items.Add(new JRegistrantAgenda() { name = item.LabelText, start = data.Component.AgendaStart, end = data.Component.AgendaEnd });
            }
            return items;
        }

        /// <summary>
        /// Tests to see if a component should be skipped.
        /// </summary>
        /// <param name="repository">The repository working with.</param>
        /// <param name="component">The component to test.</param>
        /// <param name="regsitrant">The registrant to test against.</param>
        /// <param name="admin">If the requestor is an admin.</param>
        /// <param name="comparer">Compares an IComponentItemParent to see if the item is already selected.</param>
        /// <returns></returns>
        public static ComponentSkipResult TestSkip(FormsRepository repository, IComponent component, Registrant registrant, bool admin = false)
        {
            var skip = new ComponentSkipResult();
            if (registrant != null)
            {
                if ((registrant.RSVP && component.RSVP == RSVPType.Decline) || (!registrant.RSVP && component.RSVP == RSVPType.Accept))
                    skip.NullOut = true;
                if (component.Audiences.Count > 0 && (registrant.Audience == null || !component.Audiences.Contains(registrant.Audience)))
                    skip.NullOut = true;
                var commands = LogicEngine.RunLogic(component, repository, registrant: registrant);
                foreach (var command in commands)
                {
                    switch (command.Command)
                    {
                        case JLogicWork.Hide:
                            skip.NullOut = true;
                            break;
                    }
                }
            }
            List<long> comparer = null;
            if (registrant != null)
            {
                var data = registrant.Data.FirstOrDefault(c => c.SortingId == component.SortingId);
                if (data != null)
                {
                    if (component is IComponentMultipleSelection)
                    {
                        comparer = JsonConvert.DeserializeObject<List<long>>(data.GetRaw());
                    }
                    else if (component is IComponentItemParent)
                    {
                        long t_selection;
                        if (long.TryParse(data.GetRaw(), out t_selection))
                            comparer.Add(t_selection);
                    }
                }
            }
            if (!component.Enabled)
                if (comparer == null || !comparer.Contains(component.SortingId))
                    skip.Hide = true;
            if (component.AdminOnly && !admin)
                skip.Hide = true;
            return skip;
        }

        public JConfirmation GetConfirmation(Registrant registrant, RSParser parser)
        {
            var conf = new JConfirmation();
            conf.registrant = ToRegistrationInfo(registrant);
            conf.editable = registrant.Form.Editable;
            conf.cancelable = registrant.Form.Cancelable;
            var c_page = registrant.Form.Pages.FirstOrDefault(p => p.Type == PageType.Confirmation);
            var c_freeText = c_page.Panels[0].Components[0] as FreeText;
            conf.message = parser.ParseAvailable(c_freeText.Html);
            if (!registrant.Form.DisableShoppingCart)
                conf.invoice = GetInvoice(registrant);

            return conf;
        }

        public JInvoice GetInvoice(Registrant registrant)
        {
            var t_items = new List<JInvoiceItem>();
            var invoice = new JInvoice();
            if (registrant.Form.Price.HasValue)
                t_items.Add(new JInvoiceItem() { type = "base fee", name = "Registration Fee", description = "Price to register.", quanity = 1, amount = registrant.Form.Price.Value });
            foreach (var item in registrant.GetShoppingCartItems().Items)
            {
                var t_item = new JInvoiceItem() { type = "item", name = item.Name, description = item.Name, amount = item.Ammount, quanity = item.Quanity };
                t_items.Add(t_item);
            }

            return invoice;
        }

    }

    public class JConfirmation
    {
        public JRegistrant registrant { get; set; }
        public JInvoice invoice { get; set; }
        public bool editable { get; set; }
        public bool cancelable { get; set; }
        public string message { get; set; }

        public JConfirmation()
        {
            registrant = new JRegistrant();
            invoice = null;
            editable = cancelable = false;
            message = "";
        }
    }

    public class JInvoice
    {
        public IEnumerable<JInvoiceItem> items { get; set; }

        public JInvoice()
        {
            items = new List<JInvoiceItem>();
        }
    }

    public class JInvoiceItem
    {
        public decimal amount { get; set; }
        public int quanity { get; set; }
        public string description { get; set; }
        public string type { get; set; }
        public string name { get; set; }

        public JInvoiceItem()
        {
            amount = 0m;
            quanity = 1;
            description = name = type = "";
        }
    }

    public class JRegistrant
    {
        public long id { get; set; }
        public string confirmation { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string email { get; set; }
        public string phone { get; set; }
        public string address1 { get; set; }
        public string address2 { get; set; }
        public string city { get; set; }
        public string stateOrProvince { get; set; }
        public string postalCode { get; set; }
        public string country { get; set; }
        public bool rsvp { get; set; }
        public string audience { get; set; }
        public bool attended { get; set; }
        public string status { get; set; }
        public string type { get; set; }
        public List<JRegistrantData> data { get; set; }
        public DateTimeOffset dateRegistered { get; set; }
        public DateTimeOffset dateModified { get; set; }

        public JRegistrant()
        {
            data = new List<JRegistrantData>();
            id = 0;
        }
    }

    public class JRegistrantData
    {
        public long id { get; set; }
        public string label { get; set; }
        public string value { get; set; }
        public long componentId { get; set; }
        public string raw { get; set; }

        public JRegistrantData()
        {
            id = 0L;
            componentId = 0L;
            label = null;
            value = null;
            raw = "";
        }
    }

    public class ApiData
    {
        public bool success { get; set; }
        public string message { get; set; }
        public object data { get; set; }

        public ApiData()
        {
            success = false;
            message = "Not Initialized";
        }
    }

    public class JRegistrantAgenda
    {
        public DateTimeOffset start { get; set; }
        public DateTimeOffset end { get; set; }
        public string location { get; set; }
        public string name { get; set; }

        public JRegistrantAgenda()
        {
            start = end = DateTimeOffset.MinValue;
            location = name = "";
        }
    }
}
