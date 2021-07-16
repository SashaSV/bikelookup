using Grand.Framework.Localization;
using Grand.Core.ModelBinding;
using Grand.Core.Models;
using Grand.Web.Areas.Admin.Models.Common;
using Grand.Web.Areas.Admin.Models.Discounts;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Grand.Web.Areas.Admin.Models.Catalog;

namespace Grand.Web.Areas.Admin.Models.Vendors
{
    public partial class VendorModel : BaseEntityModel, ILocalizedModel<VendorLocalizedModel>
    {
        public VendorModel()
        {
            if (PageSize < 1)
            {
                PageSize = 5;
            }
            Locales = new List<VendorLocalizedModel>();
            AssociatedCustomers = new List<AssociatedCustomerInfo>();
            Addresses = new List<AddressModel>();
            AvailableStores = new List<SelectListItem>();
            AddSpecificationAttributeModel = new AddVendorSpecificationAttributeModel();
        }

        [GrandResourceDisplayName("Admin.Vendors.Fields.Name")]

        public string Name { get; set; }

        [GrandResourceDisplayName("Admin.Vendors.Fields.Email")]

        public string Email { get; set; }

        [GrandResourceDisplayName("Admin.Vendors.Fields.Description")]

        public string Description { get; set; }

        [UIHint("Picture")]
        [GrandResourceDisplayName("Admin.Vendors.Fields.Picture")]
        public string PictureId { get; set; }

        [GrandResourceDisplayName("Admin.Vendors.Fields.Store")]
        public string StoreId { get; set; }
        public List<SelectListItem> AvailableStores { get; set; }

        [GrandResourceDisplayName("Admin.Vendors.Fields.AdminComment")]

        public string AdminComment { get; set; }

        [GrandResourceDisplayName("Admin.Vendors.Fields.Active")]
        public bool Active { get; set; }

        [GrandResourceDisplayName("Admin.Vendors.Fields.DisplayOrder")]
        public int DisplayOrder { get; set; }

        [GrandResourceDisplayName("Admin.Vendors.Fields.AllowCustomerReviews")]
        public bool AllowCustomerReviews { get; set; }

        [GrandResourceDisplayName("Admin.Vendors.Fields.MetaKeywords")]

        public string MetaKeywords { get; set; }

        [GrandResourceDisplayName("Admin.Vendors.Fields.MetaDescription")]

        public string MetaDescription { get; set; }

        [GrandResourceDisplayName("Admin.Vendors.Fields.MetaTitle")]

        public string MetaTitle { get; set; }

        [GrandResourceDisplayName("Admin.Vendors.Fields.SeName")]

        public string SeName { get; set; }

        [GrandResourceDisplayName("Admin.Vendors.Fields.PageSize")]
        public int PageSize { get; set; }

        [GrandResourceDisplayName("Admin.Vendors.Fields.AllowCustomersToSelectPageSize")]
        public bool AllowCustomersToSelectPageSize { get; set; }

        [GrandResourceDisplayName("Admin.Vendors.Fields.PageSizeOptions")]
        public string PageSizeOptions { get; set; }

        public IList<VendorLocalizedModel> Locales { get; set; }

        [GrandResourceDisplayName("Admin.Vendors.Fields.AssociatedCustomerEmails")]
        public IList<AssociatedCustomerInfo> AssociatedCustomers { get; set; }

        public IList<AddressModel> Addresses { get; set; }

        //vendor notes
        [GrandResourceDisplayName("Admin.Vendors.VendorNotes.Fields.Note")]

        public string AddVendorNoteMessage { get; set; }

        public List<DiscountModel> AvailableDiscounts { get; set; }
        public string[] SelectedDiscountIds { get; set; }

        [GrandResourceDisplayName("Admin.Vendors.Fields.Commission")]

        public decimal Commission { get; set; } = 0;

        public AddVendorSpecificationAttributeModel AddSpecificationAttributeModel { get; set; }
        #region Nested classes

        public class AssociatedCustomerInfo : BaseEntityModel
        {
            public string Email { get; set; }
        }


        public partial class VendorNote : BaseEntityModel
        {
            public string VendorId { get; set; }
            [GrandResourceDisplayName("Admin.Vendors.VendorNotes.Fields.Note")]
            public string Note { get; set; }
            [GrandResourceDisplayName("Admin.Vendors.VendorNotes.Fields.CreatedOn")]
            public DateTime CreatedOn { get; set; }
        }
        #endregion

    }

         public partial class AddVendorSpecificationAttributeModel : BaseModel
            {
                public AddVendorSpecificationAttributeModel()
                {
                    AvailableAttributes = new List<SelectListItem>();
                    AvailableOptions = new List<SelectListItem>();
                }
    
                [GrandResourceDisplayName("Admin.Catalog.Products.SpecificationAttributes.Fields.SpecificationAttribute")]
                public string SpecificationAttributeId { get; set; }
    
                [GrandResourceDisplayName("Admin.Catalog.Products.SpecificationAttributes.Fields.AttributeType")]
                public int AttributeTypeId { get; set; }
    
                [GrandResourceDisplayName("Admin.Catalog.Products.SpecificationAttributes.Fields.SpecificationAttributeOption")]
                public string SpecificationAttributeOptionId { get; set; }
    
                [GrandResourceDisplayName("Admin.Catalog.Products.SpecificationAttributes.Fields.CustomValue")]
                public string CustomValue { get; set; }
    
                [GrandResourceDisplayName("Admin.Catalog.Products.SpecificationAttributes.Fields.AllowFiltering")]
                public bool AllowFiltering { get; set; }
    
                [GrandResourceDisplayName("Admin.Catalog.Products.SpecificationAttributes.Fields.ShowOnProductPage")]
                public bool ShowOnProductPage { get; set; }
    
                [GrandResourceDisplayName("Admin.Catalog.Products.SpecificationAttributes.Fields.DisplayOrder")]
                public int DisplayOrder { get; set; }
    
                public string VendorId { get; set; }
                public IList<SelectListItem> AvailableAttributes { get; set; }
                public IList<SelectListItem> AvailableOptions { get; set; }
            }

    public partial class VendorLocalizedModel : ILocalizedModelLocal, ISlugModelLocal
    {
        public string LanguageId { get; set; }

        [GrandResourceDisplayName("Admin.Vendors.Fields.Name")]

        public string Name { get; set; }

        [GrandResourceDisplayName("Admin.Vendors.Fields.Description")]

        public string Description { get; set; }

        [GrandResourceDisplayName("Admin.Vendors.Fields.MetaKeywords")]

        public string MetaKeywords { get; set; }

        [GrandResourceDisplayName("Admin.Vendors.Fields.MetaDescription")]

        public string MetaDescription { get; set; }

        [GrandResourceDisplayName("Admin.Vendors.Fields.MetaTitle")]

        public string MetaTitle { get; set; }

        [GrandResourceDisplayName("Admin.Vendors.Fields.SeName")]

        public string SeName { get; set; }
    }
}