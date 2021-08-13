using Grand.Domain;
using Grand.Domain.Ads;
using Grand.Domain.Orders;
using Grand.Domain.Payments;
using Grand.Domain.Shipping;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Grand.Services.Ads
{
    /// <summary>
    /// Ads service interface
    /// </summary>
    public partial interface IAdsService
    {
        #region Ads

        /// <summary>
        /// Gets an Ads
        /// </summary>
        /// <param name="AdsId">The Ads identifier</param>
        /// <returns>Ads</returns>
        Task<Ad> GetAdsById(string adsId);

        /// <summary>
        /// Gets an Ads
        /// </summary>
        /// <param name="AdsItemId">The Ads item identifier</param>
        /// <returns>Ads</returns>
        Task<Ad> GetAdsByAdsItemId(string AdsItemId);

        /// <summary>
        /// Gets an Ads
        /// </summary>
        /// <param name="AdsNumber">The Ads number</param>
        /// <returns>Ads</returns>
        Task<Ad> GetAdsByNumber(int AdsNumber);

        /// <summary>
        /// Gets Adss by code
        /// </summary>
        /// <param name="code">The Ads code</param>
        /// <returns>Ads</returns>
        Task<IList<Ad>> GetAdssByCode(string code);

        /// <summary>
        /// Get Adss by identifiers
        /// </summary>
        /// <param name="AdsIds">Ads identifiers</param>
        /// <returns>Ads</returns>
        Task<IList<Ad>> GetAdssByIds(string[] AdsIds);

        /// <summary>
        /// Gets an Ads
        /// </summary>
        /// <param name="AdsGuid">The Ads identifier</param>
        /// <returns>Ads</returns>
        Task<Ad> GetAdsByGuid(Guid AdsGuid);

        /// <summary>
        /// Deletes an Ads
        /// </summary>
        /// <param name="Ads">The Ads</param>
        Task DeleteAds(Ad Ads);

        /// <summary>
        /// Search Adss
        /// </summary>
        /// <param name="storeId">Store identifier; null to load all Adss</param>
        /// <param name="vendorId">Vendor identifier; null to load all Adss</param>
        /// <param name="customerId">Customer identifier; null to load all Adss</param>
        /// <param name="productId">Product identifier which was purchased in an Ads; 0 to load all Adss</param>
        /// <param name="affiliateId">Affiliate identifier; 0 to load all Adss</param>
        /// <param name="warehouseId">Warehouse identifier, only Adss with products from a specified warehouse will be loaded; 0 to load all Adss</param>
        /// <param name="billingCountryId">Billing country identifier; 0 to load all Adss</param>
        /// <param name="ownerId">Owner identifier</param>
        /// <param name="paymentMethodSystemName">Payment method system name; null to load all records</param>
        /// <param name="createdFromUtc">Created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Created date to (UTC); null to load all records</param>
        /// <param name="os">Ads status; null to load all Adss</param>
        /// <param name="ps">Ads payment status; null to load all Adss</param>
        /// <param name="ss">Ads shipment status; null to load all Adss</param>
        /// <param name="billingEmail">Billing email. Leave empty to load all records.</param>
        /// <param name="AdsGuid">Search by Ads GUID (Global unique identifier) or part of GUID. Leave empty to load all records.</param>
        /// <param name="AdsCode">Search by Ads code.</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="AdsTagId">Ads tag identifier</param>
        /// <returns>Adss</returns>
        Task<IPagedList<Ad>> SearchAdss(string storeId = "",
            string vendorId = "", string customerId = "",
            string productId = "", string affiliateId = "", string warehouseId = "",
            string billingCountryId = "", string ownerId = "", string paymentMethodSystemName = null,
            DateTime? createdFromUtc = null, DateTime? createdToUtc = null,
            AdStatus? os = null, PaymentStatus? ps = null, ShippingStatus? ss = null,
            string billingEmail = null, string billingLastName = "", string AdsGuid = null,
            string AdsCode = null, int pageIndex = 0, int pageSize = int.MaxValue, string AdsTagId = "");
        
        /// <summary>
        /// Inserts an Ads
        /// </summary>
        /// <param name="Ad">Ads</param>
        Task InsertAds(Ad Ad);

        /// <summary>
        /// Inserts an product also purchased
        /// </summary>
        /// <param name="Ads">Ads</param>
        Task InsertProductAlsoPurchased(Ad Ad);

        /// <summary>
        /// Updates the Ads
        /// </summary>
        /// <param name="Ad">The Ads</param>
        Task UpdateAds(Ad Ad);

        /// <summary>
        /// Get an Ads by authorization transaction ID and payment method system name
        /// </summary>
        /// <param name="authorizationTransactionId">Authorization transaction ID</param>
        /// <param name="paymentMethodSystemName">Payment method system name</param>
        /// <returns>Ads</returns>
        Task<Ad> GetAdsByAuthorizationTransactionIdAndPaymentMethod(string authorizationTransactionId, string paymentMethodSystemName);

        #endregion

        #region Adss items

        /// <summary>
        /// Gets an Ads item
        /// </summary>
        /// <param name="AdsItemGuid">Ads item identifier</param>
        /// <returns>Ads item</returns>
        Task<AdsItem> GetAdsItemByGuid(Guid AdsItemGuid);

        /// <summary>
        /// Delete an Ads item
        /// </summary>
        /// <param name="AdsItem">The Ads item</param>
        Task DeleteAdsItem(AdsItem AdsItem);

        #endregion

        #region Ads notes

        /// <summary>
        /// Deletes an Ads note
        /// </summary>
        /// <param name="AdsNote">The Ads note</param>
        Task DeleteAdsNote(AdNote AdsNote);

        /// <summary>
        /// Insert an Ads note
        /// </summary>
        /// <param name="AdsNote">The Ads note</param>
        Task InsertAdsNote(AdNote AdsNote);


        /// <summary>
        /// Get Adsnotes for Ads
        /// </summary>
        /// <param name="AdsId">Ads identifier</param>
        /// <returns>AdsNote</returns>
        Task<IList<AdNote>> GetAdsNotes(string AdsId);

        /// <summary>
        /// Get Adsnote by id
        /// </summary>
        /// <param name="AdsnoteId">Ads note identifier</param>
        /// <returns>AdsNote</returns>
        Task<AdNote> GetAdsNote(string AdsnoteId);


        /// <summary>
        /// Cancel Expired UnPaid Adss
        /// </summary>
        /// <param name="expirationDateUTC">Date at which all unPaid  Adss and has pending status Would be Canceled</param>
        Task CancelExpiredAdss(DateTime expirationDateUTC);

        #endregion

        #region Recurring payments

        /// <summary>
        /// Deletes a recurring payment
        /// </summary>
        /// <param name="recurringPayment">Recurring payment</param>
        Task DeleteRecurringPayment(RecurringPayment recurringPayment);

        /// <summary>
        /// Gets a recurring payment
        /// </summary>
        /// <param name="recurringPaymentId">The recurring payment identifier</param>
        /// <returns>Recurring payment</returns>
        Task<RecurringPayment> GetRecurringPaymentById(string recurringPaymentId);

        /// <summary>
        /// Inserts a recurring payment
        /// </summary>
        /// <param name="recurringPayment">Recurring payment</param>
        Task InsertRecurringPayment(RecurringPayment recurringPayment);

        /// <summary>
        /// Updates the recurring payment
        /// </summary>
        /// <param name="recurringPayment">Recurring payment</param>
        Task UpdateRecurringPayment(RecurringPayment recurringPayment);

        /// <summary>
        /// Search recurring payments
        /// </summary>
        /// <param name="storeId">The store identifier; "" to load all records</param>
        /// <param name="customerId">The customer identifier; "" to load all records</param>
        /// <param name="initialAdsId">The initial Ads identifier; "" to load all records</param>
        /// <param name="initialAdsStatus">Initial Ads status identifier; null to load all records</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>Recurring payments</returns>
        Task<IPagedList<RecurringPayment>> SearchRecurringPayments(string storeId = "",
            string customerId = "", string initialAdsId = "", AdStatus? initialAdsStatus = null,
            int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false);



        #endregion
    }
}
