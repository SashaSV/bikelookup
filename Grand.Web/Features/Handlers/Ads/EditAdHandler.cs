using Grand.Core;
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
using Grand.Web.Features.Models.Products;
using Grand.Web.Models.Ads;
using MediatR;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static Grand.Web.Models.Ads.EditAdModel;

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
        private readonly IStoreContext _storeContext;
        private readonly ISpecificationAttributeService _atributeService;

        public EditAdHandler(
            IRepository<Ad> adRepository,
            IDateTimeHelper dateTimeHelper,
            ILocalizationService localizationService,
            IAdProcessingService adProcessingService,
            ICurrencyService currencyService,
            IMediator mediator,
            IPriceFormatter priceFormatter,
            IProductService productService,
            IStoreContext storeContext,
            ISpecificationAttributeService atributeService)
        {
            _adRepository = adRepository;
            _dateTimeHelper = dateTimeHelper;
            _localizationService = localizationService;
            _adProcessingService = adProcessingService;
            _currencyService = currencyService;
            _priceFormatter = priceFormatter;
            _mediator = mediator;
            _productService = productService;
            _storeContext = storeContext;
            _atributeService = atributeService;
        }

        public async Task<EditAdModel> Handle(EditAd request, CancellationToken cancellationToken)
        {
            var ad = await _adRepository.GetByIdAsync(request.Ad.Id);
            var product = await _productService.GetProductById(ad.ProductId);
            var productAssociated = await _productService.GetProductById(ad.AdItem.ProductId);
                
            var model = new EditAdModel() { WithDocuments = ad.WithDocuments};

            model.Id = request.Ad.Id;
            model.AdNumber = request.Ad.AdNumber;
            model.AdCode = request.Ad.Code;
            model.CreatedOn = _dateTimeHelper.ConvertToUserTime(request.Ad.CreatedOnUtc, DateTimeKind.Utc);
            model.AdStatus = request.Ad.AdStatus.GetLocalizedEnum(_localizationService, request.Language.Id);
            model.ManufactureName = productAssociated.ManufactureName;
            model.Price = decimal.ToInt32(productAssociated.Price);
            model.Model = productAssociated.Model;
            model.Size = productAssociated.Size;
            model.Color = productAssociated.Color;
            model.AdComment = ad.AdComment;
            model.IsAuction = ad.IsAuction;
            model.Mileage = ad.Mileage;
            model.WithDocuments = ad.WithDocuments;

            model.Items = new EditAdModel.AdItemModel() 
            {
                Id = ad.AdItem.Id,
                ProductId = product.Id,
                ProductName = product.Name,
                ProductSeName = product.SeName
            };
            model.Weeldiam = productAssociated.Weeldiam;
            model.Year = string.IsNullOrEmpty(productAssociated.Year)? 0 :  int.Parse(product.Year);
            model.CollorAtribure = await _atributeService.GetSpecificationAttributeBySeName("sp_color");
            // var productCollor =
            //     product.ProductSpecificationAttributes.FirstOrDefault(o =>
            //         o.SpecificationAttributeId == model.CollorAtribure.Id);
            //
            // model.Color = productCollor.SpecificationAttributeOptionId;

            var paymentAtribute = await _atributeService.GetSpecificationAttributeBySeName("v_pay");

            model.PaymentMethodType = paymentAtribute?.SpecificationAttributeOptions.Select(a => new PaymentsMethodType { Id = a.Id, Name = a.GetLocalized(x => x.Name, request.Language.Id) }).ToList();

            foreach (var paymentOption in productAssociated.ProductSpecificationAttributes.Where(psa => psa.SpecificationAttributeId == paymentAtribute.Id))
            {
                model.SelectedPaymentMethods.Add(paymentOption.SpecificationAttributeOptionId);
            }

            var delivery = await _atributeService.GetSpecificationAttributeBySeName("v_delivery");
            model.ShippingMethodType = delivery?.SpecificationAttributeOptions.Select(a => new ShipmentMethodType { Id = a.Id, Name = a.GetLocalized(x => x.Name, request.Language.Id) }).ToList();

            foreach (var paymentOption in productAssociated.ProductSpecificationAttributes.Where(psa => psa.SpecificationAttributeId == delivery.Id))
            {
                model.SelectedShippingMethods.Add(paymentOption.SpecificationAttributeOptionId);
            }

            var productModel = await _mediator.Send(new GetProductDetailsPage() {
                Store = _storeContext.CurrentStore,
                Product = productAssociated,
                IsAssociatedProduct = false,
                UpdateCartItem = null
            });
            model.PictureModels = productModel.PictureModels;
            
            return model;
        }
    }
}
