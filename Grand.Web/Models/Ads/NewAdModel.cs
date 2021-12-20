using Grand.Core.ModelBinding;
using Grand.Core.Models;
using Grand.Domain.Catalog;
using Grand.Web.Models.Common;
using Grand.Web.Models.Media;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;

namespace Grand.Web.Models.Ads
{
    public partial class NewAdModel : BaseEntityModel
    {
        public NewAdModel()
        {
            Items = new List<AdItemModel>();
            Shipments = new List<ShipmentBriefModel>();
            BillingAddress = new AddressModel();
            ShippingAddress = new AddressModel();
        }

        public DateTime CreatedOn { get; set; }

        public string AdStatus { get; set; }
        public string ShippingStatus { get; set; }
        public AddressModel ShippingAddress { get; set; }
        public IList<SelectListItem> ShippingMethodType { get; set; }
        
        [GrandResourceDisplayName("Ad.Fields.ShippingMethodType")]
        public string ShippingMethodId { get; set; }
        public string ShippingAdditionDescription { get; set; }
        public IList<ShipmentBriefModel> Shipments { get; set; }

        public AddressModel BillingAddress { get; set; }

        public string VatNumber { get; set; }
        public int AdNumber { get; set; }
        public string AdCode { get; set; }
        public IList<SelectListItem> PaymentMethodType { get; set; }

        [GrandResourceDisplayName("Ad.Fields.PaymentMethodType")]
        public string PaymentMethodId { get; set; }
        public string PaymentMethod { get; set; }
        public string PaymentMethodStatus { get; set; }
        public string AdShipping { get; set; }
        public bool ShowSku { get; set; }
        public IList<AdItemModel> Items { get; set; }

        [GrandResourceDisplayName("Ad.Fields.isDocument")]
        public bool WithDocuments { get; set; }
        public string ManufactureName { get; set; }
        public string Model { get; set; }
        public int? Year { get; set; }
        public string Weeldiam { get; set; }
        public string Size { get; set; }
        public string Color { get; set; }
        public string AdComment { get; set; }
        public int Price { get; set; }
        public int Mileage { get; set; }
        public bool IsAuction { get; set; }
        public string SearchBike { get; set; }

        public IFormFile [] ImageFile { get; set; }

        public SpecificationAttribute CollorAtribure { get; set; }

        #region Nested Classes

        public partial class AdItemModel : BaseEntityModel
        {
            public AdItemModel()
            {
                Picture = new PictureModel();
            }
            public Guid AdItemGuid { get; set; }
            public string Sku { get; set; }
            public string ProductId { get; set; }
            public string ProductName { get; set; }
            public string ProductSeName { get; set; }
            public PictureModel Picture { get; set; }
            public string UnitPrice { get; set; }
            public decimal UnitPriceValue { get; set; }
            public string UnitPriceWithoutDiscount { get; set; }
            public decimal UnitPriceWithoutDiscountValue { get; set; }
            public string SubTotal { get; set; }
            public string Discount { get; set; }
            public int Quantity { get; set; }
            public string AttributeInfo { get; set; }
            public string RentalInfo { get; set; }

            //downloadable product properties
            public string DownloadId { get; set; }
            public string LicenseId { get; set; }
        }

        public partial class TaxRate : BaseModel
        {
            public string Rate { get; set; }
            public string Value { get; set; }
        }

        public partial class GiftCard : BaseModel
        {
            public string CouponCode { get; set; }
            public string Amount { get; set; }
        }

        public partial class AdNote : BaseEntityModel
        {
            public bool HasDownload { get; set; }
            public string Note { get; set; }
            public DateTime CreatedOn { get; set; }
            public string AdId { get; set; }
        }

        public partial class ShipmentBriefModel : BaseEntityModel
        {
            public string TrackingNumber { get; set; }
            public int ShipmentNumber { get; set; }
            public DateTime? ShippedDate { get; set; }
            public DateTime? DeliveryDate { get; set; }
        }
        #endregion
    }
}