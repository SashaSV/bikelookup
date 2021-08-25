using Grand.Domain.Ads;
using Grand.Core.Html;
using Grand.Services.Catalog;
using Grand.Services.Shipping;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Text;
using System.Collections.Generic;

namespace Grand.Services.Ads
{
    public static class AdExtensions
    {
        /// <summary>
        /// Formats the order note text
        /// </summary>
        /// <param name="orderNote">Ad note</param>
        /// <returns>Formatted text</returns>
        public static string FormatAdNoteText(this AdNote orderNote)
        {
            if (orderNote == null)
                throw new ArgumentNullException("orderNote");

            string text = orderNote.Note;

            if (String.IsNullOrEmpty(text))
                return string.Empty;

            text = HtmlHelper.FormatText(text);

            return text;
        }

        
        /// <summary>
        /// Gets a total number of items in all shipments
        /// </summary>
        /// <param name="orderItem">Ad item</param>
        /// <returns>Total number of items in all shipmentss</returns>
        public static async Task<int> GetTotalNumberOfItemsInAllShipment(this AdItem orderItem, IAdService orderService, IShipmentService shipmentService)
        {
            if (orderItem == null)
                throw new ArgumentNullException("orderItem");

            var totalInShipments = 0;
            var order = await orderService.GetAdByAdItemId(orderItem.Id);
            //var shipments = await shipmentService.GetShipmentsByAd(order.Id);
            //for (int i = 0; i < shipments.Count; i++)
            //{
            //    var shipment = shipments[i];
            //    var si = shipment.ShipmentItems
            //        .FirstOrDefault(x => x.AdItemId == orderItem.Id);
            //    if (si != null)
            //    {
            //        totalInShipments += si.Quantity;
            //    }
            //}
            return totalInShipments;
        }

        /// <summary>
        /// Gets a total number of already items which can be added to new shipments
        /// </summary>
        /// <param name="orderItem">Ad item</param>
        /// <returns>Total number of already delivered items which can be added to new shipments</returns>
        public static async Task<int> GetTotalNumberOfItemsCanBeAddedToShipment(this AdItem orderItem, IAdService orderService, IShipmentService shipmentService)
        {
            if (orderItem == null)
                throw new ArgumentNullException("orderItem");

            var totalInShipments = await orderItem.GetTotalNumberOfItemsInAllShipment(orderService, shipmentService);

            var qtyAded = orderItem.Quantity;
            var qtyCanBeAddedToShipmentTotal = qtyAded - totalInShipments;
            if (qtyCanBeAddedToShipmentTotal < 0)
                qtyCanBeAddedToShipmentTotal = 0;

            return qtyCanBeAddedToShipmentTotal;
        }

        /// <summary>
        /// Gets a total number of not yet shipped items (but added to shipments)
        /// </summary>
        /// <param name="orderItem">Ad item</param>
        /// <returns>Total number of not yet shipped items (but added to shipments)</returns>
        public static async Task<int> GetTotalNumberOfNotYetShippedItems(this AdItem orderItem, IAdService orderService, IShipmentService shipmentService)
        {
            if (orderItem == null)
                throw new ArgumentNullException("orderItem");

            var order = await orderService.GetAdByAdItemId(orderItem.Id);

            var result = 0;
            //var shipments = await shipmentService.GetShipmentsByAd(order.Id);
            //for (int i = 0; i < shipments.Count; i++)
            //{
            //    var shipment = shipments[i];
            //    if (shipment.ShippedDateUtc.HasValue)
            //        //already shipped
            //        continue;

            //    var si = shipment.ShipmentItems
            //        .FirstOrDefault(x => x.AdItemId == orderItem.Id);
            //    if (si != null)
            //    {
            //        result += si.Quantity;
            //    }
            //}

            return result;
        }

        /// <summary>
        /// Gets a total number of already shipped items
        /// </summary>
        /// <param name="orderItem">Ad item</param>
        /// <returns>Total number of already shipped items</returns>
        public static async Task<int> GetTotalNumberOfShippedItems(this AdItem orderItem, Ad order, IShipmentService shipmentService)
        {
            if (orderItem == null)
                throw new ArgumentNullException("orderItem");

            var result = 0;
            //var shipments = await shipmentService.GetShipmentsByAd(order.Id);
            //for (int i = 0; i < shipments.Count; i++)
            //{
            //    var shipment = shipments[i];
            //    if (!shipment.ShippedDateUtc.HasValue)
            //        //not shipped yet
            //        continue;

            //    var si = shipment.ShipmentItems
            //        .FirstOrDefault(x => x.AdItemId == orderItem.Id);
            //    if (si != null)
            //    {
            //        result += si.Quantity;
            //    }
            //}
            
            return result;
        }

        /// <summary>
        /// Gets a total number of already delivered items
        /// </summary>
        /// <param name="orderItem">Ad  item</param>
        /// <returns>Total number of already delivered items</returns>
        public static async Task<int> GetTotalNumberOfDeliveredItems(this AdItem orderItem, Ad order, IShipmentService shipmentService)
        {
            if (orderItem == null)
                throw new ArgumentNullException("orderItem");

            var result = 0;
            //var shipments = await shipmentService.GetShipmentsByAd(order.Id);
            //for (int i = 0; i < shipments.Count; i++)
            //{
            //    var shipment = shipments[i];
            //    if (!shipment.DeliveryDateUtc.HasValue)
            //        //not delivered yet
            //        continue;

            //    var si = shipment.ShipmentItems
            //        .FirstOrDefault(x => x.AdItemId == orderItem.Id);
            //    if (si != null)
            //    {
            //        result += si.Quantity;
            //    }
            //}

            return result;
        }



        /// <summary>
        /// Gets a value indicating whether an order has items to be added to a shipment
        /// </summary>
        /// <param name="order">Ad</param>
        /// <returns>A value indicating whether an order has items to be added to a shipment</returns>
        public static async Task<bool> HasItemsToAddToShipment(this Ad order, IAdService orderService, IShipmentService shipmentService, IProductService productService)
        {
            if (order == null)
                throw new ArgumentNullException("order");

            foreach (var orderItem in order.AdItems)
            {
                //we can ship only shippable products
                var product = await productService.GetProductByIdIncludeArch(orderItem.ProductId);
                if (!product.IsShipEnabled)
                    continue;

                var totalNumberOfItemsCanBeAddedToShipment = await orderItem.GetTotalNumberOfItemsCanBeAddedToShipment(orderService, shipmentService);
                if (totalNumberOfItemsCanBeAddedToShipment <= 0)
                    continue;

                //yes, we have at least one item to create a new shipment
                return true;
            }
            return false;
        }
        /// <summary>
        /// Gets a value indicating whether an order has items to ship
        /// </summary>
        /// <param name="order">Ad</param>
        /// <returns>A value indicating whether an order has items to ship</returns>
        public static async Task<bool> HasItemsToShip(this Ad order, IAdService orderService, IShipmentService shipmentService, IProductService productService)
        {
            if (order == null)
                throw new ArgumentNullException("order");

            foreach (var orderItem in order.AdItems)
            {
                //we can ship only shippable products
                var product = await productService.GetProductByIdIncludeArch(orderItem.ProductId);
                if (!product.IsShipEnabled)
                    continue;

                var totalNumberOfNotYetShippedItems = await orderItem.GetTotalNumberOfNotYetShippedItems(orderService, shipmentService);
                if (totalNumberOfNotYetShippedItems <= 0)
                    continue;

                //yes, we have at least one item to ship
                return true;
            }
            return false;
        }
        /// <summary>
        /// Gets a value indicating whether an order has items to deliver
        /// </summary>
        /// <param name="order">Ad</param>
        /// <returns>A value indicating whether an order has items to deliver</returns>
        public static async Task<bool> HasItemsToDeliver(this Ad order, IShipmentService shipmentService, IProductService productService)
        {
            if (order == null)
                throw new ArgumentNullException("order");

            foreach (var orderItem in order.AdItems)
            {
                //we can ship only shippable products
                var product = await productService.GetProductByIdIncludeArch(orderItem.ProductId);
                if (!product.IsShipEnabled)
                    continue;

                var totalNumberOfShippedItems = await orderItem.GetTotalNumberOfShippedItems(order, shipmentService);
                var totalNumberOfDeliveredItems = await orderItem.GetTotalNumberOfDeliveredItems(order, shipmentService);
                if (totalNumberOfShippedItems <= totalNumberOfDeliveredItems)
                    continue;

                //yes, we have at least one item to deliver
                return true;
            }
            return false;
        }

        /// <summary>
        /// Indicates whether a order's tag exists
        /// </summary>
        /// <param name="order">Ad</param>
        /// <param name="orderTagId">Ad tag identifier</param>
        /// <returns>Result</returns>
        public static bool AdTagExists(this Ad order, AdTag orderTag)
        {
            if (order == null)
                throw new ArgumentNullException("order");

            bool result = order.AdTags.FirstOrDefault(t => t == orderTag.Id) != null;
            return result;
        }        
    }
}
