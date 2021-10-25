using Grand.Core.Models;

namespace Grand.Web.Models.Catalog
{
    public partial class ProductSpecificationModel : BaseModel
    {
        public string SpecificationAttributeId { get; set; }

        public string SpecificationAttributeName { get; set; }

        public string SpecificationAttributeCode { get; set; }

        //this value is already HTML encoded
        public string ValueRaw { get; set; }
        public string ColorSquaresRgb { get; set; }

        public string DetailsUrl { get; set; }

        public bool ShowOnProductMainPage { get; set; }

        public bool ShowOnSellerMainPage { get; set; }

    }
}