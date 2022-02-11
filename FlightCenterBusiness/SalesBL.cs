using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.Data.Objects.SqlClient;
using System.Linq;
using System.Text;
using System.Transactions;
using System.Web.Mvc;
using Kendo.Mvc.UI;
using FlightCenterData;
using FlightCenterModels;

namespace FlightCenterBusiness
{
    public class SalesBL
    {
        FlightCenterEntities _repository;

        public SalesBL(string connectionString)
        {
            _repository = new FlightCenterEntities(connectionString);
        }

        public SalesViewModel GetSalesViewModel()
        {
            SalesViewModel model = new SalesViewModel();

            var destinations = (from r in this._repository.Destinations
                                select new SelectListItem
                                {
                                    Text = r.Name,
                                    Value = SqlFunctions.StringConvert((double)r.DestinationID),
                                    Selected = false
                                });
            model.Destinations = destinations.OrderBy(c => c.Text).ToList();

            SetSelectedItem(model.Destinations, "Toronto");

            var saleLocations = (from r in this._repository.SaleLocations
                                 select new SelectListItem
                                 {
                                     Text = r.Name,
                                     Value = SqlFunctions.StringConvert((double)r.SaleLocationID)
                                 });
            model.SaleLocations = saleLocations.OrderBy(c => c.Text).ToList();

            SetSelectedItem(model.SaleLocations, "In Shop");

            var shops = (from r in this._repository.Shops
                         select new SelectListItem
                         {
                             Text = r.Name,
                             Value = SqlFunctions.StringConvert((double)r.ShopID)
                         });
            model.Shops = shops.OrderBy(c => c.Text).ToList();

            SetSelectedItem(model.Shops, "Bloor West Village");

            model.SourceTypes0 = GetSourceTypeItems();
            model.SourceTypes1 = GetSourceTypeItems();
            model.SourceTypes2 = GetSourceTypeItems();
            model.SourceTypes3 = GetSourceTypeItems();
            model.SourceTypes4 = GetSourceTypeItems();
            model.SourceTypes5 = GetSourceTypeItems();
            model.SourceTypes6 = GetSourceTypeItems();
            model.SourceTypes7 = GetSourceTypeItems();
            model.SourceTypes8 = GetSourceTypeItems();
            model.SourceTypes9 = GetSourceTypeItems();

            SetSelectedItem(model.SourceTypes0, "Air");
            SetSelectedItem(model.SourceTypes1, "Cruise");
            SetSelectedItem(model.SourceTypes2, "Insurance");
            SetSelectedItem(model.SourceTypes3, "Land");

            string selectSource = "Select a Source";
            model.SourceType4_Init = selectSource;
            model.SourceType5_Init = selectSource;
            model.SourceType6_Init = selectSource;
            model.SourceType7_Init = selectSource;
            model.SourceType8_Init = selectSource;
            model.SourceType9_Init = selectSource;

            model.Suppliers0 = GetSupplierItems("Air");
            model.Suppliers1 = GetSupplierItems("Cruise");
            model.Suppliers2 = GetSupplierItems("Insurance");
            model.Suppliers3 = GetSupplierItems("Land");
            model.Suppliers4 = GetSupplierItems("Air");
            model.Suppliers5 = GetSupplierItems("Air");
            model.Suppliers6 = GetSupplierItems("Air");
            model.Suppliers7 = GetSupplierItems("Air");
            model.Suppliers8 = GetSupplierItems("Air");
            model.Suppliers9 = GetSupplierItems("Air");

            string selectSupplier = "Select a Supplier";
            model.Supplier4_Init = selectSupplier;
            model.Supplier5_Init = selectSupplier;
            model.Supplier6_Init = selectSupplier;
            model.Supplier7_Init = selectSupplier;
            model.Supplier8_Init = selectSupplier;
            model.Supplier9_Init = selectSupplier;

            return model;
        }

        public SalesViewModel GetSalesViewModel(int salesId)
        {
            SalesViewModel model = (from r in this._repository.Sales
                                      where r.SalesID == salesId && r.IsDeleted == false
                                      select new SalesViewModel
                                      {
                                          SalesId = salesId,
                                          PassengerName = r.PassengerName,
                                          UserId = r.UserID,
                                          Deposit = r.Deposit,
                                          NumberPassengers = r.Passengers,
                                          BookingDate = r.BookingDate,
                                          Destination_Init = r.Destination,
                                          Shop_Init = r.Shop,
                                          SaleLocation_Init = r.SaleLocation, 
                                          Comments = r.Comments
                                      }).SingleOrDefault();

            var destinations = (from r in this._repository.Destinations
                                select new SelectListItem
                                {
                                    Text = r.Name,
                                    Value = SqlFunctions.StringConvert((double)r.DestinationID),
                                    Selected = false
                                });
            model.Destinations = destinations.OrderBy(c => c.Text).ToList();

            SetSelectedItem(model.Destinations, model.Destination_Init);

            model.SaleLocations = (from r in this._repository.SaleLocations
                                 select new SelectListItem
                                 {
                                     Text = r.Name,
                                     Value = SqlFunctions.StringConvert((double)r.SaleLocationID)
                                 }).OrderBy(c => c.Text).ToList();

            SetSelectedItem(model.SaleLocations, model.SaleLocation_Init);

            model.Shops = (from r in this._repository.Shops
                         select new SelectListItem
                         {
                             Text = r.Name,
                             Value = SqlFunctions.StringConvert((double)r.ShopID)
                         }).OrderBy(c => c.Text).ToList();

            SetSelectedItem(model.Shops, model.Shop_Init);

            List<SalesSupplierModel> salesSuppliers = (from r in this._repository.SalesSuppliers
                                                       where r.SalesID == salesId && r.IsDeleted == false
                                                       select new SalesSupplierModel
                                                       {
                                                          SourceTypeName = r.SourceType,
                                                          SupplierName = r.Supplier,
                                                          SaleValue = r.SaleValue
                                                       }).ToList();

            model.SourceTypes0 = GetSourceTypeItems();
            model.SourceTypes1 = GetSourceTypeItems();
            model.SourceTypes2 = GetSourceTypeItems();
            model.SourceTypes3 = GetSourceTypeItems();
            model.SourceTypes4 = GetSourceTypeItems();
            model.SourceTypes5 = GetSourceTypeItems();
            model.SourceTypes6 = GetSourceTypeItems();
            model.SourceTypes7 = GetSourceTypeItems();
            model.SourceTypes8 = GetSourceTypeItems();
            model.SourceTypes9 = GetSourceTypeItems();

            SetSelectedItem(model.SourceTypes0, "Air");
            SetSelectedItem(model.SourceTypes1, "Cruise");
            SetSelectedItem(model.SourceTypes2, "Insurance");
            SetSelectedItem(model.SourceTypes3, "Land");

            string selectSource = "Select a Source";
            model.SourceType4_Init = selectSource;
            model.SourceType5_Init = selectSource;
            model.SourceType6_Init = selectSource;
            model.SourceType7_Init = selectSource;
            model.SourceType8_Init = selectSource;
            model.SourceType9_Init = selectSource;

            model.Suppliers0 = GetSupplierItems("Air");
            model.Suppliers1 = GetSupplierItems("Cruise");
            model.Suppliers2 = GetSupplierItems("Insurance");
            model.Suppliers3 = GetSupplierItems("Land");
            model.Suppliers4 = GetSupplierItems("Air");
            model.Suppliers5 = GetSupplierItems("Air");
            model.Suppliers6 = GetSupplierItems("Air");
            model.Suppliers7 = GetSupplierItems("Air");
            model.Suppliers8 = GetSupplierItems("Air");
            model.Suppliers9 = GetSupplierItems("Air");

            string selectSupplier = "Select a Supplier";
            model.Supplier4_Init = selectSupplier;
            model.Supplier5_Init = selectSupplier;
            model.Supplier6_Init = selectSupplier;
            model.Supplier7_Init = selectSupplier;
            model.Supplier8_Init = selectSupplier;
            model.Supplier9_Init = selectSupplier;

            for (int i = 0; i < salesSuppliers.Count; i++)
            {
                //string propertyName = "SourceType" + i.ToString() + "_Init";
                //var propInfo = model.GetType().GetProperty(propertyName);
                //if (propInfo != null)
                //{
                //    propInfo.SetValue(model, salesSuppliers[i].SourceTypeName, null);
                //}

                string propertyName = "SourceTypes" + i.ToString();
                var propInfo = model.GetType().GetProperty(propertyName);
                if (propInfo != null)
                {
                    List<SelectListItem> sourceList = (List<SelectListItem>)propInfo.GetValue(model, null);
                    ClearAndSetSelectedItem(sourceList, salesSuppliers[i].SourceTypeName);
                }

                propertyName = "Suppliers" + i.ToString();
                propInfo = model.GetType().GetProperty(propertyName);
                if (propInfo != null)
                {
                    List<SelectListItem> supplierList = null;
                    if (salesSuppliers[i].SourceTypeName != "Air")
                    {
                        supplierList = GetSupplierItems(salesSuppliers[i].SourceTypeName);
                        SetSelectedItem(supplierList, salesSuppliers[i].SupplierName);
                        propInfo.SetValue(model, supplierList, null);
                    }
                    else
                    {
                        supplierList = (List<SelectListItem>)propInfo.GetValue(model, null);
                        ClearAndSetSelectedItem(supplierList, salesSuppliers[i].SupplierName);
                    }
                }

                propertyName = "SaleValue" + i.ToString();
                propInfo = model.GetType().GetProperty(propertyName);
                if (propInfo != null)
                {
                    propInfo.SetValue(model, (decimal)salesSuppliers[i].SaleValue, null);
                }
            }

            return model;
        }

        public void SetSelectedItem(List<SelectListItem> list, string match)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].Text == match)
                {
                    list[i].Selected = true;
                    break;
                }
            }
        }

        public void ClearAndSetSelectedItem(List<SelectListItem> list, string match)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].Text == match)
                {
                    list[i].Selected = true;
                }
                else
                {
                    list[i].Selected = false;
                }
            }
        }

        public List<SelectListItem> GetSourceTypeItems()
        {
            return (from r in this._repository.SourceTypes
                    select new SelectListItem
                    {
                        Text = r.Name,
                        Value = SqlFunctions.StringConvert((double)r.SourceTypeID),
                        Selected = false
                    }).OrderBy(c=>c.Text).ToList();
        }

        public List<SelectListItem> GetSupplierItems(string name)
        {
            List<SelectListItem> suppliers = null;
            int type = GetSourceTypeById(name);

            switch (type)
            {
                case 1:
                    suppliers = (from r in this._repository.Airlines
                                 select new SelectListItem
                                 {
                                     Text = r.Name,
                                     Value = SqlFunctions.StringConvert((double)r.AirlineID),
                                     Selected = false
                                 }).OrderBy(c => c.Text).ToList();
                    break;
                case 2:
                    suppliers = (from r in this._repository.LandSuppliers
                                 select new SelectListItem
                                 {
                                     Text = r.Name,
                                     Value = SqlFunctions.StringConvert((double)r.LandSupplierID),
                                     Selected = false
                                 }).OrderBy(c => c.Text).ToList();
                    break;
                case 3:
                    suppliers = (from r in this._repository.CruiseSuppliers
                                 select new SelectListItem
                                 {
                                     Text = r.Name,
                                     Value = SqlFunctions.StringConvert((double)r.CruiseSupplierID),
                                     Selected = false
                                 }).OrderBy(c => c.Text).ToList();
                    break;
                case 4:
                    suppliers = (from r in this._repository.InsuranceSuppliers
                                 select new SelectListItem
                                 {
                                     Text = r.Name,
                                     Value = SqlFunctions.StringConvert((double)r.InsuranceSupplierID),
                                     Selected = false
                                 }).OrderBy(c => c.Text).ToList();
                    break;
                default:
                    suppliers = (from r in this._repository.Airlines
                                 select new SelectListItem
                                 {
                                     Text = r.Name,
                                     Value = SqlFunctions.StringConvert((double)r.AirlineID),
                                     Selected = false
                                 }).OrderBy(c => c.Text).ToList();
                    break;
            }

            return suppliers;
        }

        public List<SelectListItem> GetSupplierItems(int type)
        {
            List<SelectListItem> suppliers = null;

            switch (type)
            {
                case 1:
                    suppliers = (from r in this._repository.Airlines
                                 select new SelectListItem
                                 {
                                     Text = r.Name,
                                     Value = SqlFunctions.StringConvert((double)r.AirlineID),
                                     Selected = false
                                 }).OrderBy(c => c.Text).ToList();
                    break;
                case 2:
                    suppliers = (from r in this._repository.LandSuppliers
                                 select new SelectListItem
                                 {
                                     Text = r.Name,
                                     Value = SqlFunctions.StringConvert((double)r.LandSupplierID),
                                     Selected = false
                                 }).OrderBy(c => c.Text).ToList();
                    break;
                case 3:
                    suppliers = (from r in this._repository.CruiseSuppliers
                                 select new SelectListItem
                                 {
                                     Text = r.Name,
                                     Value = SqlFunctions.StringConvert((double)r.CruiseSupplierID),
                                     Selected = false
                                 }).OrderBy(c => c.Text).ToList();
                    break;
                case 4:
                    suppliers = (from r in this._repository.InsuranceSuppliers
                                 select new SelectListItem
                                 {
                                     Text = r.Name,
                                     Value = SqlFunctions.StringConvert((double)r.InsuranceSupplierID),
                                     Selected = false
                                 }).OrderBy(c => c.Text).ToList();
                    break;
                default:
                    suppliers = (from r in this._repository.Airlines
                                 select new SelectListItem
                                 {
                                     Text = r.Name,
                                     Value = SqlFunctions.StringConvert((double)r.AirlineID),
                                     Selected = false
                                 }).OrderBy(c => c.Text).ToList();
                    break;
            }

            return suppliers;
        }

        public string GetSourceTypeByName(int sourceType)
        {
            return (from r in this._repository.SourceTypes
                    where r.SourceTypeID == sourceType
                    select r.Name
                    ).FirstOrDefault();
        }

        public int GetSourceTypeById(string sourceTypeName)
        {
            return (from r in this._repository.SourceTypes
                    where r.Name == sourceTypeName
                    select r.SourceTypeID
                    ).FirstOrDefault();
        }

        public string GetSupplierByName(int sourceType, int supplierType)
        {
            string supplierName = string.Empty;

            switch (sourceType)
            {
                case 1:
                    supplierName = (from r in this._repository.Airlines
                                    where r.AirlineID == supplierType
                                    select r.Name
                                    ).FirstOrDefault();
                    break;
                case 2:
                    supplierName = (from r in this._repository.LandSuppliers
                                    where r.LandSupplierID == supplierType
                                    select r.Name
                                    ).FirstOrDefault();
                    break;
                case 3:
                    supplierName = (from r in this._repository.CruiseSuppliers
                                    where r.CruiseSupplierID == supplierType
                                    select r.Name
                                    ).FirstOrDefault();
                    break;
                case 4:
                    supplierName = (from r in this._repository.InsuranceSuppliers
                                    where r.InsuranceSupplierID == supplierType
                                    select r.Name
                                    ).FirstOrDefault();
                    break;
                default:
                    supplierName = (from r in this._repository.Airlines
                                    where r.AirlineID == supplierType
                                    select r.Name
                                    ).FirstOrDefault();
                    break;
            }

            return supplierName;
        }

        public int GetSupplierById(int sourceType, string supplierName)
        {
            int supplierType = 0;

            switch (sourceType)
            {
                case 1:
                    supplierType = (from r in this._repository.Airlines
                                    where r.Name == supplierName
                                    select r.AirlineID
                                    ).FirstOrDefault();
                    break;
                case 2:
                    supplierType = (from r in this._repository.LandSuppliers
                                    where r.Name == supplierName
                                    select r.LandSupplierID
                                    ).FirstOrDefault();
                    break;
                case 3:
                    supplierType = (from r in this._repository.CruiseSuppliers
                                    where r.Name == supplierName
                                    select r.CruiseSupplierID
                                    ).FirstOrDefault();
                    break;
                case 4:
                    supplierType = (from r in this._repository.InsuranceSuppliers
                                    where r.Name == supplierName
                                    select r.InsuranceSupplierID
                                    ).FirstOrDefault();
                    break;
                default:
                    supplierType = (from r in this._repository.Airlines
                                    where r.Name == supplierName
                                    select r.AirlineID
                                    ).FirstOrDefault();
                    break;
            }

            return supplierType;
        }

        public void AddToSalesList(List<SalesSupplierModel> list, int sourceType, int supplierType, decimal? saleValue)
        {
            list.Add(new SalesSupplierModel
                    {
                        SourceTypeName = GetSourceTypeByName(sourceType), 
                        SupplierName = GetSupplierByName(sourceType, supplierType),
                        SaleValue = saleValue
                    });  
        }

        public string GetDestinationByName(int id)
        {
            return (from r in this._repository.Destinations
                    where r.DestinationID == id
                    select r.Name
                    ).FirstOrDefault();
        }

        public string GetSaleLocationByName(int id)
        {
            return (from r in this._repository.SaleLocations
                    where r.SaleLocationID == id
                    select r.Name
                    ).FirstOrDefault();
        }

        public string GetShopByName(int id)
        {
            return (from r in this._repository.Shops
                    where r.ShopID == id
                    select r.Name
                    ).FirstOrDefault();
        }

        public void SetSalesViewBag(dynamic viewBag)
        {
            var destinations = (from r in this._repository.Destinations
                                select new SelectListItem
                                {
                                    Text = r.Name,
                                    Value = SqlFunctions.StringConvert((double)r.DestinationID)
                                });
            viewBag.Destinations = destinations.OrderBy(c => c.Text).ToList();

            var saleLocations = (from r in this._repository.SaleLocations
                                 select new SelectListItem
                                 {
                                     Text = r.Name,
                                     Value = SqlFunctions.StringConvert((double)r.SaleLocationID)
                                 });
            viewBag.SaleLocations = saleLocations.OrderBy(c => c.Text).ToList();

            var sourceTypes = (from r in this._repository.SourceTypes
                               select new SelectListItem
                               {
                                   Text = r.Name,
                                   Value = SqlFunctions.StringConvert((double)r.SourceTypeID)
                               });
            viewBag.SourceTypes = sourceTypes.OrderBy(c => c.Text).ToList();

            var shops = (from r in this._repository.Shops
                         select new SelectListItem
                         {
                             Text = r.Name,
                             Value = SqlFunctions.StringConvert((double)r.ShopID)
                         });
            viewBag.Shops = shops.OrderBy(c => c.Text).ToList();

            var suppliers = (from r in this._repository.Airlines
                             select new SelectListItem
                             {
                                 Text = r.Name,
                                 Value = SqlFunctions.StringConvert((double)r.AirlineID)
                             });
            viewBag.Suppliers = suppliers.OrderBy(c => c.Text).ToList();
        }

        public List<SalesViewModel> GetSalesGridViewModel()
        {
            var modelQuery = (from r in this._repository.Sales
                              where r.IsDeleted == false
                              select new SalesViewModel
                              {
                                  SalesId = (int)r.SalesID,
                                  UserId = r.UserID,
                                  Shop = r.Shop,
                                  CreatedDateTime = r.CreatedDateTime,
                                  BookingDate = r.BookingDate,
                                  PassengerName = r.PassengerName,
                                  Invoice = (from s in this._repository.SalesSuppliers
                                             where s.SalesID == r.SalesID && s.IsDeleted == false
                                             select s.SaleValue).Sum(),
                                  Destination = r.Destination,
                                  Deposit = r.Deposit,
                                  NumberPassengers = r.Passengers,
                                  SaleLocation = r.SaleLocation,
                                  Comments = r.Comments
                              }).OrderBy(x => x.CreatedDateTime);

            return modelQuery.ToList();
        }

        //public List<SalesViewModel> GetSalesGridViewModel(DataSourceRequest request)
        //{
        //    var modelQuery = (from r in this._repository.Sales
        //                      where r.IsDeleted == false
        //                      select new SalesViewModel
        //                      {
        //                          SalesId = (int)r.SalesID,
        //                          UserId = r.UserID,
        //                          Shop = r.Shop,
        //                          CreatedDateTime = r.CreatedDateTime,
        //                          BookingDate = r.BookingDate,
        //                          PassengerName = r.PassengerName,
        //                          Destination = r.Destination,
        //                          Deposit = r.Deposit,
        //                          NumberPassengers = r.Passengers,
        //                          SaleLocation = r.SaleLocation,
        //                          Comments = r.Comments
        //                      });

        //    // Apply filtering from the UI
        //    if (request.Filters != null)
        //    {
        //        if (request.Filters.Any())
        //        {
        //            modelQuery = FilterSalesGrid(modelQuery, request.Filters);
        //            //modelQuery = Common.FilterMembers<SalesViewModel>(modelQuery, request.Filters);
        //        }
        //    }

        //    // Apply sorting from the UI
        //    if (request.Sorts == null)
        //    {
        //        // Apply the default sorting
        //        modelQuery = modelQuery.OrderBy(x => x.CreatedDateTime).ThenBy(x => x.PassengerName);
        //    }
        //    else if (request.Sorts.Any())
        //    {
        //        modelQuery = SortSalesGrid(modelQuery, request.Sorts);
        //        //modelQuery = Common.SortMembers<SalesViewModel>(modelQuery, request.Sorts);
        //    }
        //    else
        //    {
        //        // Apply the default sorting
        //        modelQuery = modelQuery.OrderBy(x => x.CreatedDateTime).ThenBy(x => x.PassengerName);
        //    }

        //    List<SalesViewModel> model = modelQuery.ToList(); 

        //    return model;
        //}

        public int AddSalesAndSalesSupplier(
            string userId,
            string shop,
            DateTime? createdDateTime,
            DateTime? bookingDate,
            string passengerName,
            string destination,
            decimal? deposit,
            string saleLocation,
            int? numPassengers,
            string comments,
            List<SalesSupplierModel> list
            )
        {
            int status = 0;

            using (TransactionScope scope = new TransactionScope())
            {
                // Write to Sales table
                int? returnValue = this._repository.uspAddSales(
                                                        null,
                                                        userId,
                                                        shop,
                                                        createdDateTime,
                                                        bookingDate,
                                                        passengerName,
                                                        destination,
                                                        deposit,
                                                        saleLocation,
                                                        numPassengers,
                                                        comments
                                                        ).SingleOrDefault();

                int salesId = returnValue == null ? status : (int)returnValue;

                if (salesId > 0)
                {
                    for (int i = 0; i < list.Count; i++)
                    {
                        // Write to SalesSupplier table
                        returnValue = this._repository.uspAddSalesSupplier(
                                                                salesId,
                                                                list[i].SourceTypeName,
                                                                list[i].SupplierName,
                                                                list[i].SaleValue
                                                                ).SingleOrDefault();

                        status = returnValue == null ? 0 : (int)returnValue;
                        if (status == 0)
                            break;
                    }
                }

                scope.Complete();
            }

            return status;
        }

        public int UpdateSalesAndSalesSupplier(
            int salesId,
            string userId,
            string shop,
            DateTime? updatedDateTime,
            DateTime? bookingDate,
            string passengerName,
            string destination,
            decimal? deposit,
            string saleLocation,
            int? numPassengers,
            string comments,
            List<SalesSupplierModel> list
            )
        {
            int status = 0;

            //using (TransactionScope scope = new TransactionScope())
            //{
            //    // Update to Sales table
            //    Sale sale = (from r in this._repository.Sales
            //                 where r.SalesID == salesId && r.IsDeleted == false
            //                 select r).FirstOrDefault();
            //    if (sale != null)
            //    {
            //        sale.UserID = userId;
            //        sale.Shop = shop;
            //        sale.LastUpdatedDateTime = updatedDateTime;
            //        sale.BookingDate = bookingDate;
            //        sale.PassengerName = passengerName;
            //        sale.Destination = destination;
            //        sale.Deposit = deposit;
            //        sale.SaleLocation = saleLocation;
            //        sale.Passengers = numPassengers;
            //        sale.Comments = comments;

            //        this._repository.SaveChanges();
            //    }
            //    else
            //    {
            //        return status;
            //    }

            //    // Delete all records with salesId in SalesSupplier table
            //    foreach(SalesSupplier supplier in this._repository.SalesSuppliers.Where(r => r.SalesID == salesId))
            //    {
            //        //supplier.IsDeleted = true;
            //        this._repository.SalesSuppliers.DeleteObject(supplier);
            //    }
            //    this._repository.SaveChanges();

            //    // Add to SalesSupplier table
            //    for (int i = 0; i < list.Count; i++)
            //    {
            //        SalesSupplier newSaleSupplier = new SalesSupplier
            //        {
            //            SalesID = salesId,
            //            SourceType = list[i].SourceTypeName,
            //            Supplier = list[i].SupplierName,
            //            SaleValue = list[i].SaleValue,
            //            IsDeleted = false
            //        };

            //        // Add to SalesSupplier table
            //        this._repository.SalesSuppliers.AddObject(newSaleSupplier);
            //        this._repository.SaveChanges();
            //    }

            //    scope.Complete();
            //}

            // Update to Sales table
            this._repository.uspUpdateSales(
                                    salesId,
                                    userId,
                                    shop,
                                    updatedDateTime,
                                    bookingDate,
                                    passengerName,
                                    destination,
                                    deposit,
                                    saleLocation,
                                    numPassengers,
                                    comments
                                    ).SingleOrDefault();

            //status = returnValue == null ? status : (int)returnValue;

            // Delete all records with salesId in SalesSupplier table
            this._repository.uspDeleteSalesSupplier(salesId);

            for (int i = 0; i < list.Count; i++)
            {
                // Write to SalesSupplier table
                var returnValue = this._repository.uspAddSalesSupplier(
                                                        salesId,
                                                        list[i].SourceTypeName,
                                                        list[i].SupplierName,
                                                        list[i].SaleValue
                                                        ).SingleOrDefault();

                status = returnValue == null ? 0 : (int)returnValue;

                if (status == 0)
                    break;
            }

            return status;
        }

        public int DeleteSalesAndSalesSupplier(int salesId)
        {
            int changes = 0;

            this._repository.uspDeleteSalesAndSalesSupplier(salesId);

            return changes;
        }

        public IQueryable<SalesViewModel> FilterSalesGrid(IQueryable<SalesViewModel> modelQuery, Kendo.Mvc.IFilterDescriptor iFilter)
        {
            var filter = (Kendo.Mvc.FilterDescriptor)iFilter;

            var valueObjectString = filter.Value.ToString();
            var filterValue = valueObjectString.ToLower();

            if (filter.Member == "PassengerName")
            {
                switch (filter.Operator)
                {
                    case Kendo.Mvc.FilterOperator.Contains:
                        modelQuery = modelQuery.Where(r => r.PassengerName.ToLower().Contains(filterValue));
                        break;
                    case Kendo.Mvc.FilterOperator.EndsWith:
                        modelQuery = modelQuery.Where(r => r.PassengerName.ToLower().EndsWith(filterValue));
                        break;
                    case Kendo.Mvc.FilterOperator.IsEqualTo:
                        modelQuery = modelQuery.Where(r => r.PassengerName.ToLower().Equals(filterValue));
                        break;
                    case Kendo.Mvc.FilterOperator.IsNotEqualTo:
                        modelQuery = modelQuery.Where(r => !r.PassengerName.ToLower().Equals(filterValue));
                        break;
                    case Kendo.Mvc.FilterOperator.StartsWith:
                        modelQuery = modelQuery.Where(r => r.PassengerName.ToLower().StartsWith(filterValue));
                        break;
                }
            }

            if (filter.Member == "UserId")
            {
                switch (filter.Operator)
                {
                    case Kendo.Mvc.FilterOperator.Contains:
                        modelQuery = modelQuery.Where(r => r.UserId.ToLower().Contains(filterValue));
                        break;
                    case Kendo.Mvc.FilterOperator.EndsWith:
                        modelQuery = modelQuery.Where(r => r.UserId.ToLower().EndsWith(filterValue));
                        break;
                    case Kendo.Mvc.FilterOperator.IsEqualTo:
                        modelQuery = modelQuery.Where(r => r.UserId.ToLower().Equals(filterValue));
                        break;
                    case Kendo.Mvc.FilterOperator.IsNotEqualTo:
                        modelQuery = modelQuery.Where(r => !r.UserId.ToLower().Equals(filterValue));
                        break;
                    case Kendo.Mvc.FilterOperator.StartsWith:
                        modelQuery = modelQuery.Where(r => r.UserId.ToLower().StartsWith(filterValue));
                        break;
                }
            }

            if (filter.Member == "Shop")
            {
                switch (filter.Operator)
                {
                    case Kendo.Mvc.FilterOperator.Contains:
                        modelQuery = modelQuery.Where(r => r.Shop.ToLower().Contains(filterValue));
                        break;
                    case Kendo.Mvc.FilterOperator.EndsWith:
                        modelQuery = modelQuery.Where(r => r.Shop.ToLower().EndsWith(filterValue));
                        break;
                    case Kendo.Mvc.FilterOperator.IsEqualTo:
                        modelQuery = modelQuery.Where(r => r.Shop.ToLower().Equals(filterValue));
                        break;
                    case Kendo.Mvc.FilterOperator.IsNotEqualTo:
                        modelQuery = modelQuery.Where(r => !r.Shop.ToLower().Equals(filterValue));
                        break;
                    case Kendo.Mvc.FilterOperator.StartsWith:
                        modelQuery = modelQuery.Where(r => r.Shop.ToLower().StartsWith(filterValue));
                        break;
                }
            }

            if (filter.Member == "CreatedDateTime")
            {
                //string propertyName = "CreatedDateTimeString";

            //    switch (filter.Operator)
            //    {
            //        //case Kendo.Mvc.FilterOperator.Contains:
            //        //    modelQuery = modelQuery.Where(r => r.ConvertDateTimeToString(propertyName).Contains(filterValue));
            //        //    break;
            //        //case Kendo.Mvc.FilterOperator.EndsWith:
            //        //    modelQuery = modelQuery.Where(r => r.ConvertDateTimeToString(propertyName).EndsWith(filterValue));
            //        //    break;
            //        //case Kendo.Mvc.FilterOperator.IsEqualTo:
            //        //    modelQuery = modelQuery.Where(r => r.ConvertDateTimeToString(propertyName).Equals(filterValue));
            //        //    break;
            //        //case Kendo.Mvc.FilterOperator.IsNotEqualTo:
            //        //    modelQuery = modelQuery.Where(r => !r.ConvertDateTimeToString(propertyName).Equals(filterValue));
            //        //    break;
            //        //case Kendo.Mvc.FilterOperator.StartsWith:
            //        //    modelQuery = modelQuery.Where(r => r.ConvertDateTimeToString(propertyName).StartsWith(filterValue));
            //        //    break;
            //    }
            }

            if (filter.Member == "BookingDate")
            {
                //string propertyName = "BookingDateString";

                //switch (filter.Operator)
                //{
                //    //case Kendo.Mvc.FilterOperator.Contains:
                //    //    modelQuery = modelQuery.Where(r => r.ConvertDateTimeToString(propertyName).Contains(filterValue));
                //    //    break;
                //    //case Kendo.Mvc.FilterOperator.EndsWith:
                //    //    modelQuery = modelQuery.Where(r => r.ConvertDateTimeToString(propertyName).EndsWith(filterValue));
                //    //    break;
                //    //case Kendo.Mvc.FilterOperator.IsEqualTo:
                //    //    modelQuery = modelQuery.Where(r => r.ConvertDateTimeToString(propertyName).Equals(filterValue));
                //    //    break;
                //    //case Kendo.Mvc.FilterOperator.IsNotEqualTo:
                //    //    modelQuery = modelQuery.Where(r => !r.ConvertDateTimeToString(propertyName).Equals(filterValue));
                //    //    break;
                //    //case Kendo.Mvc.FilterOperator.StartsWith:
                //    //    modelQuery = modelQuery.Where(r => r.ConvertDateTimeToString(propertyName).StartsWith(filterValue));
                //    //    break;
                //}
            }

            return modelQuery;
        }

        private IQueryable<SalesViewModel> FilterSalesGrid(IQueryable<SalesViewModel> modelQuery, IList<Kendo.Mvc.IFilterDescriptor> filters)
        {
            foreach (var ifilter in filters)
            {
                if (ifilter.GetType() == typeof(Kendo.Mvc.CompositeFilterDescriptor))
                {
                    modelQuery = FilterSalesGrid(modelQuery, ((Kendo.Mvc.CompositeFilterDescriptor)ifilter).FilterDescriptors);
                }
                else
                {
                    modelQuery = FilterSalesGrid(modelQuery, ifilter);
                }
            }

            return modelQuery;
        }

        public IQueryable<SalesViewModel> SortSalesGrid(IQueryable<SalesViewModel> modelQuery, IList<Kendo.Mvc.SortDescriptor> sorts)
        {
            // Apply sorting from the UI
            foreach (Kendo.Mvc.SortDescriptor sort in sorts)
            {
                if (sort.SortDirection == ListSortDirection.Ascending)
                {
                    switch (sort.Member)
                    {
                        case "SalesId":
                            modelQuery = modelQuery.OrderBy(x => x.SalesId);
                            break;
                        case "UserId":
                            modelQuery = modelQuery.OrderBy(x => x.UserId);
                            break;
                        case "Shop":
                            modelQuery = modelQuery.OrderBy(x => x.Shop);
                            break;
                        case "CreatedDateTime":
                            modelQuery = modelQuery.OrderBy(x => x.CreatedDateTime);
                            break;
                        case "BookingDate":
                            modelQuery = modelQuery.OrderBy(x => x.BookingDate);
                            break;
                        case "PassengerName":
                            modelQuery = modelQuery.OrderBy(x => x.PassengerName);
                            break;
                        case "Destination":
                            modelQuery = modelQuery.OrderBy(x => x.Destination);
                            break;
                        case "Deposit":
                            modelQuery = modelQuery.OrderBy(x => x.Deposit);
                            break;
                        case "NumberPassengers":
                            modelQuery = modelQuery.OrderBy(x => x.NumberPassengers);
                            break;
                        case "SaleLocation":
                            modelQuery = modelQuery.OrderBy(x => x.SaleLocation);
                            break;
                    }
                }
                else
                {
                    switch (sort.Member)
                    {
                        case "SalesId":
                            modelQuery = modelQuery.OrderByDescending(x => x.SalesId);
                            break;
                        case "UserId":
                            modelQuery = modelQuery.OrderByDescending(x => x.UserId);
                            break;
                        case "Shop":
                            modelQuery = modelQuery.OrderByDescending(x => x.Shop);
                            break;
                        case "CreatedDateTime":
                            modelQuery = modelQuery.OrderByDescending(x => x.CreatedDateTime);
                            break;
                        case "BookingDate":
                            modelQuery = modelQuery.OrderByDescending(x => x.BookingDate);
                            break;
                        case "PassengerName":
                            modelQuery = modelQuery.OrderByDescending(x => x.PassengerName);
                            break;
                        case "Destination":
                            modelQuery = modelQuery.OrderByDescending(x => x.Destination);
                            break;
                        case "Deposit":
                            modelQuery = modelQuery.OrderByDescending(x => x.Deposit);
                            break;
                        case "NumberPassengers":
                            modelQuery = modelQuery.OrderByDescending(x => x.NumberPassengers);
                            break;
                        case "SaleLocation":
                            modelQuery = modelQuery.OrderByDescending(x => x.SaleLocation);
                            break;
                    }
                }
            }

            return modelQuery;
        }
    }
}
