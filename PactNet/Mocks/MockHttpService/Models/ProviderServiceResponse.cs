using System.Collections.Generic;
using Newtonsoft.Json;
using PactNet.Configuration.Json.Converters;
using PactNet.Mocks.Models;
using PactNet.Models;

namespace PactNet.Mocks.MockHttpService.Models
{
    public class ProviderServiceResponse : IMessage
    {
        private bool _bodyWasSet;
        private dynamic _body;

        [JsonProperty(PropertyName = "status", NullValueHandling = NullValueHandling.Ignore)]
        public int Status { get; set; }

        [JsonProperty(PropertyName = "headers", NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(PreserveCasingDictionaryConverter))]
        public IDictionary<string, object> Headers { get; set; }

        [JsonProperty(PropertyName = "body")]
        public dynamic Body
        {
            get { return _body; }
            set
            {
                _bodyWasSet = true;
                _body = value;
            }
        }

        // A not so well known feature in JSON.Net to do conditional serialization at runtime
        public bool ShouldSerializeBody()
        {
            return _bodyWasSet;
        }
    }
}