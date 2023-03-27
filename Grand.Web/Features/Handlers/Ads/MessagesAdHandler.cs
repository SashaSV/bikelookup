using Grand.Domain.Customers;
using Grand.Services.Ads;
using Grand.Services.Catalog;
using Grand.Services.Directory;
using Grand.Services.Helpers;
using Grand.Services.Localization;
using Grand.Web.Features.Models.Ads;
using Grand.Web.Models.PrivateMessages;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Web.Features.Handlers.Ads
{
    //public class MessagesAdHandler : IRequestHandler<MessagesAd, PrivateMessageIndexModel>
    //{

    //    public MessagesAdHandler()
    //    {

    //    }

    //    public PrivateMessageIndexModel Handle(MessagesAd request, CancellationToken cancellationToken)
    //    {
    //        var inboxPage = 0;
    //        var sentItemsPage = 0;
    //        var sentItemsTabSelected = false;

    //        var tab = "";
    //        var pageNumber = 0;
    //        switch (tab)
    //        {
    //            case "inbox":
    //                if (pageNumber > 0 )
    //                {
    //                    inboxPage = pageNumber;
    //                }
    //                break;
    //            case "sent":
    //                if (pageNumber > 0 )
    //                {
    //                    sentItemsPage = pageNumber;
    //                }
    //                sentItemsTabSelected = true;
    //                break;
    //            default:
    //                break;
    //        }

    //        var model = new PrivateMessageIndexModel {
    //            InboxPage = inboxPage,
    //            SentItemsPage = sentItemsPage,
    //            SentItemsTabSelected = sentItemsTabSelected,
    //            AdId = request.Ad.Id
    //        };

    //        return  model;
    //    }
    //}
}
