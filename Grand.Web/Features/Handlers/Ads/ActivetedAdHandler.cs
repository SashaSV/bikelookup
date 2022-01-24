using Grand.Domain.Ads;
using Grand.Domain.Data;
using Grand.Services.Catalog;
using Grand.Services.Commands.Models.Ads;
using Grand.Web.Features.Models.Ads;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Web.Features.Handlers.Ads
{
    public class ActivetedAdHandler :  IRequestHandler<ActivetedAdCommand, bool>
    {
        private readonly IRepository<Ad> _adRepository;
        private readonly IProductService _productService;

        public ActivetedAdHandler(IRepository<Ad> adRepository, IProductService productService)
        {
            _adRepository = adRepository;
            _productService = productService;
        }

        public async Task<bool> Handle(ActivetedAdCommand request, CancellationToken cancellationToken)
        {
            var ad = request.Ad;
            ad.AdStatus = AdStatus.Active;
            var p = await _productService.GetProductById(ad.AdItem.ProductId);
            p.Published = true;
            await _productService.UpdateProduct(p);
            await _adRepository.UpdateAsync(ad);
            return false;
        }
    }
}