using Grand.Core;
using Grand.Domain.Ads;
using Grand.Domain.Catalog;
using Grand.Domain.Customers;
using Grand.Domain.Data;
using Grand.Services.Catalog;
using Grand.Services.Common;
using Grand.Services.Customers;
using Grand.Services.Media;
using Grand.Services.Vendors;
using Grand.Web.Features.Models.Ads;
using Grand.Web.Models.Ads;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Web.Features.Handlers.Ads
{
    public class SaveAdHandler : IRequestHandler<SaveAd, bool>
    {
        private readonly IRepository<Ad> _adRepository;
        private readonly IProductService _productService;
        private readonly IVendorService _vendorService;
        private readonly IPictureService _pictureService;
        private readonly IWorkContext _workContext;
        private readonly ICustomerService _customerService;
        private readonly CustomerSettings _customerSettings;

        public SaveAdHandler(IRepository<Ad> adRepository, IProductService productService, IVendorService vendorService, IPictureService pictureService, 
            IWorkContext workContext, ICustomerService customerService, CustomerSettings customerSettings)
        {
            _adRepository = adRepository;
            _productService = productService;
            _vendorService = vendorService;
            _pictureService = pictureService;
            _workContext = workContext;
            _customerService = customerService;
            _customerSettings = customerSettings;

        }
        
        public async Task<bool>  Handle(SaveAd request, CancellationToken cancellationToken)
        {
            var groupedProductId = request.AdToSave.SearchBike;
            
            if ( string.IsNullOrEmpty(request.AdToSave.SearchBike))
            {
                var groupedProduct = new Product
                {
                    Name = request.AdToSave.Model,
                    ManufactureName = request.AdToSave.ManufactureName,
                    Price = request.AdToSave.Price,
                    Year = request.AdToSave.Year.ToString(),
                    ProductType = ProductType.GroupedProduct,
                    Published = true,
                    Weeldiam = request.AdToSave.Weeldiam,
                    Color = request.AdToSave.Color,
                    Model = request.AdToSave.Model,
                    Size = request.AdToSave.Size,
                    AvailableStartDateTimeUtc = DateTime.Today,
                    AvailableEndDateTimeUtc = DateTime.Today.AddMonths(3),
                    UpdatedOnUtc = DateTime.Now
                };
                await _productService.InsertProduct(groupedProduct);
                
                groupedProductId = groupedProduct.Id;
            }

            //var customerAd = await _customerService.GetCustomerById(request.Customer.Id);
            var customerAd = _workContext.CurrentCustomer;
            var customerName = customerAd.GetFullName();
            //customerAd.FormatUserName(_customerSettings.CustomerNameFormat);
            
            var vendor = await _vendorService.GetVendorByName(customerName);
            vendor.Email = customerAd.Email;
            var pictureId = customerAd.GetAttributeFromEntity<string>(SystemCustomerAttributeNames.AvatarPictureId);
            vendor.PictureId = pictureId;

            await _vendorService.UpdateVendor(vendor);
             var product = new Product
            {
                Name = request.AdToSave.Model,
                ManufactureName = request.AdToSave.ManufactureName,
                Price = request.AdToSave.Price,
                Year = request.AdToSave.Year.ToString(),
                ProductType = ProductType.SimpleProduct,
                Published = true,
                Weeldiam = request.AdToSave.Weeldiam,
                Color = request.AdToSave.Color,
                Model = request.AdToSave.Model,
                Size = request.AdToSave.Size,
                AvailableStartDateTimeUtc = DateTime.Today,
                AvailableEndDateTimeUtc = DateTime.Today.AddMonths(3),
                VendorId = vendor.Id,
                UpdatedOnUtc = DateTime.Now
            };
            
            if (!string.IsNullOrEmpty(groupedProductId))
            {
                product.ParentGroupedProductId = groupedProductId;
            }

            await _productService.InsertProduct(product);
            if (request.AdToSave.ImageFile != null)
            {
                foreach (var pirctureFile in request.AdToSave.ImageFile)
                {
                    var pictureBites = pirctureFile.GetPictureBits();

                    var picture = await _pictureService.InsertPicture(pictureBites, pirctureFile.ContentType, _pictureService.GetPictureSeName(request.AdToSave.Model));

                    if (picture != null)
                    {
                        await _pictureService.UpdatePicture(picture.Id, await _pictureService.LoadPictureBinary(picture),
                            picture.MimeType,
                            picture.SeoFilename);

                        await _productService.InsertProductPicture(new ProductPicture {
                            PictureId = picture.Id,
                            ProductId = product.Id,
                            DisplayOrder = 1,
                            MimeType = picture.MimeType,
                            SeoFilename = picture.SeoFilename
                        });
                    }
                }
            }
            var ad = new Ad() {
                AdItem = new AdItem()
                {
                    ProductId = product.Id,
                    VendorId = request.Customer.Id
                },
                CreatedOnUtc = DateTime.Now,
                AdStatus = AdStatus.Processing,
                ProductId = request.AdToSave.SearchBike,
                Price = request.AdToSave.Price,
                EndDateTimeUtc = DateTime.Now.AddMonths(1),
                CustomerCurrencyCode = _workContext.WorkingCurrency.CurrencyCode,
                AdComment = request.AdToSave.AdComment
            };
            
            ad.StoreId = request.Store.Id;
            if (!request.Customer.IsOwner())
                ad.CustomerId = request.Customer.Id;
            else
                ad.OwnerId = request.Customer.Id;
            
            
            await _adRepository.InsertAsync(ad);
            
            return await Task.FromResult(true);
        }
    }
}