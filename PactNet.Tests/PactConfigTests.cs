using Xunit;

namespace PactNet.Tests
{
    public class PactConfigTests
    {
        [Fact]
        public void Ctor_WithDefaults_UsesDefaultPactDir()
        {
            var options = new PactConfig();

            Assert.Equal(Constants.DefaultPactDir, options.PactDir);
        }

        [Fact]
        public void Ctor_WithDefaults_UsesDefaultLogDir()
        {
            var options = new PactConfig();

            Assert.Equal(Constants.DefaultLogDir, options.LogDir);
        }
    }
}
