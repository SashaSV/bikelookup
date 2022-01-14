using Grand.Core.Models;
using Grand.Domain.Common;
using Grand.Web.Models.Common;
using Grand.Web.Models.Media;
using System;
using System.Collections.Generic;
using static Grand.Web.Models.Catalog.ProductDetailsModel;

namespace Grand.Web.Models.Ads
{
    public partial class ViewAdModel : BaseEntityModel
    {
        public ViewAdModel()
        {
        }

        public string AdPructName;
        public bool DefaultPictureZoomEnabled { get; set; }
        public PictureModel DefaultPictureModel { get; set; }
        public IList<PictureModel> PictureModels { get; set; }
        public ProductBreadcrumbModel Breadcrumb { get; set; }
        public int AdNumber { get; set; }
        public DateTime CreatedOnUtc { get; set; }
        public string AdComment { get; set; }
        public decimal Price { get; set; }
        public string AdTotal { get; set; }
        public Address CustomerAddress { get; set; }
        

    }
}