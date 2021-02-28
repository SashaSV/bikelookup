﻿using Grand.Core.Models;
using System.Collections.Generic;
using Grand.Web.Models.Media;

namespace Grand.Web.Models.Catalog
{
    public partial class VendorNavigationModel : BaseModel
    {
        public VendorNavigationModel()
        {
            Vendors = new List<VendorBriefInfoModel>();
        }

        public IList<VendorBriefInfoModel> Vendors { get; set; }

        public int TotalVendors { get; set; }
    }

    public partial class VendorBriefInfoModel : BaseEntityModel
    {
       
        public int ApprovedRatingSum { get; set; }
        
        public int NotApprovedRatingSum { get; set; }
        
        public int ApprovedTotalReviews { get; set; }
        
        public int NotApprovedTotalReviews { get; set; }
                
        public string Name { get; set; }

        public string SeName { get; set; }
        
        public PictureModel PictureModel { get; set; }
    }
}