using System.Collections.Generic;
using Grand.Domain.Vendors;
using MediatR;
using Grand.Domain.Localization;
using Grand.Web.Models.Catalog;

namespace Grand.Web.Features.Models.Vendors
{
    public class GetVendorSpecifications : IRequest<IList<ProductSpecificationModel>>
    {
        public Vendor Vendor { get; set; }
        
         public Language Language { get; set; }
    }
}