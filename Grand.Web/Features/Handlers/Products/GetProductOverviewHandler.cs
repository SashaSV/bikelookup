using Grand.Core;
using Grand.Domain.Catalog;
using Grand.Domain.Media;
using Grand.Domain.Tax;
using Grand.Services.Catalog;
using Grand.Services.Directory;
using Grand.Services.Helpers;
using Grand.Services.Localization;
using Grand.Services.Media;
using Grand.Services.Security;
using Grand.Services.Seo;
using Grand.Services.Tax;
using Grand.Services.Vendors;
using Grand.Web.Features.Models.Catalog;
using Grand.Web.Features.Models.Products;
using Grand.Web.Models.Catalog;
using Grand.Web.Models.Media;
using MediatR;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Grand.Framework.Controllers;

namespace Grand.Web.Features.Handlers.Products
{
    public class GetProductOverviewHandler : IRequestHandler<GetProductOverview, IEnumerable<ProductOverviewModel>>
    {
        private readonly IPermissionService _permissionService;
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly ILocalizationService _localizationService;
        private readonly IProductService _productService;
        private readonly IPriceCalculationService _priceCalculationService;
        private readonly ITaxService _taxService;
        private readonly ICurrencyService _currencyService;
        private readonly IPriceFormatter _priceFormatter;
        private readonly IPictureService _pictureService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IMediator _mediator;
        private readonly IVendorService _vendorService;
        //private readonly IUrlHelper _baseController;

        private readonly MediaSettings _mediaSettings;
        private readonly CatalogSettings _catalogSettings;

        public GetProductOverviewHandler(
            IPermissionService permissionService,
            IWorkContext workContext,
            IStoreContext storeContext,
            ILocalizationService localizationService,
            IProductService productService,
            IPriceCalculationService priceCalculationService,
            ITaxService taxService,
            ICurrencyService currencyService,
            IPriceFormatter priceFormatter,
            IPictureService pictureService,
            IDateTimeHelper dateTimeHelper,
            IMediator mediator,
            IVendorService vendorService,
            MediaSettings mediaSettings,
            CatalogSettings catalogSettings
            //, IUrlHelper baseController
            )
        {
            _permissionService = permissionService;
            _workContext = workContext;
            _storeContext = storeContext;
            _localizationService = localizationService;
            _productService = productService;
            _priceCalculationService = priceCalculationService;
            _taxService = taxService;
            _currencyService = currencyService;
            _priceFormatter = priceFormatter;
            _pictureService = pictureService;
            _dateTimeHelper = dateTimeHelper;
            _mediator = mediator;
            _mediaSettings = mediaSettings;
            _catalogSettings = catalogSettings;
            _vendorService = vendorService;
            //_baseController = baseController;
        }

        public async Task<IEnumerable<ProductOverviewModel>> Handle(GetProductOverview request, CancellationToken cancellationToken)
        {
            if (request.Products == null)
                throw new ArgumentNullException("products");

            var displayPrices = await _permissionService.Authorize(StandardPermissionProvider.DisplayPrices, _workContext.CurrentCustomer);
            var enableShoppingCart = await _permissionService.Authorize(StandardPermissionProvider.EnableShoppingCart, _workContext.CurrentCustomer);
            var enableWishlist = await _permissionService.Authorize(StandardPermissionProvider.EnableWishlist, _workContext.CurrentCustomer);
            int pictureSize = request.ProductThumbPictureSize.HasValue ? request.ProductThumbPictureSize.Value : _mediaSettings.ProductThumbPictureSize;
            bool priceIncludesTax = _workContext.TaxDisplayType == TaxDisplayType.IncludingTax;

            var res = new Dictionary<string, string>
            {
                { "Products.CallForPrice", _localizationService.GetResource("Products.CallForPrice", _workContext.WorkingLanguage.Id) },
                { "Products.PriceRangeFrom", _localizationService.GetResource("Products.PriceRangeFrom", _workContext.WorkingLanguage.Id)},
                { "Products.PriceRangeFromTo", _localizationService.GetResource("Products.PriceRangeFromTo", _workContext.WorkingLanguage.Id)},
                { "Media.Product.ImageLinkTitleFormat", _localizationService.GetResource("Media.Product.ImageLinkTitleFormat", _workContext.WorkingLanguage.Id) },
                { "Media.Product.ImageAlternateTextFormat", _localizationService.GetResource("Media.Product.ImageAlternateTextFormat", _workContext.WorkingLanguage.Id) }
            };

            var tasks = new List<Task<ProductOverviewModel>>();

            var associatedProducts = await _productService.GetAssociatedProducts(request.Products.Select(p => p.Id));

            foreach (var product in request.Products)
            {
                if (!associatedProducts.TryGetValue(product.Id, out var currentAssociatedProducts))
                {
                    currentAssociatedProducts = new List<Product>();
                }
                tasks.Add(GetProductOverviewModel(product, request, displayPrices, enableShoppingCart, enableWishlist, pictureSize, priceIncludesTax, res, currentAssociatedProducts));
            }
            var result = await Task.WhenAll<ProductOverviewModel>(tasks);
            return result;
        }

        private async Task<ProductOverviewModel> GetProductOverviewModel(Product product, GetProductOverview request,
            bool displayPrices, bool enableShoppingCart, bool enableWishlist, int pictureSize, bool priceIncludesTax, Dictionary<string, string> res,
            IEnumerable<Product> associatedProducts)
        {
            var model = await PrepareProductOverviewModel(product);

            #region Associated products

            if (product.ProductType == ProductType.GroupedProduct)
            {
                var bestDiscount = 0m;
                //if (product.Id == "60b8659a286161a000a41b82")
                //{

                //}

                foreach (var associatedProduct in associatedProducts)
                {
                    var associatedProductOverview =
                        await PrepareProductOverviewModel(associatedProduct, true);
                    associatedProductOverview.ProductPrice = await PreparePriceModel(associatedProduct, res,
                        request.ForceRedirectionAfterAddingToCart,
                        enableShoppingCart, displayPrices, enableWishlist, priceIncludesTax,
                        new List<Product>());

                    if (associatedProductOverview.ProductPrice.OldPriceValue > 0)
                    {
                        bestDiscount = Math.Round(
                            (associatedProductOverview.ProductPrice.OldPriceValue - associatedProductOverview.ProductPrice.PriceValue)
                            / associatedProductOverview.ProductPrice.OldPriceValue, 2);
                    }
                    model.BestDiscount = model.BestDiscount > bestDiscount ? model.BestDiscount : bestDiscount;

                    model.AssociatedProducts.Add(associatedProductOverview);
                }
            }
            #endregion

            //price
            if (request.PreparePriceModel)
            {
                model.ProductPrice = await PreparePriceModel(product, res, request.ForceRedirectionAfterAddingToCart,
                      enableShoppingCart, displayPrices, enableWishlist, priceIncludesTax, associatedProducts);
            }

            //picture
            if (request.PreparePictureModel)
            {
                var pictureModels = await PreparePictureModel(product, model.Name, res, pictureSize);
                model.DefaultPictureModel = pictureModels.FirstOrDefault();
                if (pictureModels.Count > 1) model.SecondPictureModel = pictureModels.ElementAtOrDefault(1);


                model.PictureModels = pictureModels;
            }

            //attributes
            model.ProductAttributeModels = await PrepareAttributesModel(product);

            //reviews
            model.ReviewOverviewModel = await _mediator.Send(new GetProductReviewOverview() { Product = product, Language = _workContext.WorkingLanguage, Store = _storeContext.CurrentStore });

            return model;
        }



        private async Task<VendorBriefInfoModel> PrepareVendorBriefInfoModel(Product product)
        {
            if (!string.IsNullOrEmpty(product.VendorId))
            {
                var vendor = await _vendorService.GetVendorById(product.VendorId);
                if (vendor != null && !vendor.Deleted && vendor.Active)
                {
                    return new VendorBriefInfoModel {
                        Id = vendor.Id,
                        Name = vendor.GetLocalized(x => x.Name, _workContext.WorkingLanguage.Id),
                        SeName = vendor.GetSeName(_workContext.WorkingLanguage.Id),
                    };
                }
            }
            return null;
        }

        private async Task<ProductOverviewModel> PrepareProductOverviewModel(Product product, bool isAssociatedProduct = false)
        {
            //var url = !string.IsNullOrEmpty(product.AdId) ? _baseController.RouteUrl("ViewAd", new { adId = product.AdId }) : product.Url;
            var url = !string.IsNullOrEmpty(product.AdId) ? product.Url : product.Url;

            var model = new ProductOverviewModel {
                Id = product.Id,
                Name = product.GetLocalized(x => x.Name, _workContext.WorkingLanguage.Id),
                ShortDescription = product.GetLocalized(x => x.ShortDescription, _workContext.WorkingLanguage.Id),
                FullDescription = product.GetLocalized(x => x.FullDescription, _workContext.WorkingLanguage.Id),
                Url = url,
                Weeldiam = product.Weeldiam,
                ManufactureName = product.ManufactureName,
                Model = product.Model,
                Year = product.Year,
                Color = product.Color,
                Size = product.Size,
                SeName = product.GetSeName(_workContext.WorkingLanguage.Id),
                ProductType = product.ProductType,
                Sku = product.Sku,
                Gtin = product.Gtin,
                Flag = product.Flag,
                Viewed = product.Viewed,
                UpdatedOnUtc = product.UpdatedOnUtc,
                CreatedOnUtc = product.CreatedOnUtc,
                ManufacturerPartNumber = product.ManufacturerPartNumber,
                IsFreeShipping = product.IsFreeShipping,
                ShowSku = _catalogSettings.ShowSkuOnCatalogPages,
                TaxDisplayType = _workContext.TaxDisplayType,
                EndTime = product.AvailableEndDateTimeUtc,
                EndTimeLocalTime = product.AvailableEndDateTimeUtc.HasValue ? _dateTimeHelper.ConvertToUserTime(product.AvailableEndDateTimeUtc.Value, DateTimeKind.Utc) : new DateTime?(),
                ShowQty = _catalogSettings.DisplayQuantityOnCatalogPages,
                GenericAttributes = product.GenericAttributes,
                VendorId = product.VendorId,
                MarkAsNew = product.MarkAsNew &&
                        (!product.MarkAsNewStartDateTimeUtc.HasValue || product.MarkAsNewStartDateTimeUtc.Value < DateTime.UtcNow) &&
                        (!product.MarkAsNewEndDateTimeUtc.HasValue || product.MarkAsNewEndDateTimeUtc.Value > DateTime.UtcNow),
                AdId = product.AdId,
                IsAd = !string.IsNullOrEmpty(product.AdId)
            };

            //specs
            if (product.ProductSpecificationAttributes.Any())
            {
                model.SpecificationAttributeModels = await _mediator.Send(new GetProductSpecification() { Language = _workContext.WorkingLanguage, Product = product });
                var available = model.SpecificationAttributeModels.FirstOrDefault(a => a.SpecificationAttributeCode == "sp_available");
                model.IsAvailable = available == null ? "" : available.ValueRaw;
            }



            if (isAssociatedProduct)
            {
                model.Vendor = await PrepareVendorBriefInfoModel(product);
            }

            return model;
        }

        private async Task<ProductOverviewModel.ProductPriceModel> PreparePriceModel(Product product, Dictionary<string, string> res,
            bool forceRedirectionAfterAddingToCart, bool enableShoppingCart, bool displayPrices, bool enableWishlist,
            bool priceIncludesTax, IEnumerable<Product> associatedProducts)
        {
            #region Prepare product price

            var priceModel = new ProductOverviewModel.ProductPriceModel {
                ForceRedirectionAfterAddingToCart = forceRedirectionAfterAddingToCart
            };

            switch (product.ProductType)
            {
                case ProductType.GroupedProduct:
                    {
                        #region Grouped product


                        //add to cart button (ignore "DisableBuyButton" property for grouped products)
                        priceModel.DisableBuyButton = !enableShoppingCart || !displayPrices;

                        //add to wishlist button (ignore "DisableWishlistButton" property for grouped products)
                        priceModel.DisableWishlistButton = !enableWishlist || !displayPrices;

                        //compare products
                        priceModel.DisableAddToCompareListButton = !_catalogSettings.CompareProductsEnabled;
                        priceModel.Currency = string.IsNullOrEmpty(_workContext.WorkingCurrency.Name) ? _workContext.WorkingCurrency.CurrencyCode : _workContext.WorkingCurrency.Name;
                        
                        //catalog price, not used in views, but it's for front developer
                        if (product.CatalogPrice > 0)
                        {
                            decimal catalogPrice = await _currencyService.ConvertFromPrimaryStoreCurrency(product.CatalogPrice, _workContext.WorkingCurrency);
                            priceModel.CatalogPrice = _priceFormatter.FormatPrice(catalogPrice, true, _workContext.WorkingCurrency, _workContext.WorkingLanguage, priceIncludesTax);
                        }

                        var enumerable = associatedProducts.ToList();
                        if (enumerable.Any())
                        {
                            //we have at least one associated product
                            //compare products
                            priceModel.DisableAddToCompareListButton = !_catalogSettings.CompareProductsEnabled;
                            if (displayPrices)
                            {
                                //find a minimum possible price
                                decimal? minPossiblePrice = null;
                                Product minPriceProduct = null;
                                var maxPrice = default(decimal);
                                Product maxPriceProduct = null;
                                foreach (var associatedProduct in enumerable)
                                {
                                    //calculate for the maximum quantity (in case if we have tier prices)
                                    var tmpPrice = (await _priceCalculationService.GetFinalPrice(associatedProduct,
                                        _workContext.CurrentCustomer, decimal.Zero, true, int.MaxValue)).finalPrice;
                                    if (!minPossiblePrice.HasValue || tmpPrice < minPossiblePrice.Value)
                                    {
                                        minPriceProduct = associatedProduct;
                                        minPossiblePrice = tmpPrice;
                                    }
                                    if (maxPrice < tmpPrice)
                                    {
                                        maxPrice = tmpPrice;
                                        maxPriceProduct = associatedProduct;
                                    }
                                }
                                if (minPriceProduct != null && !minPriceProduct.CustomerEntersPrice)
                                {
                                    if (minPriceProduct.CallForPrice)
                                    {
                                        priceModel.OldPrice = null;
                                        priceModel.Price = res["Products.CallForPrice"];
                                    }
                                    else if (minPossiblePrice.HasValue)
                                    {
                                        //calculate prices
                                        //decimal finalPriceBase = (await _taxService.GetProductPrice(minPriceProduct, minPossiblePrice.Value, priceIncludesTax, _workContext.CurrentCustomer)).productprice;
                                        //decimal finalPrice = await _currencyService.ConvertFromPrimaryStoreCurrency(finalPriceBase, _workContext.WorkingCurrency);
                                        var finalPrice = await _currencyService.ConvertFromPrimaryStoreCurrency(minPossiblePrice.Value, _workContext.WorkingCurrency);

                                        //var maxPriceBase = (await _taxService.GetProductPrice(maxPriceProduct, maxPrice, priceIncludesTax, _workContext.CurrentCustomer)).productprice;
                                        //var finalMaxPrice = await _currencyService.ConvertFromPrimaryStoreCurrency(maxPriceBase, _workContext.WorkingCurrency);
                                        var finalMaxPrice = await _currencyService.ConvertFromPrimaryStoreCurrency(maxPrice, _workContext.WorkingCurrency); 

                                        priceModel.OldPrice = null;

                                        var diffPrice = finalPrice != finalMaxPrice;

                                        if (diffPrice)
                                        {
                                            priceModel.Price = String.Format(res["Products.PriceRangeFromTo"],
                                                      _priceFormatter.FormatPrice(finalPrice, true, _workContext.WorkingCurrency, _workContext.WorkingLanguage, priceIncludesTax),
                                                      _priceFormatter.FormatPrice(finalMaxPrice, true, _workContext.WorkingCurrency, _workContext.WorkingLanguage, priceIncludesTax));
                                            priceModel.PriceMin = _priceFormatter.FormatPrice(finalPrice, false, _workContext.WorkingCurrency, _workContext.WorkingLanguage, priceIncludesTax);
                                            priceModel.PriceMax = _priceFormatter.FormatPrice(finalMaxPrice, false, _workContext.WorkingCurrency, _workContext.WorkingLanguage, priceIncludesTax);
                                        }
                                        else
                                        {
                                            priceModel.Price = String.Format(res["Products.PriceRangeFrom"],
                                                      _priceFormatter.FormatPrice(finalPrice, true, _workContext.WorkingCurrency, _workContext.WorkingLanguage, priceIncludesTax));

                                            priceModel.PriceMin = _priceFormatter.FormatPrice(finalPrice, false, _workContext.WorkingCurrency, _workContext.WorkingLanguage, priceIncludesTax);
                                        }

                                        priceModel.PriceValue = finalPrice;
                                        priceModel.PriceMinValue = finalPrice;
                                        priceModel.PriceMaxValue = finalMaxPrice;
                                        //PAngV baseprice (used in Germany)
                                        if (product.BasepriceEnabled)
                                            priceModel.BasePricePAngV = await _mediator.Send(new GetFormatBasePrice() { Currency = _workContext.WorkingCurrency, Product = product, ProductPrice = finalPrice });
                                    }
                                    else
                                    {
                                        //Actually it's not possible (we presume that minimalPrice always has a value)
                                        //We never should get here
                                        Debug.WriteLine("Cannot calculate minPrice for product #{0}", product.Id);
                                    }
                                }
                            }
                            else
                            {
                                //hide prices
                                priceModel.OldPrice = null;
                                priceModel.Price = null;
                            }
                        }



                        #endregion
                    }
                    break;
                case ProductType.SimpleProduct:
                case ProductType.Reservation:
                default:
                    {
                        #region Simple product

                        //add to cart button
                        priceModel.DisableBuyButton = product.DisableBuyButton || !enableShoppingCart || !displayPrices;

                        //add to wishlist button
                        priceModel.DisableWishlistButton = product.DisableWishlistButton || !enableWishlist || !displayPrices;
                        //compare products
                        priceModel.DisableAddToCompareListButton = !_catalogSettings.CompareProductsEnabled;

                        //pre-order
                        if (product.AvailableForPreOrder)
                        {
                            priceModel.AvailableForPreOrder = !product.PreOrderAvailabilityStartDateTimeUtc.HasValue ||
                                product.PreOrderAvailabilityStartDateTimeUtc.Value >= DateTime.UtcNow;
                            priceModel.PreOrderAvailabilityStartDateTimeUtc = product.PreOrderAvailabilityStartDateTimeUtc;
                        }

                        //catalog price, not used in views, but it's for front developer
                        if (product.CatalogPrice > 0)
                        {
                            decimal catalogPrice = await _currencyService.ConvertFromPrimaryStoreCurrency(product.CatalogPrice, _workContext.WorkingCurrency);
                            priceModel.CatalogPrice = _priceFormatter.FormatPrice(catalogPrice, true, _workContext.WorkingCurrency, _workContext.WorkingLanguage, priceIncludesTax);
                        }

                        //start price for product auction
                        if (product.StartPrice > 0)
                        {
                            decimal startPrice = await _currencyService.ConvertFromPrimaryStoreCurrency(product.StartPrice, _workContext.WorkingCurrency);
                            priceModel.StartPrice = _priceFormatter.FormatPrice(startPrice, true, _workContext.WorkingCurrency, _workContext.WorkingLanguage, priceIncludesTax);
                            priceModel.StartPriceValue = startPrice;
                        }

                        //highest bid for product auction
                        if (product.HighestBid > 0)
                        {
                            decimal highestBid = await _currencyService.ConvertFromPrimaryStoreCurrency(product.HighestBid, _workContext.WorkingCurrency);
                            priceModel.HighestBid = _priceFormatter.FormatPrice(highestBid, true, _workContext.WorkingCurrency, _workContext.WorkingLanguage, priceIncludesTax);
                            priceModel.HighestBidValue = highestBid;
                        }

                        //prices
                        if (displayPrices)
                        {
                            if (!product.CustomerEntersPrice)
                            {
                                if (product.CallForPrice)
                                {
                                    //call for price
                                    priceModel.OldPrice = null;
                                    priceModel.Price = res["Products.CallForPrice"];
                                }
                                else
                                {
                                    //prices

                                    //calculate for the maximum quantity (in case if we have tier prices)
                                    var infoprice = (await _priceCalculationService.GetFinalPrice(product,
                                        _workContext.CurrentCustomer, decimal.Zero, true, int.MaxValue));

                                    priceModel.AppliedDiscounts = infoprice.appliedDiscounts;
                                    priceModel.PreferredTierPrice = infoprice.preferredTierPrice;

                                    decimal minPossiblePrice = infoprice.finalPrice;

                                    decimal oldPriceBase = (await _taxService.GetProductPrice(product, product.OldPrice, priceIncludesTax, _workContext.CurrentCustomer)).productprice;
                                    decimal finalPriceBase = (await _taxService.GetProductPrice(product, minPossiblePrice, priceIncludesTax, _workContext.CurrentCustomer)).productprice;

                                    decimal oldPrice = await _currencyService.ConvertFromPrimaryStoreCurrency(oldPriceBase, _workContext.WorkingCurrency);
                                    decimal finalPrice = await _currencyService.ConvertFromPrimaryStoreCurrency(finalPriceBase, _workContext.WorkingCurrency);

                                    //do we have tier prices configured?
                                    var tierPrices = new List<TierPrice>();
                                    if (product.TierPrices.Any())
                                    {
                                        tierPrices.AddRange(product.TierPrices.OrderBy(tp => tp.Quantity)
                                            .FilterByStore(_storeContext.CurrentStore.Id)
                                            .FilterForCustomer(_workContext.CurrentCustomer)
                                            .FilterByDate()
                                            .RemoveDuplicatedQuantities());
                                    }
                                    //When there is just one tier (with  qty 1), 
                                    //there are no actual savings in the list.
                                    bool displayFromMessage = tierPrices.Any() && !(tierPrices.Count == 1 && tierPrices[0].Quantity <= 1);
                                    if (displayFromMessage)
                                    {
                                        priceModel.OldPrice = null;
                                        priceModel.Price = String.Format(res["Products.PriceRangeFrom"], _priceFormatter.FormatPrice(finalPrice, true, _workContext.WorkingCurrency, _workContext.WorkingLanguage, priceIncludesTax));
                                        priceModel.PriceValue = finalPrice;
                                    }
                                    else
                                    {
                                        if (finalPriceBase != oldPriceBase && oldPriceBase != decimal.Zero)
                                        {
                                            priceModel.OldPrice = _priceFormatter.FormatPrice(oldPrice, true, _workContext.WorkingCurrency, _workContext.WorkingLanguage, priceIncludesTax);
                                            priceModel.OldPriceValue = oldPrice;
                                            priceModel.Price = _priceFormatter.FormatPrice(finalPrice, true, _workContext.WorkingCurrency, _workContext.WorkingLanguage, priceIncludesTax);
                                            priceModel.PriceValue = finalPrice;
                                        }
                                        else
                                        {
                                            priceModel.OldPrice = null;
                                            priceModel.Price = _priceFormatter.FormatPrice(finalPrice, true, _workContext.WorkingCurrency, _workContext.WorkingLanguage, priceIncludesTax);
                                            priceModel.PriceValue = finalPrice;
                                        }
                                    }
                                    if (product.ProductType == ProductType.Reservation)
                                    {
                                        //rental product
                                        priceModel.OldPrice = _priceFormatter.FormatReservationProductPeriod(product, priceModel.OldPrice);
                                        priceModel.Price = _priceFormatter.FormatReservationProductPeriod(product, priceModel.Price);
                                    }


                                    //property for German market
                                    //we display tax/shipping info only with "shipping enabled" for this product
                                    //we also ensure this it's not free shipping
                                    priceModel.DisplayTaxShippingInfo = _catalogSettings.DisplayTaxShippingInfoProductBoxes && product.IsShipEnabled && !product.IsFreeShipping;

                                    //PAngV baseprice (used in Germany)
                                    if (product.BasepriceEnabled)
                                        priceModel.BasePricePAngV = await _mediator.Send(new GetFormatBasePrice() { Currency = _workContext.WorkingCurrency, Product = product, ProductPrice = finalPrice });

                                }
                            }
                        }
                        else
                        {
                            //hide prices
                            priceModel.OldPrice = null;
                            priceModel.Price = null;
                        }

                        #endregion
                    }
                    break;
            }

            return priceModel;

            #endregion
        }

        private async Task<IList<PictureModel>> PreparePictureModel(Product product, string name, Dictionary<string, string> res, int pictureSize)
        {
            #region Prepare product picture
            var result = new List<PictureModel>();
            async Task<PictureModel> PreparePictureModel(ProductPicture picture)
            {
                if (picture == null)
                    picture = new ProductPicture();

                var pictureModel = new PictureModel {
                    Id = picture.PictureId,
                    ImageUrl = await _pictureService.GetPictureUrl(picture.PictureId, pictureSize),
                    FullSizeImageUrl = await _pictureService.GetPictureUrl(picture.PictureId)
                };
                //"title" attribute
                pictureModel.Title = (picture != null && !string.IsNullOrEmpty(picture.TitleAttribute)) ?
                    picture.TitleAttribute :
                    string.Format(res["Media.Product.ImageLinkTitleFormat"], name);
                //"alt" attribute
                pictureModel.AlternateText = (picture != null && !string.IsNullOrEmpty(picture.AltAttribute)) ?
                    picture.AltAttribute :
                    string.Format(res["Media.Product.ImageAlternateTextFormat"], name);

                return pictureModel;
            };

            if (product.ProductPictures.Any())
            {
                //prepare picture model
                var picturesTasks = product.ProductPictures.Select(PreparePictureModel).ToList();
                await Task.WhenAll(picturesTasks);
                result.AddRange(picturesTasks.Select(t => t.Result));
            }
            else
            {
                result.Add(await PreparePictureModel(new ProductPicture()));
            }

            //prepare second picture model
            if (_catalogSettings.SecondPictureOnCatalogPages)
            {
                var secondPicture = product.ProductPictures.OrderBy(x => x.DisplayOrder).Skip(1).Take(1).FirstOrDefault();
                if (secondPicture != null)
                    result.Add(await PreparePictureModel(secondPicture));
            }

            return result;
            #endregion

        }

        private async Task<IList<ProductOverviewModel.ProductAttributeModel>> PrepareAttributesModel(Product product)
        {
            var result = new List<ProductOverviewModel.ProductAttributeModel>();
            if (product.ProductAttributeMappings.Any(x => x.ShowOnCatalogPage))
            {
                foreach (var attribute in product.ProductAttributeMappings.Where(x => x.ShowOnCatalogPage).OrderBy(x => x.DisplayOrder))
                {
                    var pa = await _mediator.Send(new GetProductAttribute() { Id = attribute.ProductAttributeId });
                    if (pa != null)
                    {
                        var productAttributeModel = new ProductOverviewModel.ProductAttributeModel();
                        productAttributeModel.Name = pa.GetLocalized(x => x.Name, _workContext.WorkingLanguage.Id);
                        productAttributeModel.SeName = pa.SeName;
                        productAttributeModel.TextPrompt = attribute.TextPrompt;
                        productAttributeModel.IsRequired = attribute.IsRequired;
                        productAttributeModel.AttributeControlType = attribute.AttributeControlType;
                        productAttributeModel.GenericAttributes = pa.GenericAttributes;
                        
                        foreach (var item in attribute.ProductAttributeValues)
                        {
                            var value = new ProductOverviewModel.ProductAttributeValueModel();
                            value.Name = item.Name;
                            value.ColorSquaresRgb = item.ColorSquaresRgb;
                            //"image square" picture (with with "image squares" attribute type only)
                            if (!string.IsNullOrEmpty(item.ImageSquaresPictureId))
                            {
                                var pm = new PictureModel();
                                pm = new PictureModel {
                                    Id = item.ImageSquaresPictureId,
                                    FullSizeImageUrl = await _pictureService.GetPictureUrl(item.ImageSquaresPictureId),
                                    ImageUrl = await _pictureService.GetPictureUrl(item.ImageSquaresPictureId, _mediaSettings.ImageSquarePictureSize)
                                };
                                value.ImageSquaresPictureModel = pm;
                            }

                            //picture of a product attribute value
                            if (!string.IsNullOrEmpty(item.PictureId))
                            {
                                var pm = new PictureModel();
                                pm = new PictureModel {
                                    Id = item.PictureId,
                                    FullSizeImageUrl = await _pictureService.GetPictureUrl(item.PictureId),
                                    ImageUrl = await _pictureService.GetPictureUrl(item.PictureId, 50)
                                };
                                value.PictureModel = pm;
                            }
                            productAttributeModel.Values.Add(value);
                        }
                        result.Add(productAttributeModel);
                    }
                }
            }
            return result;
        }

    }

}

