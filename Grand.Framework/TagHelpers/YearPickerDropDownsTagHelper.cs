using Grand.Framework.Mvc.Models;
using Grand.Services.Localization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System;
using System.Globalization;
using System.Threading.Tasks;

namespace Grand.Framework.TagHelpers
{
    [HtmlTargetElement("year-picker-dropdown")]
    public class YearPickerDropDownsTagHelper : TagHelper
    {
        private readonly IHtmlHelper _htmlHelper;
        private readonly ILocalizationService _localizationService;

        [ViewContext]
        public ViewContext ViewContext { get; set; }

        [HtmlAttributeName("Attribute")]
        public string Attribute { get; set; }

        [HtmlAttributeName("control-year")]
        public string ControlId_Year { get; set; }

        [HtmlAttributeName("begin-year")]
        public int Begin_Year { get; set; }

        [HtmlAttributeName("end-year")]
        public int End_Year { get; set; }

        [HtmlAttributeName("selected-year")]
        public int SelectedYear { get; set; }

        public YearPickerDropDownsTagHelper(IHtmlHelper htmlHelper, ILocalizationService localizationService)
        {
            _htmlHelper = htmlHelper;
            _localizationService = localizationService;
        }

        public override async Task ProcessAsync(TagHelperContext tagHelperContext, TagHelperOutput output)
        {
            (_htmlHelper as IViewContextAware).Contextualize(ViewContext);

            output.TagName = "div";
            output.TagMode = TagMode.StartTagAndEndTag;

            var model = new YearPickerDropDownsModel() {
                Attribute = Attribute,
                Begin_Year = Begin_Year,
                Year = ControlId_Year,
                End_Year = End_Year,
                SelectedYear = SelectedYear
            };

            model.SelectListYear.Add(new SelectListItem() { Value = "0", Text = _localizationService.GetResource("Common.Year") });

            if (Begin_Year == 0)
                Begin_Year = DateTime.UtcNow.Year - 100;
            if (End_Year == 0)
                End_Year = DateTime.UtcNow.Year;

            if (End_Year > Begin_Year)
            {
                for (var i = Begin_Year; i <= End_Year; i++)
                    model.SelectListYear.Add(new SelectListItem() { Value = i.ToString(), Text = i.ToString(), Selected = (SelectedYear == i) });
            }
            else
            {
                for (var i = Begin_Year; i >= End_Year; i--)
                    model.SelectListYear.Add(new SelectListItem() { Value = i.ToString(), Text = i.ToString(), Selected = (SelectedYear == i) });
            }

            output.Content.SetHtmlContent((await _htmlHelper.PartialAsync("_YearPickerDropDowns", model)).ToHtmlString());

        }
    }
}
