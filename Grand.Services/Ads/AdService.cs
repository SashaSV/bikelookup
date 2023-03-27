using Grand.Domain;
using Grand.Domain.Data;
using Grand.Domain.Catalog;
using Grand.Domain.Ads;
using Grand.Domain.Payments;
using Grand.Domain.Shipping;
using Grand.Services.Events;
using Grand.Services.Queries.Models.Ads;
using MediatR;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grand.Services.Commands.Models.Ads;
using Grand.Domain.Orders;
using Grand.Domain.Vendors;
using Grand.Services.Vendors;
using Grand.Services.Customers;

namespace Grand.Services.Ads
{
    /// <summary>
    /// Ad service
    /// </summary>
    public partial class AdService : IAdService
    {
        #region Fields

        private readonly IRepository<Ad> _adRepository;
        private readonly IRepository<AdNote> _adNoteRepository;
        private readonly IRepository<ProductAlsoPurchased> _productAlsoPurchasedRepository;
        //private readonly IRepository<RecurringPayment> _recurringPaymentRepository;
        private readonly IMediator _mediator;
        private readonly IVendorService _vendorService;
        private readonly ICustomerService _customerService;
        #endregion

        #region Ctor

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="AdRepository">Ad repository</param>
        /// <param name="AdNoteRepository">Ad note repository</param>
        /// <param name="recurringPaymentRepository">Recurring payment repository</param>
        /// <param name="productAlsoPurchasedRepository">Product also purchased repository</param>
        /// <param name="mediator">Mediator</param>
        public AdService(IRepository<Ad> adRepository,
            IRepository<AdNote> adNoteRepository,
            //IRepository<RecurringPayment> recurringPaymentRepository,
            IRepository<ProductAlsoPurchased> productAlsoPurchasedRepository,
            IMediator mediator, IVendorService vendorService, ICustomerService customerService
            )
        {
            _adRepository = adRepository;
            _adNoteRepository = adNoteRepository;
            //_recurringPaymentRepository = recurringPaymentRepository;
            _mediator = mediator;
            _productAlsoPurchasedRepository = productAlsoPurchasedRepository;
            _vendorService = vendorService;
            _customerService = customerService;
        }

        #endregion

        #region Methods

        #region Ads

        /// <summary>
        /// Gets an Ad
        /// </summary>
        /// <param name="AdId">The Ad identifier</param>
        /// <returns>Ad</returns>
        public virtual Task<Ad> GetAdById(string AdId)
        {
            return _adRepository.GetByIdAsync(AdId);
        }

        public async Task UpdateMostView(string adId)
        {
            var update = new UpdateDefinitionBuilder<Ad>().Inc(x => x.Viewed, 1);
             await _adRepository.Collection.UpdateManyAsync(x => x.Id == adId, update);
        }
        
        /// <summary>
        /// Gets an ad
        /// </summary>
        /// <param name="adId">The ad item identifier</param>
        /// <returns>Ad</returns>
        public virtual Task<Ad> GetAdByAdItemId(string adItemId)
        {
            var query = from o in _adRepository.Table
                        where o.AdItems.Any(x => x.Id == adItemId)
                        select o;

            return query.FirstOrDefaultAsync();
        }
        /// <summary>
        /// Gets an ad
        /// </summary>
        /// <param name="adNumber">The ad number</param>
        /// <returns>Ad</returns>
        public virtual Task<Ad> GetAdByNumber(int adNumber)
        {
            return _adRepository.Table.FirstOrDefaultAsync(x => x.AdNumber == adNumber);
        }

        /// <summary>
        /// Gets ads by code
        /// </summary>
        /// <param name="code">The ad code</param>
        /// <returns>Ad</returns>
        public virtual async Task<IList<Ad>> GetAdsByCode(string code)
        {
            if (string.IsNullOrEmpty(code))
                return new List<Ad>();

            return await _adRepository.Table.Where(x => x.Code == code.ToUpperInvariant()).ToListAsync();
        }


        /// <summary>
        /// Get ads by identifiers
        /// </summary>
        /// <param name="adIds">Ad identifiers</param>
        /// <returns>Ad</returns>
        public virtual async Task<IList<Ad>> GetAdsByIds(string[] adIds)
        {
            if (adIds == null || adIds.Length == 0)
                return new List<Ad>();

            var query = from o in _adRepository.Table
                        where adIds.Contains(o.Id)
                        select o;
            var ads = await query.ToListAsync();
            //sort by passed identifiers
            var sortedAds = new List<Ad>();
            foreach (string id in adIds)
            {
                var ad = ads.Find(x => x.Id == id);
                if (ad != null)
                    sortedAds.Add(ad);
            }
            return sortedAds;
        }

        /// <summary>
        /// Gets an ad
        /// </summary>
        /// <param name="adGuid">The ad identifier</param>
        /// <returns>Ad</returns>
        public virtual Task<Ad> GetAdByGuid(Guid adGuid)
        {
            var query = from o in _adRepository.Table
                        where o.AdGuid == adGuid
                        select o;
            return query.FirstOrDefaultAsync();
        }

        /// <summary>
        /// Deletes an ad
        /// </summary>
        /// <param name="ad">The ad</param>
        public virtual async Task DeleteAd(Ad ad)
        {
            if (ad == null)
                throw new ArgumentNullException("ad");

            ad.Deleted = true;
            await UpdateAd(ad);

            //delete product also purchased
            var filters = Builders<ProductAlsoPurchased>.Filter;
            var filter = filters.Where(x => x.AdId == ad.Id);
            await _productAlsoPurchasedRepository.Collection.DeleteManyAsync(filter);

        }

        /// <summary>
        /// Search ads
        /// </summary>
        /// <param name="storeId">Store identifier; 0 to load all ads</param>
        /// <param name="vendorId">Vendor identifier; null to load all ads</param>
        /// <param name="customerId">Customer identifier; 0 to load all ads</param>
        /// <param name="productId">Product identifier which was purchased in an ad; 0 to load all ads</param>
        /// <param name="affiliateId">Affiliate identifier; 0 to load all ads</param>
        /// <param name="warehouseId">Warehouse identifier, only ads with products from a specified warehouse will be loaded; 0 to load all ads</param>
        /// <param name="billingCountryId">Billing country identifier; 0 to load all ads</param>
        /// <param name="ownerId">Owner identifier</param>
        /// <param name="paymentMethodSystemName">Payment method system name; null to load all records</param>
        /// <param name="createdFromUtc">Created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Created date to (UTC); null to load all records</param>
        /// <param name="os">Ad status; null to load all ads</param>
        /// <param name="ps">Ad payment status; null to load all ads</param>
        /// <param name="ss">Ad shipment status; null to load all ads</param>
        /// <param name="billingEmail">Billing email. Leave empty to load all records.</param>
        /// <param name="adNotes">Search in ad notes. Leave empty to load all records.</param>
        /// <param name="adGuid">Search by ad GUID (Global unique identifier) or part of GUID. Leave empty to load all ads.</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="adTagId">Ad tag identifier</param>
        /// <returns>Ads</returns>
        public virtual async Task<IPagedList<Ad>> SearchAds(string storeId = "",
            string vendorId = "", string customerId = "",
            string productId = "", string affiliateId = "", string warehouseId = "",
            string billingCountryId = "", string ownerId = "", string paymentMethodSystemName = null,
            DateTime? createdFromUtc = null, DateTime? createdToUtc = null,
            AdStatus? os = null, PaymentStatus? ps = null, ShippingStatus? ss = null,
            string billingEmail = null, string billingLastName = "", string adGuid = null,
            string adCode = null, int pageIndex = 0, int pageSize = int.MaxValue, string adTagId = "")
        {
            var querymodel = new GetAdQuery() {
                AffiliateId = affiliateId,
                BillingCountryId = billingCountryId,
                BillingEmail = billingEmail,
                BillingLastName = billingLastName,
                CreatedFromUtc = createdFromUtc,
                CreatedToUtc = createdToUtc,
                CustomerId = customerId,
                AdGuid = adGuid,
                AdCode = adCode,
                Os = os,
                PageIndex = pageIndex,
                PageSize = pageSize,
                PaymentMethodSystemName = paymentMethodSystemName,
                ProductId = productId,
                Ps = ps,
                Ss = ss,
                StoreId = storeId,
                VendorId = vendorId,
                WarehouseId = warehouseId,
                AdTagId = adTagId,
                OwnerId = ownerId
            };
            var query = await _mediator.Send(querymodel);
            return await PagedList<Ad>.Create(query, pageIndex, pageSize);
        }

        /// <summary>
        /// Inserts an ad
        /// </summary>
        /// <param name="ad">Ad</param>
        public virtual async Task<Ad> InsertAd(Ad ad)
        {
            if (ad == null)
                throw new ArgumentNullException("ad");

            var adExists = _adRepository.Table.OrderByDescending(x => x.AdNumber).Select(x => x.AdNumber).FirstOrDefault();
            ad.AdNumber = adExists != 0 ? adExists + 1 : 1;

            var retAd = await _adRepository.InsertAsync(ad);

            //event notification
            await _mediator.EntityInserted(ad);
            return retAd;
        }

        /// <summary>
        /// Inserts an product also purchased
        /// </summary>
        /// <param name="ad">Ad</param>
        public virtual async Task InsertProductAlsoPurchased(Ad ad)
        {
            if (ad == null)
                throw new ArgumentNullException("ad");

            foreach (var item in ad.AdItems)
            {
                foreach (var it in ad.AdItems.Where(x => x.ProductId != item.ProductId))
                {
                    var productPurchase = new ProductAlsoPurchased();
                    productPurchase.ProductId = item.ProductId;
                    //productPurchase.AdId = ad.Id;
                    //productPurchase.CreatedAdOnUtc = ad.CreatedOnUtc;
                    productPurchase.Quantity = it.Quantity;
                    productPurchase.StoreId = ad.StoreId;
                    productPurchase.ProductId2 = it.ProductId;
                    await _productAlsoPurchasedRepository.InsertAsync(productPurchase);
                }
            }
        }

        /// <summary>
        /// Updates the ad
        /// </summary>
        /// <param name="ad">The ad</param>
        public virtual async Task UpdateAd(Ad ad)
        {
            if (ad == null)
                throw new ArgumentNullException("ad");

            await _adRepository.UpdateAsync(ad);

            //event notification
            await _mediator.EntityUpdated(ad);
        }

        /// <summary>
        /// Get an ad by authorization transaction ID and payment method system name
        /// </summary>
        /// <param name="authorizationTransactionId">Authorization transaction ID</param>
        /// <param name="paymentMethodSystemName">Payment method system name</param>
        /// <returns>Ad</returns>
        public virtual Task<Ad> GetAdByAuthorizationTransactionIdAndPaymentMethod
            (string authorizationTransactionId,
            string paymentMethodSystemName)
        {
            var query = _adRepository.Table;

            if (!string.IsNullOrWhiteSpace(authorizationTransactionId))
                query = query.Where(o => o.AuthorizationTransactionId == authorizationTransactionId);

            if (!string.IsNullOrWhiteSpace(paymentMethodSystemName))
                query = query.Where(o => o.PaymentMethodSystemName == paymentMethodSystemName);

            //query = query.AdByDescending(o => o.CreatedOnUtc);
            return query.FirstOrDefaultAsync();
        }

        public virtual async Task<Vendor> GetVendorByAd(Ad ad) 
        {
            var userId = ad.CustomerId ?? ad.OwnerId;
            var customerAd = await _customerService.GetCustomerById(userId);
            var vendor = await _vendorService.GetVendorByEmail(customerAd.Email, null);
            return vendor;
        }

        /// <summary>
        /// Cancel UnPaid Ads and has pending status
        /// </summary>
        /// <param name="expirationDateUTC">Date at which all unPaid ads and has pending status Would be Canceled</param>
        public async Task CancelExpiredAds(DateTime expirationDateUTC)
        {
            var ads = await _adRepository.Table
              .Where(o =>
              o.CreatedOnUtc < expirationDateUTC &&
              o.PaymentStatusId == (int)PaymentStatus.Pending &&
              //o.AdStatusId == (int)AdStatus.Pending &&
              (o.ShippingStatusId == (int)ShippingStatus.NotYetShipped || o.ShippingStatusId == (int)ShippingStatus.ShippingNotRequired))
              .ToListAsync();
            
            foreach (var ad in ads)
                await _mediator.Send(new CancelAdCommand() { Ad = ad, NotifyCustomer = true });
        }

        #endregion

        #region Ads items

        /// <summary>
        /// Gets an item
        /// </summary>
        /// <param name="adItemGuid">Ad identifier</param>
        /// <returns>Ad item</returns>
        public virtual Task<AdItem> GetAdItemByGuid(Guid adItemGuid)
        {
            var query = from ad in _adRepository.Table
                        from adItem in ad.AdItems
                        select adItem;

            query = from adItem in query
                    where adItem.AdItemGuid == adItemGuid
                    select adItem;

            return query.FirstOrDefaultAsync();
        }

        /// <summary>
        /// Delete an ad item
        /// </summary>
        /// <param name="adItem">The ad item</param>
        public virtual async Task DeleteAdItem(AdItem adItem)
        {
            if (adItem == null)
                throw new ArgumentNullException("adItem");

            var ad = await GetAdByAdItemId(adItem.Id);

            var updatebuilder = Builders<Ad>.Update;
            var updatefilter = updatebuilder.PullFilter(x => x.AdItems, y => y.Id == adItem.Id);
            await _adRepository.Collection.UpdateOneAsync(new BsonDocument("_id", ad.Id), updatefilter);

            var filters = Builders<ProductAlsoPurchased>.Filter;
            var filter = filters.Where(x => x.AdId == ad.Id && (x.ProductId == adItem.ProductId || x.ProductId2 == adItem.ProductId));
            await _productAlsoPurchasedRepository.Collection.DeleteManyAsync(filter);

            //event notification
            await _mediator.EntityDeleted(adItem);
        }

        #endregion

        #region Ads notes

        /// <summary>
        /// Deletes an ad note
        /// </summary>
        /// <param name="adNote">The ad note</param>
        public virtual async Task DeleteAdNote(AdNote adNote)
        {
            if (adNote == null)
                throw new ArgumentNullException("adNote");

            await _adNoteRepository.DeleteAsync(adNote);

            //event notification
            await _mediator.EntityDeleted(adNote);
        }

        /// <summary>
        /// Deletes an ad note
        /// </summary>
        /// <param name="adNote">The ad note</param>
        public virtual async Task InsertAdNote(AdNote adNote)
        {
            if (adNote == null)
                throw new ArgumentNullException("adNote");

            await _adNoteRepository.InsertAsync(adNote);

            //event notification
            await _mediator.EntityInserted(adNote);
        }

        public virtual async Task<IList<AdNote>> GetAdNotes(string adId)
        {
            var query = from adNote in _adNoteRepository.Table
                        where adNote.AdId == adId
                        orderby adNote.CreatedOnUtc descending
                        select adNote;

            return await query.ToListAsync();
        }

        /// <summary>
        /// Get adnote by id
        /// </summary>
        /// <param name="adnoteId">Ad note identifier</param>
        /// <returns>AdNote</returns>
        public virtual Task<AdNote> GetAdNote(string adnoteId)
        {
            return _adNoteRepository.Table.Where(x => x.Id == adnoteId).FirstOrDefaultAsync();
        }


        #endregion

        #region Recurring payments

        ///// <summary>
        ///// Deletes a recurring payment
        ///// </summary>
        ///// <param name="recurringPayment">Recurring payment</param>
        //public virtual async Task DeleteRecurringPayment(RecurringPayment recurringPayment)
        //{
        //    if (recurringPayment == null)
        //        throw new ArgumentNullException("recurringPayment");

        //    recurringPayment.Deleted = true;
        //    await UpdateRecurringPayment(recurringPayment);
        //}

        ///// <summary>
        ///// Gets a recurring payment
        ///// </summary>
        ///// <param name="recurringPaymentId">The recurring payment identifier</param>
        ///// <returns>Recurring payment</returns>
        //public virtual Task<RecurringPayment> GetRecurringPaymentById(string recurringPaymentId)
        //{
        //    return _recurringPaymentRepository.GetByIdAsync(recurringPaymentId);
        //}

        ///// <summary>
        ///// Inserts a recurring payment
        ///// </summary>
        ///// <param name="recurringPayment">Recurring payment</param>
        //public virtual async Task InsertRecurringPayment(RecurringPayment recurringPayment)
        //{
        //    if (recurringPayment == null)
        //        throw new ArgumentNullException("recurringPayment");

        //    await _recurringPaymentRepository.InsertAsync(recurringPayment);

        //    //event notification
        //    await _mediator.EntityInserted(recurringPayment);
        //}

        ///// <summary>
        ///// Updates the recurring payment
        ///// </summary>
        ///// <param name="recurringPayment">Recurring payment</param>
        //public virtual async Task UpdateRecurringPayment(RecurringPayment recurringPayment)
        //{
        //    if (recurringPayment == null)
        //        throw new ArgumentNullException("recurringPayment");

        //    await _recurringPaymentRepository.UpdateAsync(recurringPayment);

        //    //event notification
        //    await _mediator.EntityUpdated(recurringPayment);
        //}

        /// <summary>
        /// Search recurring payments
        /// </summary>
        /// <param name="storeId">The store identifier; "" to load all records</param>
        /// <param name="customerId">The customer identifier; "" to load all records</param>
        /// <param name="initialAdId">The initial ad identifier; "" to load all records</param>
        /// <param name="initialAdStatus">Initial ad status identifier; null to load all records</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>Recurring payments</returns>
        //public virtual async Task<IPagedList<RecurringPayment>> SearchRecurringPayments(string storeId = "",
        //    string customerId = "", string initialAdId = "", AdStatus? initialAdStatus = null,
        //    int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false)
        //{
        //    //int? initialAdStatusId = null;
        //    //if (initialAdStatus.HasValue)
        //    //    initialAdStatusId = (int)initialAdStatus.Value;
        //    ////TO DO
        //    //var query1 = from rp in _recurringPaymentRepository.Table
        //    //             where
        //    //             (!rp.Deleted) &&
        //    //             (showHidden || rp.IsActive) &&
        //    //             (customerId == "" || rp.InitialAd.CustomerId == customerId) &&
        //    //             (storeId == "" || rp.InitialAd.StoreId == storeId) &&
        //    //             (initialAdId == "" || rp.InitialAd.Id == initialAdId)
        //    //             select rp.Id;
        //    //var cc = query1.ToList();
        //    //var query2 = from rp in _recurringPaymentRepository.Table
        //    //             where cc.Contains(rp.Id)
        //    //             adby rp.StartDateUtc
        //    //             select rp;
        //    //return await PagedList<RecurringPayment>.Create(query2, pageIndex, pageSize);
        //    return null;
        //}



        #endregion

        #endregion
    }
}
