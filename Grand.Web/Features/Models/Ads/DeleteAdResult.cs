namespace Grand.Web.Features.Models.Ads
{
    public class DeleteAdResult
    {
        public bool Deleted { get; private set; }

        public string ErrorMsg { get; private set; }

        public DeleteAdResult(bool delete, string errorMsg)
        {
            Deleted = delete;
            ErrorMsg = errorMsg;
        }
    }
}