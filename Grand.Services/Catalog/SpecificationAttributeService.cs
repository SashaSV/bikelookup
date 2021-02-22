using Grand.Domain;
using Grand.Core.Caching;
using Grand.Domain.Data;
using Grand.Domain.Catalog;
using Grand.Services.Events;
using MediatR;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Grand.Services.Catalog
{
    /// <summary>
    /// Specification attribute service
    /// </summary>
    public partial class SpecificationAttributeService : ISpecificationAttributeService
    {
        #region Constants

        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : product ID
        /// </remarks>
        private const string PRODUCTS_BY_ID_KEY = "Grand.product.id-{0}";

        /// <summary>
        /// Key pattern to clear cache
        /// </summary>
        private const string PRODUCTS_PATTERN_KEY = "Grand.product.";


        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : sename
        /// </remarks>
        private const string SPECIFICATION_BY_SENAME = "Grand.specification.sename-{0}";

        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : specification ID
        /// </remarks>
        private const string SPECIFICATION_BY_ID_KEY = "Grand.specification.id-{0}";

        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : specification option ID
        /// </remarks>
        private const string SPECIFICATION_BY_OPTIONID_KEY = "Grand.specification.optionid-{0}";

        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : specification option ID
        /// </remarks>
        private const string OPTIONID_PATTERN_KEY = "Grand.specification.optionid";

        /// <summary>
        /// Key pattern to clear cache
        /// </summary>
        private const string SPECIFICATION_PATTERN_KEY = "Grand.specification.";

        #endregion

        #region Fields

        private readonly IRepository<Product> _productRepository;
        private readonly IRepository<SpecificationAttribute> _specificationAttributeRepository;
        private readonly IRepository<ProductSpecificationAttribute> _productSpecificationAttributeRepository;
        private readonly ICacheManager _cacheManager;
        private readonly IMediator _mediator;

        #endregion

        #region Ctor

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="cacheManager">Cache manager</param>
        /// <param name="specificationAttributeRepository">Specification attribute repository</param>
        /// <param name="mediator">Mediator</param>
        public SpecificationAttributeService(ICacheManager cacheManager,
            IRepository<SpecificationAttribute> specificationAttributeRepository,
            IRepository<Product> productRepository,
            IRepository<ProductSpecificationAttribute> productSpecificationAttribute,
            IMediator mediator)
        {
            _cacheManager = cacheManager;
            _specificationAttributeRepository = specificationAttributeRepository;
            _productSpecificationAttributeRepository = productSpecificationAttribute;
            _mediator = mediator;
            _productRepository = productRepository;
        }

        #endregion

        #region Methods

        #region Specification attribute

        /// <summary>
        /// Gets a specification attribute
        /// </summary>
        /// <param name="specificationAttributeId">The specification attribute identifier</param>
        /// <returns>Specification attribute</returns>
        public virtual async Task<SpecificationAttribute> GetSpecificationAttributeById(string specificationAttributeId)
        {
            string key = string.Format(SPECIFICATION_BY_ID_KEY, specificationAttributeId);
            return await _cacheManager.GetAsync(key, () => _specificationAttributeRepository.GetByIdAsync(specificationAttributeId));
        }

        /// <summary>
        /// Gets a specification attribute by sename
        /// </summary>
        /// <param name="sename">Sename</param>
        /// <returns>Specification attribute</returns>
        public virtual async Task<SpecificationAttribute> GetSpecificationAttributeBySeName(string sename)
        {
            if (string.IsNullOrEmpty(sename))
                return await Task.FromResult<SpecificationAttribute>(null);

            sename = sename.ToLowerInvariant();

            var key = string.Format(SPECIFICATION_BY_SENAME, sename);
            return await _cacheManager.GetAsync(key, async () => await _specificationAttributeRepository.Table.Where(x => x.SeName.ToLower() == sename.ToLower()).FirstOrDefaultAsync());
        }


        /// <summary>
        /// Gets specification attributes
        /// </summary>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>Specification attributes</returns>
        public virtual async Task<IPagedList<SpecificationAttribute>> GetSpecificationAttributes(int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var query = from sa in _specificationAttributeRepository.Table
                        orderby sa.DisplayOrder
                        select sa;
            return await PagedList<SpecificationAttribute>.Create(query, pageIndex, pageSize);
        }

        /// <summary>
        /// Deletes a specification attribute
        /// </summary>
        /// <param name="specificationAttribute">The specification attribute</param>
        public virtual async Task DeleteSpecificationAttribute(SpecificationAttribute specificationAttribute)
        {
            if (specificationAttribute == null)
                throw new ArgumentNullException("specificationAttribute");

            var builder = Builders<Product>.Update;
            var updatefilter = builder.PullFilter(x => x.ProductSpecificationAttributes, y => y.SpecificationAttributeId == specificationAttribute.Id);
            await _productRepository.Collection.UpdateManyAsync(new BsonDocument(), updatefilter);

            await _specificationAttributeRepository.DeleteAsync(specificationAttribute);

            //clear cache
            await _cacheManager.RemoveByPrefix(PRODUCTS_PATTERN_KEY);
            await _cacheManager.RemoveByPrefix(SPECIFICATION_PATTERN_KEY);

            //event notification
            await _mediator.EntityDeleted(specificationAttribute);
        }

        /// <summary>
        /// Inserts a specification attribute
        /// </summary>
        /// <param name="specificationAttribute">The specification attribute</param>
        public virtual async Task InsertSpecificationAttribute(SpecificationAttribute specificationAttribute)
        {
            if (specificationAttribute == null)
                throw new ArgumentNullException("specificationAttribute");

            await _specificationAttributeRepository.InsertAsync(specificationAttribute);

            //clear cache
            await _cacheManager.RemoveByPrefix(SPECIFICATION_PATTERN_KEY);

            //event notification
            await _mediator.EntityInserted(specificationAttribute);
        }

        /// <summary>
        /// Updates the specification attribute
        /// </summary>
        /// <param name="specificationAttribute">The specification attribute</param>
        public virtual async Task UpdateSpecificationAttribute(SpecificationAttribute specificationAttribute)
        {
            if (specificationAttribute == null)
                throw new ArgumentNullException("specificationAttribute");

            await _specificationAttributeRepository.UpdateAsync(specificationAttribute);

            //clear cache
            await _cacheManager.RemoveByPrefix(SPECIFICATION_PATTERN_KEY);

            //event notification
            await _mediator.EntityUpdated(specificationAttribute);
        }

        #endregion

        #region Specification attribute option

        /// <summary>
        /// Gets a specification attribute option
        /// </summary>
        /// <param name="specificationAttributeOptionId">The specification attribute option identifier</param>
        /// <returns>Specification attribute option</returns>
        public virtual async Task<SpecificationAttribute> GetSpecificationAttributeByOptionId(string specificationAttributeOptionId)
        {
            if (string.IsNullOrEmpty(specificationAttributeOptionId))
                return await Task.FromResult<SpecificationAttribute>(null);

            string key = string.Format(SPECIFICATION_BY_OPTIONID_KEY, specificationAttributeOptionId);
            return await _cacheManager.GetAsync(key, async () =>
            {
                var query = from p in _specificationAttributeRepository.Table
                            where p.SpecificationAttributeOptions.Any(x => x.Id == specificationAttributeOptionId)
                            select p;
                return await query.FirstOrDefaultAsync();
            });
        }

        /// <summary>
        /// Gets a specification attribute option
        /// </summary>
        /// <param name="specificationAttributeOptionId">The specification attribute option identifier</param>
        /// <returns>Specification attribute option</returns>
        public virtual async Task<SpecificationAttributeOption> GetSpecificationAttributeByOptionName(string specificationAttributeId, string specificationAttributeOptionName)
        {
            if (string.IsNullOrEmpty(specificationAttributeOptionName))
                return await Task.FromResult<SpecificationAttributeOption>(null);

            string key = string.Format(SPECIFICATION_BY_ID_KEY, specificationAttributeId);
            var specificationAttribute = await _cacheManager.GetAsync(key, () => _specificationAttributeRepository.GetByIdAsync(specificationAttributeId));
            return specificationAttribute.SpecificationAttributeOptions.Where(x => x.Name.ToLower() == specificationAttributeOptionName.ToLower()).FirstOrDefault();
        }

        
        /// <summary>
        /// Deletes a specification attribute option
        /// </summary>
        /// <param name="specificationAttributeOption">The specification attribute option</param>
        public virtual async Task DeleteSpecificationAttributeOption(SpecificationAttributeOption specificationAttributeOption)
        {
            if (specificationAttributeOption == null)
                throw new ArgumentNullException("specificationAttributeOption");

            var builder = Builders<Product>.Update;
            var updatefilter = builder.PullFilter(x => x.ProductSpecificationAttributes,
                y => y.SpecificationAttributeOptionId == specificationAttributeOption.Id);
            await _productRepository.Collection.UpdateManyAsync(new BsonDocument(), updatefilter);

            var specificationAttribute = await GetSpecificationAttributeByOptionId(specificationAttributeOption.Id);
            var sao = specificationAttribute.SpecificationAttributeOptions.Where(x => x.Id == specificationAttributeOption.Id).FirstOrDefault();
            if (sao == null)
                throw new ArgumentException("No specification attribute option found with the specified id");

            specificationAttribute.SpecificationAttributeOptions.Remove(sao);
            await UpdateSpecificationAttribute(specificationAttribute);

            //clear cache
            await _cacheManager.RemoveByPrefix(SPECIFICATION_PATTERN_KEY);
            await _cacheManager.RemoveByPrefix(PRODUCTS_PATTERN_KEY);

            //event notification
            await _mediator.EntityDeleted(specificationAttributeOption);
        }


        #endregion

        #region Product specification attribute

        /// <summary>
        /// Deletes a product specification attribute mapping
        /// </summary>
        /// <param name="productSpecificationAttribute">Product specification attribute</param>
        public virtual async Task DeleteProductSpecificationAttribute(ProductSpecificationAttribute productSpecificationAttribute)
        {
            if (productSpecificationAttribute == null)
                throw new ArgumentNullException("productSpecificationAttribute");

            var updatebuilder = Builders<Product>.Update;
            var update = updatebuilder.Pull(p => p.ProductSpecificationAttributes, productSpecificationAttribute);
            await _productRepository.Collection.UpdateOneAsync(new BsonDocument("_id", productSpecificationAttribute.ProductId), update);

            //clear cache
            await _cacheManager.RemoveAsync(string.Format(PRODUCTS_BY_ID_KEY, productSpecificationAttribute.ProductId));

            //event notification
            await _mediator.EntityDeleted(productSpecificationAttribute);
        }

        public virtual async Task UpdateSpecificationAttributeOption(SpecificationAttribute specificationAttribute, SpecificationAttributeOption specificationAttributeOption)
        {

            if (specificationAttributeOption == null)
                throw new ArgumentNullException("specificationAttributeOption");

            if (specificationAttribute == null)
                throw new ArgumentNullException("specificationAttribute");

            //await _specificationAttributeRepository.InsertAsync(specificationAttribute);
            specificationAttribute.SpecificationAttributeOptions.Add(specificationAttributeOption);
            await UpdateSpecificationAttribute(specificationAttribute);
            //cache
            await _cacheManager.RemoveAsync(OPTIONID_PATTERN_KEY);

            //event notification
            await _mediator.EntityInserted(specificationAttributeOption);
        }

        /// <summary>
        /// Inserts a product specification attribute mapping
        /// </summary>
        /// <param name="productSpecificationAttribute">Product specification attribute mapping</param>
        public virtual async Task InsertProductSpecificationAttribute(ProductSpecificationAttribute productSpecificationAttribute)
        {
            if (productSpecificationAttribute == null)
                throw new ArgumentNullException("productSpecificationAttribute");

            var updatebuilder = Builders<Product>.Update;
            var update = updatebuilder.AddToSet(p => p.ProductSpecificationAttributes, productSpecificationAttribute);
            await _productRepository.Collection.UpdateOneAsync(new BsonDocument("_id", productSpecificationAttribute.ProductId), update);

            //cache
            await _cacheManager.RemoveAsync(string.Format(PRODUCTS_BY_ID_KEY, productSpecificationAttribute.ProductId));

            //event notification
            await _mediator.EntityInserted(productSpecificationAttribute);
        }

        /// <summary>
        /// Updates the product specification attribute mapping
        /// </summary>
        /// <param name="productSpecificationAttribute">Product specification attribute mapping</param>
        public virtual async Task UpdateProductSpecificationAttribute(ProductSpecificationAttribute productSpecificationAttribute)
        {
            if (productSpecificationAttribute == null)
                throw new ArgumentNullException("productSpecificationAttribute");

            var builder = Builders<Product>.Filter;
            var filter = builder.Eq(x => x.Id, productSpecificationAttribute.ProductId);
            filter = filter & builder.Where(x => x.ProductSpecificationAttributes.Any(y => y.Id == productSpecificationAttribute.Id));
            var update = Builders<Product>.Update
                .Set(x => x.ProductSpecificationAttributes.ElementAt(-1).ShowOnProductPage, productSpecificationAttribute.ShowOnProductPage)
                .Set(x => x.ProductSpecificationAttributes.ElementAt(-1).CustomValue, productSpecificationAttribute.CustomValue)
                .Set(x => x.ProductSpecificationAttributes.ElementAt(-1).DisplayOrder, productSpecificationAttribute.DisplayOrder)
                .Set(x => x.ProductSpecificationAttributes.ElementAt(-1).AttributeTypeId, productSpecificationAttribute.AttributeTypeId)
                .Set(x => x.ProductSpecificationAttributes.ElementAt(-1).SpecificationAttributeId, productSpecificationAttribute.SpecificationAttributeId)
                .Set(x => x.ProductSpecificationAttributes.ElementAt(-1).SpecificationAttributeOptionId, productSpecificationAttribute.SpecificationAttributeOptionId)
                .Set(x => x.ProductSpecificationAttributes.ElementAt(-1).AllowFiltering, productSpecificationAttribute.AllowFiltering);

            await _productRepository.Collection.UpdateManyAsync(filter, update);

            //cache
            await _cacheManager.RemoveAsync(string.Format(PRODUCTS_BY_ID_KEY, productSpecificationAttribute.ProductId));

            //event notification
            await _mediator.EntityUpdated(productSpecificationAttribute);
        }

        /// <summary>
        /// Gets a count of product specification attribute mapping records
        /// </summary>
        /// <param name="productId">Product identifier; "" to load all records</param>
        /// <param name="specificationAttributeOptionId">The specification attribute option identifier; "" to load all records</param>
        /// <returns>Count</returns>
        public virtual int GetProductSpecificationAttributeCount(string productId = "", string specificationAttributeOptionId = "")
        {
            var query = _productRepository.Table;

            if (!string.IsNullOrEmpty(productId))
                query = query.Where(psa => psa.Id == productId);
            if (!string.IsNullOrEmpty(specificationAttributeOptionId))
                query = query.Where(psa => psa.ProductSpecificationAttributes.Any(x => x.SpecificationAttributeOptionId == specificationAttributeOptionId));

            return query.Count();
        }

        /// <summary>
        /// Gets a count of product specification attribute mapping records
        /// </summary>
        /// <param name="productId">Product identifier; "" to load all records</param>
        /// <param name="specificationAttributeOptionName">The specification attribute option identifier; "" to load all records</param>
        /// <returns>Count</returns>       
        public virtual async Task<ProductSpecificationAttribute> GetProductSpecificationAttributeByOptionId(
            string productId, 
            string specificationAttributeId, 
            string specificationAttributeOptionId)
        {
            if (productId == null)
                throw new ArgumentNullException("productId");

            if (specificationAttributeId == null)
                throw new ArgumentNullException("specificationAttributeId");

            if (specificationAttributeOptionId == null)
                throw new ArgumentNullException("specificationAttributeOptionId");

            var query = _productRepository.Table;
            
            query = query.Where(psa => psa.Id == productId);
            var product = await query.FirstOrDefaultAsync();
            
            if (product == null) 
            {
                throw new ArgumentNullException("Product null");
            }

            return product.ProductSpecificationAttributes.Where(psa => psa.SpecificationAttributeId == specificationAttributeId && psa.SpecificationAttributeOptionId == specificationAttributeOptionId).FirstOrDefault();
        }

        /// <summary>
        /// Get category breadcrumb 
        /// </summary>
        /// <param name="category">Category</param>
        /// <param name="categoryService">Category service</param>
        /// <param name="aclService">ACL service</param>
        /// <param name="storeMappingService">Store mapping service</param>
        /// <param name="showHidden">A value indicating whether to load hidden records</param>
        /// <returns>Category breadcrumb </returns>
        public virtual async Task<IList<SpecificationAttributeOption>> GetOptionBreadCrumb(SpecificationAttributeOption spo)
        {
            var result = new List<SpecificationAttributeOption>();

            //used to prevent circular references
            var alreadyProcessedSpoIds = new List<string>();
            var specificationAttribute = await GetSpecificationAttributeByOptionId(spo.Id);

            while (spo != null)  //not null                
                
            {
                result.Add(spo);

                alreadyProcessedSpoIds.Add(spo.Id);

                spo = specificationAttribute.SpecificationAttributeOptions.Where(s=>s.Id == spo.ParentSpecificationAttrOptionId).FirstOrDefault();

            }
            result.Reverse();
            return result;
        }

        /// <summary>
        /// Get category breadcrumb 
        /// </summary>
        /// <param name="category">Category</param>
        /// <param name="categoryService">Category service</param>
        /// <param name="aclService">ACL service</param>
        /// <param name="storeMappingService">Store mapping service</param>
        /// <param name="showHidden">A value indicating whether to load hidden records</param>
        /// <returns>Category breadcrumb </returns>
        public virtual IList<SpecificationAttributeOption> GetOptionBreadCrumb(SpecificationAttributeOption spo, ICollection<SpecificationAttributeOption>  allOptions)
        {
            var result = new List<SpecificationAttributeOption>();

            //used to prevent circular references
            var alreadyProcessedSpoIds = new List<string>();

            while (spo != null)  //not null                
            {
                result.Add(spo);
                alreadyProcessedSpoIds.Add(spo.Id);
                spo = allOptions.Where(s => s.Id == spo.ParentSpecificationAttrOptionId).FirstOrDefault();

            }
            result.Reverse();
            return result;
        }
        
        /// <summary>
        /// Get all Child Option
        /// </summary>
        /// <param name="spo">Category</param>
        /// <param name="allOptions">Category service</param>
        /// <returns>Category breadcrumb </returns>
        public virtual IList<SpecificationAttributeOption> GetOptionAllChild(SpecificationAttributeOption spoParant, ICollection<SpecificationAttributeOption> allOptions)
        {
            var result = new List<SpecificationAttributeOption>();

            //used to prevent circular references
            var alreadyProcessedSpoIds = new List<string>();
            if (spoParant == null)
            { return result; }

            result.Add(spoParant);

            var alChildrenOp = allOptions.Where(s => s.ParentSpecificationAttrOptionId == spoParant.Id);
            foreach (var childOp in alChildrenOp)
            {
                var childs = GetOptionAllChild(childOp, allOptions);
                foreach (var c in childs)
                {
                    if (!result.Contains(c))
                    { result.Add(c); }
                }
            }

            result.Reverse();
            return result;
        }

        /// <summary>
        /// Get formatted category breadcrumb 
        /// Note: ACL and store mapping is ignored
        /// </summary>
        /// <param name="category">Category</param>
        /// <param name="separator">Separator</param>
        /// <param name="languageId">Language identifier for localization</param>
        /// <returns>Formatted breadcrumb</returns>
        public virtual string GetFormattedOptionBreadCrumb(SpecificationAttributeOption spo, ICollection<SpecificationAttributeOption> allOptions, string separator = ">>", string languageId = "")
        {
            string result = string.Empty;

            var breadcrumb = GetOptionBreadCrumb(spo, allOptions);
            for (int i = 0; i <= breadcrumb.Count - 1; i++)
            {
                var optionName = breadcrumb[i].Name;
                result = String.IsNullOrEmpty(result)
                    ? optionName
                    : string.Format("{0} {1} {2}", result, separator, optionName);
            }
            return result;
        }
        /// <summary>
        /// Get formatted category breadcrumb 
        /// Note: ACL and store mapping is ignored
        /// </summary>
        /// <param name="category">Category</param>
        /// <param name="separator">Separator</param>
        /// <param name="languageId">Language identifier for localization</param>
        /// <returns>Formatted breadcrumb</returns>
        public virtual async Task<string> GetFormattedOptionBreadCrumb(SpecificationAttributeOption spo, string separator = ">>", string languageId = "")
        {
            string result = string.Empty;

            var breadcrumb = await GetOptionBreadCrumb(spo);
            for (int i = 0; i <= breadcrumb.Count - 1; i++)
            {
                var optionName = breadcrumb[i].Name;
                result = String.IsNullOrEmpty(result)
                    ? optionName
                    : string.Format("{0} {1} {2}", result, separator, optionName);
            }
            return result;
        }
        #endregion

        #endregion
    }
    }
