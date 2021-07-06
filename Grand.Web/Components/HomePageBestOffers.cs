using Grand.Core;
using Grand.Domain.Catalog;
using Grand.Framework.Components;
using Grand.Services.Queries.Models.Catalog;
using Grand.Web.Features.Models.Products;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Web.Components
{
    public class HomePageBestOffersViewComponent : BaseViewComponent
    {
        #region Fields

        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly IMediator _mediator;
        private readonly CatalogSettings _catalogSettings;

        #endregion

        #region Constructors

        public HomePageBestOffersViewComponent(
            IWorkContext workContext,
            IStoreContext storeContext,
            IMediator mediator,
            CatalogSettings catalogSettings)
        {
            _workContext = workContext;
            _storeContext = storeContext;
            _mediator = mediator;
            _catalogSettings = catalogSettings;
        }

        #endregion

        #region Invoker

        public async Task<IViewComponentResult> InvokeAsync(int? productThumbPictureSize)
        {
            if (!_catalogSettings.NewProductsOnHomePage)
                return Content("");

            var products = (await _mediator.Send(new GetSearchProductsQuery() {
                Customer = _workContext.CurrentCustomer,
                StoreId = _storeContext.CurrentStore.Id,
                VisibleIndividuallyOnly = true,
                Discount = true,
                OrderBy = ProductSortingEnum.DiscountSize,
                PageSize = _catalogSettings.DiscountProductsNumberOnHomePage
            })).products;

            if (!products.Any())
                return Content("");

            var model = await _mediator.Send(new GetProductOverview() {
                PreparePictureModel = true,
                PreparePriceModel = true,
                PrepareSpecificationAttributes = _catalogSettings.ShowSpecAttributeOnCatalogPages,
                ProductThumbPictureSize = productThumbPictureSize,
                Products = products,
            });

            return View(model);
        }

        #endregion

    }
}
