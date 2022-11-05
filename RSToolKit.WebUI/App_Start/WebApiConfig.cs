using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace RSToolKit.WebUI
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {

            config.EnableCors();

            #region Charge

            config.Routes.MapHttpRoute(
                name: "getCharge",
                routeTemplate: "api/charge/{id}",
                defaults: new { controller = "charge" }
            );

            config.Routes.MapHttpRoute(
                name: "getCharges",
                routeTemplate: "api/charges/{id}",
                defaults: new { controller = "charges" }
            );


            #endregion

            #region API

            config.Routes.MapHttpRoute(
                name: "contactHeader",
                routeTemplate: "api/contactHeader/{id}",
                defaults: new { controller = "Contacts" }
            );

            config.Routes.MapHttpRoute(
                name: "contactHeaders",
                routeTemplate: "api/contactHeaders",
                defaults: new { controller = "Contacts" }
            );

            config.Routes.MapHttpRoute(
                name: "contact",
                routeTemplate: "api/contact/{id}",
                defaults: new { controller = "Contacts" }
            );

            config.Routes.MapHttpRoute(
                name: "contacts",
                routeTemplate: "api/contacts",
                defaults: new { controller = "Contacts" }
            );

            config.Routes.MapHttpRoute(
                name: "register",
                routeTemplate: "api/register/{id}/{registrantId}/{page}",
                defaults: new { controller = "Register" }
            );

            config.Routes.MapHttpRoute(
                name: "component",
                routeTemplate: "api/component/{id}",
                defaults: new { controller = "Component" }
            );

            config.Routes.MapHttpRoute(
                name: "agenda",
                routeTemplate: "api/registrant/agenda/{id}",
                defaults: new { controller = "Registrant_Agenda" }
            );

            config.Routes.MapHttpRoute(
                name: "registrant",
                routeTemplate: "api/registrant/{id}",
                defaults: new { controller = "Registrant" }
            );

            config.Routes.MapHttpRoute(
                name: "registrations",
                routeTemplate: "api/registrations/{id}",
                defaults: new { controller = "Registrations" }
            );

            config.Routes.MapHttpRoute(
                name: "event",
                routeTemplate: "api/event/{id}",
                defaults: new { controller = "Event" }
            );

            config.Routes.MapHttpRoute(
                name: "events",
                routeTemplate: "api/events/{active}",
                defaults: new { controller = "Events" }
            );

            config.Routes.MapHttpRoute(
                name: "eventstats",
                routeTemplate: "api/eventstats/{id}",
                defaults: new { controller = "EventStats" }
            );

            #endregion

            #region Email

            config.Routes.MapHttpRoute(
                name: "Email_Send",
                routeTemplate: "api/email/send",
                defaults: new { controller = "Email_Send" });

            config.Routes.MapHttpRoute(
                name: "Email_EmailSend",
                routeTemplate: "api/email/sends",
                defaults: new { controller = "Email_EmailSend" });

            config.Routes.MapHttpRoute(
                name: "defaultapi",
                routeTemplate: "api/{controller}",
                defaults: new { controller = "login" }
            );

            #endregion
        }
    }
}