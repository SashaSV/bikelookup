using Grand.Domain.Customers;
using Grand.Services.Ads;
using Grand.Services.Catalog;
using Grand.Services.Directory;
using Grand.Services.Helpers;
using Grand.Services.Localization;
using Grand.Services.Queries.Models.Ads;
using Grand.Web.Features.Models.Ads;
using Grand.Web.Models.Ads;
using Grand.Web.Models.PrivateMessages;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Web.Features.Handlers.Ads
{
    public class MessagesAdHandler : IRequestHandler<MessagesAd, PrivateMessageIndexModel>
    {
        private readonly IAdService _adService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly ILocalizationService _localizationService;
        private readonly IAdProcessingService _adProcessingService;
        private readonly ICurrencyService _currencyService;
        private readonly IPriceFormatter _priceFormatter;
        private readonly IMediator _mediator;

        public MessagesAdHandler(
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

        public async Task<PrivateMessageIndexModel> Handle(MessagesAd request, CancellationToken cancellationToken)
        {
            int inboxPage = 0;
            int sentItemsPage = 0;
            bool sentItemsTabSelected = false;

            var tab = "";
            var pageNumber = 0;
            switch (tab)
            {
                case "inbox":
                    if (pageNumber > 0 )
                    {
                        inboxPage = pageNumber;
                    }
                    break;
                case "sent":
                    if (pageNumber > 0 )
                    {
                        sentItemsPage = pageNumber;
                    }
                    sentItemsTabSelected = true;
                    break;
                default:
                    break;
            }

            var model = new PrivateMessageIndexModel {
                InboxPage = inboxPage,
                SentItemsPage = sentItemsPage,
                SentItemsTabSelected = sentItemsTabSelected
            };

            return model;
        }
    }
}
