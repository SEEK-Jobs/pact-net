using PactNet.Core;
using PactNet.Extensions;
using PactNet.Mocks.MockHttpService.Host;
using PactNet.Models;
using Xunit;

namespace PactNet.Tests.Core
{
    public class MockProviderHostConfigTests
    {
        private IPactCoreHostConfig GetSubject(int port = 2322, bool enableSsl = false,
            string consumerName = "My Test Consumer", string providerName = "My Test Provider",
            PactConfig pactConfig = null, IPAddress host = IPAddress.Loopback, string sslCert = null, string sslKey = null)
        {
            return new MockProviderHostConfig(port, enableSsl, consumerName, providerName,
                pactConfig ?? new PactConfig(), host, sslCert, sslKey);
        }

        [Fact]
        public void Ctor_WhenCalled_SetsTheCorrectScript()
        {
            var config = GetSubject();

            Assert.Equal("pact-mock-service", config.Script);
        }

        [Fact]
        public void Ctor_WhenCalled_SetsTheCorrectArgs()
        {
            var port = 9332;
            var pactConfig = new PactConfig();
            var consumerName = "Cons";
            var providerName = "The best one";

            var config = GetSubject(port, false, consumerName, providerName, pactConfig);

            var expectedLogFilePath = BuildExpectedLogFilePath(pactConfig.LogDir, providerName);
            var expectedPactDir = BuildExpectedPactDir(pactConfig.PactDir);
            var expectedArguments = BuildExpectedArguments(port, expectedLogFilePath, expectedPactDir,
                pactConfig.SpecificationVersion, consumerName, providerName);

            Assert.Equal(expectedArguments, config.Arguments);
        }

        [Fact]
        public void Ctor_WhenCalledWithSsl_SetsTheCorrectArgs()
        {
            var port = 9332;
            var pactConfig = new PactConfig();
            var consumerName = "Cons";
            var providerName = "The best one";
            var enableSsl = true;

            var config = GetSubject(port, enableSsl, consumerName, providerName, pactConfig);

            var expectedLogFilePath = BuildExpectedLogFilePath(pactConfig.LogDir, providerName);
            var expectedPactDir = BuildExpectedPactDir(pactConfig.PactDir);
            var expectedArguments = BuildExpectedArguments(port, expectedLogFilePath, expectedPactDir,
                pactConfig.SpecificationVersion, consumerName, providerName, enableSsl);

            Assert.Equal(expectedArguments, config.Arguments);
        }

        [Fact]
        public void Ctor_WhenCalledWithCustomSsl_SetsTheCorrectArgs()
        {
            var port = 9332;
            var pactConfig = new PactConfig();
            var consumerName = "Cons";
            var providerName = "The best one";
            var enableSsl = true;
            var sslCert = "..\\cert\\localhost.cer";
            var sslKey = "..\\cert\\localhost.key";

            var config = GetSubject(port, enableSsl, consumerName, providerName, pactConfig, IPAddress.Loopback, sslCert, sslKey);

            var expectedLogFilePath = BuildExpectedLogFilePath(pactConfig.LogDir, providerName);
            var expectedPactDir = BuildExpectedPactDir(pactConfig.PactDir);
            var expectedSslCert = BuildExpectedSslOption(sslCert);
            var expectedSslKey = BuildExpectedSslOption(sslKey);
            var expectedArguments = BuildExpectedArguments(port, expectedLogFilePath, expectedPactDir,
                pactConfig.SpecificationVersion, consumerName, providerName, enableSsl, IPAddress.Loopback, expectedSslCert, expectedSslKey);

            Assert.Equal(expectedArguments, config.Arguments);
        }

        [Fact]
        public void Ctor_WhenCalledWithHost_SetsTheCorrectArgs()
        {
            var port = 9332;
            var pactConfig = new PactConfig();
            var consumerName = "Cons";
            var providerName = "The best one";
            var enableSsl = true;
            var host = IPAddress.Any;

            var config = GetSubject(port, enableSsl, consumerName, providerName, pactConfig, host);

            var expectedLogFilePath = BuildExpectedLogFilePath(pactConfig.LogDir, providerName);
            var expectedPactDir = BuildExpectedPactDir(pactConfig.PactDir);
            var expectedArguments = BuildExpectedArguments(port, expectedLogFilePath, expectedPactDir,
                pactConfig.SpecificationVersion, consumerName, providerName, enableSsl, host);

            Assert.Equal(expectedArguments, config.Arguments);
        }

        [Fact]
        public void Ctor_WhenCalledWithNonDefaultLogDirectory_SetsTheCorrectArgs()
        {
            var port = 9332;
            var logDir = "./test";
            var pactConfig = new PactConfig { LogDir = logDir };
            var consumerName = "Cons";
            var providerName = "The best one";
            var enableSsl = true;

            var config = GetSubject(port, enableSsl, consumerName, providerName, pactConfig);

            var expectedLogFilePath = BuildExpectedLogFilePath(logDir + "\\", providerName);
            var expectedPactDir = BuildExpectedPactDir(pactConfig.PactDir);
            var expectedArguments = BuildExpectedArguments(port, expectedLogFilePath, expectedPactDir,
                pactConfig.SpecificationVersion, consumerName, providerName, enableSsl);

            Assert.Equal(expectedArguments, config.Arguments);
        }

        [Fact]
        public void Ctor_WhenCalledWithNonDefaultPactDirectory_SetsTheCorrectArgs()
        {
            var port = 9332;
            var pactDir = "./test";
            var pactConfig = new PactConfig { PactDir = pactDir };
            var consumerName = "Cons";
            var providerName = "The best one";
            var enableSsl = true;

            var config = GetSubject(port, enableSsl, consumerName, providerName, pactConfig);

            var expectedLogFilePath = BuildExpectedLogFilePath(pactConfig.LogDir, providerName);
            var expectedPactDir = BuildExpectedPactDir(pactDir + "\\");
            var expectedArguments = BuildExpectedArguments(port, expectedLogFilePath, expectedPactDir,
                pactConfig.SpecificationVersion, consumerName, providerName, enableSsl);

            Assert.Equal(expectedArguments, config.Arguments);
        }

        [Fact]
        public void Ctor_WhenCalled_SetsWaitForExitToFalse()
        {
            var config = GetSubject();

            Assert.Equal(false, config.WaitForExit);
        }

        private string BuildExpectedLogFilePath(string logDir, string providerName)
        {
            return ($"{logDir}{providerName.ToLowerSnakeCase()}_mock_service.log").Replace("\\", "/");
        }

        private string BuildExpectedPactDir(string pactDir)
        {
            return pactDir.Replace("\\", "/");
        }

        private string BuildExpectedSslOption(string fullName)
        {
            return $"{fullName}".Replace("\\", "/");
        }

        private string BuildExpectedArguments(
            int port,
            string logFilePath,
            string pactFileDir,
            string pactSpecificationVersion,
            string consumerName,
            string providerName,
            bool enableSsl = false,
            IPAddress host = IPAddress.Loopback,
            string sslCert = "",
            string sslKey = ""
        )
        {
            var sslOption = enableSsl ? " --ssl" : "";
            var hostOption = host == IPAddress.Any ? " --host=0.0.0.0" : "";
            var sslCertOption = !string.IsNullOrEmpty(sslCert) ? $" --sslcert=\"{sslCert}\"" : string.Empty;
            var sslKeyOption = !string.IsNullOrEmpty(sslKey) ? $" --sslkey=\"{sslKey}\"" : string.Empty;

            return
                $"-p {port} -l \"{logFilePath}\" " +
                $"--pact-dir \"{pactFileDir}\" " +
                $"--pact-specification-version \"{pactSpecificationVersion}\" " +
                $"--consumer \"{consumerName}\" " +
                $"--provider \"{providerName}\"{sslOption}{hostOption}{sslCertOption}{sslKeyOption}";
        }
    }
}