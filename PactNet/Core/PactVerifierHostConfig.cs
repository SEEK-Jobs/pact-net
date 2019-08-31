using System;
using System.Collections.Generic;
using PactNet.Infrastructure.Outputters;

namespace PactNet.Core
{
    internal class PactVerifierHostConfig : IPactCoreHostConfig
    {
        public string Script { get; }
        public string Arguments { get; }
        public bool WaitForExit { get; }
        public IEnumerable<IOutput> Outputters { get; }
        public IDictionary<string, string> Environment { get; }

        public PactVerifierHostConfig(Uri baseUri, string pactUri, PactUriOptions pactBrokerUriOptions, Uri providerStateSetupUri, PactVerifierConfig config, IDictionary<string, string> environment)
        {
            var providerStateOption = providerStateSetupUri != null ? $" --provider-states-setup-url {providerStateSetupUri.OriginalString}" : string.Empty;
            var brokerCredentials = pactBrokerUriOptions != null ?
                !String.IsNullOrEmpty(pactBrokerUriOptions.Username) && !String.IsNullOrEmpty(pactBrokerUriOptions.Password) ? 
                    $" --broker-username \"{pactBrokerUriOptions.Username}\" --broker-password \"{pactBrokerUriOptions.Password}\"" : 
                    $" --broker-token \"{pactBrokerUriOptions.Token}\""
                 : string.Empty;
            var publishResults = config?.PublishVerificationResults == true ? $" --publish-verification-results=true --provider-app-version=\"{config.ProviderVersion}\"" : string.Empty;
            var customHeader = config?.CustomHeader != null && !string.IsNullOrEmpty(config.CustomHeader?.Key) && !string.IsNullOrEmpty(config.CustomHeader?.Value) ?
                $" --custom-provider-header \"{config.CustomHeader?.Key}:{config.CustomHeader?.Value}\"" :
                string.Empty;
            var verbose = config?.Verbose == true ? " --verbose true" : string.Empty;
            var monkeyPatchOption = !string.IsNullOrEmpty(config?.MonkeyPatchFile) ? $" --monkeypatch=\"${config.MonkeyPatchFile}\"" : string.Empty;

            Script = "pact-provider-verifier";
            Arguments = $"\"{FixPathForRuby(pactUri)}\" --provider-base-url {baseUri.OriginalString}{providerStateOption}{brokerCredentials}{publishResults}{customHeader}{verbose}{monkeyPatchOption}";
            WaitForExit = true;
            Outputters = config?.Outputters;
            Environment = new Dictionary<string, string>
            {
                { "PACT_INTERACTION_RERUN_COMMAND", "To re-run just this failing interaction, change the verify method to '.Verify(description: \"<PACT_DESCRIPTION>\", providerState: \"<PACT_PROVIDER_STATE>\")'. Please do not check in this change!" }
            };

            if (environment != null)
            {
                foreach (var envVar in environment)
                {
                    Environment.Add(envVar.Key, envVar.Value);
                }
            }
        }

        private string FixPathForRuby(string path)
        {
            return path.Replace("\\", "/");
        }
    }
}