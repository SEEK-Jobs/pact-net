using Newtonsoft.Json;
using PactNet.Configuration.Json;
using PactNet.Matchers.Type;
using System;
using Xunit;

namespace PactNet.Tests.Matchers.Type
{
    public class MinTypeMatcherTests
    {
        private MinTypeMatcher GetSubject(dynamic example, int min)
        {
            return new MinTypeMatcher(example, min);
        }

        [Fact]
        public void Ctor_WhenCalled_SerialisesCorrectly()
        {
            var example = new[] { 22, 23, 56 };
            const int min = 2;

            var matcher = GetSubject(example, min);

            var expected = new
            {
                json_class = "Pact::ArrayLike",
                contents = example,
                min = min
            };
            var expectedJson = JsonConvert.SerializeObject(expected, JsonConfig.ApiSerializerSettings);
            var actualJson = JsonConvert.SerializeObject(matcher, JsonConfig.ApiSerializerSettings);

            Assert.Equal(expectedJson, actualJson);
        }

        [Fact]
        public void Ctor_WhenCalledWithMin0_ThrowsArgumentException()
        {
            var example = new[] { 22, 23, 56 };
            const int min = 0;

            Assert.Throws<ArgumentException>(() => GetSubject(example, min));
        }
    }
}