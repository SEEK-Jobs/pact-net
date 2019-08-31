using Newtonsoft.Json;
using System;

namespace PactNet.Matchers.Type
{
    public class MinTypeMatcher : IMatcher
    {
        //Generate JSON using the Ruby spec for now

        [JsonProperty(PropertyName = "json_class")]
        public string Match { get; set; }

        [JsonProperty(PropertyName = "contents")]
        public dynamic Example { get; set; }

        [JsonProperty(PropertyName = "min")]
        public int Min { get; set; }

        public MinTypeMatcher(dynamic example, int min)
        {
            if (min < 1)
            {
                throw new ArgumentException("Min must be greater than 0");
            }

            Match = "Pact::ArrayLike";
            Example = example;
            Min = min;
        }
    }
}