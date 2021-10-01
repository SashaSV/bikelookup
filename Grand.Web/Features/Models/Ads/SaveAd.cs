using Grand.Domain.Customers;
using Grand.Domain.Stores;
using Grand.Web.Models.Ads;
using MediatR;

namespace Grand.Web.Features.Models.Ads
{
    public class SaveAd : IRequest<bool>
    {
        public Customer Customer { get; set; }
        
        public NewAdModel AdToSave { get; set; }
        
        public Store Store { get; set; }
    }
}