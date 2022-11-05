using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace RSToolKit.WebUI
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            #region Permissions
            routes.MapRoute(
                name: "Permissions - Get",
                url: "permission/get/{id}/{type}",
                defaults: new { controller = "permission", action = "get" }
            );
            #endregion

            #region Unsubscribe

            routes.MapRoute(
                name: "UnsubscribeController",
                url: "Unsubscribe/{action}/{id}/{emailSend}",
                defaults: new { controller = "Unsubscribe", action = "Form" }
            );

            #endregion

            #region FormEmailReport

            routes.MapRoute(
                name: "FormEmailReportController - get",
                url: "formemailreport/get/{id}/{page}",
                defaults: new { controller = "formemailreport", action = "get", page = UrlParameter.Optional }
            );

            #endregion

            #region Company

            routes.MapRoute(
                name: "CompanyController - Logo",
                url: "company/logo/{id}",
                defaults: new { controller = "company", action = "logo" }
            );

            routes.MapRoute(
                name: "CompanyController - Logo Thumbnail",
                url: "company/logo/thumbnail/{id}/{width}/{height}",
                defaults: new { controller = "company", action ="logothumbnail", width = UrlParameter.Optional, height = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "CompanyController - Update",
                url: "company/update",
                defaults: new { controller = "company", action = "update" }
            );

            routes.MapRoute(
                name: "CompanyController - Delete",
                url: "company/delete",
                defaults: new { controller = "company", action = "delete" }
            );

            routes.MapRoute(
                name: "CompanyController - New",
                url: "company/new",
                defaults: new { controller = "company", action = "new" }
            );

            routes.MapRoute(
                name: "CompanyController - Get",
                url: "company/{id}",
                defaults: new { controller = "company", action = "get" }
            );

            routes.MapRoute(
                name: "CompanyController - List",
                url: "companies",
                defaults: new { controller = "company", action = "list" }
            );

            #endregion

            #region Registrants Controller

            routes.MapRoute(
                name: "RegistrantController - Data",
                url: "Registrant/Data/{id}/{component}/{width}",
                defaults: new
                {
                    controller = "Registrant",
                    action = "Data",
                    width = UrlParameter.Optional
                }
            );

            routes.MapRoute(
                name: "RegistrantController - default",
                url: "Registrant/{action}/{id}",
                defaults: new
                {
                    controller = "Registrant",
                    action = "Get"
                }
            );
            
            #endregion

            #region Transaction Controller

            routes.MapRoute(
                name: "TransactionController-default",
                url: "Transaction/{action}/{id}",
                defaults: new
                {
                    controller = "Transaction",
                    action = "Get",
                }
            );

            #endregion

            #region Email Controller

            routes.MapRoute(
                name: "EmailController-Events",
                url: "Email/Events/{id}",
                defaults: new
                {
                    controller = "Email",
                    action = "Events"
                }
            );

            #endregion

            #region Email Events

            routes.MapRoute(
                name: "emailClick",
                url: "emailclick/{id}/{aId}",
                defaults: new { controller = "EmailEvent", action = "Click" }
            );

            routes.MapRoute(
                name: "emailOpen",
                url: "emailopen/{id}",
                defaults: new { controller = "EmailEvent", action = "Open" }
            );

            #endregion

            #region EmailInformation

            routes.MapRoute(
                name: "EmailInformation",
                url: "EmailInformation/{action}/{id}",
                defaults: new { controller = "EmailInformation", action = "open", id = Guid.Empty }
            );

            #endregion

            #region FormReport Controller

            routes.MapRoute(
                name: "FormReport - DownloadReport",
                url: "FormReport/Download/{token}",
                defaults: new
                {
                    controller = "FormReport",
                    action = "Download"
                }
            );

            routes.MapRoute(
                name: "FormReport - default",
                url: "FormReport/{action}/{id}/{page}",
                defaults: new
                {
                    controller = "FormReport",
                    action = "Get"
                }
            );

            #endregion

            #region Contact Controller
            routes.MapRoute(
                name: "ContactController - List",
                url: "Contact/List/{id}/{page}",
                defaults: new { controller = "Contact", action = "List", page = UrlParameter.Optional }
            );
            #endregion

            #region AdminRegistration Controller
            routes.MapRoute(
                name: "AdminRegister - Start",
                url: "AdminRegister/Start/{id}/{email}",
                defaults: new { controller = "AdminRegister", action = "Start", email = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "AdminRegister - Page",
                url: "AdminRegister/Page/{id}/{page}",
                defaults: new { controller = "AdminRegister", action = "Page" }
            );

            routes.MapRoute(
                name: "AdminRegister - Default",
                url: "AdminRegister/{action}/{id}",
                defaults: new { controller = "AdminRegister", action = "Start" }
            );
            #endregion

            #region Unsorted

            #region Register

            routes.MapRoute(
                name: "CustomLogin",
                url: "Register/CustomLogin/{formId}/{email}/{live}",
                defaults: new {  controller = "Register", action = "CustomLogin" }
            );

            routes.MapRoute(
                name: "RegisterShowConfirmation",
                url: "Register/ShowConfirmation/{RegistrantKey}",
                defaults: new { controller = "Register", action = "ShowConfirmation" }
            );

            routes.MapRoute(
                name: "SmartLink",
                url: "Register/SmartLink/{formId}/{confNumb}",
                defaults: new { controller = "Register", action = "SmartLink" }
            );

            #endregion

            #region AdminRegister

            routes.MapRoute(
                name: "AdminRegStart",
                url: "AdminRegister/Start/{formKey}",
                defaults: new { controller = "AdminRegister", action = "Start" }
            );

            routes.MapRoute(
                name: "AdminRegNext",
                url: "AdminRegister/Next/{registrantKey}/{pageNumber}",
                defaults: new { controller = "AdminRegister", action = "Next" }
            );

            routes.MapRoute(
                name: "AdminRegConfirmation",
                url: "AdminRegister/Confirmation/{registrantKey}",
                defaults: new { controller = "AdminRegister", action = "Confirmation" }
            );

            routes.MapRoute(
                name: "AdminRegShowConfirmation",
                url: "AdminRegister/ShowConfirmation/{registrantKey}",
                defaults: new { controller = "AdminRegister", action = "ShowConfirmation" }
            );


            routes.MapRoute(
                name: "AdminRegPromo",
                url: "AdminRegister/PromotionCodes/{registrantKey}",
                defaults: new { controller = "AdminRegister", action = "PromotionCodes" }
            );

            routes.MapRoute(
                name: "AdminRegShop",
                url: "AdminRegister/ShoppingCart/{registrantKey}",
                defaults: new { controller = "AdminRegister", action = "ShoppingCart" }
            );

            routes.MapRoute(
                name: "AdminRegCreditCard",
                url: "AdminRegister/CreditCard/{registrantKey}",
                defaults: new { controller = "AdminRegister", action = "CreditCard" }
            );

            routes.MapRoute(
                name: "AdminRegBillMe",
                url: "AdminRegister/BillMe/{registrantKey}",
                defaults: new { controller = "AdminRegister", action = "BillMe" }
            );

            #endregion

            #region Cloud

            routes.MapRoute(
                name: "GetContacts",
                url: "Cloud/Contacts/{id}",
                defaults: new { controller = "Cloud", action = "Contacts" }
            );

            routes.MapRoute(
                name: "DownloadReport",
                url: "Cloud/Download/Report/{id}/{remove}",
                defaults: new { controller = "Cloud", action = "DownloadReport", remove = true }
            );

            routes.MapRoute(
                name: "CreateFormReport",
                url: "Cloud/Create/Report",
                defaults: new { controller = "Cloud", action = "CreateReport" }
            );


            routes.MapRoute(
                name: "CreateSurveyReport",
                url: "Cloud/Create/SurveyReport",
                defaults: new { controller = "Cloud", action = "CreateSurveyReport" }
            );

            routes.MapRoute(
                name: "FormReportsJson",
                url: "Cloud/GetFormReports/{page}/{pageSize}/{type}",
                defaults: new { controller = "Cloud", action = "GetFormReports", page = 1, pageSize = 15, type = "all" }
            );

            routes.MapRoute(
                name: "SingleFormReportData",
                url: "Cloud/SingleFormReportData/{id}/{page}/{pageSize}",
                defaults: new { controller = "Cloud", action = "SingleFormReportData", page = 1, pageSize = 15 }
            );


            routes.MapRoute(
                name: "Unsubscribe",
                url: "Unsubscribe/Index",
                defaults: new { controller = "Unsubscribe", action = "Index" }
            );


            #endregion

            #region FormBuilder

            routes.MapRoute(
                name: "Items",
                url: "FormBuilder/GetComponentItems/{id}",
                defaults: new { controller = "FormBuilder", action = "GetComponentItems", id = "" }
            );

            routes.MapRoute(
                name: "PageLogics",
                url: "FormBuilder/PageLogics/Page-{id}/PageLoad-{pageLoad}",
                defaults: new { controller = "FormBuilder", action = "PageLogics", id = "", pageLoad = true }
            );

            routes.MapRoute(
                name: "NewLogic",
                url: "FormBuilder/NewLogic/Page-{id}/PageLoad-{onLoad}",
                defaults: new { controller = "FormBuilder", action = "NewLogic", id = "", onLoad = true }
            );

            routes.MapRoute(
                name: "NewInput",
                url: "FormBuilder/NewInput/Panel-{id}/Type-{type}",
                defaults: new { controller = "FormBuilder", action = "NewInput" }
            );

            routes.MapRoute(
                name: "NewDropdown",
                url: "FormBuilder/NewDropdown/{Panel}-{id}/Type-{type}",
                defaults: new { controller = "FormBuilder", action = "NewDropdown" }
            );

            routes.MapRoute(
                name: "NewRadioGroup",
                url: "FormBuilder/NewRadioGroup/{Panel}-{id}/Type-{type}",
                defaults: new { controller = "FormBuilder", action = "NewRadioGroup" }
            );

            routes.MapRoute(
                name: "NewCheckboxGroup",
                url: "FormBuilder/NewCheckboxGroup/{Panel}-{id}/Type-{type}",
                defaults: new { controller = "FormBuilder", action = "NewCheckboxGroup" }
            );

            routes.MapRoute(
                name: "NewFreeText",
                url: "FormBuilder/NewFreeText/{Panel}-{id}/Type-{type}",
                defaults: new { controller = "FormBuilder", action = "NewFreeText" }
            );

            #endregion

            #region Account

            //Email Confirmation
            routes.MapRoute(
                name: "EmailConfirmation",
                url: "Account/ConfirmEmail/{userId}/{validationToken}",
                defaults: new { controller = "Account", action = "ConfirmEmail" }
            );

            #endregion

            #region Email

            routes.MapRoute(
                name: "New Email - No Template",
                url: "Email/NewEmail/Holder-{id}/Template-{emailTemplate}",
                defaults: new { controller = "Email", action = "NewEmail" }
            );

            routes.MapRoute(
                name: "New Email",
                url: "Email/NewEmail/Holder-{id}",
                defaults: new { controller = "Email", action = "NewEmail" }
            );


            #endregion

            routes.MapRoute(
                name: "Error",
                url: "Error/{action}/{id}",
                defaults: new { controller = "Error", action = "Error500", id = 1L }
            );

            #endregion

            // Default Route
            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Cloud", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
