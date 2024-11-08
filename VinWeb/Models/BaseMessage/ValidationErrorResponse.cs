using Newtonsoft.Json;

namespace Vin.Web.Models.BaseMessage
{
    public class ValidationErrorResponse
    {
        [JsonProperty("errors")]
        public Dictionary<string, string[]> Errors { get; set; }
    }
}
