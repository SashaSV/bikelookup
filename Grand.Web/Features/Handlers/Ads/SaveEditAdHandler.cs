using Grand.Core;
using Grand.Domain.Ads;
using Grand.Domain.Catalog;
using Grand.Domain.Common;
using Grand.Domain.Customers;
using Grand.Domain.Data;
using Grand.Services.Ads;
using Grand.Services.Catalog;
using Grand.Services.Common;
using Grand.Services.Customers;
using Grand.Services.Media;
using Grand.Services.Vendors;
using Grand.Web.Features.Models.Ads;
using MediatR;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Web.Features.Handlers.Ads
{
    public class SaveEditAdHandler : IRequestHandler<EditAdSave, bool>
    {
        private readonly IAdService _adRepository;
        private readonly IProductService _productService;
        private readonly IVendorService _vendorService;
        private readonly IPictureService _pictureService;
        private readonly IWorkContext _workContext;
        private readonly ICustomerService _customerService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly ISpecificationAttributeService _atributeService;
        private readonly ISpecificationAttributeService _attributeProductService;

        public SaveEditAdHandler(IAdService adRepository, IProductService productService, IVendorService vendorService
            , IPictureService pictureService
            , IWorkContext workContext
            , ICustomerService customerService
            , IGenericAttributeService genericAttributeService
            , ISpecificationAttributeService atributeService
            , ISpecificationAttributeService attributeProductService)
        {
            _adRepository = adRepository;
            _productService = productService;
            _vendorService = vendorService;
            _pictureService = pictureService;
            _workContext = workContext;
            _customerService = customerService;
            _atributeService = atributeService;
            _attributeProductService = attributeProductService;
            _genericAttributeService = genericAttributeService;

        }

        public async Task<bool> Handle(EditAdSave request, CancellationToken cancellationToken)
        {
            var ad = await _adRepository.GetAdById(request.Model.Id);
            var product = await _productService.GetProductById(ad.AdItem.ProductId);


            ad.Price = request.Model.Price;
            ad.AdComment = request.Model.AdComment;
            ad.AdStatus = AdStatus.Active;
            ad.WithDocuments = request.Model.WithDocuments;
            ad.Mileage = request.Model.Mileage;
            ad.IsAuction = request.Model.IsAuction;

            var userId = ad.CustomerId == null ? ad.OwnerId : ad.CustomerId;
            var customerAd = await _customerService.GetCustomerById(userId);
            //await _vendorService.GetVendorByEmail(customerAd.Email, null);
            var vendor = await _adRepository.GetVendorByAd(ad);
            
            var payment = await _atributeService.GetSpecificationAttributeBySeName("v_pay");
            var oldPaymentAtributes = product.ProductSpecificationAttributes.Where(a => a.SpecificationAttributeId == payment.Id).ToList();
            foreach (var oldPaymentSettings in oldPaymentAtributes)
            {
                oldPaymentSettings.ProductId = product.Id;
                await _atributeService.DeleteProductSpecificationAttribute(oldPaymentSettings);
            }

            foreach (var paymentMethod in request.Model.SelectedPaymentMethods)
            {
                var psa = new ProductSpecificationAttribute {
                    AttributeTypeId = (int)SpecificationAttributeType.Option,
                    SpecificationAttributeOptionId = paymentMethod,
                    SpecificationAttributeId = payment.Id,
                    ProductId = product.Id,
                    AllowFiltering = true,
                    ShowOnProductPage = true,
                    ShowOnSellerPage = true,
                    DisplayOrder = 1,
                };
                await _atributeService.InsertProductSpecificationAttribute(psa);
            }
            
            var shipment = await _atributeService.GetSpecificationAttributeBySeName("v_delivery");
            var oldShipmentAtributes = product.ProductSpecificationAttributes.Where(a => a.SpecificationAttributeId == shipment.Id).ToList();
            
            foreach (var oldShipmentSettings in oldShipmentAtributes)
            {
                oldShipmentSettings.ProductId = product.Id;
                await _atributeService.DeleteProductSpecificationAttribute(oldShipmentSettings);
            }

            foreach (var shipMethod in request.Model.SelectedShippingMethods)
            {
                var psa = new ProductSpecificationAttribute {
                    AttributeTypeId = (int)SpecificationAttributeType.Option,
                    SpecificationAttributeOptionId = shipMethod,
                    SpecificationAttributeId = shipment.Id,
                    ProductId = product.Id,
                    AllowFiltering = true,
                    ShowOnProductPage = true,
                    ShowOnSellerPage = true,
                    DisplayOrder = 1,
                };
                //product.ProductSpecificationAttributes.Add(psa);
                await _atributeService.InsertProductSpecificationAttribute(psa);
            }

            await _customerService.UpdateAddressFromCustomerFileds(customerAd);
            vendor.Addresses = customerAd.Addresses;
            vendor.Name = customerAd.Addresses.First().Company;

            if (product != null)
            {
                product.VendorId = ad.AdItem.VendorId;
                product.Price = request.Model.Price;
                await _productService.UpdateProduct(product);
            }

            var venAddr = ad.ShippingAddress;
            
            if (venAddr == null) {
                if (userId != null)
                {
                    var customerName = string.Format("{0} ({1})", 
                            customerAd.FormatUserName(CustomerNameFormat.ShowFirstName), 
                                customerAd.Addresses.First().City);
                    
                    vendor.Addresses = customerAd.Addresses;

                    await _vendorService.UpdateVendor(vendor);
                    ad.ShippingAddress = customerAd.ShippingAddress;
                }
            }

            await _adRepository.UpdateAd(ad);
            return true;
        }
    }

}