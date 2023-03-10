using Grand.Domain.Common;
using Grand.Domain.Customers;
using Grand.Domain.Localization;
using Grand.Domain.Ads;
using Grand.Domain.Shipping;
using Grand.Domain.Tax;
using Grand.Services.Discounts;
using System.Collections.Generic;
using Grand.Services.Orders;
using Grand.Domain.Orders;

namespace Grand.Services.Ads
{
    public class PlaceAdContainter
    {
        public PlaceAdContainter()
        {
            Cart = new List<ShoppingCartItem>();
            AppliedDiscounts = new List<AppliedDiscount>();
            AppliedGiftCards = new List<AppliedGiftCard>();
        }

        public Customer Customer { get; set; }
        public Language CustomerLanguage { get; set; }
        public string AffiliateId { get; set; }
        public TaxDisplayType CustomerTaxDisplayType { get; set; }
        public string CustomerCurrencyCode { get; set; }
        public decimal CustomerCurrencyRate { get; set; }
        public string PrimaryCurrencyCode { get; set; }

        public Address BillingAddress { get; set; }
        public Address ShippingAddress { get; set; }
        public ShippingStatus ShippingStatus { get; set; }
        public string ShippingMethodName { get; set; }
        public string ShippingRateComputationMethodSystemName { get; set; }
        public bool PickUpInStore { get; set; }
        public PickupPoint PickupPoint { get; set; }
        public bool IsRecurringShoppingCart { get; set; }
        //initial order (used with recurring payments)
        public Ad InitialAd { get; set; }

        public string CheckoutAttributeDescription { get; set; }
        public string CheckoutAttributesXml { get; set; }

        public IList<ShoppingCartItem> Cart { get; set; }
        public List<AppliedDiscount> AppliedDiscounts { get; set; }
        public List<AppliedGiftCard> AppliedGiftCards { get; set; }

        public decimal AdSubTotalInclTax { get; set; }
        public decimal AdSubTotalExclTax { get; set; }
        public decimal AdSubTotalDiscountInclTax { get; set; }
        public decimal AdSubTotalDiscountExclTax { get; set; }
        public decimal AdShippingTotalInclTax { get; set; }
        public decimal AdShippingTotalExclTax { get; set; }
        public decimal PaymentAdditionalFeeInclTax { get; set; }
        public decimal PaymentAdditionalFeeExclTax { get; set; }
        public decimal AdTaxTotal { get; set; }
        public string TaxRates { get; set; }
        public decimal AdDiscountAmount { get; set; }
        public int RedeemedRewardPoints { get; set; }
        public decimal RedeemedRewardPointsAmount { get; set; }
        public decimal AdTotal { get; set; }
        public string[] SelectedPaymentMethodId { get; set; }
    }

}
