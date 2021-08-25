using Grand.Domain.Ads;
using Grand.Domain.Data;
using Grand.Services.Queries.Models.Ads;
using MediatR;
using MongoDB.Driver.Linq;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Services.Queries.Handlers.Ads
{
    public class GetAddQueryHandler : IRequestHandler<GetAdQuery, IMongoQueryable<Ad>>
    {
        private readonly IRepository<Ad> _adRepository;

        public GetAddQueryHandler(IRepository<Ad> adRepository)
        {
            _adRepository = adRepository;
        }

        public Task<IMongoQueryable<Ad>> Handle(GetAdQuery request, CancellationToken cancellationToken)
        {
            
            int? orderStatusId = null;

            if (request.Os.HasValue)
                orderStatusId = (int)request.Os.Value;

            int? paymentStatusId = null;
            if (request.Ps.HasValue)
                paymentStatusId = (int)request.Ps.Value;

            int? shippingStatusId = null;
            if (request.Ss.HasValue)
                shippingStatusId = (int)request.Ss.Value;

            var query = _adRepository.Table;
            if (!string.IsNullOrEmpty(request.AdId))
                query = query.Where(o => o.Id == request.AdId);

            if (!string.IsNullOrEmpty(request.StoreId))
                query = query.Where(o => o.StoreId == request.StoreId);

            if (!string.IsNullOrEmpty(request.VendorId))
            {
                query = query
                    .Where(o => o.AdItems
                    .Any(orderItem => orderItem.VendorId == request.VendorId));
            }

            if (!string.IsNullOrEmpty(request.CustomerId))
                query = query.Where(o => o.CustomerId == request.CustomerId);

            if (!string.IsNullOrEmpty(request.ProductId))
            {
                query = query
                    .Where(o => o.AdItems
                    .Any(orderItem => orderItem.ProductId == request.ProductId));
            }
            if (!string.IsNullOrEmpty(request.WarehouseId))
            {
                query = query
                    .Where(o => o.AdItems
                    .Any(orderItem =>
                        orderItem.WarehouseId == request.WarehouseId
                        ));
            }
            if (!string.IsNullOrEmpty(request.BillingCountryId))
                query = query.Where(o => o.BillingAddress != null && o.BillingAddress.CountryId == request.BillingCountryId);

            if (!string.IsNullOrEmpty(request.PaymentMethodSystemName))
                query = query.Where(o => o.PaymentMethodSystemName == request.PaymentMethodSystemName);

            if (!string.IsNullOrEmpty(request.AffiliateId))
                query = query.Where(o => o.AffiliateId == request.AffiliateId);

            if (!string.IsNullOrEmpty(request.OwnerId))
                query = query.Where(o => o.OwnerId == request.OwnerId);

            if (request.CreatedFromUtc.HasValue)
                query = query.Where(o => request.CreatedFromUtc.Value <= o.CreatedOnUtc);

            if (request.CreatedToUtc.HasValue)
                query = query.Where(o => request.CreatedToUtc.Value >= o.CreatedOnUtc);

            if (orderStatusId.HasValue)
                query = query.Where(o => orderStatusId.Value == o.AdStatusId);

            if (paymentStatusId.HasValue)
                query = query.Where(o => paymentStatusId.Value == o.PaymentStatusId);

            if (shippingStatusId.HasValue)
                query = query.Where(o => shippingStatusId.Value == o.ShippingStatusId);

            if (!string.IsNullOrEmpty(request.BillingEmail))
                query = query.Where(o => o.BillingAddress != null && o.BillingAddress.Email == request.BillingEmail);

            if (!string.IsNullOrEmpty(request.BillingLastName))
                query = query.Where(o => o.BillingAddress != null && o.BillingAddress.LastName.Contains(request.BillingLastName));

            if (!string.IsNullOrEmpty(request.AdGuid))
            {
                if(Guid.TryParse(request.AdGuid, out Guid orderguid))
                    query = query.Where(o => o.AdGuid == orderguid); 
            }
            if (!string.IsNullOrEmpty(request.AdCode))
            {
                query = query.Where(o => o.Code == request.AdCode.ToUpperInvariant());
            }

            //tag filtering 
            if (!string.IsNullOrEmpty(request.AdTagId))
                query = query.Where(o => o.AdTags.Any(y => y == request.AdTagId));

            query = query.Where(o => !o.Deleted);
            query = query.OrderByDescending(o => o.CreatedOnUtc);

            return Task.FromResult(query);
        }
    }
}
