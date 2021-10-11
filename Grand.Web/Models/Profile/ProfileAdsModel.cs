using Grand.Web.Models.Ads;
using Grand.Web.Models.Common;
using System.Collections.Generic;

namespace Grand.Web.Models.Profile
{
    public partial class ProfileAdsModel
    {
        public IList<CustomerAdListModel.AdDetailsModel> Ads { get; set; }
        public PagerModel PagerModel { get; set; }
    }
}