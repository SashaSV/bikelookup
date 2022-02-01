using Grand.Core;
using Grand.Core.Caching;
using Grand.Domain.Ads;
using Grand.Domain.Catalog;
using Grand.Domain.Customers;
using Grand.Domain.Data;
using Grand.Domain.Forums;
using Grand.Domain.Localization;
using Grand.Domain.Media;
using Grand.Domain.Vendors;
using Grand.Framework.Security.Captcha;
using Grand.Services.Ads;
using Grand.Services.Catalog;
using Grand.Services.Customers;
using Grand.Services.Directory;
using Grand.Services.Forums;
using Grand.Services.Helpers;
using Grand.Services.Localization;
using Grand.Services.Media;
using Grand.Services.Queries.Models.Ads;
using Grand.Services.Seo;
using Grand.Services.Vendors;
using Grand.Web.Features.Models.Ads;
using Grand.Web.Features.Models.Catalog;
using Grand.Web.Features.Models.Common;
using Grand.Web.Infrastructure.Cache;
using Grand.Web.Models.Ads;
using Grand.Web.Models.Catalog;
using Grand.Web.Models.Media;
using Grand.Web.Models.PrivateMessages;
using Grand.Web.Models.Vendors;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static Grand.Web.Models.Ads.ViewAdModel;

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
        private readonly IVendorService _vendorService;
        private readonly VendorSettings _vendorSettings;
        private readonly CustomerSettings _customerSettings;
        private readonly ICustomerService _customerService;
        private readonly CaptchaSettings _captchaSettings;
        private readonly ForumSettings _forumSettings;
        private readonly IForumService _forumService;
        private readonly ISpecificationAttributeService _atributeService;
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
            ICategoryService categoryService,
            IVendorService vendorService,
            VendorSettings vendorSettings,
            CustomerSettings customerSettings,
            ICustomerService customerService,
            CaptchaSettings captchaSettings,
            ForumSettings forumSettings,
            IForumService forumService,
            ISpecificationAttributeService atributeService
            )
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
            _vendorService = vendorService;
            _vendorSettings = vendorSettings;
            _customerSettings = customerSettings;
            _customerService = customerService;
            _captchaSettings = captchaSettings;
            _forumSettings = forumSettings;
            _forumService = forumService;
            _atributeService = atributeService;
        }

        public async Task<ViewAdModel> Handle(ViewAd request, CancellationToken cancellationToken)
        {
            var model = new ViewAdModel() { };

            var ad = await _adService.GetAdById(request.Ad.Id);
            var product = await _productService.GetProductById(ad.AdItem.ProductId);
            var productAssociated = product;

            var rp = await _productService.GetProductById(ad.ProductId);
            var vendor = await _adService.GetVendorByAd(ad);

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
            model.VendorModel = await PrepareVendorModel(vendor, cancellationToken, request.Language);

            var toCustomerId = string.IsNullOrEmpty(ad.OwnerId) ? ad.CustomerId : ad.OwnerId;
            var fromCustomer = await _customerService.GetCustomerById(_workContext.CurrentCustomer.Id);
            var toCustomer = await _customerService.GetCustomerById(toCustomerId);

            var messages = new List<PrivateMessageModel>();
            messages = await GetMessage(messages, ad, rp, fromCustomer, toCustomer);
            messages = await GetMessage(messages, ad, rp, toCustomer, fromCustomer);

            var messages2 = messages.OrderBy(x => x.CreatedOn).ToList();
            var dates = messages2.Select(x => x.CreatedOn.Date).Distinct().OrderBy(x => x.Date).ToList();
            //PrivateMessageChatModel
            model.PrivateMessageChatModel = new PrivateMessageChatModel {
                Messages = messages2,
                ToCustomerId = toCustomerId,
                AdId = ad.Id,
                Subject = rp.Name,
                Dates = dates,
                IsVisibleMessageChat = (toCustomerId != _workContext.CurrentCustomer.Id)
            };

            var paymentAtribute = await _atributeService.GetSpecificationAttributeBySeName("v_pay");

            //model.PaymentMethodType = paymentAtribute?.SpecificationAttributeOptions.Select(a => new PaymentsMethodType { Id = a.Id, Name = a.GetLocalized(x => x.Name, request.Language.Id) }).ToList();

            foreach (var paymentOption in productAssociated.ProductSpecificationAttributes.Where(psa => psa.SpecificationAttributeId == paymentAtribute.Id))
            {
                //model.SelectedPaymentMethods.Add(paymentOption.SpecificationAttributeOptionId);
                var sao = paymentAtribute?.SpecificationAttributeOptions.FirstOrDefault(a => a.Id == paymentOption.SpecificationAttributeOptionId);
                if (sao != null)
                {
                    model.PaymentMethodType.Add(new PaymentsMethodType { Id = sao.Id, Name = sao.GetLocalized(x => x.Name, request.Language.Id) });
                }
                

            }

            var delivery = await _atributeService.GetSpecificationAttributeBySeName("v_delivery");
            //model.ShippingMethodType = delivery?.SpecificationAttributeOptions.Select(a => new ShipmentMethodType { Id = a.Id, Name = a.GetLocalized(x => x.Name, request.Language.Id) }).ToList();

            foreach (var deliveryOption in productAssociated.ProductSpecificationAttributes.Where(psa => psa.SpecificationAttributeId == delivery.Id))
            {
                // model.SelectedShippingMethods.Add(paymentOption.SpecificationAttributeOptionId);
                var sao = delivery?.SpecificationAttributeOptions.FirstOrDefault(a => a.Id == deliveryOption.SpecificationAttributeOptionId);
                if (sao != null) 
                {
                    model.ShippingMethodType.Add(new ShipmentMethodType { Id = sao.Id, Name = sao.GetLocalized(x => x.Name, request.Language.Id) });
                }
                
            }

            //await _mediator.Send(new GetVendor() {
            //    Command = request.Command,
            //    Vendor = vendor,
            //    Language = _workContext.WorkingLanguage,
            //    Customer = _workContext.CurrentCustomer,
            //    Store = _storeContext.CurrentStore,
            //});

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


        public async Task<VendorModel> PrepareVendorModel(Vendor vendor, CancellationToken cancellationToken, Language language)
        {
            var model = new VendorModel {
                Id = vendor.Id,
                Name = vendor.GetLocalized(x => x.Name, language.Id),
                Description = vendor.GetLocalized(x => x.Description, language.Id),
                MetaKeywords = vendor.GetLocalized(x => x.MetaKeywords, language.Id),
                MetaDescription = vendor.GetLocalized(x => x.MetaDescription, language.Id),
                MetaTitle = vendor.GetLocalized(x => x.MetaTitle, language.Id),
                SeName = vendor.GetSeName(language.Id),
                AllowCustomersToContactVendors = _vendorSettings.AllowCustomersToContactVendors,
                GenericAttributes = vendor.GenericAttributes,
                IsPrivatePerson = vendor.IsPrivatePerson
            };

            model.Address = await _mediator.Send(new GetVendorAddress() {
                Language = language,
                Address = vendor.Addresses.FirstOrDefault(),
                ExcludeProperties = false,
            });

            //prepare picture model
            var pictureModel = new PictureModel {
                Id = vendor.PictureId,
                FullSizeImageUrl = await _pictureService.GetPictureUrl(vendor.PictureId),
                ImageUrl = await _pictureService.GetPictureUrl(vendor.PictureId, _mediaSettings.VendorThumbPictureSize),
                Title = string.Format(_localizationService.GetResource("Media.Vendor.ImageLinkTitleFormat"), model.Name),
                AlternateText = string.Format(_localizationService.GetResource("Media.Vendor.ImageAlternateTextFormat"), model.Name)
            };
            model.PictureModel = pictureModel;

            //VendorReviewOverview
            model.VendorReviewOverview = new VendorReviewOverviewModel() {
                RatingSum = vendor.ApprovedRatingSum,
                TotalReviews = vendor.ApprovedTotalReviews,
                VendorId = vendor.Id,
                AllowCustomerReviews = vendor.AllowCustomerReviews,
                VendorSeName = vendor.SeName
            };
            model.VendorReviews = await PrepareVendorReviesModel(vendor);

            return model;
        }

        public virtual async Task<List<PrivateMessageModel>> GetMessage(List<PrivateMessageModel> message, Ad ad, Product rp, Customer fromCustomer, Customer toCustomer)
        {
            var pageNumber = 0;
            if (pageNumber > 0)
            {
                pageNumber -= 1;
            }

            var pageSize = _forumSettings.PrivateMessagesPageSize;

            var list = await _forumService.GetAllPrivateMessages(_storeContext.CurrentStore.Id,
                fromCustomer.Id, toCustomer.Id, ad.Id, null, null, false, string.Empty, pageNumber, pageSize);

            foreach (var pm in list)
            {
                if (!pm.IsRead && pm.ToCustomerId == _workContext.CurrentCustomer.Id)
                {
                    pm.IsRead = true;
                    await _forumService.UpdatePrivateMessage(pm);
                }
                message.Add(new PrivateMessageModel {
                    Id = pm.Id,
                    FromCustomerId = (fromCustomer.Id == _workContext.CurrentCustomer.Id) ? string.Empty : fromCustomer.Id,
                    CustomerFromName = fromCustomer.FormatUserName(_customerSettings.CustomerNameFormat),
                    AllowViewingFromProfile = _customerSettings.AllowViewingProfiles && fromCustomer != null && !fromCustomer.IsGuest(),
                    ToCustomerId = (toCustomer.Id == _workContext.CurrentCustomer.Id) ? string.Empty : toCustomer.Id,
                    CustomerToName = toCustomer.FormatUserName(_customerSettings.CustomerNameFormat),
                    AllowViewingToProfile = _customerSettings.AllowViewingProfiles && toCustomer != null && !toCustomer.IsGuest(),
                    Subject = rp.Name,
                    Message = pm.Text,
                    CreatedOn = _dateTimeHelper.ConvertToUserTime(pm.CreatedOnUtc, DateTimeKind.Utc),
                    IsRead = pm.IsRead,
                    AdId = ad.Id,
                    AdProductName = rp.Name
                });
            }
            return message;
        }

        private async Task<VendorReviewsModel> PrepareVendorReviesModel(Vendor vendor)
        {
            var model = new VendorReviewsModel();
            model.VendorId = vendor.Id;
            model.VendorName = vendor.GetLocalized(x => x.Name, _workContext.WorkingLanguage.Id);
            model.VendorSeName = vendor.GetSeName(_workContext.WorkingLanguage.Id);

            var vendorReviews = await _vendorService.GetAllVendorReviews("", true, null, null, "", vendor.Id);
            foreach (var pr in vendorReviews)
            {
                var customer = await _customerService.GetCustomerById(pr.CustomerId);
                model.Items.Add(new VendorReviewModel {
                    Id = pr.Id,
                    CustomerId = pr.CustomerId,
                    CustomerName = customer.FormatUserName(_customerSettings.CustomerNameFormat),
                    AllowViewingProfiles = _customerSettings.AllowViewingProfiles && customer != null && !customer.IsGuest(),
                    Title = pr.Title,
                    ReviewText = pr.ReviewText,
                    Rating = pr.Rating,
                    Helpfulness = new VendorReviewHelpfulnessModel {
                        VendorId = vendor.Id,
                        VendorReviewId = pr.Id,
                        HelpfulYesTotal = pr.HelpfulYesTotal,
                        HelpfulNoTotal = pr.HelpfulNoTotal,
                    },
                    WrittenOnStr = _dateTimeHelper.ConvertToUserTime(pr.CreatedOnUtc, DateTimeKind.Utc).ToString("g"),
                });
                return model;
            }

            model.AddVendorReview.CanCurrentCustomerLeaveReview = _vendorSettings.AllowAnonymousUsersToReviewVendor || !_workContext.CurrentCustomer.IsGuest();
            model.AddVendorReview.DisplayCaptcha = _captchaSettings.Enabled && _captchaSettings.ShowOnVendorReviewPage;

            return model;
        }
    }
}
