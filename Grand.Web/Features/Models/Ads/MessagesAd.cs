using Grand.Web.Models.Ads;
using Grand.Web.Models.PrivateMessages;
using MediatR;

namespace Grand.Web.Features.Models.Ads
{
    public class MessagesAd : IRequest<PrivateMessageIndexModel>
    {
        public Domain.Ads.Ad Ad { get; set; }
        public Domain.Localization.Language Language { get; set; }
    }
}
