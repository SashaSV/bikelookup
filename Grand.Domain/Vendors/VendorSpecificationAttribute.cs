using System.Collections.Generic;
using Grand.Domain.Catalog;
using Grand.Domain.Localization;

namespace Grand.Domain.Vendors
{
    public class VendorSpecificationAttribute: BaseEntity, ILocalizedEntity
        {
            public VendorSpecificationAttribute()
            {
                Locales = new List<LocalizedProperty>();
            }
    
            /// <summary>
            /// Gets or sets the product identifier
            /// </summary>
            public string VendorId { get; set; }
            
            /// <summary>
            /// Gets or sets the attribute type ID
            /// </summary>
            public int AttributeTypeId { get; set; }
    
            /// <summary>
            /// Gets or sets the specification attribute Id
            /// </summary>
            public string SpecificationAttributeId { get; set; }

            /// <summary>
            /// Url of atribute optin details
            /// </summary>
            public string DetailsUrl { get; set; }

            /// <summary>
            /// Gets or sets the specification attribute identifier
            /// </summary>
            public string SpecificationAttributeOptionId { get; set; }
    
            /// <summary>
            /// Gets or sets the custom value
            /// </summary>
            public string CustomValue { get; set; }
    
            /// <summary>
            /// Gets or sets whether the attribute can be filtered by
            /// </summary>
            public bool AllowFiltering { get; set; }
    
            /// <summary>
            /// Gets or sets whether the attribute will be shown on the product page
            /// </summary>
            public bool ShowOnProductPage { get; set; }
    
            /// <summary>
            /// Gets or sets the display order
            /// </summary>
            public int DisplayOrder { get; set; }
    
            /// <summary>
            /// Gets or sets the collection of locales
            /// </summary>
            public IList<LocalizedProperty> Locales { get; set; }
    
            /// <summary>
            /// Gets the attribute control type
            /// </summary>
            public SpecificationAttributeType AttributeType
            {
                get
                {
                    return (SpecificationAttributeType)this.AttributeTypeId;
                }
                set
                {
                    this.AttributeTypeId = (int)value;
                }
            }
        }
}