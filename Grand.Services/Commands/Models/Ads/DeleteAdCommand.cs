using Grand.Domain.Ads;
using MediatR;

namespace Grand.Services.Commands.Models.Ads
{
    public class DeleteAdCommand : IRequest<bool>
    {
        public Ad Ad { get; set; }
    }
}
