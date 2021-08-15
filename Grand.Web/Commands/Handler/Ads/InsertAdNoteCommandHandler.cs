using Grand.Domain.Ads;
using Grand.Services.Messages;
using Grand.Services.Ads;
using Grand.Web.Commands.Models.Ads;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Web.Commands.Handler.Ads
{
    public class InsertAdNoteCommandHandler : IRequestHandler<InsertAdNoteCommand, AdNote>
    {
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly IAdService _adService;

        public InsertAdNoteCommandHandler(IWorkflowMessageService workflowMessageService, IAdService adService)
        {
            _workflowMessageService = workflowMessageService;
            _adService = adService;
        }

        public async Task<AdNote> Handle(InsertAdNoteCommand request, CancellationToken cancellationToken)
        {
            var adNote = new AdNote {
                CreatedOnUtc = DateTime.UtcNow,
                DisplayToCustomer = true,
                Note = request.AdNote.Note,
                AdsId = request.AdNote.AdId,
                CreatedByCustomer = true
            };
            await _adService.InsertAdsNote(adNote);

            //email
            await _workflowMessageService.SendNewAdNoteAddedCustomerNotification(request.Ad, adNote);

            return adNote;
        }
    }
}
