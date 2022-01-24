using Grand.Core;
using Grand.Domain.Ads;
using Grand.Domain.Catalog;
using Grand.Domain.Common;
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
using System.Linq;
using Grand.Services.Ads;
using SkiaSharp;
using System.Collections.Generic;

namespace Grand.Web.Features.Handlers.Ads
{
    public class SaveAdHandler : IRequestHandler<SaveAd, bool>
    {
        private readonly IAdService _adRepository;
        private readonly IProductService _productService;
        private readonly IVendorService _vendorService;
        private readonly IPictureService _pictureService;
        private readonly IWorkContext _workContext;
        private readonly ICustomerService _customerService;
        private readonly CustomerSettings _customerSettings;
        private readonly IGenericAttributeService _genericAttributeService;

        public SaveAdHandler(IAdService adRepository, IProductService productService, IVendorService vendorService, IPictureService pictureService, 
            IWorkContext workContext, ICustomerService customerService, CustomerSettings customerSettings, IGenericAttributeService genericAttributeService)
        {
            _adRepository = adRepository;
            _productService = productService;
            _vendorService = vendorService;
            _pictureService = pictureService;
            _workContext = workContext;
            _customerService = customerService;
            _customerSettings = customerSettings;
            _genericAttributeService = genericAttributeService;
        }
        
        public async Task<bool>  Handle(SaveAd request, CancellationToken cancellationToken)
        {
            var groupedProductId = request.AdToSave.SearchBike;
            var groupedProduct = await _productService.GetProductById(groupedProductId);

            if ( string.IsNullOrEmpty(request.AdToSave.SearchBike))
            {
                groupedProduct = new Product
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
            
            //customerAd.FormatUserName(_customerSettings.CustomerNameFormat);
            await _customerService.UpdateAddressFromCustomerFileds(customerAd);
            var address = customerAd.Addresses.FirstOrDefault();
            var customerName = $"{customerAd.FormatUserName(CustomerNameFormat.ShowFirstName)} ({address?.City ?? ""})";

            var vendor = await _vendorService.GetVendorByEmail(customerAd.Email,null);

            vendor.Email = customerAd.Email;
            var pictureId = customerAd.GetAttributeFromEntity<string>(SystemCustomerAttributeNames.AvatarPictureId);
            vendor.PictureId = pictureId;
            vendor.Addresses = customerAd.Addresses;
            var addressCustomer = customerAd.ShippingAddress;

            await _vendorService.UpdateVendor(vendor);
            
            var ad = new Ad() {
                CreatedOnUtc = DateTime.Now,
                AdStatus = AdStatus.Active,
                ProductId = request.AdToSave.SearchBike,
                Price = request.AdToSave.Price,
                EndDateTimeUtc = DateTime.Now.AddMonths(1),
                CustomerCurrencyCode = _workContext.WorkingCurrency.CurrencyCode,
                AdComment = request.AdToSave.AdComment,
                ShippingAddress = addressCustomer,
                WithDocuments = request.AdToSave.WithDocuments,
                Mileage = request.AdToSave.Mileage,
                IsAuction = request.AdToSave.IsAuction
            };

            var newAd = await _adRepository.InsertAd(ad);

            var product = new Product
            {
                Name = string.Format("{0} {1} {2}", groupedProduct.Name, customerName, request.AdToSave.Price),
                Price = request.AdToSave.Price,
                Year = request.AdToSave.Year.ToString(),
                ProductType = ProductType.SimpleProduct,
                Published = true,
                ManufactureName = request.AdToSave.ManufactureName,
                Weeldiam = request.AdToSave.Weeldiam,
                Color = request.AdToSave.Color,
                Model = request.AdToSave.Model,
                Size = request.AdToSave.Size,
                AvailableStartDateTimeUtc = DateTime.Today,
                AvailableEndDateTimeUtc = DateTime.Today.AddMonths(1),
                VendorId = vendor.Id,
                UpdatedOnUtc = DateTime.Now,
                AdId = newAd.Id
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

                    var pictureBinary = _pictureService.ValidatePicture(pictureBites, pirctureFile.ContentType);


                    var picture = await _pictureService.InsertPicture(pictureBites, pirctureFile.ContentType, _pictureService.GetPictureSeName(request.AdToSave.Model), validateBinary: true);

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

            newAd.AdItem = new AdItem() {
                ProductId = product.Id,
                VendorId = vendor.Id
            };

            newAd.StoreId = request.Store.Id;
            if (!request.Customer.IsOwner())
                newAd.CustomerId = request.Customer.Id;
            else
                newAd.OwnerId = request.Customer.Id;

            await _adRepository.UpdateAd(newAd);
            //var newAd = await _adRepository.InsertAsync(ad);

            
            //await _productService.UpdateProduct(product);

            return await Task.FromResult(true);
        }
    }
}