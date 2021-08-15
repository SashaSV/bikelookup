using Grand.Domain.Customers;
using Grand.Domain.Localization;
using Grand.Domain.Ads;
using Grand.Domain.Stores;
using Grand.Web.Models.Ads;
using MediatR;

namespace Grand.Web.Features.Models.Ads
{
    public class GetReturnRequestAd : IRequest<ReturnRequestModelAd>
    {
        public Ad Ad { get; set; }
        public Language Language { get; set; }
        public Store Store { get; set; }
    }
}
