using Grand.Core;
using Grand.Domain.Vendors;
using Grand.Framework.Controllers;
using Grand.Framework.Kendoui;
using Grand.Framework.Mvc;
using Grand.Framework.Mvc.Filters;
using Grand.Framework.Security.Authorization;
using Grand.Services.Catalog;
using Grand.Services.Common;
using Grand.Services.Localization;
using Grand.Services.Security;
using Grand.Services.Seo;
using Grand.Services.Vendors;
using Grand.Web.Areas.Admin.Extensions;
using Grand.Web.Areas.Admin.Models.Vendors;
using Grand.Web.Areas.Admin.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Grand.Web.Areas.Admin.Models.Catalog;
using Grand.Web.Areas.Admin.Models.Common;
using Grand.Web.Areas.Admin.Models.Customers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Grand.Web.Areas.Admin.Controllers
{
    [PermissionAuthorize(PermissionSystemName.Vendors)]
    public partial class VendorController : BaseAdminController
    {
        #region Fields
        private readonly IVendorViewModelService _vendorViewModelService;
        private readonly ILocalizationService _localizationService;
        private readonly IVendorService _vendorService;
        private readonly ILanguageService _languageService;
        private readonly ISpecificationAttributeService _specificationAttributeService;
        private readonly IAddressAttributeParser _addressAttributeParser;
        #endregion

        #region Constructors

        public VendorController(
            IVendorViewModelService vendorViewModelService,
            ILocalizationService localizationService,
            IVendorService vendorService,
            ILanguageService languageService,
            ISpecificationAttributeService specificationAttributeService,
            IAddressAttributeParser addressAttributeParser)
        {
            _vendorViewModelService = vendorViewModelService;
            _localizationService = localizationService;
            _vendorService = vendorService;
            _languageService = languageService;
            _specificationAttributeService = specificationAttributeService;
            _addressAttributeParser = addressAttributeParser;
        }

        #endregion

        #region Methods

        //list
        public IActionResult Index() => RedirectToAction("List");

        public IActionResult List()
        {
            var model = new VendorListModel();
            return View(model);
        }

        [PermissionAuthorizeAction(PermissionActionName.List)]
        [HttpPost]
        public async Task<IActionResult> List(DataSourceRequest command, VendorListModel model)
        {
            var vendors = await _vendorService.GetAllVendors(model.SearchName, command.Page - 1, command.PageSize, true);
            var gridModel = new DataSourceResult
            {
                Data = vendors.Select(x =>
                {
                    var vendorModel = x.ToModel();
                    return vendorModel;
                }),
                Total = vendors.TotalCount,
            };
            return Json(gridModel);
        }

        //create
        [PermissionAuthorizeAction(PermissionActionName.Create)]
        public async Task<IActionResult> Create()
        {
            var model = await _vendorViewModelService.PrepareVendorModel();
            //locales
            await AddLocales(_languageService, model.Locales);
            return View(model);
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        [FormValueRequired("save", "save-continue")]
        public async Task<IActionResult> Create(VendorModel model, bool continueEditing)
        {
            if (ModelState.IsValid)
            {
                var vendor = await _vendorViewModelService.InsertVendorModel(model);
                SuccessNotification(_localizationService.GetResource("Admin.Vendors.Added"));
                return continueEditing ? RedirectToAction("Edit", new { id = vendor.Id }) : RedirectToAction("List");
            }
            //prepare address model
            await _vendorViewModelService.PrepareVendorAddressModel(model, null);
            //discounts
            await _vendorViewModelService.PrepareDiscountModel(model, null, true);
            //stores
            await _vendorViewModelService.PrepareStore(model);

            //If we got this far, something failed, redisplay form
            return View(model);
        }


        //edit
        [PermissionAuthorizeAction(PermissionActionName.Preview)]
        public async Task<IActionResult> Edit(string id)
        {
            var vendor = await _vendorService.GetVendorById(id);
            if (vendor == null || vendor.Deleted)
                //No vendor found with the specified id
                return RedirectToAction("List");

            var model = vendor.ToModel();
            //locales
            await AddLocales(_languageService, model.Locales, (locale, languageId) =>
            {
                locale.Name = vendor.GetLocalized(x => x.Name, languageId, false, false);
                locale.Description = vendor.GetLocalized(x => x.Description, languageId, false, false);
                locale.MetaKeywords = vendor.GetLocalized(x => x.MetaKeywords, languageId, false, false);
                locale.MetaDescription = vendor.GetLocalized(x => x.MetaDescription, languageId, false, false);
                locale.MetaTitle = vendor.GetLocalized(x => x.MetaTitle, languageId, false, false);
                locale.SeName = vendor.GetSeName(languageId, false, false);
            });
            //discounts
            await _vendorViewModelService.PrepareDiscountModel(model, vendor, false);
            //associated customer emails
            model.AssociatedCustomers = await _vendorViewModelService.AssociatedCustomers(vendor.Id);
            //prepare address model
            await _vendorViewModelService.PrepareVendorAddressModel(model, vendor);
            //stores
            await _vendorViewModelService.PrepareStore(model);

            //specification attributes
            var availableSpecificationAttributes = new List<SelectListItem>();
            foreach (var sa in await _specificationAttributeService.GetSpecificationAttributes())
            {
                availableSpecificationAttributes.Add(new SelectListItem {
                    Text = sa.Name,
                    Value = sa.Id.ToString()
                });
            }
            model.AddSpecificationAttributeModel.AvailableAttributes = availableSpecificationAttributes;

            //default specs values
            model.AddSpecificationAttributeModel.ShowOnProductPage = true;

            return View(model);
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public async Task<IActionResult> Edit(VendorModel model, bool continueEditing)
        {
            var vendor = await _vendorService.GetVendorById(model.Id);
            if (vendor == null || vendor.Deleted)
                //No vendor found with the specified id
                return RedirectToAction("List");
            
            if (ModelState.IsValid)
            {
                vendor = await _vendorViewModelService.UpdateVendorModel(vendor, model);

                SuccessNotification(_localizationService.GetResource("Admin.Vendors.Updated"));
                if (continueEditing)
                {
                    //selected tab
                    await SaveSelectedTabIndex();

                    return RedirectToAction("Edit", new { id = vendor.Id });
                }
                return RedirectToAction("List");
            }

            //If we got this far, something failed, redisplay form
            //discounts
            await _vendorViewModelService.PrepareDiscountModel(model, vendor, true);
            //prepare address model
            await _vendorViewModelService.PrepareVendorAddressModel(model, vendor);
            //associated customer emails
            model.AssociatedCustomers = await _vendorViewModelService.AssociatedCustomers(vendor.Id);
            //stores
            await _vendorViewModelService.PrepareStore(model);

            return View(model);
        }

        //delete
        [PermissionAuthorizeAction(PermissionActionName.Delete)]
        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            var vendor = await _vendorService.GetVendorById(id);
            if (vendor == null)
                //No vendor found with the specified id
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                await _vendorViewModelService.DeleteVendor(vendor);
                SuccessNotification(_localizationService.GetResource("Admin.Vendors.Deleted"));
                return RedirectToAction("List");
            }
            ErrorNotification(ModelState);
            return RedirectToAction("Edit", new { id = vendor.Id });
        }
        
        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        public async Task<IActionResult> ProductSpecificationAttributeAdd(AddVendorSpecificationAttributeModel model)
        {
            if (ModelState.IsValid)
            {
                var vendor = await _vendorService.GetVendorById(model.VendorId);
                if (vendor == null)
                    return Content("Product not exists");

                await _vendorViewModelService.InsertVendorSpecificationAttributeModel(model, vendor);

                return Json(new { Result = true });
            }
            return Json(new { Result = false });
        }
        
        [PermissionAuthorizeAction(PermissionActionName.Preview)]
        [HttpPost]
        public async Task<IActionResult> VendorSpecAttrList(DataSourceRequest command, string vendorId)
        {
            var vendor = await _vendorService.GetVendorById(vendorId);
            if (vendor == null)
                            //No vendor found with the specified id
                            return RedirectToAction("List");
          
            var vendorSpecsModel = await _vendorViewModelService.PrepareProductSpecificationAttributeModel(vendor);
            var gridModel = new DataSourceResult {
                Data = vendorSpecsModel,
                Total = vendorSpecsModel.Count
            };
            return Json(gridModel);
        }
        
        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        [HttpPost]
        public async Task<IActionResult> VendorSpecAttrUpdate(VendorSpecificationAttributeModel model)
        {
            if (ModelState.IsValid)
            {
                var vendor = await _vendorService.GetVendorById(model.ProductId);
                if (vendor == null)
                    return Content("Product not exists");

                var psa = vendor.VendorSpecificationAttributes.Where(x => x.SpecificationAttributeId == model.ProductSpecificationId).Where(x => x.Id == model.Id).FirstOrDefault();
                if (psa == null)
                    return Content("No product specification attribute found with the specified id");

                await _vendorViewModelService.UpdateVendorSpecificationAttributeModel(vendor, psa, model);
                return new NullJsonResult();
            }
            return ErrorForKendoGridJson(ModelState);
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        [HttpPost]
        public async Task<IActionResult> VendorSpecAttrDelete(VendorSpecificationAttributeModel model)
        {
            if (ModelState.IsValid)
            {
                var vendor = await _vendorService.GetVendorById(model.ProductId);
                if (vendor == null)
                    return Content("Product not exists");

                var psa = vendor.VendorSpecificationAttributes.Where(x => x.Id == model.Id && x.SpecificationAttributeId == model.ProductSpecificationId).FirstOrDefault();
                if (psa == null)
                    throw new ArgumentException("No specification attribute found with the specified id");

                await _vendorViewModelService.DeleteProductSpecificationAttribute(vendor, psa);
                return new NullJsonResult();
            }
            return ErrorForKendoGridJson(ModelState);
        }

        #endregion

        #region Vendor notes

        [PermissionAuthorizeAction(PermissionActionName.Preview)]
        [HttpPost]
        public async Task<IActionResult> VendorNotesSelect(string vendorId, DataSourceRequest command)
        {
            var vendor = await _vendorService.GetVendorById(vendorId);
            if (vendor == null)
                throw new ArgumentException("No vendor found with the specified id");

            var vendorNoteModels = _vendorViewModelService.PrepareVendorNote(vendor);
            var gridModel = new DataSourceResult
            {
                Data = vendorNoteModels,
                Total = vendorNoteModels.Count
            };

            return Json(gridModel);
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        public async Task<IActionResult> VendorNoteAdd(string vendorId, string message)
        {
            if (ModelState.IsValid)
            {
                var result = await _vendorViewModelService.InsertVendorNote(vendorId, message);
                return Json(new { Result = result });
            }
            return ErrorForKendoGridJson(ModelState);
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        [HttpPost]
        public async Task<IActionResult> VendorNoteDelete(string id, string vendorId)
        {
            if (ModelState.IsValid)
            {
                await _vendorViewModelService.DeleteVendorNote(id, vendorId);
                return new NullJsonResult();
            }
            return ErrorForKendoGridJson(ModelState);
        }

        #endregion

        #region Reviews

        [PermissionAuthorizeAction(PermissionActionName.Preview)]
        [HttpPost]
        public async Task<IActionResult> Reviews(DataSourceRequest command, string vendorId, [FromServices] IWorkContext workContext)
        {
            var vendor = await _vendorService.GetVendorById(vendorId);
            if (vendor == null)
                throw new ArgumentException("No vendor found with the specified id");

            //a vendor should have access only to his own profile
            if (workContext.CurrentVendor != null && vendor.Id != workContext.CurrentVendor.Id)
                return Content("This is not your vendor");

            var vendorReviews = await _vendorService.GetAllVendorReviews("", null,
                null, null, "", vendorId, command.Page - 1, command.PageSize);
            var items = new List<VendorReviewModel>();
            foreach (var item in vendorReviews)
            {
                var m = new VendorReviewModel();
                await _vendorViewModelService.PrepareVendorReviewModel(m, item, false, true);
                items.Add(m);
            }
            var gridModel = new DataSourceResult
            {
                Data = items,
                Total = vendorReviews.TotalCount,
            };

            return Json(gridModel);
        }

        #endregion

        [PermissionAuthorizeAction(PermissionActionName.Preview)]
        public async Task<IActionResult> AddressEdit(string addressId, string vendorID)
        {
            var vendor = await _vendorService.GetVendorById(vendorID);
            if (vendor == null)
                //No customer found with the specified id
                return RedirectToAction("List");

            var vendorModel =  vendor.ToModel();
                
            await _vendorViewModelService.PrepareVendorAddressModel(vendorModel, vendor);
            var model = vendorModel.Addresses.Where(x => x.Id == addressId).FirstOrDefault();
            
            return View(model);
            
            //if (ModelState.IsValid)
            //{ 
                //address = await _vendorViewModelService.UpdateAddress(vendor, model, address);
                //SuccessNotification(_localizationService.GetResource("Admin.Customers.Customers.Addresses.Updated"));
                //return RedirectToAction("AddressEdit", new { addressId = model.Id, vendorID = vendorID});
           // }
            //If we got this far, something failed, redisplay form
            //await _vendorViewModelService.PrepareVendorAddressModel(vendorModel, vendor);

            //return View(model);   
        }
        
        [HttpPost]
        public async Task<IActionResult> AddressEdit(VendorViewAddresModel model, IFormCollection form)
        {
            var vendor = await _vendorService.GetVendorById(model.VendorId);
            
            if (vendor == null)
                //No customer found with the specified id 
                return RedirectToAction("List");

            var address = vendor.Addresses.Where(x => x.Id == model.Address.Id).FirstOrDefault();
            if (address == null)
                //No address found with the specified id
                return RedirectToAction("Edit", new { id = vendor.Id });
           
            if (ModelState.IsValid)
            {
                address = model.Address.ToEntity(address);
                await _vendorService.UpdateVendor(vendor);
                SuccessNotification(_localizationService.GetResource("Admin.Customers.Customers.Addresses.Updated"));
                return RedirectToAction("AddressEdit", new { addressId = model.Address.Id, vendorID = model.VendorId });
            }
            //If we got this far, something failed, redisplay form
            await  _vendorViewModelService.PrepareVendorAddressModel(vendor.ToModel(), vendor);

            return View(model.Address);
        }
                
        [PermissionAuthorizeAction(PermissionActionName.Preview)]
        [HttpPost]
        public async Task<IActionResult> AddressesSelect(string customerId, DataSourceRequest command)
        {
            var vendor = await _vendorService.GetVendorById(customerId);
            if (vendor == null)
                throw new ArgumentException("No customer found with the specified id", "customerId");

            var vendorModel =  vendor.ToModel();
            await _vendorViewModelService.PrepareVendorAddressModel(vendorModel, vendor);
            
            var gridModel = new DataSourceResult {
                Data = vendorModel.Addresses,
                Total = vendorModel.Addresses.Count()
            };

            return Json(gridModel);
        }

        
         [PermissionAuthorizeAction(PermissionActionName.Edit)]
        public async Task<IActionResult> AddressCreate(string vendorId, AddressModel model)
        {
            if (ModelState.IsValid)
            {
                var vendor = await _vendorService.GetVendorById(vendorId);
                if (vendor == null)
                    return Content("Vendor not exists");

                await _vendorViewModelService.AddAddressToVendor(vendor,model);
              
                vendor = await _vendorService.GetVendorById(vendorId);
                var vemdorModel = vendor.ToModel();
                
                await _vendorViewModelService.PrepareVendorAddressModel (vemdorModel, vendor);
                var newAddresModell = vemdorModel.Addresses.LastOrDefault();
                
                return View(newAddresModell);
            }
            return View(model);
        }
        
        [HttpPost]
        public async Task<IActionResult> AddressCreate(VendorViewAddresModel model, IFormCollection form)
         {
            var vendor = await _vendorService.GetVendorById(model.VendorId);
            
            if (vendor == null)
                //No customer found with the specified id 
                return RedirectToAction("List");

            var address = vendor.Addresses.Where(x => x.Id == model.Address.Id).FirstOrDefault();
            if (address == null)
                //No address found with the specified id
                return RedirectToAction("Edit", new { id = vendor.Id });
           
            if (ModelState.IsValid)
            {
                address = model.Address.ToEntity(address);
                await _vendorService.UpdateVendor(vendor);
                SuccessNotification(_localizationService.GetResource("Admin.Customers.Customers.Addresses.Updated"));
                return RedirectToAction("AddressEdit", new { addressId = model.Address.Id, vendorID = model.VendorId });
            }
            //If we got this far, something failed, redisplay form
            await  _vendorViewModelService.PrepareVendorAddressModel(vendor.ToModel(), vendor);

            return View(model.Address);
        }
        
        [HttpPost]  
        [RequestFormLimits(ValueCountLimit = Int32.MaxValue)]
        public async Task<IActionResult> RemoveAdddresFromVendor(string id, string vendorId)
        {
            if (ModelState.IsValid)
            {
                var vendor = await _vendorService.GetVendorById(vendorId);
                if (vendor == null)
                    return Content("Product not existxs");

                var address = vendor.Addresses.FirstOrDefault(a=>a.Id == id);
                
                if (address == null)
                    return Content("Address not existxs");
                
                await _vendorViewModelService.RemoveAddressFromVendor(vendor, address);
                return new NullJsonResult();
            }
            return ErrorForKendoGridJson(ModelState);
        }
        
        
    }
}
