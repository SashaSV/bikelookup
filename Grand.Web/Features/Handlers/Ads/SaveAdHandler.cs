using Grand.Domain.Ads;
using Grand.Domain.Catalog;
using Grand.Domain.Customers;
using Grand.Domain.Data;
using Grand.Services.Catalog;
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

        public SaveAdHandler(IRepository<Ad> adRepository, IProductService productService, IVendorService vendorService)
        {
            _adRepository = adRepository;
            _productService = productService;
            _vendorService = vendorService;

        }
        
        public async Task<bool>  Handle(SaveAd request, CancellationToken cancellationToken)
        {
            var vendor = await _vendorService.GetVendorByName(request.Customer.Username);
            
            var product = new Product
            {
                Name = request.AdToSave.Model,
                ManufactureName = request.AdToSave.ManufactureName,
                Price = request.AdToSave.Price,
                Year = request.AdToSave.Year.ToString(),
                ProductType = ProductType.SimpleProduct,
                Published = true,
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
            
           
            var ad = new Ad() {
                AdItem = new AdItem()
                {
                    ProductId = product.Id,
                    VendorId = request.Customer.Id
                },
                CreatedOnUtc = DateTime.Now,
                AdStatus = AdStatus.Processing,

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