using Grand.Core;
using Grand.Domain.Blogs;
using Grand.Domain.Catalog;
using Grand.Domain.Common;
using Grand.Domain.Media;
using Grand.Services.Blogs;
using Grand.Services.Catalog;
using Grand.Services.Common;
using Grand.Services.Directory;
using Grand.Services.Localization;
using Grand.Services.Media;
using Grand.Services.Queries.Models.Catalog;
using Grand.Services.Security;
using Grand.Services.Stores;
using Grand.Services.Tax;
using Grand.Web.Features.Models.Catalog;
using Grand.Web.Features.Models.Products;
using Grand.Web.Models.Catalog;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Web.Features.Handlers.Catalog
{
    public class GetSearchAutoSpecCompleteHandler : IRequestHandler<GetSpecAutoComplete, IList<SpecificationAttributeOption>>
    {
        private readonly IWebHelper _webHelper;
        private readonly IPictureService _pictureService;
        private readonly IManufacturerService _manufacturerService;
        private readonly ICategoryService _categoryService;
        private readonly IAclService _aclService;
        private readonly IStoreMappingService _storeMappingService;
        private readonly ISearchTermService _searchTermService;
        private readonly IBlogService _blogService;
        private readonly ITaxService _taxService;
        private readonly IPriceCalculationService _priceCalculationService;
        private readonly ICurrencyService _currencyService;
        private readonly IPriceFormatter _priceFormatter;
        private readonly IMediator _mediator;

        private readonly CatalogSettings _catalogSettings;
        private readonly MediaSettings _mediaSettings;
        private readonly BlogSettings _blogSettings;
        private readonly ISpecificationAttributeService _specificationAttributeService;
        private readonly ISpecificationAttributeService _specificationAttribute;
        public GetSearchAutoSpecCompleteHandler(
            IWebHelper webHelper,
            IPictureService pictureService,
            IManufacturerService manufacturerService,
            ICategoryService categoryService,
            IAclService aclService,
            IStoreMappingService storeMappingService,
            ISearchTermService searchTermService,
            IBlogService blogService,
            IPriceCalculationService priceCalculationService,
            ITaxService taxService,
            ICurrencyService currencyService,
            IPriceFormatter priceFormatter,
            IMediator mediator,
            CatalogSettings catalogSettings,
            MediaSettings mediaSettings,
            BlogSettings blogSettings,
            ISpecificationAttributeService specificationAttributeService,
            ISpecificationAttributeService specificationAttribute)
        {
            _webHelper = webHelper;
            _pictureService = pictureService;
            _manufacturerService = manufacturerService;
            _categoryService = categoryService;
            _aclService = aclService;
            _storeMappingService = storeMappingService;
            _searchTermService = searchTermService;
            _blogService = blogService;
            _priceCalculationService = priceCalculationService;
            _taxService = taxService;
            _currencyService = currencyService;
            _priceFormatter = priceFormatter;
            _mediator = mediator;
            _catalogSettings = catalogSettings;
            _mediaSettings = mediaSettings;
            _blogSettings = blogSettings;
            _specificationAttributeService = specificationAttributeService;
            _specificationAttribute = specificationAttribute;
        }

        public async Task<IList<SpecificationAttributeOption>> Handle(GetSpecAutoComplete request, CancellationToken cancellationToken)
        {
            var specification =
                await _specificationAttribute.GetSpecificationAttributeBySeName(request.Spec);
            
            var attributeOptions =
                await _specificationAttribute.GetSpecificationAttributeByOptionNameAutocomplete(specification.Id, request.Term);

            return attributeOptions.ToList();
        }
    }
}
