using Grand.Core;
using Grand.Domain.Ads;
using Grand.Domain.Catalog;
using Grand.Domain.Customers;
using Grand.Domain.Forums;
using Grand.Domain.Localization;
using Grand.Domain.Media;
using Grand.Framework.Controllers;
using Grand.Services.Ads;
using Grand.Services.Catalog;
using Grand.Services.Customers;
using Grand.Services.Forums;
using Grand.Services.Helpers;
using Grand.Services.Localization;
using Grand.Services.Logging;
using Grand.Services.Media;
using Grand.Services.Queries.Models.Ads;
using Grand.Web.Models.Ads;
using Grand.Web.Models.Common;
using Grand.Web.Models.Media;
using Grand.Web.Models.PrivateMessages;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Web.Controllers
{
    public partial class PrivateMessagesAdController : BasePublicController
    {
        #region Fields

        private readonly IForumService _forumService;
        private readonly ICustomerService _customerService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly ILocalizationService _localizationService;
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IAdService _adService;
        private readonly IProductService _productService;
        private readonly IPictureService _pictureService;
        readonly IMediator _mediator;
        private readonly ForumSettings _forumSettings;
        private readonly CustomerSettings _customerSettings;
        private readonly MediaSettings _mediaSettings;

        #endregion

        #region Constructors

        public PrivateMessagesAdController(IForumService forumService,
            ICustomerService customerService, ICustomerActivityService customerActivityService,
            ILocalizationService localizationService, IWorkContext workContext,
            IStoreContext storeContext, IDateTimeHelper dateTimeHelper,
            ForumSettings forumSettings, CustomerSettings customerSettings,
            IAdService adService, IProductService productService, IPictureService pictureService, IMediator mediator, MediaSettings mediaSettings)
        {
            _forumService = forumService;
            _customerService = customerService;
            _customerActivityService = customerActivityService;
            _localizationService = localizationService;
            _workContext = workContext;
            _storeContext = storeContext;
            _dateTimeHelper = dateTimeHelper;
            _forumSettings = forumSettings;
            _customerSettings = customerSettings;
            _adService = adService;
            _productService = productService;
            _pictureService = pictureService;
            _mediator = mediator;
            _mediaSettings = mediaSettings;
        }

        #endregion

        #region Methods

        public virtual IActionResult Index(int? pageNumber, string tab)
        {
            if (!_forumSettings.AllowPrivateMessages)
            {
                return RedirectToRoute("HomePage");
            }

            if (_workContext.CurrentCustomer.IsGuest())
            {
                return Challenge();
            }

            int inboxPage = 0;
            int sentItemsPage = 0;
            bool sentItemsTabSelected = false;

            switch (tab)
            {
                case "inbox":
                    if (pageNumber.HasValue)
                    {
                        inboxPage = pageNumber.Value;
                    }
                    break;
                case "sent":
                    if (pageNumber.HasValue)
                    {
                        sentItemsPage = pageNumber.Value;
                    }
                    sentItemsTabSelected = true;
                    break;
                default:
                    break;
            }

            var model = new PrivateMessageIndexModel {
                InboxPage = inboxPage,
                SentItemsPage = sentItemsPage,
                SentItemsTabSelected = sentItemsTabSelected
            };

            return View(model);
        }

        [HttpPost, FormValueRequired("delete-inbox"), ActionName("InboxUpdate")]
        [AutoValidateAntiforgeryToken]
        public virtual async Task<IActionResult> DeleteInboxPM(IFormCollection formCollection)
        {
            foreach (var key in formCollection.Keys)
            {
                var value = formCollection[key];

                if (value.Equals("on") && key.StartsWith("pm", StringComparison.OrdinalIgnoreCase))
                {
                    var id = key.Replace("pm", "").Trim();
                    var pm = await _forumService.GetPrivateMessageById(id);
                    if (pm != null)
                    {
                        if (pm.ToCustomerId == _workContext.CurrentCustomer.Id)
                        {
                            pm.IsDeletedByRecipient = true;
                            await _forumService.UpdatePrivateMessage(pm);
                        }
                    }
                }
            }
            return RedirectToRoute("PrivateMessages");
        }

        [HttpPost, FormValueRequired("mark-unread"), ActionName("InboxUpdate")]
        [AutoValidateAntiforgeryToken]
        public virtual async Task<IActionResult> MarkUnread(IFormCollection formCollection)
        {
            foreach (var key in formCollection.Keys)
            {
                var value = formCollection[key];

                if (value.Equals("on") && key.StartsWith("pm", StringComparison.OrdinalIgnoreCase))
                {
                    var id = key.Replace("pm", "").Trim();
                    var pm = await _forumService.GetPrivateMessageById(id);
                    if (pm != null)
                    {
                        if (pm.ToCustomerId == _workContext.CurrentCustomer.Id)
                        {
                            pm.IsRead = false;
                            await _forumService.UpdatePrivateMessage(pm);
                        }
                    }
                }
            }
            return RedirectToRoute("PrivateMessages");
        }

        //updates sent items (deletes PrivateMessages)
        [HttpPost, FormValueRequired("delete-sent"), ActionName("SentUpdate")]
        [AutoValidateAntiforgeryToken]
        public virtual async Task<IActionResult> DeleteSentPM(IFormCollection formCollection)
        {
            foreach (var key in formCollection.Keys)
            {
                var value = formCollection[key];

                if (value.Equals("on") && key.StartsWith("si", StringComparison.OrdinalIgnoreCase))
                {
                    var id = key.Replace("si", "").Trim();
                    PrivateMessage pm = await _forumService.GetPrivateMessageById(id);
                    if (pm != null)
                    {
                        if (pm.FromCustomerId == _workContext.CurrentCustomer.Id)
                        {
                            pm.IsDeletedByAuthor = true;
                            await _forumService.UpdatePrivateMessage(pm);
                        }
                    }
                }

            }
            return RedirectToRoute("PrivateMessages", new { tab = "sent" });
        }

        public virtual async Task<IActionResult> SentPMChat(string toCustomerId, string replyToMessageId)
        {
            if (!_forumSettings.AllowPrivateMessages)
            {
                return RedirectToRoute("HomePage");
            }

            if (_workContext.CurrentCustomer.IsGuest())
            {
                return Challenge();
            }

            var customerTo = await _customerService.GetCustomerById(toCustomerId);

            if (customerTo == null || customerTo.IsGuest())
            {
                return RedirectToRoute("PrivateMessages");
            }

            var model = new SendPrivateMessageModel();
            model.ToCustomerId = customerTo.Id;
            model.CustomerToName = customerTo.FormatUserName(_customerSettings.CustomerNameFormat);
            model.AllowViewingToProfile = _customerSettings.AllowViewingProfiles && !customerTo.IsGuest();

            if (!String.IsNullOrEmpty(replyToMessageId))
            {
                var replyToPM = await _forumService.GetPrivateMessageById(replyToMessageId);
                if (replyToPM == null)
                {
                    return RedirectToRoute("PrivateMessages");
                }

                if (replyToPM.ToCustomerId == _workContext.CurrentCustomer.Id || replyToPM.FromCustomerId == _workContext.CurrentCustomer.Id)
                {
                    model.ReplyToMessageId = replyToPM.Id;
                    model.Subject = string.Format("Re: {0}", replyToPM.Subject);
                }
                else
                {
                    return RedirectToRoute("PrivateMessages");
                }
            }
            return View(model);
        }

        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public virtual async Task<IActionResult> SentPMChat(PrivateMessageChatModel chatModel)
        {
            if (!_forumSettings.AllowPrivateMessages)
            {
                return RedirectToRoute("HomePage");
            }

            if (_workContext.CurrentCustomer.IsGuest())
            {
                return Challenge();
            }

            var model = chatModel;
            var sentPM = chatModel.SendPrivateMessage;
            Customer toCustomer = null;
            toCustomer = await _customerService.GetCustomerById(model.ToCustomerId);
            //var replyToPM = await _forumService.GetPrivateMessageById(sentPM.ReplyToMessageId);
            //if (replyToPM != null)
            //{
            //    if (replyToPM.ToCustomerId == _workContext.CurrentCustomer.Id || replyToPM.FromCustomerId == _workContext.CurrentCustomer.Id)
            //    {
            //        toCustomer = replyToPM.FromCustomerId == _workContext.CurrentCustomer.Id
            //                    ? await _customerService.GetCustomerById(replyToPM.ToCustomerId)
            //                    : await _customerService.GetCustomerById(replyToPM.FromCustomerId);

            //    }
            //    else
            //    {
            //        return RedirectToRoute("PrivateMessages");
            //    }
            //}
            //else
            //{
            //    toCustomer = await _customerService.GetCustomerById(model.ToCustomerId);
            //}

            if (toCustomer == null || toCustomer.IsGuest())
            {
                return RedirectToRoute("PrivateMessages");
            }
            model.ToCustomerId = toCustomer.Id;
            model.SendPrivateMessage.CustomerToName = toCustomer.FormatUserName(_customerSettings.CustomerNameFormat);
            model.SendPrivateMessage.AllowViewingToProfile = _customerSettings.AllowViewingProfiles && !toCustomer.IsGuest();

            if (ModelState.IsValid || true)
            {
                try
                {
                    string subject = model.Subject;
                    if (_forumSettings.PMSubjectMaxLength > 0 && subject.Length > _forumSettings.PMSubjectMaxLength)
                    {
                        subject = subject.Substring(0, _forumSettings.PMSubjectMaxLength);
                    }

                    var text = sentPM.Message;
                    if (_forumSettings.PMTextMaxLength > 0 && text.Length > _forumSettings.PMTextMaxLength)
                    {
                        text = text.Substring(0, _forumSettings.PMTextMaxLength);
                    }

                    var nowUtc = DateTime.UtcNow;

                    var privateMessage = new PrivateMessage {
                        StoreId = _storeContext.CurrentStore.Id,
                        ToCustomerId = toCustomer.Id,
                        FromCustomerId = _workContext.CurrentCustomer.Id,
                        Subject = subject,
                        Text = text,
                        IsDeletedByAuthor = false,
                        IsDeletedByRecipient = false,
                        IsRead = false,
                        CreatedOnUtc = nowUtc,
                        AdId = model.AdId
                    };

                    await _forumService.InsertPrivateMessage(privateMessage);

                    //activity log
                    await _customerActivityService.InsertActivity("PublicStore.SendPM", "", _localizationService.GetResource("ActivityLog.PublicStore.SendPM"), toCustomer.Email);
                    
                    return RedirectToRoute("ViewAdPM", new { AdId = model.AdId, toCustomerId = model.ToCustomerId });
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
            }
            return RedirectToRoute("ViewAdPM", new { AdId = model.AdId, toCustomerId = model.ToCustomerId });
            //return View(model);
        }

        public virtual async Task<IActionResult> ViewAdPM(string AdId, string toCustomerId)
        {
            if (!_forumSettings.AllowPrivateMessages)
            {
                return RedirectToRoute("HomePage");
            }

            if (_workContext.CurrentCustomer.IsGuest())
            {
                return Challenge();
            }
           
            var messages = new List<PrivateMessageModel>();
            
            var fromCustomer = await _customerService.GetCustomerById(_workContext.CurrentCustomer.Id);
            var toCustomer = await _customerService.GetCustomerById(toCustomerId);
            var ad = await _adService.GetAdById(AdId);
            var rp = await _productService.GetProductById(ad.ProductId);

            messages = await GetMessage(messages, ad, rp, fromCustomer, toCustomer);
            messages = await GetMessage(messages, ad, rp, toCustomer, fromCustomer);

            var messages2 = messages.OrderBy(x => x.CreatedOn).ToList();
            var newMessage = new SendPrivateMessageModel() {
                ToCustomerId = toCustomerId,
                CustomerToName = toCustomer.FormatUserName(_customerSettings.CustomerNameFormat),
                Subject = rp.Name,
                AdId = ad.Id
            };

            var dates = messages2.Select(x => x.CreatedOn.Date).Distinct().OrderBy(x => x.Date).ToList();
            var adModel = await PrepareAd(_workContext.WorkingLanguage, ad);

            var model = new PrivateMessageChatModel {
                Messages = messages2,
                ToCustomerId = toCustomerId,
                AdId = ad.Id, 
                Subject = rp.Name,
                Dates = dates,
                Ad = adModel
            };

            return View(model);
        }

        public virtual async Task<CustomerAdListModel.AdDetailsModel> PrepareAd(Language curLanguage, Ad ad)
        {
            var rp = await _productService.GetProductById(ad.ProductId);
            var nameProduct = rp == null ? "" : rp.Name;

            var productIdAd = ad.AdItem == null ? "" : ad.AdItem.ProductId;
            var rpAd = await _productService.GetProductById(productIdAd);
            var firstPictureId = rpAd == null || rpAd.ProductPictures == null ? "" : rpAd.ProductPictures.First().PictureId;

            var pictureModel = new PictureModel {
                Id = firstPictureId,
                FullSizeImageUrl = await _pictureService.GetPictureUrl(firstPictureId),
                ImageUrl = await _pictureService.GetPictureUrl(firstPictureId, _mediaSettings.VendorThumbPictureSize),
                Title = string.Format(_localizationService.GetResource("Media.Vendor.ImageLinkTitleFormat"), nameProduct),
                AlternateText = string.Format(_localizationService.GetResource("Media.Ad.ImageAlternateTextFormat"), nameProduct),
            };

            var adModel = new CustomerAdListModel.AdDetailsModel {
                Id = ad.Id,
                AdNumber = ad.AdNumber,
                AdCode = ad.Code,
                CustomerEmail = ad.BillingAddress?.Email,
                CreatedOn = _dateTimeHelper.ConvertToUserTime(ad.CreatedOnUtc, DateTimeKind.Utc),
                EndDateTimeUtc = _dateTimeHelper.ConvertToUserTime(ad.EndDateTimeUtc, DateTimeKind.Utc),
                AdStatusEnum = ad.AdStatus,
                Price = ad.Price,
                AdStatus = ad.AdStatus.GetLocalizedEnum(_localizationService, curLanguage.Id),
                PaymentStatus = ad.PaymentStatus.GetLocalizedEnum(_localizationService, curLanguage.Id),
                ShippingStatus = ad.ShippingStatus.GetLocalizedEnum(_localizationService, curLanguage.Id),
                IsOpenFromMenu = false,
                PictureModel = pictureModel,
                AdComment = ad.AdComment
            };
            return adModel;
        }

        public virtual async Task<List<PrivateMessageModel>> GetMessage(List<PrivateMessageModel> message, Ad ad, Product rp, Customer fromCustomer, Customer toCustomer)
        {
            var pageNumber = 0;
            if (pageNumber > 0)
            {
                pageNumber -= 1;
            }

            var pageSize = _forumSettings.PrivateMessagesPageSize;

            var list = await _forumService.GetAllPrivateMessages(_storeContext.CurrentStore.Id,
                fromCustomer.Id, toCustomer.Id, ad.Id, null, null, false, string.Empty, pageNumber, pageSize);
            
            foreach (var pm in list)
            {
                if (!pm.IsRead && pm.ToCustomerId == _workContext.CurrentCustomer.Id) {
                    pm.IsRead = true;
                    await _forumService.UpdatePrivateMessage(pm);
                }
                message.Add(new PrivateMessageModel {
                    Id = pm.Id,
                    FromCustomerId = (fromCustomer.Id == _workContext.CurrentCustomer.Id) ? string.Empty : fromCustomer.Id,
                    CustomerFromName = fromCustomer.FormatUserName(_customerSettings.CustomerNameFormat),
                    AllowViewingFromProfile = _customerSettings.AllowViewingProfiles && fromCustomer != null && !fromCustomer.IsGuest(),
                    ToCustomerId = (toCustomer.Id == _workContext.CurrentCustomer.Id) ? string.Empty : toCustomer.Id,
                    CustomerToName = toCustomer.FormatUserName(_customerSettings.CustomerNameFormat),
                    AllowViewingToProfile = _customerSettings.AllowViewingProfiles && toCustomer != null && !toCustomer.IsGuest(),
                    Subject = rp.Name,
                    Message = pm.Text,
                    CreatedOn = _dateTimeHelper.ConvertToUserTime(pm.CreatedOnUtc, DateTimeKind.Utc),
                    IsRead = pm.IsRead,
                    AdId = ad.Id,
                    AdProductName = rp.Name
                });
            }
            return message;
        }
        public virtual async Task<IActionResult> DeletePM(string privateMessageId)
        {
            if (!_forumSettings.AllowPrivateMessages)
            {
                return RedirectToRoute("HomePage");
            }

            if (_workContext.CurrentCustomer.IsGuest())
            {
                return Challenge();
            }

            var pm = await _forumService.GetPrivateMessageById(privateMessageId);
            if (pm != null)
            {
                if (pm.FromCustomerId == _workContext.CurrentCustomer.Id)
                {
                    pm.IsDeletedByAuthor = true;
                    await _forumService.UpdatePrivateMessage(pm);
                }

                if (pm.ToCustomerId == _workContext.CurrentCustomer.Id)
                {
                    pm.IsDeletedByRecipient = true;
                    await _forumService.UpdatePrivateMessage(pm);
                }
            }
            return RedirectToRoute("PrivateMessages");
        }

        #endregion
    }
}
