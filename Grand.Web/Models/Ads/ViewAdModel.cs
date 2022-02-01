using Grand.Core.Models;
using Grand.Domain.Common;
using Grand.Web.Models.Catalog;
using Grand.Web.Models.Common;
using Grand.Web.Models.Media;
using Grand.Web.Models.PrivateMessages;
using System;
using System.Collections.Generic;
using static Grand.Web.Models.Catalog.ProductDetailsModel;

namespace Grand.Web.Models.Ads
{
    public partial class ViewAdModel : BaseEntityModel
    {
        public ViewAdModel()
        {
            SelectedPaymentMethods = new List<string>();
            SelectedShippingMethods = new List<string>();
            PaymentMethodType = new List<PaymentsMethodType>();
            ShippingMethodType = new List<ShipmentMethodType>();
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
        public bool WithDocuments { get; set; }
        public int Mileage { get; set; }
        public bool IsAuction { get; set; }

        public VendorModel VendorModel { get; set; }
        public PrivateMessageChatModel PrivateMessageChatModel { get; set; }
        
        public IList<string> SelectedShippingMethods { get; set; }
        public IList<ShipmentMethodType> ShippingMethodType { get; set; }
        public IList<string> SelectedPaymentMethods { get; set; }
        public IList<PaymentsMethodType> PaymentMethodType { get; set; }

        #region Nested Classes
        public partial class PaymentsMethodType : BaseEntityModel
        {
            public string Name { get; set; }
        }

        public partial class ShipmentMethodType : BaseEntityModel
        {
            public string Name { get; set; }
        }
        #endregion
    }


}