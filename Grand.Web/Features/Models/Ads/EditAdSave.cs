using Grand.Web.Models.Ads;
using MediatR;

namespace Grand.Web.Features.Models.Ads
{
    public class EditAdSave : IRequest<bool>
    {
        public EditAdModel Model { get; set; }
    }
}
