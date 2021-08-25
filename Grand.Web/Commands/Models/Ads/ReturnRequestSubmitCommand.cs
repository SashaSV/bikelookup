using Grand.Domain.Common;
using Grand.Domain.Ads;
using Grand.Web.Models.Ads;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Grand.Web.Commands.Models.Ads
{
    public class ReturnRequestSubmitCommandAd : IRequest<(ReturnRequestModelAd model, ReturnRequestAd rr)>
    {
        public ReturnRequestModelAd Model { get; set; }
        public Ad Ad { get; set; }
        public Address Address { get; set; }
        public IFormCollection Form { get; set; }
    }
}
