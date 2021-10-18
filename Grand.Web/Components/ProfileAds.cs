using Grand.Core;
using Grand.Domain.Customers;
using Grand.Domain.Forums;
using Grand.Domain.Localization;
using Grand.Domain.Media;
using Grand.Framework.Components;
using Grand.Services.Catalog;
using Grand.Services.Customers;
using Grand.Services.Forums;
using Grand.Services.Helpers;
using Grand.Services.Localization;
using Grand.Services.Media;
using Grand.Services.Queries.Models.Ads;
using Grand.Services.Seo;
using Grand.Web.Models.Ads;
using Grand.Web.Models.Common;
using Grand.Web.Models.Media;
using Grand.Web.Models.Profile;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Grand.Web.ViewComponents
{
    public class ProfileAdsViewComponent : BaseViewComponent
    {
        private readonly ICustomerService _customerService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly ILocalizationService _localizationService;
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        readonly IMediator _mediator;
        private readonly IPictureService _pictureService;
        private readonly MediaSettings _mediaSettings;
        private readonly IProductService _productService;
        public ProfileAdsViewComponent(
            ICustomerService customerService,
            IDateTimeHelper dateTimeHelper,
            ILocalizationService localizationService,
            IMediator mediator,
            IWorkContext workContext,
            IStoreContext storeContext,
            IPictureService pictureService,
            MediaSettings mediaSettings,
            IProductService productService)
        {
            _workContext = workContext;
            _storeContext = storeContext;
            _customerService = customerService;
            _dateTimeHelper = dateTimeHelper;
            _localizationService = localizationService;
            _mediator = mediator;
            _pictureService = pictureService;
            _mediaSettings = mediaSettings;
            _productService = productService;
        }

        public async Task<IViewComponentResult> InvokeAsync(string customerProfileId, int pageNumber)
        {
            var customer = await _customerService.GetCustomerById(customerProfileId);
            if (customer == null)
                return Content("");
            
            if (!_workContext.CurrentCustomer.IsRegistered())
                return Content("");

            if (pageNumber > 0)
                pageNumber -= 1;

            var pagerModel = new PagerModel(_localizationService) {
                PageSize = 0,
                TotalRecords = 0,
                PageIndex = 0,
                ShowTotalSummary = false,
                RouteActionName = "CustomerProfilePaged",
                UseRouteLinks = true,
                RouteValues = new RouteValues { pageNumber = pageNumber, id = customerProfileId }
            };

            var profileAds = await PrepareAd(_workContext.WorkingLanguage, customer);

            var model = new ProfileAdsModel {
                PagerModel = pagerModel,
                Ads = profileAds,
            };

            return View(model);
        }

        private async Task<IList<CustomerAdListModel.AdDetailsModel>> PrepareAd(Language curLanguage, Customer customer)
        {
            var query = new GetAdQuery {
                //StoreId = request.Store.Id
                CustomerId = customer.Id
            };

            var ads = await _mediator.Send(query);
            var retAds = new List<CustomerAdListModel.AdDetailsModel>();

            foreach (var ad in ads)
            {
                var rp = await _productService.GetProductById(ad.ProductId);

                var pictureModel = new PictureModel {
                    Id = ad.PictureId,
                    FullSizeImageUrl = await _pictureService.GetPictureUrl(ad.PictureId),
                    ImageUrl = await _pictureService.GetPictureUrl(ad.PictureId, _mediaSettings.VendorThumbPictureSize),
                    Title = string.Format(_localizationService.GetResource("Media.Vendor.ImageLinkTitleFormat"), rp.Name),
                    AlternateText = string.Format(_localizationService.GetResource("Media.Ad.ImageAlternateTextFormat"), rp.Name),
                };

                var adModel = new CustomerAdListModel.AdDetailsModel {
                    Id = ad.Id,
                    AdNumber = ad.AdNumber,
                    AdCode = ad.Code,
                    CustomerEmail = ad.BillingAddress?.Email,
                    CreatedOn = _dateTimeHelper.ConvertToUserTime(ad.CreatedOnUtc, DateTimeKind.Utc),
                    EndDateTimeUtc = _dateTimeHelper.ConvertToUserTime(ad.EndDateTimeUtc, DateTimeKind.Utc),
                    Price = ad.Price,
                    AdStatusEnum = ad.AdStatus,
                    AdStatus = ad.AdStatus.GetLocalizedEnum(_localizationService, curLanguage.Id),
                    PaymentStatus = ad.PaymentStatus.GetLocalizedEnum(_localizationService, curLanguage.Id),
                    ShippingStatus = ad.ShippingStatus.GetLocalizedEnum(_localizationService, curLanguage.Id),
                    PictureModel = pictureModel,
                    ProductName = rp.Name,
                    IsOpenFromMenu = false
                    //IsReturnRequestAllowed = await _mediator.Send(new IsReturnRequestAllowedQuery() { Ad = ad })
                };
                var adTotalInCustomerCurrency = 0;
                retAds.Add(adModel);
            }
            return retAds;
        }
    }
}