using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        public PactVerifierHostConfig(Uri baseUri, string pactUri, PactBrokerConfig brokerConfig, PactUriOptions pactBrokerUriOptions, Uri providerStateSetupUri, PactVerifierConfig config, IDictionary<string, string> environment)
        {
            var pactUriOption = pactUri != null ? $"\"{FixPathForRuby(pactUri)}\" " : "";
            var providerStateOption = providerStateSetupUri != null ? $" --provider-states-setup-url \"{providerStateSetupUri.OriginalString}\"" : string.Empty;
            var pactBrokerOptions = BuildPactBrokerOptions(brokerConfig);
            var brokerCredentials = string.Empty;
            
            if (!string.IsNullOrEmpty(pactBrokerUriOptions?.Username) && !string.IsNullOrEmpty(pactBrokerUriOptions?.Password))
            {
                brokerCredentials = $" --broker-username \"{pactBrokerUriOptions.Username}\" --broker-password \"{pactBrokerUriOptions.Password}\"";
            }
            else if (!string.IsNullOrEmpty(pactBrokerUriOptions?.Token))
            {
                brokerCredentials = $" --broker-token \"{pactBrokerUriOptions.Token}\"";
            }

            var publishResults = config?.PublishVerificationResults == true ? $" --publish-verification-results=true --provider-app-version=\"{config.ProviderVersion}\"" : string.Empty;
            var customHeaders = this.BuildCustomHeaders(config);
            var verbose = config?.Verbose == true ? " --verbose true" : string.Empty;
            var monkeyPatchOption = !string.IsNullOrEmpty(config?.MonkeyPatchFile) ? $" --monkeypatch=\"${config.MonkeyPatchFile}\"" : string.Empty;

            Script = "pact-provider-verifier";
            Arguments = $"{pactUriOption}--provider-base-url \"{baseUri.OriginalString}\"{providerStateOption}{pactBrokerOptions}{brokerCredentials}{publishResults}{customHeaders}{verbose}{monkeyPatchOption}";
            WaitForExit = true;
            Outputters = config?.Outputters;
            Environment = new Dictionary<string, string>
            {
                { "PACT_INTERACTION_RERUN_COMMAND", "To re-run just this failing interaction, change the verify method to '.Verify(description: \"<PACT_DESCRIPTION>\", providerState: \"<PACT_PROVIDER_STATE>\")'. Please do not check in this change!" }
            };

            if (!string.IsNullOrEmpty(pactBrokerUriOptions?.SslCaFilePath))
            {
                Environment.Add("SSL_CERT_FILE", pactBrokerUriOptions.SslCaFilePath);
            }

            if (!string.IsNullOrEmpty(pactBrokerUriOptions?.HttpProxy))
            {
                Environment.Add("HTTP_PROXY", pactBrokerUriOptions.HttpProxy);
            }

            if (!string.IsNullOrEmpty(pactBrokerUriOptions?.HttpsProxy))
            {
                Environment.Add("HTTPS_PROXY", pactBrokerUriOptions.HttpsProxy);
            }

            if (environment != null)
            {
                foreach (var envVar in environment)
                {
                    Environment.Add(envVar.Key, envVar.Value);
                }
            }
        }

        private string BuildCustomHeaders(PactVerifierConfig config)
        {
            if (config?.CustomHeaders == null)
            {
                return string.Empty;
            }

            var builder = new StringBuilder();
            foreach (var header in config.CustomHeaders.Where(kv => !string.IsNullOrEmpty(kv.Key) && !string.IsNullOrEmpty(kv.Value)))
            {
                builder.Append($" --custom-provider-header \"{header.Key}:{header.Value}\"");
            }

            return builder.ToString();
        }

        private string BuildPactBrokerOptions(PactBrokerConfig config)
        {
            if (config == null)
            {
                return string.Empty;
            }

            var consumerVersionTags = BuildTags("consumer-version-tag", config.ConsumerVersionTags);
            var providerVersionTags = BuildTags("provider-version-tag", config.ProviderVersionTags);
            var consumerVersionSelectors = BuildTags("consumer-version-selector", config.ConsumerVersionSelectors);
            var enablePending = config.EnablePending ? " --enable-pending" : "";
            var includeWipPactsSince = !string.IsNullOrEmpty(config.IncludeWipPactsSince) ? $" --include-wip-pacts-since \"{config.IncludeWipPactsSince}\"" : string.Empty;

            return $" --pact-broker-base-url \"{config.BrokerBaseUri}\" --provider \"{config.ProviderName}\"{consumerVersionTags}{providerVersionTags}{consumerVersionSelectors}{enablePending}{includeWipPactsSince}";
        }

        private string BuildTags<T>(string tagOption, IEnumerable<T> tags)
        {
            if (string.IsNullOrEmpty(tagOption) || tags == null || !tags.Any())
            {
                return string.Empty;
            }

            var builder = new StringBuilder();
            foreach (var tag in tags)
            {
                var tagVal = tag?.ToString();
                if (!string.IsNullOrEmpty(tagVal))
                {
                    builder.Append($" --{tagOption} \"{tagVal}\"");
                }
            }

            return builder.ToString();
        }

        private string FixPathForRuby(string path)
        {
            return path.Replace("\\", "/");
        }
    }
}