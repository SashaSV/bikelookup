using Grand.Domain.Ads;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Grand.Services.Ads
{
    /// <summary>
    /// Ad's tag service interface
    /// </summary>
    public partial interface IAdTagService
    {
        /// <summary>
        /// Delete a order's tag
        /// </summary>
        /// <param name="orderTag">Ad's tag</param>
        Task DeleteAdTag(AdTag orderTag);

        /// <summary>
        /// Gets all order's tags
        /// </summary>
        /// <returns>order's tags</returns>
        Task<IList<AdTag>> GetAllAdTags();

        /// <summary>
        /// Gets order's tag
        /// </summary>
        /// <param name="orderTagId">order's tag identifier</param>
        /// <returns>order's tag</returns>
        Task<AdTag> GetAdTagById(string orderTagId);

        /// <summary>
        /// Gets order tag by name
        /// </summary>
        /// <param name="name">order tag name</param>
        /// <returns>order tag</returns>
        Task<AdTag> GetAdTagByName(string name);

        /// <summary>
        /// Inserts a order tag
        /// </summary>
        /// <param name="orderTag">order tag</param>
        Task InsertAdTag(AdTag orderTag);

        /// <summary>
        /// Update a order tag
        /// </summary>
        /// <param name="orderTag">order tag</param>
        Task UpdateAdTag(AdTag orderTag);

        /// <summary>
        /// Assign a tag to the order
        /// </summary>
        /// <param name="orderTag">order Tag</param>
        Task AttachAdTag(string orderTagId, string orderId);

        /// <summary>
        /// Detach a tag from the order
        /// </summary>
        /// <param name="orderTag">order Tag</param>
        Task DetachAdTag(string orderTagId, string orderId);

        /// <summary>
        /// Get number of orders
        /// </summary>
        /// <param name="orderTagId">order tag identifier</param>
        /// <param name="storeId">Store identifier</param>
        /// <returns>Number of orders</returns>
        Task<int> GetAdCount(string orderTagId, string storeId);
    }
}
