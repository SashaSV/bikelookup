using System;

namespace Grand.Domain.Ads
{
    /// <summary>
    /// Represents an order note
    /// </summary>
    public partial class AdNote : BaseEntity
    {
        /// <summary>
        /// Gets or sets the order identifier
        /// </summary>
        public string AdId { get; set; }

        /// <summary>
        /// Gets or sets the note
        /// </summary>
        public string Note { get; set; }

        /// <summary>
        /// Gets or sets the attached file (download) identifier
        /// </summary>
        public string DownloadId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether a customer can see a note
        /// </summary>
        public bool DisplayToCustomer { get; set; }

        /// <summary>
        /// Gets or sets the date and time of order note creation
        /// </summary>
        public DateTime CreatedOnUtc { get; set; }

        /// <summary>
        /// Gets or sets value indicating whether this order note was create by customer
        /// </summary>
        public bool CreatedByCustomer { get; set; }
    }
}
