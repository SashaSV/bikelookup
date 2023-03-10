using Grand.Domain.Ads;
using Grand.Domain.Customers;
using Grand.Domain.Media;
using Grand.Services.Ads;
using Grand.Services.Catalog;
using Grand.Services.Directory;
using Grand.Services.Helpers;
using Grand.Services.Localization;
using Grand.Services.Media;
using Grand.Services.Queries.Models.Ads;
using Grand.Web.Features.Models.Ads;
using Grand.Web.Models.Ads;
using Grand.Web.Models.Media;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using Grand.Services.Forums;
using Grand.Domain.Forums;

namespace Grand.Web.Features.Handlers.Ads
{
    public class GetCustomerAdListHandler : IRequestHandler<GetCustomerAdList, CustomerAdListModel>
    {
        private readonly IAdService _adService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly ILocalizationService _localizationService;
        private readonly IAdProcessingService _adProcessingService;
        private readonly ICurrencyService _currencyService;
        private readonly IPriceFormatter _priceFormatter;
        private readonly IMediator _mediator;
        private readonly IPictureService _pictureService;
        private readonly MediaSettings _mediaSettings;
        private readonly IProductService _productService;
        private readonly IForumService _forumService;
        private readonly ForumSettings _forumSettings;

        public GetCustomerAdListHandler(
            IAdService adService,
            IDateTimeHelper dateTimeHelper,
            ILocalizationService localizationService,
            IAdProcessingService adProcessingService,
            ICurrencyService currencyService,
            IMediator mediator,
            IPriceFormatter priceFormatter,
            IPictureService pictureService,
            MediaSettings mediaSettings,
            IProductService productService, 
            IForumService forumService,
            ForumSettings forumSettings)
        {
            _adService = adService;
            _dateTimeHelper = dateTimeHelper;
            _localizationService = localizationService;
            _adProcessingService = adProcessingService;
            _currencyService = currencyService;
            _priceFormatter = priceFormatter;
            _mediator = mediator;
            _pictureService = pictureService;
            _mediaSettings = mediaSettings;
            _productService = productService;
            _forumService = forumService;
            _forumSettings = forumSettings;
        }

        public async Task<CustomerAdListModel> Handle(GetCustomerAdList request, CancellationToken cancellationToken)
        {
            var model = new CustomerAdListModel();
            await PrepareAd(model, request);
            //await PrepareRecurringPayments(model, request);
            return model;
        }

        private async Task PrepareAd(CustomerAdListModel model, GetCustomerAdList request)
        {
            var query = new GetAdQuery {
                StoreId = request.Store.Id
            };

            if (!request.Customer.IsOwner())
                query.CustomerId = request.Customer.Id;
            else
                query.OwnerId = request.Customer.Id;

            var ads = await _mediator.Send(query);

            foreach (var ad in ads)
            {
                
                var rp = await _productService.GetProductById(ad.ProductId);

                var nameProduct = rp == null ? "" : rp.Name;
                var productIdAd = ad.AdItem == null ? "" : ad.AdItem.ProductId;
                var rpAd = await _productService.GetProductById(productIdAd);
                var firstPictureId = rpAd == null || rpAd.ProductPictures == null || rpAd.ProductPictures.Count == 0 ? "": rpAd.ProductPictures.First().PictureId;

                var pictureModel = new PictureModel {
                    Id = firstPictureId,
                    FullSizeImageUrl = await _pictureService.GetPictureUrl(firstPictureId),
                    ImageUrl = await _pictureService.GetPictureUrl(firstPictureId, _mediaSettings.CategoryThumbPictureSize),
                    Title = string.Format(_localizationService.GetResource("Media.Vendor.ImageLinkTitleFormat"), nameProduct),
                    AlternateText = string.Format(_localizationService.GetResource("Media.Ad.ImageAlternateTextFormat"), nameProduct),
                };


                var adModel = new CustomerAdListModel.AdDetailsModel {
                    Id = ad.Id,
                    AdNumber = ad.AdNumber,
                    AdCode = ad.Code,
                    CustomerEmail = ad.BillingAddress?.Email,
                    CreatedOn = _dateTimeHelper.ConvertToUserTime(ad.CreatedOnUtc, DateTimeKind.Utc),
                    EndDateTimeUtc = _dateTimeHelper.ConvertToUserTime(ad.EndDateTimeUtc, DateTimeKind.Utc),
                    AdStatusEnum = ad.AdStatus,
                    AdStatus = ad.AdStatus.GetLocalizedEnum(_localizationService, request.Language.Id),
                    PaymentStatus = ad.PaymentStatus.GetLocalizedEnum(_localizationService, request.Language.Id),
                    ShippingStatus = ad.ShippingStatus.GetLocalizedEnum(_localizationService, request.Language.Id),
                    PictureModel = pictureModel,
                    ProductName = nameProduct,
                    Price = ad.Price,
                    IsOpenFromMenu = true,
                    IsCancel = (ad.AdStatus == AdStatus.Cancelled),
                    AdComment = string.IsNullOrEmpty(ad.AdComment) ? string.Empty : ad.AdComment
                    //IsReturnRequestAllowed = await _mediator.Send(new IsReturnRequestAllowedQuery() { Ad = ad })
                };

                adModel.AdComment = adModel.AdComment != null && adModel.AdComment.Trim().Length >= 250 
                    ? adModel.AdComment.Trim().Substring(0, 250) + " ..." 
                    : adModel.AdComment.Trim();

                //var adTotalInCustomerCurrency = _currencyService.ConvertCurrency(ad.AdTotal, ad.CurrencyRate);
                var adTotalInCustomerCurrency = ad.Price;
                adModel.AdTotal = await _priceFormatter.FormatPrice(adTotalInCustomerCurrency, true, ad.CustomerCurrencyCode, false, request.Language);
                
                var pageSize = _forumSettings.PrivateMessagesPageSize;
                var listMsg = await _forumService.GetAllPrivateMessages(request.Store.Id,
                    "", "", adModel.Id, null, false, null, string.Empty, 0, pageSize);
                adModel.CountMsg = listMsg.Count;
                model.Ads.Add(adModel);
            }
        }
        private async Task PrepareRecurringPayments(CustomerAdListModel model, GetCustomerAdList request)
        {
            var recurringPayments = await _adService.SearchRecurringPayments(request.Store.Id,
                request.Customer.Id);
            //foreach (var recurringPayment in recurringPayments)
            //{
            //    var recurringPaymentModel = new CustomerAdListModel.RecurringAdModel {
            //        Id = recurringPayment.Id,
            //        StartDate = _dateTimeHelper.ConvertToUserTime(recurringPayment.StartDateUtc, DateTimeKind.Utc).ToString(),
            //        CycleInfo = string.Format("{0} {1}", recurringPayment.CycleLength, recurringPayment.CyclePeriod.GetLocalizedEnum(_localizationService, request.Language.Id)),
            //        NextPayment = recurringPayment.NextPaymentDate.HasValue ? _dateTimeHelper.ConvertToUserTime(recurringPayment.NextPaymentDate.Value, DateTimeKind.Utc).ToString() : "",
            //        TotalCycles = recurringPayment.TotalCycles,
            //        CyclesRemaining = recurringPayment.CyclesRemaining,
            //        InitialAdId = recurringPayment.InitialAd.Id,
            //        CanCancel = await _adProcessingService.CanCancelRecurringPayment(request.Customer, recurringPayment),
            //    };

                //model.RecurringAds.Add(recurringPaymentModel);
            //}
        }

    }
}
