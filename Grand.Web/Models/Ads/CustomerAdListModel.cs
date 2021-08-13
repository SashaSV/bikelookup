using Grand.Domain.Ads;
using Grand.Core.Models;
using System;
using System.Collections.Generic;

namespace Grand.Web.Models.Ads
{
    public partial class CustomerAdListModel : BaseModel
    {
        public CustomerAdListModel()
        {
            Ads = new List<AdDetailsModel>();
            RecurringAds = new List<RecurringAdsModel>();
            CancelRecurringPaymentErrors = new List<string>();
        }

        public IList<AdDetailsModel> Ads { get; set; }
        public IList<RecurringAdsModel> RecurringAds { get; set; }
        public IList<string> CancelRecurringPaymentErrors { get; set; }


        #region Nested classes

        public partial class AdDetailsModel : BaseEntityModel
        {
            public string AdTotal { get; set; }
            public bool IsReturnRequestAllowed { get; set; }
            public AdStatus AdStatusEnum { get; set; }
            public string AdStatus { get; set; }
            public string PaymentStatus { get; set; }
            public string ShippingStatus { get; set; }
            public DateTime CreatedOn { get; set; }
            public int AdNumber { get; set; }
            public string AdCode { get; set; }
            public string CustomerEmail { get; set; }
        }

        public partial class RecurringAdsModel : BaseEntityModel
        {
            public string StartDate { get; set; }
            public string CycleInfo { get; set; }
            public string NextPayment { get; set; }
            public int TotalCycles { get; set; }
            public int CyclesRemaining { get; set; }
            public string InitialAdId { get; set; }
            public bool CanCancel { get; set; }
        }

        #endregion
    }
}