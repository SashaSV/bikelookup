namespace Grand.Web.Models.PrivateMessages
{
    public partial class PrivateMessageIndexModel
    {
        public int InboxPage { get; set; }
        public int SentItemsPage { get; set; }
        public bool SentItemsTabSelected { get; set; }
        public string AdId { get; set; }
    }
}