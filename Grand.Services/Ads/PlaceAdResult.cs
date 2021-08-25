using Grand.Domain.Ads;
using System.Collections.Generic;

namespace Grand.Services.Ads
{
    /// <summary>
    /// Place order result
    /// </summary>
    public partial class PlaceAdResult
    {
        /// <summary>
        /// Ctor
        /// </summary>
        public PlaceAdResult() 
        {
            Errors = new List<string>();
        }

        /// <summary>
        /// Gets a value indicating whether request has been completed successfully
        /// </summary>
        public bool Success
        {
            get { return (Errors.Count == 0); }
        }

        /// <summary>
        /// Add error
        /// </summary>
        /// <param name="error">Error</param>
        public void AddError(string error)
        {
            Errors.Add(error);
        }

        /// <summary>
        /// Errors
        /// </summary>
        public IList<string> Errors { get; set; }
        
        /// <summary>
        /// Gets or sets the placed order
        /// </summary>
        public Ad PlacedAd { get; set; }
    }
}
