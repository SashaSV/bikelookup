using Grand.Domain.Ads;
using Grand.Domain.Catalog;
using Grand.Domain.Customers;
using Grand.Domain.Data;
using Grand.Services.Ads;
using Grand.Services.Catalog;
using Grand.Services.Directory;
using Grand.Services.Helpers;
using Grand.Services.Localization;
using Grand.Services.Queries.Models.Ads;
using Grand.Web.Features.Models.Ads;
using Grand.Web.Models.Ads;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Web.Features.Handlers.Ads
{
    public class EditAdHandler : IRequestHandler<EditAd, EditAdModel>
    {
        private readonly  IRepository<Ad>  _adRepository;
        private readonly IProductService _productService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly ILocalizationService _localizationService;
        private readonly IAdProcessingService _adProcessingService;
        private readonly ICurrencyService _currencyService;
        private readonly IPriceFormatter _priceFormatter;
        private readonly IMediator _mediator;

        public EditAdHandler(
            IRepository<Ad> adRepository,
            IDateTimeHelper dateTimeHelper,
            ILocalizationService localizationService,
            IAdProcessingService adProcessingService,
            ICurrencyService currencyService,
            IMediator mediator,
            IPriceFormatter priceFormatter,
            IProductService productService)
        {
            _adRepository = adRepository;
            _dateTimeHelper = dateTimeHelper;
            _localizationService = localizationService;
            _adProcessingService = adProcessingService;
            _currencyService = currencyService;
            _priceFormatter = priceFormatter;
            _mediator = mediator;
            _productService = productService;
        }

        public async Task<EditAdModel> Handle(EditAd request, CancellationToken cancellationToken)
        {
            var ad = await _adRepository.GetByIdAsync(request.Ad.Id);
            var product = await _productService.GetProductById(ad.AdItem.ProductId);
            
            var model = new EditAdModel() { WithDocuments = true};
      
            model.Id = request.Ad.Id;
            model.AdNumber = request.Ad.AdNumber;
            model.AdCode = request.Ad.Code;
            model.CreatedOn = _dateTimeHelper.ConvertToUserTime(request.Ad.CreatedOnUtc, DateTimeKind.Utc);
            model.AdStatus = request.Ad.AdStatus.GetLocalizedEnum(_localizationService, request.Language.Id);
            model.ManufactureName = product.ManufactureName;
            model.Price = product.Price;
            model.Model = product.Model;
            model.Color = product.Color;
            model.Size = product.Size;
            model.Weeldiam = product.Weeldiam;
            model.Year = string.IsNullOrEmpty(product.Year)? 0 :  int.Parse(product.Year);
            
            return model;
        }
    }
}
