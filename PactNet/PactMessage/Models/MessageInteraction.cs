using System.Collections.Generic;
using Newtonsoft.Json;
using PactNet.Models;

namespace PactNet.PactMessage.Models
{
    public class MessageInteraction : Interaction
    {
        [JsonProperty(PropertyName = "contents")]
        public dynamic Contents
        {
            get; set;
        }

        [JsonProperty(Order = -2, PropertyName = "providerStates")]
        public IEnumerable<ProviderState> ProviderStates
        {
            get; set;
        }
    }
}
