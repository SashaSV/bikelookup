using Grand.Core;
using Grand.Domain.Catalog;
using Grand.Domain.Directory;
using Grand.Domain.Media;
using Grand.Domain.Messages;
using Grand.Domain.Seo;
using Grand.Domain.Shipping;
using Grand.Domain.Tax;
using Grand.Domain.Vendors;
using Grand.Services.Catalog;
using Grand.Services.Directory;
using Grand.Services.ExportImport.Help;
using Grand.Services.Localization;
using Grand.Services.Media;
using Grand.Services.Messages;
using Grand.Services.Seo;
using Grand.Services.Shipping;
using Grand.Services.Tax;
using Grand.Services.Vendors;
using Microsoft.AspNetCore.StaticFiles;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Services.ExportImport
{
    /// <summary>
    /// Import manager
    /// </summary>
    public partial class ImportManager : IImportManager
    {
        #region Fields

        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;
        private readonly ISpecificationAttributeService _specificationAttributeService;
        private readonly IManufacturerService _manufacturerService;
        private readonly IPictureService _pictureService;
        private readonly IUrlRecordService _urlRecordService;
        private readonly IStoreContext _storeContext;
        private readonly INewsLetterSubscriptionService _newsLetterSubscriptionService;
        private readonly INewsletterCategoryService _newsletterCategoryService;
        private readonly ICountryService _countryService;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly IVendorService _vendorService;
        private readonly ICategoryTemplateService _categoryTemplateService;
        private readonly IManufacturerTemplateService _manufacturerTemplateService;
        private readonly IProductTemplateService _productTemplateService;
        private readonly IDownloadService _downloadService;
        private readonly IShippingService _shippingService;
        private readonly ITaxCategoryService _taxService;
        private readonly IMeasureService _measureService;
        private readonly ILanguageService _languageService;
        private readonly SeoSettings _seoSetting;

        #endregion

        #region Ctor

        public ImportManager(IProductService productService,
            ICategoryService categoryService,
            IManufacturerService manufacturerService,
            ISpecificationAttributeService specificationAttributeService,
            IPictureService pictureService,
            IUrlRecordService urlRecordService,
            IStoreContext storeContext,
            INewsLetterSubscriptionService newsLetterSubscriptionService,
            INewsletterCategoryService newsletterCategoryService,
            ICountryService countryService,
            IStateProvinceService stateProvinceService,
            IVendorService vendorService,
            ICategoryTemplateService categoryTemplateService,
            IManufacturerTemplateService manufacturerTemplateService,
            IProductTemplateService productTemplateService,
            IDownloadService downloadService,
            IShippingService shippingService,
            ITaxCategoryService taxService,
            IMeasureService measureService,
            ILanguageService languageService,
            SeoSettings seoSetting)
        {
            _productService = productService;
            _categoryService = categoryService;
            _manufacturerService = manufacturerService;
            _specificationAttributeService = specificationAttributeService;
            _pictureService = pictureService;
            _urlRecordService = urlRecordService;
            _storeContext = storeContext;
            _newsLetterSubscriptionService = newsLetterSubscriptionService;
            _newsletterCategoryService = newsletterCategoryService;
            _countryService = countryService;
            _stateProvinceService = stateProvinceService;
            _vendorService = vendorService;
            _categoryTemplateService = categoryTemplateService;
            _manufacturerTemplateService = manufacturerTemplateService;
            _productTemplateService = productTemplateService;
            _downloadService = downloadService;
            _shippingService = shippingService;
            _taxService = taxService;
            _measureService = measureService;
            _languageService = languageService;
            _seoSetting = seoSetting;
        }

        #endregion

        #region Utilities

        protected virtual PropertyManager<T> GetPropertyManager<T>(NPOI.SS.UserModel.ISheet worksheet)
        {
            var properties = new List<PropertyByName<T>>();
            var poz = 0;
            while (true)
            {
                try
                {
                    var cell = worksheet.GetRow(0).Cells[poz];

                    if (cell == null || string.IsNullOrEmpty(cell.StringCellValue))
                        break;

                    poz += 1;
                    properties.Add(new PropertyByName<T>(cell.StringCellValue.ToLower()));
                }
                catch
                {
                    break;
                }
            }
            return new PropertyManager<T>(properties.ToArray());
        }

        protected virtual void PrepareProductMapping(Product product, PropertyManager<Product> manager,
            IList<ProductTemplate> templates,
            IList<DeliveryDate> deliveryDates,
            IList<Warehouse> warehouses,
            IList<MeasureUnit> units,
            IList<TaxCategory> taxes,
            Vendor vendor)
        {

            foreach (var property in manager.GetProperties)
            {
                switch (property.PropertyName.ToLower())
                {
                    case "producttypeid":
                        product.ProductTypeId = property.IntValue;
                        break;
                    case "parentgroupedproductid":
                        var parentgroupedproductid = property.StringValue;
                        if (_productService.GetProductById(parentgroupedproductid) != null)
                            product.ParentGroupedProductId = property.StringValue;
                        break;
                    case "visibleindividually":
                        product.VisibleIndividually = property.BooleanValue;
                        break;
                    case "name":
                        product.Name = property.StringValue;
                        break;
                    case "shortdescription":
                        product.ShortDescription = property.StringValue;
                        break;
                    case "url":
                        product.Url = property.StringValue;
                        break;
                    case "Weeldiam":
                        product.Weeldiam = property.StringValue;
                        break;
                    case "manufacturename":
                        product.ManufactureName = property.StringValue;
                        break;
                    case "model":
                        product.Model = property.StringValue;
                        break;
                    case "year":
                        product.Year = property.StringValue;
                        break;
                    case "color":
                        product.Color = property.StringValue;
                        break;
                    case "size":
                        product.Size = property.StringValue;
                        break;
                    case "fulldescription":
                        product.FullDescription = property.StringValue;
                        break;
                    case "vendorid":
                        var vendorid = property.StringValue;
                        if (_vendorService.GetVendorById(vendorid) != null)
                            product.VendorId = property.StringValue;
                        break;
                    case "vendor":
                        if (vendor != null)
                            product.VendorId = vendor.Id;
                        break;
                    case "producttemplateid":
                        var templateid = property.StringValue;
                        if (templates.FirstOrDefault(x => x.Id == templateid) != null)
                            product.ProductTemplateId = property.StringValue;
                        break;
                    case "showonhomepage":
                        product.ShowOnHomePage = property.BooleanValue;
                        break;
                    case "metakeywords":
                        product.MetaKeywords = property.StringValue;
                        break;
                    case "metadescription":
                        product.MetaDescription = property.StringValue;
                        break;
                    case "metatitle":
                        product.MetaTitle = property.StringValue;
                        break;
                    case "allowcustomerreviews":
                        product.AllowCustomerReviews = property.BooleanValue;
                        break;
                    case "published":
                        product.Published = property.BooleanValue;
                        break;
                    case "sku":
                        product.Sku = property.StringValue;
                        break;
                    case "manufacturerpartnumber":
                        product.ManufacturerPartNumber = property.StringValue;
                        break;
                    case "gtin":
                        product.Gtin = property.StringValue;
                        break;
                    case "isgiftcard":
                        product.IsGiftCard = property.BooleanValue;
                        break;
                    case "giftcardtypeid":
                        product.GiftCardTypeId = property.IntValue;
                        break;
                    case "overriddengiftcardamount":
                        product.OverriddenGiftCardAmount = property.DecimalValue;
                        break;
                    case "requireotherproducts":
                        product.RequireOtherProducts = property.BooleanValue;
                        break;
                    case "requiredproductids":
                        product.RequiredProductIds = property.StringValue;
                        break;
                    case "automaticallyaddrequiredproducts":
                        product.AutomaticallyAddRequiredProducts = property.BooleanValue;
                        break;
                    case "isdownload":
                        product.IsDownload = property.BooleanValue;
                        break;
                    case "downloadid":
                        var downloadid = property.StringValue;
                        if (_downloadService.GetDownloadById(downloadid) != null)
                            product.DownloadId = downloadid;
                        break;
                    case "unlimiteddownloads":
                        product.UnlimitedDownloads = property.BooleanValue;
                        break;
                    case "maxnumberofdownloads":
                        product.MaxNumberOfDownloads = property.IntValue;
                        break;
                    case "downloadactivationtypeid":
                        product.DownloadActivationTypeId = property.IntValue;
                        break;
                    case "hassampledownload":
                        product.HasSampleDownload = property.BooleanValue;
                        break;
                    case "sampledownloadid":
                        var sampledownloadid = property.StringValue;
                        if (_downloadService.GetDownloadById(sampledownloadid) != null)
                            product.SampleDownloadId = property.StringValue;
                        break;
                    case "hasuseragreement":
                        product.HasUserAgreement = property.BooleanValue;
                        break;
                    case "useragreementtext":
                        product.UserAgreementText = property.StringValue;
                        break;
                    case "isrecurring":
                        product.IsRecurring = property.BooleanValue;
                        break;
                    case "recurringcyclelength":
                        product.RecurringCycleLength = property.IntValue;
                        break;
                    case "recurringcycleperiodid":
                        product.RecurringCyclePeriodId = property.IntValue;
                        break;
                    case "recurringtotalcycles":
                        product.RecurringTotalCycles = property.IntValue;
                        break;
                    case "interval":
                        product.Interval = property.IntValue;
                        break;
                    case "intervalunitId":
                        product.IntervalUnitId = property.IntValue;
                        break;
                    case "isshipenabled":
                        product.IsShipEnabled = property.BooleanValue;
                        break;
                    case "isfreeshipping":
                        product.IsFreeShipping = property.BooleanValue;
                        break;
                    case "shipseparately":
                        product.ShipSeparately = property.BooleanValue;
                        break;
                    case "additionalshippingcharge":
                        product.AdditionalShippingCharge = property.DecimalValue;
                        break;
                    case "deliverydateId":
                        var deliverydateid = property.StringValue;
                        if (deliveryDates.FirstOrDefault(x => x.Id == deliverydateid) != null)
                            product.DeliveryDateId = deliverydateid;
                        break;
                    case "istaxexempt":
                        product.IsTaxExempt = property.BooleanValue;
                        break;
                    case "taxcategoryid":
                        var taxcategoryid = property.StringValue;
                        if (taxes.FirstOrDefault(x => x.Id == taxcategoryid) != null)
                            product.TaxCategoryId = property.StringValue;
                        break;
                    case "istele":
                        product.IsTele = property.BooleanValue;
                        break;
                    case "manageinventorymethodid":
                        product.ManageInventoryMethodId = property.IntValue;
                        break;
                    case "usemultiplewarehouses":
                        product.UseMultipleWarehouses = property.BooleanValue;
                        break;
                    case "warehouseid":
                        var warehouseid = property.StringValue;
                        if (warehouses.FirstOrDefault(x => x.Id == warehouseid) != null)
                            product.WarehouseId = property.StringValue;
                        break;
                    case "stockquantity":
                        product.StockQuantity = property.IntValue;
                        break;
                    case "displaystockavailability":
                        product.DisplayStockAvailability = property.BooleanValue;
                        break;
                    case "displaystockquantity":
                        product.DisplayStockQuantity = property.BooleanValue;
                        break;
                    case "minstockquantity":
                        product.MinStockQuantity = property.IntValue;
                        break;
                    case "lowstockactivityid":
                        product.LowStockActivityId = property.IntValue;
                        break;
                    case "notifyadminforquantitybelow":
                        product.NotifyAdminForQuantityBelow = property.IntValue;
                        break;
                    case "admincomment":
                        product.AdminComment = property.StringValue;
                        break;
                    case "flag":
                        product.Flag = property.StringValue;
                        break;
                    case "backordermodeid":
                        product.BackorderModeId = property.IntValue;
                        break;
                    case "allowbackinstocksubscriptions":
                        product.AllowBackInStockSubscriptions = property.BooleanValue;
                        break;
                    case "orderminimumquantity":
                        product.OrderMinimumQuantity = property.IntValue;
                        break;
                    case "ordermaximumquantity":
                        product.OrderMaximumQuantity = property.IntValue;
                        break;
                    case "allowedquantities":
                        product.AllowedQuantities = property.StringValue;
                        break;
                    case "allowaddingonlyexistingattributecombinations":
                        product.AllowAddingOnlyExistingAttributeCombinations = property.BooleanValue;
                        break;
                    case "disablebuybutton":
                        product.DisableBuyButton = property.BooleanValue;
                        break;
                    case "disablewishlistbutton":
                        product.DisableWishlistButton = property.BooleanValue;
                        break;
                    case "availableforpreorder":
                        product.AvailableForPreOrder = property.BooleanValue;
                        break;
                    case "preorderavailabilitystartdatetimeutc":
                        product.PreOrderAvailabilityStartDateTimeUtc = property.DateTimeNullable;
                        break;
                    case "callforprice":
                        product.CallForPrice = property.BooleanValue;
                        break;
                    case "price":
                        product.Price = property.DecimalValue;
                        break;
                    case "oldprice":
                        product.OldPrice = property.DecimalValue;
                        break;
                    case "catalogprice":
                        product.CatalogPrice = property.DecimalValue;
                        break;
                    case "startprice":
                        product.StartPrice = property.DecimalValue;
                        break;
                    case "productcost":
                        product.ProductCost = property.DecimalValue;
                        break;
                    case "customerentersprice":
                        product.CustomerEntersPrice = property.BooleanValue;
                        break;
                    case "minimumcustomerenteredprice":
                        product.MinimumCustomerEnteredPrice = property.DecimalValue;
                        break;
                    case "maximumcustomerenteredprice":
                        product.MaximumCustomerEnteredPrice = property.DecimalValue;
                        break;
                    case "basepriceenabled":
                        product.BasepriceEnabled = property.BooleanValue;
                        break;
                    case "basepriceamount":
                        product.BasepriceAmount = property.DecimalValue;
                        break;
                    case "basepriceunitid":
                        product.BasepriceUnitId = property.StringValue;
                        break;
                    case "basepricebaseamount":
                        product.BasepriceBaseAmount = property.DecimalValue;
                        break;
                    case "basepricebaseunitid":
                        product.BasepriceBaseUnitId = property.StringValue;
                        break;
                    case "markasnew":
                        product.MarkAsNew = property.BooleanValue;
                        break;
                    case "markasnewstartdatetimeutc":
                        product.MarkAsNewStartDateTimeUtc = property.DateTimeNullable;
                        break;
                    case "markasnewenddatetimeutc":
                        product.MarkAsNewEndDateTimeUtc = property.DateTimeNullable;
                        break;
                    case "unitid":
                        var unitid = property.StringValue;
                        if (units.FirstOrDefault(x => x.Id == unitid) != null)
                            product.UnitId = property.StringValue;
                        break;
                    case "weight":
                        product.Weight = property.DecimalValue;
                        break;
                    case "length":
                        product.Length = property.DecimalValue;
                        break;
                    case "width":
                        product.Width = property.DecimalValue;
                        break;
                    case "height":
                        product.Height = property.DecimalValue;
                        break;
                }
            }
        }

        protected virtual async Task PrepareProductCategories(Product product, string categoryIds)
        {
            foreach (var id in categoryIds.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()))
            {
                if (product.ProductCategories.FirstOrDefault(x => x.CategoryId == id) == null)
                {
                    //ensure that category exists
                    var category = await _categoryService.GetCategoryById(id);
                    if (category != null)
                    {
                        var productCategory = new ProductCategory {
                            ProductId = product.Id,
                            CategoryId = category.Id,
                            IsFeaturedProduct = false,
                            DisplayOrder = 1
                        };
                        await _categoryService.InsertProductCategory(productCategory);
                    }
                }
            }
        }

        protected virtual async Task PrepareProductCategoriesByName(Product product, string categoryName, IList<CategoryTemplate> templatesCategory)
        {
            if (string.IsNullOrWhiteSpace(categoryName))
                return;

            var categorys = await _categoryService.GetAllCategories(categoryName);
            var category = categorys.FirstOrDefault();

            if (category == null)
            {
                //add new category
                category ??= new Category();
                category.Name = categoryName;
                category.CreatedOnUtc = DateTime.UtcNow;
                category.CategoryTemplateId = templatesCategory.FirstOrDefault()?.Id;

                category.UpdatedOnUtc = DateTime.UtcNow;
                category.ShowOnHomePage = false;
                category.FeaturedProductsOnHomaPage = false;
                category.IncludeInTopMenu = true;
                category.ShowOnSearchBox = false;
                category.AllowCustomersToSelectPageSize = false;
                category.PageSize = 20;
                //category.DefaultSort = 
                category.Published = true;
                category.HideOnCatalog = true;

                var sename = category.Name;
                sename = await category.ValidateSeName(sename, category.Name, true, _seoSetting, _urlRecordService, _languageService);
                category.SeName = sename;

                await _categoryService.InsertCategory(category);

                await _urlRecordService.SaveSlug(category, sename, "");

            }
            await PrepareProductCategories(product, category.Id);
        }

        protected virtual async Task PrepareProductManufacturersByName(Product product, string manufacturerName, IList<ManufacturerTemplate> templatesManufacturer)
        {
            if (string.IsNullOrEmpty(manufacturerName))
                return;
            var manufacturers = await _manufacturerService.GetAllManufacturers(manufacturerName);
            var manufacturer = manufacturers.FirstOrDefault();
            if (manufacturer == null)
            {
                manufacturer ??= new Manufacturer();
                manufacturer.Name = manufacturerName;

                manufacturer.CreatedOnUtc = DateTime.UtcNow;
                manufacturer.ManufacturerTemplateId = templatesManufacturer.FirstOrDefault()?.Id;
                manufacturer.UpdatedOnUtc = DateTime.UtcNow;
                manufacturer.ShowOnHomePage = false;
                manufacturer.FeaturedProductsOnHomaPage = false;
                manufacturer.IncludeInTopMenu = false;
                manufacturer.AllowCustomersToSelectPageSize = false;
                manufacturer.PageSize = 20;
                manufacturer.Published = true;

                var sename = manufacturer.Name;
                sename = await manufacturer.ValidateSeName(sename, manufacturer.Name, true, _seoSetting, _urlRecordService, _languageService);
                manufacturer.SeName = sename;

                await _manufacturerService.InsertManufacturer(manufacturer);

                await _urlRecordService.SaveSlug(manufacturer, manufacturer.SeName, "");

            }
            await PrepareProductManufacturers(product, manufacturer.Id);
        }

        protected virtual async Task PrepareProductManufacturers(Product product, string manufacturerIds)
        {
            foreach (var id in manufacturerIds.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()))
            {
                if (product.ProductManufacturers.FirstOrDefault(x => x.ManufacturerId == id) == null)
                {
                    //ensure that manufacturer exists
                    var manufacturer = await _manufacturerService.GetManufacturerById(id);
                    if (manufacturer != null)
                    {
                        var productManufacturer = new ProductManufacturer {
                            ProductId = product.Id,
                            ManufacturerId = manufacturer.Id,
                            IsFeaturedProduct = false,
                            DisplayOrder = 1
                        };
                        await _manufacturerService.InsertProductManufacturer(productManufacturer);
                    }
                }
            }
        }

        protected virtual async Task PrepareProductPictures(Product product, PropertyManager<Product> manager, bool isNew)
        {
            /*
            var picture1 = manager.GetProperty("picture1") != null ? manager.GetProperty("picture1").StringValue : string.Empty;
            var picture2 = manager.GetProperty("picture2") != null ? manager.GetProperty("picture2").StringValue : string.Empty;
            var picture3 = manager.GetProperty("picture3") != null ? manager.GetProperty("picture3").StringValue : string.Empty;
            */

            var pictures = new List<string>();

            foreach (var property in manager.GetProperties)
            {

                if (property.PropertyName.ToLower().Substring(0, property.PropertyName.Length - 1) == "picture")
                {
                    if (!string.IsNullOrEmpty(property.StringValue))
                    {
                        pictures.Add(property.StringValue);
                    }
                }
            }


            foreach (var picturePath in pictures)
            {
                if (String.IsNullOrEmpty(picturePath))
                    continue;
                if (!picturePath.ToLower().StartsWith(("http".ToLower())))
                {
                    var mimeType = GetMimeTypeFromFilePath(picturePath);
                    var newPictureBinary = File.ReadAllBytes(picturePath);
                    var pictureAlreadyExists = false;
                    if (!isNew)
                    {
                        //compare with existing product pictures
                        var existingPictures = product.ProductPictures;
                        foreach (var existingPicture in existingPictures)
                        {
                            var pp = await _pictureService.GetPictureById(existingPicture.PictureId);
                            var existingBinary = await _pictureService.LoadPictureBinary(pp);
                            //picture binary after validation (like in database)
                            var validatedPictureBinary = _pictureService.ValidatePicture(newPictureBinary, mimeType);
                            if (existingBinary.SequenceEqual(validatedPictureBinary) || existingBinary.SequenceEqual(newPictureBinary))
                            {
                                //the same picture content
                                pictureAlreadyExists = true;
                                break;
                            }
                        }
                    }

                    if (!pictureAlreadyExists)
                    {
                        var picture = await _pictureService.InsertPicture(newPictureBinary, mimeType, _pictureService.GetPictureSeName(product.Name));
                        var productPicture = new ProductPicture {
                            PictureId = picture.Id,
                            ProductId = product.Id,
                            DisplayOrder = 1,
                        };
                        await _productService.InsertProductPicture(productPicture);
                    }
                }
                else
                {
                    byte[] fileBinary = await DownloadUrl.DownloadFile(picturePath);
                    if (fileBinary != null)
                    {
                        var mimeType = GetMimeTypeFromFilePath(picturePath);
                        var picture = await _pictureService.InsertPicture(fileBinary, mimeType, _pictureService.GetPictureSeName(product.Name));
                        var productPicture = new ProductPicture {
                            PictureId = picture.Id,
                            ProductId = product.Id,
                            DisplayOrder = 1,
                        };
                        await _productService.InsertProductPicture(productPicture);
                    }
                }
            }

        }

        protected virtual void PrepareCategoryMapping(Category category, PropertyManager<Category> manager, IList<CategoryTemplate> templates)
        {
            foreach (var property in manager.GetProperties)
            {
                switch (property.PropertyName.ToLower())
                {
                    case "name":
                        category.Name = property.StringValue;
                        break;
                    case "description":
                        category.Description = property.StringValue;
                        break;
                    case "categorytemplateid":
                        var categorytemplateid = property.StringValue;
                        if (templates.FirstOrDefault(x => x.Id == categorytemplateid) != null)
                            category.CategoryTemplateId = property.StringValue;
                        break;
                    case "metakeywords":
                        category.MetaKeywords = property.StringValue;
                        break;
                    case "metadescription":
                        category.MetaDescription = property.StringValue;
                        break;
                    case "metatitle":
                        category.MetaTitle = property.StringValue;
                        break;
                    case "pagesize":
                        category.PageSize = property.IntValue > 0 ? property.IntValue : 10;
                        break;
                    case "allowcustomerstoselectpageSize":
                        category.AllowCustomersToSelectPageSize = property.BooleanValue;
                        break;
                    case "pagesizeoptions":
                        category.PageSizeOptions = property.StringValue;
                        break;
                    case "priceranges":
                        category.PriceRanges = property.StringValue;
                        break;
                    case "published":
                        category.Published = property.BooleanValue;
                        break;
                    case "displayorder":
                        category.DisplayOrder = property.IntValue;
                        break;
                    case "showonhomepage":
                        category.ShowOnHomePage = property.BooleanValue;
                        break;
                    case "includeintopmenu":
                        category.IncludeInTopMenu = property.BooleanValue;
                        break;
                    case "showonsearchbox":
                        category.ShowOnSearchBox = property.BooleanValue;
                        break;
                    case "searchboxdisplayorder":
                        category.SearchBoxDisplayOrder = property.IntValue;
                        break;
                    case "flag":
                        category.Flag = property.StringValue;
                        break;
                    case "flagstyle":
                        category.FlagStyle = property.StringValue;
                        break;
                    case "icon":
                        category.Icon = property.StringValue;
                        break;
                    case "parentcategoryid":
                        if (!string.IsNullOrEmpty(property.StringValue) && property.StringValue != "0")
                            category.ParentCategoryId = property.StringValue;
                        break;
                }
            }
        }

        protected virtual void PrepareManufacturerMapping(Manufacturer manufacturer, PropertyManager<Manufacturer> manager, IList<ManufacturerTemplate> templates)
        {
            foreach (var property in manager.GetProperties)
            {
                switch (property.PropertyName.ToLower())
                {
                    case "name":
                        manufacturer.Name = property.StringValue;
                        break;
                    case "description":
                        manufacturer.Description = property.StringValue;
                        break;
                    case "manufacturertemplateid":
                        var manufacturerTemplateId = property.StringValue;
                        if (templates.FirstOrDefault(x => x.Id == manufacturerTemplateId) != null)
                            manufacturer.ManufacturerTemplateId = property.StringValue;
                        break;
                    case "metakeywords":
                        manufacturer.MetaKeywords = property.StringValue;
                        break;
                    case "metadescription":
                        manufacturer.MetaDescription = property.StringValue;
                        break;
                    case "metatitle":
                        manufacturer.MetaTitle = property.StringValue;
                        break;
                    case "pagesize":
                        manufacturer.PageSize = property.IntValue > 0 ? property.IntValue : 10;
                        break;
                    case "allowcustomerstoselectpageSize":
                        manufacturer.AllowCustomersToSelectPageSize = property.BooleanValue;
                        break;
                    case "pagesizeoptions":
                        manufacturer.PageSizeOptions = property.StringValue;
                        break;
                    case "priceranges":
                        manufacturer.PriceRanges = property.StringValue;
                        break;
                    case "published":
                        manufacturer.Published = property.BooleanValue;
                        break;
                    case "showonhomepage":
                        manufacturer.ShowOnHomePage = property.BooleanValue;
                        break;
                    case "includeintopmenu":
                        manufacturer.IncludeInTopMenu = property.BooleanValue;
                        break;
                    case "displayorder":
                        manufacturer.DisplayOrder = property.IntValue;
                        break;
                }
            }
        }



        protected virtual async Task ImportSubscription(string email, string storeId, bool isActive, bool iscategories, List<string> categories)
        {
            var subscription = await _newsLetterSubscriptionService.GetNewsLetterSubscriptionByEmailAndStoreId(email, storeId);
            if (subscription != null)
            {
                subscription.Email = email;
                subscription.Active = isActive;
                if (iscategories)
                {
                    subscription.Categories.Clear();
                    foreach (var item in categories)
                    {
                        subscription.Categories.Add(item);
                    }
                }
                await _newsLetterSubscriptionService.UpdateNewsLetterSubscription(subscription);
            }
            else
            {
                subscription = new NewsLetterSubscription {
                    Active = isActive,
                    CreatedOnUtc = DateTime.UtcNow,
                    Email = email,
                    StoreId = storeId,
                    NewsLetterSubscriptionGuid = Guid.NewGuid()
                };
                foreach (var item in categories)
                {
                    subscription.Categories.Add(item);
                }
                await _newsLetterSubscriptionService.InsertNewsLetterSubscription(subscription);
            }
        }
        protected virtual string GetMimeTypeFromFilePath(string filePath)
        {
            new FileExtensionContentTypeProvider().TryGetContentType(filePath, out string mimeType);
            //set to jpeg in case mime type cannot be found
            if (mimeType == null)
                mimeType = "image/jpeg";
            return mimeType;
        }

        /// <summary>
        /// Creates or loads the image
        /// </summary>
        /// <param name="picturePath">The path to the image file</param>
        /// <param name="name">The name of the object</param>
        /// <param name="picId">Image identifier, may be null</param>
        /// <returns>The image or null if the image has not changed</returns>
        protected virtual async Task<Picture> LoadPicture(string picturePath, string name, string picId = "")
        {
            if (String.IsNullOrEmpty(picturePath) || !File.Exists(picturePath))
                return null;

            var mimeType = GetMimeTypeFromFilePath(picturePath);
            var newPictureBinary = File.ReadAllBytes(picturePath);
            var pictureAlreadyExists = false;
            if (!String.IsNullOrEmpty(picId))
            {
                //compare with existing product pictures
                var existingPicture = await _pictureService.GetPictureById(picId);

                var existingBinary = await _pictureService.LoadPictureBinary(existingPicture);
                //picture binary after validation (like in database)
                var validatedPictureBinary = _pictureService.ValidatePicture(newPictureBinary, mimeType);
                if (existingBinary.SequenceEqual(validatedPictureBinary) ||
                    existingBinary.SequenceEqual(newPictureBinary))
                {
                    pictureAlreadyExists = true;
                }
            }

            if (pictureAlreadyExists) return null;

            var newPicture = await _pictureService.InsertPicture(newPictureBinary, mimeType,
                _pictureService.GetPictureSeName(name));
            return newPicture;
        }

        #endregion

        #region Methods
        /// <summary>
        /// Set default property for new object Product 
        /// </summary>
        /// <param name="product">Product object</param>
        /// <param name="isNew"> if new Product</param>
        /// <param name="manager"> object XLS</param>
        public virtual void SetDefaultProductProp(Product product, bool isNew, PropertyManager<Product> manager)
        {
            if (isNew && manager.GetProperties.All(p => p.PropertyName.ToLower() != "producttypeid"))
                if (!string.IsNullOrEmpty(product.Url))
                    product.ProductType = ProductType.SimpleProduct;
                else
                    product.ProductType = ProductType.GroupedProduct;

            if (isNew && manager.GetProperties.All(p => p.PropertyName.ToLower() != "shortdescription"))
                product.ShortDescription = product.Url;

            //product.Name = product.Name + " (" + product.Sku.Trim() + ") ";
            product.LowStock = product.MinStockQuantity > 0 && product.MinStockQuantity >= product.StockQuantity;
            product.UpdatedOnUtc = DateTime.UtcNow;

            if (isNew && manager.GetProperties.All(p => p.PropertyName.ToLower() != "showonhomepage"))
                product.ShowOnHomePage = false;

            if (isNew && manager.GetProperties.All(p => p.PropertyName.ToLower() != "allowcustomerreviews"))
                product.AllowCustomerReviews = true;

            if (isNew && manager.GetProperties.All(p => p.PropertyName.ToLower() != "visibleindividually"))
                product.VisibleIndividually = true;

            if (isNew && manager.GetProperties.All(p => p.PropertyName.ToLower() != "published"))
                product.Published = true;

            if (isNew && manager.GetProperties.All(p => p.PropertyName.ToLower() != "unlimiteddownloads"))
                product.UnlimitedDownloads = true;

            if (isNew && manager.GetProperties.All(p => p.PropertyName.ToLower() != "maxnumberofdownloads"))
                product.MaxNumberOfDownloads = 10;

            if (isNew && manager.GetProperties.All(p => p.PropertyName.ToLower() != "recurringcyclelength"))
                product.RecurringCycleLength = 100;

            if (isNew && manager.GetProperties.All(p => p.PropertyName.ToLower() != "recurringtotalcycles"))
                product.RecurringTotalCycles = 10;

            if (isNew && manager.GetProperties.All(p => p.PropertyName.ToLower() != "recurringtotalcycles"))
                product.RecurringTotalCycles = 10;

            if (isNew && manager.GetProperties.All(p => p.PropertyName.ToLower() != "notifyadminforquantitybelow"))
                product.NotifyAdminForQuantityBelow = 1;

            if (isNew && manager.GetProperties.All(p => p.PropertyName.ToLower() != "orderminimumquantity"))
                product.OrderMinimumQuantity = 1;

            if (isNew && manager.GetProperties.All(p => p.PropertyName.ToLower() != "ordermaximumquantity"))
                product.OrderMaximumQuantity = 1000;

            if (isNew && manager.GetProperties.All(p => p.PropertyName.ToLower() != "maximumcustomerenteredprice"))
                product.MaximumCustomerEnteredPrice = 1000;

            if (isNew && manager.GetProperties.All(p => p.PropertyName.ToLower() != "markasnew"))
                product.MarkAsNew = true;

            if (isNew && manager.GetProperties.All(p => p.PropertyName.ToLower() != "markasnewstartdatetimeutc"))
                product.MarkAsNewStartDateTimeUtc = DateTime.UtcNow;

            if (isNew && manager.GetProperties.All(p => p.PropertyName.ToLower() != "markasnewEnddatetimeutc"))
                product.MarkAsNewEndDateTimeUtc = DateTime.UtcNow.AddDays(30);

        }

        public virtual async Task<Product> CheckMainProducts(
            Product product,
            PropertyManager<Product> manager,
            IList<ProductTemplate> templates,
            IList<DeliveryDate> deliveryDates,
            IList<Warehouse> warehouses,
            IList<MeasureUnit> units,
            IList<TaxCategory> taxes,
            IList<CategoryTemplate> templatesCategory,
            IList<ManufacturerTemplate> templatesManufacturer)
        {
            Product productMain = null;

            if (!string.IsNullOrEmpty(product.Sku))
                productMain = await _productService.GetProductBySkuAndVendor(product.Sku, "");

            var isNew = productMain == null;


            if (isNew)
            {
                productMain ??= new Product();

                productMain.CreatedOnUtc = DateTime.UtcNow;
                productMain.ProductTemplateId = templates.FirstOrDefault()?.Id;
                productMain.DeliveryDateId = deliveryDates.FirstOrDefault()?.Id;
                productMain.TaxCategoryId = taxes.FirstOrDefault()?.Id;
                productMain.WarehouseId = warehouses.FirstOrDefault()?.Id;
                productMain.UnitId = units.FirstOrDefault().Id;
                productMain.VendorId = string.Empty;
                PrepareProductMapping(productMain, manager, templates, deliveryDates, warehouses, units, taxes, null);
                productMain.Url = string.Empty;
                productMain.ManufactureName = string.Empty;
                productMain.Model = string.Empty;
                productMain.Year = string.Empty;
                productMain.Color = string.Empty;
                productMain.Size = string.Empty;
                SetDefaultProductProp(productMain, isNew, manager);

                var templateName = "Grouped product (with variants)";
                var template = templates.FirstOrDefault(x => x.Name == templateName);
                productMain.ProductTemplateId = template.Id;

                await InsertOrUpdateProduct(productMain, manager, isNew, templatesCategory, templatesManufacturer);


            }

            return productMain;
        }

        protected virtual async Task UpdateSpecficationAtribute(Product product, string specAtrName, string specValue)
        {
            var _filteringAtribute = new List<string> { "manufacturer", "sp_size", "sp_available", "sp_typebike", "sp_for", "sp_material_frame"
                , "sp_wheeldiams", "sp_type_brake","sp_year","sp_count_speed","sp_type_fork","sp_type_rear_hub","sp_weight_Limit"
                , "sp_age", "sp_Fork_travel", "sp_equipment", "sp_model", "sp_color" };

            var _nonVisibleAtribute = new List<string> { "sp_available", "sp_brand_fork" };

            var _separeteAtribute = new List<string> { "sp_size", "sp_typebike", "sp_for", "sp_type_brake", "sp_color" };

            var _separeteAtributeWithLinq = new List<string> { "sp_model" };

            var specificationAttribute = await _specificationAttributeService.GetSpecificationAttributeBySeName(specAtrName);

            if (specificationAttribute == null)
            {
                specificationAttribute = new SpecificationAttribute {
                    Name = specAtrName,
                    SeName = specAtrName
                };

                await _specificationAttributeService.InsertSpecificationAttribute(specificationAttribute);
            }

            
            //specValue.Trim().Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            var separeteSymbol = _separeteAtribute.Contains(specAtrName) ? new[] { ", ", "/" } : new[] { "NOSEPARATE" };
            var props = specValue.Trim().Split(separeteSymbol, StringSplitOptions.RemoveEmptyEntries).ToList();

            foreach (var prop in props)
            {

                //var nameSpecification = prop.Trim();
                //var ? = (specAtrName == "sp_model") ? ' ' : '^';
                separeteSymbol = _separeteAtributeWithLinq.Contains(specAtrName) ? new[] { "/" } : new[] { "NOSEPARATE" };
                var nameSpecificationTree = prop.Trim().Split(separeteSymbol, StringSplitOptions.RemoveEmptyEntries).ToList();

                var parantSpecAttrOptionId = "";
                

                foreach (var nameSpecification in nameSpecificationTree)
                {
                    
                    var nameSpecificationTrim = nameSpecification.Trim();

                    var specificationAttributeOption = await _specificationAttributeService.GetSpecificationAttributeByOptionName(specificationAttribute.Id, nameSpecificationTrim);

                    if (specificationAttributeOption == null)
                    {
                        specificationAttributeOption ??= new SpecificationAttributeOption();
                        specificationAttributeOption.Name = nameSpecificationTrim;
                        specificationAttributeOption.SeName = SeoExtensions.GetSeName(nameSpecification, true, true, nameSpecificationTrim);
                        specificationAttributeOption.ParentSpecificationAttrOptionId = parantSpecAttrOptionId;
                        await _specificationAttributeService.UpdateSpecificationAttributeOption(specificationAttribute, specificationAttributeOption);
                    }
                    
                    parantSpecAttrOptionId = specificationAttributeOption.Id;

                    var productId = product.Id;
                    
                    if (!product.Name.Contains("Frame"))
                    {
                        product.Name = specAtrName == "sp_size" ? product.Name + " Frame " + nameSpecificationTrim : product.Name;
                    }

                    var productSpecificationAttribute = product.ProductSpecificationAttributes.Where(s =>
                         s.SpecificationAttributeId == specificationAttribute.Id &&
                         s.SpecificationAttributeOptionId == specificationAttributeOption.Id).FirstOrDefault();

                    if (productSpecificationAttribute == null)
                    {
                        productSpecificationAttribute = new ProductSpecificationAttribute {

                            ProductId = productId,
                            SpecificationAttributeId = specificationAttribute.Id,
                            SpecificationAttributeOptionId = specificationAttributeOption.Id,
                            AllowFiltering = _filteringAtribute.Contains(specAtrName),
                            ShowOnProductPage = !_nonVisibleAtribute.Contains(specAtrName)
                        };

                        await _specificationAttributeService.InsertProductSpecificationAttribute(productSpecificationAttribute);
                    }
                }
            }
        }
        protected virtual async Task PrepareSpecficationAtributeMapping(Product product, PropertyManager<Product> manager)
        {
            foreach (var property in manager.GetProperties)
            {

                if (property.PropertyName.ToLower().Substring(0, 3) == "sp_" 
                    || property.PropertyName.ToLower() == "manufacturer")
                {
                    var specAtrName = property.PropertyName;
                    var specValue = property.StringValue;
                    await UpdateSpecficationAtribute(product, specAtrName, specValue);
                    
                }
            }
        }


        /// <summary>
        /// Import products from XLSX file
        /// </summary>
        /// <param name="stream">Stream</param>
        public virtual async Task ImportProductsFromXlsx(Stream stream)
        {
            var workbook = new XSSFWorkbook(stream);
            var worksheet = workbook.GetSheetAt(0);
            if (worksheet == null)
                throw new GrandException("No worksheet found");

            var manager = GetPropertyManager<Product>(worksheet);

            var templates = await _productTemplateService.GetAllProductTemplates();
            var deliveryDates = await _shippingService.GetAllDeliveryDates();
            var taxes = await _taxService.GetAllTaxCategories();
            var warehouses = await _shippingService.GetAllWarehouses();
            var units = await _measureService.GetAllMeasureUnits();
            var templatesCategory = await _categoryTemplateService.GetAllCategoryTemplates();
            var templatesManufacturer = await _manufacturerTemplateService.GetAllManufacturerTemplates();

            for (var iRow = 1; iRow < worksheet.PhysicalNumberOfRows; iRow++)
            {

                manager.ReadFromXlsx(worksheet, iRow);
                var sku = manager.GetProperty("sku") != null ? manager.GetProperty("sku").StringValue : string.Empty;
                var productid = manager.GetProperty("id") != null ? manager.GetProperty("id").StringValue : string.Empty;

                var vendorName = string.Empty;
                var vendorId = manager.GetProperty("vendorid") != null ? manager.GetProperty("vendorid").StringValue : string.Empty;
                var vendor = new Vendor();

                if (string.IsNullOrEmpty(vendorId))
                {
                    vendorName = manager.GetProperty("vendor") != null ? manager.GetProperty("vendor").StringValue : string.Empty;
                    vendor = await _vendorService.GetVendorByName(vendorName);
                    vendorId = vendor.Id;
                }

                Product product = null;

                ////product = await _productService.GetProductBySku(sku);

                if (!string.IsNullOrEmpty(sku))
                    product = await _productService.GetProductBySkuAndVendor(sku, vendorId);

                if (!string.IsNullOrEmpty(productid))
                    product = await _productService.GetProductById(productid);

                var isNew = product == null;

                product ??= new Product();

                if (isNew)
                {
                    product.CreatedOnUtc = DateTime.UtcNow;
                    product.ProductTemplateId = templates.FirstOrDefault()?.Id;
                    product.DeliveryDateId = deliveryDates.FirstOrDefault()?.Id;
                    product.TaxCategoryId = taxes.FirstOrDefault()?.Id;
                    product.WarehouseId = warehouses.FirstOrDefault()?.Id;
                    product.UnitId = units.FirstOrDefault().Id;
                    if (!string.IsNullOrEmpty(productid))
                        product.Id = productid;
                    if (!string.IsNullOrEmpty(vendorId))
                        product.VendorId = vendorId;

                }

                PrepareProductMapping(product, manager, templates, deliveryDates, warehouses, units, taxes, vendor);
                SetDefaultProductProp(product, isNew, manager);

                var mainProduct = await CheckMainProducts(product, manager, templates, deliveryDates, warehouses, units, taxes, templatesCategory, templatesManufacturer);
                
                if (mainProduct != null) {
                    await PrepareSpecficationAtributeMapping(mainProduct, manager);
                    //pictures
                    await PrepareProductPictures(mainProduct, manager, isNew);

                    await _productService.UpdateProduct(mainProduct);
                }

                if (mainProduct != null)
                    product.ParentGroupedProductId = mainProduct.Id;

                product.Name += " - " + vendorName.Trim();
                product.VisibleIndividually = false;

                await InsertOrUpdateProduct(product, manager, isNew, templatesCategory, templatesManufacturer);

            }

        }

        public virtual async Task InsertOrUpdateProduct(
            Product product,
            PropertyManager<Product> manager,
            bool isNew,
            IList<CategoryTemplate> templatesCategory,
            IList<ManufacturerTemplate> templatesManufacturer)
        {

            if (isNew)
            {
                await _productService.InsertProduct(product);
            }
            else
            {
                await _productService.UpdateProduct(product);
            }

            //search engine name
            var seName = manager.GetProperty("sename") != null ? manager.GetProperty("sename").StringValue : product.Name;
            await _urlRecordService.SaveSlug(product, await product.ValidateSeName(seName, product.Name, true, _seoSetting, _urlRecordService, _languageService), "");
            var _seName = await product.ValidateSeName(seName, product.Name, true, _seoSetting, _urlRecordService, _languageService);

            //search engine name
            await _urlRecordService.SaveSlug(product, _seName, "");
            product.SeName = _seName;
            await _productService.UpdateProduct(product);

            //category mappings
            var categoryIds = manager.GetProperty("categoryids") != null ? manager.GetProperty("categoryids").StringValue : string.Empty;
            if (!string.IsNullOrEmpty(categoryIds))
                await PrepareProductCategories(product, categoryIds);

            var categoryName = manager.GetProperty("category") != null ? manager.GetProperty("category").StringValue : string.Empty;
            if (!string.IsNullOrEmpty(categoryName))
                await PrepareProductCategoriesByName(product, categoryName, templatesCategory);

            //manufacturer mappings
            var manufacturerIds = manager.GetProperty("manufacturerids") != null ? manager.GetProperty("manufacturerids").StringValue : string.Empty;
            if (!string.IsNullOrEmpty(manufacturerIds))
                await PrepareProductManufacturers(product, manufacturerIds);

            var manufacturerName = manager.GetProperty("manufacturer") != null ? manager.GetProperty("manufacturer").StringValue : string.Empty;
            if (!string.IsNullOrEmpty(manufacturerName))
            {
                //await UpdateSpecficationAtribute(product, "manufacturer", manufacturerName);
                await PrepareProductManufacturersByName(product, manufacturerName, templatesManufacturer);
            }
            
            await _productService.UpdateProduct(product);

        }

        /// <summary>
        /// Import newsletter subscribers from TXT file
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <returns>Number of imported subscribers</returns>
        public virtual async Task<int> ImportNewsletterSubscribersFromTxt(Stream stream)
        {
            int count = 0;
            using (var reader = new StreamReader(stream))
            {
                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();
                    if (string.IsNullOrWhiteSpace(line))
                        continue;
                    string[] tmp = line.Split(',');

                    var email = "";
                    bool isActive = true;
                    var categories = new List<string>();
                    bool iscategories = false;
                    string storeId = _storeContext.CurrentStore.Id;
                    //parse
                    if (tmp.Length == 1)
                    {
                        //"email" only
                        email = tmp[0].Trim();
                    }
                    else if (tmp.Length == 2)
                    {
                        //"email" and "active" fields specified
                        email = tmp[0].Trim();
                        isActive = Boolean.Parse(tmp[1].Trim());
                    }
                    else if (tmp.Length == 3)
                    {
                        //"email" and "active" and "storeId" fields specified
                        email = tmp[0].Trim();
                        isActive = Boolean.Parse(tmp[1].Trim());
                        storeId = tmp[2].Trim();
                    }
                    else if (tmp.Length == 4)
                    {
                        //"email" and "active" and "storeId" and categories fields specified
                        email = tmp[0].Trim();
                        isActive = Boolean.Parse(tmp[1].Trim());
                        storeId = tmp[2].Trim();
                        try
                        {
                            var items = tmp[3].Trim().Split(';').ToList();
                            foreach (var item in items)
                            {
                                if (!string.IsNullOrEmpty(item))
                                {
                                    if (_newsletterCategoryService.GetNewsletterCategoryById(item) != null)
                                        categories.Add(item);
                                }
                            }
                            iscategories = true;
                        }
                        catch { };
                    }
                    else
                        throw new GrandException("Wrong file format");

                    //import
                    await ImportSubscription(email, storeId, isActive, iscategories, categories);

                    count++;
                }
            }

            return count;
        }

        /// <summary>
        /// Import states from TXT file
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <returns>Number of imported states</returns>
        public virtual async Task<int> ImportStatesFromTxt(Stream stream)
        {
            int count = 0;
            using (var reader = new StreamReader(stream))
            {
                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();
                    if (String.IsNullOrWhiteSpace(line))
                        continue;
                    string[] tmp = line.Split(',');

                    if (tmp.Length != 5)
                        throw new GrandException("Wrong file format");

                    //parse
                    var countryTwoLetterIsoCode = tmp[0].Trim();
                    var name = tmp[1].Trim();
                    var abbreviation = tmp[2].Trim();
                    bool published = Boolean.Parse(tmp[3].Trim());
                    int displayOrder = Int32.Parse(tmp[4].Trim());

                    var country = await _countryService.GetCountryByTwoLetterIsoCode(countryTwoLetterIsoCode);
                    if (country == null)
                    {
                        //country cannot be loaded. skip
                        continue;
                    }

                    //import
                    var states = await _stateProvinceService.GetStateProvincesByCountryId(country.Id, showHidden: true);
                    var state = states.FirstOrDefault(x => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

                    if (state != null)
                    {
                        state.Abbreviation = abbreviation;
                        state.Published = published;
                        state.DisplayOrder = displayOrder;
                        await _stateProvinceService.UpdateStateProvince(state);
                    }
                    else
                    {
                        state = new StateProvince {
                            CountryId = country.Id,
                            Name = name,
                            Abbreviation = abbreviation,
                            Published = published,
                            DisplayOrder = displayOrder,
                        };
                        await _stateProvinceService.InsertStateProvince(state);
                    }
                    count++;
                }
            }

            return count;
        }

        /// <summary>
        /// Import manufacturers from XLSX file
        /// </summary>
        /// <param name="stream">Stream</param>
        public virtual async Task ImportManufacturerFromXlsx(Stream stream)
        {
            var workbook = new XSSFWorkbook(stream);
            var worksheet = workbook.GetSheetAt(0);
            if (worksheet == null)
                throw new GrandException("No worksheet found");

            var manager = GetPropertyManager<Manufacturer>(worksheet);

            var templates = await _manufacturerTemplateService.GetAllManufacturerTemplates();

            for (var iRow = 1; iRow < worksheet.PhysicalNumberOfRows; iRow++)
            {

                manager.ReadFromXlsx(worksheet, iRow);
                var manufacturerid = manager.GetProperty("id") != null ? manager.GetProperty("id").StringValue : string.Empty;
                var manufacturer = string.IsNullOrEmpty(manufacturerid) ? null : await _manufacturerService.GetManufacturerById(manufacturerid);

                var isNew = manufacturer == null;

                manufacturer ??= new Manufacturer();

                if (isNew)
                {
                    manufacturer.CreatedOnUtc = DateTime.UtcNow;
                    manufacturer.ManufacturerTemplateId = templates.FirstOrDefault()?.Id;
                    if (!string.IsNullOrEmpty(manufacturerid))
                        manufacturer.Id = manufacturerid;
                }

                PrepareManufacturerMapping(manufacturer, manager, templates);


                var picture = manager.GetProperty("picture") != null ? manager.GetProperty("sename").StringValue : "";
                if (!string.IsNullOrEmpty(picture))
                {
                    var _picture = await LoadPicture(picture, manufacturer.Name,
                        isNew ? "" : manufacturer.PictureId);
                    if (_picture != null)
                        manufacturer.PictureId = _picture.Id;
                }
                manufacturer.UpdatedOnUtc = DateTime.UtcNow;

                if (isNew)
                    await _manufacturerService.InsertManufacturer(manufacturer);
                else
                    await _manufacturerService.UpdateManufacturer(manufacturer);

                var sename = manager.GetProperty("sename") != null ? manager.GetProperty("sename").StringValue : manufacturer.Name;
                sename = await manufacturer.ValidateSeName(sename, manufacturer.Name, true, _seoSetting, _urlRecordService, _languageService);
                manufacturer.SeName = sename;
                await _manufacturerService.UpdateManufacturer(manufacturer);
                await _urlRecordService.SaveSlug(manufacturer, manufacturer.SeName, "");

            }

        }

        /// <summary>
        /// Import categories from XLSX file
        /// </summary>
        /// <param name="stream">Stream</param>
        public virtual async Task ImportCategoryFromXlsx(Stream stream)
        {
            var workbook = new XSSFWorkbook(stream);
            var worksheet = workbook.GetSheetAt(0);
            if (worksheet == null)
                throw new GrandException("No worksheet found");

            var manager = GetPropertyManager<Category>(worksheet);

            var templates = await _categoryTemplateService.GetAllCategoryTemplates();

            for (var iRow = 1; iRow < worksheet.PhysicalNumberOfRows; iRow++)
            {
                manager.ReadFromXlsx(worksheet, iRow);

                var categoryid = manager.GetProperty("id") != null ? manager.GetProperty("id").StringValue : string.Empty;
                var category = string.IsNullOrEmpty(categoryid) ? null : await _categoryService.GetCategoryById(categoryid);

                var isNew = category == null;

                category ??= new Category();

                if (isNew)
                {
                    category.CreatedOnUtc = DateTime.UtcNow;
                    category.CategoryTemplateId = templates.FirstOrDefault()?.Id;
                    if (!string.IsNullOrEmpty(categoryid))
                        category.Id = categoryid;
                }

                PrepareCategoryMapping(category, manager, templates);
                category.UpdatedOnUtc = DateTime.UtcNow;

                if (isNew)
                    await _categoryService.InsertCategory(category);
                else
                    await _categoryService.UpdateCategory(category);

                var picture = manager.GetProperty("picture") != null ? manager.GetProperty("sename").StringValue : "";
                if (!string.IsNullOrEmpty(picture))
                {
                    var _picture = await LoadPicture(picture, category.Name, isNew ? "" : category.PictureId);
                    if (_picture != null)
                        category.PictureId = _picture.Id;
                }

                var sename = manager.GetProperty("sename") != null ? manager.GetProperty("sename").StringValue : category.Name;
                sename = await category.ValidateSeName(sename, category.Name, true, _seoSetting, _urlRecordService, _languageService);
                category.SeName = sename;
                category.HideOnCatalog = true;
                await _categoryService.UpdateCategory(category);
                await _urlRecordService.SaveSlug(category, sename, "");

            }

        }

        #endregion
    }
}
