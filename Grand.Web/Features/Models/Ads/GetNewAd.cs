using Grand.Domain.Ads;
using Grand.Domain.Customers;
using Grand.Domain.Localization;
using Grand.Domain.Stores;
using Grand.Web.Models.Ads;
using Grand.Web.Models.Customer;
using MediatR;

namespace Grand.Web.Features.Models.Ads
{
    public class GetNewAd : IRequest<AdDetailsModel>
    {
        public Ad Ad { get; set; }
        public Customer Customer { get; set; }
        public Store Store { get; set; }
        public Language Language { get; set; }
        public bool ExcludeProperties { get; set; }
        public string OverrideCustomCustomerAttributesXml { get; set; } = "";
    }
}
