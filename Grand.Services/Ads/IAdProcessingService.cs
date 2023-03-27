using Grand.Domain.Customers;
using Grand.Domain.Ads;
using Grand.Domain.Shipping;
using Grand.Services.Payments;
using System.Collections.Generic;
using System.Threading.Tasks;
using Grand.Domain.Orders;

namespace Grand.Services.Ads
{
    /// <summary>
    /// Ad processing service interface
    /// </summary>
    public partial interface IAdProcessingService
    {
        /// <summary>
        /// Send notification Ad 
        /// </summary>
        /// <param name="Ad">Ad</param>
        Task SendNotification(Ad Ad);


        /// <summary>
        /// Process next recurring psayment
        /// </summary>
        /// <param name="recurringPayment">Recurring payment</param>
        Task ProcessNextRecurringPayment(RecurringPayment recurringPayment);

        /// <summary>
        /// Cancels a recurring payment
        /// </summary>
        /// <param name="recurringPayment">Recurring payment</param>
        Task<IList<string>> CancelRecurringPayment(RecurringPayment recurringPayment);

        /// <summary>
        /// Gets a value indicating whether a customer can cancel recurring payment
        /// </summary>
        /// <param name="customerToValidate">Customer</param>
        /// <param name="recurringPayment">Recurring Payment</param>
        /// <returns>value indicating whether a customer can cancel recurring payment</returns>
        Task<bool> CanCancelRecurringPayment(Customer customerToValidate, RecurringPayment recurringPayment);

        /// <summary>
        /// Cancel a Ad
        /// </summary>
        /// <param name="Ad">Ad</param>
        /// <param name="notifyCustomer">Notify Customer</param>
        /// <param name="notifyStoreOwner">Notify StoreOwner</param>
        Task CancelAd(Ad Ad, bool notifyCustomer = true, bool notifyStoreOwner = true);

        /// <summary>
        /// Gets a value indicating whether cancel is allowed
        /// </summary>
        /// <param name="Ad">Ad</param>
        /// <returns>A value indicating whether cancel is allowed</returns>
        bool CanCancelAd(Ad Ad);
        
        /// <summary>
        /// Gets a value indicating whether Ad can be marked as authorized
        /// </summary>
        /// <param name="Ad">Ad</param>
        /// <returns>A value indicating whether Ad can be marked as authorized</returns>
        bool CanMarkAdAsAuthorized(Ad Ad);

        /// <summary>
        /// Marks Ad as authorized
        /// </summary>
        /// <param name="Ad">Ad</param>
        Task MarkAsAuthorized(Ad Ad);

        /// <summary>
        /// Gets a value indicating whether capture from admin panel is allowed
        /// </summary>
        /// <param name="Ad">Ad</param>
        /// <returns>A value indicating whether capture from admin panel is allowed</returns>
        Task<bool> CanCapture(Ad Ad);

        /// <summary>
        /// Capture an Ad (from admin panel)
        /// </summary>
        /// <param name="Ad">Ad</param>
        /// <returns>A list of errors; empty list if no errors</returns>
        Task<IList<string>> Capture(Ad Ad);

        /// <summary>
        /// Gets a value indicating whether Ad can be marked as paid
        /// </summary>
        /// <param name="Ad">Ad</param>
        /// <returns>A value indicating whether Ad can be marked as paid</returns>
        Task<bool> CanMarkAdAsPaid(Ad Ad);

        /// <summary>
        /// Marks Ad as paid
        /// </summary>
        /// <param name="Ad">Ad</param>
        Task MarkAdAsPaid(Ad Ad);

        /// <summary>
        /// Gets a value indicating whether refund from admin panel is allowed
        /// </summary>
        /// <param name="Ad">Ad</param>
        /// <returns>A value indicating whether refund from admin panel is allowed</returns>
        Task<bool> CanRefund(Ad Ad);

        /// <summary>
        /// Refunds an Ad (from admin panel)
        /// </summary>
        /// <param name="Ad">Ad</param>
        /// <returns>A list of errors; empty list if no errors</returns>
        Task<IList<string>> Refund(Ad Ad);

        /// <summary>
        /// Gets a value indicating whether Ad can be marked as refunded
        /// </summary>
        /// <param name="Ad">Ad</param>
        /// <returns>A value indicating whether Ad can be marked as refunded</returns>
        bool CanRefundOffline(Ad Ad);

        /// <summary>
        /// Refunds an Ad (offline)
        /// </summary>
        /// <param name="Ad">Ad</param>
        Task RefundOffline(Ad Ad);

        /// <summary>
        /// Gets a value indicating whether partial refund from admin panel is allowed
        /// </summary>
        /// <param name="Ad">Ad</param>
        /// <param name="amountToRefund">Amount to refund</param>
        /// <returns>A value indicating whether refund from admin panel is allowed</returns>
        Task<bool> CanPartiallyRefund(Ad Ad, decimal amountToRefund);

        /// <summary>
        /// Partially refunds an Ad (from admin panel)
        /// </summary>
        /// <param name="Ad">Ad</param>
        /// <param name="amountToRefund">Amount to refund</param>
        /// <returns>A list of errors; empty list if no errors</returns>
        Task<IList<string>> PartiallyRefund(Ad Ad, decimal amountToRefund);

        /// <summary>
        /// Gets a value indicating whether Ad can be marked as partially refunded
        /// </summary>
        /// <param name="Ad">Ad</param>
        /// <param name="amountToRefund">Amount to refund</param>
        /// <returns>A value indicating whether Ad can be marked as partially refunded</returns>
        bool CanPartiallyRefundOffline(Ad Ad, decimal amountToRefund);

        /// <summary>
        /// Partially refunds an Ad (offline)
        /// </summary>
        /// <param name="Ad">Ad</param>
        /// <param name="amountToRefund">Amount to refund</param>
        Task PartiallyRefundOffline(Ad Ad, decimal amountToRefund);

        /// <summary>
        /// Gets a value indicating whether void from admin panel is allowed
        /// </summary>
        /// <param name="Ad">Ad</param>
        /// <returns>A value indicating whether void from admin panel is allowed</returns>
        Task<bool> CanVoid(Ad Ad);

        /// <summary>
        /// Voids Ad (from admin panel)
        /// </summary>
        /// <param name="Ad">Ad</param>
        /// <returns>Voided Ad</returns>
        Task<IList<string>> Void(Ad Ad);

        /// <summary>
        /// Gets a value indicating whether Ad can be marked as voided
        /// </summary>
        /// <param name="Ad">Ad</param>
        /// <returns>A value indicating whether Ad can be marked as voided</returns>
        bool CanVoidOffline(Ad Ad);

        /// <summary>
        /// Voids Ad (offline)
        /// </summary>
        /// <param name="Ad">Ad</param>
        Task VoidOffline(Ad Ad);

        /// <summary>
        /// Valdiate minimum Ad sub-total amount
        /// </summary>
        /// <param name="cart">Shopping cart</param>
        /// <returns>true - OK; false - minimum Ad sub-total amount is not reached</returns>
        //Task<bool> ValidateMinAdSubtotalAmount(IList<ShoppingCartItem> cart);

        /// <summary>
        /// Validate Ad total amount
        /// </summary>
        /// <param name="customer">Customer</param>
        /// <param name="cart">Shopping cart</param>
        /// <returns>true - OK; false - minimum/maximum Ad total amount is not reached</returns>
        //Task<bool> ValidateAdTotalAmount(Customer customer, IList<ShoppingCartItem> cart);
    }
}
