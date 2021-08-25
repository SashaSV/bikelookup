using Grand.Domain.Localization;
using System.Collections.Generic;

namespace Grand.Domain.Ads
{
    /// <summary>
    /// Represents a return request reason
    /// </summary>
    public partial class ReturnRequestReasonAd : BaseEntity, ILocalizedEntity
    {
        public ReturnRequestReasonAd()
        {
            Locales = new List<LocalizedProperty>();
        }
        /// <summary>
        /// Gets or sets the name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the display order
        /// </summary>
        public int DisplayAd { get; set; }

        /// <summary>
        /// Gets or sets the collection of locales
        /// </summary>
        public IList<LocalizedProperty> Locales { get; set; }
    }
}
