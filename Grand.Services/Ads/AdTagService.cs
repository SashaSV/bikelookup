using Grand.Core.Caching;
using Grand.Domain.Data;
using Grand.Domain.Ads;
using System.Threading.Tasks;
using MediatR;
using System.Linq;
using System;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Grand.Services.Events;

namespace Grand.Services.Ads
{
    public partial class AdTagService : IAdTagService
    {
        #region Constants

        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : store ID
        /// </remarks>
        private const string ADTAG_COUNT_KEY = "Grand.adtag.count-{0}";

        /// <summary>
        /// Key for all tags
        /// </summary>
        private const string ADTAG_ALL_KEY = "Grand.adtag.all";

        /// <summary>
        /// Key pattern to clear cache
        /// </summary>
        private const string ADTAG_PATTERN_KEY = "Grand.adtag.";

        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : ad ID
        /// </remarks>
        private const string ADS_BY_ID_KEY = "Grand.ad.id-{0}";

        /// <summary>
        /// Key pattern to clear cache
        /// </summary>        
        private const string ADS_PATTERN_KEY = "Grand.ad.";


        #endregion

        #region Fields

        private readonly IRepository<AdTag> _adTagRepository;
        private readonly IRepository<Ad> _adRepository;
        private readonly ICacheManager _cacheManager;
        private readonly IMediator _mediator;

        #endregion

        #region Ctor

        public AdTagService(IRepository<AdTag> adTagRepository,
            IRepository<Ad> adRepository,
            ICacheManager cacheManager,
            IMediator mediator
            )
        {
            _adTagRepository = adTagRepository;
            _adRepository = adRepository;
            _mediator = mediator;
            _cacheManager = cacheManager;
        }

        #endregion
                
        #region Utilities

        /// <summary>
        /// Get ad's  count for each of existing ad tag
        /// </summary>
        /// <param name="storeId">Store identifier</param>
        /// <returns>Dictionary of "ad's tag ID : ad's count"</returns>
        private async Task<Dictionary<string, int>> GetAdCount(string adTagId)
        {
            string key = string.Format(ADTAG_COUNT_KEY, adTagId);
            return await _cacheManager.GetAsync(key, async () => 
            {
                var query = from ot in _adTagRepository.Table
                            select ot;

                var dictionary = new Dictionary<string, int>();
                foreach (var tag in await query.ToListAsync())
                {
                    dictionary.Add(tag.Id, tag.Count);
                }
                return dictionary;
            });
        }

        #endregion

        #region Methods

        /// <summary>
        /// Delete an ad's tag
        /// </summary>
        /// <param name="adTag">Ad's tag</param>
        public virtual async Task DeleteAdTag(AdTag adTag)
        {
            if (adTag == null)
                throw new ArgumentNullException("adTag");

            var builder = Builders<Ad>.Update;
            var updatefilter = builder.Pull(x => x.AdTags, adTag.Id);
            await _adRepository.Collection.UpdateManyAsync(new BsonDocument(), updatefilter);

            await _adTagRepository.DeleteAsync(adTag);

            //cache
            await _cacheManager.RemoveByPrefix(ADTAG_PATTERN_KEY);
            await _cacheManager.RemoveByPrefix(ADS_PATTERN_KEY);

            //event notification
            await _mediator.EntityDeleted(adTag);
        }

        /// <summary>
        /// Gets all ad tags
        /// </summary>
        /// <returns>Ad tags</returns>
        public virtual async Task<IList<AdTag>> GetAllAdTags()
        {
            var query = _adTagRepository.Table;
            return await query.ToListAsync();
        }

        /// <summary>
        /// Gets ad's tag by id
        /// </summary>
        /// <param name="adTagId">Ad's tag identifier</param>
        /// <returns>Ad's tag</returns>
        public virtual Task<AdTag> GetAdTagById(string adTagId)
        {
            return _adTagRepository.GetByIdAsync(adTagId);
        }

        /// <summary>
        /// Gets ad's tag by name
        /// </summary>
        /// <param name="name">Ad's tag name</param>
        /// <returns>Ad's tag</returns>
        public virtual Task<AdTag> GetAdTagByName(string name)
        {
            var query = from pt in _adTagRepository.Table
                        where pt.Name == name
                        select pt;

            return query.FirstOrDefaultAsync();
        }

        /// <summary>
        /// Inserts a ad's tag
        /// </summary>
        /// <param name="adTag">Ad's tag</param>
        public virtual async Task InsertAdTag(AdTag adTag)
        {
            if (adTag == null)
                throw new ArgumentNullException("adTag");

            await _adTagRepository.InsertAsync(adTag);

            //cache
            await _cacheManager.RemoveByPrefix(ADTAG_PATTERN_KEY);

            //event notification
            await _mediator.EntityInserted(adTag);
        }

        /// <summary>
        /// Updating a ad's tag
        /// </summary>
        /// <param name="adTag">Ad tag</param>
        public virtual async Task UpdateAdTag(AdTag adTag)
        {
            if (adTag == null)
                throw new ArgumentNullException("adTag");

            await _adTagRepository.UpdateAsync(adTag);

            //cache
            await _cacheManager.RemoveByPrefix(ADTAG_PATTERN_KEY);

            //event notification
            await _mediator.EntityUpdated(adTag);
        }

        /// <summary>
        /// Attach a tag to the ad
        /// </summary>
        /// <param name="adTag">Ad's identification</param>
        public virtual async Task AttachAdTag(string adTagId, string adId)
        {
            var updateBuilder = Builders<Ad>.Update;
            var update = updateBuilder.AddToSet(p => p.AdTags, adTagId);
            await _adRepository.Collection.UpdateOneAsync(new BsonDocument("_id", adId), update);
            
            // update adtag with count's ad and new ad id
            var updateBuilderTag = Builders<AdTag>.Update
                .Inc(x => x.Count, 1);

            await _adTagRepository.Collection.UpdateOneAsync(new BsonDocument("_id", adTagId), updateBuilderTag);
            var adTag =   await _adTagRepository.GetByIdAsync(adTagId);

            //cache
            await _cacheManager.RemoveAsync(string.Format(ADS_BY_ID_KEY, adId));
            await _cacheManager.RemoveAsync(string.Format(ADTAG_COUNT_KEY, adTagId));

            //event notification
            await _mediator.EntityUpdated(adTag);
        }

        // <summary>
        /// Detach a tag from the ad
        /// </summary>
        /// <param name="adTag">Ad Tag</param>
        public virtual async Task DetachAdTag(string adTagId, string adId)
        {
            var updateBuilder = Builders<Ad>.Update;
            var update = updateBuilder.Pull(p => p.AdTags, adTagId);
            await _adRepository.Collection.UpdateOneAsync(new BsonDocument("_id", adId), update);
            
            var updateTag = Builders<AdTag>.Update
                .Inc(x => x.Count, -1);
            await _adTagRepository.Collection.UpdateManyAsync(new BsonDocument("_id", adTagId), updateTag);

            //cache
            await _cacheManager.RemoveAsync(string.Format(ADS_BY_ID_KEY, adId));
            await _cacheManager.RemoveAsync(string.Format(ADTAG_COUNT_KEY, adTagId));
        }

        /// <summary>
        /// Get number of ads
        /// </summary>
        /// <param name="adTagId">Ad's tag identifier</param>
        /// <param name="storeId">Store identifier</param>
        /// <returns>Number of ads</returns>
        public virtual async Task<int> GetAdCount(string adTagId, string storeId)
        {
            var dictionary = await GetAdCount(adTagId);
            if (dictionary.ContainsKey(adTagId))
                return dictionary[adTagId];

            return 0;
        }

        #endregion
    }
}
