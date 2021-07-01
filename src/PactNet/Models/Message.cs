
using Newtonsoft.Json;

namespace PactNet.Models
{
    /// <summary>
    /// An object representation of a messaging event
    /// </summary>  
    public class Message
    {
        /// <summary>
        /// The message metadata, specific to different messaging solutions
        /// </summary>
        [JsonProperty(Order = -1, PropertyName = "metaData")]
        public dynamic MetaData { get; set; }

        /// <summary>
        /// The message content
        /// </summary>
        [JsonProperty(Order = 0, PropertyName = "contents")]
        public dynamic Contents { get; set; }
    }
}
