using Grand.Domain.Catalog;
using Grand.Domain.Common;
using Grand.Domain.Ads;
using Grand.Domain.Shipping;
using Grand.Domain.Tax;
using Grand.Services.Catalog;
using Grand.Services.Directory;
using Grand.Services.Helpers;
using Grand.Services.Localization;
using Grand.Services.Media;
using Grand.Services.Ads;
using Grand.Services.Payments;
using Grand.Services.Queries.Models.Ads;
using Grand.Services.Shipping;
using Grand.Web.Features.Models.Ads;
using Grand.Web.Features.Models.Common;
using Grand.Web.Features.Models.Ads;
using Grand.Web.Models.Ads;
using Grand.Web.Models.Media;
using Grand.Web.Models.Ads;
using MediatR;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Web.Features.Handlers.Ads
{
    public class GetAdDetailsHandler : IRequestHandler<GetAdDetails, AdDetailsModel>
    {
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IProductService _productService;
        private readonly IProductAttributeParser _productAttributeParser;
        private readonly ILocalizationService _localizationService;
        private readonly IShipmentService _shipmentService;
        private readonly IPaymentService _paymentService;
        private readonly ICurrencyService _currencyService;
        private readonly IPriceFormatter _priceFormatter;
        private readonly IAdService _orderService;
        private readonly IPictureService _pictureService;
        private readonly IDownloadService _downloadService;
        private readonly IMediator _mediator;
        private readonly CatalogSettings _catalogSettings;
        private readonly AdSettings _orderSettings;
        private readonly PdfSettings _pdfSettings;
        private readonly TaxSettings _taxSettings;

        public GetAdDetailsHandler(
            IDateTimeHelper dateTimeHelper, 
            IProductService productService, 
            IProductAttributeParser productAttributeParser,
            ILocalizationService localizationService,
            IShipmentService shipmentService, 
            IPaymentService paymentService,
            ICurrencyService currencyService, 
            IPriceFormatter priceFormatter, 
            IAdService orderService,
            IPictureService pictureService, 
            IDownloadService downloadService, 
            IMediator mediator,
            CatalogSettings catalogSettings, 
            AdSettings orderSettings, 
            PdfSettings pdfSettings, 
            TaxSettings taxSettings)
        {
            _dateTimeHelper = dateTimeHelper;
            _productService = productService;
            _productAttributeParser = productAttributeParser;
            _localizationService = localizationService;
            _shipmentService = shipmentService;
            _paymentService = paymentService;
            _currencyService = currencyService;
            _priceFormatter = priceFormatter;
            _orderService = orderService;
            _pictureService = pictureService;
            _downloadService = downloadService;
            _mediator = mediator;
            _orderSettings = orderSettings;
            _catalogSettings = catalogSettings;
            _pdfSettings = pdfSettings;
            _taxSettings = taxSettings;
        }

        public async Task<AdDetailsModel> Handle(GetAdDetails request, CancellationToken cancellationToken)
        {
            var model = new AdDetailsModel();

            model.Id = request.Ad.Id;
            model.AdNumber = request.Ad.AdNumber;
            model.AdCode = request.Ad.Code;
            model.CreatedOn = _dateTimeHelper.ConvertToUserTime(request.Ad.CreatedOnUtc, DateTimeKind.Utc);
            model.AdStatus = request.Ad.AdStatus.GetLocalizedEnum(_localizationService, request.Language.Id);
            model.IsReAdAllowed = _orderSettings.IsReAdsAllowed;

            //shipping info

            //billing info
            model.BillingAddress = await _mediator.Send(new GetAddressModel() {
                Language = request.Language,
                Model = null,
                Address = request.Ad.BillingAddress,
                ExcludeProperties = false,
            });

            //VAT number
            model.VatNumber = request.Ad.VatNumber;

            //payment method
            //await PreparePaymentMethod(request, model);

            ////order subtotal
            //await PrepareAdTotal(request, model);

            ////tax
            //await PrepareTax(request, model);

            ////discount (applied to order total)
            //await PrepareDiscount(request, model);

            ////gift cards
            //await PrepareGiftCards(request, model);

            ////reward points           
            //await PrepareRewardPoints(request, model);

            //checkout attributes
            model.CheckoutAttributeInfo = request.Ad.CheckoutAttributeDescription;

            //order notes
            await PrepareAdNotes(request, model);

            //allow cancel order
            if (_orderSettings.UserCanCancelUnpaidAd)
            {
                if (request.Ad.AdStatus == AdStatus.Pending && request.Ad.PaymentStatus == Domain.Payments.PaymentStatus.Pending
                    && (request.Ad.ShippingStatus == ShippingStatus.ShippingNotRequired || request.Ad.ShippingStatus == ShippingStatus.NotYetShipped))
                    model.UserCanCancelUnpaidAd = true;
            }

            //purchased products
            await PrepareAdItems(request, model);
            return model;

        }

       

        private async Task PrepareAdNotes(GetAdDetails request, AdDetailsModel model)
        {
            foreach (var orderNote in (await _orderService.GetAdNotes(request.Ad.Id))
                .Where(on => on.DisplayToCustomer)
                .OrderByDescending(on => on.CreatedOnUtc)
                .ToList())
            {
                model.AdNotes.Add(new AdDetailsModel.AdNote {
                    Id = orderNote.Id,
                    AdId = orderNote.AdId,
                    HasDownload = !String.IsNullOrEmpty(orderNote.DownloadId),
                    Note = orderNote.FormatAdNoteText(),
                    CreatedOn = _dateTimeHelper.ConvertToUserTime(orderNote.CreatedOnUtc, DateTimeKind.Utc)
                });
            }

        }

        private async Task PrepareAdItems(GetAdDetails request, AdDetailsModel model)
        {
            model.ShowSku = _catalogSettings.ShowSkuOnProductDetailsPage;
            foreach (var orderItem in request.Ad.AdItems)
            {
                var product = await _productService.GetProductByIdIncludeArch(orderItem.ProductId);
                var orderItemModel = new AdDetailsModel.AdItemModel {
                    Id = orderItem.Id,
                    AdItemGuid = orderItem.AdItemGuid,
                    Sku = product.FormatSku(orderItem.AttributesXml, _productAttributeParser),
                    ProductId = product.Id,
                    ProductName = product.GetLocalized(x => x.Name, request.Language.Id),
                    ProductSeName = product.SeName,
                    Quantity = orderItem.Quantity,
                    AttributeInfo = orderItem.AttributeDescription,
                };
                //prepare picture
                orderItemModel.Picture = await PrepareAdItemPicture(product, orderItem.AttributesXml, orderItemModel.ProductName);

                model.Items.Add(orderItemModel);

                //unit price, subtotal
                if (request.Ad.CustomerTaxDisplayType == TaxDisplayType.IncludingTax)
                {
                    //including tax
                    var unitPriceInclTaxInCustomerCurrency = _currencyService.ConvertCurrency(orderItem.UnitPriceInclTax, request.Ad.CurrencyRate);
                    orderItemModel.UnitPrice = await _priceFormatter.FormatPrice(unitPriceInclTaxInCustomerCurrency, true, request.Ad.CustomerCurrencyCode, request.Language, true);
                    orderItemModel.UnitPriceValue = unitPriceInclTaxInCustomerCurrency;

                    var unitPriceWithDiscInclTaxInCustomerCurrency = _currencyService.ConvertCurrency(orderItem.UnitPriceWithoutDiscInclTax, request.Ad.CurrencyRate);
                    orderItemModel.UnitPriceWithoutDiscount = await _priceFormatter.FormatPrice(unitPriceWithDiscInclTaxInCustomerCurrency, true, request.Ad.CustomerCurrencyCode, request.Language, true);
                    orderItemModel.UnitPriceWithoutDiscountValue = unitPriceWithDiscInclTaxInCustomerCurrency;

                    var priceInclTaxInCustomerCurrency = _currencyService.ConvertCurrency(orderItem.PriceInclTax, request.Ad.CurrencyRate);
                    orderItemModel.SubTotal = await _priceFormatter.FormatPrice(priceInclTaxInCustomerCurrency, true, request.Ad.CustomerCurrencyCode, request.Language, true);
                    if (orderItem.DiscountAmountInclTax > 0)
                    {
                        var discountCustomerCurrency = _currencyService.ConvertCurrency(orderItem.DiscountAmountInclTax, request.Ad.CurrencyRate);
                        orderItemModel.Discount = await _priceFormatter.FormatPrice(discountCustomerCurrency, true, request.Ad.CustomerCurrencyCode, request.Language, true);
                    }
                }
                else
                {
                    //excluding tax
                    var unitPriceExclTaxInCustomerCurrency = _currencyService.ConvertCurrency(orderItem.UnitPriceExclTax, request.Ad.CurrencyRate);
                    orderItemModel.UnitPrice = await _priceFormatter.FormatPrice(unitPriceExclTaxInCustomerCurrency, true, request.Ad.CustomerCurrencyCode, request.Language, false);
                    orderItemModel.UnitPriceValue = unitPriceExclTaxInCustomerCurrency;

                    var unitPriceExclWithDiscTaxInCustomerCurrency = _currencyService.ConvertCurrency(orderItem.UnitPriceWithoutDiscExclTax, request.Ad.CurrencyRate);
                    orderItemModel.UnitPriceWithoutDiscount = await _priceFormatter.FormatPrice(unitPriceExclWithDiscTaxInCustomerCurrency, true, request.Ad.CustomerCurrencyCode, request.Language, true);
                    orderItemModel.UnitPriceWithoutDiscountValue = unitPriceExclWithDiscTaxInCustomerCurrency;

                    var priceExclTaxInCustomerCurrency = _currencyService.ConvertCurrency(orderItem.PriceExclTax, request.Ad.CurrencyRate);
                    orderItemModel.SubTotal = await _priceFormatter.FormatPrice(priceExclTaxInCustomerCurrency, true, request.Ad.CustomerCurrencyCode, request.Language, false);
                    if (orderItem.DiscountAmountExclTax > 0)
                    {
                        var discountCustomerCurrency = _currencyService.ConvertCurrency(orderItem.DiscountAmountExclTax, request.Ad.CurrencyRate);
                        orderItemModel.Discount = await _priceFormatter.FormatPrice(discountCustomerCurrency, true, request.Ad.CustomerCurrencyCode, request.Language, true);
                    }
                }

                ////downloadable products
                //if (_downloadService.IsDownloadAllowed(request.Ad, orderItem, product))
                //    orderItemModel.DownloadId = product.DownloadId;
                //if (_downloadService.IsLicenseDownloadAllowed(request.Ad, orderItem, product))
                //    orderItemModel.LicenseId = !string.IsNullOrEmpty(orderItem.LicenseDownloadId) ? orderItem.LicenseDownloadId : "";
            }

        }

        private async Task<PictureModel> PrepareAdItemPicture(Product product, string attributesXml, string productName)
        {
            var sciPicture = await product.GetProductPicture(attributesXml, _productService, _pictureService, _productAttributeParser);
            return new PictureModel {
                Id = sciPicture?.Id,
                ImageUrl = await _pictureService.GetPictureUrl(sciPicture, 80),
                Title = string.Format(_localizationService.GetResource("Media.Product.ImageLinkTitleFormat"), productName),
                AlternateText = string.Format(_localizationService.GetResource("Media.Product.ImageAlternateTextFormat"), productName),
            };
        }


    }
}
