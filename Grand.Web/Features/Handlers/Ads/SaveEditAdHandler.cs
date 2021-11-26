using Grand.Core;
using Grand.Domain.Ads;
using Grand.Domain.Catalog;
using Grand.Domain.Customers;
using Grand.Domain.Data;
using Grand.Services.Catalog;
using Grand.Services.Customers;
using Grand.Services.Media;
using Grand.Services.Vendors;
using Grand.Web.Features.Models.Ads;
using MediatR;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Web.Features.Handlers.Ads
{
    public class SaveEditAdHandler : IRequestHandler<EditAdSave, bool>
    {
        private readonly IRepository<Ad> _adRepository;
        private readonly IProductService _productService;
        private readonly IVendorService _vendorService;
        private readonly IPictureService _pictureService;
        private readonly IWorkContext _workContext;
        private readonly ICustomerService _customerService;

        public SaveEditAdHandler(IRepository<Ad> adRepository, IProductService productService, IVendorService vendorService
            , IPictureService pictureService
            , IWorkContext workContext
            , ICustomerService customerService)
        {
            _adRepository = adRepository;
            _productService = productService;
            _vendorService = vendorService;
            _pictureService = pictureService;
            _workContext = workContext;
            _customerService = customerService;

        }

        public async Task<bool> Handle(EditAdSave request, CancellationToken cancellationToken)
        {
            var ad = await _adRepository.GetByIdAsync(request.Model.Id);
            var product = await _productService.GetProductById(ad.AdItem.Id);


            ad.Price = request.Model.Price;
            ad.AdComment = request.Model.AdComment;
            ad.AdStatus = AdStatus.Processing;


            if (product != null)
            {
                product.Price = request.Model.Price;
                await _productService.UpdateProduct(product);
            }

            var venAddr = ad.ShippingAddress;
            
            if (venAddr == null) {
                var userId = ad.CustomerId == null ? ad.OwnerId : ad.CustomerId;

                if (userId != null)
                {
                    var customerAd = await _customerService.GetCustomerById(userId);
                    var customerName = string.Format("{0} ({1})", 
                            customerAd.FormatUserName(CustomerNameFormat.ShowFirstName), 
                                customerAd.Addresses.First().City);
                    
                    var vendor = await _vendorService.GetVendorByEmail(customerAd.Email, customerName);

                    vendor.Addresses = customerAd.Addresses;

                    await _vendorService.UpdateVendor(vendor);
                    ad.ShippingAddress = customerAd.ShippingAddress;
                }
            }

            await _adRepository.UpdateAsync(ad);
            return true;
        }
    }

}