using Grand.Domain.Ads;
using Grand.Domain.Data;
using Grand.Services.Catalog;
using Grand.Web.Features.Models.Ads;
using Grand.Web.Models.Ads;
using MediatR;
using NPOI.SS.Formula.Functions;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Web.Features.Handlers.Ads
{
    public class DeleteAdHandler : IRequestHandler<DeleteAd, DeleteAdResult>
    {
        private readonly IRepository<Ad> _adRepository;
        private readonly IProductService _productService;

        public DeleteAdHandler(IRepository<Ad> adRepository, IProductService productService)
        {
            _adRepository = adRepository;
            _productService = productService;
        }
        
        public async Task<DeleteAdResult> Handle(DeleteAd request, CancellationToken cancellationToken)
        {
            var ad = await _adRepository.GetByIdAsync(request.Ad.Id);
            if (ad == null)
            {
                return new DeleteAdResult(false, "Not found");
            }
            if (ad.AdItem != null)
            {
                var p = await _productService.GetProductById(ad.AdItem.ProductId);
                await _productService.DeleteProduct(p);
            }

            await _adRepository.DeleteAsync(ad);

            return new DeleteAdResult(false, string.Empty);
        }
    }
}
