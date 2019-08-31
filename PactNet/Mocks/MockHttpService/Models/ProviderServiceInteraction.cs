using Newtonsoft.Json;
using PactNet.Models;

namespace PactNet.Mocks.MockHttpService.Models
{
    internal class ProviderServiceInteraction : Interaction
    {
        [JsonProperty(PropertyName = "request", NullValueHandling = NullValueHandling.Ignore)]
        public ProviderServiceRequest Request { get; set; }

        [JsonProperty(PropertyName = "response", NullValueHandling = NullValueHandling.Ignore)]
        public ProviderServiceResponse Response { get; set; }
    }
}