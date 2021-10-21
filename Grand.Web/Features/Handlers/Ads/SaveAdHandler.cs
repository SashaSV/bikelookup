using Grand.Core;
using Grand.Domain.Ads;
using Grand.Domain.Catalog;
using Grand.Domain.Customers;
using Grand.Domain.Data;
using Grand.Services.Catalog;
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
        public SaveAdHandler(IRepository<Ad> adRepository, IProductService productService, IVendorService vendorService, IPictureService pictureService, IWorkContext workContext)
        {
            _adRepository = adRepository;
            _productService = productService;
            _vendorService = vendorService;
            _pictureService = pictureService;
            _workContext = workContext;

        }
        
        public async Task<bool>  Handle(SaveAd request, CancellationToken cancellationToken)
        {
            var vendor = await _vendorService.GetVendorByName(request.Customer.Username);
            var pictureBites = request.AdToSave.ImageFile.GetPictureBits();
            
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
            
            if (!string.IsNullOrEmpty(request.AdToSave.SearchBike))
            {
                product.ParentGroupedProductId = request.AdToSave.SearchBike;
            }

            await _productService.InsertProduct(product);
            
            var picture = await _pictureService.InsertPicture(pictureBites, request.AdToSave.ImageFile.ContentType,_pictureService.GetPictureSeName(request.AdToSave.Model));

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
                CustomerCurrencyCode = _workContext.WorkingCurrency.CurrencyCode
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