using Grand.Core;
using Grand.Domain.Customers;
using Grand.Domain.Shipping;
using Grand.Framework.Controllers;
using Grand.Services.Common;
using Grand.Services.Localization;
using Grand.Services.Payments;
using Grand.Services.Shipping;
using Grand.Web.Events;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Grand.Web.Extensions;
using Grand.Domain.Ads;
using Grand.Services.Ads;
using Grand.Web.Features.Models.Ads;
using Grand.Services.Commands.Models.Ads;
using Grand.Web.Commands.Models.Ads;
using Grand.Web.Features.Handlers.Ads;
using Grand.Web.Models.Ads;

namespace Grand.Web.Controllers
{
    public partial class AdController : BasePublicController
    {
        #region Fields

        private readonly IAdService _adService;
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        //private readonly IAdProcessingService _adsProcessingService;
        private readonly ILocalizationService _localizationService;
        private readonly IMediator _mediator;
        private readonly AdSettings _adSettings;

        #endregion

        #region Constructors

        public AdController(IAdService adService,
            IWorkContext workContext,
            IStoreContext storeContext,
            //IAdProcessingService adsProcessingService,
            ILocalizationService localizationService,
            IMediator mediator,
            AdSettings adSettings)
        {
            _adService = adService;
            _workContext = workContext;
            _storeContext = storeContext;
            //_adsProcessingService = adsProcessingService;
            _localizationService = localizationService;
            _mediator = mediator;
            _adSettings = adSettings;
        }

        #endregion

        #region Methods

        //My account / Ads
        public virtual async Task<IActionResult> CustomerAds()
        {
            if (!_workContext.CurrentCustomer.IsRegistered())
                return Challenge();

            var model = await _mediator.Send(new GetCustomerAdList() {
                Customer = _workContext.CurrentCustomer,
                Language = _workContext.WorkingLanguage,
                Store = _storeContext.CurrentStore
            });
            return View(model);
        }


        public virtual async Task<IActionResult> NewAd()
        {
            if (!_workContext.CurrentCustomer.IsRegistered())
                return Challenge();

            var model = await _mediator.Send(new NewAd() {
                Customer = _workContext.CurrentCustomer,
                Language = _workContext.WorkingLanguage,
                Store = _storeContext.CurrentStore
            });
            return View(model);
        }
        
        [HttpPost]
        public virtual async Task<IActionResult> NewAd([FromForm]NewAdModel newAdModel)
        {
            var saved = await _mediator.Send(new SaveAd()
            {
                AdToSave = newAdModel,
                Customer  = _workContext.CurrentCustomer,
                Store =  _storeContext.CurrentStore
            });
            //Url.RouteUrl("ViewAd", new { adId = product.AdId })
            return RedirectToRoute("CustomerAds");
        }

        public virtual async Task<IActionResult> EditAd(string AdId)
        {
            if (!_workContext.CurrentCustomer.IsRegistered())
                return Challenge();
            
            var Ad = await _adService.GetAdById(AdId);

            var model = await _mediator.Send(new EditAd() { Ad = Ad, Language = _workContext.WorkingLanguage });

            return View(model);
        }
        
        [HttpPost]
        public virtual async Task<IActionResult> EditAd([FromForm]EditAdModel editAdModel)
        {
            if (!_workContext.CurrentCustomer.IsRegistered())
                return Challenge();
            
            await _mediator.Send(new EditAdSave { Model = editAdModel });
           
            return RedirectToRoute("CustomerAds");
        }

        //My account / Ad details page / Cancel Unpaid Ad
        public virtual async Task<IActionResult> CancelAd(string AdId)
        {
            var Ad = await _adService.GetAdById(AdId);
            if (!_workContext.CurrentCustomer.IsRegistered())
                return Challenge();

            await _mediator.Send(new CancelAdCommand() { Ad = Ad, NotifyCustomer = true, NotifyStoreOwner = true });

            return RedirectToRoute("CustomerAds", new { AdId = AdId });
        }
        
        //My account / Ad details page / Cancel Unpaid Ad
        public virtual async Task<IActionResult> DeleteAd(string AdId)
        {
            var Ad = await _adService.GetAdById(AdId);
            //if (!Ad.Access(_workContext.CurrentCustomer) || Ad.PaymentStatus != Domain.Payments.PaymentStatus.Pending
            //    || (Ad.ShippingStatus != ShippingStatus.ShippingNotRequired && Ad.ShippingStatus != ShippingStatus.NotYetShipped)
            //    || Ad.AdStatus != AdStatus.Pending
            //    || !_adSettings.UserCanCancelUnpaidAd)
            
            if (!_workContext.CurrentCustomer.IsRegistered())
                return Challenge();

            var model = await _mediator.Send(new DeleteAd() { Ad = Ad, Language = _workContext.WorkingLanguage });

            return RedirectToRoute("CustomerAds");
        }

        public virtual async Task<IActionResult> ViewAd(string AdId)
        {
            var ad = await _adService.GetAdById(AdId);
            if (!ad.Access(_workContext.CurrentCustomer))
                return Challenge();

            var model = await _mediator.Send(new ViewAd() { Ad = ad, Language = _workContext.WorkingLanguage });

            return View(model);
        }
        
        public virtual async Task<IActionResult> MessagesAd(string AdId)
        {
            var Ad = await _adService.GetAdById(AdId);
            //if (!Ad.Access(_workContext.CurrentCustomer) || Ad.PaymentStatus != Domain.Payments.PaymentStatus.Pending
            //    || (Ad.ShippingStatus != ShippingStatus.ShippingNotRequired && Ad.ShippingStatus != ShippingStatus.NotYetShipped)
            //    || Ad.AdStatus != AdStatus.Pending
            //    || !_adSettings.UserCanCancelUnpaidAd)

            if (!_workContext.CurrentCustomer.IsRegistered())
                return Challenge();

            var model = await _mediator.Send(new MessagesAd() { Ad = Ad, Language = _workContext.WorkingLanguage });

            //return RedirectToRoute("MessagesAd", new { AdId = AdId });
            return View(model);
        }

        //My account / Ad details page
        public virtual async Task<IActionResult> Details(string AdId)
        {
            var ad = await _adService.GetAdById(AdId);
            if (!ad.Access(_workContext.CurrentCustomer))
                return Challenge();

            var model = await _mediator.Send(new GetAdDetails() { Ad = ad, Language = _workContext.WorkingLanguage });

            return View(model);
        }
        //My account / Ads / Cancel recurring Ad
        [HttpPost, ActionName("CustomerAds")]
        [AutoValidateAntiforgeryToken]
        [FormValueRequired(FormValueRequirement.StartsWith, "cancelRecurringPayment")]
        public virtual async Task<IActionResult> CancelRecurringPayment(IFormCollection form)
        {
            if (!_workContext.CurrentCustomer.IsRegistered())
                return Challenge();

            //get recurring payment identifier
            string recurringPaymentId = "";
            foreach (var formValue in form.Keys)
                if (formValue.StartsWith("cancelRecurringPayment", StringComparison.OrdinalIgnoreCase))
                    recurringPaymentId = formValue.Substring("cancelRecurringPayment".Length);

            //var recurringPayment = await _adService.GetRecurringPaymentById(recurringPaymentId);
            //if (recurringPayment == null)
            //{
            //    return RedirectToRoute("CustomerAds");
            //}

            //if (await _adsProcessingService.CanCancelRecurringPayment(_workContext.CurrentCustomer, recurringPayment))
            //{
            //    var errors = await _adsProcessingService.CancelRecurringPayment(recurringPayment);

            //    var model = await _mediator.Send(new GetCustomerAdList() {
            //        Customer = _workContext.CurrentCustomer,
            //        Language = _workContext.WorkingLanguage,
            //        Store = _storeContext.CurrentStore
            //    });
            //    model.CancelRecurringPaymentErrors = errors;

            //    return View(model);
            //}
            //else
            //{
            //    return RedirectToRoute("CustomerAds");
            //}
            return RedirectToRoute("CustomerAds");
        }




        //My account / Reward points
        public virtual async Task<IActionResult> CustomerRewardPoints([FromServices] RewardPointsSettings rewardPointsSettings)
        {
            if (!_workContext.CurrentCustomer.IsRegistered())
                return Challenge();

            if (!rewardPointsSettings.Enabled)
                return RedirectToRoute("CustomerInfo");

            //var model = await _mediator.Send(new GetCustomerRewardPoints() {
            //    Customer = _workContext.CurrentCustomer,
            //    Store = _storeContext.CurrentStore,
            //    Currency = _workContext.WorkingCurrency
            //});
            //return View(model);
            return Challenge();
        }

        //My account / Ad details page / Print
        public virtual async Task<IActionResult> PrintAdDetails(string AdId)
        {
            var Ad = await _adService.GetAdById(AdId);
            if (!Ad.Access(_workContext.CurrentCustomer))
                return Challenge();

            var model = await _mediator.Send(new GetAdDetails() { Ad = Ad, Language = _workContext.WorkingLanguage });
            model.PrintMode = true;

            return View("Details", model);
        }

        //My account / Ad details page / PDF invoice
        public virtual async Task<IActionResult> GetPdfInvoice(string AdId, [FromServices] IPdfService pdfService)
        {
            var Ad = await _adService.GetAdById(AdId);
            if (!Ad.Access(_workContext.CurrentCustomer))
                return Challenge();

            var Ads = new List<Ad>();
            Ads.Add(Ad);
            byte[] bytes;
            using (var stream = new MemoryStream())
            {
                //await pdfService.PrintAdsToPdf(stream, Ads, _workContext.WorkingLanguage.Id);
                bytes = stream.ToArray();
            }
            return File(bytes, "application/pdf", string.Format("Ad_{0}.pdf", Ad.Id));
        }

        //My account / Ad details page / Add Ad note
        public virtual async Task<IActionResult> AddAdNote(string AdId)
        {
            if (!_adSettings.AllowCustomerToAddAdsNote)
                return RedirectToRoute("HomePage");

            var Ad = await _adService.GetAdById(AdId);
            if (!Ad.Access(_workContext.CurrentCustomer))
                return Challenge();

            var model = new AddAdNoteModel();
            model.AdId = AdId;
            return View("AddAdNote", model);
        }

        //My account / Ad details page / Add Ad note
        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public virtual async Task<IActionResult> AddAdNote(AddAdNoteModel model)
        {
            if (!_adSettings.AllowCustomerToAddAdsNote)
                return RedirectToRoute("HomePage");

            if (!ModelState.IsValid)
            {
                return View("AddAdNote", model);
            }

            var Ad = await _adService.GetAdById(model.AdId);
            if (!Ad.Access(_workContext.CurrentCustomer))
                return Challenge();

            await _mediator.Send(new InsertAdNoteCommand() { Ad = Ad, AdNote = model, Language = _workContext.WorkingLanguage });

            //notification
            await _mediator.Publish(new AdNoteEvent(Ad, model));

            AddNotification(Framework.UI.NotifyType.Success, _localizationService.GetResource("AdNote.Added"), true);
            return RedirectToRoute("AdDetails", new { AdId = model.AdId });
        }

        //My account / Ad details page / re-Ad
        public virtual async Task<IActionResult> ReAd(string AdId)
        {
            var Ad = await _adService.GetAdById(AdId);
            if (!Ad.Access(_workContext.CurrentCustomer))
                return Challenge();

            var warnings = await _mediator.Send(new ReAdCommand() { Ad = Ad });

            if(warnings.Any())
                AddNotification(Framework.UI.NotifyType.Error, string.Join(",", warnings), true);

            return RedirectToRoute("ShoppingCart");
        }

        //My account / Ad details page / Complete payment
        [HttpPost, ActionName("Details")]
        [FormValueRequired("repost-payment")]
        [AutoValidateAntiforgeryToken]
        public virtual async Task<IActionResult> RePostPayment(string AdId, [FromServices] IWebHelper webHelper)
        {
            var ad = await _adService.GetAdById(AdId);
            if (!ad.Access(_workContext.CurrentCustomer))
                return Challenge();

            //if (!await _paymentService.CanRePostProcessPayment(Ad))
            //    return RedirectToRoute("AdDetails", new { AdId = AdId });

            //var postProcessPaymentRequest = new PostProcessPaymentRequest {
            //    Ad = Ad
            //};

            //await _paymentService.PostProcessPayment(postProcessPaymentRequest);

            if (webHelper.IsRequestBeingRedirected || webHelper.IsPostBeingDone)
            {
                //redirection or POST has been done in PostProcessPayment
                return Content("Redirected");
            }

            //if no redirection has been done (to a third-party payment page)
            //theoretically it's not possible
            return RedirectToRoute("AdDetails", new { AdId = AdId });
        }

        //My account / Ad details page / Shipment details page
        public virtual async Task<IActionResult> ShipmentDetails(string shipmentId, [FromServices] IShipmentService shipmentService)
        {
            var shipment = await shipmentService.GetShipmentById(shipmentId);
            if (shipment == null)
                return Challenge();

            var Ad = await _adService.GetAdById(shipment.AdId);
            if (!Ad.Access(_workContext.CurrentCustomer))
                return Challenge();

            //var model = await _mediator.Send(new GetShipmentDetails() {
            //    Customer = _workContext.CurrentCustomer,
            //    Language = _workContext.WorkingLanguage,
            //    //Ad = Ad,
            //    Shipment = shipment
            //});

            //return View(model);
            return Challenge();
        }

        #endregion
    }
}
