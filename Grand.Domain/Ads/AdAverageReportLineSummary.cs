namespace Grand.Domain.Ads
{
    /// <summary>
    /// Represents an order average report line summary
    /// </summary>
    public partial class AdAverageReportLineSummary
    {
        /// <summary>
        /// Gets or sets the order status
        /// </summary>
        public AdStatus AdStatus { get; set; }

        /// <summary>
        /// Gets or sets the sum today total
        /// </summary>
        public decimal SumTodayAds { get; set; }

        /// <summary>
        /// Gets or sets the today count
        /// </summary>
        public int CountTodayAds { get; set; }

        /// <summary>
        /// Gets or sets the sum this week total
        /// </summary>
        public decimal SumThisWeekAds { get; set; }

        /// <summary>
        /// Gets or sets the this week count
        /// </summary>
        public int CountThisWeekAds { get; set; }

        /// <summary>
        /// Gets or sets the sum this month total
        /// </summary>
        public decimal SumThisMonthAds { get; set; }

        /// <summary>
        /// Gets or sets the this month count
        /// </summary>
        public int CountThisMonthAds { get; set; }

        /// <summary>
        /// Gets or sets the sum this year total
        /// </summary>
        public decimal SumThisYearAds { get; set; }

        /// <summary>
        /// Gets or sets the this year count
        /// </summary>
        public int CountThisYearAds { get; set; }

        /// <summary>
        /// Gets or sets the sum all time total
        /// </summary>
        public decimal SumAllTimeAds { get; set; }

        /// <summary>
        /// Gets or sets the all time count
        /// </summary>
        public int CountAllTimeAds { get; set; }
    }
}
