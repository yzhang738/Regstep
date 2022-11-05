using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace RSToolKit.WebUI.Models.Inputs.Company
{
    /// <summary>
    /// Model for binding input data about a company.
    /// </summary>
    public class CompanyInput
    {
        /// <summary>
        /// The unique id of the company.
        /// </summary>
        public Guid Id { get; set; }
        /// <summary>
        /// The name of the company.
        /// </summary>
        [Required]
        public string Name { get; set; }
        /// <summary>
        /// Line 1 of billing address.
        /// </summary>
        public string BillingAddressLine1 { get; set; }
        /// <summary>
        /// Line 2 of billing address.
        /// </summary>
        public string BillingAddressLine2 { get; set; }
        /// <summary>
        /// Billing address city.
        /// </summary>
        public string BillingCity { get; set; }
        /// <summary>
        /// Billing address state.
        /// </summary>
        public string BillingState { get; set; }
        /// <summary>
        /// Billing address country.
        /// </summary>
        public string BillingCountry { get; set; }
        /// <summary>
        /// Billing address zip / postal code.
        /// </summary>
        public string BillingZipCode { get; set; }
        /// <summary>
        /// Shipping address line 1.
        /// </summary>
        public string ShippingAddressLine1 { get; set; }
        /// <summary>
        /// Shipping addres line 2.
        /// </summary>
        public string ShippingAddressLine2 { get; set; }
        /// <summary>
        /// Shipping address city.
        /// </summary>
        public string ShippingCity { get; set; }
        /// <summary>
        /// Shipping address state.
        /// </summary>
        public string ShippingState { get; set; }
        /// <summary>
        /// Shipping address country.
        /// </summary>
        public string ShippingCountry { get; set; }
        /// <summary>
        /// Shipping address zip / postal code.
        /// </summary>
        public string ShippingZipCode { get; set; }
        /// <summary>
        /// Invoice address line 1.
        /// </summary>
        public string InvoiceAddressLine1 { get; set; }
        /// <summary>
        /// Invoice address line 2.
        /// </summary>
        public string InvoiceAddressLine2 { get; set; }
        /// <summary>
        /// Invoice address city.
        /// </summary>
        public string InvoiceCity { get; set; }
        /// <summary>
        /// Invoice address state.
        /// </summary>
        public string InvoiceState { get; set; }
        /// <summary>
        /// Invoice address state.
        /// </summary>
        public string InvoiceCountry { get; set; }
        /// <summary>
        /// Invoice address zip code.
        /// </summary>
        public string InvoiceZipCode { get; set; }
        /// <summary>
        /// Invoice phone number.
        /// </summary>
        public string InvoicePhone { get; set; }
        /// <summary>
        /// Invoice fax number.
        /// </summary>
        public string InvoiceFax { get; set; }
        /// <summary>
        /// Invoice email address.
        /// </summary>
        public string InvoiceEmail { get; set; }
    }
}