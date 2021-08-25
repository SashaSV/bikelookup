namespace Grand.Domain.Ads
{
    /// <summary>
    /// Represents an "order by country" report line
    /// </summary>
    public partial class AdByTimeReportLine
    {
        public string Time { get; set; }

        public int TotalAds { get; set; }

        public decimal SumAds { get; set; }
    }
}
