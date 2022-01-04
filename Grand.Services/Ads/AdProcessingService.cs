using Grand.Core;
using Grand.Domain.Catalog;
using Grand.Domain.Common;
using Grand.Domain.Customers;
using Grand.Domain.Discounts;
using Grand.Domain.Localization;
using Grand.Domain.Logging;
using Grand.Domain.Ads;
using Grand.Domain.Payments;
using Grand.Domain.Shipping;
using Grand.Domain.Tax;
using Grand.Domain.Vendors;
using Grand.Services.Affiliates;
using Grand.Services.Catalog;
using Grand.Services.Commands.Models.Customers;
using Grand.Services.Commands.Models.Ads;
using Grand.Services.Common;
using Grand.Services.Customers;
using Grand.Services.Directory;
using Grand.Services.Discounts;
using Grand.Services.Events.Extensions;
using Grand.Services.Localization;
using Grand.Services.Logging;
using Grand.Services.Messages;
using Grand.Services.Payments;
using Grand.Services.Security;
using Grand.Services.Shipping;
using Grand.Services.Tax;
using Grand.Services.Vendors;
using MediatR;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grand.Domain.Orders;

namespace Grand.Services.Ads
{
    /// <summary>
    /// Ad processing service
    /// </summary>
    public partial class AdProcessingService : IAdProcessingService
    {
        #region Fields

        private readonly IAdService _orderService;
        private readonly IWebHelper _webHelper;
        private readonly ILocalizationService _localizationService;
        private readonly ILanguageService _languageService;
        private readonly IProductService _productService;
        private readonly IPaymentService _paymentService;
        private readonly ILogger _logger;
        private readonly IPriceCalculationService _priceCalculationService;
        private readonly IPriceFormatter _priceFormatter;
        private readonly IProductAttributeParser _productAttributeParser;
        private readonly IProductAttributeFormatter _productAttributeFormatter;
        private readonly IShippingService _shippingService;
        private readonly ITaxService _taxService;
        private readonly ICustomerService _customerService;
        private readonly IDiscountService _discountService;
        private readonly IEncryptionService _encryptionService;
        private readonly IWorkContext _workContext;
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly IVendorService _vendorService;
        private readonly ICurrencyService _currencyService;
        private readonly IAffiliateService _affiliateService;
        private readonly IMediator _mediator;
        private readonly IPdfService _pdfService;
        private readonly IStoreContext _storeContext;
        private readonly IProductReservationService _productReservationService;
        private readonly IAuctionService _auctionService;
        private readonly ICountryService _countryService;
        private readonly ShippingSettings _shippingSettings;
        private readonly PaymentSettings _paymentSettings;
        private readonly AdSettings _orderSettings;
        private readonly TaxSettings _taxSettings;
        private readonly LocalizationSettings _localizationSettings;
        //private readonly IAdTotalCalculationService _orderTotalCalculationService;
        //private readonly IGiftCardService _giftCardService;
        //private readonly IShoppingCartService _shoppingCartService;
        //private readonly ICheckoutAttributeFormatter _checkoutAttributeFormatter;
        //private readonly IRewardPointsService _rewardPointsService;
        //private readonly ShoppingCartSettings _shoppingCartSettings;
        #endregion

        #region Ctor

        public AdProcessingService(IAdService orderService,
            IWebHelper webHelper,
            ILocalizationService localizationService,
            ILanguageService languageService,
            IProductService productService,
            IPaymentService paymentService,
            ILogger logger,
            IPriceCalculationService priceCalculationService,
            IPriceFormatter priceFormatter,
            IProductAttributeParser productAttributeParser,
            IProductAttributeFormatter productAttributeFormatter,
            IShippingService shippingService,
            ITaxService taxService,
            ICustomerService customerService,
            IDiscountService discountService,
            IEncryptionService encryptionService,
            IWorkContext workContext,
            IWorkflowMessageService workflowMessageService,
            IVendorService vendorService,
            ICurrencyService currencyService,
            IAffiliateService affiliateService,
            IMediator mediator,
            IPdfService pdfService,
            IStoreContext storeContext,
            IProductReservationService productReservationService,
            IAuctionService auctionService,
            ICountryService countryService,
            ShippingSettings shippingSettings,
            PaymentSettings paymentSettings,
            AdSettings orderSettings,
            TaxSettings taxSettings,
            //IAdTotalCalculationService orderTotalCalculationService,
            //IGiftCardService giftCardService,
            //IShoppingCartService shoppingCartService,
            //ICheckoutAttributeFormatter checkoutAttributeFormatter,
            //IRewardPointsService rewardPointsService,
            //ShoppingCartSettings shoppingCartSettings,
            LocalizationSettings localizationSettings)
        {
            _orderService = orderService;
            _webHelper = webHelper;
            _localizationService = localizationService;
            _languageService = languageService;
            _productService = productService;
            _paymentService = paymentService;
            _logger = logger;
            _priceCalculationService = priceCalculationService;
            _priceFormatter = priceFormatter;
            _productAttributeParser = productAttributeParser;
            _productAttributeFormatter = productAttributeFormatter;
            _workContext = workContext;
            _workflowMessageService = workflowMessageService;
            _vendorService = vendorService;
            _shippingService = shippingService;
            _taxService = taxService;
            _customerService = customerService;
            _discountService = discountService;
            _encryptionService = encryptionService;
            _currencyService = currencyService;
            _affiliateService = affiliateService;
            _mediator = mediator;
            _pdfService = pdfService;
            _storeContext = storeContext;
            _productReservationService = productReservationService;
            _auctionService = auctionService;
            _countryService = countryService;
            _paymentSettings = paymentSettings;
            _shippingSettings = shippingSettings;
            //_orderTotalCalculationService = orderTotalCalculationService;
            //_giftCardService = giftCardService;
            //_shoppingCartService = shoppingCartService;
            //_checkoutAttributeFormatter = checkoutAttributeFormatter;
            //_rewardPointsService = rewardPointsService;
            //_shoppingCartSettings = shoppingCartSettings;
            _orderSettings = orderSettings;
            _taxSettings = taxSettings;
            _localizationSettings = localizationSettings;
        }

        #endregion

        #region Utilities

        protected virtual async Task<PlaceAdContainter> PreparePlaceAdDetailsForRecurringPayment(ProcessPaymentRequest processPaymentRequest)
        {
            var details = new PlaceAdContainter();
            //customer
            details.Customer = await _customerService.GetCustomerById(processPaymentRequest.CustomerId);
            if (details.Customer == null)
                throw new ArgumentException("Customer is not set");

            //affiliate
            if (!string.IsNullOrEmpty(details.Customer.AffiliateId))
            {
                var affiliate = await _affiliateService.GetAffiliateById(details.Customer.AffiliateId);
                if (affiliate != null && affiliate.Active && !affiliate.Deleted)
                    details.AffiliateId = affiliate.Id;
            }

            // Recurring orders.Load initial order
            //details.InitialAd = await _orderService.GetAdById(processPaymentRequest.InitialAdId);
            if (details.InitialAd == null)
                throw new ArgumentException("Initial order is not set for recurring payment");

            details.InitialAd.Code = await _mediator.Send(new PrepareAdCodeCommand());
            processPaymentRequest.PaymentMethodSystemName = details.InitialAd.PaymentMethodSystemName;
            details.CustomerCurrencyCode = details.InitialAd.CustomerCurrencyCode;
            details.CustomerCurrencyRate = details.InitialAd.CurrencyRate;
            details.CustomerLanguage = await _languageService.GetLanguageById(details.InitialAd.CustomerLanguageId);
            details.CheckoutAttributesXml = details.InitialAd.CheckoutAttributesXml;
            details.CheckoutAttributeDescription = details.InitialAd.CheckoutAttributeDescription;
            details.CustomerTaxDisplayType = details.InitialAd.CustomerTaxDisplayType;
            //details.AdSubTotalInclTax = details.InitialAd.AdSubtotalInclTax;
            //details.AdSubTotalExclTax = details.InitialAd.AdSubtotalExclTax;
            //details.AdDiscountAmount = details.InitialAd.AdDiscount;
            //details.AdSubTotalDiscountExclTax = details.InitialAd.AdSubTotalDiscountExclTax;
            //details.AdSubTotalDiscountInclTax = details.InitialAd.AdSubTotalDiscountInclTax;
            //details.AdTotal = details.InitialAd.AdTotal;
            details.BillingAddress = details.InitialAd.BillingAddress;
            details.ShippingAddress = details.InitialAd.ShippingAddress;
            details.PickupPoint = details.InitialAd.PickupPoint;
            details.ShippingMethodName = details.InitialAd.ShippingMethod;
            details.ShippingRateComputationMethodSystemName = details.InitialAd.ShippingRateComputationMethodSystemName;
            var shoppingCartRequiresShipping = details.InitialAd.ShippingStatus != ShippingStatus.ShippingNotRequired;
            details.ShippingStatus = shoppingCartRequiresShipping ? ShippingStatus.NotYetShipped : ShippingStatus.ShippingNotRequired;
            details.PaymentAdditionalFeeInclTax = details.InitialAd.PaymentMethodAdditionalFeeInclTax;
            details.PaymentAdditionalFeeExclTax = details.InitialAd.PaymentMethodAdditionalFeeExclTax;
            //details.AdShippingTotalInclTax = details.InitialAd.AdShippingInclTax;
            //details.AdShippingTotalExclTax = details.InitialAd.AdShippingExclTax;
            //details.AdTaxTotal = details.InitialAd.AdTax;
            //processPaymentRequest.AdTotal = details.AdTotal;
            details.TaxRates = details.InitialAd.TaxRates;
            details.IsRecurringShoppingCart = true;

            return details;
        }

        protected virtual async Task<ProcessPaymentResult> PrepareProcessPaymentResult(ProcessPaymentRequest processPaymentRequest, PlaceAdContainter details)
        {
            //process payment
            ProcessPaymentResult processPaymentResult = null;
            //skip payment workflow if order total equals zero
            var skipPaymentWorkflow = details.AdTotal == decimal.Zero;
            if (!skipPaymentWorkflow)
            {
                var paymentMethod = _paymentService.LoadPaymentMethodBySystemName(processPaymentRequest.PaymentMethodSystemName);
                if (paymentMethod == null)
                    throw new GrandException("Payment method couldn't be loaded");

                //ensure that payment method is active
                if (!paymentMethod.IsPaymentMethodActive(_paymentSettings))
                    throw new GrandException("Payment method is not active");

                if (!processPaymentRequest.IsRecurringPayment)
                {
                    if (details.IsRecurringShoppingCart)
                    {
                        //recurring cart
                        var recurringPaymentType = _paymentService.GetRecurringPaymentType(processPaymentRequest.PaymentMethodSystemName);
                        switch (recurringPaymentType)
                        {
                            case RecurringPaymentType.NotSupported:
                                throw new GrandException("Recurring payments are not supported by selected payment method");
                            case RecurringPaymentType.Manual:
                            case RecurringPaymentType.Automatic:
                                processPaymentResult = await _paymentService.ProcessRecurringPayment(processPaymentRequest);
                                break;
                            default:
                                throw new GrandException("Not supported recurring payment type");
                        }
                    }
                    else
                    {
                        //standard cart
                        processPaymentResult = await _paymentService.ProcessPayment(processPaymentRequest);
                    }
                }
                else
                {
                    if (details.IsRecurringShoppingCart)
                    {
                        //Old credit card info
                        processPaymentRequest.CreditCardType = details.InitialAd.AllowStoringCreditCardNumber ? _encryptionService.DecryptText(details.InitialAd.CardType) : "";
                        processPaymentRequest.CreditCardName = details.InitialAd.AllowStoringCreditCardNumber ? _encryptionService.DecryptText(details.InitialAd.CardName) : "";
                        processPaymentRequest.CreditCardNumber = details.InitialAd.AllowStoringCreditCardNumber ? _encryptionService.DecryptText(details.InitialAd.CardNumber) : "";
                        processPaymentRequest.CreditCardCvv2 = details.InitialAd.AllowStoringCreditCardNumber ? _encryptionService.DecryptText(details.InitialAd.CardCvv2) : "";
                        try
                        {
                            processPaymentRequest.CreditCardExpireMonth = details.InitialAd.AllowStoringCreditCardNumber ? Convert.ToInt32(_encryptionService.DecryptText(details.InitialAd.CardExpirationMonth)) : 0;
                            processPaymentRequest.CreditCardExpireYear = details.InitialAd.AllowStoringCreditCardNumber ? Convert.ToInt32(_encryptionService.DecryptText(details.InitialAd.CardExpirationYear)) : 0;
                        }
                        catch { }

                        var recurringPaymentType = _paymentService.GetRecurringPaymentType(processPaymentRequest.PaymentMethodSystemName);
                        switch (recurringPaymentType)
                        {
                            case RecurringPaymentType.NotSupported:
                                throw new GrandException("Recurring payments are not supported by selected payment method");
                            case RecurringPaymentType.Manual:
                                processPaymentResult = await _paymentService.ProcessRecurringPayment(processPaymentRequest);
                                break;
                            case RecurringPaymentType.Automatic:
                                //payment is processed on payment gateway site
                                processPaymentResult = new ProcessPaymentResult();
                                break;
                            default:
                                throw new GrandException("Not supported recurring payment type");
                        }
                    }
                    else
                    {
                        throw new GrandException("No recurring products");
                    }
                }
            }
            else
            {
                //payment is not required
                if (processPaymentResult == null)
                    processPaymentResult = new ProcessPaymentResult();
                processPaymentResult.NewPaymentStatus = PaymentStatus.Paid;
            }

            if (processPaymentResult == null)
                throw new GrandException("processPaymentResult is not available");

            return processPaymentResult;
        }

        protected virtual Ad PrepareAd(ProcessPaymentRequest processPaymentRequest, ProcessPaymentResult processPaymentResult, PlaceAdContainter details)
        {
            var order = new Ad {
                StoreId = processPaymentRequest.StoreId,
                AdGuid = processPaymentRequest.OrderGuid,
                Code = "",
                CustomerId = details.Customer.Id,
                OwnerId = string.IsNullOrEmpty(details.Customer.OwnerId) ? details.Customer.Id : details.Customer.OwnerId,
                CustomerLanguageId = details.CustomerLanguage.Id,
                CustomerTaxDisplayType = details.CustomerTaxDisplayType,
                CustomerIp = _webHelper.GetCurrentIpAddress(),
                //AdSubtotalInclTax = Math.Round(details.AdSubTotalInclTax, 6),
                //AdSubtotalExclTax = Math.Round(details.AdSubTotalExclTax, 6),
                //AdSubTotalDiscountInclTax = Math.Round(details.AdSubTotalDiscountInclTax, 6),
                //AdSubTotalDiscountExclTax = Math.Round(details.AdSubTotalDiscountExclTax, 6),
                //AdShippingInclTax = Math.Round(details.AdShippingTotalInclTax, 6),
                //AdShippingExclTax = Math.Round(details.AdShippingTotalExclTax, 6),
                PaymentMethodAdditionalFeeInclTax = Math.Round(details.PaymentAdditionalFeeInclTax, 6),
                PaymentMethodAdditionalFeeExclTax = Math.Round(details.PaymentAdditionalFeeExclTax, 6),
                TaxRates = details.TaxRates,
                //AdTax = Math.Round(details.AdTaxTotal, 6),
                //AdTotal = Math.Round(details.AdTotal, 6),
                //AdDiscount = Math.Round(details.AdDiscountAmount, 6),
                RefundedAmount = decimal.Zero,
                CheckoutAttributeDescription = details.CheckoutAttributeDescription,
                CheckoutAttributesXml = details.CheckoutAttributesXml,
                CustomerCurrencyCode = details.CustomerCurrencyCode,
                PrimaryCurrencyCode = details.PrimaryCurrencyCode,
                CurrencyRate = details.CustomerCurrencyRate,
                AffiliateId = details.AffiliateId,
                AdStatus = AdStatus.Active,
                AllowStoringCreditCardNumber = processPaymentResult.AllowStoringCreditCardNumber,
                CardType = processPaymentResult.AllowStoringCreditCardNumber ? _encryptionService.EncryptText(processPaymentRequest.CreditCardType) : string.Empty,
                CardName = processPaymentResult.AllowStoringCreditCardNumber ? _encryptionService.EncryptText(processPaymentRequest.CreditCardName) : string.Empty,
                CardNumber = processPaymentResult.AllowStoringCreditCardNumber ? _encryptionService.EncryptText(processPaymentRequest.CreditCardNumber) : string.Empty,
                MaskedCreditCardNumber = _encryptionService.EncryptText(_paymentService.GetMaskedCreditCardNumber(processPaymentRequest.CreditCardNumber)),
                CardCvv2 = processPaymentResult.AllowStoringCreditCardNumber ? _encryptionService.EncryptText(processPaymentRequest.CreditCardCvv2) : string.Empty,
                CardExpirationMonth = processPaymentResult.AllowStoringCreditCardNumber ? _encryptionService.EncryptText(processPaymentRequest.CreditCardExpireMonth.ToString()) : string.Empty,
                CardExpirationYear = processPaymentResult.AllowStoringCreditCardNumber ? _encryptionService.EncryptText(processPaymentRequest.CreditCardExpireYear.ToString()) : string.Empty,
                PaymentMethodSystemName = processPaymentRequest.PaymentMethodSystemName,
                AuthorizationTransactionId = processPaymentResult.AuthorizationTransactionId,
                AuthorizationTransactionCode = processPaymentResult.AuthorizationTransactionCode,
                AuthorizationTransactionResult = processPaymentResult.AuthorizationTransactionResult,
                CaptureTransactionId = processPaymentResult.CaptureTransactionId,
                CaptureTransactionResult = processPaymentResult.CaptureTransactionResult,
                SubscriptionTransactionId = processPaymentResult.SubscriptionTransactionId,
                PaymentStatus = processPaymentResult.NewPaymentStatus,
                PaidDateUtc = null,
                BillingAddress = details.BillingAddress,
                ShippingAddress = details.ShippingAddress,
                ShippingStatus = details.ShippingStatus,
                ShippingMethod = details.ShippingMethodName,
                PickUpInStore = details.PickUpInStore,
                PickupPoint = details.PickupPoint,
                ShippingRateComputationMethodSystemName = details.ShippingRateComputationMethodSystemName,
                CustomValuesXml = processPaymentRequest.SerializeCustomValues(),
                VatNumber = details.Customer.GetAttributeFromEntity<string>(SystemCustomerAttributeNames.VatNumber),
                VatNumberStatusId = details.Customer.GetAttributeFromEntity<int>(SystemCustomerAttributeNames.VatNumberStatusId),
                CompanyName = details.Customer.GetAttributeFromEntity<string>(SystemCustomerAttributeNames.Company),
                FirstName = details.Customer.GetAttributeFromEntity<string>(SystemCustomerAttributeNames.FirstName),
                LastName = details.Customer.GetAttributeFromEntity<string>(SystemCustomerAttributeNames.LastName),
                CustomerEmail = details.Customer.Email,
                UrlReferrer = details.Customer.GetAttributeFromEntity<string>(SystemCustomerAttributeNames.LastUrlReferrer),
                ShippingOptionAttributeDescription = details.Customer.GetAttributeFromEntity<string>(SystemCustomerAttributeNames.ShippingOptionAttributeDescription, processPaymentRequest.StoreId),
                ShippingOptionAttributeXml = details.Customer.GetAttributeFromEntity<string>(SystemCustomerAttributeNames.ShippingOptionAttributeXml, processPaymentRequest.StoreId),
                SelectedPaymentMethodId = details.SelectedPaymentMethodId,
                CreatedOnUtc = DateTime.UtcNow
            };

            return order;
        }

        protected virtual async Task<PlaceAdContainter> PreparePlaceAdDetails(ProcessPaymentRequest processPaymentRequest)
        {
            var details = new PlaceAdContainter();

            //customer
            details.Customer = await _customerService.GetCustomerById(processPaymentRequest.CustomerId);
            if (details.Customer == null)
                throw new ArgumentException("Customer is not set");

            //affiliate
            if (!string.IsNullOrEmpty(details.Customer.AffiliateId))
            {
                var affiliate = await _affiliateService.GetAffiliateById(details.Customer.AffiliateId);
                if (affiliate != null && affiliate.Active && !affiliate.Deleted)
                    details.AffiliateId = affiliate.Id;
            }
            //customer currency
            var currencyTmp = await _currencyService.GetCurrencyById(details.Customer.GetAttributeFromEntity<string>(SystemCustomerAttributeNames.CurrencyId, processPaymentRequest.StoreId));
            var customerCurrency = (currencyTmp != null && currencyTmp.Published) ? currencyTmp : _workContext.WorkingCurrency;
            details.CustomerCurrencyCode = customerCurrency.CurrencyCode;
            var primaryStoreCurrency = await _currencyService.GetPrimaryStoreCurrency();
            details.CustomerCurrencyRate = customerCurrency.Rate / primaryStoreCurrency.Rate;
            details.PrimaryCurrencyCode = primaryStoreCurrency.CurrencyCode;

            //customer language
            details.CustomerLanguage = await _languageService.GetLanguageById(details.Customer.GetAttributeFromEntity<string>(SystemCustomerAttributeNames.LanguageId, processPaymentRequest.StoreId));

            if (details.CustomerLanguage == null || !details.CustomerLanguage.Published)
                details.CustomerLanguage = _workContext.WorkingLanguage;

            //check whether customer is guest
            if (details.Customer.IsGuest() && !_orderSettings.AnonymousCheckoutAllowed)
                throw new GrandException("Anonymous checkout is not allowed");

            //billing address
            if (details.Customer.BillingAddress == null)
                throw new GrandException("Billing address is not provided");

            if (!CommonHelper.IsValidEmail(details.Customer.BillingAddress.Email))
                throw new GrandException("Email is not valid");

            //clone billing address
            details.BillingAddress = (Address)details.Customer.BillingAddress.Clone();
            if (!String.IsNullOrEmpty(details.BillingAddress.CountryId))
            {
                var country = await _countryService.GetCountryById(details.BillingAddress.CountryId);
                if (country != null)
                    if (!country.AllowsBilling)
                        throw new GrandException(string.Format("Country '{0}' is not allowed for billing", country.Name));
            }

            //checkout attributes
            details.CheckoutAttributesXml = details.Customer.GetAttributeFromEntity<string>(SystemCustomerAttributeNames.CheckoutAttributes, processPaymentRequest.StoreId);
            //details.CheckoutAttributeDescription = await _checkoutAttributeFormatter.FormatAttributes(details.CheckoutAttributesXml, details.Customer);

            //load and validate customer shopping cart
            //details.Cart = details.Customer.ShoppingCartItems
            //    .Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart || sci.ShoppingCartType == ShoppingCartType.Auctions)
            //    .LimitPerStore(_shoppingCartSettings.CartsSharedBetweenStores, processPaymentRequest.StoreId)
            //    .ToList();

            if (!details.Cart.Any())
                throw new GrandException("Cart is empty");

            //validate the entire shopping cart
            //var warnings = await _shoppingCartService.GetShoppingCartWarnings(details.Cart,
            //    details.CheckoutAttributesXml,
            //    true);
            //if (warnings.Any())
            //{
            //    var warningsSb = new StringBuilder();
            //    foreach (string warning in warnings)
            //    {
            //        warningsSb.Append(warning);
            //        warningsSb.Append(";");
            //    }
            //    throw new GrandException(warningsSb.ToString());
            //}

            //validate individual cart items
            //foreach (var sci in details.Cart)
            //{
            //    var product = await _productService.GetProductById(sci.ProductId);
            //    var sciWarnings = await _shoppingCartService.GetShoppingCartItemWarnings(details.Customer, sci, product, false);
            //    if (sciWarnings.Any())
            //    {
            //        var warningsSb = new StringBuilder();
            //        foreach (string warning in sciWarnings)
            //        {
            //            warningsSb.Append(warning);
            //            warningsSb.Append(";");
            //        }
            //        throw new GrandException(warningsSb.ToString());
            //    }
            //}

            ////min totals validation
            //bool minAdSubtotalAmountOk = await ValidateMinAdSubtotalAmount(details.Cart);
            //if (!minAdSubtotalAmountOk)
            //{
            //    decimal minAdSubtotalAmount = await _currencyService.ConvertFromPrimaryStoreCurrency(_orderSettings.MinAdSubtotalAmount, _workContext.WorkingCurrency);
            //    throw new GrandException(string.Format(_localizationService.GetResource("Checkout.MinAdSubtotalAmount"), _priceFormatter.FormatPrice(minAdSubtotalAmount, true, false)));
            //}

            //bool minmaxAdTotalAmountOk = await ValidateAdTotalAmount(details.Customer, details.Cart);
            //if (!minmaxAdTotalAmountOk)
            //{
            //    throw new GrandException(_localizationService.GetResource("Checkout.MinMaxAdTotalAmount"));
            //}

            ////tax display type
            //if (_taxSettings.AllowCustomersToSelectTaxDisplayType)
            //    details.CustomerTaxDisplayType = (TaxDisplayType)details.Customer.GetAttributeFromEntity<int>(SystemCustomerAttributeNames.TaxDisplayTypeId, processPaymentRequest.StoreId);
            //else
            //    details.CustomerTaxDisplayType = _taxSettings.TaxDisplayType;

            ////sub total
            ////sub total (incl tax)
            //var shoppingCartSubTotal = await _orderTotalCalculationService.GetShoppingCartSubTotal(details.Cart, true);
            //decimal orderSubTotalDiscountAmount = shoppingCartSubTotal.discountAmount;
            //List<AppliedDiscount> orderSubTotalAppliedDiscounts = shoppingCartSubTotal.appliedDiscounts;
            //decimal subTotalWithoutDiscountBase = shoppingCartSubTotal.subTotalWithoutDiscount;
            //decimal subTotalWithDiscountBase = shoppingCartSubTotal.subTotalWithDiscount;

            //details.AdSubTotalInclTax = subTotalWithoutDiscountBase;
            //details.AdSubTotalDiscountInclTax = orderSubTotalDiscountAmount;

            //foreach (var disc in orderSubTotalAppliedDiscounts)
            //    if (!details.AppliedDiscounts.Where(x => x.DiscountId == disc.DiscountId).Any())
            //        details.AppliedDiscounts.Add(disc);

            ////sub total (excl tax)
            //shoppingCartSubTotal = await _orderTotalCalculationService.GetShoppingCartSubTotal(details.Cart, false);
            //orderSubTotalDiscountAmount = shoppingCartSubTotal.discountAmount;
            //orderSubTotalAppliedDiscounts = shoppingCartSubTotal.appliedDiscounts;
            //subTotalWithoutDiscountBase = shoppingCartSubTotal.subTotalWithoutDiscount;
            //subTotalWithDiscountBase = shoppingCartSubTotal.subTotalWithDiscount;

            //details.AdSubTotalExclTax = subTotalWithoutDiscountBase;
            //details.AdSubTotalDiscountExclTax = orderSubTotalDiscountAmount;

            ////shipping info
            //bool shoppingCartRequiresShipping = shoppingCartRequiresShipping = details.Cart.RequiresShipping();

            //if (shoppingCartRequiresShipping)
            //{
            //    var pickupPoint = details.Customer.GetAttributeFromEntity<string>(SystemCustomerAttributeNames.SelectedPickupPoint, processPaymentRequest.StoreId);
            //    if (_shippingSettings.AllowPickUpInStore && pickupPoint != null)
            //    {
            //        details.PickUpInStore = true;
            //        details.PickupPoint = await _shippingService.GetPickupPointById(pickupPoint);
            //    }
            //    else
            //    {
            //        if (details.Customer.ShippingAddress == null)
            //            throw new GrandException("Shipping address is not provided");

            //        if (!CommonHelper.IsValidEmail(details.Customer.ShippingAddress.Email))
            //            throw new GrandException("Email is not valid");

            //        //clone shipping address
            //        details.ShippingAddress = details.Customer.ShippingAddress;
            //        if (!String.IsNullOrEmpty(details.ShippingAddress.CountryId))
            //        {
            //            var country = await _countryService.GetCountryById(details.ShippingAddress.CountryId);
            //            if (country != null)
            //                if (!country.AllowsShipping)
            //                    throw new GrandException(string.Format("Country '{0}' is not allowed for shipping", country.Name));
            //        }
            //    }
            //    var shippingOption = details.Customer.GetAttributeFromEntity<ShippingOption>(SystemCustomerAttributeNames.SelectedShippingOption, processPaymentRequest.StoreId);
            //    if (shippingOption != null)
            //    {
            //        details.ShippingMethodName = shippingOption.Name;
            //        details.ShippingRateComputationMethodSystemName = shippingOption.ShippingRateComputationMethodSystemName;
            //    }
            //}
            //details.ShippingStatus = shoppingCartRequiresShipping
            //    ? ShippingStatus.NotYetShipped
            //    : ShippingStatus.ShippingNotRequired;

            ////shipping total

            //var shoppingCartShippingTotal = await _orderTotalCalculationService.GetShoppingCartShippingTotal(details.Cart, true);
            //decimal tax = shoppingCartShippingTotal.taxRate;
            //List<AppliedDiscount> shippingTotalDiscounts = shoppingCartShippingTotal.appliedDiscounts;
            //var orderShippingTotalInclTax = shoppingCartShippingTotal.shoppingCartShippingTotal;
            //var orderShippingTotalExclTax = (await _orderTotalCalculationService.GetShoppingCartShippingTotal(details.Cart, false)).shoppingCartShippingTotal;
            //if (!orderShippingTotalInclTax.HasValue || !orderShippingTotalExclTax.HasValue)
            //    throw new GrandException("Shipping total couldn't be calculated");

            //foreach (var disc in shippingTotalDiscounts)
            //{
            //    if (!details.AppliedDiscounts.Where(x => x.DiscountId == disc.DiscountId).Any())
            //        details.AppliedDiscounts.Add(disc);
            //}


            //details.AdShippingTotalInclTax = orderShippingTotalInclTax.Value;
            //details.AdShippingTotalExclTax = orderShippingTotalExclTax.Value;

            ////payment total
            //decimal paymentAdditionalFee = await _paymentService.GetAdditionalHandlingFee(details.Cart, processPaymentRequest.PaymentMethodSystemName);
            //details.PaymentAdditionalFeeInclTax = (await _taxService.GetPaymentMethodAdditionalFee(paymentAdditionalFee, true, details.Customer)).paymentPrice;
            //details.PaymentAdditionalFeeExclTax = (await _taxService.GetPaymentMethodAdditionalFee(paymentAdditionalFee, false, details.Customer)).paymentPrice;

            ////tax total
            ////tax amount
            //var (taxtotal, taxRates) = await _orderTotalCalculationService.GetTaxTotal(details.Cart);
            //SortedDictionary<decimal, decimal> taxRatesDictionary = taxRates;
            //details.AdTaxTotal = taxtotal;

            ////tax rates
            //foreach (var kvp in taxRatesDictionary)
            //{
            //    var taxRate = kvp.Key;
            //    var taxValue = kvp.Value;
            //    details.TaxRates += string.Format("{0}:{1};   ", taxRate.ToString(CultureInfo.InvariantCulture), taxValue.ToString(CultureInfo.InvariantCulture));
            //}


            ////order total (and applied discounts, gift cards, reward points)
            //var shoppingCartTotal = await _orderTotalCalculationService.GetShoppingCartTotal(details.Cart);
            //List<AppliedGiftCard> appliedGiftCards = shoppingCartTotal.appliedGiftCards;
            //List<AppliedDiscount> orderAppliedDiscounts = shoppingCartTotal.appliedDiscounts;
            //decimal orderDiscountAmount = shoppingCartTotal.discountAmount;
            //int redeemedRewardPoints = shoppingCartTotal.redeemedRewardPoints;
            //decimal redeemedRewardPointsAmount = shoppingCartTotal.redeemedRewardPointsAmount;
            //var orderTotal = shoppingCartTotal.shoppingCartTotal;
            //if (!orderTotal.HasValue)
            //    throw new GrandException("Ad total couldn't be calculated");

            //details.AdDiscountAmount = orderDiscountAmount;
            //details.RedeemedRewardPoints = redeemedRewardPoints;
            //details.RedeemedRewardPointsAmount = redeemedRewardPointsAmount;
            //details.AppliedGiftCards = appliedGiftCards;
            //details.AdTotal = orderTotal.Value;

            ////discount history
            //foreach (var disc in orderAppliedDiscounts)
            //{
            //    if (!details.AppliedDiscounts.Where(x => x.DiscountId == disc.DiscountId).Any())
            //        details.AppliedDiscounts.Add(disc);
            //}

            //details.IsRecurringShoppingCart = details.Cart.IsRecurring();
            //if (details.IsRecurringShoppingCart)
            //{
            //    var (info, cycleLength, cyclePeriod, totalCycles) = await details.Cart.GetRecurringCycleInfo(_localizationService, _productService);

            //    int recurringCycleLength = cycleLength;
            //    RecurringProductCyclePeriod recurringCyclePeriod = cyclePeriod;
            //    int recurringTotalCycles = totalCycles;
            //    string recurringCyclesError = info;
            //    if (!string.IsNullOrEmpty(recurringCyclesError))
            //        throw new GrandException(recurringCyclesError);

            //    processPaymentRequest.RecurringCycleLength = recurringCycleLength;
            //    processPaymentRequest.RecurringCyclePeriod = recurringCyclePeriod;
            //    processPaymentRequest.RecurringTotalCycles = recurringTotalCycles;
            //}

            //processPaymentRequest.AdTotal = details.AdTotal;

            return details;
        }

        protected virtual async Task<Ad> SaveAdDetails(PlaceAdContainter details, Ad order)
        {
            var reservationsToUpdate = new List<ProductReservation>();
            var bidsToUpdate = new List<Bid>();

            //move shopping cart items to order items
            foreach (var sc in details.Cart)
            {
                //prices
                decimal taxRate;
                List<AppliedDiscount> scDiscounts;
                decimal discountAmount;
                decimal commissionRate;
                decimal scUnitPrice = (await _priceCalculationService.GetUnitPrice(sc)).unitprice;
                decimal scUnitPriceWithoutDisc = (await _priceCalculationService.GetUnitPrice(sc, false)).unitprice;

                var product = await _productService.GetProductById(sc.ProductId);
                var subtotal = await _priceCalculationService.GetSubTotal(sc, true);
                decimal scSubTotal = subtotal.subTotal;
                discountAmount = subtotal.discountAmount;
                scDiscounts = subtotal.appliedDiscounts;

                if (string.IsNullOrEmpty(product.VendorId))
                {
                    commissionRate = 0;
                }
                else
                {
                    commissionRate = (await _vendorService.GetVendorById(product.VendorId)).Commission;
                }

                var prices = await _taxService.GetTaxProductPrice(product, details.Customer, scUnitPrice, scUnitPriceWithoutDisc, scSubTotal, discountAmount, _taxSettings.PricesIncludeTax);
                taxRate = prices.taxRate;
                decimal scUnitPriceWithoutDiscInclTax = prices.UnitPriceWihoutDiscInclTax;
                decimal scUnitPriceWithoutDiscExclTax = prices.UnitPriceWihoutDiscExclTax;
                decimal scUnitPriceInclTax = prices.UnitPriceInclTax;
                decimal scUnitPriceExclTax = prices.UnitPriceExclTax;
                decimal scSubTotalInclTax = prices.SubTotalInclTax;
                decimal scSubTotalExclTax = prices.SubTotalExclTax;
                decimal discountAmountInclTax = prices.discountAmountInclTax;
                decimal discountAmountExclTax = prices.discountAmountExclTax;

                foreach (var disc in scDiscounts)
                {
                    if (!details.AppliedDiscounts.Where(x => x.DiscountId == disc.DiscountId).Any())
                        details.AppliedDiscounts.Add(disc);
                }

                //attributes
                string attributeDescription = await _productAttributeFormatter.FormatAttributes(product, sc.AttributesXml, details.Customer);

                //if (string.IsNullOrEmpty(attributeDescription) && sc.ShoppingCartType == ShoppingCartType.Auctions)
                //    attributeDescription = _localizationService.GetResource("ShoppingCart.auctionwonon") + " " + product.AvailableEndDateTimeUtc;

                var itemWeight = await _shippingService.GetShoppingCartItemWeight(sc);

                var warehouseId = !string.IsNullOrEmpty(sc.WarehouseId) ? sc.WarehouseId : _storeContext.CurrentStore.DefaultWarehouseId;
                if (!product.UseMultipleWarehouses && string.IsNullOrEmpty(warehouseId))
                {
                    if (!string.IsNullOrEmpty(product.WarehouseId))
                    {
                        warehouseId = product.WarehouseId;
                    }
                }

                //save order item
                var orderItem = new AdItem {
                    AdItemGuid = Guid.NewGuid(),
                    ProductId = sc.ProductId,
                    VendorId = product.VendorId,
                    WarehouseId = warehouseId,
                    UnitPriceWithoutDiscInclTax = Math.Round(scUnitPriceWithoutDiscInclTax, 6),
                    UnitPriceWithoutDiscExclTax = Math.Round(scUnitPriceWithoutDiscExclTax, 6),
                    UnitPriceInclTax = Math.Round(scUnitPriceInclTax, 6),
                    UnitPriceExclTax = Math.Round(scUnitPriceExclTax, 6),
                    PriceInclTax = Math.Round(scSubTotalInclTax, 6),
                    PriceExclTax = Math.Round(scSubTotalExclTax, 6),
                    OriginalProductCost = await _priceCalculationService.GetProductCost(product, sc.AttributesXml),
                    AttributeDescription = attributeDescription,
                    AttributesXml = sc.AttributesXml,
                    Quantity = sc.Quantity,
                    DiscountAmountInclTax = Math.Round(discountAmountInclTax, 6),
                    DiscountAmountExclTax = Math.Round(discountAmountExclTax, 6),
                    DownloadCount = 0,
                    IsDownloadActivated = false,
                    LicenseDownloadId = "",
                    ItemWeight = itemWeight,
                    RentalStartDateUtc = sc.RentalStartDateUtc,
                    RentalEndDateUtc = sc.RentalEndDateUtc,
                    CreatedOnUtc = DateTime.UtcNow,
                    Commission = Math.Round((commissionRate * scSubTotal / 100), 2),
                };

                string reservationInfo = "";
                if (product.ProductType == ProductType.Reservation)
                {
                    if (sc.RentalEndDateUtc == default(DateTime) || sc.RentalEndDateUtc == null)
                    {
                        reservationInfo = sc.RentalStartDateUtc.ToString();
                    }
                    else
                    {
                        reservationInfo = sc.RentalStartDateUtc + " - " + sc.RentalEndDateUtc;
                    }
                    if (!string.IsNullOrEmpty(sc.Parameter))
                    {
                        reservationInfo += "<br>" + string.Format(_localizationService.GetResource("ShoppingCart.Reservation.Option"), sc.Parameter);
                    }
                    if (!string.IsNullOrEmpty(sc.Duration))
                    {
                        reservationInfo += "<br>" + _localizationService.GetResource("Products.Duration") + ": " + sc.Duration;
                    }
                }
                if (!string.IsNullOrEmpty(reservationInfo))
                {
                    if (!string.IsNullOrEmpty(orderItem.AttributeDescription))
                    {
                        orderItem.AttributeDescription += "<br>" + reservationInfo;
                    }
                    else
                    {
                        orderItem.AttributeDescription = reservationInfo;
                    }
                }

                order.AdItems.Add(orderItem);

                await _productService.UpdateSold(sc.ProductId, sc.Quantity);

                //gift cards
                if (product.IsGiftCard)
                {
                    _productAttributeParser.GetGiftCardAttribute(sc.AttributesXml,
                        out string giftCardRecipientName, out string giftCardRecipientEmail,
                        out string giftCardSenderName, out string giftCardSenderEmail, out string giftCardMessage);

                    //for (int i = 0; i < sc.Quantity; i++)
                    //{
                    //    var gc = new GiftCard {
                    //        GiftCardType = product.GiftCardType,
                    //        PurchasedWithAdItem = orderItem,
                    //        Amount = product.OverriddenGiftCardAmount ?? scUnitPriceExclTax,
                    //        IsGiftCardActivated = false,
                    //        GiftCardCouponCode = _giftCardService.GenerateGiftCardCode(),
                    //        RecipientName = giftCardRecipientName,
                    //        RecipientEmail = giftCardRecipientEmail,
                    //        SenderName = giftCardSenderName,
                    //        SenderEmail = giftCardSenderEmail,
                    //        Message = giftCardMessage,
                    //        IsRecipientNotified = false,
                    //        CreatedOnUtc = DateTime.UtcNow
                    //    };
                    //    await _giftCardService.InsertGiftCard(gc);
                    //}
                }

                //reservations
                if (product.ProductType == ProductType.Reservation)
                {
                    if (!string.IsNullOrEmpty(sc.ReservationId))
                    {
                        var reservation = await _productReservationService.GetProductReservation(sc.ReservationId);
                        reservationsToUpdate.Add(reservation);
                    }

                    if (sc.RentalStartDateUtc.HasValue && sc.RentalEndDateUtc.HasValue)
                    {
                        var reservations = await _productReservationService.GetProductReservationsByProductId(product.Id, true, null);
                        var grouped = reservations.GroupBy(x => x.Resource);

                        IGrouping<string, ProductReservation> groupToBook = null;
                        foreach (var group in grouped)
                        {
                            bool groupCanBeBooked = true;
                            if (product.IncBothDate && product.IntervalUnitType == IntervalUnit.Day)
                            {
                                for (DateTime iterator = sc.RentalStartDateUtc.Value; iterator <= sc.RentalEndDateUtc.Value; iterator += new TimeSpan(24, 0, 0))
                                {
                                    if (!group.Select(x => x.Date).Contains(iterator))
                                    {
                                        groupCanBeBooked = false;
                                        break;
                                    }
                                }
                            }
                            else
                            {
                                for (DateTime iterator = sc.RentalStartDateUtc.Value; iterator < sc.RentalEndDateUtc.Value; iterator += new TimeSpan(24, 0, 0))
                                {
                                    if (!group.Select(x => x.Date).Contains(iterator))
                                    {
                                        groupCanBeBooked = false;
                                        break;
                                    }
                                }
                            }
                            if (groupCanBeBooked)
                            {
                                groupToBook = group;
                                break;
                            }
                        }

                        if (groupToBook == null)
                        {
                            throw new Exception("ShoppingCart.Reservation.Nofreereservationsinthisperiod");
                        }
                        else
                        {
                            var temp = groupToBook.AsQueryable();
                            if (product.IncBothDate && product.IntervalUnitType == IntervalUnit.Day)
                            {
                                temp = temp.Where(x => x.Date >= sc.RentalStartDateUtc && x.Date <= sc.RentalEndDateUtc);
                            }
                            else
                            {
                                temp = temp.Where(x => x.Date >= sc.RentalStartDateUtc && x.Date < sc.RentalEndDateUtc);
                            }

                            //foreach (var item in temp)
                            //{
                            //    item.AdId = order.AdGuid.ToString();
                            //    await _productReservationService.UpdateProductReservation(item);
                            //}

                            reservationsToUpdate.AddRange(temp);
                        }
                    }
                }

                ////auctions
                //if (sc.ShoppingCartType == ShoppingCartType.Auctions)
                //{
                //    var bid = (await _auctionService.GetBidsByProductId(product.Id)).Where(x => x.Amount == product.HighestBid).FirstOrDefault();
                //    if (bid == null)
                //        throw new ArgumentNullException("bid");

                //    bidsToUpdate.Add(bid);
                //}

                //if (product.ProductType == ProductType.Auction && sc.ShoppingCartType == ShoppingCartType.ShoppingCart)
                //{
                //    await _auctionService.UpdateAuctionEnded(product, true, true);
                //    await _auctionService.UpdateHighestBid(product, product.Price, order.CustomerId);
                //    await _workflowMessageService.SendAuctionEndedCustomerNotificationBin(product, order.CustomerId, order.CustomerLanguageId, order.StoreId);
                //    await _auctionService.InsertBid(new Bid() {
                //        CustomerId = order.CustomerId,
                //        AdId = order.Id,
                //        Amount = product.Price,
                //        Date = DateTime.UtcNow,
                //        ProductId = product.Id,
                //        StoreId = order.StoreId,
                //        Win = true,
                //        Bin = true,
                //    });
                //}
                if (product.ProductType == ProductType.Auction && _orderSettings.UnpublishAuctionProduct)
                {
                    await _productService.UnpublishProduct(product);
                }

                //inventory
                await _productService.AdjustInventory(product, -sc.Quantity, sc.AttributesXml, warehouseId);
            }

            //insert order
            await _orderService.InsertAd(order);

            var reserved = await _productReservationService.GetCustomerReservationsHelpers(order.CustomerId);
            foreach (var res in reserved)
            {
                await _productReservationService.DeleteCustomerReservationsHelper(res);
            }

            //foreach (var resToUpdate in reservationsToUpdate)
            //{
            //    resToUpdate.AdId = order.Id;
            //    await _productReservationService.UpdateProductReservation(resToUpdate);
            //}

            //foreach (var bid in bidsToUpdate)
            //{
            //    bid.AdId = order.Id;
            //    await _auctionService.UpdateBid(bid);
            //}

            ////clear shopping cart
            //await _customerService.ClearShoppingCartItem(order.CustomerId, details.Cart);

            ////product also purchased
            //await _orderService.InsertProductAlsoPurchased(order);

            //if (!details.Customer.HasContributions)
            //{
            //    await _customerService.UpdateContributions(details.Customer);
            //}

            ////discount usage history
            //foreach (var discount in details.AppliedDiscounts)
            //{
            //    var duh = new DiscountUsageHistory {
            //        DiscountId = discount.DiscountId,
            //        CouponCode = discount.CouponCode,
            //        AdId = order.Id,
            //        CustomerId = order.CustomerId,
            //        CreatedOnUtc = DateTime.UtcNow
            //    };
            //    await _discountService.InsertDiscountUsageHistory(duh);
            //}

            ////gift card usage history
            //if (details.AppliedGiftCards != null)
            //    foreach (var agc in details.AppliedGiftCards)
            //    {
            //        decimal amountUsed = agc.AmountCanBeUsed;
            //        var gcuh = new GiftCardUsageHistory {
            //            GiftCardId = agc.GiftCard.Id,
            //            UsedWithAdId = order.Id,
            //            UsedValue = amountUsed,
            //            CreatedOnUtc = DateTime.UtcNow
            //        };
            //        agc.GiftCard.GiftCardUsageHistory.Add(gcuh);
            //        await _giftCardService.UpdateGiftCard(agc.GiftCard);
            //    }

            ////reset checkout data
            //await _customerService.ResetCheckoutData(details.Customer, order.StoreId, clearCouponCodes: true, clearCheckoutAttributes: true);

            return order;
        }

        protected virtual async Task<Ad> SaveAdDetailsForReccuringPayment(PlaceAdContainter details, Ad order)
        {
            #region recurring payment

            var initialAdItems = details.InitialAd.AdItems;
            foreach (var orderItem in initialAdItems)
            {
                //save item
                var newAdItem = new AdItem {
                    AdItemGuid = Guid.NewGuid(),
                    ProductId = orderItem.ProductId,
                    VendorId = orderItem.VendorId,
                    WarehouseId = orderItem.WarehouseId,
                    UnitPriceWithoutDiscInclTax = orderItem.UnitPriceWithoutDiscInclTax,
                    UnitPriceWithoutDiscExclTax = orderItem.UnitPriceWithoutDiscExclTax,
                    UnitPriceInclTax = orderItem.UnitPriceInclTax,
                    UnitPriceExclTax = orderItem.UnitPriceExclTax,
                    PriceInclTax = orderItem.PriceInclTax,
                    PriceExclTax = orderItem.PriceExclTax,
                    OriginalProductCost = orderItem.OriginalProductCost,
                    AttributeDescription = orderItem.AttributeDescription,
                    AttributesXml = orderItem.AttributesXml,
                    Quantity = orderItem.Quantity,
                    DiscountAmountInclTax = orderItem.DiscountAmountInclTax,
                    DiscountAmountExclTax = orderItem.DiscountAmountExclTax,
                    DownloadCount = 0,
                    IsDownloadActivated = false,
                    LicenseDownloadId = "",
                    ItemWeight = orderItem.ItemWeight,
                    RentalStartDateUtc = orderItem.RentalStartDateUtc,
                    RentalEndDateUtc = orderItem.RentalEndDateUtc,
                    CreatedOnUtc = DateTime.UtcNow,
                    Commission = orderItem.Commission
                };
                order.AdItems.Add(newAdItem);

                //gift cards
                var product = await _productService.GetProductById(orderItem.ProductId);
                if (product.IsGiftCard)
                {
                    _productAttributeParser.GetGiftCardAttribute(orderItem.AttributesXml,
                        out string giftCardRecipientName, out string giftCardRecipientEmail,
                        out string giftCardSenderName, out string giftCardSenderEmail, out string giftCardMessage);

                    //for (int i = 0; i < orderItem.Quantity; i++)
                    //{
                    //    var gc = new GiftCard {
                    //        GiftCardType = product.GiftCardType,
                    //        PurchasedWithAdItem = newAdItem,
                    //        Amount = orderItem.UnitPriceExclTax,
                    //        IsGiftCardActivated = false,
                    //        GiftCardCouponCode = _giftCardService.GenerateGiftCardCode(),
                    //        RecipientName = giftCardRecipientName,
                    //        RecipientEmail = giftCardRecipientEmail,
                    //        SenderName = giftCardSenderName,
                    //        SenderEmail = giftCardSenderEmail,
                    //        Message = giftCardMessage,
                    //        IsRecipientNotified = false,
                    //        CreatedOnUtc = DateTime.UtcNow
                    //    };
                    //    await _giftCardService.InsertGiftCard(gc);
                    //}
                }

                //inventory
                await _productService.AdjustInventory(product, -orderItem.Quantity, orderItem.AttributesXml, orderItem.WarehouseId);
            }

            //insert order
            await _orderService.InsertAd(order);

            return order;

            #endregion
        }

        protected virtual async Task UpdateCustomer(Ad order)
        {
            //Update customer reminder history
            await _mediator.Send(new UpdateCustomerReminderHistoryCommand() { CustomerId = order.CustomerId, AdId = order.Id });

            //Updated field "free shipping" after added a new order
            await _customerService.UpdateFreeShipping(order.CustomerId, false);

            //Update field Last purchase date after added a new order
            await _customerService.UpdateCustomerLastPurchaseDate(order.CustomerId, order.CreatedOnUtc);

            //Update field Last purchase date after added a new order
            await _customerService.UpdateCustomerLastUpdateCartDate(order.CustomerId, null);

        }

        protected virtual async Task CreateRecurringPayment(ProcessPaymentRequest processPaymentRequest, Ad order)
        {
            //var rp = new RecurringPayment {
            //    CycleLength = processPaymentRequest.RecurringCycleLength,
            //    CyclePeriod = processPaymentRequest.RecurringCyclePeriod,
            //    TotalCycles = processPaymentRequest.RecurringTotalCycles,
            //    StartDateUtc = DateTime.UtcNow,
            //    IsActive = true,
            //    CreatedOnUtc = DateTime.UtcNow,
            //    InitialAd = order,
            //};
            //await _orderService.InsertRecurringPayment(rp);


            var recurringPaymentType = _paymentService.GetRecurringPaymentType(processPaymentRequest.PaymentMethodSystemName);
            switch (recurringPaymentType)
            {
                case RecurringPaymentType.NotSupported:
                    {
                        //not supported
                    }
                    break;
                case RecurringPaymentType.Manual:
                    //{
                    //    //first payment
                    //    var rph = new RecurringPaymentHistory {
                    //        CreatedOnUtc = DateTime.UtcNow,
                    //        AdId = order.Id,
                    //        RecurringPaymentId = rp.Id
                    //    };
                    //    rp.RecurringPaymentHistory.Add(rph);
                    //    await _orderService.UpdateRecurringPayment(rp);
                    //}
                    break;
                case RecurringPaymentType.Automatic:
                    {
                        //will be created later (process is automated)
                    }
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Return back redeemded reward points to a customer (spent when placing an order)
        /// </summary>
        /// <param name="order">Ad</param>
        protected virtual async Task ReturnBackRedeemedRewardPoints(Ad order)
        {
            //were some points redeemed when placing an order?
            if (order.RedeemedRewardPointsEntry == null)
                return;

            //return back
            //await _rewardPointsService.AddRewardPointsHistory(order.CustomerId, -order.RedeemedRewardPointsEntry.Points, order.StoreId,
            //    string.Format(_localizationService.GetResource("RewardPoints.Message.ReturnedForAd"), order.AdNumber));

        }

        /// <summary>
        /// Process order paid status
        /// </summary>
        /// <param name="order">Ad</param>
        protected virtual async Task ProcessAdPaid(Ad order)
        {
            if (order == null)
                throw new ArgumentNullException("order");

            //raise event
            //await _mediator.Publish(new AdPaidEvent(order));

            ////order paid email notification
            //if (order.AdTotal != decimal.Zero)
            //{
            //    //we should not send it for free ($0 total) orders?
            //    //remove this "if" statement if you want to send it in this case

            //    var orderPaidAttachmentFilePath = _orderSettings.AttachPdfInvoiceToAdPaidEmail && !_orderSettings.AttachPdfInvoiceToBinary ?
            //        await _pdfService.PrintAdToPdf(order, "")
            //        : null;
            //    var orderPaidAttachmentFileName = _orderSettings.AttachPdfInvoiceToAdPaidEmail && !_orderSettings.AttachPdfInvoiceToBinary ?
            //        "order.pdf" : null;

            //    var orderPaidAttachments = _orderSettings.AttachPdfInvoiceToAdPaidEmail && _orderSettings.AttachPdfInvoiceToBinary ?
            //        new List<string> { await _pdfService.SaveAdToBinary(order, "") } : new List<string>();

            //    await _workflowMessageService.SendAdPaidCustomerNotification(order, order.CustomerLanguageId,
            //        orderPaidAttachmentFilePath, orderPaidAttachmentFileName, orderPaidAttachments);

            //    await _workflowMessageService.SendAdPaidStoreOwnerNotification(order, _localizationSettings.DefaultAdminLanguageId);
            //    var vendors = await GetVendorsInAd(order);
            //    foreach (var vendor in vendors)
            //    {
            //        await _workflowMessageService.SendAdPaidVendorNotification(order, vendor, _localizationSettings.DefaultAdminLanguageId);
            //    }
            //    //TODO add "order paid email sent" order note
            //}
        }

        /// <summary>
        /// Get a list of vendors in order (order items)
        /// </summary>
        /// <param name="order">Ad</param>
        /// <returns>Vendors</returns>
        protected virtual async Task<IList<Vendor>> GetVendorsInAd(Ad order)
        {
            var vendors = new List<Vendor>();
            foreach (var orderItem in order.AdItems)
            {
                //find existing
                var vendor = vendors.FirstOrDefault(v => v.Id == orderItem.VendorId);
                if (vendor == null && !string.IsNullOrEmpty(orderItem.VendorId))
                {
                    //not found. load by Id
                    vendor = await _vendorService.GetVendorById(orderItem.VendorId);
                    if (vendor != null && !vendor.Deleted && vendor.Active)
                    {
                        vendors.Add(vendor);
                    }
                }
            }

            return vendors;
        }

        #endregion

        #region Methods
        /// <summary>
        /// Send notification order 
        /// </summary>
        /// <param name="order">Ad</param>
        public virtual async Task SendNotification(Ad order)
        {
            //notes, messages
            if (_workContext.OriginalCustomerIfImpersonated != null)
            {
                //this order is placed by a store administrator impersonating a customer
                await _orderService.InsertAdNote(new AdNote {
                    Note = string.Format("Ad placed by a store owner ('{0}'. ID = {1}) impersonating the customer.",
                        _workContext.OriginalCustomerIfImpersonated.Email, _workContext.OriginalCustomerIfImpersonated.Id),
                    DisplayToCustomer = false,
                    CreatedOnUtc = DateTime.UtcNow,
                    AdId = order.Id,
                });
            }
            else
            {
                await _orderService.InsertAdNote(new AdNote {
                    Note = "Ad placed",
                    DisplayToCustomer = false,
                    CreatedOnUtc = DateTime.UtcNow,
                    AdId = order.Id,

                });
            }

            //send email notifications
            //int orderPlacedStoreOwnerNotificationQueuedEmailId = await _workflowMessageService.SendAdPlacedStoreOwnerNotification(order, _localizationSettings.DefaultAdminLanguageId);
            int orderPlacedStoreOwnerNotificationQueuedEmailId = 0;
            if (orderPlacedStoreOwnerNotificationQueuedEmailId > 0)
            {
                await _orderService.InsertAdNote(new AdNote {
                    Note = "\"Ad placed\" email (to store owner) has been queued",
                    DisplayToCustomer = false,
                    CreatedOnUtc = DateTime.UtcNow,
                    AdId = order.Id,

                });
            }

            //var orderPlacedAttachmentFilePath = _orderSettings.AttachPdfInvoiceToAdPlacedEmail && !_orderSettings.AttachPdfInvoiceToBinary ?
            //    await _pdfService.PrintAdToPdf(order, order.CustomerLanguageId) : null;
            //var orderPlacedAttachmentFileName = _orderSettings.AttachPdfInvoiceToAdPlacedEmail && !_orderSettings.AttachPdfInvoiceToBinary ?
            //    "order.pdf" : null;
            //var orderPlacedAttachments = _orderSettings.AttachPdfInvoiceToAdPlacedEmail && _orderSettings.AttachPdfInvoiceToBinary ?
            //    new List<string> { await _pdfService.SaveAdToBinary(order, order.CustomerLanguageId) } : new List<string>();

            //int orderPlacedCustomerNotificationQueuedEmailId = await _workflowMessageService
            //    .SendAdPlacedCustomerNotification(order, order.CustomerLanguageId, orderPlacedAttachmentFilePath, orderPlacedAttachmentFileName, orderPlacedAttachments);
            
            int orderPlacedCustomerNotificationQueuedEmailId = 0;

            if (orderPlacedCustomerNotificationQueuedEmailId > 0)
            {
                await _orderService.InsertAdNote(new AdNote {
                    Note = "\"Ad placed\" email (to customer) has been queued",
                    DisplayToCustomer = false,
                    CreatedOnUtc = DateTime.UtcNow,
                    AdId = order.Id,

                });
            }

            var vendors = await GetVendorsInAd(order);
            //foreach (var vendor in vendors)
            //{
            //    int orderPlacedVendorNotificationQueuedEmailId = await _workflowMessageService.SendAdPlacedVendorNotification(order, vendor, _localizationSettings.DefaultAdminLanguageId);
            //    if (orderPlacedVendorNotificationQueuedEmailId > 0)
            //    {
            //        await _orderService.InsertAdNote(new AdNote {
            //            Note = "\"Ad placed\" email (to vendor) has been queued",
            //            DisplayToCustomer = false,
            //            CreatedOnUtc = DateTime.UtcNow,
            //            AdId = order.Id,
            //        });
            //    }
            //}
        }

        /// <summary>
        /// Places an order
        /// </summary>
        /// <param name="processPaymentRequest">Process payment request</param>
        /// <returns>Place order result</returns>
        public virtual async Task<PlaceAdResult> PlaceAd(ProcessPaymentRequest processPaymentRequest)
        {
            if (processPaymentRequest == null)
                throw new ArgumentNullException("processPaymentRequest");

            var result = new PlaceAdResult();
            try
            {
                //if (processPaymentRequest.AdGuid == Guid.Empty)
                //    processPaymentRequest.AdGuid = Guid.NewGuid();

                //if (string.IsNullOrEmpty(processPaymentRequest.AdCode))
                //    processPaymentRequest.AdCode = await _mediator.Send(new PrepareAdCodeCommand());

                ////prepare order details
                //var details = !processPaymentRequest.IsRecurringPayment ?
                //    await PreparePlaceAdDetails(processPaymentRequest) :
                //    await PreparePlaceAdDetailsForRecurringPayment(processPaymentRequest);

                ////event notification
                //await _mediator.PlaceAdDetailsEvent(result, details);

                //return if exist errors
                if (result.Errors.Any())
                    return result;

                #region Payment workflow
                var details = await PreparePlaceAdDetails(processPaymentRequest);
                var processPaymentResult = await PrepareProcessPaymentResult(processPaymentRequest, details);

                #endregion

                if (processPaymentResult.Success)
                {
                    #region Save order details

                    var order = PrepareAd(processPaymentRequest, processPaymentResult, details);

                    if (!processPaymentRequest.IsRecurringPayment)
                    {
                        result.PlacedAd = await SaveAdDetails(details, order);
                    }
                    else
                    {
                        result.PlacedAd = await SaveAdDetailsForReccuringPayment(details, order);

                    }
                    //recurring orders
                    if (details.IsRecurringShoppingCart && !processPaymentRequest.IsRecurringPayment)
                    {
                        //create recurring payment (the first payment)
                        await CreateRecurringPayment(processPaymentRequest, order);
                    }
                    //reward points history
                    if (details.RedeemedRewardPointsAmount > decimal.Zero)
                    {
                        //var rph = await _rewardPointsService.AddRewardPointsHistory(details.Customer.Id,
                        //    -details.RedeemedRewardPoints, order.StoreId,
                        //    string.Format(_localizationService.GetResource("RewardPoints.Message.RedeemedForAd", order.CustomerLanguageId), order.AdNumber),
                        //    order.Id, details.RedeemedRewardPointsAmount);
                        //order.RewardPointsWereAdded = true;
                        //order.RedeemedRewardPointsEntry = rph;
                        //await _orderService.UpdateAd(order);
                    }

                    #endregion

                    #region Notifications & notes

                    await SendNotification(order);

                    //check order status
                    //await _mediator.Send(new CheckAdStatusCommand() { Ad = order });

                    //update customer 
                    await UpdateCustomer(order);

                    //raise event       
                    //await _mediator.Publish(new AdPlacedEvent(order));

                    if (order.PaymentStatus == PaymentStatus.Paid)
                    {
                        await ProcessAdPaid(order);
                    }
                    #endregion
                }
                else
                {
                    foreach (var paymentError in processPaymentResult.Errors)
                        result.AddError(string.Format(_localizationService.GetResource("Checkout.PaymentError"), paymentError));
                }
            }
            catch (Exception exc)
            {
                _logger.Error(exc.Message, exc);
                result.AddError(exc.Message);
            }

            #region Process errors

            string error = "";
            for (int i = 0; i < result.Errors.Count; i++)
            {
                error += string.Format("Error {0}: {1}", i + 1, result.Errors[i]);
                if (i != result.Errors.Count - 1)
                    error += ". ";
            }
            if (!string.IsNullOrEmpty(error))
            {
                //log it
                string logError = string.Format("Error while placing order. {0}", error);
                var customer = await _customerService.GetCustomerById(processPaymentRequest.CustomerId);
                _logger.Error(logError, customer: customer);
            }

            #endregion

            return result;
        }

        /// <summary>
        /// Process next recurring psayment
        /// </summary>
        /// <param name="recurringPayment">Recurring payment</param>
        public virtual async Task ProcessNextRecurringPayment(RecurringPayment recurringPayment)
        {
            if (recurringPayment == null)
                throw new ArgumentNullException("recurringPayment");
            try
            {
                if (!recurringPayment.IsActive)
                    throw new GrandException("Recurring payment is not active");

                var initialAd = recurringPayment.InitialOrder;

                if (initialAd == null)
                    throw new GrandException("Initial order could not be loaded");

                var customer = await _customerService.GetCustomerById(initialAd.CustomerId);
                if (customer == null)
                    throw new GrandException("Customer could not be loaded");

                var nextPaymentDate = recurringPayment.NextPaymentDate;
                if (!nextPaymentDate.HasValue)
                    throw new GrandException("Next payment date could not be calculated");

                //payment info
                var paymentInfo = new ProcessPaymentRequest {
                    StoreId = initialAd.StoreId,
                    CustomerId = customer.Id,
                    OrderGuid = Guid.NewGuid(),
                    IsRecurringPayment = true,
                    InitialOrderId = initialAd.Id,
                    RecurringCycleLength = recurringPayment.CycleLength,
                    RecurringCyclePeriod = recurringPayment.CyclePeriod,
                    RecurringTotalCycles = recurringPayment.TotalCycles,
                };

                //place a new order
                var result = await PlaceAd(paymentInfo);
                if (result.Success)
                {
                    if (result.PlacedAd == null)
                        throw new GrandException("Placed order could not be loaded");

                    var rph = new RecurringPaymentHistory {
                        RecurringPaymentId = recurringPayment.Id,
                        CreatedOnUtc = DateTime.UtcNow,
                        OrderId = result.PlacedAd.Id,
                    };
                    recurringPayment.RecurringPaymentHistory.Add(rph);
                    //await _orderService.UpdateRecurringPayment(recurringPayment);
                }
                else
                {
                    string error = "";
                    for (int i = 0; i < result.Errors.Count; i++)
                    {
                        error += string.Format("Error {0}: {1}", i, result.Errors[i]);
                        if (i != result.Errors.Count - 1)
                            error += ". ";
                    }
                    throw new GrandException(error);
                }
            }
            catch (Exception exc)
            {
                _logger.Error(string.Format("Error while processing recurring order. {0}", exc.Message), exc);
                throw;
            }
        }

        /// <summary>
        /// Cancels a recurring payment
        /// </summary>
        /// <param name="recurringPayment">Recurring payment</param>
        public virtual async Task<IList<string>> CancelRecurringPayment(RecurringPayment recurringPayment)
        {
            if (recurringPayment == null)
                throw new ArgumentNullException("recurringPayment");

            var initialAd = recurringPayment.InitialOrder;
            if (initialAd == null)
                return new List<string> { "Initial order could not be loaded" };


            var request = new CancelRecurringPaymentRequest();
            CancelRecurringPaymentResult result = null;
            try
            {
                request.Order = initialAd;
                result = await _paymentService.CancelRecurringPayment(request);
                if (result.Success)
                {
                    //update recurring payment
                    recurringPayment.IsActive = false;
                    //await _orderService.UpdateRecurringPayment(recurringPayment);

                    //add a note
                    await _orderService.InsertAdNote(new AdNote {
                        Note = "Recurring payment has been cancelled",
                        DisplayToCustomer = false,
                        CreatedOnUtc = DateTime.UtcNow,
                        AdId = initialAd.Id,

                    });

                    //notify a store owner
                    await _workflowMessageService
                        .SendRecurringPaymentCancelledStoreOwnerNotification(recurringPayment,
                        _localizationSettings.DefaultAdminLanguageId);
                }
            }
            catch (Exception exc)
            {
                if (result == null)
                    result = new CancelRecurringPaymentResult();
                result.AddError(string.Format("Error: {0}. Full exception: {1}", exc.Message, exc));
            }


            //process errors
            string error = "";
            for (int i = 0; i < result.Errors.Count; i++)
            {
                error += string.Format("Error {0}: {1}", i, result.Errors[i]);
                if (i != result.Errors.Count - 1)
                    error += ". ";
            }
            if (!String.IsNullOrEmpty(error))
            {
                //add a note
                await _orderService.InsertAdNote(new AdNote {
                    Note = string.Format("Unable to cancel recurring payment. {0}", error),
                    DisplayToCustomer = false,
                    CreatedOnUtc = DateTime.UtcNow,
                    AdId = initialAd.Id,

                });

                //log it
                string logError = string.Format("Error cancelling recurring payment. Ad #{0}. Error: {1}", initialAd.Id, error);
                await _logger.InsertLog(LogLevel.Error, logError, logError);
            }
            return result.Errors;
        }

        /// <summary>
        /// Gets a value indicating whether a customer can cancel recurring payment
        /// </summary>
        /// <param name="customerToValidate">Customer</param>
        /// <param name="recurringPayment">Recurring Payment</param>
        /// <returns>value indicating whether a customer can cancel recurring payment</returns>
        public virtual async Task<bool> CanCancelRecurringPayment(Customer customerToValidate, RecurringPayment recurringPayment)
        {
            if (recurringPayment == null)
                return false;

            if (customerToValidate == null)
                return false;

            var initialAd = recurringPayment.InitialOrder;
            if (initialAd == null)
                return false;

            var customer = await _customerService.GetCustomerById(recurringPayment.InitialOrder.CustomerId);
            if (customer == null)
                return false;

            //if (initialAd.AdStatus == AdStatus.Cancelled)
            //    return false;

            if (!customerToValidate.IsAdmin())
            {
                if (customer.Id != customerToValidate.Id)
                    return false;
            }

            if (!recurringPayment.NextPaymentDate.HasValue)
                return false;

            return true;
        }

        /// <summary>
        /// Cancel a order
        /// </summary>
        /// <param name="order">Ad</param>
        /// <param name="notifyCustomer">Notify Customer</param>
        /// <param name="notifyStoreOwner">Notify StoreOwner</param>
        public virtual Task CancelAd(Ad order, bool notifyCustomer = true, bool notifyStoreOwner = true)
        {
            return _mediator.Send(new CancelAdCommand() { Ad = order, NotifyCustomer = notifyCustomer, NotifyStoreOwner = notifyStoreOwner });
        }

        /// <summary>
        /// Gets a value indicating whether cancel is allowed
        /// </summary>
        /// <param name="order">Ad</param>
        /// <returns>A value indicating whether cancel is allowed</returns>
        public virtual bool CanCancelAd(Ad order)
        {
            if (order == null)
                throw new ArgumentNullException("order");

            if (order.AdStatus == AdStatus.Cancelled)
                return false;

            return true;
        }

        /// <summary>
        /// Gets a value indicating whether order can be marked as authorized
        /// </summary>
        /// <param name="order">Ad</param>
        /// <returns>A value indicating whether order can be marked as authorized</returns>
        public virtual bool CanMarkAdAsAuthorized(Ad order)
        {
            if (order == null)
                throw new ArgumentNullException("order");

            if (order.AdStatus == AdStatus.Cancelled)
                return false;

            if (order.PaymentStatus == PaymentStatus.Pending)
                return true;

            return false;
        }

        /// <summary>
        /// Marks order as authorized
        /// </summary>
        /// <param name="order">Ad</param>
        public virtual async Task MarkAsAuthorized(Ad order)
        {
            if (order == null)
                throw new ArgumentNullException("order");

            order.PaymentStatusId = (int)PaymentStatus.Authorized;
            await _orderService.UpdateAd(order);

            //add a note
            await _orderService.InsertAdNote(new AdNote {
                Note = "Ad has been marked as authorized",
                DisplayToCustomer = false,
                CreatedOnUtc = DateTime.UtcNow,
                AdId = order.Id,
            });

            ////event notification
            //await _mediator.Publish(new AdMarkAsAuthorizedEvent(order));

            ////check order status
            //await _mediator.Send(new CheckAdStatusCommand() { Ad = order });
        }

        /// <summary>
        /// Gets a value indicating whether capture from admin panel is allowed
        /// </summary>
        /// <param name="order">Ad</param>
        /// <returns>A value indicating whether capture from admin panel is allowed</returns>
        public virtual async Task<bool> CanCapture(Ad order)
        {
            if (order == null)
                throw new ArgumentNullException("order");

            if (order.AdStatus == AdStatus.Cancelled)
                return false;

            if (order.PaymentStatus == PaymentStatus.Authorized &&
                await _paymentService.SupportCapture(order.PaymentMethodSystemName))
                return true;

            return false;
        }

        /// <summary>
        /// Capture an order (from admin panel)
        /// </summary>
        /// <param name="order">Ad</param>
        /// <returns>A list of errors; empty list if no errors</returns>
        public virtual async Task<IList<string>> Capture(Ad order)
        {
            if (order == null)
                throw new ArgumentNullException("order");

            if (!await CanCapture(order))
                throw new GrandException("Cannot do capture for order.");

            var request = new CapturePaymentRequest();
            CapturePaymentResult result = null;
            try
            {
                //old info from placing order
                //request.Order = order;
                //result = await _paymentService.Capture(request);

                ////event notification
                //await _mediator.CaptureAdDetailsEvent(result, request);

                if (result.Success)
                {
                    var paidDate = order.PaidDateUtc;
                    if (result.NewPaymentStatus == PaymentStatus.Paid)
                        paidDate = DateTime.UtcNow;

                    order.CaptureTransactionId = result.CaptureTransactionId;
                    order.CaptureTransactionResult = result.CaptureTransactionResult;
                    order.PaymentStatus = result.NewPaymentStatus;
                    order.PaidDateUtc = paidDate;
                    await _orderService.UpdateAd(order);

                    //add a note
                    await _orderService.InsertAdNote(new AdNote {
                        Note = "Ad has been captured",
                        DisplayToCustomer = false,
                        CreatedOnUtc = DateTime.UtcNow,
                        AdId = order.Id,

                    });

                    //await _mediator.Send(new CheckAdStatusCommand() { Ad = order });
                    //if (order.PaymentStatus == PaymentStatus.Paid)
                    //{
                    //    await ProcessAdPaid(order);
                    //}
                }
            }
            catch (Exception exc)
            {
                if (result == null)
                    result = new CapturePaymentResult();
                result.AddError(string.Format("Error: {0}. Full exception: {1}", exc.Message, exc));
            }


            //process errors
            string error = "";
            for (int i = 0; i < result.Errors.Count; i++)
            {
                error += string.Format("Error {0}: {1}", i, result.Errors[i]);
                if (i != result.Errors.Count - 1)
                    error += ". ";
            }
            if (!String.IsNullOrEmpty(error))
            {
                //add a note
                await _orderService.InsertAdNote(new AdNote {
                    Note = string.Format("Unable to capture order. {0}", error),
                    DisplayToCustomer = false,
                    CreatedOnUtc = DateTime.UtcNow,
                    AdId = order.Id,
                });

                //log it
                string logError = string.Format("Error capturing order #{0}. Error: {1}", order.Id, error);
                await _logger.InsertLog(LogLevel.Error, logError, logError);
            }
            return result.Errors;
        }

        /// <summary>
        /// Gets a value indicating whether order can be marked as paid
        /// </summary>
        /// <param name="order">Ad</param>
        /// <returns>A value indicating whether order can be marked as paid</returns>
        public virtual async Task<bool> CanMarkAdAsPaid(Ad order)
        {
            if (order == null)
                throw new ArgumentNullException("order");

            if (order.AdStatus == AdStatus.Cancelled)
                return false;

            if (order.PaymentStatus == PaymentStatus.Paid ||
                order.PaymentStatus == PaymentStatus.Refunded ||
                order.PaymentStatus == PaymentStatus.Voided)
                return false;

            return await Task.FromResult(true);
        }

        /// <summary>
        /// Marks order as paid
        /// </summary>
        /// <param name="order">Ad</param>
        public virtual async Task MarkAdAsPaid(Ad order)
        {
            if (order == null)
                throw new ArgumentNullException("order");

            if (!await CanMarkAdAsPaid(order))
                throw new GrandException("You can't mark this order as paid");

            order.PaymentStatusId = (int)PaymentStatus.Paid;
            order.PaidDateUtc = DateTime.UtcNow;
            await _orderService.UpdateAd(order);

            //add a note
            await _orderService.InsertAdNote(new AdNote {
                Note = "Ad has been marked as paid",
                DisplayToCustomer = false,
                CreatedOnUtc = DateTime.UtcNow,
                AdId = order.Id,

            });

            //await _mediator.Send(new CheckAdStatusCommand() { Ad = order });
            if (order.PaymentStatus == PaymentStatus.Paid)
            {
                await ProcessAdPaid(order);
            }
        }

        /// <summary>
        /// Gets a value indicating whether refund from admin panel is allowed
        /// </summary>
        /// <param name="order">Ad</param>
        /// <returns>A value indicating whether refund from admin panel is allowed</returns>
        public virtual async Task<bool> CanRefund(Ad order)
        {
            if (order == null)
                throw new ArgumentNullException("order");

            //if (order.AdTotal == decimal.Zero)
            //    return false;

            //refund cannot be made if previously a partial refund has been already done. only other partial refund can be made in this case
            if (order.RefundedAmount > decimal.Zero)
                return false;

            //uncomment the lines below in order to allow this operation for cancelled orders
            //if (order.AdStatus == AdStatus.Cancelled)
            //    return false;

            if (order.PaymentStatus == PaymentStatus.Paid &&
                await _paymentService.SupportRefund(order.PaymentMethodSystemName))
                return true;

            return false;
        }

        /// <summary>
        /// Refunds an order (from admin panel)
        /// </summary>
        /// <param name="order">Ad</param>
        /// <returns>A list of errors; empty list if no errors</returns>
        public virtual async Task<IList<string>> Refund(Ad order)
        {
            if (order == null)
                throw new ArgumentNullException("order");

            if (!await CanRefund(order))
                throw new GrandException("Cannot do refund for order.");

            var request = new RefundPaymentRequest();
            RefundPaymentResult result = null;
            try
            {
                //request.Ad = order;
                //request.AmountToRefund = order.AdTo;
                request.IsPartialRefund = false;
                result = await _paymentService.Refund(request);
                if (result.Success)
                {
                    //total amount refunded
                    decimal totalAmountRefunded = order.RefundedAmount + request.AmountToRefund;

                    //update order info
                    order.RefundedAmount = totalAmountRefunded;
                    order.PaymentStatus = result.NewPaymentStatus;
                    await _orderService.UpdateAd(order);

                    //add a note
                    await _orderService.InsertAdNote(new AdNote {
                        Note = string.Format("Ad has been refunded. Amount = {0}", request.AmountToRefund),
                        DisplayToCustomer = false,
                        CreatedOnUtc = DateTime.UtcNow,
                        AdId = order.Id,
                    });

                    //check order status
                    //await _mediator.Send(new CheckAdStatusCommand() { Ad = order });

                    //notifications
                    //var orderRefundedStoreOwnerNotificationQueuedEmailId = await _workflowMessageService.SendAdRefundedStoreOwnerNotification(order, request.AmountToRefund, _localizationSettings.DefaultAdminLanguageId);
                    //if (orderRefundedStoreOwnerNotificationQueuedEmailId > 0)
                    //{
                    //    await _orderService.InsertAdNote(new AdNote {
                    //        Note = "\"Ad refunded\" email (to store owner) has been queued.",
                    //        DisplayToCustomer = false,
                    //        CreatedOnUtc = DateTime.UtcNow,
                    //        AdId = order.Id,
                    //    });
                    //}

                    //notifications
                    //var orderRefundedCustomerNotificationQueuedEmailId = await _workflowMessageService.SendAdRefundedCustomerNotification(order, request.AmountToRefund, order.CustomerLanguageId);
                    //if (orderRefundedCustomerNotificationQueuedEmailId > 0)
                    //{
                    //    await _orderService.InsertAdNote(new AdNote {
                    //        Note = "\"Ad refunded\" email (to customer) has been queued.",
                    //        DisplayToCustomer = false,
                    //        CreatedOnUtc = DateTime.UtcNow,
                    //        AdId = order.Id,
                    //    });
                    //}

                    ////raise event       
                    //await _mediator.Publish(new AdRefundedEvent(order, request.AmountToRefund));
                }

            }
            catch (Exception exc)
            {
                if (result == null)
                    result = new RefundPaymentResult();
                result.AddError(string.Format("Error: {0}. Full exception: {1}", exc.Message, exc.ToString()));
            }

            //process errors
            string error = "";
            for (int i = 0; i < result.Errors.Count; i++)
            {
                error += string.Format("Error {0}: {1}", i, result.Errors[i]);
                if (i != result.Errors.Count - 1)
                    error += ". ";
            }
            if (!String.IsNullOrEmpty(error))
            {
                //add a note
                await _orderService.InsertAdNote(new AdNote {
                    Note = string.Format("Unable to refund order. {0}", error),
                    DisplayToCustomer = false,
                    CreatedOnUtc = DateTime.UtcNow,
                    AdId = order.Id,
                });

                //log it
                string logError = string.Format("Error refunding order #{0}. Error: {1}", order.Id, error);
                await _logger.InsertLog(LogLevel.Error, logError, logError);
            }
            return result.Errors;
        }

        /// <summary>
        /// Gets a value indicating whether order can be marked as refunded
        /// </summary>
        /// <param name="order">Ad</param>
        /// <returns>A value indicating whether order can be marked as refunded</returns>
        public virtual bool CanRefundOffline(Ad order)
        {
            if (order == null)
                throw new ArgumentNullException("order");

            //if (order.AdTotal == decimal.Zero)
            //    return false;

            //refund cannot be made if previously a partial refund has been already done. only other partial refund can be made in this case
            if (order.RefundedAmount > decimal.Zero)
                return false;


            //uncomment the lines below in order to allow this operation for cancelled orders
            //if (order.AdStatus == AdStatus.Cancelled)
            //     return false;

            if (order.PaymentStatus == PaymentStatus.Paid)
                return true;

            return false;
        }

        /// <summary>
        /// Refunds an order (offline)
        /// </summary>
        /// <param name="order">Ad</param>
        public virtual async Task RefundOffline(Ad order)
        {
            if (order == null)
                throw new ArgumentNullException("order");

            if (!CanRefundOffline(order))
                throw new GrandException("You can't refund this order");

            //amout to refund
            decimal amountToRefund = order.AdTotal;

            //total amount refunded
            decimal totalAmountRefunded = order.RefundedAmount + amountToRefund;

            //update order info
            order.RefundedAmount = totalAmountRefunded;
            order.PaymentStatus = PaymentStatus.Refunded;
            await _orderService.UpdateAd(order);

            //add a note
            await _orderService.InsertAdNote(new AdNote {
                Note = string.Format("Ad has been marked as refunded. Amount = {0}", amountToRefund),
                DisplayToCustomer = false,
                CreatedOnUtc = DateTime.UtcNow,
                AdId = order.Id,
            });

            //check order status
            //await _mediator.Send(new CheckAdStatusCommand() { Ad = order });

            //notifications
            //var orderRefundedStoreOwnerNotificationQueuedEmailId = await _workflowMessageService.SendAdRefundedStoreOwnerNotification(order, amountToRefund, _localizationSettings.DefaultAdminLanguageId);
            //if (orderRefundedStoreOwnerNotificationQueuedEmailId > 0)
            //{
            //    await _orderService.InsertAdNote(new AdNote {
            //        Note = "\"Ad refunded\" email (to store owner) has been queued.",
            //        DisplayToCustomer = false,
            //        CreatedOnUtc = DateTime.UtcNow,
            //        AdId = order.Id,
            //    });

            //}


            //var orderRefundedCustomerNotificationQueuedEmailId = await _workflowMessageService.SendAdRefundedCustomerNotification(order, amountToRefund, order.CustomerLanguageId);
            //if (orderRefundedCustomerNotificationQueuedEmailId > 0)
            //{
            //    await _orderService.InsertAdNote(new AdNote {
            //        Note = "\"Ad refunded\" email (to customer) has been queued.",
            //        DisplayToCustomer = false,
            //        CreatedOnUtc = DateTime.UtcNow,
            //        AdId = order.Id,
            //    });
            //}

            //raise event       
            //await _mediator.Publish(new AdRefundedEvent(order, amountToRefund));
        }

        /// <summary>
        /// Gets a value indicating whether partial refund from admin panel is allowed
        /// </summary>
        /// <param name="order">Ad</param>
        /// <param name="amountToRefund">Amount to refund</param>
        /// <returns>A value indicating whether refund from admin panel is allowed</returns>
        public virtual async Task<bool> CanPartiallyRefund(Ad order, decimal amountToRefund)
        {
            if (order == null)
                throw new ArgumentNullException("order");

            if (order.AdTotal == decimal.Zero)
                return false;

            //uncomment the lines below in order to allow this operation for cancelled orders
            //if (order.AdStatus == AdStatus.Cancelled)
            //    return false;

            decimal canBeRefunded = order.AdTotal - order.RefundedAmount;
            if (canBeRefunded <= decimal.Zero)
                return false;

            if (amountToRefund > canBeRefunded)
                return false;

            if ((order.PaymentStatus == PaymentStatus.Paid ||
                order.PaymentStatus == PaymentStatus.PartiallyRefunded) &&
                await _paymentService.SupportPartiallyRefund(order.PaymentMethodSystemName))
                return true;

            return false;
        }

        /// <summary>
        /// Partially refunds an order (from admin panel)
        /// </summary>
        /// <param name="order">Ad</param>
        /// <param name="amountToRefund">Amount to refund</param>
        /// <returns>A list of errors; empty list if no errors</returns>
        public virtual async Task<IList<string>> PartiallyRefund(Ad order, decimal amountToRefund)
        {
            if (order == null)
                throw new ArgumentNullException("order");

            if (!await CanPartiallyRefund(order, amountToRefund))
                throw new GrandException("Cannot do partial refund for order.");

            var request = new RefundPaymentRequest();
            RefundPaymentResult result = null;
            try
            {
                request.Ad = order;
                request.AmountToRefund = amountToRefund;
                request.IsPartialRefund = true;

                result = await _paymentService.Refund(request);

                if (result.Success)
                {
                    //total amount refunded
                    decimal totalAmountRefunded = order.RefundedAmount + amountToRefund;

                    //update order info
                    order.RefundedAmount = totalAmountRefunded;
                    order.PaymentStatus = result.NewPaymentStatus;
                    await _orderService.UpdateAd(order);

                    //add a note
                    await _orderService.InsertAdNote(new AdNote {
                        Note = string.Format("Ad has been partially refunded. Amount = {0}", amountToRefund),
                        DisplayToCustomer = false,
                        CreatedOnUtc = DateTime.UtcNow,
                        AdId = order.Id,
                    });

                    ////check order status
                    //await _mediator.Send(new CheckAdStatusCommand() { Ad = order });

                    ////notifications
                    //var orderRefundedStoreOwnerNotificationQueuedEmailId = await _workflowMessageService.SendAdRefundedStoreOwnerNotification(order, amountToRefund, _localizationSettings.DefaultAdminLanguageId);
                    //if (orderRefundedStoreOwnerNotificationQueuedEmailId > 0)
                    //{
                    //    await _orderService.InsertAdNote(new AdNote {
                    //        Note = "\"Ad refunded\" email (to store owner) has been queued.",
                    //        DisplayToCustomer = false,
                    //        CreatedOnUtc = DateTime.UtcNow,
                    //        AdId = order.Id,
                    //    });
                    //}


                    //var orderRefundedCustomerNotificationQueuedEmailId = await _workflowMessageService.SendAdRefundedCustomerNotification(order, amountToRefund, order.CustomerLanguageId);
                    //if (orderRefundedCustomerNotificationQueuedEmailId > 0)
                    //{
                    //    await _orderService.InsertAdNote(new AdNote {
                    //        Note = "\"Ad refunded\" email (to customer) has been queued.",
                    //        DisplayToCustomer = false,
                    //        CreatedOnUtc = DateTime.UtcNow,
                    //        AdId = order.Id,
                    //    });
                    //}

                    ////raise event       
                    //await _mediator.Publish(new AdRefundedEvent(order, amountToRefund));
                }
            }
            catch (Exception exc)
            {
                if (result == null)
                    result = new RefundPaymentResult();
                result.AddError(string.Format("Error: {0}. Full exception: {1}", exc.Message, exc.ToString()));
            }

            //process errors
            string error = "";
            for (int i = 0; i < result.Errors.Count; i++)
            {
                error += string.Format("Error {0}: {1}", i, result.Errors[i]);
                if (i != result.Errors.Count - 1)
                    error += ". ";
            }
            if (!String.IsNullOrEmpty(error))
            {
                //add a note
                await _orderService.InsertAdNote(new AdNote {
                    Note = string.Format("Unable to partially refund order. {0}", error),
                    DisplayToCustomer = false,
                    CreatedOnUtc = DateTime.UtcNow,
                    AdId = order.Id,
                });

                //log it
                string logError = string.Format("Error refunding order #{0}. Error: {1}", order.Id, error);
                await _logger.InsertLog(LogLevel.Error, logError, logError);
            }
            return result.Errors;
        }

        /// <summary>
        /// Gets a value indicating whether order can be marked as partially refunded
        /// </summary>
        /// <param name="order">Ad</param>
        /// <param name="amountToRefund">Amount to refund</param>
        /// <returns>A value indicating whether order can be marked as partially refunded</returns>
        public virtual bool CanPartiallyRefundOffline(Ad order, decimal amountToRefund)
        {
            if (order == null)
                throw new ArgumentNullException("order");

            if (order.AdTotal == decimal.Zero)
                return false;

            //uncomment the lines below in order to allow this operation for cancelled orders
            //if (order.AdStatus == AdStatus.Cancelled)
            //    return false;

            decimal canBeRefunded = order.AdTotal - order.RefundedAmount;
            if (canBeRefunded <= decimal.Zero)
                return false;

            if (amountToRefund > canBeRefunded)
                return false;

            if (order.PaymentStatus == PaymentStatus.Paid ||
                order.PaymentStatus == PaymentStatus.PartiallyRefunded)
                return true;

            return false;
        }

        /// <summary>
        /// Partially refunds an order (offline)
        /// </summary>
        /// <param name="order">Ad</param>
        /// <param name="amountToRefund">Amount to refund</param>
        public virtual async Task PartiallyRefundOffline(Ad order, decimal amountToRefund)
        {
            if (order == null)
                throw new ArgumentNullException("order");

            if (!CanPartiallyRefundOffline(order, amountToRefund))
                throw new GrandException("You can't partially refund (offline) this order");

            //total amount refunded
            decimal totalAmountRefunded = order.RefundedAmount + amountToRefund;

            //update order info
            order.RefundedAmount = totalAmountRefunded;
            order.PaymentStatus = order.RefundedAmount >= order.AdTotal ? PaymentStatus.Refunded : PaymentStatus.PartiallyRefunded;
            await _orderService.UpdateAd(order);

            //add a note
            await _orderService.InsertAdNote(new AdNote {
                Note = string.Format("Ad has been marked as partially refunded. Amount = {0}", amountToRefund),
                DisplayToCustomer = false,
                CreatedOnUtc = DateTime.UtcNow,
                AdId = order.Id,
            });

            //check order status
            //await _mediator.Send(new CheckAdStatusCommand() { Ad = order });

            ////notifications
            //var orderRefundedStoreOwnerNotificationQueuedEmailId = await _workflowMessageService.SendAdRefundedStoreOwnerNotification(order, amountToRefund, _localizationSettings.DefaultAdminLanguageId);
            //if (orderRefundedStoreOwnerNotificationQueuedEmailId > 0)
            //{
            //    await _orderService.InsertAdNote(new AdNote {
            //        Note = "\"Ad refunded\" email (to store owner) has been queued.",
            //        DisplayToCustomer = false,
            //        CreatedOnUtc = DateTime.UtcNow,
            //        AdId = order.Id,
            //    });
            //}

            //var orderRefundedCustomerNotificationQueuedEmailId = await _workflowMessageService.SendAdRefundedCustomerNotification(order, amountToRefund, order.CustomerLanguageId);
            //if (orderRefundedCustomerNotificationQueuedEmailId > 0)
            //{
            //    await _orderService.InsertAdNote(new AdNote {
            //        Note = "\"Ad refunded\" email (to customer) has been queued.",
            //        DisplayToCustomer = false,
            //        CreatedOnUtc = DateTime.UtcNow,
            //        AdId = order.Id,
            //    });
            //}
            //raise event       
            //await _mediator.Publish(new AdRefundedEvent(order, amountToRefund));
        }

        /// <summary>
        /// Gets a value indicating whether void from admin panel is allowed
        /// </summary>
        /// <param name="order">Ad</param>
        /// <returns>A value indicating whether void from admin panel is allowed</returns>
        public virtual async Task<bool> CanVoid(Ad order)
        {
            if (order == null)
                throw new ArgumentNullException("order");

            if (order.AdTotal == decimal.Zero)
                return false;

            if (order.PaymentStatus == PaymentStatus.Authorized &&
                await _paymentService.SupportVoid(order.PaymentMethodSystemName))
                return true;

            return false;
        }

        /// <summary>
        /// Voids order (from admin panel)
        /// </summary>
        /// <param name="order">Ad</param>
        /// <returns>Voided order</returns>
        public virtual async Task<IList<string>> Void(Ad order)
        {
            if (order == null)
                throw new ArgumentNullException("order");

            if (!await CanVoid(order))
                throw new GrandException("Cannot do void for order.");

            var request = new VoidPaymentRequest();
            VoidPaymentResult result = null;
            try
            {
                request.Ad = order;
                result = await _paymentService.Void(request);

                //event notification
                //await _mediator.VoidAdDetailsEvent(result, request);

                if (result.Success)
                {
                    //update order info
                    order.PaymentStatus = result.NewPaymentStatus;
                    await _orderService.UpdateAd(order);

                    //add a note
                    await _orderService.InsertAdNote(new AdNote {
                        Note = "Ad has been voided",
                        DisplayToCustomer = false,
                        CreatedOnUtc = DateTime.UtcNow,
                        AdId = order.Id,
                    });

                    //check order status
                    //await _mediator.Send(new CheckAdStatusCommand() { Ad = order });
                }
            }
            catch (Exception exc)
            {
                if (result == null)
                    result = new VoidPaymentResult();
                result.AddError(string.Format("Error: {0}. Full exception: {1}", exc.Message, exc.ToString()));
            }

            //process errors
            string error = "";
            for (int i = 0; i < result.Errors.Count; i++)
            {
                error += string.Format("Error {0}: {1}", i, result.Errors[i]);
                if (i != result.Errors.Count - 1)
                    error += ". ";
            }
            if (!String.IsNullOrEmpty(error))
            {
                //add a note
                await _orderService.InsertAdNote(new AdNote {
                    Note = string.Format("Unable to voiding order. {0}", error),
                    DisplayToCustomer = false,
                    CreatedOnUtc = DateTime.UtcNow,
                    AdId = order.Id,
                });

                //log it
                string logError = string.Format("Error voiding order #{0}. Error: {1}", order.Id, error);
                await _logger.InsertLog(LogLevel.Error, logError, logError);
            }
            return result.Errors;
        }

        /// <summary>
        /// Gets a value indicating whether order can be marked as voided
        /// </summary>
        /// <param name="order">Ad</param>
        /// <returns>A value indicating whether order can be marked as voided</returns>
        public virtual bool CanVoidOffline(Ad order)
        {
            if (order == null)
                throw new ArgumentNullException("order");

            if (order.AdTotal == decimal.Zero)
                return false;

            if (order.PaymentStatus == PaymentStatus.Authorized)
                return true;

            return false;
        }

        /// <summary>
        /// Voids order (offline)
        /// </summary>
        /// <param name="order">Ad</param>
        public virtual async Task VoidOffline(Ad order)
        {
            if (order == null)
                throw new ArgumentNullException("order");

            if (!CanVoidOffline(order))
                throw new GrandException("You can't void this order");

            order.PaymentStatusId = (int)PaymentStatus.Voided;
            await _orderService.UpdateAd(order);

            //add a note
            await _orderService.InsertAdNote(new AdNote {
                Note = "Ad has been marked as voided",
                DisplayToCustomer = false,
                CreatedOnUtc = DateTime.UtcNow,
                AdId = order.Id,
            });

            ////event notification
            //await _mediator.Publish(new AdVoidOfflineEvent(order));

            ////check orer status
            //await _mediator.Send(new CheckAdStatusCommand() { Ad = order });
        }

        /// <summary>
        /// Valdiate minimum order sub-total amount
        /// </summary>
        /// <param name="cart">Shopping cart</param>
        /// <returns>true - OK; false - minimum order sub-total amount is not reached</returns>
        public virtual async Task<bool> ValidateMinAdSubtotalAmount(IList<ShoppingCartItem> cart)
        {
            if (cart == null)
                throw new ArgumentNullException("cart");

            if (!cart.Any())
                return false;

            //min order amount sub-total validation
            //if (cart.Any() && _orderSettings.MinAdSubtotalAmount > decimal.Zero)
            //{
            //    //subtotal
            //    var (_, _, subTotalWithoutDiscount, _, _) = await _orderTotalCalculationService.GetShoppingCartSubTotal(cart, false);
            //    if (subTotalWithoutDiscount < _orderSettings.MinAdSubtotalAmount)
            //        return false;
            //}

            return true;
        }

        /// <summary>
        /// Valdiate minimum order total amount
        /// </summary>
        /// <param name="cart">Shopping cart</param>
        /// <returns>true - OK; false - minimum order total amount is not reached</returns>
        public virtual async Task<bool> ValidateMinAdTotalAmount(IList<ShoppingCartItem> cart)
        {
            if (cart == null)
                throw new ArgumentNullException("cart");

            //if (cart.Any() && _orderSettings.MinAdTotalAmount > decimal.Zero)
            //{
            //    decimal? shoppingCartTotalBase = (await _orderTotalCalculationService.GetShoppingCartTotal(cart)).shoppingCartTotal;
            //    if (shoppingCartTotalBase.HasValue && shoppingCartTotalBase.Value < _orderSettings.MinAdTotalAmount)
            //        return false;
            //}

            return true;
        }

        /// <summary>
        /// Validate order total amount
        /// </summary>
        /// <param name="customer">Customer</param>
        /// <param name="cart">Shopping cart</param>
        /// <returns>true - OK; false - minimum/maximum order total amount is not reached</returns>
        public virtual async Task<bool> ValidateAdTotalAmount(Customer customer, IList<ShoppingCartItem> cart)
        {
            if (cart == null)
                throw new ArgumentNullException("cart");

            //var minroles = customer.CustomerRoles.AdBy(x => x.MinAdAmount).FirstOrDefault(x => x.Active && x.MinAdAmount.HasValue);
            //var minAdAmount = minroles?.MinAdAmount ?? decimal.MinValue;

            //var maxroles = customer.CustomerRoles.AdByDescending(x => x.MaxAdAmount).FirstOrDefault(x => x.Active && x.MaxAdAmount.HasValue);
            //var maxAdAmount = maxroles?.MaxAdAmount ?? decimal.MaxValue;

            //if (cart.Any() && (minAdAmount > decimal.Zero || maxAdAmount > decimal.Zero))
            //{
            //    decimal? shoppingCartTotalBase = (await _orderTotalCalculationService.GetShoppingCartTotal(cart)).shoppingCartTotal;
            //    if (shoppingCartTotalBase.HasValue && (shoppingCartTotalBase.Value < minAdAmount || shoppingCartTotalBase.Value > maxAdAmount))
            //        return false;
            //}

            return true;
        }
        #endregion
    }
}
