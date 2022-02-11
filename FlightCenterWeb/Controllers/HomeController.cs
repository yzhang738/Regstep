using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data.Objects.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Kendo.Mvc.UI;
using Kendo.Mvc.Extensions;
using FlightCenterData;
using FlightCenterBusiness;
using FlightCenterModels;

namespace FlightCenterWeb.Controllers
{
    public class HomeController : Controller
    {
        SalesBL _salesBL;

        public HomeController()
        {
            _salesBL = new SalesBL(ConfigurationManager.ConnectionStrings["FlightCenterEntities"].ConnectionString);
        }

        public ActionResult Index()
        {
            ViewBag.Message = "Welcome to Flight Center!";

            return View();
        }

        public ActionResult EditPage(string salesId)
        {
            SalesViewModel model = null;

            if (salesId == null)
            {
                model = this._salesBL.GetSalesViewModel();
            }
            else
            {
                model = this._salesBL.GetSalesViewModel(Convert.ToInt32(salesId));
            }

            return View(model);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult GetSuppliers(int type)
        {
            var model = this._salesBL.GetSupplierItems(type);

            return Json(model, JsonRequestBehavior.AllowGet);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult EditPageSubmit(FormCollection collection)
        {
            ViewBag.Message = "<font color='red'>There is some error in your order!</font>";

            var updatedModel = new SalesViewModel();

            if (ModelState.IsValid)
            {
                if (TryUpdateModel(updatedModel, null, null, new[] { "Destinations", "SaleLocations", "Shops", "SourceTypes", "Suppliers" }))
                {
                    updatedModel.CreatedDateTime = DateTime.Now;
                    updatedModel.UpdatedDateTime = DateTime.Now;

                    string destination = this._salesBL.GetDestinationByName(Convert.ToInt32(updatedModel.Destination));
                    string shop = this._salesBL.GetShopByName(Convert.ToInt32(updatedModel.Shop));
                    string saleLocation = this._salesBL.GetSaleLocationByName(Convert.ToInt32(updatedModel.SaleLocation));

                    List<SalesSupplierModel> salesList = new List<SalesSupplierModel>();

                    for (int i = 0; i < Constant.NumberSalesSupplier; i++)
                    {
                        string propertyOfSaleValue = "SaleValue" + i.ToString();
                        var propInfoOfSaleValue = updatedModel.GetType().GetProperty(propertyOfSaleValue);
                        if (propInfoOfSaleValue != null)
                        {
                            decimal? saleValue = (decimal?)propInfoOfSaleValue.GetValue(updatedModel, null);
                            if (saleValue == null)
                                continue;

                            string propertyOfSourceType = "SourceType" + i.ToString();
                            string propValOfSourceType = (string)updatedModel.GetType().GetProperty(propertyOfSourceType).GetValue(updatedModel, null);

                            string propertyOfSupplier = "Supplier" + i.ToString();
                            string propValOfSupplier = (string)updatedModel.GetType().GetProperty(propertyOfSupplier).GetValue(updatedModel, null);

                            this._salesBL.AddToSalesList(salesList, Convert.ToInt32(propValOfSourceType), Convert.ToInt32(propValOfSupplier), saleValue);
                        }
                    }

                    if (salesList.Count == 0)
                    {
                        ViewBag.Message = "<font color='red'>At least one of the Source and Suppliers line must be entered! (You must enter 'Sale Value'.)</font>";
                        return View();
                    }

                    int status = 0;
                    if (updatedModel.SalesId == 0)
                    {
                        status = this._salesBL.AddSalesAndSalesSupplier(
                                    updatedModel.UserId,
                                    shop,
                                    updatedModel.CreatedDateTime,
                                    updatedModel.BookingDate,
                                    updatedModel.PassengerName,
                                    destination,
                                    updatedModel.Deposit,
                                    saleLocation,
                                    updatedModel.NumberPassengers,
                                    updatedModel.Comments,
                                    salesList);

                        if (status > 0)
                            ViewBag.Message = "<font color='blue'>Your order has been entered successfully!</font>";
                    }
                    else
                    {
                        status = this._salesBL.UpdateSalesAndSalesSupplier(
                                    updatedModel.SalesId,
                                    updatedModel.UserId,
                                    shop,
                                    updatedModel.UpdatedDateTime,
                                    updatedModel.BookingDate,
                                    updatedModel.PassengerName,
                                    destination,
                                    updatedModel.Deposit,
                                    saleLocation,
                                    updatedModel.NumberPassengers,
                                    updatedModel.Comments,
                                    salesList);

                        if (status > 0)
                            ViewBag.Message = "<font color='blue'>Your order has been updated successfully!</font>";
                    }
                }
                else
                {
                    ViewBag.Message = "<font color='red'>" + string.Join("<br/>", ModelState.Values
                                            .SelectMany(v => v.Errors)
                                            .Select(e => e.ErrorMessage)) + "</font>";

                    //return View("~/Views/Home/EditPage.aspx");
                }
            }

            return View();
        }

        public ActionResult ListSales()
        {
            return View();
        }

        public ActionResult SaleRecordsGridAjax([DataSourceRequest]DataSourceRequest request)
        {
            List<SalesViewModel> model = this._salesBL.GetSalesGridViewModel();
 
            DataSourceResult result = model.ToDataSourceResult(request);

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult InsertSaleRecord(DataSourceRequest request, string id)
        {
            return SaleRecordsGridAjax(request);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult UpdateSaleRecord(DataSourceRequest request, string id)
        {
            return SaleRecordsGridAjax(request);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult DeleteSaleRecord(DataSourceRequest request, string id)
        {
            this._salesBL.DeleteSalesAndSalesSupplier(Convert.ToInt32(id));

            return SaleRecordsGridAjax(request);
        }

    }
}
