using System.Collections.Generic;
using PactNet.Core;
using PactNet.Extensions;
using PactNet.Infrastructure.Outputters;
using PactNet.Models;

namespace PactNet.Mocks.MockHttpService.Host
{
    internal class MockProviderHostConfig : IPactCoreHostConfig
    {
        public string Script { get; }
        public string Arguments { get; }
        public bool WaitForExit { get; }
        public IEnumerable<IOutput> Outputters { get; }
        public IDictionary<string, string> Environment { get; }

        public MockProviderHostConfig(int port, bool enableSsl, string consumerName, string providerName,
            PactConfig config, IPAddress host, string sslCert, string sslKey)
        {
            var logFile = $"{config.LogDir}{providerName.ToLowerSnakeCase()}_mock_service.log";
            var sslOption = enableSsl ? " --ssl" : string.Empty;
            var sslCertOption = !string.IsNullOrEmpty(sslCert)
                ? $" --sslcert=\"{FixPathForRuby(sslCert)}\""
                : string.Empty;
            var sslKeyOption = !string.IsNullOrEmpty(sslKey)
                ? $" --sslkey=\"{FixPathForRuby(sslKey)}\""
                : string.Empty;
            var hostOption = host == IPAddress.Any ? " --host=0.0.0.0" : string.Empty;
            var monkeyPatchOption = !string.IsNullOrEmpty(config?.MonkeyPatchFile)
                ? $" --monkeypatch=\"${config.MonkeyPatchFile}\""
                : string.Empty;

            Script = "pact-mock-service";
            Arguments = $"-p {port} -l \"{FixPathForRuby(logFile)}\" --pact-dir \"{FixPathForRuby(config.PactDir)}\" --pact-specification-version \"{config.SpecificationVersion}\" --consumer \"{consumerName}\" --provider \"{providerName}\"{sslOption}{hostOption}{monkeyPatchOption}";
            Arguments = $"-p {port} -l \"{FixPathForRuby(logFile)}\" " +
                        $"--pact-dir \"{FixPathForRuby(config.PactDir)}\" " +
                        $"--pact-specification-version \"{config.SpecificationVersion}\" " +
                        $"--consumer \"{consumerName}\" " +
                        $"--provider \"{providerName}\"{sslOption}{hostOption}{sslCertOption}{sslKeyOption}{monkeyPatchOption}";

            WaitForExit = false;
            Outputters = config?.Outputters;
        }

        private string FixPathForRuby(string path)
        {
            return path.Replace("\\", "/");
        }
    }
}