using Grand.Domain.Ads;
using MediatR;
using System.Collections.Generic;

namespace Grand.Services.Commands.Models.Ads
{
    public class ReAdCommand : IRequest<IList<string>>
    {
        public Ad Ad { get; set; }
    }
}
