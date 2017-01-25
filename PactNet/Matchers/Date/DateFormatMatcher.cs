using System;
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace PactNet.Matchers.Date
{
    public class DateFormatMatcher : IMatcher
    {
        [JsonIgnore]
        public string Type
        {
            get { return DateFormatMatchDefinition.Name; }
        }

        [JsonProperty("date")]
        public string DateFormat { get; protected set; }
        
        public DateFormatMatcher(string dateFormat)
        {
            DateFormat = dateFormat;
        }

        public MatcherResult Match(string path, JToken expected, JToken actual)
        {
            var act = actual as JValue;

            DateTime dateTime;
            var matches = act != null &&
                          DateTime.TryParseExact(act.Value.ToString(), this.DateFormat, CultureInfo.InvariantCulture,
                              DateTimeStyles.None, out dateTime);

            return matches ?
                new MatcherResult(new SuccessfulMatcherCheck(path)) :
                new MatcherResult(new FailedMatcherCheck(path, MatcherCheckFailureType.ValueDoesNotMatchDateFormat));
        }
    }
}