using FluentValidation;
using Grand.Core.Validators;
using Grand.Services.Localization;
using Grand.Web.Models.Ads;
using System.Collections.Generic;

namespace Grand.Web.Validators.Customer
{
    public class AddAdNoteValidator : BaseGrandValidator<AddAdNoteModel>
    {
        public AddAdNoteValidator(
            IEnumerable<IValidatorConsumer<AddAdNoteModel>> validators,
            ILocalizationService localizationService)
            : base(validators)
        {
            RuleFor(x => x.Note).NotEmpty().WithMessage(localizationService.GetResource("AdNote.Fields.Title.Required"));
        }
    }
}
