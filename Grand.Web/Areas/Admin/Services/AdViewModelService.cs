using Grand.Core;
using Grand.Domain.Catalog;
using Grand.Domain.Common;
using Grand.Domain.Customers;
using Grand.Domain.Directory;
using Grand.Domain.Ads;
using Grand.Domain.Payments;
using Grand.Domain.Shipping;
using Grand.Domain.Tax;
using Grand.Framework.Extensions;
using Grand.Services.Affiliates;
using Grand.Services.Catalog;
using Grand.Services.Common;
using Grand.Services.Customers;
using Grand.Services.Directory;
using Grand.Services.Discounts;
using Grand.Services.Helpers;
using Grand.Services.Localization;
using Grand.Services.Logging;
using Grand.Services.Media;
using Grand.Services.Messages;
using Grand.Services.Ads;
using Grand.Services.Payments;
using Grand.Services.Security;
using Grand.Services.Shipping;
using Grand.Services.Stores;
using Grand.Services.Tax;
using Grand.Services.Vendors;
using Grand.Web.Areas.Admin.Extensions;
using Grand.Web.Areas.Admin.Interfaces;
using Grand.Web.Areas.Admin.Models.Ads;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Grand.Web.Areas.Admin.Services
{
    public partial class AdViewModelService : IAdViewModelService
    {
        #region Fields

        private readonly IAdService _orderService;
        private readonly IAdReportService _orderReportService;
        private readonly IAdProcessingService _orderProcessingService;
        private readonly IPriceCalculationService _priceCalculationService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IPriceFormatter _priceFormatter;
        private readonly IDiscountService _discountService;
        private readonly ILocalizationService _localizationService;
        private readonly IWorkContext _workContext;
        private readonly ICurrencyService _currencyService;
        private readonly IEncryptionService _encryptionService;
        private readonly IPaymentService _paymentService;
        private readonly ICountryService _countryService;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly IProductService _productService;
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly ICategoryService _categoryService;
        private readonly IManufacturerService _manufacturerService;
        private readonly IProductAttributeService _productAttributeService;
        private readonly IProductAttributeParser _productAttributeParser;
        private readonly IDownloadService _downloadService;
        private readonly IStoreService _storeService;
        private readonly IVendorService _vendorService;
        private readonly IShippingService _shippingService;
        private readonly IAddressAttributeParser _addressAttributeParser;
        private readonly IAddressAttributeService _addressAttributeService;
        private readonly IAddressAttributeFormatter _addressAttributeFormatter;
        private readonly IAffiliateService _affiliateService;
        private readonly IPictureService _pictureService;
        private readonly ITaxService _taxService;
        private readonly IReturnRequestServiceAd _returnRequestService;
        private readonly ICustomerService _customerService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly IShipmentService _shipmentService;
        private readonly IServiceProvider _serviceProvider;
        private readonly CurrencySettings _currencySettings;
        private readonly TaxSettings _taxSettings;
        private readonly AddressSettings _addressSettings;
        private readonly IAdTagService _orderTagService;
        #endregion

        #region Ctor

        public AdViewModelService(IAdService orderService,
            IAdReportService orderReportService,
            IAdProcessingService orderProcessingService,
            IPriceCalculationService priceCalculationService,
            IDateTimeHelper dateTimeHelper,
            IPriceFormatter priceFormatter,
            IDiscountService discountService,
            ILocalizationService localizationService,
            IWorkContext workContext,
            ICurrencyService currencyService,
            IEncryptionService encryptionService,
            IPaymentService paymentService,
            ICountryService countryService,
            IStateProvinceService stateProvinceService,
            IProductService productService,
            IWorkflowMessageService workflowMessageService,
            ICategoryService categoryService,
            IManufacturerService manufacturerService,
            IProductAttributeService productAttributeService,
            IProductAttributeParser productAttributeParser,
            IDownloadService downloadService,
            IShippingService shippingService,
            IStoreService storeService,
            IVendorService vendorService,
            IAddressAttributeParser addressAttributeParser,
            IAddressAttributeService addressAttributeService,
            IAddressAttributeFormatter addressAttributeFormatter,
            IAffiliateService affiliateService,
            IPictureService pictureService,
            ITaxService taxService,
            IReturnRequestServiceAd returnRequestService,
            ICustomerService customerService,
            ICustomerActivityService customerActivityService,
            IShipmentService shipmentService,
            IServiceProvider serviceProvider,
            CurrencySettings currencySettings,
            TaxSettings taxSettings,
            AddressSettings addressSettings
            ,IAdTagService orderTagService
            )
        {
            _orderService = orderService;
            _orderReportService = orderReportService;
            _orderProcessingService = orderProcessingService;
            _priceCalculationService = priceCalculationService;
            _dateTimeHelper = dateTimeHelper;
            _priceFormatter = priceFormatter;
            _discountService = discountService;
            _localizationService = localizationService;
            _workContext = workContext;
            _currencyService = currencyService;
            _encryptionService = encryptionService;
            _paymentService = paymentService;
            _countryService = countryService;
            _stateProvinceService = stateProvinceService;
            _productService = productService;
            _workflowMessageService = workflowMessageService;
            _categoryService = categoryService;
            _manufacturerService = manufacturerService;
            _productAttributeService = productAttributeService;
            _productAttributeParser = productAttributeParser;
            _downloadService = downloadService;
            _shippingService = shippingService;
            _storeService = storeService;
            _vendorService = vendorService;
            _addressAttributeParser = addressAttributeParser;
            _addressAttributeService = addressAttributeService;
            _addressAttributeFormatter = addressAttributeFormatter;
            _affiliateService = affiliateService;
            _pictureService = pictureService;
            _taxService = taxService;
            _returnRequestService = returnRequestService;
            _customerActivityService = customerActivityService;
            _shipmentService = shipmentService;
            _serviceProvider = serviceProvider;
            _currencySettings = currencySettings;
            _taxSettings = taxSettings;
            _addressSettings = addressSettings;
            _customerService = customerService;
            _orderTagService = orderTagService;
        }

        #endregion

        private IList<string> ParseAdTagsToList(string orderTags)
        {
            var result = new List<string>();
            if (!string.IsNullOrWhiteSpace(orderTags))
            {
                var values = orderTags.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var val1 in values)
                {
                    if (!string.IsNullOrEmpty(val1.Trim()))
                        result.Add(val1.Trim());
                }
            }
            return result;
        }


        public virtual async Task<AdListModel> PrepareAdListModel(
            int? orderStatusId = null,
            int? paymentStatusId = null,
            int? shippingStatusId = null,
            DateTime? startDate = null,
            string storeId = null,
            string code = null)
        {
            //order statuses
            var model = new AdListModel {
                AvailableAdStatuses = AdStatus.Active.ToSelectList(_localizationService, _workContext, false).ToList()
            };
            model.AvailableAdStatuses.Insert(0, new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = " " });
            if (orderStatusId.HasValue)
            {
                //pre-select value?
                var item = model.AvailableAdStatuses.FirstOrDefault(x => x.Value == orderStatusId.Value.ToString());
                if (item != null)
                    item.Selected = true;
            }

            //payment statuses
            model.AvailablePaymentStatuses = PaymentStatus.Pending.ToSelectList(_localizationService, _workContext, false).ToList();
            model.AvailablePaymentStatuses.Insert(0, new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = " " });
            if (paymentStatusId.HasValue)
            {
                //pre-select value?
                var item = model.AvailablePaymentStatuses.FirstOrDefault(x => x.Value == paymentStatusId.Value.ToString());
                if (item != null)
                    item.Selected = true;
            }

            //order's tags
            //model.AvailableAdTags.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = " " });
            //foreach (var s in (await _orderTagService.GetAllAdTags()))
            //{
            //    model.AvailableAdTags.Add(new SelectListItem { Text = s.Name, Value = s.Id });
            //}

            //shipping statuses
            model.AvailableShippingStatuses = ShippingStatus.NotYetShipped.ToSelectList(_localizationService, _workContext, false).ToList();
            model.AvailableShippingStatuses.Insert(0, new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = " " });
            if (shippingStatusId.HasValue)
            {
                //pre-select value?
                var item = model.AvailableShippingStatuses.FirstOrDefault(x => x.Value == shippingStatusId.Value.ToString());
                if (item != null)
                    item.Selected = true;
            }

            //stores
            model.AvailableStores.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = " " });
            foreach (var s in (await _storeService.GetAllStores()).Where(x => x.Id == storeId || string.IsNullOrWhiteSpace(storeId)))
                model.AvailableStores.Add(new SelectListItem { Text = s.Shortcut, Value = s.Id.ToString() });

            //vendors
            model.AvailableVendors.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = " " });
            foreach (var v in await _vendorService.GetAllVendors(showHidden: true))
                model.AvailableVendors.Add(new SelectListItem { Text = v.Name, Value = v.Id.ToString() });

            //warehouses
            model.AvailableWarehouses.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = " " });
            foreach (var w in await _shippingService.GetAllWarehouses())
                model.AvailableWarehouses.Add(new SelectListItem { Text = w.Name, Value = w.Id.ToString() });

            //payment methods
            model.AvailablePaymentMethods.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = " " });
            foreach (var pm in _paymentService.LoadAllPaymentMethods())
                model.AvailablePaymentMethods.Add(new SelectListItem { Text = pm.PluginDescriptor.FriendlyName, Value = pm.PluginDescriptor.SystemName });

            //billing countries
            foreach (var c in await _countryService.GetAllCountriesForBilling(showHidden: true))
            {
                model.AvailableCountries.Add(new SelectListItem { Text = c.Name, Value = c.Id.ToString() });
            }
            model.AvailableCountries.Insert(0, new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = " " });

            //a vendor should have access only to orders with his products
            model.IsLoggedInAsVendor = _workContext.CurrentVendor != null && !_workContext.CurrentCustomer.IsStaff();
            if (startDate.HasValue)
                model.StartDate = startDate.Value;

            if (!string.IsNullOrEmpty(code))
                model.GoDirectlyToNumber = code;

            return model;
        }
        public virtual async Task<(IEnumerable<AdModel> orderModels, AdAggreratorModel aggreratorModel, int totalCount)> PrepareAdModel(AdListModel model, int pageIndex, int pageSize)
        {
            DateTime? startDateValue = (model.StartDate == null) ? null
                            : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.StartDate.Value, _dateTimeHelper.CurrentTimeZone);

            DateTime? endDateValue = (model.EndDate == null) ? null
                            : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.EndDate.Value, _dateTimeHelper.CurrentTimeZone).AddDays(1);

            AdStatus? orderStatus = model.AdStatusId > 0 ? (AdStatus?)(model.AdStatusId) : null;
            PaymentStatus? paymentStatus = model.PaymentStatusId > 0 ? (PaymentStatus?)(model.PaymentStatusId) : null;
            ShippingStatus? shippingStatus = model.ShippingStatusId > 0 ? (ShippingStatus?)(model.ShippingStatusId) : null;


            var filterByProductId = "";
            var product = await _productService.GetProductById(model.ProductId);
            if (product != null && _workContext.HasAccessToProduct(product))
                filterByProductId = model.ProductId;

            //load orders
            var orders = await _orderService.SearchAds(storeId: model.StoreId,
                vendorId: model.VendorId,
                productId: filterByProductId,
                warehouseId: model.WarehouseId,
                paymentMethodSystemName: model.PaymentMethodSystemName,
                createdFromUtc: startDateValue,
                createdToUtc: endDateValue,
                os: orderStatus,
                ps: paymentStatus,
                ss: shippingStatus,
                billingEmail: model.BillingEmail,
                billingLastName: model.BillingLastName,
                billingCountryId: model.BillingCountryId,
                adGuid: model.AdGuid,
                adCode: model.GoDirectlyToNumber,
                pageIndex: pageIndex - 1,
                pageSize: pageSize,
                adTagId: model.AdTag);

            //summary report
            //currently we do not support productId and warehouseId parameters for this report
            var reportSummary = await _orderReportService.GetAdAverageReportLine(
                storeId: model.StoreId,
                vendorId: model.VendorId,
                orderId: "",
                paymentMethodSystemName: model.PaymentMethodSystemName,
                os: orderStatus,
                ps: paymentStatus,
                ss: shippingStatus,
                startTimeUtc: startDateValue,
                endTimeUtc: endDateValue,
                billingEmail: model.BillingEmail,
                billingLastName: model.BillingLastName,
                billingCountryId: model.BillingCountryId,
                tagid: model.AdTag
                );
            var profit = await _orderReportService.ProfitReport(
                storeId: model.StoreId,
                vendorId: model.VendorId,
                paymentMethodSystemName: model.PaymentMethodSystemName,
                os: orderStatus,
                ps: paymentStatus,
                ss: shippingStatus,
                startTimeUtc: startDateValue,
                endTimeUtc: endDateValue,
                billingEmail: model.BillingEmail,
                billingLastName: model.BillingLastName,
                billingCountryId: model.BillingCountryId,
                tagid: model.AdTag
                );
            var primaryStoreCurrency = await _currencyService.GetCurrencyById(_currencySettings.PrimaryStoreCurrencyId);
            if (primaryStoreCurrency == null)
                throw new Exception("Cannot load primary store currency");

            var aggregate = new AdAggreratorModel {
                aggregatorprofit = _priceFormatter.FormatPrice(profit, true, false),
                aggregatorshipping = _priceFormatter.FormatShippingPrice(reportSummary.SumShippingExclTax, true, primaryStoreCurrency, _workContext.WorkingLanguage, false),
                aggregatortax = _priceFormatter.FormatPrice(reportSummary.SumTax, true, false),
                aggregatortotal = _priceFormatter.FormatPrice(reportSummary.SumAds, true, false)
            };
            var items = new List<AdModel>();
            foreach (var x in orders)
            {
                var currency = await _currencyService.GetCurrencyByCode(x.CustomerCurrencyCode);
                var store = await _storeService.GetStoreById(x.StoreId);
                var orderTotal = _priceFormatter.FormatPrice(x.AdTotal * x.CurrencyRate, true, currency);
                if (x.CustomerCurrencyCode != x.PrimaryCurrencyCode)
                {
                    var primaryCurrency = await _currencyService.GetCurrencyByCode(x.PrimaryCurrencyCode);
                    if (primaryCurrency == null)
                        primaryCurrency = await _currencyService.GetPrimaryStoreCurrency();
                    orderTotal = $"{_priceFormatter.FormatPrice(x.AdTotal, true, primaryCurrency)} ({orderTotal})";
                }

                items.Add(new AdModel {
                    Id = x.Id,
                    AdNumber = x.AdNumber,
                    Code = x.Code,
                    StoreName = store != null ? store.Shortcut : "Unknown",
                    AdTotal = orderTotal,
                    CurrencyCode = x.CustomerCurrencyCode,
                    AdStatus = x.AdStatus.GetLocalizedEnum(_localizationService, _workContext),
                    AdStatusId = x.AdStatusId,
                    PaymentStatus = x.PaymentStatus.GetLocalizedEnum(_localizationService, _workContext),
                    ShippingStatus = x.ShippingStatus.GetLocalizedEnum(_localizationService, _workContext),
                    CustomerEmail = x.BillingAddress?.Email,
                    CustomerId = x.CustomerId,
                    CustomerFullName = string.Format("{0} {1}", x.BillingAddress?.FirstName, x.BillingAddress?.LastName),
                    CreatedOn = _dateTimeHelper.ConvertToUserTime(x.CreatedOnUtc, DateTimeKind.Utc)
                });
            }
            return (items, aggregate, orders.TotalCount);
        }


        public virtual async Task PrepareAdDetailsModel(AdModel model, Ad order)
        {
            if (order == null)
                throw new ArgumentNullException("order");

            if (model == null)
                throw new ArgumentNullException("model");

            model.Id = order.Id;
            model.AdNumber = order.AdNumber;
            model.Code = order.Code;
            model.AdStatus = order.AdStatus.GetLocalizedEnum(_localizationService, _workContext);
            model.AdStatusId = order.AdStatusId;
            model.AdGuid = order.AdGuid;
            var store = await _storeService.GetStoreById(order.StoreId);
            model.StoreName = store != null ? store.Shortcut : "Unknown";
            model.CustomerId = order.CustomerId;
            model.GenericAttributes = order.GenericAttributes;

            var customer = await _customerService.GetCustomerById(order.CustomerId);
            if (customer != null)
                model.CustomerInfo = customer.IsRegistered() ? customer.Email : _localizationService.GetResource("Admin.Customers.Guest");

            model.CustomerIp = order.CustomerIp;
            model.UrlReferrer = order.UrlReferrer;
            model.VatNumber = order.VatNumber;
            model.CreatedOn = _dateTimeHelper.ConvertToUserTime(order.CreatedOnUtc, DateTimeKind.Utc);
            model.AllowCustomersToSelectTaxDisplayType = _taxSettings.AllowCustomersToSelectTaxDisplayType;
            model.TaxDisplayType = _taxSettings.TaxDisplayType;

            var affiliate = await _affiliateService.GetAffiliateById(order.AffiliateId);
            if (affiliate != null)
            {
                model.AffiliateId = affiliate.Id;
                model.AffiliateName = affiliate.GetFullName();
            }

            //a vendor should have access only to his products
            model.IsLoggedInAsVendor = _workContext.CurrentVendor != null && !_workContext.CurrentCustomer.IsStaff();
            //custom values
            //model.CustomValues = order.DeserializeCustomValues();

            //order's tags
            if (order != null && order.AdTags.Any())
            {
                var tagsName = new List<string>();
                foreach (var item in order.AdTags)
                {
                    var tag = await _orderTagService.GetAdTagById(item);
                    if (tag != null)
                        tagsName.Add(tag.Name);
                }
                model.AdTags = string.Join(",", tagsName);
            }

            #region Ad totals

            var primaryStoreCurrency = await _currencyService.GetCurrencyByCode(order.PrimaryCurrencyCode);

            if (primaryStoreCurrency == null)
                primaryStoreCurrency = await _currencyService.GetCurrencyById(_currencySettings.PrimaryStoreCurrencyId);

            if (primaryStoreCurrency == null)
                throw new Exception("Cannot load primary store currency");

            //subtotal
            //model.AdSubtotalInclTax = _priceFormatter.FormatPrice(order.AdSubtotalInclTax, true, primaryStoreCurrency, _workContext.WorkingLanguage, true);
            //model.AdSubtotalExclTax = _priceFormatter.FormatPrice(order.AdSubtotalExclTax, true, primaryStoreCurrency, _workContext.WorkingLanguage, false);
            //model.AdSubtotalInclTaxValue = order.AdSubtotalInclTax;
            //model.AdSubtotalExclTaxValue = order.AdSubtotalExclTax;
            //discount (applied to order subtotal)
            //string orderSubtotalDiscountInclTaxStr = _priceFormatter.FormatPrice(order.AdSubTotalDiscountInclTax, true, primaryStoreCurrency, _workContext.WorkingLanguage, true);
            //string orderSubtotalDiscountExclTaxStr = _priceFormatter.FormatPrice(order.AdSubTotalDiscountExclTax, true, primaryStoreCurrency, _workContext.WorkingLanguage, false);
            //if (order.AdSubTotalDiscountInclTax > decimal.Zero)
            //    model.AdSubTotalDiscountInclTax = orderSubtotalDiscountInclTaxStr;
            //if (order.AdSubTotalDiscountExclTax > decimal.Zero)
            //    model.AdSubTotalDiscountExclTax = orderSubtotalDiscountExclTaxStr;
            //model.AdSubTotalDiscountInclTaxValue = order.AdSubTotalDiscountInclTax;
            //model.AdSubTotalDiscountExclTaxValue = order.AdSubTotalDiscountExclTax;

            //shipping
            model.AdShippingInclTax = _priceFormatter.FormatShippingPrice(order.AdShippingInclTax, true, primaryStoreCurrency, _workContext.WorkingLanguage, true);
            model.AdShippingExclTax = _priceFormatter.FormatShippingPrice(order.AdShippingExclTax, true, primaryStoreCurrency, _workContext.WorkingLanguage, false);
            model.AdShippingInclTaxValue = order.AdShippingInclTax;
            model.AdShippingExclTaxValue = order.AdShippingExclTax;

            //payment method additional fee
            if (order.PaymentMethodAdditionalFeeInclTax > decimal.Zero)
            {
                model.PaymentMethodAdditionalFeeInclTax = _priceFormatter.FormatPaymentMethodAdditionalFee(order.PaymentMethodAdditionalFeeInclTax, true, primaryStoreCurrency, _workContext.WorkingLanguage, true);
                model.PaymentMethodAdditionalFeeExclTax = _priceFormatter.FormatPaymentMethodAdditionalFee(order.PaymentMethodAdditionalFeeExclTax, true, primaryStoreCurrency, _workContext.WorkingLanguage, false);
            }
            model.PaymentMethodAdditionalFeeInclTaxValue = order.PaymentMethodAdditionalFeeInclTax;
            model.PaymentMethodAdditionalFeeExclTaxValue = order.PaymentMethodAdditionalFeeExclTax;

            //tax
            //model.Tax = await _priceFormatter.FormatPrice(order.AdTax, true, primaryStoreCurrency.CurrencyCode, false, _workContext.WorkingLanguage);
            //SortedDictionary<decimal, decimal> taxRates = order.TaxRatesDictionary;
            //bool displayTaxRates = _taxSettings.DisplayTaxRates && taxRates.Count > 0;
            //bool displayTax = !displayTaxRates;
            //foreach (var tr in order.TaxRatesDictionary)
            //{
            //    model.TaxRates.Add(new AdModel.TaxRate {
            //        Rate = _priceFormatter.FormatTaxRate(tr.Key),
            //        Value = await _priceFormatter.FormatPrice(tr.Value, true, primaryStoreCurrency.CurrencyCode, false, _workContext.WorkingLanguage),
            //    });
            //}
            //model.DisplayTaxRates = displayTaxRates;
            //model.DisplayTax = displayTax;
            //model.TaxValue = order.AdTax;
            //model.TaxRatesValue = order.TaxRates;

            ////discount
            //if (order.AdDiscount > 0)
            //    model.AdTotalDiscount = await _priceFormatter.FormatPrice(-order.AdDiscount, true, primaryStoreCurrency.CurrencyCode, false, _workContext.WorkingLanguage);
            //model.AdTotalDiscountValue = order.AdDiscount;

            ////gift cards
            //foreach (var gcuh in await _giftCardService.GetAllGiftCardUsageHistory(order.Id))
            //{
            //    var giftCard = await _giftCardService.GetGiftCardById(gcuh.GiftCardId);
            //    if (giftCard != null)
            //    {
            //        model.GiftCards.Add(new AdModel.GiftCard {
            //            CouponCode = giftCard.GiftCardCouponCode,
            //            Amount = await _priceFormatter.FormatPrice(-gcuh.UsedValue, true, primaryStoreCurrency.CurrencyCode, false, _workContext.WorkingLanguage),
            //        });
            //    }
            //}

            //reward points
            if (order.RedeemedRewardPointsEntry != null)
            {
                model.RedeemedRewardPoints = -order.RedeemedRewardPointsEntry.Points;
                model.RedeemedRewardPointsAmount = await _priceFormatter.FormatPrice(-order.RedeemedRewardPointsEntry.UsedAmount, true, primaryStoreCurrency.CurrencyCode, false, _workContext.WorkingLanguage);
            }

            //total
            model.AdTotal = await _priceFormatter.FormatPrice(order.AdTotal, true, primaryStoreCurrency.CurrencyCode, false, _workContext.WorkingLanguage);
            model.AdTotalValue = order.AdTotal;
            model.CurrencyRate = order.CurrencyRate;
            model.CurrencyCode = order.CustomerCurrencyCode;

            //refunded amount
            if (order.RefundedAmount > decimal.Zero)
                model.RefundedAmount = await _priceFormatter.FormatPrice(order.RefundedAmount, true, primaryStoreCurrency.CurrencyCode, false, _workContext.WorkingLanguage);

            //used discounts
            var duh = await _discountService.GetAllDiscountUsageHistory(orderId: order.Id);
            foreach (var d in duh)
            {
                var discount = await _discountService.GetDiscountById(d.DiscountId);
                if (discount != null)
                {
                    model.UsedDiscounts.Add(new AdModel.UsedDiscountModel {
                        DiscountId = d.DiscountId,
                        DiscountName = discount.Name,
                    });
                }
            }

            //profit (hide for vendors)
            if (_workContext.CurrentVendor == null)
            {
                var profit = await _orderReportService.ProfitReport(orderId: order.Id);
                model.Profit = await _priceFormatter.FormatPrice(profit, true, primaryStoreCurrency.CurrencyCode, false, _workContext.WorkingLanguage);
            }

            if (order.PrimaryCurrencyCode != order.CustomerCurrencyCode)
            {
                //var currency = await _currencyService.GetCurrencyByCode(order.CustomerCurrencyCode);
                model.AdTotal += $" ({await _priceFormatter.FormatPrice(order.AdTotal * order.CurrencyRate, true, order.CustomerCurrencyCode, false, _workContext.WorkingLanguage)})";
                //model.AdSubtotalInclTax += $" ({await _priceFormatter.FormatPrice(order.AdSubtotalInclTax * order.CurrencyRate, true, order.CustomerCurrencyCode, _workContext.WorkingLanguage, true)})";
                //model.AdSubtotalExclTax += $" ({await _priceFormatter.FormatPrice(order.AdSubtotalExclTax * order.CurrencyRate, true, order.CustomerCurrencyCode, _workContext.WorkingLanguage, false)})";

                //discount (applied to order subtotal)
                //if (order.AdSubTotalDiscountInclTax > decimal.Zero)
                //    model.AdSubTotalDiscountInclTax += $" ({await _priceFormatter.FormatPrice(order.AdSubTotalDiscountInclTax * order.CurrencyRate, true, order.CustomerCurrencyCode, _workContext.WorkingLanguage, true)})";
                if (order.AdSubTotalDiscountExclTax > decimal.Zero)
                    model.AdSubTotalDiscountExclTax += $" ({await _priceFormatter.FormatPrice(order.AdSubTotalDiscountExclTax * order.CurrencyRate, true, order.CustomerCurrencyCode, _workContext.WorkingLanguage, false)})";

                //shipping
                model.AdShippingInclTax += $" ({await _priceFormatter.FormatShippingPrice(order.AdShippingInclTax * order.CurrencyRate, true, order.CustomerCurrencyCode, _workContext.WorkingLanguage, true)})";
                model.AdShippingExclTax += $" ({await _priceFormatter.FormatShippingPrice(order.AdShippingExclTax * order.CurrencyRate, true, order.CustomerCurrencyCode, _workContext.WorkingLanguage, false)})";

                //payment method additional fee
                if (order.PaymentMethodAdditionalFeeInclTax > decimal.Zero)
                {
                    model.PaymentMethodAdditionalFeeInclTax += $" ({await _priceFormatter.FormatPaymentMethodAdditionalFee(order.PaymentMethodAdditionalFeeInclTax * order.CurrencyRate, true, order.CustomerCurrencyCode, _workContext.WorkingLanguage, true)})";
                    model.PaymentMethodAdditionalFeeExclTax += $" ({await _priceFormatter.FormatPaymentMethodAdditionalFee(order.PaymentMethodAdditionalFeeExclTax * order.CurrencyRate, true, order.CustomerCurrencyCode, _workContext.WorkingLanguage, false)})";
                }
                model.Tax += $" ({await _priceFormatter.FormatPrice(order.AdTax * order.CurrencyRate, true, order.CustomerCurrencyCode, false, _workContext.WorkingLanguage)})";

                //refunded amount
                if (order.RefundedAmount > decimal.Zero)
                    model.RefundedAmount += $" ({await _priceFormatter.FormatPrice(order.RefundedAmount * order.CurrencyRate, true, order.CustomerCurrencyCode, false, _workContext.WorkingLanguage)})";
            }

            #endregion

            #region Payment info

            if (order.AllowStoringCreditCardNumber)
            {
                //card type
                model.CardType = _encryptionService.DecryptText(order.CardType);
                //cardholder name
                model.CardName = _encryptionService.DecryptText(order.CardName);
                //card number
                model.CardNumber = _encryptionService.DecryptText(order.CardNumber);
                //cvv
                model.CardCvv2 = _encryptionService.DecryptText(order.CardCvv2);
                //expiry date
                string cardExpirationMonthDecrypted = _encryptionService.DecryptText(order.CardExpirationMonth);
                if (!String.IsNullOrEmpty(cardExpirationMonthDecrypted) && cardExpirationMonthDecrypted != "0")
                    model.CardExpirationMonth = cardExpirationMonthDecrypted;
                string cardExpirationYearDecrypted = _encryptionService.DecryptText(order.CardExpirationYear);
                if (!String.IsNullOrEmpty(cardExpirationYearDecrypted) && cardExpirationYearDecrypted != "0")
                    model.CardExpirationYear = cardExpirationYearDecrypted;

                model.AllowStoringCreditCardNumber = true;
            }
            else
            {
                string maskedCreditCardNumberDecrypted = _encryptionService.DecryptText(order.MaskedCreditCardNumber);
                if (!String.IsNullOrEmpty(maskedCreditCardNumberDecrypted))
                    model.CardNumber = maskedCreditCardNumberDecrypted;
            }


            //payment transaction info
            model.AuthorizationTransactionId = order.AuthorizationTransactionId;
            model.CaptureTransactionId = order.CaptureTransactionId;
            model.SubscriptionTransactionId = order.SubscriptionTransactionId;

            //payment method info
            var pm = _paymentService.LoadPaymentMethodBySystemName(order.PaymentMethodSystemName);
            model.PaymentMethod = pm != null ? pm.PluginDescriptor.FriendlyName : order.PaymentMethodSystemName;
            model.PaymentStatus = order.PaymentStatus.GetLocalizedEnum(_localizationService, _workContext);
            model.PaymentStatusEnum = order.PaymentStatus;

            //payment method buttons
            model.CanCancelAd = _orderProcessingService.CanCancelAd(order);
            model.CanCapture = await _orderProcessingService.CanCapture(order);
            model.CanMarkAdAsPaid = await _orderProcessingService.CanMarkAdAsPaid(order);
            model.CanRefund = await _orderProcessingService.CanRefund(order);
            model.CanRefundOffline = _orderProcessingService.CanRefundOffline(order);
            model.CanPartiallyRefund = await _orderProcessingService.CanPartiallyRefund(order, decimal.Zero);
            model.CanPartiallyRefundOffline = _orderProcessingService.CanPartiallyRefundOffline(order, decimal.Zero);
            model.CanVoid = await _orderProcessingService.CanVoid(order);
            model.CanVoidOffline = _orderProcessingService.CanVoidOffline(order);

            model.PrimaryStoreCurrencyCode = (await _currencyService.GetCurrencyById(_currencySettings.PrimaryStoreCurrencyId)).CurrencyCode;
            model.MaxAmountToRefund = order.AdTotal - order.RefundedAmount;

            //recurring payment record
            //var recurringPayment = (await _orderService.SearchRecurringPayments(initialAdId: order.Id, showHidden: true)).FirstOrDefault();
            //if (recurringPayment != null)
            //{
            //    model.RecurringPaymentId = recurringPayment.Id;
            //}
            #endregion

            #region Billing & shipping info

            model.BillingAddress = await order.BillingAddress.ToModel(_countryService, _stateProvinceService);
            model.BillingAddress.FormattedCustomAddressAttributes = await _addressAttributeFormatter.FormatAttributes(order.BillingAddress.CustomAttributes);
            model.BillingAddress.FirstNameEnabled = true;
            model.BillingAddress.FirstNameRequired = true;
            model.BillingAddress.LastNameEnabled = true;
            model.BillingAddress.LastNameRequired = true;
            model.BillingAddress.EmailEnabled = true;
            model.BillingAddress.EmailRequired = true;
            model.BillingAddress.CompanyEnabled = _addressSettings.CompanyEnabled;
            model.BillingAddress.CompanyRequired = _addressSettings.CompanyRequired;
            model.BillingAddress.VatNumberEnabled = _addressSettings.VatNumberEnabled;
            model.BillingAddress.VatNumberRequired = _addressSettings.VatNumberRequired;
            model.BillingAddress.CountryEnabled = _addressSettings.CountryEnabled;
            model.BillingAddress.StateProvinceEnabled = _addressSettings.StateProvinceEnabled;
            model.BillingAddress.CityEnabled = _addressSettings.CityEnabled;
            model.BillingAddress.CityRequired = _addressSettings.CityRequired;
            model.BillingAddress.StreetAddressEnabled = _addressSettings.StreetAddressEnabled;
            model.BillingAddress.StreetAddressRequired = _addressSettings.StreetAddressRequired;
            model.BillingAddress.StreetAddress2Enabled = _addressSettings.StreetAddress2Enabled;
            model.BillingAddress.StreetAddress2Required = _addressSettings.StreetAddress2Required;
            model.BillingAddress.ZipPostalCodeEnabled = _addressSettings.ZipPostalCodeEnabled;
            model.BillingAddress.ZipPostalCodeRequired = _addressSettings.ZipPostalCodeRequired;
            model.BillingAddress.PhoneEnabled = _addressSettings.PhoneEnabled;
            model.BillingAddress.PhoneRequired = _addressSettings.PhoneRequired;
            model.BillingAddress.FaxEnabled = _addressSettings.FaxEnabled;
            model.BillingAddress.FaxRequired = _addressSettings.FaxRequired;

            model.ShippingStatus = order.ShippingStatus.GetLocalizedEnum(_localizationService, _workContext);
            if (order.ShippingStatus != ShippingStatus.ShippingNotRequired)
            {
                model.IsShippable = true;

                model.PickUpInStore = order.PickUpInStore;
                if (!order.PickUpInStore)
                {
                    if (order.ShippingAddress != null)
                    {
                        model.ShippingAddress = await order.ShippingAddress.ToModel(_countryService, _stateProvinceService);
                        model.ShippingAddress.FormattedCustomAddressAttributes = await _addressAttributeFormatter.FormatAttributes(order.ShippingAddress.CustomAttributes);
                        model.ShippingAddress.FirstNameEnabled = true;
                        model.ShippingAddress.FirstNameRequired = true;
                        model.ShippingAddress.LastNameEnabled = true;
                        model.ShippingAddress.LastNameRequired = true;
                        model.ShippingAddress.EmailEnabled = true;
                        model.ShippingAddress.EmailRequired = true;
                        model.ShippingAddress.CompanyEnabled = _addressSettings.CompanyEnabled;
                        model.ShippingAddress.CompanyRequired = _addressSettings.CompanyRequired;
                        model.ShippingAddress.VatNumberEnabled = _addressSettings.VatNumberEnabled;
                        model.ShippingAddress.VatNumberRequired = _addressSettings.VatNumberRequired;
                        model.ShippingAddress.CountryEnabled = _addressSettings.CountryEnabled;
                        model.ShippingAddress.StateProvinceEnabled = _addressSettings.StateProvinceEnabled;
                        model.ShippingAddress.CityEnabled = _addressSettings.CityEnabled;
                        model.ShippingAddress.CityRequired = _addressSettings.CityRequired;
                        model.ShippingAddress.StreetAddressEnabled = _addressSettings.StreetAddressEnabled;
                        model.ShippingAddress.StreetAddressRequired = _addressSettings.StreetAddressRequired;
                        model.ShippingAddress.StreetAddress2Enabled = _addressSettings.StreetAddress2Enabled;
                        model.ShippingAddress.StreetAddress2Required = _addressSettings.StreetAddress2Required;
                        model.ShippingAddress.ZipPostalCodeEnabled = _addressSettings.ZipPostalCodeEnabled;
                        model.ShippingAddress.ZipPostalCodeRequired = _addressSettings.ZipPostalCodeRequired;
                        model.ShippingAddress.PhoneEnabled = _addressSettings.PhoneEnabled;
                        model.ShippingAddress.PhoneRequired = _addressSettings.PhoneRequired;
                        model.ShippingAddress.FaxEnabled = _addressSettings.FaxEnabled;
                        model.ShippingAddress.FaxRequired = _addressSettings.FaxRequired;

                        model.ShippingAddressGoogleMapsUrl = string.Format("http://maps.google.com/maps?f=q&hl=en&ie=UTF8&oe=UTF8&geocode=&q={0}", WebUtility.UrlEncode(order.ShippingAddress.Address1 + " " + order.ShippingAddress.ZipPostalCode + " " + order.ShippingAddress.City + " " + (!String.IsNullOrEmpty(order.ShippingAddress.CountryId) ? (await _countryService.GetCountryById(order.ShippingAddress.CountryId))?.Name : "")));
                    }
                }
                else
                {
                    if (order.PickupPoint != null)
                    {
                        if (order.PickupPoint.Address != null)
                        {
                            model.PickupAddress = await order.PickupPoint.Address.ToModel(_countryService, _stateProvinceService);
                            var country = await _countryService.GetCountryById(order.PickupPoint.Address.CountryId);
                            if (country != null)
                                model.PickupAddress.CountryName = country.Name;
                        }
                    }
                }
                model.ShippingMethod = order.ShippingMethod;
                model.ShippingAdditionDescription = order.ShippingOptionAttributeDescription;
                model.CanAddNewShipments = false;

                foreach (var orderItem in order.AdItems)
                {
                    //we can ship only shippable products
                    var product = await _productService.GetProductByIdIncludeArch(orderItem.ProductId);
                    if (product != null)
                    {
                        if (!product.IsShipEnabled)
                            continue;
                    }
                    //var totalNumberOfItemsCanBeAddedToShipment = await orderItem.GetTotalNumberOfItemsCanBeAddedToShipment(_orderService, _shipmentService);
                    //if (totalNumberOfItemsCanBeAddedToShipment <= 0)
                    //    continue;
                    //model.CanAddNewShipments = true;
                }

            }

            #endregion

            #region Products

            model.CheckoutAttributeInfo = order.CheckoutAttributeDescription;
            bool hasDownloadableItems = false;
            var products = order.AdItems;
            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && !_workContext.CurrentCustomer.IsStaff())
            {
                products = products
                    .Where(orderItem => orderItem.VendorId == _workContext.CurrentVendor.Id)
                    .ToList();
            }
            foreach (var orderItem in products)
            {
                var product = await _productService.GetProductByIdIncludeArch(orderItem.ProductId);

                if (product != null)
                {

                    if (product.IsDownload)
                        hasDownloadableItems = true;

                    var orderItemModel = new AdModel.AdItemModel {
                        Id = orderItem.Id,
                        ProductId = orderItem.ProductId,
                        ProductName = product.Name,
                        Sku = product.FormatSku(orderItem.AttributesXml, _productAttributeParser),
                        Quantity = orderItem.Quantity,
                        IsDownload = product.IsDownload,
                        DownloadCount = orderItem.DownloadCount,
                        DownloadActivationType = product.DownloadActivationType,
                        IsDownloadActivated = orderItem.IsDownloadActivated,
                    };
                    //picture
                    var orderItemPicture = await product.GetProductPicture(orderItem.AttributesXml, _productService, _pictureService, _productAttributeParser);
                    orderItemModel.PictureThumbnailUrl = await _pictureService.GetPictureUrl(orderItemPicture, 75, true);

                    //license file
                    if (!String.IsNullOrEmpty(orderItem.LicenseDownloadId))
                    {
                        var licenseDownload = await _downloadService.GetDownloadById(orderItem.LicenseDownloadId);
                        if (licenseDownload != null)
                        {
                            orderItemModel.LicenseDownloadGuid = licenseDownload.DownloadGuid;
                        }
                    }
                    //vendor
                    var vendor = await _vendorService.GetVendorById(orderItem.VendorId);
                    orderItemModel.VendorName = vendor != null ? vendor.Name : "";

                    //unit price
                    orderItemModel.UnitPriceInclTaxValue = orderItem.UnitPriceInclTax;
                    orderItemModel.UnitPriceExclTaxValue = orderItem.UnitPriceExclTax;
                    orderItemModel.UnitPriceInclTax = _priceFormatter.FormatPrice(orderItem.UnitPriceInclTax, true, primaryStoreCurrency, _workContext.WorkingLanguage, true, true);
                    orderItemModel.UnitPriceExclTax = _priceFormatter.FormatPrice(orderItem.UnitPriceExclTax, true, primaryStoreCurrency, _workContext.WorkingLanguage, false, true);
                    //discounts
                    orderItemModel.DiscountInclTaxValue = orderItem.DiscountAmountInclTax;
                    orderItemModel.DiscountExclTaxValue = orderItem.DiscountAmountExclTax;
                    orderItemModel.DiscountInclTax = _priceFormatter.FormatPrice(orderItem.DiscountAmountInclTax, true, primaryStoreCurrency, _workContext.WorkingLanguage, true, true);
                    orderItemModel.DiscountExclTax = _priceFormatter.FormatPrice(orderItem.DiscountAmountExclTax, true, primaryStoreCurrency, _workContext.WorkingLanguage, false, true);
                    //subtotal
                    orderItemModel.SubTotalInclTaxValue = orderItem.PriceInclTax;
                    orderItemModel.SubTotalExclTaxValue = orderItem.PriceExclTax;
                    orderItemModel.SubTotalInclTax = _priceFormatter.FormatPrice(orderItem.PriceInclTax, true, primaryStoreCurrency, _workContext.WorkingLanguage, false, false);
                    orderItemModel.SubTotalExclTax = _priceFormatter.FormatPrice(orderItem.PriceExclTax, true, primaryStoreCurrency, _workContext.WorkingLanguage, false, true);

                    if (order.PrimaryCurrencyCode != order.CustomerCurrencyCode)
                    {
                        var currency = await _currencyService.GetCurrencyByCode(order.CustomerCurrencyCode);
                        if (currency != null)
                        {
                            orderItemModel.UnitPriceInclTax += $" ({_priceFormatter.FormatPrice(orderItem.UnitPriceInclTax * order.CurrencyRate, true, currency, _workContext.WorkingLanguage, true, true)})";
                            orderItemModel.UnitPriceExclTax += $" ({_priceFormatter.FormatPrice(orderItem.UnitPriceExclTax * order.CurrencyRate, true, currency, _workContext.WorkingLanguage, false, true)})";
                            orderItemModel.DiscountInclTax += $" ({_priceFormatter.FormatPrice(orderItem.DiscountAmountInclTax * order.CurrencyRate, true, currency, _workContext.WorkingLanguage, true, true)})";
                            orderItemModel.DiscountExclTax += $" ({_priceFormatter.FormatPrice(orderItem.DiscountAmountExclTax * order.CurrencyRate, true, currency, _workContext.WorkingLanguage, false, true)})";
                            orderItemModel.SubTotalInclTax += $" ({_priceFormatter.FormatPrice(orderItem.PriceInclTax * order.CurrencyRate, true, currency, _workContext.WorkingLanguage, false, false)})";
                            orderItemModel.SubTotalExclTax += $" ({_priceFormatter.FormatPrice(orderItem.PriceExclTax * order.CurrencyRate, true, currency, _workContext.WorkingLanguage, false, true)})";
                        }
                    }

                    // commission
                    orderItemModel.CommissionValue = orderItem.Commission;
                    orderItemModel.Commission = _priceFormatter.FormatPrice(orderItem.Commission, true, primaryStoreCurrency, _workContext.WorkingLanguage, true, true);

                    orderItemModel.AttributeInfo = orderItem.AttributeDescription;
                    if (product.IsRecurring)
                        orderItemModel.RecurringInfo = string.Format(_localizationService.GetResource("Admin.Ads.Products.RecurringPeriod"), product.RecurringCycleLength, product.RecurringCyclePeriod.GetLocalizedEnum(_localizationService, _workContext));

                    //return requests
                    orderItemModel.ReturnRequestIds = (await _returnRequestService.SearchReturnRequests(orderItemId: orderItem.Id))
                        .Select(rr => rr.Id).ToList();
                    //gift cards
                    //orderItemModel.PurchasedGiftCardIds = (await _giftCardService.GetGiftCardsByPurchasedWithAdItemId(orderItem.Id))
                    //    .Select(gc => gc.Id).ToList();

                    model.Items.Add(orderItemModel);
                }
            }
            model.HasDownloadableProducts = hasDownloadableItems;
            #endregion
        }

        public virtual async Task<AdModel.AddAdProductModel> PrepareAddAdProductModel(Ad order)
        {
            var model = new AdModel.AddAdProductModel {
                AdId = order.Id,
                AdNumber = order.AdNumber
            };
            //categories
            model.AvailableCategories.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = " " });
            var categories = await _categoryService.GetAllCategories(showHidden: true, storeId: order.StoreId);
            foreach (var c in categories)
                model.AvailableCategories.Add(new SelectListItem { Text = _categoryService.GetFormattedBreadCrumb(c, categories), Value = c.Id.ToString() });

            //manufacturers
            model.AvailableManufacturers.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = " " });
            foreach (var m in await _manufacturerService.GetAllManufacturers(showHidden: true, storeId: order.StoreId))
                model.AvailableManufacturers.Add(new SelectListItem { Text = m.Name, Value = m.Id.ToString() });

            //product types
            model.AvailableProductTypes = ProductType.SimpleProduct.ToSelectList(_localizationService, _workContext, false).ToList();
            model.AvailableProductTypes.Insert(0, new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "0" });

            return model;
        }

        public virtual async Task<AdModel.AddAdProductModel.ProductDetailsModel> PrepareAddProductToAdModel(Ad order, string productId)
        {
            var product = await _productService.GetProductById(productId);
            if (product == null)
                throw new ArgumentException("No product found with the specified id");

            var customer = await _customerService.GetCustomerById(order.CustomerId);

            var presetQty = 1;
            var presetPrice = (await _priceCalculationService.GetFinalPrice(product, customer, decimal.Zero, true, presetQty)).finalPrice;
            decimal presetPriceInclTax = (await _taxService.GetProductPrice(product, presetPrice, true, customer)).productprice;
            decimal presetPriceExclTax = (await _taxService.GetProductPrice(product, presetPrice, false, customer)).productprice;

            var model = new AdModel.AddAdProductModel.ProductDetailsModel {
                ProductId = product.Id,
                AdId = order.Id,
                AdNumber = order.AdNumber,
                Name = product.Name,
                ProductType = product.ProductType,
                UnitPriceExclTax = presetPriceExclTax,
                UnitPriceInclTax = presetPriceInclTax,
                Quantity = presetQty,
                SubTotalExclTax = presetPriceExclTax,
                SubTotalInclTax = presetPriceInclTax
            };

            //attributes
            var attributes = product.ProductAttributeMappings;
            foreach (var attribute in attributes)
            {
                var productAttribute = await _productAttributeService.GetProductAttributeById(attribute.ProductAttributeId);
                var attributeModel = new AdModel.AddAdProductModel.ProductAttributeModel {
                    Id = attribute.Id,
                    ProductAttributeId = attribute.ProductAttributeId,
                    Name = productAttribute.Name,
                    TextPrompt = attribute.TextPrompt,
                    IsRequired = attribute.IsRequired,
                    AttributeControlType = attribute.AttributeControlType
                };

                if (attribute.ShouldHaveValues())
                {
                    //values
                    var attributeValues = attribute.ProductAttributeValues;
                    foreach (var attributeValue in attributeValues)
                    {
                        var attributeValueModel = new AdModel.AddAdProductModel.ProductAttributeValueModel {
                            Id = attributeValue.Id,
                            Name = attributeValue.Name,
                            IsPreSelected = attributeValue.IsPreSelected
                        };
                        attributeModel.Values.Add(attributeValueModel);
                    }
                }

                model.ProductAttributes.Add(attributeModel);
            }
            //gift card
            model.GiftCard.IsGiftCard = product.IsGiftCard;
            if (model.GiftCard.IsGiftCard)
            {
                model.GiftCard.GiftCardType = product.GiftCardType;
            }
            return model;
        }
        public virtual async Task<AdAddressModel> PrepareAdAddressModel(Ad order, Address address)
        {
            var model = new AdAddressModel {
                AdId = order.Id,
                Address = await address.ToModel(_countryService, _stateProvinceService)
            };
            model.Address.Id = address.Id;
            model.Address.FirstNameEnabled = true;
            model.Address.FirstNameRequired = true;
            model.Address.LastNameEnabled = true;
            model.Address.LastNameRequired = true;
            model.Address.EmailEnabled = true;
            model.Address.EmailRequired = true;
            model.Address.CompanyEnabled = _addressSettings.CompanyEnabled;
            model.Address.CompanyRequired = _addressSettings.CompanyRequired;
            model.Address.VatNumberEnabled = _addressSettings.VatNumberEnabled;
            model.Address.VatNumberRequired = _addressSettings.VatNumberRequired;
            model.Address.CountryEnabled = _addressSettings.CountryEnabled;
            model.Address.StateProvinceEnabled = _addressSettings.StateProvinceEnabled;
            model.Address.CityEnabled = _addressSettings.CityEnabled;
            model.Address.CityRequired = _addressSettings.CityRequired;
            model.Address.StreetAddressEnabled = _addressSettings.StreetAddressEnabled;
            model.Address.StreetAddressRequired = _addressSettings.StreetAddressRequired;
            model.Address.StreetAddress2Enabled = _addressSettings.StreetAddress2Enabled;
            model.Address.StreetAddress2Required = _addressSettings.StreetAddress2Required;
            model.Address.ZipPostalCodeEnabled = _addressSettings.ZipPostalCodeEnabled;
            model.Address.ZipPostalCodeRequired = _addressSettings.ZipPostalCodeRequired;
            model.Address.PhoneEnabled = _addressSettings.PhoneEnabled;
            model.Address.PhoneRequired = _addressSettings.PhoneRequired;
            model.Address.FaxEnabled = _addressSettings.FaxEnabled;
            model.Address.FaxRequired = _addressSettings.FaxRequired;

            //countries
            model.Address.AvailableCountries.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Address.SelectCountry"), Value = "" });
            foreach (var c in await _countryService.GetAllCountries(showHidden: true))
                model.Address.AvailableCountries.Add(new SelectListItem { Text = c.Name, Value = c.Id.ToString(), Selected = (c.Id == address.CountryId) });
            //states
            var states = !String.IsNullOrEmpty(address.CountryId) ? await _stateProvinceService.GetStateProvincesByCountryId(address.CountryId, showHidden: true) : new List<StateProvince>();
            if (states.Count > 0)
            {
                foreach (var s in states)
                    model.Address.AvailableStates.Add(new SelectListItem { Text = s.Name, Value = s.Id.ToString(), Selected = (s.Id == address.StateProvinceId) });
            }
            else
                model.Address.AvailableStates.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Address.OtherNonUS"), Value = "" });
            //customer attribute services
            await model.Address.PrepareCustomAddressAttributes(address, _addressAttributeService, _addressAttributeParser);

            return model;
        }

        public virtual async Task<IList<AdModel.AdNote>> PrepareAdNotes(Ad order)
        {
            //order notes
            var orderNoteModels = new List<AdModel.AdNote>();
            foreach (var orderNote in (await _orderService.GetAdNotes(order.Id))
                .OrderByDescending(on => on.CreatedOnUtc))
            {
                var download = await _downloadService.GetDownloadById(orderNote.DownloadId);
                orderNoteModels.Add(new AdModel.AdNote {
                    Id = orderNote.Id,
                    AdId = order.Id,
                    DownloadId = String.IsNullOrEmpty(orderNote.DownloadId) ? "" : orderNote.DownloadId,
                    DownloadGuid = download != null ? download.DownloadGuid : Guid.Empty,
                    DisplayToCustomer = orderNote.DisplayToCustomer,
                    Note = orderNote.FormatAdNoteText(),
                    CreatedOn = _dateTimeHelper.ConvertToUserTime(orderNote.CreatedOnUtc, DateTimeKind.Utc),
                    CreatedByCustomer = orderNote.CreatedByCustomer
                });
            }
            return orderNoteModels;
        }

        public virtual async Task InsertAdNote(Ad order, string downloadId, bool displayToCustomer, string message)
        {
            var orderNote = new AdNote {
                DisplayToCustomer = displayToCustomer,
                Note = message,
                DownloadId = downloadId,
                AdId = order.Id,
                CreatedOnUtc = DateTime.UtcNow,
            };
            await _orderService.InsertAdNote(orderNote);

            //new order notification
            if (displayToCustomer)
            {
                //email
                await _workflowMessageService.SendNewAdNoteAddedCustomerNotification(order, orderNote);
            }
        }

        public virtual async Task DeleteAdNote(Ad order, string id)
        {
            var orderNote = (await _orderService.GetAdNotes(order.Id)).FirstOrDefault(on => on.Id == id);
            if (orderNote == null)
                throw new ArgumentException("No order note found with the specified id");

            orderNote.AdId = order.Id;
            await _orderService.DeleteAdNote(orderNote);
        }

        public virtual async Task LogEditAd(string orderId)
        {
            await _customerActivityService.InsertActivity("EditAd", orderId, _localizationService.GetResource("ActivityLog.EditAd"), orderId);
        }
        public virtual async Task<Address> UpdateAdAddress(Ad order, Address address, AdAddressModel model, string customAttributes)
        {
            address = model.Address.ToEntity(address);
            address.CustomAttributes = customAttributes;
            await _orderService.UpdateAd(order);

            //add a note
            await _orderService.InsertAdNote(new AdNote {
                Note = "Address has been edited",
                DisplayToCustomer = false,
                CreatedOnUtc = DateTime.UtcNow,
                AdId = order.Id,
            });
            await LogEditAd(order.Id);
            return address;
        }
        public virtual async Task<IList<string>> AddProductToAdDetails(string orderId, string productId, IFormCollection form)
        {
            var order = await _orderService.GetAdById(orderId);
            var product = await _productService.GetProductById(productId);
            var customer = await _customerService.GetCustomerById(order.CustomerId);
            //save order item

            //basic properties
            decimal.TryParse(form["UnitPriceInclTax"], out decimal unitPriceInclTax);
            decimal.TryParse(form["UnitPriceExclTax"], out decimal unitPriceExclTax);
            int.TryParse(form["Quantity"], out int quantity);
            decimal.TryParse(form["SubTotalInclTax"], out decimal priceInclTax);
            decimal.TryParse(form["SubTotalExclTax"], out decimal priceExclTax);

            //attributes
            //warnings
            var warnings = new List<string>();
            string attributesXml = "";

            #region Product attributes

            var attributes = product.ProductAttributeMappings;
            foreach (var attribute in attributes)
            {
                string controlId = string.Format("product_attribute_{0}", attribute.Id);
                switch (attribute.AttributeControlType)
                {
                    case AttributeControlType.DropdownList:
                    case AttributeControlType.RadioList:
                    case AttributeControlType.ColorSquares:
                    case AttributeControlType.ImageSquares:
                        {
                            var ctrlAttributes = form[controlId];
                            if (!String.IsNullOrEmpty(ctrlAttributes))
                            {
                                attributesXml = _productAttributeParser.AddProductAttribute(attributesXml,
                                    attribute, ctrlAttributes);
                            }
                        }
                        break;
                    case AttributeControlType.Checkboxes:
                        {
                            var ctrlAttributes = form[controlId];
                            if (!String.IsNullOrEmpty(ctrlAttributes))
                            {
                                foreach (var item in ctrlAttributes.ToString().Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                                {
                                    attributesXml = _productAttributeParser.AddProductAttribute(attributesXml,
                                        attribute, item);
                                }
                            }
                        }
                        break;
                    case AttributeControlType.ReadonlyCheckboxes:
                        {
                            //load read-only (already server-side selected) values
                            var attributeValues = attribute.ProductAttributeValues;
                            foreach (var selectedAttributeId in attributeValues
                                .Where(v => v.IsPreSelected)
                                .Select(v => v.Id)
                                .ToList())
                            {
                                attributesXml = _productAttributeParser.AddProductAttribute(attributesXml,
                                    attribute, selectedAttributeId.ToString());
                            }
                        }
                        break;
                    case AttributeControlType.TextBox:
                    case AttributeControlType.MultilineTextbox:
                        {
                            var ctrlAttributes = form[controlId];
                            if (!String.IsNullOrEmpty(ctrlAttributes))
                            {
                                string enteredText = ctrlAttributes.ToString().Trim();
                                attributesXml = _productAttributeParser.AddProductAttribute(attributesXml,
                                    attribute, enteredText);
                            }
                        }
                        break;
                    case AttributeControlType.Datepicker:
                        {
                            var day = form[controlId + "_day"];
                            var month = form[controlId + "_month"];
                            var year = form[controlId + "_year"];
                            DateTime? selectedDate = null;
                            try
                            {
                                selectedDate = new DateTime(Int32.Parse(year), Int32.Parse(month), Int32.Parse(day));
                            }
                            catch { }
                            if (selectedDate.HasValue)
                            {
                                attributesXml = _productAttributeParser.AddProductAttribute(attributesXml,
                                    attribute, selectedDate.Value.ToString("D"));
                            }
                        }
                        break;
                    case AttributeControlType.FileUpload:
                        {
                            Guid.TryParse(form[controlId], out Guid downloadGuid);
                            var download = await _downloadService.GetDownloadByGuid(downloadGuid);
                            if (download != null)
                            {
                                attributesXml = _productAttributeParser.AddProductAttribute(attributesXml,
                                        attribute, download.DownloadGuid.ToString());
                            }
                        }
                        break;
                    default:
                        break;
                }
            }
            //validate conditional attributes (if specified)
            foreach (var attribute in attributes)
            {
                var conditionMet = _productAttributeParser.IsConditionMet(product, attribute, attributesXml);
                if (conditionMet.HasValue && !conditionMet.Value)
                {
                    attributesXml = _productAttributeParser.RemoveProductAttribute(attributesXml, attribute);
                }
            }

            #endregion

            #region Gift cards

            string recipientName = "";
            string recipientEmail = "";
            string senderName = "";
            string senderEmail = "";
            string giftCardMessage = "";
            if (product.IsGiftCard)
            {
                foreach (string formKey in form.Keys)
                {
                    if (formKey.Equals("giftcard.RecipientName", StringComparison.OrdinalIgnoreCase))
                    {
                        recipientName = form[formKey];
                        continue;
                    }
                    if (formKey.Equals("giftcard.RecipientEmail", StringComparison.OrdinalIgnoreCase))
                    {
                        recipientEmail = form[formKey];
                        continue;
                    }
                    if (formKey.Equals("giftcard.SenderName", StringComparison.OrdinalIgnoreCase))
                    {
                        senderName = form[formKey];
                        continue;
                    }
                    if (formKey.Equals("giftcard.SenderEmail", StringComparison.OrdinalIgnoreCase))
                    {
                        senderEmail = form[formKey];
                        continue;
                    }
                    if (formKey.Equals("giftcard.Message", StringComparison.OrdinalIgnoreCase))
                    {
                        giftCardMessage = form[formKey];
                        continue;
                    }
                }

                attributesXml = _productAttributeParser.AddGiftCardAttribute(attributesXml,
                    recipientName, recipientEmail, senderName, senderEmail, giftCardMessage);
            }

            #endregion

            //warnings
            //var shoppingCartService = _serviceProvider.GetRequiredService<IShoppingCartService>();
            //warnings.AddRange(await shoppingCartService.GetShoppingCartItemAttributeWarnings(customer, ShoppingCartType.ShoppingCart, product, quantity, attributesXml));
            //warnings.AddRange(shoppingCartService.GetShoppingCartItemGiftCardWarnings(ShoppingCartType.ShoppingCart, product, attributesXml));
            if (warnings.Count == 0)
            {
                //no errors
                var productAttributeFormatter = _serviceProvider.GetRequiredService<IProductAttributeFormatter>();
                //attributes
                string attributeDescription = await productAttributeFormatter.FormatAttributes(product, attributesXml, customer);

                //save item
                var orderItem = new AdItem {
                    AdItemGuid = Guid.NewGuid(),
                    ProductId = product.Id,
                    VendorId = product.VendorId,
                    UnitPriceInclTax = unitPriceInclTax,
                    UnitPriceExclTax = unitPriceExclTax,
                    PriceInclTax = priceInclTax,
                    PriceExclTax = priceExclTax,
                    OriginalProductCost = await _priceCalculationService.GetProductCost(product, attributesXml),
                    AttributeDescription = attributeDescription,
                    AttributesXml = attributesXml,
                    Quantity = quantity,
                    DiscountAmountInclTax = decimal.Zero,
                    DiscountAmountExclTax = decimal.Zero,
                    DownloadCount = 0,
                    IsDownloadActivated = false,
                    LicenseDownloadId = "",
                    CreatedOnUtc = DateTime.UtcNow,

                };
                order.AdItems.Add(orderItem);
                await _orderService.UpdateAd(order);
                await LogEditAd(order.Id);
                //adjust inventory
                await _productService.AdjustInventory(product, -orderItem.Quantity, orderItem.AttributesXml, orderItem.WarehouseId);

                //add a note
                await _orderService.InsertAdNote(new AdNote {
                    Note = "A new order item has been added",
                    DisplayToCustomer = false,
                    CreatedOnUtc = DateTime.UtcNow,
                    AdId = order.Id,
                });

                //gift cards
                //if (product.IsGiftCard)
                //{
                //    for (int i = 0; i < orderItem.Quantity; i++)
                //    {
                //        var gc = new GiftCard {
                //            GiftCardType = product.GiftCardType,
                //            PurchasedWithAdItem = orderItem,
                //            Amount = unitPriceExclTax,
                //            IsGiftCardActivated = false,
                //            GiftCardCouponCode = _giftCardService.GenerateGiftCardCode(),
                //            RecipientName = recipientName,
                //            RecipientEmail = recipientEmail,
                //            SenderName = senderName,
                //            SenderEmail = senderEmail,
                //            Message = giftCardMessage,
                //            IsRecipientNotified = false,
                //            CreatedOnUtc = DateTime.UtcNow
                //        };
                //        await _giftCardService.InsertGiftCard(gc);
                //    }
                //}

            }
            return warnings;
        }
        public virtual async Task EditCreditCardInfo(Ad order, AdModel model)
        {
            string cardType = model.CardType;
            string cardName = model.CardName;
            string cardNumber = model.CardNumber;
            string cardCvv2 = model.CardCvv2;
            string cardExpirationMonth = model.CardExpirationMonth;
            string cardExpirationYear = model.CardExpirationYear;

            order.CardType = _encryptionService.EncryptText(cardType);
            order.CardName = _encryptionService.EncryptText(cardName);
            order.CardNumber = _encryptionService.EncryptText(cardNumber);
            order.MaskedCreditCardNumber = _encryptionService.EncryptText(_paymentService.GetMaskedCreditCardNumber(cardNumber));
            order.CardCvv2 = _encryptionService.EncryptText(cardCvv2);
            order.CardExpirationMonth = _encryptionService.EncryptText(cardExpirationMonth);
            order.CardExpirationYear = _encryptionService.EncryptText(cardExpirationYear);
            await _orderService.UpdateAd(order);
        }
        public virtual async Task<IList<Ad>> PrepareAds(AdListModel model)
        {
            DateTime? startDateValue = (model.StartDate == null) ? null
                            : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.StartDate.Value, _dateTimeHelper.CurrentTimeZone);

            DateTime? endDateValue = (model.EndDate == null) ? null
                            : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.EndDate.Value, _dateTimeHelper.CurrentTimeZone).AddDays(1);

            AdStatus? orderStatus = model.AdStatusId > 0 ? (AdStatus?)(model.AdStatusId) : null;
            PaymentStatus? paymentStatus = model.PaymentStatusId > 0 ? (PaymentStatus?)(model.PaymentStatusId) : null;
            ShippingStatus? shippingStatus = model.ShippingStatusId > 0 ? (ShippingStatus?)(model.ShippingStatusId) : null;

            var filterByProductId = "";
            var product = await _productService.GetProductById(model.ProductId);
            if (product != null && _workContext.HasAccessToProduct(product))
                filterByProductId = model.ProductId;

            //load orders
            var orders = await _orderService.SearchAds(storeId: model.StoreId,
                vendorId: model.VendorId,
                productId: filterByProductId,
                warehouseId: model.WarehouseId,
                paymentMethodSystemName: model.PaymentMethodSystemName,
                createdFromUtc: startDateValue,
                createdToUtc: endDateValue,
                os: orderStatus,
                ps: paymentStatus,
                ss: shippingStatus,
                billingEmail: model.BillingEmail,
                billingLastName: model.BillingLastName,
                billingCountryId: model.BillingCountryId,
                adGuid: model.AdGuid);

            return orders;
        }

        /// <summary>
        /// Save order's tag by id
        /// </summary>
        /// <param name="order">Ad identifier</param>
        /// <param name="orderTags">Ad's tag identifier</param>
        /// <returns>Ad's tag</returns>
        public virtual async Task SaveAdTags(Ad order, string orderTags)
        {
            if (order == null)
                throw new ArgumentNullException("order");

            //order's tags
            var existingAdTags = new List<string>();
            foreach (var item in order.AdTags)
            {
                var tag = await _orderTagService.GetAdTagById(item);
                if (tag != null)
                    existingAdTags.Add(tag.Name);
            }
            var newAdTags = ParseAdTagsToList(orderTags);

            // compare 
            var orderTagsToRemove = existingAdTags.Except(newAdTags);

            foreach (var orderTag in orderTagsToRemove)
            {
                var ot = await _orderTagService.GetAdTagByName(orderTag);
                if (ot != null)
                {
                    await _orderTagService.DetachAdTag(ot.Id, order.Id);
                }
            }

            var allAdTags = await _orderTagService.GetAllAdTags();
            foreach (var newAdTag in newAdTags)
            {
                AdTag orderTag;
                var orderTag2 = allAdTags.ToList().Find(o => o.Name == newAdTag);

                if (orderTag2 == null)
                {
                    orderTag = new AdTag { Name = newAdTag, Count = 0 };
                    await _orderTagService.InsertAdTag(orderTag);
                }
                else
                {
                    orderTag = orderTag2;
                }

                if (!order.AdTagExists(orderTag))
                {
                    await _orderTagService.AttachAdTag(orderTag.Id, order.Id);
                }
            }
        }

    }
}
