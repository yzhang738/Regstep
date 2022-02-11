using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FlightCenterModels
{
    [Serializable]
    public class SalesViewModel
    {
        [Editable(false)]
        public int SalesId { get; set; }

        [Required(ErrorMessage = "UserId is required")]
        public string UserId { get; set; }

        public DateTime? CreatedDateTime { get; set; }

        public DateTime? UpdatedDateTime { get; set; }

        [Required(ErrorMessage = "BookingDate is required")]
        public DateTime? BookingDate { get; set; }

        [Required(ErrorMessage = "Passenger name is required")]
        public string PassengerName { get; set; }

        [Required(ErrorMessage = "Deposit is required")]
        [RegularExpression(@"^\d+.\d{0,2}$")]
        [Range(0, 9999999999999999.99, ErrorMessage = "Please enter a valid decimal Number with 2 decimal places")]
        [DisplayFormat(DataFormatString = "{0:C2}", ApplyFormatInEditMode = true)]
        public decimal? Deposit { get; set; }

        [Required(ErrorMessage = "Number of passengers is required")]
        [Range(0, int.MaxValue, ErrorMessage = "Please enter valid integer Number.")]
        public int? NumberPassengers { get; set; }

        public decimal? Invoice { get; set; }

        public string Destination { get; set; }

        public string Destination_Init { get; set; }

        public string Shop { get; set; }

        public string Shop_Init { get; set; }

        public string SaleLocation { get; set; }

        public string SaleLocation_Init { get; set; }

        public string Comments { get; set; }

        public bool? IsDeleted { get; set; }

        public string Airline { get; set; }

        public string SourceType0 { get; set; }

        public string Supplier0 { get; set; }

        public string SourceType0_Init { get; set; }

        public string Supplier0_Init { get; set; }

        [DisplayFormat(DataFormatString = "{0:C2}", ApplyFormatInEditMode = true)]
        public decimal? SaleValue0 { get; set; }

        public string SourceType1 { get; set; }

        public string Supplier1 { get; set; }

        public string SourceType1_Init { get; set; }

        public string Supplier1_Init { get; set; }

        [DisplayFormat(DataFormatString = "{0:C2}", ApplyFormatInEditMode = true)]
        public decimal? SaleValue1 { get; set; }

        public string SourceType2 { get; set; }

        public string Supplier2 { get; set; }

        public string SourceType2_Init { get; set; }

        public string Supplier2_Init { get; set; }

        [DisplayFormat(DataFormatString = "{0:C2}", ApplyFormatInEditMode = true)]
        public decimal? SaleValue2 { get; set; }

        public string SourceType3 { get; set; }

        public string Supplier3 { get; set; }

        public string SourceType3_Init { get; set; }

        public string Supplier3_Init { get; set; }

        [DisplayFormat(DataFormatString = "{0:C2}", ApplyFormatInEditMode = true)]
        public decimal? SaleValue3 { get; set; }

        public string SourceType4 { get; set; }

        public string Supplier4 { get; set; }

        public string SourceType4_Init { get; set; }

        public string Supplier4_Init { get; set; }

        [DisplayFormat(DataFormatString = "{0:C2}", ApplyFormatInEditMode = true)]
        public decimal? SaleValue4 { get; set; }

        public string SourceType5 { get; set; }

        public string Supplier5 { get; set; }

        public string SourceType5_Init { get; set; }

        public string Supplier5_Init { get; set; }

        [DisplayFormat(DataFormatString = "{0:C2}", ApplyFormatInEditMode = true)]
        public decimal? SaleValue5 { get; set; }

        public string SourceType6 { get; set; }

        public string Supplier6 { get; set; }

        public string SourceType6_Init { get; set; }

        public string Supplier6_Init { get; set; }

        [DisplayFormat(DataFormatString = "{0:C2}", ApplyFormatInEditMode = true)]
        public decimal? SaleValue6 { get; set; }

        public string SourceType7 { get; set; }

        public string Supplier7 { get; set; }

        public string SourceType7_Init { get; set; }

        public string Supplier7_Init { get; set; }

        [DisplayFormat(DataFormatString = "{0:C2}", ApplyFormatInEditMode = true)]
        public decimal? SaleValue7 { get; set; }

        public string SourceType8 { get; set; }

        public string Supplier8 { get; set; }

        public string SourceType8_Init { get; set; }

        public string Supplier8_Init { get; set; }

        [DisplayFormat(DataFormatString = "{0:C2}", ApplyFormatInEditMode = true)]
        public decimal? SaleValue8 { get; set; }

        public string SourceType9 { get; set; }

        public string Supplier9 { get; set; }

        public string SourceType9_Init { get; set; }

        public string Supplier9_Init { get; set; }

        [DisplayFormat(DataFormatString = "{0:C2}", ApplyFormatInEditMode = true)]
        public decimal? SaleValue9 { get; set; }

        public List<SelectListItem> Destinations { get; set; }

        public List<SelectListItem> SaleLocations { get; set; }

        public List<SelectListItem> Shops { get; set; }

        public List<SelectListItem> SourceTypes0 { get; set; }

        public List<SelectListItem> Suppliers0 { get; set; }

        public List<SelectListItem> SourceTypes1 { get; set; }

        public List<SelectListItem> Suppliers1 { get; set; }

        public List<SelectListItem> SourceTypes2 { get; set; }

        public List<SelectListItem> Suppliers2 { get; set; }

        public List<SelectListItem> SourceTypes3 { get; set; }

        public List<SelectListItem> Suppliers3 { get; set; }

        public List<SelectListItem> SourceTypes4 { get; set; }

        public List<SelectListItem> Suppliers4 { get; set; }

        public List<SelectListItem> SourceTypes5 { get; set; }

        public List<SelectListItem> Suppliers5 { get; set; }

        public List<SelectListItem> SourceTypes6 { get; set; }

        public List<SelectListItem> Suppliers6 { get; set; }

        public List<SelectListItem> SourceTypes7 { get; set; }

        public List<SelectListItem> Suppliers7 { get; set; }

        public List<SelectListItem> SourceTypes8 { get; set; }

        public List<SelectListItem> Suppliers8 { get; set; }

        public List<SelectListItem> SourceTypes9 { get; set; }

        public List<SelectListItem> Suppliers9 { get; set; }

        public List<SelectListItem> Airlines { get; set; }

        public List<SelectListItem> InsuranceSuppliers { get; set; }

        public List<SelectListItem> LandSuppliers { get; set; }

        public List<SelectListItem> CruiseSuppliers { get; set; }

    }

    //[Serializable]
    public class SalesSupplierModel
    {
        public string SourceTypeName { get; set; }
        public string SupplierName { get; set; }
        public decimal? SaleValue { get; set; }
    }

}