using Grand.Domain.Ads;
using MediatR;

namespace Grand.Services.Queries.Models.Ads
{
    public class IsReturnRequestAllowedQueryAd : IRequest<bool>
    {
        public Ad Ad { get; set; }
    }
}
