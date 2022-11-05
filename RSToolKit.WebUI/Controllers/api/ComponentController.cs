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
using RSToolKit.WebUI.Infrastructure;
using RSToolKit.Domain.Engines;
using RSToolKit.Domain.JItems;
using RSToolKit.Domain.Entities.Components;

namespace RSToolKit.WebUI.Controllers.API
{
    public class ComponentController : AuthApiController
    {
        [ApiTokenAttendeeAuthorization]
        public IHttpActionResult Get(long id)
        {
            using (var context = new EFDbContext())
            using (var repository = new FormsRepository(context, User, Principal))
            {
                IComponent component = null;
                component = repository.Search<IComponent>(c => c.SortingId == id).FirstOrDefault();
                if (component == null)
                    return NotFound();
                var r_comp = new JComponent();
                r_comp.id = component.SortingId;
                r_comp.label = component.LabelText;
                r_comp.variable = component.Variable != null ? component.Variable.Value : null;
                if (component is IComponentItemParent)
                {
                    r_comp.type = "singleSelection";
                    if (component is IComponentMultipleSelection)
                        r_comp.type = "multipleSelection";
                    var t_items = new List<JComponentItem>();
                    foreach (var item in (component as IComponentItemParent).Children.OrderBy(i => i.Order))
                    {
                        var skip = false;
                        if (Registrant != null)
                        {
                            var t_test = TestSkip(repository, component, Registrant);
                            skip = t_test.Skip || t_test.Hide;
                        }
                        if (!skip)
                            t_items.Add(new JComponentItem() { id = item.SortingId, label = item.LabelText });                      
                    }
                    r_comp.items = t_items;
                }
                else if (component is RSToolKit.Domain.Entities.Components.Input)
                {
                    var input = component as RSToolKit.Domain.Entities.Components.Input;
                    switch (input.Type)
                    {
                        case InputType.Date:
                            r_comp.type = "date";
                            break;
                        case InputType.DateTime:
                            r_comp.type = "datetime";
                            break;
                        case InputType.Time:
                            r_comp.type = "time";
                            break;
                        case InputType.File:
                            r_comp.type = "file";
                            break;
                        default:
                            switch (input.ValueType)
                            {
                                case Domain.Entities.Components.ValueType.ZipCode:
                                    r_comp.type = "postalCodeorProvince";
                                    break;
                                case Domain.Entities.Components.ValueType.USPhone:
                                    r_comp.type = "usPhone";
                                    break;
                                case Domain.Entities.Components.ValueType.Number:
                                    r_comp.type = "number";
                                    break;
                                case Domain.Entities.Components.ValueType.Email:
                                    r_comp.type = "email";
                                    break;
                                case Domain.Entities.Components.ValueType.Decimal:
                                    r_comp.type = "money";
                                    break;
                                default:
                                    r_comp.type = "input";
                                    break;
                            }
                            break;
                    }
                }
                else
                {
                    r_comp.type = "input";
                }
                return Ok(new ApiData() { success = true, message = "Found component.", data = r_comp });
            }
        }
    }

    public class JComponent
    {
        public long id { get; set; }
        public string label { get; set; }
        public string type { get; set; }
        public string variable { get; set; }
        public IEnumerable<JComponentItem> items { get; set; }
        public string currentValue { get; set; }
        public int order { get; set; }
        public bool disabled { get; set; }

        public JComponent()
        {
            id = -1;
            label = "";
            type = "";
            variable = "";
            items = new List<JComponentItem>();
            currentValue = null;
            disabled = false;
        }
    }

    public class JComponentItem
    {
        public long id { get; set; }
        public string label { get; set; }
        public bool disabled { get; set; }
        public int order { get; set; }
        public JComponentItemWaitlisting waiting { get; set; }

        public JComponentItem()
        {
            id = -1;
            label = "";
            disabled = false;
            order = 0;
            waiting = null;
        }
    }

    public class JComponentItemWaitlisting
    {
        public string label { get; set; }
        public bool waiting { get; set; }
    }
}
