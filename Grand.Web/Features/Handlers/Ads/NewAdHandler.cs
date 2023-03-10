using Grand.Domain.Customers;
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
using System.Linq;
using static Grand.Web.Models.Ads.NewAdModel;

namespace Grand.Web.Features.Handlers.Ads
{
    public class NewAdHandler : IRequestHandler<NewAd, NewAdModel>
    {
        private readonly IAdService _adService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly ILocalizationService _localizationService;
        private readonly IAdProcessingService _adProcessingService;
        private readonly ICurrencyService _currencyService;
        private readonly IPriceFormatter _priceFormatter;
        private readonly IMediator _mediator;
        private readonly ISpecificationAttributeService _atributeService;
        
        public NewAdHandler(
            IAdService adService,
            IDateTimeHelper dateTimeHelper,
            ILocalizationService localizationService,
            IAdProcessingService adProcessingService,
            ICurrencyService currencyService,
            IMediator mediator,
            IPriceFormatter priceFormatter,
            ISpecificationAttributeService atributeService)
        {
            _adService = adService;
            _dateTimeHelper = dateTimeHelper;
            _localizationService = localizationService;
            _adProcessingService = adProcessingService;
            _currencyService = currencyService;
            _priceFormatter = priceFormatter;
            _mediator = mediator;
            _atributeService = atributeService;
        }

        public async Task<NewAdModel> Handle(NewAd request, CancellationToken cancellationToken)
        {
            var delivery = await _atributeService.GetSpecificationAttributeBySeName("v_delivery");
            var payment = await _atributeService.GetSpecificationAttributeBySeName("v_pay");

            
            var model = new NewAdModel() {
                WithDocuments = true,
                Year = DateTime.Now.Year,
                CollorAtribure = await _atributeService.GetSpecificationAttributeBySeName("sp_color"),
                ShippingMethodType = delivery?.SpecificationAttributeOptions?.Select(a=>new ShipmentMethodType{ Id = a.Id, Name = a.GetLocalized(x => x.Name, request.Language.Id)}).ToList(),
                //PaymentMethodType = payment?.SpecificationAttributeOptions?.Select(a => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem(a.GetLocalized(x => x.Name, request.Language.Id), a.Id)).ToList()
                PaymentMethodType = payment?.SpecificationAttributeOptions?.Select(a => new PaymentsMethodType { Id = a.Id, Name = a.GetLocalized(x => x.Name, request.Language.Id)}).ToList()
            };

            //await PrepareAd(model, request);
            //await PrepareRecurringPayments(model, request);
            return model;
        }

        private async Task PrepareAd(NewAdModel model, NewAd request)
        {
            //var query = new GetAdQuery {
            //    StoreId = request.Store.Id
            //};

            //if (!request.Customer.IsOwner())
            //    query.CustomerId = request.Customer.Id;
            //else
            //    query.OwnerId = request.Customer.Id;

            //var ads = await _mediator.Send(query);

            //foreach (var ad in ads)
            //{
            //    var adModel = new CustomerAdListModel.AdDetailsModel {
            //        Id = ad.Id,
            //        AdNumber = ad.AdNumber,
            //        AdCode = ad.Code,
            //        CustomerEmail = ad.BillingAddress?.Email,
            //        CreatedOn = _dateTimeHelper.ConvertToUserTime(ad.CreatedOnUtc, DateTimeKind.Utc),
            //        AdStatusEnum = ad.AdStatus,
            //        AdStatus = ad.AdStatus.GetLocalizedEnum(_localizationService, request.Language.Id),
            //        PaymentStatus = ad.PaymentStatus.GetLocalizedEnum(_localizationService, request.Language.Id),
            //        ShippingStatus = ad.ShippingStatus.GetLocalizedEnum(_localizationService, request.Language.Id),
            //        //IsReturnRequestAllowed = await _mediator.Send(new IsReturnRequestAllowedQuery() { Ad = ad })
            //    };
            //    //var adTotalInCustomerCurrency = _currencyService.ConvertCurrency(ad.AdTotal, ad.CurrencyRate);
            //    var adTotalInCustomerCurrency = 0;
            //    adModel.AdTotal = await _priceFormatter.FormatPrice(adTotalInCustomerCurrency, true, ad.CustomerCurrencyCode, false, request.Language);

            //    model.Ads.Add(adModel);
            //}
        }
        private async Task PrepareRecurringPayments(CustomerAdListModel model, GetCustomerAdList request)
        {
            var recurringPayments = await _adService.SearchRecurringPayments(request.Store.Id,
                request.Customer.Id);
            //foreach (var recurringPayment in recurringPayments)
            //{
            //    var recurringPaymentModel = new CustomerAdListModel.RecurringAdModel {
            //        Id = recurringPayment.Id,
            //        StartDate = _dateTimeHelper.ConvertToUserTime(recurringPayment.StartDateUtc, DateTimeKind.Utc).ToString(),
            //        CycleInfo = string.Format("{0} {1}", recurringPayment.CycleLength, recurringPayment.CyclePeriod.GetLocalizedEnum(_localizationService, request.Language.Id)),
            //        NextPayment = recurringPayment.NextPaymentDate.HasValue ? _dateTimeHelper.ConvertToUserTime(recurringPayment.NextPaymentDate.Value, DateTimeKind.Utc).ToString() : "",
            //        TotalCycles = recurringPayment.TotalCycles,
            //        CyclesRemaining = recurringPayment.CyclesRemaining,
            //        InitialAdId = recurringPayment.InitialAd.Id,
            //        CanCancel = await _adProcessingService.CanCancelRecurringPayment(request.Customer, recurringPayment),
            //    };

                //model.RecurringAds.Add(recurringPaymentModel);
            //}
        }

    }
}
