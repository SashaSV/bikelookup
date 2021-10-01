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

namespace Grand.Web.Features.Handlers.Ads
{
    public class EditAdHandler : IRequestHandler<EditAd, EditAdModel>
    {
        private readonly IAdService _adService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly ILocalizationService _localizationService;
        private readonly IAdProcessingService _adProcessingService;
        private readonly ICurrencyService _currencyService;
        private readonly IPriceFormatter _priceFormatter;
        private readonly IMediator _mediator;

        public EditAdHandler(
            IAdService adService,
            IDateTimeHelper dateTimeHelper,
            ILocalizationService localizationService,
            IAdProcessingService adProcessingService,
            ICurrencyService currencyService,
            IMediator mediator,
            IPriceFormatter priceFormatter)
        {
            _adService = adService;
            _dateTimeHelper = dateTimeHelper;
            _localizationService = localizationService;
            _adProcessingService = adProcessingService;
            _currencyService = currencyService;
            _priceFormatter = priceFormatter;
            _mediator = mediator;
        }

        public async Task<EditAdModel> Handle(EditAd request, CancellationToken cancellationToken)
        {
            var model = new EditAdModel() { WithDocuments = true};

            model.Id = request.Ad.Id;
            model.AdNumber = request.Ad.AdNumber;
            model.AdCode = request.Ad.Code;
            model.CreatedOn = _dateTimeHelper.ConvertToUserTime(request.Ad.CreatedOnUtc, DateTimeKind.Utc);
            model.AdStatus = request.Ad.AdStatus.GetLocalizedEnum(_localizationService, request.Language.Id);

            //await PrepareAd(model, request);
            //await PrepareRecurringPayments(model, request);
            return model;
        }
    }
}
