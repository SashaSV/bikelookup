using Grand.Domain.Ads;
using Grand.Web.Models.Ads;
using MediatR;

namespace Grand.Web.Events
{
    public class AdNoteEvent : INotification
    {
        public Ad Ad { get; private set; }
        public AddAdNoteModel NoteModel { get; private set; }
        public AdNoteEvent(Ad ad, AddAdNoteModel noteModel)
        {
            Ad = ad;
            NoteModel = noteModel;
        }
    }
}
