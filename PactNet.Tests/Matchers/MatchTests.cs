using PactNet.Matchers;
using PactNet.Matchers.Regex;
using PactNet.Matchers.Type;
using Xunit;

namespace PactNet.Tests.Matchers
{
    public class MatchTests
    {
        [Fact]
        public void Regex_WhenCalled_ReturnsARegexMatcher()
        {
            const string example = "hello@tester.com";
            const string regex = "/\\A([\\w+\\-].?)+@[a-z\\d\\-]+(\\.[a-z]+)*\\.[a-z]+\\z/i";

            var matcher = Match.Regex(example, regex);

            Assert.IsType<RegexMatcher>(matcher);
        }

        [Fact]
        public void Type_WhenCalled_ReturnsATypeMatcher()
        {
            const int example = 22;

            var matcher = Match.Type(example);

            Assert.IsType<TypeMatcher>(matcher);
        }

        [Fact]
        public void MinType_WhenCalled_ReturnsATypeMatcher()
        {
            var example = new[] { 22, 23, 56 };

            var matcher = Match.MinType(example, 2);

            Assert.IsType<MinTypeMatcher>(matcher);
        }

        [Fact]
        public void ComposingTwoTypeMatchers_WhenCalled_ReturnsAllMatchers()
        {
            const int example = 22;

            var matcher = Match.Type(Match.Type(example));

            Assert.IsType<TypeMatcher>(matcher);
            Assert.IsType<TypeMatcher>(matcher.Example);
        }

        [Fact]
        public void ComposingATypeMatcherAndARegexMatcher_WhenCalled_ReturnsAllMatchers()
        {
            const string example = "hello@tester.com";
            const string regex = "/\\A([\\w+\\-].?)+@[a-z\\d\\-]+(\\.[a-z]+)*\\.[a-z]+\\z/i";

            var matcher = Match.Type(Match.Regex(example, regex));

            Assert.IsType<TypeMatcher>(matcher);
            Assert.IsType<RegexMatcher>(matcher.Example);
        }

        [Fact]
        public void ComposingAMinTypeMatcherAndATypeMatcher_WhenCalled_ReturnsAllMatchers()
        {
            var example = new[] { Match.Type(22), Match.Type(23), Match.Type(56) };

            var matcher = Match.MinType(example, 2);

            Assert.IsType<MinTypeMatcher>(matcher);
            var actualExample = matcher.Example;
            Assert.IsType<TypeMatcher>(actualExample[0]);
            Assert.IsType<TypeMatcher>(actualExample[1]);
            Assert.IsType<TypeMatcher>(actualExample[2]);
        }
    }
}
