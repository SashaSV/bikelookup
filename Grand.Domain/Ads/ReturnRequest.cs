using Grand.Domain.Common;
using System;
using System.Collections.Generic;

namespace Grand.Domain.Ads
{
    /// <summary>
    /// Represents a return request
    /// </summary>
    public partial class ReturnRequestAd : BaseEntity
    {
        public ReturnRequestAd()
        {
            ReturnRequestItems = new List<ReturnRequestItemAd>();
        }

        public int ReturnNumber { get; set; }

        /// <summary>
        /// Gets or sets the ExternalId
        /// </summary>
        public string ExternalId { get; set; }

        /// <summary>
        /// Gets or sets the store identifier
        /// </summary>
        public string StoreId { get; set; }

        /// <summary>
        /// Gets or sets the Ad item identifier
        /// </summary>
        public string AdId { get; set; }

        /// <summary>
        /// Gets or sets the owner item identifier
        /// </summary>
        public string OwnerId { get; set; }

        /// <summary>
        /// Gets or sets the return request items
        /// </summary>
        public IList<ReturnRequestItemAd> ReturnRequestItems { get; set; }

        /// <summary>
        /// Gets or sets the customer identifier
        /// </summary>
        public string CustomerId { get; set; }

        /// <summary>
        /// Gets or sets the vendor item identifier
        /// </summary>
        public string VendorId { get; set; }

        /// <summary>
        /// Gets or sets the customer comments
        /// </summary>
        public string CustomerComments { get; set; }

        /// <summary>
        /// Gets or sets the staff notes
        /// </summary>
        public string StaffNotes { get; set; }

        /// <summary>
        /// Gets or sets the return status identifier
        /// </summary>
        public int ReturnRequestStatusId { get; set; }

        /// <summary>
        /// Gets or sets the date and time of entity creation
        /// </summary>
        public DateTime CreatedOnUtc { get; set; }

        /// <summary>
        /// Gets or sets the date and time of entity update
        /// </summary>
        public DateTime UpdatedOnUtc { get; set; }

        /// <summary>
        /// Gets or sets the return status
        /// </summary>
        public ReturnRequestStatus ReturnRequestStatus
        {
            get
            {
                return (ReturnRequestStatus)this.ReturnRequestStatusId;
            }
            set
            {
                this.ReturnRequestStatusId = (int)value;
            }
        }

        /// <summary>
        /// Gets or sets the pickup date
        /// </summary>
        public DateTime PickupDate { get; set; }

        /// <summary>
        /// Gets or sets the pickup address
        /// </summary>
        public Address PickupAddress { get; set; }

        /// <summary>
        /// Get or sets notify customer
        /// </summary>
        public bool NotifyCustomer { get; set; }
    }
}
