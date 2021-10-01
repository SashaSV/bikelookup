using Grand.Web.Models.Ads;
using MediatR;

namespace Grand.Web.Features.Models.Ads
{
    public class EditAd : IRequest<EditAdModel>
    {
        public Domain.Ads.Ad Ad { get; set; }
        public Domain.Localization.Language Language { get; set; }
    }
}
