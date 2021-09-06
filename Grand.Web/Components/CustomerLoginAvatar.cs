using Grand.Core;
using Grand.Domain.Customers;
using Grand.Domain.Media;
using Grand.Services.Common;
using Grand.Domain.Knowledgebase;
using Grand.Domain.Localization;
using Grand.Framework.Components;
using Grand.Services.Customers;
using Grand.Services.Media;
using Grand.Services.Orders;
using Grand.Services.Security;
using Grand.Web.Models.Common;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Web.ViewComponents
{
    public class CustomerLoginAvatarViewComponent : BaseViewComponent
    {
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly IPermissionService _permissionService;
        private readonly IPictureService _pictureService;
        private readonly ICustomerService _customerService;

        private readonly CustomerSettings _customerSettings;
        private readonly MediaSettings _mediaSettings;

        public CustomerLoginAvatarViewComponent(
            IWorkContext workContext,
            IStoreContext storeContext,
            IPermissionService permissionService,
            IPictureService pictureService,
            ICustomerService customerService,
            
            CustomerSettings customerSettings,
            MediaSettings mediaSettings
            )
        {
            _workContext = workContext;
            _storeContext = storeContext;
            _permissionService = permissionService;
            _pictureService = pictureService;
            _customerService = customerService;

            _customerSettings = customerSettings;
            _mediaSettings = mediaSettings;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var model = await PrepareCustomerLoginAvatar();
            return View(model);
        }

        private async Task<CustomerLoginAvatarModel> PrepareCustomerLoginAvatar()
        {
            var customer = _workContext.CurrentCustomer;

            var model = new CustomerLoginAvatarModel {
                CustomerId = customer.Id,
                FullName = customer.GetFullName()
            };
            //var customer = await _customerService.GetCustomerById(_workContext.CurrentCustomer.Id);
            

            if (_customerSettings.AllowCustomersToUploadAvatars)
            {
                var pictureId = customer.GetAttributeFromEntity<string>(SystemCustomerAttributeNames.AvatarPictureId);

                model.CustomerAvatarUrl = await _pictureService.GetPictureUrl(
                    pictureId,
                    _mediaSettings.AvatarPictureSize,
                    _customerSettings.DefaultAvatarEnabled,
                    defaultPictureType: PictureType.Avatar);

            }

            return model;
        }

    }
}