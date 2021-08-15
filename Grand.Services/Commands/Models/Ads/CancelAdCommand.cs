using Grand.Domain.Ads;
using MediatR;

namespace Grand.Services.Commands.Models.Ads
{
    public class CancelAdCommand : IRequest<bool>
    {
        public Ad Ad { get; set; }
        public bool NotifyCustomer { get; set; }
        public bool NotifyStoreOwner { get; set; } = false;
    }
}
