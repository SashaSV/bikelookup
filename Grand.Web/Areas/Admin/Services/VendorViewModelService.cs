using Grand.Domain;
using Grand.Domain.Customers;
using Grand.Domain.Directory;
using Grand.Domain.Discounts;
using Grand.Domain.Seo;
using Grand.Domain.Vendors;
using Grand.Services.Customers;
using Grand.Services.Directory;
using Grand.Services.Discounts;
using Grand.Services.Helpers;
using Grand.Services.Localization;
using Grand.Services.Media;
using Grand.Services.Notifications.Vendors;
using Grand.Services.Seo;
using Grand.Services.Stores;
using Grand.Services.Vendors;
using Grand.Web.Areas.Admin.Extensions;
using Grand.Web.Areas.Admin.Interfaces;
using Grand.Web.Areas.Admin.Models.Customers;
using Grand.Web.Areas.Admin.Models.Vendors;
using MediatR;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grand.Core;
using Grand.Domain.Catalog;
using Grand.Domain.Common;
using Grand.Services.Catalog;
using Grand.Web.Areas.Admin.Models.Common;
using NUglify.Helpers;

namespace Grand.Web.Areas.Admin.Services
{
    public partial class VendorViewModelService : IVendorViewModelService
    {
        private readonly IDiscountService _discountService;
        private readonly IVendorService _vendorService;
        private readonly ICustomerService _customerService;
        private readonly ILocalizationService _localizationService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly ICountryService _countryService;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly IStoreService _storeService;
        private readonly IUrlRecordService _urlRecordService;
        private readonly IPictureService _pictureService;
        private readonly IMediator _mediator;
        private readonly ILanguageService _languageService;
        private readonly SeoSettings _seoSettings;
        private readonly VendorSettings _vendorSettings;
        private readonly ISpecificationAttributeService _specificationAttributeService;
        private readonly IWorkContext _workContext;
        public VendorViewModelService(IDiscountService discountService, IVendorService vendorService, ICustomerService customerService, ILocalizationService localizationService,
            IDateTimeHelper dateTimeHelper, ICountryService countryService, IStateProvinceService stateProvinceService, IStoreService storeService, IUrlRecordService urlRecordService,
            IPictureService pictureService, IMediator mediator, VendorSettings vendorSettings, ILanguageService languageService, 
            SeoSettings seoSettings, ISpecificationAttributeService specificationAttributeService, IWorkContext workContext)
        {
            _discountService = discountService;
            _vendorService = vendorService;
            _customerService = customerService;
            _localizationService = localizationService;
            _dateTimeHelper = dateTimeHelper;
            _countryService = countryService;
            _stateProvinceService = stateProvinceService;
            _storeService = storeService;
            _urlRecordService = urlRecordService;
            _pictureService = pictureService;
            _mediator = mediator;
            _languageService = languageService;
            _vendorSettings = vendorSettings;
            _seoSettings = seoSettings;
            _specificationAttributeService = specificationAttributeService;
            _workContext = workContext ;
        }

        public virtual async Task PrepareDiscountModel(VendorModel model, Vendor vendor, bool excludeProperties)
        {
            if (model == null)
                throw new ArgumentNullException("model");
            
            model.AvailableDiscounts = (await _discountService
                .GetAllDiscounts(DiscountType.AssignedToVendors, showHidden: true))
                .Select(d => d.ToModel(_dateTimeHelper))
                .ToList();

            if (!excludeProperties && vendor != null)
            {
                model.SelectedDiscountIds = vendor.AppliedDiscounts.ToArray();
            }
        }

        public virtual async Task PrepareVendorReviewModel(VendorReviewModel model,
            VendorReview vendorReview, bool excludeProperties, bool formatReviewText)
        {
            if (model == null)
                throw new ArgumentNullException("model");

            if (vendorReview == null)
                throw new ArgumentNullException("vendorReview");
            var vendor = await _vendorService.GetVendorById(vendorReview.VendorId);
            var customer = await _customerService.GetCustomerById(vendorReview.CustomerId);

            model.Id = vendorReview.Id;
            model.VendorId = vendorReview.VendorId;
            model.VendorName = vendor.Name;
            model.CustomerId = vendorReview.CustomerId;
            model.CustomerInfo = customer != null ? customer.IsRegistered() ? customer.Email : _localizationService.GetResource("Admin.Customers.Guest") : "";
            model.Rating = vendorReview.Rating;
            model.CreatedOn = _dateTimeHelper.ConvertToUserTime(vendorReview.CreatedOnUtc, DateTimeKind.Utc);
            if (!excludeProperties)
            {
                model.Title = vendorReview.Title;
                if (formatReviewText)
                    model.ReviewText = Core.Html.HtmlHelper.FormatText(vendorReview.ReviewText);
                else
                    model.ReviewText = vendorReview.ReviewText;
                model.IsApproved = vendorReview.IsApproved;
            }
        }

        public virtual async Task PrepareVendorAddressModel(VendorModel model, Vendor vendor)
        {

            if (model.Addresses == null)
                model.Addresses = new List<Models.Common.AddressModel>();

            foreach (var Aadress in model.Addresses)
            {
                Aadress.VendorId = vendor.Id;
                Aadress.FirstNameEnabled = false;
                Aadress.FirstNameRequired = false;
                Aadress.LastNameEnabled = false;
                Aadress.LastNameRequired = false;
                Aadress.EmailEnabled = false;
                Aadress.EmailRequired = false;
                Aadress.CompanyEnabled = _vendorSettings.AddressSettings.CompanyEnabled;
                Aadress.CountryEnabled = _vendorSettings.AddressSettings.CountryEnabled;
                Aadress.StateProvinceEnabled = _vendorSettings.AddressSettings.StateProvinceEnabled;
                Aadress.CityEnabled = _vendorSettings.AddressSettings.CityEnabled;
                Aadress.CityRequired = _vendorSettings.AddressSettings.CityRequired;
                Aadress.StreetAddressEnabled = _vendorSettings.AddressSettings.StreetAddressEnabled;
                Aadress.StreetAddressRequired = _vendorSettings.AddressSettings.StreetAddressRequired;
                Aadress.StreetAddress2Enabled = _vendorSettings.AddressSettings.StreetAddress2Enabled;
                Aadress.ZipPostalCodeEnabled = _vendorSettings.AddressSettings.ZipPostalCodeEnabled;
                Aadress.ZipPostalCodeRequired = _vendorSettings.AddressSettings.ZipPostalCodeRequired;
                Aadress.PhoneEnabled = _vendorSettings.AddressSettings.PhoneEnabled;
                Aadress.PhoneRequired = _vendorSettings.AddressSettings.PhoneRequired;
                Aadress.FaxEnabled = _vendorSettings.AddressSettings.FaxEnabled;
                Aadress.FaxRequired = _vendorSettings.AddressSettings.FaxRequired;

                //address
                Aadress.AvailableCountries.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Address.SelectCountry"), Value = "" });
                foreach (var c in await _countryService.GetAllCountries(showHidden: true))
                    Aadress.AvailableCountries.Add(new SelectListItem { Text = c.Name, Value = c.Id.ToString(), Selected = (vendor != null && c.Id == Aadress?.CountryId) });

                var states = !String.IsNullOrEmpty(Aadress.CountryId) ? await _stateProvinceService.GetStateProvincesByCountryId(Aadress.CountryId, showHidden: true) : new List<StateProvince>();
                if (states.Count > 0)
                {
                    foreach (var s in states)
                        Aadress.AvailableStates.Add(new SelectListItem { Text = s.Name, Value = s.Id.ToString(), Selected = (vendor != null && s.Id == Aadress?.StateProvinceId) });
                }
                else
                    Aadress.AvailableStates.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Address.OtherNonUS"), Value = "" });
             }
        }
        
        public virtual async Task PrepareStore(VendorModel model)
        {
            model.AvailableStores.Add(new SelectListItem
            {
                Text = "[None]",
                Value = ""
            });

            foreach (var s in await _storeService.GetAllStores())
            {
                model.AvailableStores.Add(new SelectListItem
                {
                    Text = s.Shortcut,
                    Value = s.Id.ToString()
                });
            }
        }
        public virtual async Task<VendorModel> PrepareVendorModel()
        {
            var model = new VendorModel();
            //discounts
            await PrepareDiscountModel(model, null, true);
            //default values
            model.PageSize = 6;
            model.Active = true;
            model.AllowCustomersToSelectPageSize = true;
            model.PageSizeOptions = _vendorSettings.DefaultVendorPageSizeOptions;

            //default value
            model.Active = true;

            //stores
            await PrepareStore(model);

            //prepare address model
            await PrepareVendorAddressModel(model, null);
            
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
     
            return model;
        }
        public virtual async Task<IList<VendorModel.AssociatedCustomerInfo>> AssociatedCustomers(string vendorId)
        {
            return (await _customerService
                .GetAllCustomers(vendorId: vendorId))
                .Select(c => new VendorModel.AssociatedCustomerInfo()
                {
                    Id = c.Id,
                    Email = c.Email
                })
                .ToList();
        }
        public virtual async Task<Vendor> InsertVendorModel(VendorModel model)
        {
            var vendor = model.ToEntity();
            vendor.Addresses = model.Addresses.Select(a => a.ToEntity()).ToList();
            vendor.Addresses.ForEach(a => a.CreatedOnUtc = DateTime.UtcNow);

            await _vendorService.InsertVendor(vendor);

            //discounts
            var allDiscounts = await _discountService.GetAllDiscounts(DiscountType.AssignedToVendors, showHidden: true);
            foreach (var discount in allDiscounts)
            {
                if (model.SelectedDiscountIds != null && model.SelectedDiscountIds.Contains(discount.Id))
                    vendor.AppliedDiscounts.Add(discount.Id);
            }

            //search engine name
            model.SeName = await vendor.ValidateSeName(model.SeName, vendor.Name, true, _seoSettings, _urlRecordService, _languageService);
            vendor.Locales = await model.Locales.ToLocalizedProperty(vendor, x => x.Name, _seoSettings, _urlRecordService, _languageService);
            vendor.SeName = model.SeName;
            await _vendorService.UpdateVendor(vendor);

            //update picture seo file name
            await _pictureService.UpdatePictureSeoNames(vendor.PictureId, vendor.Name);
            await _urlRecordService.SaveSlug(vendor, model.SeName, "");

            return vendor;
        }

        public async Task<Vendor> UpdateVendorAddressModel(Vendor vendor, AddressModel model)
        {
            return vendor;
        }

        public virtual async Task<Vendor> UpdateVendorModel(Vendor vendor, VendorModel model)
        {
            string prevPictureId = vendor.PictureId;
            vendor = model.ToEntity(vendor);
            vendor.Locales = await model.Locales.ToLocalizedProperty(vendor, x => x.Name, _seoSettings, _urlRecordService, _languageService);
            model.SeName = await vendor.ValidateSeName(model.SeName, vendor.Name, true, _seoSettings, _urlRecordService, _languageService);
            vendor.Addresses = model.Addresses.Select(a=>a.ToEntity()).ToList();

            //discounts
            var allDiscounts = await _discountService.GetAllDiscounts(DiscountType.AssignedToVendors, showHidden: true);
            foreach (var discount in allDiscounts)
            {
                if (model.SelectedDiscountIds != null && model.SelectedDiscountIds.Contains(discount.Id))
                {
                    //new discount
                    if (vendor.AppliedDiscounts.Count(d => d == discount.Id) == 0)
                        vendor.AppliedDiscounts.Add(discount.Id);
                }
                else
                {
                    //remove discount
                    if (vendor.AppliedDiscounts.Count(d => d == discount.Id) > 0)
                        vendor.AppliedDiscounts.Remove(discount.Id);
                }
            }

            vendor.SeName = model.SeName;

            await _vendorService.UpdateVendor(vendor);
            //search engine name                
            await _urlRecordService.SaveSlug(vendor, model.SeName, "");

            //delete an old picture (if deleted or updated)
            if (!String.IsNullOrEmpty(prevPictureId) && prevPictureId != vendor.PictureId)
            {
                var prevPicture = await _pictureService.GetPictureById(prevPictureId);
                if (prevPicture != null)
                    await _pictureService.DeletePicture(prevPicture);
            }
            //update picture seo file name
            await _pictureService.UpdatePictureSeoNames(vendor.PictureId, vendor.Name);
            return vendor;
        }
        public virtual async Task DeleteVendor(Vendor vendor)
        {
            //clear associated customer references
            var associatedCustomers = await _customerService.GetAllCustomers(vendorId: vendor.Id);
            foreach (var customer in associatedCustomers)
            {
                customer.VendorId = "";
                await _customerService.UpdateCustomer(customer);
            }
            await _vendorService.DeleteVendor(vendor);
        }
        public virtual IList<VendorModel.VendorNote> PrepareVendorNote(Vendor vendor)
        {
            var vendorNoteModels = new List<VendorModel.VendorNote>();
            foreach (var vendorNote in vendor.VendorNotes
                .OrderByDescending(vn => vn.CreatedOnUtc))
            {
                vendorNoteModels.Add(new VendorModel.VendorNote
                {
                    Id = vendorNote.Id,
                    VendorId = vendor.Id,
                    Note = vendorNote.FormatVendorNoteText(),
                    CreatedOn = _dateTimeHelper.ConvertToUserTime(vendorNote.CreatedOnUtc, DateTimeKind.Utc)
                });
            }
            return vendorNoteModels;
        }
        public virtual async Task<bool> InsertVendorNote(string vendorId, string message)
        {
            var vendor = await _vendorService.GetVendorById(vendorId);
            if (vendor == null)
                return false;

            var vendorNote = new VendorNote
            {
                Note = message,
                VendorId = vendorId,
                CreatedOnUtc = DateTime.UtcNow,
            };
            vendor.VendorNotes.Add(vendorNote);
            await _vendorService.UpdateVendor(vendor);

            return true;
        }
        public virtual async Task DeleteVendorNote(string id, string vendorId)
        {
            var vendor = await _vendorService.GetVendorById(vendorId);
            if (vendor == null)
                throw new ArgumentException("No vendor found with the specified id");

            var vendorNote = vendor.VendorNotes.FirstOrDefault(vn => vn.Id == id);
            if (vendorNote == null)
                throw new ArgumentException("No vendor note found with the specified id");
            vendorNote.VendorId = vendor.Id;
            await _vendorService.DeleteVendorNote(vendorNote);
        }

        public virtual async Task<(IEnumerable<VendorReviewModel> vendorReviewModels, int totalCount)> PrepareVendorReviewModel(VendorReviewListModel model, int pageIndex, int pageSize)
        {
            DateTime? createdOnFromValue = (model.CreatedOnFrom == null) ? null
                            : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.CreatedOnFrom.Value, _dateTimeHelper.CurrentTimeZone);

            DateTime? createdToFromValue = (model.CreatedOnTo == null) ? null
                            : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.CreatedOnTo.Value, _dateTimeHelper.CurrentTimeZone).AddDays(1);

            IPagedList<VendorReview> vendorReviews = await _vendorService.GetAllVendorReviews("", null,
                     createdOnFromValue, createdToFromValue, model.SearchText, model.SearchVendorId, pageIndex - 1, pageSize);
            var items = new List<VendorReviewModel>();
            foreach (var x in vendorReviews)
            {
                var m = new VendorReviewModel();
                await PrepareVendorReviewModel(m, x, false, true);
                items.Add(m);
            }
            return (items, vendorReviews.TotalCount);
        }
        public virtual async Task<VendorReview> UpdateVendorReviewModel(VendorReview vendorReview, VendorReviewModel model)
        {
            vendorReview.Title = model.Title;
            vendorReview.ReviewText = model.ReviewText;
            vendorReview.IsApproved = model.IsApproved;

            await _vendorService.UpdateVendorReview(vendorReview);

            var vendor = await _vendorService.GetVendorById(vendorReview.VendorId);
            //update vendor totals
            await _vendorService.UpdateVendorReviewTotals(vendor);
            return vendorReview;
        }
        public virtual async Task DeleteVendorReview(VendorReview vendorReview)
        {
            await _vendorService.DeleteVendorReview(vendorReview);
            var vendor = await _vendorService.GetVendorById(vendorReview.VendorId);
            //update vendor totals
            await _vendorService.UpdateVendorReviewTotals(vendor);
        }
        public virtual async Task ApproveVendorReviews(IList<string> selectedIds)
        {
            foreach (var id in selectedIds)
            {
                string idReview = id.Split(':').First().ToString();
                string idVendor = id.Split(':').Last().ToString();
                var vendor = await _vendorService.GetVendorById(idVendor);
                var vendorReview = await _vendorService.GetVendorReviewById(idReview);
                if (vendorReview != null)
                {
                    var previousIsApproved = vendorReview.IsApproved;
                    vendorReview.IsApproved = true;
                    await _vendorService.UpdateVendorReview(vendorReview);
                    await _vendorService.UpdateVendorReviewTotals(vendor);

                    //raise event (only if it wasn't approved before)
                    if (!previousIsApproved)
                        await _mediator.Publish(new VendorReviewApprovedEvent(vendorReview));
                }
            }
        }
        public virtual async Task DisapproveVendorReviews(IList<string> selectedIds)
        {
            foreach (var id in selectedIds)
            {
                string idReview = id.Split(':').First().ToString();
                string idVendor = id.Split(':').Last().ToString();

                var vendor = await _vendorService.GetVendorById(idVendor);
                var vendorReview = await _vendorService.GetVendorReviewById(idReview);
                if (vendorReview != null)
                {
                    vendorReview.IsApproved = false;
                    await _vendorService.UpdateVendorReview(vendorReview);
                    await _vendorService.UpdateVendorReviewTotals(vendor);
                }
            }
        }

        public async Task<IList<VendorSpecificationAttributeModel>> PrepareProductSpecificationAttributeModel(Vendor product)
        {
             var items = new List<VendorSpecificationAttributeModel>();
            foreach (var x in product.VendorSpecificationAttributes.OrderBy(x => x.DisplayOrder))
            {
                var specificationAttribute = await _specificationAttributeService.GetSpecificationAttributeById(x.SpecificationAttributeId);
                var psaModel = new VendorSpecificationAttributeModel {
                    Id = x.Id,
                    AttributeTypeId = (int)x.AttributeType,
                    ProductSpecificationId = specificationAttribute.Id,
                    AttributeId = x.SpecificationAttributeId,
                    ProductId = product.Id,
                    AttributeTypeName = x.AttributeType.GetLocalizedEnum(_localizationService, _workContext),
                    AttributeName = specificationAttribute.Name,
                    AllowFiltering = x.AllowFiltering,
                    ShowOnProductPage = x.ShowOnProductPage,
                    DisplayOrder = x.DisplayOrder,
                    DetailsUrl = x.DetailsUrl
                    
                };
                switch (x.AttributeType)
                {
                    case SpecificationAttributeType.Option:
                        psaModel.ValueRaw = System.Net.WebUtility.HtmlEncode(specificationAttribute.SpecificationAttributeOptions.Where(y => y.Id == x.SpecificationAttributeOptionId).FirstOrDefault()?.Name);
                        psaModel.SpecificationAttributeOptionId = x.SpecificationAttributeOptionId;
                        break;
                    case SpecificationAttributeType.CustomText:
                        psaModel.ValueRaw = System.Net.WebUtility.HtmlEncode(x.CustomValue);
                        break;
                    case SpecificationAttributeType.CustomHtmlText:
                        //do not encode?
                        psaModel.ValueRaw = System.Net.WebUtility.HtmlEncode(x.CustomValue);
                        break;
                    case SpecificationAttributeType.Hyperlink:
                        psaModel.ValueRaw = x.CustomValue;
                        break;
                    default:
                        break;
                }
                items.Add(psaModel);
            }
            return items;
        }

        public async Task InsertVendorSpecificationAttributeModel(AddVendorSpecificationAttributeModel model, Vendor vendor)
        {
            //we allow filtering only for "Option" attribute type
            if (model.AttributeTypeId != (int)SpecificationAttributeType.Option)
            {
                model.AllowFiltering = false;
                model.SpecificationAttributeOptionId = null;
            }

            var psa = new VendorSpecificationAttribute {
                AttributeTypeId = model.AttributeTypeId,
                SpecificationAttributeOptionId = model.SpecificationAttributeOptionId,
                SpecificationAttributeId = model.SpecificationAttributeId,
                VendorId = vendor.Id,
                CustomValue = model.CustomValue,
                AllowFiltering = model.AllowFiltering,
                ShowOnProductPage = model.ShowOnProductPage,
                DisplayOrder = model.DisplayOrder,
                DetailsUrl = model.DetailsUrl
            };

            await _specificationAttributeService.InsertVendorSpecificationAttribute(psa);
            vendor.VendorSpecificationAttributes.Add(psa);
        }

        public async Task UpdateVendorSpecificationAttributeModel(Vendor vendor, VendorSpecificationAttribute psa,
            VendorSpecificationAttributeModel model)
        {
            if (model.AttributeTypeId == (int)SpecificationAttributeType.Option)
            {
                psa.AllowFiltering = model.AllowFiltering;
                psa.SpecificationAttributeOptionId = model.SpecificationAttributeOptionId;
            }
            else
            {
                psa.CustomValue = model.ValueRaw;
            }
            psa.ShowOnProductPage = model.ShowOnProductPage;
            psa.DisplayOrder = model.DisplayOrder;
            psa.DetailsUrl = model.DetailsUrl;
            psa.VendorId = model.ProductId;
            await _specificationAttributeService.UpdateVendorSpecificationAttribute(psa);
        }

        public async Task DeleteProductSpecificationAttribute(Vendor vendor, VendorSpecificationAttribute vsa)
        {
            vsa.VendorId = vendor.Id;
            vendor.VendorSpecificationAttributes.Remove(vsa);
            await _specificationAttributeService.DeleteVendorSpecificationAttribute(vsa);
        }

        public async Task<Address> UpdateAddress(Vendor vendor, AddressModel addressModel, Address address)
        {
            address = addressModel.ToEntity(address);
            await _vendorService.UpdateVendor(vendor);
            return address;
        }

        public async Task AddAddressToVendor(Vendor vendor, AddressModel addressModel)
        {
            var address = addressModel.ToEntity();
            addressModel.Id = address.Id;
            await _vendorService.InsertVendorAdress(vendor.Id,address);
        }
        
        public async Task RemoveAddressFromVendor(Vendor vendor, Address address)
        {
            vendor.Addresses.Remove(address);
            await _vendorService.UpdateVendor(vendor);
        }
    }
}
