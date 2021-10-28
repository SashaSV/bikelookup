using Grand.Core;
using Grand.Domain.Ads;
using Grand.Domain.Catalog;
using Grand.Domain.Customers;
using Grand.Domain.Data;
using Grand.Services.Catalog;
using Grand.Services.Media;
using Grand.Services.Vendors;
using Grand.Web.Features.Models.Ads;
using MediatR;
using System;
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
        public SaveEditAdHandler(IRepository<Ad> adRepository, IProductService productService, IVendorService vendorService, IPictureService pictureService, IWorkContext workContext)
        {
            _adRepository = adRepository;
            _productService = productService;
            _vendorService = vendorService;
            _pictureService = pictureService;
            _workContext = workContext;

        }
        
        public async Task<bool>  Handle(EditAdSave request, CancellationToken cancellationToken)
        {
            var ad = await _adRepository.GetByIdAsync(request.Model.Id);
            
            var product = await  _productService.GetProductById(ad.AdItem.Id);
           
            ad.Price = request.Model.Price;
            ad.AdComment = request.Model.AdComment;
            
             await _adRepository.UpdateAsync(ad);
             
            if(product!=null)
            {
              product.Price = request.Model.Price;
              await _productService.UpdateProduct(product);
                
            }

            return true;
        }
    }

}