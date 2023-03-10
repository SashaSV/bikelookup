using Grand.Core.Models;

namespace Grand.Web.Areas.Admin.Models.Vendors
{
    public class VendorSpecificationAttributeModel : BaseEntityModel
     {
            public int AttributeTypeId { get; set; }
    
            public string AttributeTypeName { get; set; }
    
            public string AttributeName { get; set; }
    
            public string AttributeId { get; set; }
    
            public string ValueRaw { get; set; }
    
            public bool AllowFiltering { get; set; }
    
            public bool ShowOnProductPage { get; set; }
    
            public int DisplayOrder { get; set; }
    
            public string ProductSpecificationId { get; set; }
    
            public string SpecificationAttributeOptionId { get; set; }

            public string DetailsUrl { get; set; }
            public string ProductId { get; set; }
        }
}
