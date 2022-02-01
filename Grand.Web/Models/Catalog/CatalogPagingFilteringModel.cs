using Grand.Core;
using Grand.Core.Caching;
using Grand.Domain.Catalog;
using Grand.Core.Models;
using Grand.Framework.UI.Paging;
using Grand.Services.Catalog;
using Grand.Services.Localization;
using Grand.Web.Infrastructure.Cache;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Grand.Domain.Directory;

namespace Grand.Web.Models.Catalog
{
    public partial class CatalogPagingFilteringModel : BasePageableModel
    {
        #region Constructors

        public CatalogPagingFilteringModel()
        {
            AvailableSortOptions = new List<SelectListItem>();
            AvailableViewModes = new List<SelectListItem>();
            PageSizeOptions = new List<SelectListItem>();

            PriceRangeFilter = new PriceRangeFilterModel();
            SpecificationFilter = new SpecificationFilterModel();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Price range filter model
        /// </summary>
        public PriceRangeFilterModel PriceRangeFilter { get; set; }

        /// <summary>
        /// Specification filter model
        /// </summary>
        public SpecificationFilterModel SpecificationFilter { get; set; }

        public bool AllowProductSorting { get; set; }
        public IList<SelectListItem> AvailableSortOptions { get; set; }

        public bool AllowProductViewModeChanging { get; set; }
        public IList<SelectListItem> AvailableViewModes { get; set; }

        public bool AllowCustomersToSelectPageSize { get; set; }
        public IList<SelectListItem> PageSizeOptions { get; set; }

        /// <summary>
        /// Order by
        /// </summary>
        public int? OrderBy { get; set; }

        /// <summary>
        /// Product sorting
        /// </summary>
        public string ViewMode { get; set; }


        #endregion

        #region Nested classes

        public partial class PriceRangeFilterModel : BaseModel
        {
            #region Const

            private const string QUERYSTRINGPARAM = "price";

            #endregion 

            #region Ctor

            public PriceRangeFilterModel()
            {
                Items = new List<PriceRangeFilterItem>();
            }

            #endregion

            #region Utilities

            /// <summary>
            /// Gets parsed price ranges
            /// </summary>
            protected virtual IList<PriceRange> GetPriceRangeList(string priceRangesStr)
            {
                var priceRanges = new List<PriceRange>();
                if (string.IsNullOrWhiteSpace(priceRangesStr))
                    return priceRanges;
                try
                {
                    string[] rangeArray = priceRangesStr.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (string str1 in rangeArray)
                    {
                        string[] fromTo = str1.Trim().Split(new[] { '-' });

                        decimal? from = null;
                        if (!String.IsNullOrEmpty(fromTo[0]) && !String.IsNullOrEmpty(fromTo[0].Trim()))
                            from = decimal.Parse(fromTo[0].Trim(), new CultureInfo("en-US"));

                        decimal? to = null;
                        if (!String.IsNullOrEmpty(fromTo[1]) && !String.IsNullOrEmpty(fromTo[1].Trim()))
                            to = decimal.Parse(fromTo[1].Trim(), new CultureInfo("en-US"));

                        priceRanges.Add(new PriceRange { From = from, To = to });
                    }
                    return priceRanges;
                }
                catch
                {
                    return priceRanges;
                }
            }

            protected virtual string ExcludeQueryStringParams(string url, IWebHelper webHelper)
            {
                const string excludedQueryStringParams = "pagenumber";
                var excludedQueryStringParamsSplitted = excludedQueryStringParams.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string exclude in excludedQueryStringParamsSplitted)
                    url = webHelper.ModifyQueryString(url, exclude, null);
                return url;
            }

            #endregion

            #region Methods

            public virtual PriceRange GetSelectedPriceRange(IWebHelper webHelper)
            {
                var range = webHelper.QueryString<string>(QUERYSTRINGPARAM);
                if (String.IsNullOrEmpty(range))
                    return null;
                string[] fromTo = range.Trim().Split(new[] { '-' });
                if (fromTo.Length == 2)
                {
                    decimal? from = null;
                    if (!String.IsNullOrEmpty(fromTo[0]) && !String.IsNullOrEmpty(fromTo[0].Trim()))
                        from = decimal.Parse(fromTo[0].Trim(), new CultureInfo("en-US"));
                    decimal? to = null;
                    if (!String.IsNullOrEmpty(fromTo[1]) && !String.IsNullOrEmpty(fromTo[1].Trim()))
                        to = decimal.Parse(fromTo[1].Trim(), new CultureInfo("en-US"));


                    return new PriceRange() {From = from, To = to};
                }
                return null;
            }

            public virtual PriceRange GetSelectedPriceRange(IWebHelper webHelper, string priceRangesStr)
            {
                var range = webHelper.QueryString<string>(QUERYSTRINGPARAM);
                if (String.IsNullOrEmpty(range))
                    return null;
                string[] fromTo = range.Trim().Split(new[] { '-' });
                if (fromTo.Length == 2)
                {
                    decimal? from = null;
                    if (!String.IsNullOrEmpty(fromTo[0]) && !String.IsNullOrEmpty(fromTo[0].Trim()))
                        from = decimal.Parse(fromTo[0].Trim(), new CultureInfo("en-US"));
                    decimal? to = null;
                    if (!String.IsNullOrEmpty(fromTo[1]) && !String.IsNullOrEmpty(fromTo[1].Trim()))
                        to = decimal.Parse(fromTo[1].Trim(), new CultureInfo("en-US"));

                    var priceRangeList = GetPriceRangeList(priceRangesStr);
                    foreach (var pr in priceRangeList)
                    {
                        if (pr.From == from && pr.To == to)
                            return pr;
                    }
                }
                return null;
            }


            private PriceRangeFilterItem rangeToFilter(PriceRange priceRange, IPriceFormatter priceFormatter, IWebHelper webHelper, Currency currency)
            {
                
               var item = new PriceRangeFilterItem();

               if (priceRange == null)
               {
                   return item;
               }

               item.Currency = currency.CurrencyCode;

               if (priceRange.From.HasValue)
               {
                   item.From = priceFormatter.FormatPrice(priceRange.From.Value, true, false);
                   item.FromValue = priceRange.From.Value;
               }

               if (priceRange.To.HasValue)
               {
                         item.To = priceFormatter.FormatPrice(priceRange.To.Value, true, false);
                         item.ToValue = priceRange.To.Value;
               }

                string fromQuery = string.Empty;
                if (priceRange.From.HasValue)
                    fromQuery = priceRange.From.Value.ToString(new CultureInfo("en-US"));
                string toQuery = string.Empty;
                if (priceRange.To.HasValue)
                    toQuery = priceRange.To.Value.ToString(new CultureInfo("en-US"));
                //filter URL
                string url = webHelper.ModifyQueryString(webHelper.GetThisPageUrl(true), QUERYSTRINGPARAM, $"{fromQuery}-{toQuery}");
                url = ExcludeQueryStringParams(url, webHelper);
                item.FilterUrl = url;
               
               
                return item;
            }

            public virtual void LoadPriceRangeFilter(PriceRange priceRange, IWebHelper webHelper, IPriceFormatter priceFormatter, Currency currency)
            {
                if (priceRange == null || priceRange.From == null || priceRange.To == null)
                {
                    this.Enabled = false;
                    return;
                }

                //from&to
              
                AvalibleRange = rangeToFilter(priceRange, priceFormatter, webHelper, currency);
                var selectedRange = GetSelectedPriceRange(webHelper);
                SelectedRange = rangeToFilter(selectedRange, priceFormatter, webHelper, currency);
             
                //remove filter URL
                string url = webHelper.ModifyQueryString(webHelper.GetThisPageUrl(true), QUERYSTRINGPARAM, null);
                url = ExcludeQueryStringParams(url, webHelper);
                this.RemoveFilterUrl = url;
                this.Enabled = true;
            }

            public virtual void LoadPriceRangeFilters(string priceRangeStr, IWebHelper webHelper, IPriceFormatter priceFormatter)
            {
                var priceRangeList = GetPriceRangeList(priceRangeStr);
                if (priceRangeList.Any())
                {
                    this.Enabled = true;

                    var selectedPriceRange = GetSelectedPriceRange(webHelper, priceRangeStr);

                    this.Items = priceRangeList.ToList().Select(x =>
                    {
                        //from&to
                        var item = new PriceRangeFilterItem();
                        if (x.From.HasValue)
                            item.From = priceFormatter.FormatPrice(x.From.Value, true, false);
                        if (x.To.HasValue)
                            item.To = priceFormatter.FormatPrice(x.To.Value, true, false);
                        string fromQuery = string.Empty;
                        if (x.From.HasValue)
                            fromQuery = x.From.Value.ToString(new CultureInfo("en-US"));
                        string toQuery = string.Empty;
                        if (x.To.HasValue)
                            toQuery = x.To.Value.ToString(new CultureInfo("en-US"));

                        //is selected?
                        if (selectedPriceRange != null
                            && selectedPriceRange.From == x.From
                            && selectedPriceRange.To == x.To)
                            item.Selected = true;

                        //filter URL
                        string url = webHelper.ModifyQueryString(webHelper.GetThisPageUrl(true), QUERYSTRINGPARAM, $"{fromQuery}-{toQuery}");
                        url = ExcludeQueryStringParams(url, webHelper);
                        item.FilterUrl = url;


                        return item;
                    }).ToList();

                    if (selectedPriceRange != null)
                    {
                        //remove filter URL
                        string url = webHelper.ModifyQueryString(webHelper.GetThisPageUrl(true), QUERYSTRINGPARAM, null);
                        url = ExcludeQueryStringParams(url, webHelper);
                        this.RemoveFilterUrl = url;
                    }
                }
                else
                {
                    this.Enabled = false;
                }
            }

            #endregion

            #region Properties
            public bool Enabled { get; set; }
            public IList<PriceRangeFilterItem> Items { get; set; }

            public PriceRangeFilterItem AvalibleRange { get; set; }

            public PriceRangeFilterItem SelectedRange { get; set; }
            public string RemoveFilterUrl { get; set; }

            #endregion
        }

        public partial class PriceRangeFilterItem : BaseModel
        {
            public string From { get; set; }

            public decimal FromValue { get; set; }

            public decimal ToValue { get; set; }

            public string To { get; set; }
            public string FilterUrl { get; set; }
            public bool Selected { get; set; }

            public string Currency { get; set; }
        }

        public partial class SpecificationFilterModel : BaseModel
        {
            #region Ctor

            public SpecificationFilterModel()
            {
                AlreadyFilteredItems = new List<SpecificationFilterItem>();
                NotFilteredItems = new List<SpecificationFilterItem>();
            }

            #endregion

            #region Utilities

            protected virtual string ExcludeQueryStringParams(string url, IWebHelper webHelper)
            {
                //comma separated list of parameters to exclude
                const string excludedQueryStringParams = "pagenumber";
                var excludedQueryStringParamsSplitted = excludedQueryStringParams.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string exclude in excludedQueryStringParamsSplitted)
                    url = webHelper.ModifyQueryString(url, exclude, null);
                return url;
            }

            protected virtual string GenerateFilteredSpecQueryParam(IList<string> optionIds)
            {
                if (optionIds == null)
                    return "";

                string result = string.Join(",", optionIds);
                return result;
            }

            #endregion

            #region Methods

            public virtual async Task<List<string>> GetAlreadyFilteredSpecOptionIds
                (IHttpContextAccessor httpContextAccessor, ISpecificationAttributeService specificationAttributeService)
            {
                var result = new List<string>();

                foreach (var item in httpContextAccessor.HttpContext.Request.Query)
                {
                    var spec = await specificationAttributeService.GetSpecificationAttributeBySeName(item.Key);
                    if (spec != null)
                    {
                        foreach (var value in item.Value)
                        {
                            foreach (var option in value.Split(","))
                            {
                                var opt = spec.SpecificationAttributeOptions.FirstOrDefault(x => x.SeName == option.ToLowerInvariant());
                                
                                if (opt != null)
                                {
                                    //select all child opt
                                    var allChildren = specificationAttributeService.GetOptionAllChild(opt, spec.SpecificationAttributeOptions);
                                    foreach (var childO in allChildren) 
                                    {
                                        if (!result.Contains(childO.Id))
                                            result.Add(childO.Id);
                                    }

                                    if (!result.Contains(opt.Id))
                                        result.Add(opt.Id);
                                }

                            }
                        }
                    }
                }

                return result;
            }

            public virtual async Task PrepareSpecsFilters(IList<string> alreadyFilteredSpecOptionIds,
                IList<string> filterableSpecificationAttributeOptionIds,
                ISpecificationAttributeService specificationAttributeService,
                IWebHelper webHelper, ICacheManager cacheManager, string langId)
            {
                Enabled = false;
                var optionIds = filterableSpecificationAttributeOptionIds != null
                    ? string.Join(",", filterableSpecificationAttributeOptionIds.Union(alreadyFilteredSpecOptionIds)) : string.Empty;
                var cacheKey = string.Format(ModelCacheEventConst.SPECS_FILTER_MODEL_KEY, optionIds, langId);

                var allFilters = await cacheManager.GetAsync(cacheKey, async () =>
                {
                    var _allFilters = new List<SpecificationAttributeOptionFilter>();
                    foreach (var sao in filterableSpecificationAttributeOptionIds.Union(alreadyFilteredSpecOptionIds))
                    {
                        var sa = await specificationAttributeService.GetSpecificationAttributeByOptionId(sao);
                        if (sa != null)
                        {
                            var saOption = sa.SpecificationAttributeOptions.FirstOrDefault(x => x.Id == sao);

                            if (string.IsNullOrEmpty(saOption.ParentSpecificationAttrOptionId) && !_allFilters.Any(s => s.SpecificationAttributeOptionId == sao))
                            {
                                _allFilters.Add(new SpecificationAttributeOptionFilter {
                                    SpecificationAttributeId = sa.Id,
                                    SpecificationAttributeName = sa.GetLocalized(x => x.Name, langId),
                                    SpecificationAttributeSeName = sa.SeName,
                                    SpecificationAttributeDisplayOrder = sa.DisplayOrder,
                                    SpecificationAttributeOptionId = sao,
                                    SpecificationAttributeOptionName = saOption.GetLocalized(x => x.Name, langId),
                                    SpecificationAttributeOptionSeName = saOption.SeName,
                                    SpecificationAttributeOptionDisplayOrder = saOption.DisplayOrder,
                                    SpecificationAttributeOptionParentSpecificationAttrOptionId = saOption.ParentSpecificationAttrOptionId,
                                    SpecificationAttributeOptionColorRgb = saOption.ColorSquaresRgb,
                                });
                            }
                            else 
                            {
                                var saParentOptions = specificationAttributeService.GetOptionBreadCrumb(saOption,sa.SpecificationAttributeOptions);
                                foreach (var saoP in saParentOptions) 
                                {
                                    if (!_allFilters.Any(s=>s.SpecificationAttributeOptionId == saoP.Id)) 
                                    {
                                        _allFilters.Add(new SpecificationAttributeOptionFilter {
                                            SpecificationAttributeId = sa.Id,
                                            SpecificationAttributeName = sa.GetLocalized(x => x.Name, langId),
                                            SpecificationAttributeSeName = sa.SeName,
                                            SpecificationAttributeDisplayOrder = sa.DisplayOrder,
                                            SpecificationAttributeOptionId = saoP.Id,
                                            SpecificationAttributeOptionName = saoP.GetLocalized(x => x.Name, langId),
                                            SpecificationAttributeOptionSeName = saoP.SeName,
                                            SpecificationAttributeOptionDisplayOrder = saoP.DisplayOrder,
                                            SpecificationAttributeOptionParentSpecificationAttrOptionId = saoP.ParentSpecificationAttrOptionId,
                                            SpecificationAttributeOptionColorRgb = saoP.ColorSquaresRgb,
                                        });
                                    }
                                }
                            }
                        }
                    }
                    return _allFilters.ToList();
                });

                if (!allFilters.Any())
                    return;

                //sort loaded options
                allFilters = allFilters.OrderBy(saof => saof.SpecificationAttributeDisplayOrder)
                    .ThenBy(saof => saof.SpecificationAttributeName)
                    .ThenBy(saof => saof.SpecificationAttributeOptionDisplayOrder)
                    .ThenBy(saof => saof.SpecificationAttributeOptionName).ToList();

                //prepare the model properties
                Enabled = true;
                string removeFilterUrl = webHelper.GetThisPageUrl(true);
                foreach (var item in allFilters.GroupBy(x => x.SpecificationAttributeSeName))
                {
                    removeFilterUrl = webHelper.ModifyQueryString(removeFilterUrl, item.Key, null);
                }
                RemoveFilterUrl = ExcludeQueryStringParams(removeFilterUrl, webHelper);

                //get already filtered specification options
                var alreadyFilteredOptions = allFilters.Where(x => alreadyFilteredSpecOptionIds.Contains(x.SpecificationAttributeOptionId));
                AlreadyFilteredItems = alreadyFilteredOptions.Select(x =>
                {
                    var alreadyFiltered = alreadyFilteredOptions.Where(y => y.SpecificationAttributeId == x.SpecificationAttributeId).Select(z => z.SpecificationAttributeOptionSeName)
                    .Except(new List<string> { x.SpecificationAttributeOptionSeName }).ToList();

                    var filterUrl = webHelper.ModifyQueryString(webHelper.GetThisPageUrl(true), x.SpecificationAttributeSeName, GenerateFilteredSpecQueryParam(alreadyFiltered));
                    
                    var parentChain = GetParentsChain(x,allFilters);

                    var parentName = string.Join(' ',parentChain.Select(c=>c.SpecificationAttributeOptionName));
                    
                    var filterItem = new SpecificationFilterItem {
                        SpecificationAttributeName = x.SpecificationAttributeName,
                        SpecificationAttributeSeName = x.SpecificationAttributeSeName,
                        SpecificationAttributeOptionName = parentName + x.SpecificationAttributeOptionName,
                        SpecificationAttributeOptionSeName = x.SpecificationAttributeOptionSeName,
                        SpecificationAttributeOptionColorRgb = x.SpecificationAttributeOptionColorRgb,
                        SpecificationAttributeOptionParentSpecificationAttrOptionId = x.SpecificationAttributeOptionParentSpecificationAttrOptionId,
                        FilterUrl = ExcludeQueryStringParams(filterUrl, webHelper),
                        Id = x.SpecificationAttributeOptionId
                    };
                    return filterItem;


                }).ToList();

                //get not filtered specification options
                NotFilteredItems = allFilters.Except(alreadyFilteredOptions).Select(x =>
                {
                    //filter URL
                    var alreadyFiltered = alreadyFilteredOptions.Where(y => y.SpecificationAttributeId == x.SpecificationAttributeId).Select(x => x.SpecificationAttributeOptionSeName)
                    .Concat(new List<string> { x.SpecificationAttributeOptionSeName });

                    var filterUrl = webHelper.ModifyQueryString(webHelper.GetThisPageUrl(true), x.SpecificationAttributeSeName, GenerateFilteredSpecQueryParam(alreadyFiltered.ToList()));
                    return new SpecificationFilterItem() {
                        SpecificationAttributeName = x.SpecificationAttributeName,
                        SpecificationAttributeSeName = x.SpecificationAttributeSeName,
                        SpecificationAttributeOptionName = x.SpecificationAttributeOptionName,
                        SpecificationAttributeOptionParentSpecificationAttrOptionId = x.SpecificationAttributeOptionParentSpecificationAttrOptionId,
                        SpecificationAttributeOptionSeName = x.SpecificationAttributeOptionSeName,
                        SpecificationAttributeOptionColorRgb = x.SpecificationAttributeOptionColorRgb,
                        Id = x.SpecificationAttributeOptionId,
                        FilterUrl = ExcludeQueryStringParams(filterUrl, webHelper)
                    };
                }).ToList();
            }

            #endregion

            #region Properties
            public bool Enabled { get; set; }
            public IList<SpecificationFilterItem> AlreadyFilteredItems { get; set; }
            public IList<SpecificationFilterItem> NotFilteredItems { get; set; }
            public string RemoveFilterUrl { get; set; }

            #endregion
        }

        private static IEnumerable<SpecificationAttributeOptionFilter> GetParentsChain(SpecificationAttributeOptionFilter item, IEnumerable<SpecificationAttributeOptionFilter> allSpecifications)
        {
            var parentSpec = allSpecifications.FirstOrDefault(a =>
                a.SpecificationAttributeOptionId == item.SpecificationAttributeOptionParentSpecificationAttrOptionId);

            if (parentSpec == null)
            {
                return new List<SpecificationAttributeOptionFilter>();
            }

            var parentChain = GetParentsChain(parentSpec, allSpecifications);
            return parentChain.Union(new List<SpecificationAttributeOptionFilter>{parentSpec});
        }

        public partial class SpecificationFilterItem : BaseModel
        {
            public string SpecificationAttributeName { get; set; }
            public string SpecificationAttributeSeName { get; set; }
            public string SpecificationAttributeOptionName { get; set; }
            public string SpecificationAttributeOptionSeName { get; set; }
            public string SpecificationAttributeOptionColorRgb { get; set; }
            public string FilterUrl { get; set; }
            public string SpecificationAttributeOptionParentSpecificationAttrOptionId { get; set; }

            public string Id { get; set; }
        }

        #endregion
    }
}