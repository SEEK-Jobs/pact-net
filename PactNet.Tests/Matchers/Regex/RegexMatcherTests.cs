using Newtonsoft.Json;
using PactNet.Configuration.Json;
using PactNet.Matchers.Regex;
using Xunit;

namespace PactNet.Tests.Matchers.Regex
{
    public class RegexMatcherTests
    {
        private RegexMatcher GetSubject(string example, string regex)
        {
            return new RegexMatcher(example, regex);
        }

        [Fact]
        public void Ctor_WhenCalled_SerialisesCorrectly()
        {
            const string example = "hello@tester.com";
            const string regex = "/\\A([\\w+\\-].?)+@[a-z\\d\\-]+(\\.[a-z]+)*\\.[a-z]+\\z/i";

            var matcher = GetSubject(example, regex);

            var expected = new
            {
                json_class = "Pact::Term",
                data = new
                {
                    generate = example,
                    matcher = new
                    {
                        json_class = "Regexp",
                        o = 0,
                        s = regex
                    }
                }
            };
            var expectedJson = JsonConvert.SerializeObject(expected, JsonConfig.ApiSerializerSettings);
            var actualJson = JsonConvert.SerializeObject(matcher, JsonConfig.ApiSerializerSettings);

            Assert.Equal(expectedJson, actualJson);
        }
    }
}