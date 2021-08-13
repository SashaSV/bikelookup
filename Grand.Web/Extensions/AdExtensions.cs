using Grand.Domain.Ads;
using Grand.Domain.Customers;
using Grand.Domain.Orders;

namespace Grand.Web.Extensions
{
    public static class AdExtensions
    {
        public static bool Access(this Ad ad, Customer customer)
        {
            if (ad == null || ad.Deleted)
                return false;

            //owner
            if (customer.IsOwner() && (customer.Id == ad.CustomerId || customer.Id == ad.OwnerId))
                return true;

            //subaccount
            if (!customer.IsOwner() && customer.Id == ad.CustomerId)
                return true;

            return false;
        }
        public static bool Access(this ReturnRequest returnRequest, Customer customer)
        {
            if (returnRequest == null)
                return false;

            //owner
            if (customer.IsOwner() && (customer.Id == returnRequest.CustomerId || customer.Id == returnRequest.OwnerId))
                return true;

            //subaccount
            if (!customer.IsOwner() && customer.Id == returnRequest.CustomerId)
                return true;

            return false;
        }
    }
}
