using Grand.Domain.Catalog;
using Grand.Domain.Ads;
using Grand.Domain.Payments;
using Grand.Domain.Tax;
using Grand.Core.ModelBinding;
using Grand.Core.Models;
using Grand.Web.Areas.Admin.Models.Common;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Grand.Web.Areas.Admin.Models.Ads
{
    public partial class AdModel : BaseEntityModel
    {
        public AdModel()
        {
            CustomValues = new Dictionary<string, object>();
            TaxRates = new List<TaxRate>();
            GiftCards = new List<GiftCard>();
            Items = new List<AdItemModel>();
            UsedDiscounts = new List<UsedDiscountModel>();
        }

        public bool IsLoggedInAsVendor { get; set; }

        //identifiers
        [GrandResourceDisplayName("Admin.Ads.Fields.ID")]
        public override string Id { get; set; }

        [GrandResourceDisplayName("Admin.Ads.Fields.ID")]
        public int AdNumber { get; set; }
        [GrandResourceDisplayName("Admin.Ads.Fields.Code")]
        public string Code { get; set; }

        [GrandResourceDisplayName("Admin.Ads.Fields.AdGuid")]
        public Guid AdGuid { get; set; }

        //store
        [GrandResourceDisplayName("Admin.Ads.Fields.Store")]
        public string StoreName { get; set; }

        //customer info
        [GrandResourceDisplayName("Admin.Ads.Fields.Customer")]
        public string CustomerId { get; set; }
        [GrandResourceDisplayName("Admin.Ads.Fields.Customer")]
        public string CustomerInfo { get; set; }
        [GrandResourceDisplayName("Admin.Ads.Fields.CustomerEmail")]
        public string CustomerEmail { get; set; }
        public string CustomerFullName { get; set; }
        [GrandResourceDisplayName("Admin.Ads.Fields.CustomerIP")]
        public string CustomerIp { get; set; }
        [GrandResourceDisplayName("Admin.Ads.Fields.UrlReferrer")]
        public string UrlReferrer { get; set; }

        [GrandResourceDisplayName("Admin.Ads.Fields.CustomValues")]
        public Dictionary<string, object> CustomValues { get; set; }

        [GrandResourceDisplayName("Admin.Ads.Fields.Affiliate")]
        public string AffiliateId { get; set; }
        [GrandResourceDisplayName("Admin.Ads.Fields.Affiliate")]
        public string AffiliateName { get; set; }

        //Used discounts
        [GrandResourceDisplayName("Admin.Ads.Fields.UsedDiscounts")]
        public IList<UsedDiscountModel> UsedDiscounts { get; set; }

        //totals
        public bool AllowCustomersToSelectTaxDisplayType { get; set; }
        public TaxDisplayType TaxDisplayType { get; set; }
        [GrandResourceDisplayName("Admin.Ads.Fields.AdSubtotalInclTax")]
        public string AdSubtotalInclTax { get; set; }
        [GrandResourceDisplayName("Admin.Ads.Fields.AdSubtotalExclTax")]
        public string AdSubtotalExclTax { get; set; }
        [GrandResourceDisplayName("Admin.Ads.Fields.AdSubTotalDiscountInclTax")]
        public string AdSubTotalDiscountInclTax { get; set; }
        [GrandResourceDisplayName("Admin.Ads.Fields.AdSubTotalDiscountExclTax")]
        public string AdSubTotalDiscountExclTax { get; set; }
        [GrandResourceDisplayName("Admin.Ads.Fields.AdShippingInclTax")]
        public string AdShippingInclTax { get; set; }
        [GrandResourceDisplayName("Admin.Ads.Fields.AdShippingExclTax")]
        public string AdShippingExclTax { get; set; }
        [GrandResourceDisplayName("Admin.Ads.Fields.PaymentMethodAdditionalFeeInclTax")]
        public string PaymentMethodAdditionalFeeInclTax { get; set; }
        [GrandResourceDisplayName("Admin.Ads.Fields.PaymentMethodAdditionalFeeExclTax")]
        public string PaymentMethodAdditionalFeeExclTax { get; set; }
        [GrandResourceDisplayName("Admin.Ads.Fields.Tax")]
        public string Tax { get; set; }
        public IList<TaxRate> TaxRates { get; set; }
        public bool DisplayTax { get; set; }
        public bool DisplayTaxRates { get; set; }
        [GrandResourceDisplayName("Admin.Ads.Fields.AdTotalDiscount")]
        public string AdTotalDiscount { get; set; }
        [GrandResourceDisplayName("Admin.Ads.Fields.RedeemedRewardPoints")]
        public int RedeemedRewardPoints { get; set; }
        [GrandResourceDisplayName("Admin.Ads.Fields.RedeemedRewardPoints")]
        public string RedeemedRewardPointsAmount { get; set; }
        [GrandResourceDisplayName("Admin.Ads.Fields.AdTotal")]
        public string AdTotal { get; set; }
        [GrandResourceDisplayName("Admin.Ads.Fields.RefundedAmount")]
        public string RefundedAmount { get; set; }
        [GrandResourceDisplayName("Admin.Ads.Fields.Profit")]
        public string Profit { get; set; }
        [GrandResourceDisplayName("Admin.Ads.Fields.Currency")]
        public string CurrencyCode { get; set; }
        [GrandResourceDisplayName("Admin.Ads.Fields.CurrencyRate")]

        [UIHint("DecimalN4")]
        public decimal CurrencyRate { get; set; }
        //edit totals
        [GrandResourceDisplayName("Admin.Ads.Fields.Edit.AdSubtotal")]
        public decimal AdSubtotalInclTaxValue { get; set; }
        [GrandResourceDisplayName("Admin.Ads.Fields.Edit.AdSubtotal")]
        public decimal AdSubtotalExclTaxValue { get; set; }
        [GrandResourceDisplayName("Admin.Ads.Fields.Edit.AdSubTotalDiscount")]
        public decimal AdSubTotalDiscountInclTaxValue { get; set; }
        [GrandResourceDisplayName("Admin.Ads.Fields.Edit.AdSubTotalDiscount")]
        public decimal AdSubTotalDiscountExclTaxValue { get; set; }
        [GrandResourceDisplayName("Admin.Ads.Fields.Edit.AdShipping")]
        public decimal AdShippingInclTaxValue { get; set; }
        [GrandResourceDisplayName("Admin.Ads.Fields.Edit.AdShipping")]
        public decimal AdShippingExclTaxValue { get; set; }
        [GrandResourceDisplayName("Admin.Ads.Fields.Edit.PaymentMethodAdditionalFee")]
        public decimal PaymentMethodAdditionalFeeInclTaxValue { get; set; }
        [GrandResourceDisplayName("Admin.Ads.Fields.Edit.PaymentMethodAdditionalFee")]
        public decimal PaymentMethodAdditionalFeeExclTaxValue { get; set; }
        [GrandResourceDisplayName("Admin.Ads.Fields.Edit.Tax")]
        public decimal TaxValue { get; set; }
        [GrandResourceDisplayName("Admin.Ads.Fields.Edit.TaxRates")]
        public string TaxRatesValue { get; set; }
        [GrandResourceDisplayName("Admin.Ads.Fields.Edit.AdTotalDiscount")]
        public decimal AdTotalDiscountValue { get; set; }
        [GrandResourceDisplayName("Admin.Ads.Fields.Edit.AdTotal")]
        public decimal AdTotalValue { get; set; }

        //associated recurring payment id
        [GrandResourceDisplayName("Admin.Ads.Fields.RecurringPayment")]
        public string RecurringPaymentId { get; set; }

        //Ad status
        [GrandResourceDisplayName("Admin.Ads.Fields.AdStatus")]
        public string AdStatus { get; set; }
        [GrandResourceDisplayName("Admin.Ads.Fields.AdStatus")]
        public int AdStatusId { get; set; }

        //payment info
        [GrandResourceDisplayName("Admin.Ads.Fields.PaymentStatus")]
        public string PaymentStatus { get; set; }
        public PaymentStatus PaymentStatusEnum { get; set; }
        [GrandResourceDisplayName("Admin.Ads.Fields.PaymentMethod")]
        public string PaymentMethod { get; set; }

        //credit card info
        public bool AllowStoringCreditCardNumber { get; set; }
        [GrandResourceDisplayName("Admin.Ads.Fields.CardType")]
        
        public string CardType { get; set; }
        [GrandResourceDisplayName("Admin.Ads.Fields.CardName")]
        
        public string CardName { get; set; }
        [GrandResourceDisplayName("Admin.Ads.Fields.CardNumber")]
        
        public string CardNumber { get; set; }
        [GrandResourceDisplayName("Admin.Ads.Fields.CardCVV2")]
        
        public string CardCvv2 { get; set; }
        [GrandResourceDisplayName("Admin.Ads.Fields.CardExpirationMonth")]
        
        public string CardExpirationMonth { get; set; }
        [GrandResourceDisplayName("Admin.Ads.Fields.CardExpirationYear")]
        
        public string CardExpirationYear { get; set; }

        //misc payment info
        [GrandResourceDisplayName("Admin.Ads.Fields.AuthorizationTransactionID")]
        public string AuthorizationTransactionId { get; set; }
        [GrandResourceDisplayName("Admin.Ads.Fields.CaptureTransactionID")]
        public string CaptureTransactionId { get; set; }
        [GrandResourceDisplayName("Admin.Ads.Fields.SubscriptionTransactionID")]
        public string SubscriptionTransactionId { get; set; }

        //shipping info
        public bool IsShippable { get; set; }
        public bool PickUpInStore { get; set; }

        [GrandResourceDisplayName("Admin.Ads.Fields.PickupAddress")]
        public AddressModel PickupAddress { get; set; }

        [GrandResourceDisplayName("Admin.Ads.Fields.ShippingStatus")]
        public string ShippingStatus { get; set; }
        [GrandResourceDisplayName("Admin.Ads.Fields.ShippingAddress")]
        public AddressModel ShippingAddress { get; set; }
        [GrandResourceDisplayName("Admin.Ads.Fields.ShippingMethod")]
        public string ShippingMethod { get; set; }
        public string ShippingAdditionDescription { get; set; }
        public string ShippingAddressGoogleMapsUrl { get; set; }
        public bool CanAddNewShipments { get; set; }

        //billing info
        [GrandResourceDisplayName("Admin.Ads.Fields.BillingAddress")]
        public AddressModel BillingAddress { get; set; }
        [GrandResourceDisplayName("Admin.Ads.Fields.VatNumber")]
        public string VatNumber { get; set; }
        
        //gift cards
        public IList<GiftCard> GiftCards { get; set; }

        //items
        public bool HasDownloadableProducts { get; set; }
        public IList<AdItemModel> Items { get; set; }

        //creation date
        [GrandResourceDisplayName("Admin.Ads.Fields.CreatedOn")]
        public DateTime CreatedOn { get; set; }

        //checkout attributes
        public string CheckoutAttributeInfo { get; set; }


        //Ad notes
        [GrandResourceDisplayName("Admin.Ads.AdNotes.Fields.DisplayToCustomer")]
        public bool AddAdNoteDisplayToCustomer { get; set; }
        [GrandResourceDisplayName("Admin.Ads.AdNotes.Fields.Note")]
        
        public string AddAdNoteMessage { get; set; }
        public bool AddAdNoteHasDownload { get; set; }
        [GrandResourceDisplayName("Admin.Ads.AdNotes.Fields.Download")]
        [UIHint("Download")]
        public string AddAdNoteDownloadId { get; set; }

        //refund info
        [GrandResourceDisplayName("Admin.Ads.Fields.PartialRefund.AmountToRefund")]
        public decimal AmountToRefund { get; set; }
        public decimal MaxAmountToRefund { get; set; }
        public string PrimaryStoreCurrencyCode { get; set; }

        //workflow info
        public bool CanCancelAd { get; set; }
        public bool CanCapture { get; set; }
        public bool CanMarkAdAsPaid { get; set; }
        public bool CanRefund { get; set; }
        public bool CanRefundOffline { get; set; }
        public bool CanPartiallyRefund { get; set; }
        public bool CanPartiallyRefundOffline { get; set; }
        public bool CanVoid { get; set; }
        public bool CanVoidOffline { get; set; }

        //Ad's tags
        [GrandResourceDisplayName("Admin.Ads.Fields.AdTags")]
        public string AdTags { get; set; }

        #region Nested Classes

        public partial class AdItemModel : BaseEntityModel
        {
            public AdItemModel()
            {
                ReturnRequestIds = new List<string>();
                PurchasedGiftCardIds = new List<string>();
            }
            public string ProductId { get; set; }
            public string ProductName { get; set; }
            public string VendorName { get; set; }
            public string Sku { get; set; }

            public string PictureThumbnailUrl { get; set; }

            public string UnitPriceInclTax { get; set; }
            public string UnitPriceExclTax { get; set; }
            public decimal UnitPriceInclTaxValue { get; set; }
            public decimal UnitPriceExclTaxValue { get; set; }

            public int Quantity { get; set; }

            public string DiscountInclTax { get; set; }
            public string DiscountExclTax { get; set; }
            public decimal DiscountInclTaxValue { get; set; }
            public decimal DiscountExclTaxValue { get; set; }

            public string SubTotalInclTax { get; set; }
            public string SubTotalExclTax { get; set; }
            public decimal SubTotalInclTaxValue { get; set; }
            public decimal SubTotalExclTaxValue { get; set; }

            public string AttributeInfo { get; set; }
            public string RecurringInfo { get; set; }
            public string RentalInfo { get; set; }
            public IList<string> ReturnRequestIds { get; set; }
            public IList<string> PurchasedGiftCardIds { get; set; }

            public bool IsDownload { get; set; }
            public int DownloadCount { get; set; }
            public DownloadActivationType DownloadActivationType { get; set; }
            public bool IsDownloadActivated { get; set; }
            public Guid LicenseDownloadGuid { get; set; }

            public string Commission { get; set; }
            public decimal CommissionValue { get; set; } 
        }

        public partial class TaxRate : BaseModel
        {
            public string Rate { get; set; }
            public string Value { get; set; }
        }

        public partial class GiftCard : BaseModel
        {
            [GrandResourceDisplayName("Admin.Ads.Fields.GiftCardInfo")]
            public string CouponCode { get; set; }
            public string Amount { get; set; }
        }

        public partial class AdNote : BaseEntityModel
        {
            public string AdId { get; set; }
            [GrandResourceDisplayName("Admin.Ads.AdNotes.Fields.DisplayToCustomer")]
            public bool DisplayToCustomer { get; set; }
            [GrandResourceDisplayName("Admin.Ads.AdNotes.Fields.Note")]
            public string Note { get; set; }
            [GrandResourceDisplayName("Admin.Ads.AdNotes.Fields.Download")]
            public string DownloadId { get; set; }
            [GrandResourceDisplayName("Admin.Ads.AdNotes.Fields.Download")]
            public Guid DownloadGuid { get; set; }
            [GrandResourceDisplayName("Admin.Ads.AdNotes.Fields.CreatedOn")]
            public DateTime CreatedOn { get; set; }
            [GrandResourceDisplayName("Admin.Ads.AdNotes.Fields.CreatedByCustomer")]
            public bool CreatedByCustomer { get; set; }
        }

        public partial class UploadLicenseModel : BaseModel
        {
            public string AdId { get; set; }

            public string AdItemId { get; set; }

            [UIHint("Download")]
            public string LicenseDownloadId { get; set; }

        }

        public partial class AddAdProductModel : BaseModel
        {
            public AddAdProductModel()
            {
                AvailableCategories = new List<SelectListItem>();
                AvailableManufacturers = new List<SelectListItem>();
                AvailableProductTypes = new List<SelectListItem>();
            }

            [GrandResourceDisplayName("Admin.Catalog.Products.List.SearchProductName")]
            
            public string SearchProductName { get; set; }
            [GrandResourceDisplayName("Admin.Catalog.Products.List.SearchCategory")]
            public string SearchCategoryId { get; set; }
            [GrandResourceDisplayName("Admin.Catalog.Products.List.SearchManufacturer")]
            public string SearchManufacturerId { get; set; }
            [GrandResourceDisplayName("Admin.Catalog.Products.List.SearchProductType")]
            public int SearchProductTypeId { get; set; }

            public IList<SelectListItem> AvailableCategories { get; set; }
            public IList<SelectListItem> AvailableManufacturers { get; set; }
            public IList<SelectListItem> AvailableProductTypes { get; set; }

            public string AdId { get; set; }
            public int AdNumber { get; set; }
            #region Nested classes
            
            public partial class ProductModel : BaseEntityModel
            {
                [GrandResourceDisplayName("Admin.Ads.Products.AddNew.Name")]
                
                public string Name { get; set; }

                [GrandResourceDisplayName("Admin.Ads.Products.AddNew.SKU")]
                
                public string Sku { get; set; }
            }

            public partial class ProductDetailsModel : BaseModel
            {
                public ProductDetailsModel()
                {
                    ProductAttributes = new List<ProductAttributeModel>();
                    GiftCard = new GiftCardModel();
                    Warnings = new List<string>();
                }

                public string ProductId { get; set; }

                public string AdId { get; set; }
                public int AdNumber { get; set; }

                public ProductType ProductType { get; set; }

                public string Name { get; set; }

                [GrandResourceDisplayName("Admin.Ads.Products.AddNew.UnitPriceInclTax")]
                public decimal UnitPriceInclTax { get; set; }
                [GrandResourceDisplayName("Admin.Ads.Products.AddNew.UnitPriceExclTax")]
                public decimal UnitPriceExclTax { get; set; }

                [GrandResourceDisplayName("Admin.Ads.Products.AddNew.Quantity")]
                public int Quantity { get; set; }

                [GrandResourceDisplayName("Admin.Ads.Products.AddNew.SubTotalInclTax")]
                public decimal SubTotalInclTax { get; set; }
                [GrandResourceDisplayName("Admin.Ads.Products.AddNew.SubTotalExclTax")]
                public decimal SubTotalExclTax { get; set; }

                //product attributes
                public IList<ProductAttributeModel> ProductAttributes { get; set; }
                //gift card info
                public GiftCardModel GiftCard { get; set; }

                public List<string> Warnings { get; set; }

            }

            public partial class ProductAttributeModel : BaseEntityModel
            {
                public ProductAttributeModel()
                {
                    Values = new List<ProductAttributeValueModel>();
                }

                public string ProductAttributeId { get; set; }

                public string Name { get; set; }

                public string TextPrompt { get; set; }

                public bool IsRequired { get; set; }

                public AttributeControlType AttributeControlType { get; set; }

                public IList<ProductAttributeValueModel> Values { get; set; }
            }

            public partial class ProductAttributeValueModel : BaseEntityModel
            {
                public string Name { get; set; }

                public bool IsPreSelected { get; set; }
            }


            public partial class GiftCardModel : BaseModel
            {
                public bool IsGiftCard { get; set; }

                [GrandResourceDisplayName("Admin.Ads.Products.GiftCard.RecipientName")]
                
                public string RecipientName { get; set; }
                [GrandResourceDisplayName("Admin.Ads.Products.GiftCard.RecipientEmail")]
                
                public string RecipientEmail { get; set; }
                [GrandResourceDisplayName("Admin.Ads.Products.GiftCard.SenderName")]
                
                public string SenderName { get; set; }
                [GrandResourceDisplayName("Admin.Ads.Products.GiftCard.SenderEmail")]
                
                public string SenderEmail { get; set; }
                [GrandResourceDisplayName("Admin.Ads.Products.GiftCard.Message")]
                
                public string Message { get; set; }

                public GiftCardType GiftCardType { get; set; }
            }
            #endregion
        }

        public partial class UsedDiscountModel:BaseModel
        {
            public string DiscountId { get; set; }
            public string DiscountName { get; set; }
        }

        #endregion
    }


    public partial class AdAggreratorModel : BaseModel
    {
        //aggergator properties
        public string aggregatorprofit { get; set; }
        public string aggregatorshipping { get; set; }
        public string aggregatortax { get; set; }
        public string aggregatortotal { get; set; }
    }
}
