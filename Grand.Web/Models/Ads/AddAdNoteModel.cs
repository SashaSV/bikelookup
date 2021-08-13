using Grand.Core.Models;

namespace Grand.Web.Models.Ads
{
    public class AddAdNoteModel : BaseModel
    {
        public string AdId { get; set; }
        public string Note { get; set; }
    }
}
