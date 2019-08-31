using Newtonsoft.Json;

namespace PactNet.PactMessage.Models
{
    public class Message
    {
        [JsonProperty(PropertyName = "contents")]
        public dynamic Contents
        {
            get; set;
        }
    }
}
