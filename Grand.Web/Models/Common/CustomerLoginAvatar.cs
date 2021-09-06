using Grand.Core.Models;

namespace Grand.Web.Models.Common
{
    public partial class CustomerLoginAvatarModel : BaseModel
    {
        public string CustomerAvatarUrl { get; set; }
        public string CustomerId { get; set; }
        public string FirstName { get; set; }
        public string LasttName { get; set; }
        public string FullName { get; set; }
    }
}
