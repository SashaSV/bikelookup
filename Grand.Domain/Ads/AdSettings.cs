using Grand.Domain.Configuration;

namespace Grand.Domain.Ads
{
    public class AdSettings : ISettings
    {
        /// <summary>
        /// Gets or sets a value indicating whether customer can make re-Ad
        /// </summary>
        public bool IsReAdsAllowed { get; set; }

        /// <summary>
        /// Gets or sets a minimum Ad subtotal amount
        /// </summary>
        public decimal MinAdsSubtotalAmount { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether 'inimum Ad subtotal amount' option
        /// should be evaluated over 'X' value including tax or not
        /// </summary>
        public bool MinAdsSubtotalAmountIncludingTax { get; set; }

        /// <summary>
        /// Gets or sets a minimum Ad total amount
        /// </summary>
        public decimal MinAdsTotalAmount { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether anonymous checkout allowed
        /// </summary>
        public bool AnonymousCheckoutAllowed { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether 'Terms of service' enabled on the shopping cart page
        /// </summary>
        public bool TermsOfServiceOnShoppingCartPage { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether 'Terms of service' enabled on the Ad confirmation page
        /// </summary>
        public bool TermsOfServiceOnAdsConfirmPage { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether 'One-page checkout' is enabled
        /// </summary>
        public bool OnePageCheckoutEnabled { get; set; }


        /// <summary>
        /// Gets or sets a value indicating whether "Billing address" step should be skipped
        /// </summary>
        public bool DisableBillingAddressCheckoutStep { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether "Ad completed" page should be skipped
        /// </summary>
        public bool DisableAdCompletedPage { get; set; }

        /// <summary>
        /// Gets or sets a value indicating we should attach PDF invoice to "Ad placed" email
        /// </summary>
        public bool AttachPdfInvoiceToAdPlacedEmail { get; set; }
        /// <summary>
        /// Gets or sets a value indicating we should attach PDF invoice to "Ad paid" email
        /// </summary>
        public bool AttachPdfInvoiceToAdPaidEmail { get; set; }    
        /// <summary>
        /// Gets or sets a value indicating we should attach PDF invoice to "Ad completed" email
        /// </summary>
        public bool AttachPdfInvoiceToAdCompletedEmail { get; set; }
        /// <summary>
        /// Gets or sets a value indicating we should attach PDF invoice to binary field
        /// </summary>
        public bool AttachPdfInvoiceToBinary { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether "Return requests" are allowed
        /// </summary>
        public bool ReturnRequestsEnabled { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether return request pickup address can be specified by customer
        /// </summary>
        public bool ReturnRequests_AllowToSpecifyPickupAddress { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether return request pickup date can be specified by customer
        /// </summary>
        public bool ReturnRequests_AllowToSpecifyPickupDate { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether return request pickup date has to be specified by customer
        /// </summary>
        public bool ReturnRequests_PickupDateRequired { get; set; }

        /// <summary>
        /// Gets or sets a number of days that the Return Request Link will be available for customers after Ad placing.
        /// </summary>
        public int NumberOfDaysReturnRequestAvailable { get; set; }

        /// <summary>
        ///  Gift cards are activated when the Ad status is
        /// </summary>
        public int GiftCards_Activated_AdStatusId { get; set; }

        /// <summary>
        ///  Gift cards are deactivated when the Ad is canceled
        /// </summary>
        public bool DeactivateGiftCardsAfterCancelAd { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to deactivate related gift cards after deleting the Ad
        /// </summary>
        public bool DeactivateGiftCardsAfterDeletingAd { get; set; }
        /// <summary>
        /// Gets or sets an Ad placement interval in seconds (prevent 2 Ads being placed within an X seconds time frame).
        /// </summary>
        public int MinimumAdPlacementInterval { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether an Ad status should be set to "Complete" only when its shipping status is "Delivered". Otherwise, "Shipped" status will be enough.
        /// </summary>
        public bool CompleteAdWhenDelivered { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether Ad status can be marked as cancelled by user (if Ad isn't paid and shipped yet)
        /// </summary>
        public bool UserCanCancelUnpaidAd { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether unpublish auction product after made Ad.
        /// </summary>
        public bool UnpublishAuctionProduct { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether customers can add Ad notes
        /// </summary>
        public bool AllowCustomerToAddAdsNote { get; set; }

        /// <summary>
        /// Gets or sets a length for Ad code
        /// </summary>
        public int LengthCode { get; set; }

        /// <summary>
        /// Gets or sets Number of Days after which Ad would automatically Canceled - if not paid and has pending status
        /// </summary>
        public int? DaysToCancelUnpaidAds { get; set; }
    }
}