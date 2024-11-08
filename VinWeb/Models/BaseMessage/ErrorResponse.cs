namespace Vin.Web.Models.BaseMessage
{
    public class ErrorResponse
    {
        public string Message { get; set; }
        public string[] Errors { get; set; }
    }
}
