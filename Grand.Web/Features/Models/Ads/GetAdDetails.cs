using Grand.Web.Models.Orders;
using MediatR;

namespace Grand.Web.Features.Models.Ads
{
    public class GetAdDetails : IRequest<OrderDetailsModel>
    {
        public Domain.Ads.Ad Ad { get; set; }
        public Domain.Localization.Language Language { get; set; }
    }
}
