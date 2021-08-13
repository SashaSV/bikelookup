using Grand.Core.Models;
using Grand.Web.Models.Common;
using Grand.Web.Models.Media;
using System;
using System.Collections.Generic;

namespace Grand.Web.Models.Ads
{
    public partial class AdDetailsModel : BaseEntityModel
    {
        public AdDetailsModel()
        {
            TaxRates = new List<TaxRate>();
            GiftCards = new List<GiftCard>();
            Items = new List<AdItemModel>();
            AdNotes = new List<AdNote>();
            Shipments = new List<ShipmentBriefModel>();

            BillingAddress = new AddressModel();
            ShippingAddress = new AddressModel();
            PickupAddress = new AddressModel();
            CustomValues = new Dictionary<string, object>();
        }

        public bool PrintMode { get; set; }
        public bool PdfInvoiceDisabled { get; set; }

        public bool UserCanCancelUnpaidAd { get; set; }

        public DateTime CreatedOn { get; set; }

        public string AdStatus { get; set; }

        public bool IsReAdAllowed { get; set; }

        public bool IsReturnRequestAllowed { get; set; }

        public bool IsShippable { get; set; }
        public bool PickUpInStore { get; set; }
        public AddressModel PickupAddress { get; set; }
        public string ShippingStatus { get; set; }
        public AddressModel ShippingAddress { get; set; }
        public string ShippingMethod { get; set; }
        public string ShippingAdditionDescription { get; set; }
        public IList<ShipmentBriefModel> Shipments { get; set; }

        public AddressModel BillingAddress { get; set; }

        public string VatNumber { get; set; }
        public int AdNumber { get; set; }
        public string AdCode { get; set; }
        public string PaymentMethod { get; set; }
        public string PaymentMethodStatus { get; set; }
        public bool CanRePostProcessPayment { get; set; }
        public Dictionary<string, object> CustomValues { get; set; }

        public string AdSubtotal { get; set; }
        public string AdSubTotalDiscount { get; set; }
        public string AdShipping { get; set; }
        public string PaymentMethodAdditionalFee { get; set; }
        public string CheckoutAttributeInfo { get; set; }

        public bool PricesIncludeTax { get; set; }
        public bool DisplayTaxShippingInfo { get; set; }
        public string Tax { get; set; }
        public IList<TaxRate> TaxRates { get; set; }
        public bool DisplayTax { get; set; }
        public bool DisplayTaxRates { get; set; }

        public string AdTotalDiscount { get; set; }
        public int RedeemedRewardPoints { get; set; }
        public string RedeemedRewardPointsAmount { get; set; }
        public string AdTotal { get; set; }

        public IList<GiftCard> GiftCards { get; set; }

        public bool ShowSku { get; set; }
        public IList<AdItemModel> Items { get; set; }

        public IList<AdNote> AdNotes { get; set; }

        public bool ShowAddAdNote { get; set; }

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