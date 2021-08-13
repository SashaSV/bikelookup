using Grand.Domain.Ads;
using Grand.Domain.Localization;
using Grand.Web.Models.Ads;

using MediatR;

namespace Grand.Web.Commands.Models.Ads
{
    public class InsertAdNoteCommand : IRequest<AdNote>
    {
        public Ad Ad { get; set; } 
        public Language Language { get; set; }
        public AddAdNoteModel AdNote { get; set; }
    }
}
