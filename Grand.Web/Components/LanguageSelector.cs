using Grand.Core;
using Grand.Domain.Localization;
using Grand.Framework.Components;
using Grand.Services.Localization;
using Grand.Web.Models.Common;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;
using MailKit.Search;

namespace Grand.Web.ViewComponents
{
    public class LanguageSelectorViewComponent : BaseViewComponent
    {
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly ILanguageService _languageService;
        private readonly LocalizationSettings _localizationSettings;

        public LanguageSelectorViewComponent(
            IWorkContext workContext,
            IStoreContext storeContext,
            ILanguageService languageService,
            LocalizationSettings localizationSettings
            )
        {
            _workContext = workContext;
            _storeContext = storeContext;
            _languageService = languageService;
            _localizationSettings = localizationSettings;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var model = await PrepareLanguageSelector();
            if (model.AvailableLanguages.Count == 1)
                Content("");

            return View(model);
        }

        private async Task<LanguageSelectorModel> PrepareLanguageSelector()
        {
            var availableLanguages = (await _languageService
                     .GetAllLanguages(storeId: _storeContext.CurrentStore.Id))
                     .OrderBy(x => x.DisplayOrder)
                     .Select(x => new LanguageModel {
                         Id = x.Id,
                         Name = x.Name,
                         FlagImageFileName = x.FlagImageFileName,
                         DisplayOrder = x.DisplayOrder
                     }).ToList();

            var model = new LanguageSelectorModel {
                CurrentLanguageId = _workContext.WorkingLanguage.Id,
                AvailableLanguages = availableLanguages,
                UseImages = _localizationSettings.UseImagesForLanguageSelection,
                
            };

            return model;
        }
    }
}