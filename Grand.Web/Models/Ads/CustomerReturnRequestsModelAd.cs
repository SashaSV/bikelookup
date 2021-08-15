using Grand.Core.Models;
using System;
using System.Collections.Generic;

namespace Grand.Web.Models.Ads
{
    public partial class CustomerReturnRequestsModelAd : BaseModel
    {
        public CustomerReturnRequestsModelAd()
        {
            Items = new List<ReturnRequestModelAd>();
        }

        public IList<ReturnRequestModelAd> Items { get; set; }

        #region Nested classes
        public partial class ReturnRequestModelAd : BaseEntityModel
        {
            public int ReturnNumber { get; set; }
            public string ReturnRequestStatus { get; set; }
            public DateTime CreatedOn { get; set; }
            public int ProductsCount { get; set; }
            public string ReturnTotal { get; set; }
        }
        #endregion
    }
}