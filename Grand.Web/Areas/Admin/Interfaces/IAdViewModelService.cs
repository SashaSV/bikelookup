using Grand.Domain.Catalog;
using Grand.Domain.Common;
using Grand.Domain.Ads;
using Grand.Web.Areas.Admin.Models.Ads;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Grand.Web.Areas.Admin.Interfaces
{
    public interface IAdViewModelService
    {
        Task<AdListModel> PrepareAdListModel(int? orderStatusId = null, int? paymentStatusId = null, int? shippingStatusId = null, DateTime? startDate = null, string storeId = null, string code = null);
        Task<(IEnumerable<AdModel> orderModels, AdAggreratorModel aggreratorModel, int totalCount)> PrepareAdModel(AdListModel model, int pageIndex, int pageSize);
        Task PrepareAdDetailsModel(AdModel model, Ad order);
        Task<AdModel.AddAdProductModel> PrepareAddAdProductModel(Ad order);
        Task<AdModel.AddAdProductModel.ProductDetailsModel> PrepareAddProductToAdModel(Ad order, string productId);
        Task<AdAddressModel> PrepareAdAddressModel(Ad order, Address address);
        Task LogEditAd(string orderId);
        Task<IList<AdModel.AdNote>> PrepareAdNotes(Ad order);
        Task InsertAdNote(Ad order, string downloadId, bool displayToCustomer, string message);
        Task DeleteAdNote(Ad order, string id);
        Task<Address> UpdateAdAddress(Ad order, Address address, AdAddressModel model, string customAttributes);
        Task<IList<string>> AddProductToAdDetails(string orderId, string productId, IFormCollection form);
        Task EditCreditCardInfo(Ad order, AdModel model);
        Task<IList<Ad>> PrepareAds(AdListModel model);
        Task SaveAdTags(Ad order, string tags);

    }
}
