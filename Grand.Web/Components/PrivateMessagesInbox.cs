using Grand.Core;
using Grand.Domain.Customers;
using Grand.Domain.Forums;
using Grand.Framework.Components;
using Grand.Services.Customers;
using Grand.Services.Forums;
using Grand.Services.Helpers;
using Grand.Services.Localization;
using Grand.Services.Queries.Models.Ads;
using Grand.Web.Models.Common;
using Grand.Web.Models.PrivateMessages;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Grand.Services.Ads;
using Grand.Services.Catalog;

namespace Grand.Web.ViewComponents
{
    public class PrivateMessagesInboxViewComponent : BaseViewComponent
    {
        private readonly ForumSettings _forumSettings;
        private readonly IForumService _forumService;
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly ICustomerService _customerService;
        private readonly CustomerSettings _customerSettings;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly ILocalizationService _localizationService;
        private readonly IAdService _adService;
        private readonly IProductService _productService;
        readonly IMediator _mediator;

        public PrivateMessagesInboxViewComponent(ForumSettings forumSettings, IForumService forumService,
            IWorkContext workContext, IStoreContext storeContext, ICustomerService customerService,
            CustomerSettings customerSettings, IDateTimeHelper dateTimeHelper, ILocalizationService localizationService, IMediator mediator, IAdService adService,
            IProductService productService)
        {
            _forumSettings = forumSettings;
            _forumService = forumService;
            _workContext = workContext;
            _storeContext = storeContext;
            _customerService = customerService;
            _customerSettings = customerSettings;
            _dateTimeHelper = dateTimeHelper;
            _localizationService = localizationService;
            _mediator = mediator;
            _adService = adService;
            _productService = productService;
        }

        public async Task<IViewComponentResult> InvokeAsync(int pageNumber, string tab)
        {
            if (pageNumber > 0)
            {
                pageNumber -= 1;
            }

            var pageSize = _forumSettings.PrivateMessagesPageSize;

            var list = await _forumService.GetAllPrivateMessages(_storeContext.CurrentStore.Id,
                "", _workContext.CurrentCustomer.Id, "", null, null, false, string.Empty, pageNumber, pageSize);

            var query = new GetAdQuery {
                //StoreId = request.Store.Id
                CustomerId = _workContext.CurrentCustomer.Id
            };

            var ads = await _mediator.Send(query);

            var inbox = new List<PrivateMessageModel>();
            var groupAd = list.GroupBy(x => new { x.AdId, x.FromCustomerId });

            foreach (var pmG in groupAd)
            {
                var pm = pmG.Key;
                var lastMessage = pmG.OrderByDescending(x => x.CreatedOnUtc).FirstOrDefault();

                var fromCustomer = await _customerService.GetCustomerById(pm.FromCustomerId);
                var toCustomer = await _customerService.GetCustomerById(lastMessage.ToCustomerId);
                var ad = await _adService.GetAdById(pm.AdId);
                if (ad != null)
                {
                    var rp = await _productService.GetProductById(ad.ProductId);
                    inbox.Add(new PrivateMessageModel {
                        Id = lastMessage.Id,
                        FromCustomerId = fromCustomer.Id,
                        CustomerFromName = fromCustomer.FormatUserName(_customerSettings.CustomerNameFormat),
                        AllowViewingFromProfile = _customerSettings.AllowViewingProfiles && fromCustomer != null && !fromCustomer.IsGuest(),
                        ToCustomerId = toCustomer.Id,
                        CustomerToName = toCustomer.FormatUserName(_customerSettings.CustomerNameFormat),
                        AllowViewingToProfile = _customerSettings.AllowViewingProfiles && toCustomer != null && !toCustomer.IsGuest(),
                        Subject = lastMessage.Subject,
                        Message = lastMessage.Text,
                        CreatedOn = _dateTimeHelper.ConvertToUserTime(lastMessage.CreatedOnUtc, DateTimeKind.Utc),
                        IsRead = lastMessage.IsRead,
                        AdId = pm.AdId,
                        AdProductName = rp.Name
                    });
                }
            }

            var pagerModel = new PagerModel(_localizationService)
            {
                PageSize = list.PageSize,
                TotalRecords = list.TotalCount,
                PageIndex = list.PageIndex,
                ShowTotalSummary = false,
                RouteActionName = "PrivateMessagesPaged",
                UseRouteLinks = true,
                RouteValues = new PrivateMessageRouteValues { pageNumber = pageNumber, tab = tab }
            };

            var model = new PrivateMessageListModel
            {
                Messages = inbox,
                PagerModel = pagerModel
            };

            return View(model);

        }
    }
}