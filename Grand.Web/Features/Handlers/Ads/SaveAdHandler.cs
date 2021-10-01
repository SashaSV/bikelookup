using Grand.Domain.Ads;
using Grand.Domain.Customers;
using Grand.Domain.Data;
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

        public SaveAdHandler(IRepository<Ad> adRepository)
        {
            _adRepository = adRepository;
        }
        
        public async Task<bool>  Handle(SaveAd request, CancellationToken cancellationToken)
        {
            var ad = new Ad() {
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