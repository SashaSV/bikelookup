using Grand.Domain;
using Grand.Domain.Catalog;
using System.Collections.Generic;
using System.Threading.Tasks;
using Grand.Domain.Vendors;

namespace Grand.Services.Catalog
{
    /// <summary>
    /// Specification attribute service interface
    /// </summary>
    public partial interface ISpecificationAttributeService
    {
        #region Specification attribute

        /// <summary>
        /// Gets a specification attribute
        /// </summary>
        /// <param name="specificationAttributeId">The specification attribute identifier</param>
        /// <returns>Specification attribute</returns>
        Task<SpecificationAttribute> GetSpecificationAttributeById(string specificationAttributeId);

        /// <summary>
        /// Gets a specification attribute by sename
        /// </summary>
        /// <param name="sename">Sename</param>
        /// <returns>Specification attribute</returns>
        Task<SpecificationAttribute> GetSpecificationAttributeBySeName(string sename);
        
        /// <summary>
        /// Gets a specification attribute by sename
        /// </summary>
        /// <param name="sename">Sename</param>
        /// <returns>Specification attribute</returns>
        Task<SpecificationAttribute> GetSpecificationAttributeBySeNameAutocomplete(string sename);
        
        /// <summary>
        /// Gets specification attributes
        /// </summary>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>Specification attributes</returns>
        Task<IPagedList<SpecificationAttribute>> GetSpecificationAttributes(int pageIndex = 0, int pageSize = int.MaxValue);

        /// <summary>
        /// Deletes a specification attribute
        /// </summary>
        /// <param name="specificationAttribute">The specification attribute</param>
        Task DeleteSpecificationAttribute(SpecificationAttribute specificationAttribute);

        /// <summary>
        /// Inserts a specification attribute
        /// </summary>
        /// <param name="specificationAttribute">The specification attribute</param>
        Task InsertSpecificationAttribute(SpecificationAttribute specificationAttribute);

        /// <summary>
        /// Updates the specification attribute
        /// </summary>
        /// <param name="specificationAttribute">The specification attribute</param>
        Task UpdateSpecificationAttribute(SpecificationAttribute specificationAttribute);

        #endregion

        #region Specification attribute option

        /// <summary>
        /// Gets a specification attribute option
        /// </summary>
        /// <param name="specificationAttributeOption">The specification attribute option</param>
        /// <returns>Specification attribute option</returns>
        Task<SpecificationAttributeOption> GetChildren(string specificationAttributeOption);
        
        /// <summary>
        /// Gets a specification attribute option
        /// </summary>
        /// <param name="specificationAttributeOption">The specification attribute option</param>
        /// <returns>Specification attribute option</returns>
        Task<SpecificationAttribute> GetSpecificationAttributeByOptionId(string specificationAttributeOption);

        /// <summary>
        /// Gets a specification attribute option 
        /// </summary>
        /// <param name="specificationAttributeId">The specification attribute option Id</param>
        /// <param name="specificationAttributeOptionName">The specification attribute option name</param>
        /// <returns>Specification attribute option</returns>
        Task<SpecificationAttributeOption> GetSpecificationAttributeByOptionName(string specificationAttributeId, string specificationAttributeOptionName);

        Task<IEnumerable<SpecificationAttributeOption>> GetSpecificationAttributeByOptionNameAutocomplete(
            string specificationAttributeId, string specificationAttributeOptionName);
        
        /// <summary>
        /// Deletes a specification attribute option
        /// </summary>
        /// <param name="specificationAttributeOption">The specification attribute option</param>
        Task DeleteSpecificationAttributeOption(SpecificationAttributeOption specificationAttributeOption);

        /// <summary>
        /// Inserts a specification attribute option
        /// </summary>
        /// <param name="specificationAttributeOption">The specification attribute option</param>
        Task UpdateSpecificationAttributeOption(SpecificationAttribute specificationAttribute, SpecificationAttributeOption specificationAttributeOption);

        #endregion

        #region Product specification attribute

        /// <summary>
        /// Deletes a product specification attribute mapping
        /// </summary>
        /// <param name="productSpecificationAttribute">Product specification attribute</param>
        Task DeleteProductSpecificationAttribute(ProductSpecificationAttribute productSpecificationAttribute);

        /// <summary>
        /// Inserts a product specification attribute mapping
        /// </summary>
        /// <param name="productSpecificationAttribute">Product specification attribute mapping</param>
        Task InsertProductSpecificationAttribute(ProductSpecificationAttribute productSpecificationAttribute);

        /// <summary>
        /// Updates the product specification attribute mapping
        /// </summary>
        /// <param name="productSpecificationAttribute">Product specification attribute mapping</param>
        Task UpdateProductSpecificationAttribute(ProductSpecificationAttribute productSpecificationAttribute);

        /// <summary>
        /// Gets a count of product specification attribute mapping records
        /// </summary>
        /// <param name="productId">Product identifier; "" to load all records</param>
        /// <param name="specificationAttributeOptionId">The specification attribute option identifier; "" to load all records</param>
        /// <returns>Count</returns>
        int GetProductSpecificationAttributeCount(string productId = "", string specificationAttributeOptionId = "");

        /// <summary>
        /// Get formatted category breadcrumb 
        /// Note: ACL and store mapping is ignored
        /// </summary>
        /// <param name="spo">Category</param>
        /// <param name="allOptions">Category</param>
        /// <param name="separator">Separator</param>
        /// <param name="languageId">Language identifier for localization</param>
        /// <returns>Formatted breadcrumb</returns>
        string GetFormattedOptionBreadCrumb(SpecificationAttributeOption spo, ICollection<SpecificationAttributeOption> allOptions, string separator = ">>", string languageId = "");

        /// <summary>
        /// Get category breadcrumb 
        /// </summary>
        /// <param name="SpecificationAttributeOption">Category</param>
        /// <param name="allOptions">Category service</param>
        /// <returns>Category breadcrumb </returns>
        IList<SpecificationAttributeOption> GetOptionBreadCrumb(SpecificationAttributeOption spo, ICollection<SpecificationAttributeOption> allOptions);

        /// <summary>
        /// Get all Child Option
        /// </summary>
        /// <param name="SpecificationAttributeOption">Category</param>
        /// <param name="allOptions">Category service</param>
        /// <returns>Category breadcrumb </returns>
        IList<SpecificationAttributeOption> GetOptionAllChild(SpecificationAttributeOption spo, ICollection<SpecificationAttributeOption> allOptions);

        /// <summary>
        /// Get category breadcrumb 
        /// </summary>
        /// <param name="SpecificationAttributeOption">Category</param>
        /// <returns>Category breadcrumb </returns>
        Task<IList<SpecificationAttributeOption>> GetOptionBreadCrumb(SpecificationAttributeOption spo);

        /// <summary>
        /// Get formatted category breadcrumb 
        /// Note: ACL and store mapping is ignored
        /// </summary>
        /// <param name="category">Category</param>
        /// <param name="separator">Separator</param>
        /// <param name="languageId">Language identifier for localization</param>
        /// <returns>Formatted breadcrumb</returns>
        Task<string> GetFormattedOptionBreadCrumb(SpecificationAttributeOption spo, string separator = ">>", string languageId = "");

        /// <summary>
        /// Gets a product specification attribute mapping records
        /// </summary>
        /// <param name="productId">Product identifier; "" to load all records</param>
        /// <param name="specificationAttributeOptionId">The specification attribute option identifier; "" to load all records</param>
        /// <returns>Count</returns>
        Task<ProductSpecificationAttribute> GetProductSpecificationAttributeByOptionId(
            string productId = "",
            string specificationAttributeId = "",
            string specificationAttributeOptionId = "");
        
        #endregion

        Task DeleteVendorSpecificationAttribute(VendorSpecificationAttribute vendorSpecificationAttribute);
        
        Task UpdateVendorSpecificationAttribute(VendorSpecificationAttribute productSpecificationAttribute);

        Task InsertVendorSpecificationAttribute(VendorSpecificationAttribute productSpecificationAttribute);
    }
}
