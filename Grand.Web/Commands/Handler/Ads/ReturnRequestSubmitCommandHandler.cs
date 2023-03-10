using Grand.Core;
using Grand.Domain.Customers;
using Grand.Domain.Localization;
using Grand.Domain.Ads;
using Grand.Services.Catalog;
using Grand.Services.Localization;
using Grand.Services.Messages;
using Grand.Services.Ads;
using Grand.Web.Commands.Models.Ads;
using Grand.Web.Models.Ads;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Web.Commands.Handler.Ads
{
    public class ReturnRequestSubmitCommandHandler : IRequestHandler<ReturnRequestSubmitCommandAd, (ReturnRequestModelAd model, ReturnRequestAd rr)>
    {
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly IProductService _productService;
        private readonly IReturnRequestServiceAd _returnRequestService;
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly ILocalizationService _localizationService;
        private readonly LocalizationSettings _localizationSettings;


        public ReturnRequestSubmitCommandHandler(IWorkContext workContext,
            IStoreContext storeContext,
            IProductService productService,
            IReturnRequestServiceAd returnRequestService,
            IWorkflowMessageService workflowMessageService,
            ILocalizationService localizationService,
            LocalizationSettings localizationSettings)
        {
            _workContext = workContext;
            _storeContext = storeContext;
            _productService = productService;
            _returnRequestService = returnRequestService;
            _workflowMessageService = workflowMessageService;
            _localizationService = localizationService;
            _localizationSettings = localizationSettings;
        }

        public async Task<(ReturnRequestModelAd model, ReturnRequestAd rr)> Handle(ReturnRequestSubmitCommandAd request, CancellationToken cancellationToken)
        {
            var rr = new ReturnRequestAd {
                StoreId = _storeContext.CurrentStore.Id,
                AdId = request.Ad.Id,
                CustomerId = _workContext.CurrentCustomer.Id,
                OwnerId = _workContext.CurrentCustomer.IsOwner() ? _workContext.CurrentCustomer.Id : _workContext.CurrentCustomer.OwnerId,
                CustomerComments = request.Model.Comments,
                StaffNotes = string.Empty,
                ReturnRequestStatus = ReturnRequestStatus.Pending,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow,
                PickupAddress = request.Address,
            };

            if (request.Model.PickupDate.HasValue)
                rr.PickupDate = request.Model.PickupDate.Value;
            var vendors = new List<string>();
            foreach (var orderItem in request.Ad.AdItems)
            {
                var product = await _productService.GetProductById(orderItem.ProductId);
                if (!product.NotReturnable)
                {
                    int quantity = 0; //parse quantity
                    string rrrId = "";
                    string rraId = "";

                    foreach (string formKey in request.Form.Keys)
                    {
                        if (formKey.Equals(string.Format("quantity{0}", orderItem.Id), StringComparison.OrdinalIgnoreCase))
                        {
                            int.TryParse(request.Form[formKey], out quantity);
                        }

                        if (formKey.Equals(string.Format("reason{0}", orderItem.Id), StringComparison.OrdinalIgnoreCase))
                        {
                            rrrId = request.Form[formKey];
                        }

                        if (formKey.Equals(string.Format("action{0}", orderItem.Id), StringComparison.OrdinalIgnoreCase))
                        {
                            rraId = request.Form[formKey];
                        }
                    }

                    if (quantity > 0)
                    {
                        var rrr = await _returnRequestService.GetReturnRequestReasonById(rrrId);
                        var rra = await _returnRequestService.GetReturnRequestActionById(rraId);
                        rr.ReturnRequestItems.Add(new ReturnRequestItemAd {
                            RequestedAction = rra != null ? rra.GetLocalized(x => x.Name, _workContext.WorkingLanguage.Id) : "not available",
                            ReasonForReturn = rrr != null ? rrr.GetLocalized(x => x.Name, _workContext.WorkingLanguage.Id) : "not available",
                            Quantity = quantity,
                            AdItemId = orderItem.Id
                        });
                        rr.VendorId = orderItem.VendorId;
                        vendors.Add(orderItem.VendorId);
                    }
                }
            }
            if (vendors.Distinct().Count() > 1)
            {
                request.Model.Error = _localizationService.GetResource("ReturnRequests.MultiVendorsItems");
                return (request.Model, rr);
            }
            if (rr.ReturnRequestItems.Any())
            {
                await _returnRequestService.InsertReturnRequest(rr);

                //notify store owner here (email)
                //await _workflowMessageService.SendNewReturnRequestStoreOwnerNotification(rr, request.Ad, _localizationSettings.DefaultAdminLanguageId);
                //notify customer
                //await _workflowMessageService.SendNewReturnRequestCustomerNotification(rr, request.Ad, request.Ad.CustomerLanguageId);
            }
            else
            {
                request.Model.Error = _localizationService.GetResource("ReturnRequests.NoItemsSubmitted");
            }

            return (request.Model, rr);
        }
    }
}
