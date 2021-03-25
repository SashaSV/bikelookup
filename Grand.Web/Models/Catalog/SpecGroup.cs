using Grand.Core.Models;
using System.Collections.Generic;

namespace Grand.Web.Models.Catalog
{
    public class SpecGroupModel : BaseModel
    {
        public  IList<ProductSpecificationModel> Specs{ get; set; }

        public string Icon { get; set; }

        public string Name { get; set; }
    }
} 