using Grand.Web.Models.Ads;
using Grand.Web.Models.Common;
using System.Collections.Generic;

namespace Grand.Web.Models.PrivateMessages
{
    public partial class PrivateMessageChatModel
    {
        public IList<PrivateMessageModel> Messages { get; set; }
        public SendPrivateMessageModel SendPrivateMessage { get; set; }
        public string ToCustomerId { get; set; }
        public string AdId { get; set; }
        public string Subject { get; set; }
        public IList<System.DateTime> Dates { get; set; }
        public CustomerAdListModel.AdDetailsModel Ad { get; set; }
        public bool IsVisibleMessageChat { get; set; }
    }
}