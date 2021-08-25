namespace Grand.Domain.Ads
{
    /// <summary>
    /// Represents an "order by country" report line
    /// </summary>
    public partial class AdByCountryReportLine
    {
        /// <summary>
        /// Country identifier; null for unknow country
        /// </summary>
        public string CountryId { get; set; }

        /// <summary>
        /// Gets or sets the number of orders
        /// </summary>
        public int TotalAds { get; set; }

        /// <summary>
        /// Gets or sets the order total summary
        /// </summary>
        public decimal SumAds { get; set; }
    }
}
