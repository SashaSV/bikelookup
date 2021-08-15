using Grand.Core.ModelBinding;
using Grand.Core.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Grand.Web.Areas.Admin.Models.Ads
{
    public partial class AdListModel : BaseModel
    {
        public AdListModel()
        {
            AvailableAdStatuses = new List<SelectListItem>();
            AvailablePaymentStatuses = new List<SelectListItem>();
            AvailableShippingStatuses = new List<SelectListItem>();
            AvailableStores = new List<SelectListItem>();
            AvailableVendors = new List<SelectListItem>();
            AvailableWarehouses = new List<SelectListItem>();
            AvailablePaymentMethods = new List<SelectListItem>();
            AvailableCountries = new List<SelectListItem>();
            AvailableAdTags = new List<SelectListItem>();
        }

        [GrandResourceDisplayName("Admin.Ads.List.StartDate")]
        [UIHint("DateNullable")]
        public DateTime? StartDate { get; set; }

        [GrandResourceDisplayName("Admin.Ads.List.EndDate")]
        [UIHint("DateNullable")]
        public DateTime? EndDate { get; set; }

        [GrandResourceDisplayName("Admin.Ads.List.BillingEmail")]
        
        public string BillingEmail { get; set; }

        [GrandResourceDisplayName("Admin.Ads.List.BillingLastName")]
        
        public string BillingLastName { get; set; }

        [GrandResourceDisplayName("Admin.Ads.List.AdStatus")]
        public int AdStatusId { get; set; }
        [GrandResourceDisplayName("Admin.Ads.List.PaymentStatus")]
        public int PaymentStatusId { get; set; }
        [GrandResourceDisplayName("Admin.Ads.List.ShippingStatus")]
        public int ShippingStatusId { get; set; }

        [GrandResourceDisplayName("Admin.Ads.List.PaymentMethod")]
        public string PaymentMethodSystemName { get; set; }

        [GrandResourceDisplayName("Admin.Ads.List.Store")]
        public string StoreId { get; set; }

        [GrandResourceDisplayName("Admin.Ads.List.Vendor")]
        public string VendorId { get; set; }

        [GrandResourceDisplayName("Admin.Ads.List.Warehouse")]
        public string WarehouseId { get; set; }

        [GrandResourceDisplayName("Admin.Ads.List.Product")]
        public string ProductId { get; set; }

        [GrandResourceDisplayName("Admin.Ads.List.BillingCountry")]
        public string BillingCountryId { get; set; }

        [GrandResourceDisplayName("Admin.Ads.List.AdNotes")]
        
        public string AdNotes { get; set; }

        [GrandResourceDisplayName("Admin.Ads.List.AdGuid")]
        
        public string AdGuid { get; set; }

        [GrandResourceDisplayName("Admin.Ads.List.GoDirectlyToNumber")]
        
        public string GoDirectlyToNumber { get; set; }

        public bool IsLoggedInAsVendor { get; set; }

        [GrandResourceDisplayName("Admin.Ads.List.AdTagId")]
        public string AdTag { get; set; }

        public IList<SelectListItem> AvailableAdStatuses { get; set; }
        public IList<SelectListItem> AvailablePaymentStatuses { get; set; }
        public IList<SelectListItem> AvailableShippingStatuses { get; set; }
        public IList<SelectListItem> AvailableStores { get; set; }
        public IList<SelectListItem> AvailableVendors { get; set; }
        public IList<SelectListItem> AvailableWarehouses { get; set; }
        public IList<SelectListItem> AvailablePaymentMethods { get; set; }
        public IList<SelectListItem> AvailableCountries { get; set; }
        public IList<SelectListItem> AvailableAdTags { get; set; } 
    }
}