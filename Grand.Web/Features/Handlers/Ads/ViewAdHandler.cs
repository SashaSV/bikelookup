using Grand.Core;
using Grand.Core.Caching;
using Grand.Domain.Ads;
using Grand.Domain.Catalog;
using Grand.Domain.Customers;
using Grand.Domain.Data;
using Grand.Domain.Media;
using Grand.Services.Ads;
using Grand.Services.Catalog;
using Grand.Services.Customers;
using Grand.Services.Directory;
using Grand.Services.Helpers;
using Grand.Services.Localization;
using Grand.Services.Media;
using Grand.Services.Queries.Models.Ads;
using Grand.Services.Seo;
using Grand.Web.Features.Models.Ads;
using Grand.Web.Infrastructure.Cache;
using Grand.Web.Models.Ads;
using Grand.Web.Models.Catalog;
using Grand.Web.Models.Media;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Web.Features.Handlers.Ads
{
    public class ViewAdHandler : IRequestHandler<ViewAd, ViewAdModel>
    {
        private readonly IAdService _adService;
        private readonly IRepository<Ad> _adRepository;
        private readonly IProductService _productService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly ILocalizationService _localizationService;
        private readonly IAdProcessingService _adProcessingService;
        private readonly ICurrencyService _currencyService;
        private readonly IPriceFormatter _priceFormatter;
        private readonly IMediator _mediator;
        private readonly MediaSettings _mediaSettings;
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly ICacheManager _cacheManager;
        private readonly IPictureService _pictureService;
        private readonly CatalogSettings _catalogSettings;
        private readonly ICategoryService _categoryService;

        public ViewAdHandler(
            IAdService adService,
            IDateTimeHelper dateTimeHelper,
            ILocalizationService localizationService,
            IAdProcessingService adProcessingService,
            ICurrencyService currencyService,
            IMediator mediator,
            IPriceFormatter priceFormatter,
            IRepository<Ad> adRepository,
            IProductService productService,
            MediaSettings mediaSettings,
            IWorkContext workContext,
            IStoreContext storeContext,
            ICacheManager cacheManager,
            IPictureService pictureService,
            CatalogSettings catalogSettings,
            ICategoryService categoryService)
        {
            _adService = adService;
            _dateTimeHelper = dateTimeHelper;
            _localizationService = localizationService;
            _adProcessingService = adProcessingService;
            _currencyService = currencyService;
            _priceFormatter = priceFormatter;
            _mediator = mediator;
            _adRepository = adRepository;
            _productService = productService;
            _mediaSettings = mediaSettings;
            _workContext = workContext;
            _storeContext = storeContext;
            _cacheManager = cacheManager;
            _pictureService = pictureService;
            _catalogSettings = catalogSettings;
            _categoryService = categoryService;
        }

        public async Task<ViewAdModel> Handle(ViewAd request, CancellationToken cancellationToken)
        {
            var model = new ViewAdModel() { };

            var ad = await _adRepository.GetByIdAsync(request.Ad.Id);
            var product = await _productService.GetProductById(ad.AdItem.ProductId);
            var rp = await _productService.GetProductById(ad.ProductId);
            model.AdPructName = rp == null ? "" : rp.Name;

            model.DefaultPictureZoomEnabled = _mediaSettings.DefaultPictureZoomEnabled;

            var defaultPictureSize = _mediaSettings.ProductDetailsPictureSize;
            var isAssociatedProduct = false;

            var cachedPictures = await PrepareProductPictureModel(product, defaultPictureSize, isAssociatedProduct, model.AdPructName);
            model.DefaultPictureModel = cachedPictures.defaultPictureModel;
            model.PictureModels = cachedPictures.pictureModels;
            model.Breadcrumb = await PrepareProductBreadcrumbModel(rp);

            model.AdNumber = ad.AdNumber;
            model.CreatedOnUtc = ad.CreatedOnUtc;
            model.AdComment = ad.AdComment;
            model.Price = ad.Price;
            model.CustomerAddress = ad.ShippingAddress;
            model.WithDocuments = ad.WithDocuments;
            model.Mileage = ad.Mileage;
            model.IsAuction = ad.IsAuction;
            var adTotalInCustomerCurrency = ad.Price;
            model.AdTotal = await _priceFormatter.FormatPrice(adTotalInCustomerCurrency, true, ad.CustomerCurrencyCode, false, request.Language);

            return model;
        }

        private async Task<(PictureModel defaultPictureModel, List<PictureModel> pictureModels)> PrepareProductPictureModel(Product product, int defaultPictureSize, bool isAssociatedProduct, string name)
        {
            var productPicturesCacheKey = string.Format(ModelCacheEventConst.PRODUCT_DETAILS_PICTURES_MODEL_KEY, product.Id, defaultPictureSize,
                isAssociatedProduct, _workContext.WorkingLanguage.Id, _storeContext.CurrentStore.Id);
            return await _cacheManager.GetAsync(productPicturesCacheKey, async () =>
            {
                var defaultPicture = product.ProductPictures.OrderBy(x => x.DisplayOrder).FirstOrDefault();
                if (defaultPicture == null)
                    defaultPicture = new ProductPicture();

                var defaultPictureModel = new PictureModel {
                    Id = defaultPicture.PictureId,
                    ImageUrl = await _pictureService.GetPictureUrl(defaultPicture.PictureId, defaultPictureSize, !isAssociatedProduct),
                    FullSizeImageUrl = await _pictureService.GetPictureUrl(defaultPicture.PictureId, 0, !isAssociatedProduct),
                };
                //"title" attribute
                defaultPictureModel.Title = (defaultPicture != null && !string.IsNullOrEmpty(defaultPicture.TitleAttribute)) ?
                    defaultPicture.TitleAttribute :
                    string.Format(_localizationService.GetResource("Media.Product.ImageLinkTitleFormat.Details"), name);
                //"alt" attribute
                defaultPictureModel.AlternateText = (defaultPicture != null && !string.IsNullOrEmpty(defaultPicture.AltAttribute)) ?
                    defaultPicture.AltAttribute :
                    string.Format(_localizationService.GetResource("Media.Product.ImageAlternateTextFormat.Details"), name);

                //all pictures
                var pictureModels = new List<PictureModel>();
                foreach (var picture in product.ProductPictures.OrderBy(x => x.DisplayOrder))
                {
                    var pictureModel = new PictureModel {
                        Id = picture.PictureId,
                        ThumbImageUrl = await _pictureService.GetPictureUrl(picture.PictureId, _mediaSettings.ProductThumbPictureSizeOnProductDetailsPage),
                        ImageUrl = await _pictureService.GetPictureUrl(picture.PictureId, _mediaSettings.ProductDetailsPictureSize),
                        FullSizeImageUrl = await _pictureService.GetPictureUrl(picture.PictureId),
                        Geometry = picture.Geometry,
                        Title = string.Format(_localizationService.GetResource("Media.Product.ImageLinkTitleFormat.Details"), name),
                        AlternateText = string.Format(_localizationService.GetResource("Media.Product.ImageAlternateTextFormat.Details"), name),
                    };
                    //"title" attribute
                    pictureModel.Title = !string.IsNullOrEmpty(picture.TitleAttribute) ?
                        picture.TitleAttribute :
                        string.Format(_localizationService.GetResource("Media.Product.ImageLinkTitleFormat.Details"), name);
                    //"alt" attribute
                    pictureModel.AlternateText = !string.IsNullOrEmpty(picture.AltAttribute) ?
                       picture.AltAttribute :
                       string.Format(_localizationService.GetResource("Media.Product.ImageAlternateTextFormat.Details"), name);

                    pictureModels.Add(pictureModel);
                }
                return (defaultPictureModel, pictureModels);
            });
        }

        private async Task<ProductDetailsModel.ProductBreadcrumbModel> PrepareProductBreadcrumbModel(Product product)
        {
            var breadcrumbCacheKey = string.Format(ModelCacheEventConst.PRODUCT_BREADCRUMB_MODEL_KEY,
                product.Id,
                _workContext.WorkingLanguage.Id,
                string.Join(",", _workContext.CurrentCustomer.GetCustomerRoleIds()),
                _storeContext.CurrentStore.Id);
            return await _cacheManager.GetAsync(breadcrumbCacheKey, async () =>
            {
                var breadcrumbModel = new ProductDetailsModel.ProductBreadcrumbModel {

                    Enabled = _catalogSettings.CategoryBreadcrumbEnabled,
                    ProductId = product.Id,
                    ProductName = product.GetLocalized(x => x.Name, _workContext.WorkingLanguage.Id),
                    ProductSeName = product.GetSeName(_workContext.WorkingLanguage.Id)
                };
                var productCategories = product.ProductCategories;
                if (productCategories.Any())
                {
                    var category = await _categoryService.GetCategoryById(productCategories.OrderBy(x => x.DisplayOrder).FirstOrDefault().CategoryId);
                    if (category != null)
                    {
                        foreach (var catBr in await _categoryService.GetCategoryBreadCrumb(category))
                        {
                            breadcrumbModel.CategoryBreadcrumb.Add(new CategorySimpleModel {
                                Id = catBr.Id,
                                Name = catBr.GetLocalized(x => x.Name, _workContext.WorkingLanguage.Id),
                                SeName = catBr.GetSeName(_workContext.WorkingLanguage.Id),
                                IncludeInTopMenu = catBr.IncludeInTopMenu
                            });
                        }
                    }
                }
                return breadcrumbModel;
            });
        }
    }
}
