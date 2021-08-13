using Grand.Domain.Ads;
using Grand.Domain.Payments;
using Grand.Domain.Shipping;
using MediatR;
using MongoDB.Driver.Linq;
using System;

namespace Grand.Services.Queries.Models.Ads
{
    public class GetAdQuery : IRequest<IMongoQueryable<Ad>>
    {
        public string AdId { get; set; } = "";
        public string StoreId { get; set; } = "";
        public string VendorId { get; set; } = "";
        public string CustomerId { get; set; } = "";
        public string ProductId { get; set; } = "";
        public string AffiliateId { get; set; } = "";
        public string WarehouseId { get; set; } = "";
        public string BillingCountryId { get; set; } = "";
        public string OwnerId { get; set; } = "";
        public string PaymentMethodSystemName { get; set; } = null;
        public DateTime? CreatedFromUtc { get; set; } = null;
        public DateTime? CreatedToUtc { get; set; } = null;
        public AdStatus? Os { get; set; } = null;
        public PaymentStatus? Ps { get; set; } = null;
        public ShippingStatus? Ss { get; set; } = null;
        public string BillingEmail { get; set; } = null;
        public string BillingLastName { get; set; } = "";
        public string AdGuid { get; set; } = null;
        public string AdCode { get; set; } = null;
        public int PageIndex { get; set; } = 0;
        public int PageSize { get; set; } = int.MaxValue;
        public string AdTagId { get; set; } = "";
    }
}
