using Grand.Core;
using Grand.Domain.Catalog;
using Grand.Domain.Common;
using Grand.Domain.Customers;
using Grand.Domain.Ads;
using Grand.Framework.Controllers;
using Grand.Framework.Kendoui;
using Grand.Framework.Mvc;
using Grand.Framework.Security.Authorization;
using Grand.Services.Catalog;
using Grand.Services.Commands.Models.Ads;
using Grand.Services.Common;
using Grand.Services.ExportImport;
using Grand.Services.Localization;
using Grand.Services.Logging;
using Grand.Services.Ads;
using Grand.Services.Security;
using Grand.Services.Shipping;
using Grand.Web.Areas.Admin.Extensions;
using Grand.Web.Areas.Admin.Interfaces;
using Grand.Web.Areas.Admin.Models.Ads;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grand.Web.Areas.Admin.Controllers
{
    [PermissionAuthorize(PermissionSystemName.Ads)]
    public partial class AdController : BaseAdminController
    {
        #region Fields

        private readonly IAdViewModelService _adViewModelService;
        private readonly IAdService _adService;
        private readonly IAdProcessingService _adProcessingService;
        private readonly ILocalizationService _localizationService;
        private readonly IWorkContext _workContext;
        private readonly IPdfService _pdfService;
        private readonly IExportManager _exportManager;
        private readonly IMediator _mediator;

        #endregion

        #region Ctor

        public AdController(
            IAdViewModelService adViewModelService,
            IAdService adService,
            IAdProcessingService adProcessingService,
            ILocalizationService localizationService,
            IWorkContext workContext,
            IPdfService pdfService,
            IExportManager exportManager,
            IMediator mediator)
        {
            _adViewModelService = adViewModelService;
            _adService = adService;
            _adProcessingService = adProcessingService;
            _localizationService = localizationService;
            _workContext = workContext;
            _pdfService = pdfService;
            _exportManager = exportManager;
            _mediator = mediator;
        }

        #endregion

        #region Ad list

        public IActionResult Index() => RedirectToAction("List");

        public async Task<IActionResult> List(int? adStatusId = null,
            int? paymentStatusId = null, int? shippingStatusId = null, DateTime? startDate = null, string code = null)
        {
            var model = await _adViewModelService.PrepareAdListModel(adStatusId, paymentStatusId, shippingStatusId, startDate, _workContext.CurrentCustomer.StaffStoreId, code);
            return View(model);
        }

        public async Task<IActionResult> ProductSearchAutoComplete(string term, [FromServices] IProductService productService)
        {
            const int searchTermMinimumLength = 3;
            if (string.IsNullOrWhiteSpace(term) || term.Length < searchTermMinimumLength)
                return Content("");

            var storeId = string.Empty;
            if (_workContext.CurrentCustomer.IsStaff())
                storeId = _workContext.CurrentCustomer.StaffStoreId;

            string vendorId = string.Empty;
            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null)
            {
                vendorId = _workContext.CurrentVendor.Id;
            }
            //products
            const int productNumber = 15;
            var products = (await productService.SearchProducts(
                storeId: storeId,
                vendorId: vendorId,
                keywords: term,
                pageSize: productNumber,
                showHidden: true)).products;

            var result = (from p in products
                          select new
                          {
                              label = p.Name,
                              productid = p.Id
                          })
                .ToList();
            return Json(result);
        }

        [PermissionAuthorizeAction(PermissionActionName.List)]
        [HttpPost]
        public async Task<IActionResult> AdList(DataSourceRequest command, AdListModel model)
        {
            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && !_workContext.CurrentCustomer.IsStaff())
            {
                model.VendorId = _workContext.CurrentVendor.Id;
            }

            if (_workContext.CurrentCustomer.IsStaff())
            {
                model.StoreId = _workContext.CurrentCustomer.StaffStoreId;
            }

            var (adModels, aggreratorModel, totalCount) = await _adViewModelService.PrepareAdModel(model, command.Page, command.PageSize);

            var gridModel = new DataSourceResult {
                Data = adModels.ToList(),
                ExtraData = aggreratorModel,
                Total = totalCount
            };
            return Json(gridModel);
        }

        [PermissionAuthorizeAction(PermissionActionName.Preview)]
        [HttpPost, ActionName("List")]
        [FormValueRequired("go-to-ad-by-number")]
        public async Task<IActionResult> GoToAdId(AdListModel model)
        {
            Ad ad = null;
            int.TryParse(model.GoDirectlyToNumber, out var adNumber);
            if (adNumber > 0)
            {
                ad = await _adService.GetAdByNumber(adNumber);
            }
            var ads = await _adService.GetAdsByCode(model.GoDirectlyToNumber);
            if (ads.Count > 1)
            {
                return RedirectToAction("List", new { Code = model.GoDirectlyToNumber });
            }
            if (ads.Count == 1)
            {
                ad = ads.FirstOrDefault();
            }
            if (ad == null)
                return RedirectToAction("List");

            if (_workContext.CurrentCustomer.IsStaff() && ad.StoreId != _workContext.CurrentCustomer.StaffStoreId)
            {
                return RedirectToAction("List");
            }

            return RedirectToAction("Edit", "Ad", new { id = ad.Id });
        }

        #endregion

        #region Export / Import

        [PermissionAuthorizeAction(PermissionActionName.Export)]
        [HttpPost, ActionName("List")]
        [FormValueRequired("exportxml-all")]
        public async Task<IActionResult> ExportXmlAll(AdListModel model)
        {
            //a vendor cannot export ads
            if (_workContext.CurrentVendor != null && !_workContext.CurrentCustomer.IsStaff())
                return AccessDeniedView();

            if (_workContext.CurrentCustomer.IsStaff())
            {
                model.StoreId = _workContext.CurrentCustomer.StaffStoreId;
            }

            var ads = await _adViewModelService.PrepareAds(model);
            try
            {
                var xml = await _exportManager.ExportAdsToXml(ads);
                return File(Encoding.UTF8.GetBytes(xml), "application/xml", "ads.xml");
            }
            catch (Exception exc)
            {
                ErrorNotification(exc);
                return RedirectToAction("List");
            }
        }

        [PermissionAuthorizeAction(PermissionActionName.Export)]
        [HttpPost]
        public async Task<IActionResult> ExportXmlSelected(string selectedIds)
        {
            //a vendor cannot export ads
            if (_workContext.CurrentVendor != null && !_workContext.CurrentCustomer.IsStaff())
                return AccessDeniedView();

            var ads = new List<Ad>();
            if (selectedIds != null)
            {
                var ids = selectedIds
                    .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => x)
                    .ToArray();
                ads.AddRange(await _adService.GetAdsByIds(ids));
            }
            if (_workContext.CurrentCustomer.IsStaff())
            {
                ads = ads.Where(x => x.StoreId == _workContext.CurrentCustomer.StaffStoreId).ToList();
            }
            var xml = await _exportManager.ExportAdsToXml(ads);
            return File(Encoding.UTF8.GetBytes(xml), "application/xml", "ads.xml");
        }

        [PermissionAuthorizeAction(PermissionActionName.Export)]
        [HttpPost, ActionName("List")]
        [FormValueRequired("exportexcel-all")]
        public async Task<IActionResult> ExportExcelAll(AdListModel model)
        {
            //a vendor cannot export ads
            if (_workContext.CurrentVendor != null && !_workContext.CurrentCustomer.IsStaff())
                return AccessDeniedView();

            if (_workContext.CurrentCustomer.IsStaff())
            {
                model.StoreId = _workContext.CurrentCustomer.StaffStoreId;
            }

            //load ads
            var ads = await _adViewModelService.PrepareAds(model);
            try
            {
                byte[] bytes = _exportManager.ExportAdsToXlsx(ads);
                return File(bytes, "text/xls", "ads.xlsx");
            }
            catch (Exception exc)
            {
                ErrorNotification(exc);
                return RedirectToAction("List");
            }
        }

        [PermissionAuthorizeAction(PermissionActionName.Export)]
        [HttpPost]
        public async Task<IActionResult> ExportExcelSelected(string selectedIds)
        {
            //a vendor cannot export ads
            if (_workContext.CurrentVendor != null)
                return AccessDeniedView();

            var ads = new List<Ad>();
            if (selectedIds != null)
            {
                var ids = selectedIds
                    .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => x)
                    .ToArray();
                ads.AddRange(await _adService.GetAdsByIds(ids));
            }
            if (_workContext.CurrentCustomer.IsStaff())
            {
                ads = ads.Where(x => x.StoreId == _workContext.CurrentCustomer.StaffStoreId).ToList();
            }
            byte[] bytes = _exportManager.ExportAdsToXlsx(ads);
            return File(bytes, "text/xls", "ads.xlsx");
        }

        #endregion

        #region Ad details

        #region Payments and other ad workflow

        [PermissionAuthorizeAction(PermissionActionName.Cancel)]
        [HttpPost, ActionName("Edit")]
        [FormValueRequired("cancelad")]
        public async Task<IActionResult> CancelAd(string id)
        {
            var ad = await _adService.GetAdById(id);
            if (ad == null)
                //No ad found with the specified id
                return RedirectToAction("List");

            //a vendor does not have access to this functionality
            if (_workContext.CurrentVendor != null && !_workContext.CurrentCustomer.IsStaff())
                return RedirectToAction("Edit", "Ad", new { id = id });

            if (_workContext.CurrentCustomer.IsStaff() && ad.StoreId != _workContext.CurrentCustomer.StaffStoreId)
            {
                return RedirectToAction("List");
            }

            try
            {
                await _mediator.Send(new CancelAdCommand() { Ad = ad, NotifyCustomer = true });
                await _adViewModelService.LogEditAd(ad.Id);
                var model = new AdModel();
                await _adViewModelService.PrepareAdDetailsModel(model, ad);
                return View(model);
            }
            catch (Exception exc)
            {
                //error
                var model = new AdModel();
                await _adViewModelService.PrepareAdDetailsModel(model, ad);
                ErrorNotification(exc, false);
                return View(model);
            }
        }

        [PermissionAuthorizeAction(PermissionActionName.Payments)]
        [HttpPost, ActionName("Edit")]
        [FormValueRequired("capturead")]
        public async Task<IActionResult> CaptureAd(string id)
        {
            var ad = await _adService.GetAdById(id);
            if (ad == null)
                //No ad found with the specified id
                return RedirectToAction("List");

            //a vendor does not have access to this functionality
            if (_workContext.CurrentVendor != null && !_workContext.CurrentCustomer.IsStaff())
                return RedirectToAction("Edit", "Ad", new { id = id });

            if (_workContext.CurrentCustomer.IsStaff() && ad.StoreId != _workContext.CurrentCustomer.StaffStoreId)
            {
                return RedirectToAction("List");
            }

            try
            {
                var errors = await _adProcessingService.Capture(ad);
                await _adViewModelService.LogEditAd(ad.Id);
                var model = new AdModel();
                await _adViewModelService.PrepareAdDetailsModel(model, ad);
                foreach (var error in errors)
                    ErrorNotification(error, false);
                return View(model);
            }
            catch (Exception exc)
            {
                //error
                var model = new AdModel();
                await _adViewModelService.PrepareAdDetailsModel(model, ad);
                ErrorNotification(exc, false);
                return View(model);
            }

        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        [HttpPost, ActionName("Edit")]
        [FormValueRequired("btnSaveAdTags")]
        public async Task<IActionResult> SaveAdTags(AdModel adModel)
        {
            var ad = await _adService.GetAdById(adModel.Id);
            if (ad == null)
                //No ad found with the specified id
                return RedirectToAction("List");

            //a vendor does not have access to this functionality
            if (_workContext.CurrentVendor != null && !_workContext.CurrentCustomer.IsStaff())
                return RedirectToAction("Edit", "Ad", new { id = ad.Id });

            if (_workContext.CurrentCustomer.IsStaff() && ad.StoreId != _workContext.CurrentCustomer.StaffStoreId)
            {
                return RedirectToAction("List");
            }

            try
            {
                await _adViewModelService.SaveAdTags(ad, adModel.AdTags);
                await _adViewModelService.LogEditAd(ad.Id);
                var model = new AdModel();
                await _adViewModelService.PrepareAdDetailsModel(model, ad);
                return View(model);
            }
            catch (Exception exception)
            {
                //error
                var model = new AdModel();
                await _adViewModelService.PrepareAdDetailsModel(model, ad);
                ErrorNotification(exception, false);
                return View(model);
            }
        }

        [PermissionAuthorizeAction(PermissionActionName.Payments)]
        [HttpPost, ActionName("Edit")]
        [FormValueRequired("markadaspaid")]
        public async Task<IActionResult> MarkAdAsPaid(string id)
        {
            var ad = await _adService.GetAdById(id);
            if (ad == null)
                //No ad found with the specified id
                return RedirectToAction("List");

            //a vendor does not have access to this functionality
            if (_workContext.CurrentVendor != null && !_workContext.CurrentCustomer.IsStaff())
                return RedirectToAction("Edit", "Ad", new { id = id });

            if (_workContext.CurrentCustomer.IsStaff() && ad.StoreId != _workContext.CurrentCustomer.StaffStoreId)
            {
                return RedirectToAction("List");
            }

            try
            {
                await _adProcessingService.MarkAdAsPaid(ad);
                await _adViewModelService.LogEditAd(ad.Id);
                var model = new AdModel();
                await _adViewModelService.PrepareAdDetailsModel(model, ad);
                return View(model);
            }
            catch (Exception exc)
            {
                //error
                var model = new AdModel();
                await _adViewModelService.PrepareAdDetailsModel(model, ad);
                ErrorNotification(exc, false);
                return View(model);
            }
        }

        [PermissionAuthorizeAction(PermissionActionName.Payments)]
        [HttpPost, ActionName("Edit")]
        [FormValueRequired("refundad")]
        public async Task<IActionResult> RefundAd(string id)
        {
            var ad = await _adService.GetAdById(id);
            if (ad == null)
                //No ad found with the specified id
                return RedirectToAction("List");

            //a vendor does not have access to this functionality
            if (_workContext.CurrentVendor != null && !_workContext.CurrentCustomer.IsStaff())
                return RedirectToAction("Edit", "Ad", new { id = id });

            if (_workContext.CurrentCustomer.IsStaff() && ad.StoreId != _workContext.CurrentCustomer.StaffStoreId)
            {
                return RedirectToAction("List");
            }

            try
            {
                var errors = await _adProcessingService.Refund(ad);
                await _adViewModelService.LogEditAd(ad.Id);
                var model = new AdModel();
                await _adViewModelService.PrepareAdDetailsModel(model, ad);
                foreach (var error in errors)
                    ErrorNotification(error, false);
                return View(model);
            }
            catch (Exception exc)
            {
                //error
                var model = new AdModel();
                await _adViewModelService.PrepareAdDetailsModel(model, ad);
                ErrorNotification(exc, false);
                return View(model);
            }
        }

        [PermissionAuthorizeAction(PermissionActionName.Payments)]
        [HttpPost, ActionName("Edit")]
        [FormValueRequired("refundadoffline")]
        public async Task<IActionResult> RefundAdOffline(string id)
        {
            var ad = await _adService.GetAdById(id);
            if (ad == null)
                //No ad found with the specified id
                return RedirectToAction("List");

            //a vendor does not have access to this functionality
            if (_workContext.CurrentVendor != null && !_workContext.CurrentCustomer.IsStaff())
                return RedirectToAction("Edit", "Ad", new { id = id });

            if (_workContext.CurrentCustomer.IsStaff() && ad.StoreId != _workContext.CurrentCustomer.StaffStoreId)
            {
                return RedirectToAction("List");
            }

            try
            {
                await _adProcessingService.RefundOffline(ad);
                await _adViewModelService.LogEditAd(ad.Id);
                var model = new AdModel();
                await _adViewModelService.PrepareAdDetailsModel(model, ad);
                return View(model);
            }
            catch (Exception exc)
            {
                //error
                var model = new AdModel();
                await _adViewModelService.PrepareAdDetailsModel(model, ad);
                ErrorNotification(exc, false);
                return View(model);
            }
        }

        [PermissionAuthorizeAction(PermissionActionName.Payments)]
        [HttpPost, ActionName("Edit")]
        [FormValueRequired("voidad")]
        public async Task<IActionResult> VoidAd(string id)
        {
            var ad = await _adService.GetAdById(id);
            if (ad == null)
                //No ad found with the specified id
                return RedirectToAction("List");

            //a vendor does not have access to this functionality
            if (_workContext.CurrentVendor != null && !_workContext.CurrentCustomer.IsStaff())
                return RedirectToAction("Edit", "Ad", new { id = id });

            if (_workContext.CurrentCustomer.IsStaff() && ad.StoreId != _workContext.CurrentCustomer.StaffStoreId)
            {
                return RedirectToAction("List");
            }

            try
            {
                var errors = await _adProcessingService.Void(ad);
                await _adViewModelService.LogEditAd(ad.Id);
                var model = new AdModel();
                await _adViewModelService.PrepareAdDetailsModel(model, ad);
                foreach (var error in errors)
                    ErrorNotification(error, false);
                return View(model);
            }
            catch (Exception exc)
            {
                //error
                var model = new AdModel();
                await _adViewModelService.PrepareAdDetailsModel(model, ad);
                ErrorNotification(exc, false);
                return View(model);
            }
        }

        [PermissionAuthorizeAction(PermissionActionName.Payments)]
        [HttpPost, ActionName("Edit")]
        [FormValueRequired("voidadoffline")]
        public async Task<IActionResult> VoidAdOffline(string id)
        {
            var ad = await _adService.GetAdById(id);
            if (ad == null)
                //No ad found with the specified id
                return RedirectToAction("List");

            //a vendor does not have access to this functionality
            if (_workContext.CurrentVendor != null && !_workContext.CurrentCustomer.IsStaff())
                return RedirectToAction("Edit", "Ad", new { id = id });

            if (_workContext.CurrentCustomer.IsStaff() && ad.StoreId != _workContext.CurrentCustomer.StaffStoreId)
            {
                return RedirectToAction("List");
            }

            try
            {
                await _adProcessingService.VoidOffline(ad);
                await _adViewModelService.LogEditAd(ad.Id);
                var model = new AdModel();
                await _adViewModelService.PrepareAdDetailsModel(model, ad);
                return View(model);
            }
            catch (Exception exc)
            {
                //error
                var model = new AdModel();
                await _adViewModelService.PrepareAdDetailsModel(model, ad);
                ErrorNotification(exc, false);
                return View(model);
            }
        }

        [PermissionAuthorizeAction(PermissionActionName.Payments)]
        public async Task<IActionResult> PartiallyRefundAdPopup(string id, bool online)
        {
            var ad = await _adService.GetAdById(id);
            if (ad == null)
                //No ad found with the specified id
                return RedirectToAction("List");

            //a vendor does not have access to this functionality
            if (_workContext.CurrentVendor != null && !_workContext.CurrentCustomer.IsStaff())
                return RedirectToAction("Edit", "Ad", new { id = id });

            if (_workContext.CurrentCustomer.IsStaff() && ad.StoreId != _workContext.CurrentCustomer.StaffStoreId)
            {
                return RedirectToAction("List");
            }

            var model = new AdModel();
            await _adViewModelService.PrepareAdDetailsModel(model, ad);

            return View(model);
        }

        [PermissionAuthorizeAction(PermissionActionName.Payments)]
        [HttpPost]
        [FormValueRequired("partialrefundad")]
        public async Task<IActionResult> PartiallyRefundAdPopup(string id, bool online, AdModel model)
        {
            var ad = await _adService.GetAdById(id);
            if (ad == null)
                //No ad found with the specified id
                return RedirectToAction("List");

            //a vendor does not have access to this functionality
            if (_workContext.CurrentVendor != null && !_workContext.CurrentCustomer.IsStaff())
                return RedirectToAction("Edit", "Ad", new { id = id });

            if (_workContext.CurrentCustomer.IsStaff() && ad.StoreId != _workContext.CurrentCustomer.StaffStoreId)
            {
                return RedirectToAction("List");
            }

            try
            {
                decimal amountToRefund = model.AmountToRefund;
                if (amountToRefund <= decimal.Zero)
                    throw new GrandException("Enter amount to refund");

                decimal maxAmountToRefund = ad.AdTotal - ad.RefundedAmount;
                if (amountToRefund > maxAmountToRefund)
                    amountToRefund = maxAmountToRefund;

                var errors = new List<string>();
                if (online)
                    errors = (await _adProcessingService.PartiallyRefund(ad, amountToRefund)).ToList();
                else
                    await _adProcessingService.PartiallyRefundOffline(ad, amountToRefund);

                await _adViewModelService.LogEditAd(ad.Id);
                if (errors.Count == 0)
                {
                    //success
                    ViewBag.RefreshPage = true;
                    await _adViewModelService.PrepareAdDetailsModel(model, ad);
                    return View(model);
                }
                //error
                await _adViewModelService.PrepareAdDetailsModel(model, ad);
                foreach (var error in errors)
                    ErrorNotification(error, false);
                return View(model);
            }
            catch (Exception exc)
            {
                //error
                await _adViewModelService.PrepareAdDetailsModel(model, ad);
                ErrorNotification(exc, false);
                return View(model);
            }
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        [HttpPost, ActionName("Edit")]
        [FormValueRequired("btnSaveAdStatus")]
        public async Task<IActionResult> ChangeAdStatus(string id, AdModel model)
        {
            var ad = await _adService.GetAdById(id);
            if (ad == null)
                //No ad found with the specified id
                return RedirectToAction("List");

            //a vendor does not have access to this functionality
            if (_workContext.CurrentVendor != null && !_workContext.CurrentCustomer.IsStaff())
                return RedirectToAction("Edit", "Ad", new { id = id });

            if (_workContext.CurrentCustomer.IsStaff() && ad.StoreId != _workContext.CurrentCustomer.StaffStoreId)
            {
                return RedirectToAction("List");
            }

            try
            {
                ad.AdStatusId = model.AdStatusId;
                await _adService.UpdateAd(ad);

                //add a note
                await _adService.InsertAdNote(new AdNote {
                    Note = string.Format("Ad status has been edited. New status: {0}", ad.AdStatus.GetLocalizedEnum(_localizationService, _workContext)),
                    DisplayToCustomer = false,
                    CreatedOnUtc = DateTime.UtcNow,
                    AdId = ad.Id,

                });
                await _adViewModelService.LogEditAd(ad.Id);
                model = new AdModel();
                await _adViewModelService.PrepareAdDetailsModel(model, ad);
                return View(model);
            }
            catch (Exception exc)
            {
                //error
                model = new AdModel();
                await _adViewModelService.PrepareAdDetailsModel(model, ad);
                ErrorNotification(exc, false);
                return View(model);
            }
        }

        #endregion

        #region Edit, delete

        [PermissionAuthorizeAction(PermissionActionName.Preview)]
        public async Task<IActionResult> Edit(string id)
        {
            var ad = await _adService.GetAdById(id);
            if (ad == null || ad.Deleted)
                //No ad found with the specified id
                return RedirectToAction("List");

            //a vendor does not have access to this functionality
            //if (_workContext.CurrentVendor != null && !_workContext.HasAccessToAd(ad) && !_workContext.CurrentCustomer.IsStaff())
            //    return RedirectToAction("List");

            if (_workContext.CurrentCustomer.IsStaff() && ad.StoreId != _workContext.CurrentCustomer.StaffStoreId)
            {
                return RedirectToAction("List");
            }

            var model = new AdModel();
            await _adViewModelService.PrepareAdDetailsModel(model, ad);

            return View(model);
        }

        [PermissionAuthorizeAction(PermissionActionName.Delete)]
        [HttpPost]
        public async Task<IActionResult> Delete(string id, [FromServices] ICustomerActivityService customerActivityService)
        {
            var ad = await _adService.GetAdById(id);
            if (ad == null)
                //No ad found with the specified id
                return RedirectToAction("List");

            //a vendor or staff does not have access to this functionality
            if (_workContext.CurrentVendor != null || _workContext.CurrentCustomer.IsStaff())
                return RedirectToAction("Edit", "Ad", new { id = id });

            if (ModelState.IsValid)
            {
                await _mediator.Send(new DeleteAdCommand() { Ad = ad });
                await customerActivityService.InsertActivity("DeleteAd", id, _localizationService.GetResource("ActivityLog.DeleteAd"), ad.Id);
                return RedirectToAction("List");
            }
            ErrorNotification(ModelState);
            return RedirectToAction("Edit", "Ad", new { id = id });
        }

        [PermissionAuthorizeAction(PermissionActionName.Delete)]
        [HttpPost]
        public async Task<IActionResult> DeleteSelected(ICollection<string> selectedIds, [FromServices] ICustomerActivityService customerActivityService)
        {
            if (_workContext.CurrentVendor != null || _workContext.CurrentCustomer.IsStaff())
                return RedirectToAction("List", "Ad");

            if (selectedIds != null)
            {
                var ads = new List<Ad>();
                ads.AddRange(await _adService.GetAdsByIds(selectedIds.ToArray()));
                for (var i = 0; i < ads.Count; i++)
                {
                    var ad = ads[i];
                    await _adService.DeleteAd(ad);
                    await customerActivityService.InsertActivity("DeleteAd", ad.Id, _localizationService.GetResource("ActivityLog.DeleteAd"), ad.Id);
                }
            }

            return Json(new { Result = true });
        }

        public async Task<IActionResult> PdfInvoice(string adId)
        {
            var vendorId = string.Empty;

            //a vendor does not have access to this functionality
            if (_workContext.CurrentVendor != null && !_workContext.CurrentCustomer.IsStaff())
            {
                vendorId = _workContext.CurrentVendor.Id;
            }

            var ad = await _adService.GetAdById(adId);
            if (_workContext.CurrentCustomer.IsStaff() && ad.StoreId != _workContext.CurrentCustomer.StaffStoreId)
            {
                return RedirectToAction("List");
            }

            var ads = new List<Ad>
            {
                ad
            };
            byte[] bytes;
            using (var stream = new MemoryStream())
            {
                //await _pdfService.PrintAdsToPdf(stream, ads, _workContext.WorkingLanguage.Id, vendorId);
                bytes = stream.ToArray();
            }
            return File(bytes, "application/pdf", string.Format("ad_{0}.pdf", ad.Id));
        }

        [PermissionAuthorizeAction(PermissionActionName.Export)]
        [HttpPost, ActionName("List")]
        [FormValueRequired("pdf-invoice-all")]
        public async Task<IActionResult> PdfInvoiceAll(AdListModel model)
        {
            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && !_workContext.CurrentCustomer.IsStaff())
            {
                model.VendorId = _workContext.CurrentVendor.Id;
            }
            //load ads
            var ads = await _adViewModelService.PrepareAds(model);
            if (_workContext.CurrentCustomer.IsStaff())
            {
                ads = ads.Where(x => x.StoreId == _workContext.CurrentCustomer.StaffStoreId).ToList();
            }

            byte[] bytes;
            using (var stream = new MemoryStream())
            {
                //await _pdfService.PrintAdsToPdf(stream, ads, _workContext.WorkingLanguage.Id, model.VendorId);
                bytes = stream.ToArray();
            }
            return File(bytes, "application/pdf", "ads.pdf");
        }

        [PermissionAuthorizeAction(PermissionActionName.Export)]
        [HttpPost]
        public async Task<IActionResult> PdfInvoiceSelected(string selectedIds)
        {
            var ads = new List<Ad>();
            if (selectedIds != null)
            {
                var ids = selectedIds
                    .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => x)
                    .ToArray();
                ads.AddRange(await _adService.GetAdsByIds(ids));
            }
            var vendorId = string.Empty;
            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && !_workContext.CurrentCustomer.IsStaff())
            {
                ads = ads.Where(_workContext.HasAccessToAd).ToList();
                vendorId = _workContext.CurrentVendor.Id;
            }
            if (_workContext.CurrentCustomer.IsStaff())
            {
                ads = ads.Where(x => x.StoreId == _workContext.CurrentCustomer.StaffStoreId).ToList();
            }

            //ensure that we at least one ad selected
            if (ads.Count == 0)
            {
                ErrorNotification(_localizationService.GetResource("Admin.Ads.PdfInvoice.NoAds"));
                return RedirectToAction("List");
            }

            byte[] bytes;
            using (var stream = new MemoryStream())
            {
                //await _pdfService.PrintAdsToPdf(stream, ads, _workContext.WorkingLanguage.Id, vendorId);
                bytes = stream.ToArray();
            }
            return File(bytes, "application/pdf", "ads.pdf");
        }

        [PermissionAuthorizeAction(PermissionActionName.Payments)]
        [HttpPost, ActionName("Edit")]
        [FormValueRequired("btnSaveCC")]
        public async Task<IActionResult> EditCreditCardInfo(string id, AdModel model)
        {
            var ad = await _adService.GetAdById(id);
            if (ad == null)
                //No ad found with the specified id
                return RedirectToAction("List");

            //a vendor does not have access to this functionality
            if (_workContext.CurrentVendor != null && !_workContext.CurrentCustomer.IsStaff())
                return RedirectToAction("Edit", "Ad", new { id = id });

            if (_workContext.CurrentCustomer.IsStaff() && ad.StoreId != _workContext.CurrentCustomer.StaffStoreId)
            {
                return RedirectToAction("Edit", "Ad", new { id = id });
            }

            if (ad.AllowStoringCreditCardNumber)
            {
                await _adViewModelService.EditCreditCardInfo(ad, model);
            }

            //add a note
            await _adService.InsertAdNote(new AdNote {
                Note = "Credit card info has been edited",
                DisplayToCustomer = false,
                CreatedOnUtc = DateTime.UtcNow,
                AdId = ad.Id,
            });
            await _adViewModelService.LogEditAd(ad.Id);
            await _adViewModelService.PrepareAdDetailsModel(model, ad);
            return View(model);
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        [HttpPost, ActionName("Edit")]
        [FormValueRequired("btnSaveAdTotals")]
        public async Task<IActionResult> EditAdTotals(string id, AdModel model)
        {
            var ad = await _adService.GetAdById(id);
            if (ad == null)
                //No ad found with the specified id
                return RedirectToAction("List");

            //a vendor does not have access to this functionality
            if (_workContext.CurrentVendor != null && !_workContext.CurrentCustomer.IsStaff())
                return RedirectToAction("Edit", "Ad", new { id = id });

            if (_workContext.CurrentCustomer.IsStaff() && ad.StoreId != _workContext.CurrentCustomer.StaffStoreId)
            {
                return RedirectToAction("Edit", "Ad", new { id = id });
            }

            ad.AdSubTotalDiscountExclTax = model.AdSubTotalDiscountExclTaxValue;
            ad.AdShippingInclTax = model.AdShippingInclTaxValue;
            ad.AdShippingExclTax = model.AdShippingExclTaxValue;
            ad.PaymentMethodAdditionalFeeInclTax = model.PaymentMethodAdditionalFeeInclTaxValue;
            ad.PaymentMethodAdditionalFeeExclTax = model.PaymentMethodAdditionalFeeExclTaxValue;
            ad.TaxRates = model.TaxRatesValue;
            ad.AdTax = model.TaxValue;
            //ad.AdSubtotalInclTax = model.AdSubtotalInclTaxValue;
            //ad.AdSubtotalExclTax = model.AdSubtotalExclTaxValue;
            //ad.AdSubTotalDiscountInclTax = model.AdSubTotalDiscountInclTaxValue;
            //ad.AdDiscount = model.AdTotalDiscountValue;
            ad.AdTotal = model.AdTotalValue;
            ad.CurrencyRate = model.CurrencyRate;
            await _adService.UpdateAd(ad);

            //add a note
            await _adService.InsertAdNote(new AdNote {
                Note = "Ad totals have been edited",
                DisplayToCustomer = false,
                CreatedOnUtc = DateTime.UtcNow,
                AdId = ad.Id,
            });

            await _adViewModelService.LogEditAd(ad.Id);
            await _adViewModelService.PrepareAdDetailsModel(model, ad);
            return View(model);
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        [HttpPost, ActionName("Edit")]
        [FormValueRequired("save-shipping-method")]
        public async Task<IActionResult> EditShippingMethod(string id, AdModel model)
        {
            var ad = await _adService.GetAdById(id);
            if (ad == null)
                //No ad found with the specified id
                return RedirectToAction("List");

            //a vendor does not have access to this functionality
            if (_workContext.CurrentVendor != null && !_workContext.CurrentCustomer.IsStaff())
                return RedirectToAction("Edit", "Ad", new { id = id });

            if (_workContext.CurrentCustomer.IsStaff() && ad.StoreId != _workContext.CurrentCustomer.StaffStoreId)
            {
                return RedirectToAction("Edit", "Ad", new { id = id });
            }

            ad.ShippingMethod = model.ShippingMethod;
            await _adService.UpdateAd(ad);

            //add a note
            await _adService.InsertAdNote(new AdNote {
                Note = "Shipping method has been edited",
                DisplayToCustomer = false,
                CreatedOnUtc = DateTime.UtcNow,
                AdId = ad.Id,
            });
            await _adViewModelService.LogEditAd(ad.Id);
            await _adViewModelService.PrepareAdDetailsModel(model, ad);

            //selected tab
            await SaveSelectedTabIndex(persistForTheNextRequest: false);

            return View(model);
        }

        [HttpPost, ActionName("Edit")]
        [FormValueRequired("save-generic-attributes")]
        public async Task<IActionResult> EditGenericAttributes(string id, AdModel model)
        {
            var ad = await _adService.GetAdById(id);
            if (ad == null)
                //No ad found with the specified id
                return RedirectToAction("List");

            //a vendor does not have access to this functionality
            if (_workContext.CurrentVendor != null && !_workContext.CurrentCustomer.IsStaff())
                return RedirectToAction("Edit", "Ad", new { id = id });

            if (_workContext.CurrentCustomer.IsStaff() && ad.StoreId != _workContext.CurrentCustomer.StaffStoreId)
            {
                return RedirectToAction("Edit", "Ad", new { id = id });
            }

            ad.GenericAttributes = model.GenericAttributes;

            await _adService.UpdateAd(ad);
            await _adViewModelService.LogEditAd(ad.Id);

            await _adViewModelService.PrepareAdDetailsModel(model, ad);

            //selected tab
            await SaveSelectedTabIndex(persistForTheNextRequest: false);

            return View(model);
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        [HttpPost, ActionName("Edit")]
        [FormValueRequired(FormValueRequirement.StartsWith, "btnSaveAdItem")]
        public async Task<IActionResult> EditAdItem(string id, IFormCollection form, [FromServices] IProductService productService)
        {
            var ad = await _adService.GetAdById(id);
            if (ad == null)
                //No ad found with the specified id
                return RedirectToAction("List");

            //a vendor does not have access to this functionality
            if (_workContext.CurrentVendor != null && !_workContext.CurrentCustomer.IsStaff())
                return RedirectToAction("Edit", "Ad", new { id = id });

            if (_workContext.CurrentCustomer.IsStaff() && ad.StoreId != _workContext.CurrentCustomer.StaffStoreId)
            {
                return RedirectToAction("Edit", "Ad", new { id = id });
            }

            //get ad item identifier
            string adItemId = "";
            foreach (var formValue in form.Keys)
                if (formValue.StartsWith("btnSaveAdItem", StringComparison.OrdinalIgnoreCase))
                    adItemId = formValue.Substring("btnSaveAdItem".Length);

            var adItem = ad.AdItems.FirstOrDefault(x => x.Id == adItemId);
            if (adItem == null)
                throw new ArgumentException("No ad item found with the specified id");

            var product = await productService.GetProductByIdIncludeArch(adItem.ProductId);

            if (!decimal.TryParse(form["pvUnitPriceInclTax" + adItemId], out decimal unitPriceInclTax))
                unitPriceInclTax = adItem.UnitPriceInclTax;
            if (!decimal.TryParse(form["pvUnitPriceExclTax" + adItemId], out decimal unitPriceExclTax))
                unitPriceExclTax = adItem.UnitPriceExclTax;
            if (!int.TryParse(form["pvQuantity" + adItemId], out int quantity))
                quantity = adItem.Quantity;
            if (!decimal.TryParse(form["pvDiscountInclTax" + adItemId], out decimal discountInclTax))
                discountInclTax = adItem.DiscountAmountInclTax;
            if (!decimal.TryParse(form["pvDiscountExclTax" + adItemId], out decimal discountExclTax))
                discountExclTax = adItem.DiscountAmountExclTax;
            if (!decimal.TryParse(form["pvPriceInclTax" + adItemId], out decimal priceInclTax))
                priceInclTax = adItem.PriceInclTax;
            if (!decimal.TryParse(form["pvPriceExclTax" + adItemId], out decimal priceExclTax))
                priceExclTax = adItem.PriceExclTax;

            if (quantity > 0)
            {
                int qtyDifference = adItem.Quantity - quantity;

                adItem.UnitPriceInclTax = unitPriceInclTax;
                adItem.UnitPriceExclTax = unitPriceExclTax;
                adItem.Quantity = quantity;
                adItem.DiscountAmountInclTax = discountInclTax;
                adItem.DiscountAmountExclTax = discountExclTax;
                adItem.PriceInclTax = priceInclTax;
                adItem.PriceExclTax = priceExclTax;
                await _adService.UpdateAd(ad);
                //adjust inventory
                await productService.AdjustInventory(product, qtyDifference, adItem.AttributesXml, adItem.WarehouseId);

            }
            else
            {
                //adjust inventory
                await productService.AdjustInventory(product, adItem.Quantity, adItem.AttributesXml, adItem.WarehouseId);
                await _adService.DeleteAdItem(adItem);
            }

            ad = await _adService.GetAdById(id);
            //add a note
            await _adService.InsertAdNote(new AdNote {
                Note = "Ad item has been edited",
                DisplayToCustomer = false,
                CreatedOnUtc = DateTime.UtcNow,
                AdId = ad.Id,
            });

            await _adViewModelService.LogEditAd(ad.Id);
            var model = new AdModel();
            await _adViewModelService.PrepareAdDetailsModel(model, ad);

            //selected tab
            await SaveSelectedTabIndex(persistForTheNextRequest: false);

            return View(model);
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        [HttpPost, ActionName("Edit")]
        [FormValueRequired(FormValueRequirement.StartsWith, "btnDeleteAdItem")]
        public async Task<IActionResult> DeleteAdItem(string id, IFormCollection form,
            //[FromServices] IGiftCardService giftCardService,
            [FromServices] IProductService productService,
            [FromServices] IShipmentService shipmentService)
        {
            var ad = await _adService.GetAdById(id);
            if (ad == null)
                //No ad found with the specified id
                return RedirectToAction("List");

            //a vendor does not have access to this functionality
            if (_workContext.CurrentVendor != null && !_workContext.CurrentCustomer.IsStaff())
                return RedirectToAction("Edit", "Ad", new { id = id });

            if (_workContext.CurrentCustomer.IsStaff() && ad.StoreId != _workContext.CurrentCustomer.StaffStoreId)
            {
                return RedirectToAction("Edit", "Ad", new { id = id });
            }
            //get ad item identifier
            string adItemId = "";
            foreach (var formValue in form.Keys)
                if (formValue.StartsWith("btnDeleteAdItem", StringComparison.OrdinalIgnoreCase))
                    adItemId = formValue.Substring("btnDeleteAdItem".Length);

            var adItem = ad.AdItems.FirstOrDefault(x => x.Id == adItemId);
            if (adItem == null)
                throw new ArgumentException("No ad item found with the specified id");

            //var shipments = (await shipmentService.GetShipmentsByAd(ad.Id));
            //foreach (var shipment in shipments)
            //{
            //    if (shipment.ShipmentItems.Where(x => x.AdItemId == adItemId).Any())
            //    {
            //        ErrorNotification($"This ad item is in associated with shipment {shipment.ShipmentNumber}. Please delete it first.", false);
            //        //selected tab
            //        await SaveSelectedTabIndex(persistForTheNextRequest: false);
            //        var model = new AdModel();
            //        await _adViewModelService.PrepareAdDetailsModel(model, ad);
            //        return View(model);
            //    }
            //}

            var product = await productService.GetProductById(adItem.ProductId);
            //(await giftCardService.GetGiftCardsByPurchasedWithAdItemId(adItem.Id)).Count > 0
            if (false)
            {
                //we cannot delete an ad item with associated gift cards
                //a store owner should delete them first

                var model = new AdModel();
                await _adViewModelService.PrepareAdDetailsModel(model, ad);

                ErrorNotification("This ad item has an associated gift card record. Please delete it first.", false);

                //selected tab
                await SaveSelectedTabIndex(persistForTheNextRequest: false);

                return View(model);

            }
            else
            {
                //add a note
                await _adService.InsertAdNote(new AdNote {
                    Note = "Ad item has been deleted",
                    DisplayToCustomer = false,
                    CreatedOnUtc = DateTime.UtcNow,
                    AdId = ad.Id,
                });

                //adjust inventory
                if (product != null)
                    await productService.AdjustInventory(product, adItem.Quantity, adItem.AttributesXml, adItem.WarehouseId);

                await _adService.DeleteAdItem(adItem);
                ad = await _adService.GetAdById(id);
                await _adViewModelService.LogEditAd(ad.Id);
                var model = new AdModel();
                await _adViewModelService.PrepareAdDetailsModel(model, ad);

                //selected tab
                await SaveSelectedTabIndex(persistForTheNextRequest: false);

                return View(model);
            }
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        [HttpPost, ActionName("Edit")]
        [FormValueRequired(FormValueRequirement.StartsWith, "btnResetDownloadCount")]
        public async Task<IActionResult> ResetDownloadCount(string id, IFormCollection form)
        {
            var ad = await _adService.GetAdById(id);
            if (ad == null)
                //No ad found with the specified id
                return RedirectToAction("List");

            if (_workContext.CurrentCustomer.IsStaff() && ad.StoreId != _workContext.CurrentCustomer.StaffStoreId)
            {
                return RedirectToAction("Edit", "Ad", new { id = id });
            }

            //get ad item identifier
            string adItemId = "";
            foreach (var formValue in form.Keys)
                if (formValue.StartsWith("btnResetDownloadCount", StringComparison.OrdinalIgnoreCase))
                    adItemId = formValue.Substring("btnResetDownloadCount".Length);

            var adItem = ad.AdItems.FirstOrDefault(x => x.Id == adItemId);
            if (adItem == null)
                throw new ArgumentException("No ad item found with the specified id");

            //ensure a vendor has access only to his products 
            if (_workContext.CurrentVendor != null && !_workContext.HasAccessToAdItem(adItem) && !_workContext.CurrentCustomer.IsStaff())
                return RedirectToAction("List");


            adItem.DownloadCount = 0;
            await _adService.UpdateAd(ad);
            await _adViewModelService.LogEditAd(ad.Id);
            var model = new AdModel();
            await _adViewModelService.PrepareAdDetailsModel(model, ad);

            //selected tab
            await SaveSelectedTabIndex(persistForTheNextRequest: false);

            return View(model);
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        [HttpPost, ActionName("Edit")]
        [FormValueRequired(FormValueRequirement.StartsWith, "btnPvActivateDownload")]

        public async Task<IActionResult> ActivateDownloadItem(string id, IFormCollection form)
        {
            var ad = await _adService.GetAdById(id);
            if (ad == null)
                //No ad found with the specified id
                return RedirectToAction("List");

            if (_workContext.CurrentCustomer.IsStaff() && ad.StoreId != _workContext.CurrentCustomer.StaffStoreId)
            {
                return RedirectToAction("Edit", "Ad", new { id = id });
            }

            //get ad item identifier
            string adItemId = "";
            foreach (var formValue in form.Keys)
                if (formValue.StartsWith("btnPvActivateDownload", StringComparison.OrdinalIgnoreCase))
                    adItemId = formValue.Substring("btnPvActivateDownload".Length);

            var adItem = ad.AdItems.FirstOrDefault(x => x.Id == adItemId);
            if (adItem == null)
                throw new ArgumentException("No ad item found with the specified id");

            //ensure a vendor has access only to his products 
            if (_workContext.CurrentVendor != null && !_workContext.HasAccessToAdItem(adItem) && !_workContext.CurrentCustomer.IsStaff())
                return RedirectToAction("List");

            adItem.IsDownloadActivated = !adItem.IsDownloadActivated;
            await _adService.UpdateAd(ad);
            await _adViewModelService.LogEditAd(ad.Id);
            var model = new AdModel();
            await _adViewModelService.PrepareAdDetailsModel(model, ad);

            //selected tab
            await SaveSelectedTabIndex(persistForTheNextRequest: false);

            return View(model);
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        public async Task<IActionResult> UploadLicenseFilePopup(string id, string adItemId, [FromServices] IProductService productService)
        {
            var ad = await _adService.GetAdById(id);
            if (ad == null)
                //No ad found with the specified id
                return RedirectToAction("List");

            if (_workContext.CurrentCustomer.IsStaff() && ad.StoreId != _workContext.CurrentCustomer.StaffStoreId)
            {
                return RedirectToAction("Edit", "Ad", new { id = id });
            }

            var adItem = ad.AdItems.FirstOrDefault(x => x.Id == adItemId);
            if (adItem == null)
                throw new ArgumentException("No ad item found with the specified id");

            var product = await productService.GetProductByIdIncludeArch(adItem.ProductId);

            if (!product.IsDownload)
                throw new ArgumentException("Product is not downloadable");

            //ensure a vendor has access only to his products 
            if (_workContext.CurrentVendor != null && !_workContext.HasAccessToAdItem(adItem) && !_workContext.CurrentCustomer.IsStaff())
                return RedirectToAction("List");

            var model = new AdModel.UploadLicenseModel {
                LicenseDownloadId = !String.IsNullOrEmpty(adItem.LicenseDownloadId) ? adItem.LicenseDownloadId : "",
                AdId = ad.Id,
                AdItemId = adItem.Id
            };

            return View(model);
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        [HttpPost]
        [FormValueRequired("uploadlicense")]
        public async Task<IActionResult> UploadLicenseFilePopup(AdModel.UploadLicenseModel model)
        {
            var ad = await _adService.GetAdById(model.AdId);
            if (ad == null)
                //No ad found with the specified id
                return RedirectToAction("List");

            if (_workContext.CurrentCustomer.IsStaff() && ad.StoreId != _workContext.CurrentCustomer.StaffStoreId)
            {
                return RedirectToAction("Edit", "Ad", new { id = ad.Id });
            }

            var adItem = ad.AdItems.FirstOrDefault(x => x.Id == model.AdItemId);
            if (adItem == null)
                throw new ArgumentException("No ad item found with the specified id");

            //ensure a vendor has access only to his products 
            if (_workContext.CurrentVendor != null && !_workContext.HasAccessToAdItem(adItem) && _workContext.CurrentCustomer.IsStaff())
                return RedirectToAction("List");

            //attach license
            if (!string.IsNullOrEmpty(model.LicenseDownloadId))
                adItem.LicenseDownloadId = model.LicenseDownloadId;
            else
                adItem.LicenseDownloadId = null;
            await _adService.UpdateAd(ad);

            await _adViewModelService.LogEditAd(ad.Id);
            //success
            ViewBag.RefreshPage = true;

            return View(model);
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        [HttpPost, ActionName("UploadLicenseFilePopup")]
        [FormValueRequired("deletelicense")]
        public async Task<IActionResult> DeleteLicenseFilePopup(AdModel.UploadLicenseModel model)
        {
            var ad = await _adService.GetAdById(model.AdId);
            if (ad == null)
                //No ad found with the specified id
                return RedirectToAction("List");

            if (_workContext.CurrentCustomer.IsStaff() && ad.StoreId != _workContext.CurrentCustomer.StaffStoreId)
            {
                return RedirectToAction("Edit", "Ad", new { id = model.AdId });
            }

            var adItem = ad.AdItems.FirstOrDefault(x => x.Id == model.AdItemId);
            if (adItem == null)
                throw new ArgumentException("No ad item found with the specified id");

            //ensure a vendor has access only to his products 
            if (_workContext.CurrentVendor != null && !_workContext.HasAccessToAdItem(adItem) && !_workContext.CurrentCustomer.IsStaff())
                return RedirectToAction("List");

            //attach license
            adItem.LicenseDownloadId = null;
            await _adService.UpdateAd(ad);
            await _adViewModelService.LogEditAd(ad.Id);

            //success
            ViewBag.RefreshPage = true;

            return View(model);
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        public async Task<IActionResult> AddProductToAd(string adId)
        {
            //a vendor does not have access to this functionality
            if (_workContext.CurrentVendor != null && !_workContext.CurrentCustomer.IsStaff())
                return RedirectToAction("Edit", "Ad", new { id = adId });

            var ad = await _adService.GetAdById(adId);
            if (ad == null)
                //No ad found with the specified id
                return RedirectToAction("List");

            var model = await _adViewModelService.PrepareAddAdProductModel(ad);
            return View(model);
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        [HttpPost]
        public async Task<IActionResult> AddProductToAd(DataSourceRequest command, AdModel.AddAdProductModel model, [FromServices] IProductService productService)
        {
            //a vendor does not have access to this functionality
            if (_workContext.CurrentVendor != null && !_workContext.CurrentCustomer.IsStaff())
                return Content("");

            var categoryIds = new List<string>();
            if (!string.IsNullOrEmpty(model.SearchCategoryId))
                categoryIds.Add(model.SearchCategoryId);

            var storeId = string.Empty;
            if (_workContext.CurrentCustomer.IsStaff())
            {
                storeId = _workContext.CurrentCustomer.StaffStoreId;
            }

            var gridModel = new DataSourceResult();
            var products = (await productService.SearchProducts(categoryIds: categoryIds,
                storeId: storeId,
                manufacturerId: model.SearchManufacturerId,
                productType: model.SearchProductTypeId > 0 ? (ProductType?)model.SearchProductTypeId : null,
                keywords: model.SearchProductName,
                pageIndex: command.Page - 1,
                pageSize: command.PageSize,
                showHidden: true)).products;
            gridModel.Data = products.Select(x =>
            {
                var productModel = new AdModel.AddAdProductModel.ProductModel {
                    Id = x.Id,
                    Name = x.Name,
                    Sku = x.Sku,
                };

                return productModel;
            });
            gridModel.Total = products.TotalCount;

            return Json(gridModel);
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        public async Task<IActionResult> AddProductToAdDetails(string adId, string productId)
        {
            //a vendor does not have access to this functionality
            if (_workContext.CurrentVendor != null && !_workContext.CurrentCustomer.IsStaff())
                return RedirectToAction("Edit", "Ad", new { id = adId });

            var ad = await _adService.GetAdById(adId);
            if (ad == null)
                return RedirectToAction("List");

            if (ad == null)
                return RedirectToAction("List");

            if (_workContext.CurrentCustomer.IsStaff() && ad.StoreId != _workContext.CurrentCustomer.StaffStoreId)
            {
                return RedirectToAction("List");
            }

            var model = await _adViewModelService.PrepareAddProductToAdModel(ad, productId);
            return View(model);
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        [HttpPost]
        public async Task<IActionResult> AddProductToAdDetails(string adId, string productId, IFormCollection form)
        {
            //a vendor does not have access to this functionality
            if (_workContext.CurrentVendor != null && !_workContext.CurrentCustomer.IsStaff())
                return RedirectToAction("Edit", "Ad", new { id = adId });

            var ad = await _adService.GetAdById(adId);
            if (ad == null)
                return RedirectToAction("List");

            if (_workContext.CurrentCustomer.IsStaff() && ad.StoreId != _workContext.CurrentCustomer.StaffStoreId)
            {
                return RedirectToAction("List");
            }

            var warnings = await _adViewModelService.AddProductToAdDetails(adId, productId, form);
            if (!warnings.Any())
                //redirect to ad details page
                return RedirectToAction("Edit", "Ad", new { id = adId });

            //errors
            var model = await _adViewModelService.PrepareAddProductToAdModel(ad, productId);
            model.Warnings.AddRange(warnings);
            return View(model);
        }

        #endregion

        #endregion

        #region Addresses

        [PermissionAuthorizeAction(PermissionActionName.Preview)]
        public async Task<IActionResult> AddressEdit(string addressId, string adId, bool billingAddress)
        {
            var ad = await _adService.GetAdById(adId);
            if (ad == null)
                //No ad found with the specified id
                return RedirectToAction("List");

            //a vendor does not have access to this functionality
            if (_workContext.CurrentVendor != null && !_workContext.CurrentCustomer.IsStaff())
                return RedirectToAction("Edit", "Ad", new { id = adId });

            if (_workContext.CurrentCustomer.IsStaff() && ad.StoreId != _workContext.CurrentCustomer.StaffStoreId)
            {
                return RedirectToAction("List");
            }

            var address = new Address();
            if (billingAddress && ad.BillingAddress != null)
                if (ad.BillingAddress.Id == addressId)
                    address = ad.BillingAddress;
            if (!billingAddress && ad.ShippingAddress != null)
                if (ad.ShippingAddress.Id == addressId)
                    address = ad.ShippingAddress;

            if (address == null)
                throw new ArgumentException("No address found with the specified id", "addressId");

            var model = await _adViewModelService.PrepareAdAddressModel(ad, address);
            return View(model);
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        [HttpPost]
        public async Task<IActionResult> AddressEdit(AdAddressModel model, IFormCollection form,
            [FromServices] IAddressAttributeService addressAttributeService,
            [FromServices] IAddressAttributeParser addressAttributeParser)
        {
            var ad = await _adService.GetAdById(model.AdId);
            if (ad == null)
                //No ad found with the specified id
                return RedirectToAction("List");

            //a vendor does not have access to this functionality
            if (_workContext.CurrentVendor != null && !_workContext.CurrentCustomer.IsStaff())
                return RedirectToAction("Edit", "Ad", new { id = ad.Id });

            if (_workContext.CurrentCustomer.IsStaff() && ad.StoreId != _workContext.CurrentCustomer.StaffStoreId)
            {
                return RedirectToAction("List");
            }

            var address = new Address();
            if (model.BillingAddress && ad.BillingAddress != null)
                if (ad.BillingAddress.Id == model.Address.Id)
                    address = ad.BillingAddress;
            if (!model.BillingAddress && ad.ShippingAddress != null)
                if (ad.ShippingAddress.Id == model.Address.Id)
                    address = ad.ShippingAddress;

            if (address == null)
                throw new ArgumentException("No address found with the specified id");

            //custom address attributes
            var customAttributes = await form.ParseCustomAddressAttributes(addressAttributeParser, addressAttributeService);
            var customAttributeWarnings = await addressAttributeParser.GetAttributeWarnings(customAttributes);
            foreach (var error in customAttributeWarnings)
            {
                ModelState.AddModelError("", error);
            }

            if (ModelState.IsValid)
            {
                await _adViewModelService.UpdateAdAddress(ad, address, model, customAttributes);
                return RedirectToAction("AddressEdit", new { addressId = model.Address.Id, adId = model.AdId, BillingAddress = model.BillingAddress });
            }

            //If we got this far, something failed, redisplay form
            model = await _adViewModelService.PrepareAdAddressModel(ad, address);
            model.BillingAddress = model.BillingAddress;

            return View(model);
        }

        #endregion

        #region Ad notes

        [PermissionAuthorizeAction(PermissionActionName.List)]
        [HttpPost]
        public async Task<IActionResult> AdNotesSelect(string adId, DataSourceRequest command)
        {
            var ad = await _adService.GetAdById(adId);
            if (ad == null)
                throw new ArgumentException("No ad found with the specified id");

            //a vendor does not have access to this functionality
            if (_workContext.CurrentVendor != null && !_workContext.CurrentCustomer.IsStaff())
                return Content("");

            if (_workContext.CurrentCustomer.IsStaff() && ad.StoreId != _workContext.CurrentCustomer.StaffStoreId)
            {
                return Content("");
            }
            //ad notes
            var adNoteModels = await _adViewModelService.PrepareAdNotes(ad);
            var gridModel = new DataSourceResult {
                Data = adNoteModels,
                Total = adNoteModels.Count
            };
            return Json(gridModel);
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        public async Task<IActionResult> AdNoteAdd(string adId, string downloadId, bool displayToCustomer, string message)
        {
            var ad = await _adService.GetAdById(adId);
            if (ad == null)
                return Json(new { Result = false });

            //a vendor does not have access to this functionality
            if (_workContext.CurrentVendor != null && !_workContext.CurrentCustomer.IsStaff())
                return Json(new { Result = false });

            if (_workContext.CurrentCustomer.IsStaff() && ad.StoreId != _workContext.CurrentCustomer.StaffStoreId)
            {
                return Json(new { Result = false });
            }
            await _adViewModelService.InsertAdNote(ad, downloadId, displayToCustomer, message);

            return Json(new { Result = true });
        }

        [PermissionAuthorizeAction(PermissionActionName.Delete)]
        [HttpPost]
        public async Task<IActionResult> AdNoteDelete(string id, string adId)
        {
            var ad = await _adService.GetAdById(adId);
            if (ad == null)
                throw new ArgumentException("No ad found with the specified id");

            //a vendor does not have access to this functionality
            if (_workContext.CurrentVendor != null && !_workContext.CurrentCustomer.IsStaff())
                return Json(new { Result = false });

            if (_workContext.CurrentCustomer.IsStaff() && ad.StoreId != _workContext.CurrentCustomer.StaffStoreId)
            {
                return Json(new { Result = false });
            }

            await _adViewModelService.DeleteAdNote(ad, id);

            return new NullJsonResult();
        }
        #endregion

    }
}
