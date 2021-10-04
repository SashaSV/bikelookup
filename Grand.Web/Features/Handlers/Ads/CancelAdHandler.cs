using Grand.Domain.Ads;
using Grand.Domain.Data;
using Grand.Services.Commands.Models.Ads;
using Grand.Web.Features.Models.Ads;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Web.Features.Handlers.Ads
{
    public class CancelAdHandler :  IRequestHandler<CancelAdCommand, bool>
    {
        private readonly IRepository<Ad> _adRepository;
        
        public  CancelAdHandler(IRepository<Ad> adRepository)
        {
            _adRepository = adRepository;
        }
        public async Task<bool> Handle(CancelAdCommand request, CancellationToken cancellationToken)
        {
            request.Ad.AdStatus = AdStatus.Cancelled;
            await _adRepository.UpdateAsync(request.Ad);
            return false;
        }
    }
}