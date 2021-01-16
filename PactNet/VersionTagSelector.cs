using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace PactNet
{
    public class VersionTagSelector
    {
        private readonly JsonSerializerSettings _jsonSettings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
            Formatting = Formatting.None,
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };

        public string Tag { get; }
        public string Consumer { get; }
        public string FallbackTag { get; }
        public bool? All { get; }
        public bool? Latest { get; }

        public VersionTagSelector(string tag, string consumer = null, string fallbackTag = null, bool ? all = null, bool? latest = null)
        {
            Tag = tag;
            Consumer = consumer;
            FallbackTag = fallbackTag;
            All = all;
            Latest = latest;
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this, _jsonSettings).Replace("\"", "\\\"");
        }
    }
}
