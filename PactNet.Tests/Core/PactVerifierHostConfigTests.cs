using System;
using System.Collections.Generic;
using PactNet.Core;
using Xunit;

namespace PactNet.Tests.Core
{
    public class PactVerifierHostConfigTests
    {
        private IPactCoreHostConfig GetSubject(Uri baseUri = null, 
            string pactUri = null, 
            PactBrokerConfig brokerConfig = null, 
            PactUriOptions pactBrokerUriOptions = null, 
            Uri providerStateSetupUri = null, 
            PactVerifierConfig verifierConfig = null, 
            IDictionary<string, string> environment = null)
        {
            return new PactVerifierHostConfig(
                baseUri ?? new Uri("http://localhost:2833"), 
                pactUri,
                brokerConfig,
                pactBrokerUriOptions,
                providerStateSetupUri,
                verifierConfig,
                environment);
        }

        [Fact]
        public void Ctor_WhenCalled_SetsTheCorrectScript()
        {
            var config = GetSubject(pactUri: "../test/pact.json");

            Assert.Equal("pact-provider-verifier", config.Script);
        }

        [Fact]
        public void Ctor_WhenCalled_SetsTheCorrectArgs()
        {
            var config = GetSubject(baseUri: new Uri("http://127.0.0.1"), 
                pactUri: "./tester-pact/pact-file.json", 
                providerStateSetupUri: new Uri("http://127.0.0.1/states/"));

            var expectedArguments = "\"./tester-pact/pact-file.json\" --provider-base-url \"http://127.0.0.1\" --provider-states-setup-url \"http://127.0.0.1/states/\"";
            Assert.Equal(expectedArguments, config.Arguments);
        }

        [Fact]
        public void Ctor_WhenCalledWithAHttpPactUri_SetsTheCorrectArgs()
        {
            var config = GetSubject(baseUri: new Uri("http://127.0.0.1"), 
                pactUri: "http://broker:9292/test", 
                providerStateSetupUri: new Uri("http://127.0.0.1/states/"));

            var expectedArguments = "\"http://broker:9292/test\" --provider-base-url \"http://127.0.0.1\" --provider-states-setup-url \"http://127.0.0.1/states/\"";
            Assert.Equal(expectedArguments, config.Arguments);
        }

        [Fact]
        public void Ctor_WhenCalledWithAnEmptyPactUriOptions_SetsTheCorrectArgs()
        {
            var config = GetSubject(baseUri: new Uri("http://127.0.0.1"),
                pactUri: "http://broker:9292/test",
                pactBrokerUriOptions: new PactUriOptions(),
                providerStateSetupUri: new Uri("http://127.0.0.1/states/"));

            var expectedArguments = "\"http://broker:9292/test\" --provider-base-url \"http://127.0.0.1\" --provider-states-setup-url \"http://127.0.0.1/states/\"";
            Assert.Equal(expectedArguments, config.Arguments);
        }

        [Fact]
        public void Ctor_WhenCalledWithABasicAuthenticatedHttpsPactUri_SetsTheCorrectArgs()
        {
            var config = GetSubject(baseUri: new Uri("http://127.0.0.1"), 
                pactUri: "https://broker:9292/test",
                pactBrokerUriOptions: new PactUriOptions().SetBasicAuthentication("username", "password"), 
                providerStateSetupUri: new Uri("http://127.0.0.1/states/"));

            var expectedArguments = "\"https://broker:9292/test\" --provider-base-url \"http://127.0.0.1\" --provider-states-setup-url \"http://127.0.0.1/states/\" --broker-username \"username\" --broker-password \"password\"";
            Assert.Equal(expectedArguments, config.Arguments);
        }

        [Fact]
        public void Ctor_WhenCalledWithABearerAuthenticatedHttpsPactUri_SetsTheCorrectArgs()
        {
            var config = GetSubject(baseUri: new Uri("http://127.0.0.1"),
                pactUri: "https://broker:9292/test",
                pactBrokerUriOptions: new PactUriOptions().SetBearerAuthentication("token"), 
                providerStateSetupUri: new Uri("http://127.0.0.1/states/"));

            var expectedArguments = "\"https://broker:9292/test\" --provider-base-url \"http://127.0.0.1\" --provider-states-setup-url \"http://127.0.0.1/states/\" --broker-token \"token\"";
            Assert.Equal(expectedArguments, config.Arguments);
        }

        [Fact]
        public void Ctor_WhenCalledWithSslCaFilePathHttpsPactUri_AddSslCertFileEnvironmentVariable()
        {
            var config = GetSubject(baseUri: new Uri("http://127.0.0.1"),
                pactUri: "https://broker:9292/test",
                pactBrokerUriOptions: new PactUriOptions().SetSslCaFilePath("C:/path/to/some/ca-file.crt"),
                providerStateSetupUri: new Uri("http://127.0.0.1/states/"));

            var environment = new Dictionary<string, string>
            {
                { "SSL_CERT_FILE", "C:/path/to/some/ca-file.crt" }
            };
            
            AssertEnvironmentIsCorrectlySet(environment, config.Environment);
        }

        [Fact]
        public void Ctor_WhenCalledWithHttpProxyHttpsPactUri_AddHttpProxyEnvironmentVariable()
        {
            var config = GetSubject(baseUri: new Uri("http://127.0.0.1"),
                pactUri: "https://broker:9292/test",
                pactBrokerUriOptions: new PactUriOptions().SetHttpProxy("http://my-http-proxy"),
                providerStateSetupUri: new Uri("http://127.0.0.1/states/"));

            var environment = new Dictionary<string, string>
            {
                { "HTTP_PROXY", "http://my-http-proxy" },
                { "HTTPS_PROXY", "http://my-http-proxy" },
            };

            AssertEnvironmentIsCorrectlySet(environment, config.Environment);
        }

        [Fact]
        public void Ctor_WhenCalledWithHttpAndHttpsProxyHttpsPactUri_AddHttpProxyEnvironmentVariable()
        {
            var config = GetSubject(baseUri: new Uri("http://127.0.0.1"),
                pactUri: "https://broker:9292/test",
                pactBrokerUriOptions: new PactUriOptions().SetHttpProxy("http://my-http-proxy", "http://my-https-proxy"),
                providerStateSetupUri: new Uri("http://127.0.0.1/states/"));

            var environment = new Dictionary<string, string>
            {
                { "HTTP_PROXY", "http://my-http-proxy" },
                { "HTTPS_PROXY", "http://my-https-proxy" },
            };

            AssertEnvironmentIsCorrectlySet(environment, config.Environment);
        }

        [Fact]
        public void Ctor_WhenCalledWithABasicAuthenticatedBrokerConfig_SetsTheCorrectArgs()
        {
            var config = GetSubject(baseUri: new Uri("http://127.0.0.1"), 
                brokerConfig: new PactBrokerConfig("Provider Name", "https://broker:9292/test", false, null, null, null, null),
                pactBrokerUriOptions: new PactUriOptions().SetBasicAuthentication("username", "password"), 
                providerStateSetupUri: new Uri("http://127.0.0.1/states/"));

            var expectedArguments = "--provider-base-url \"http://127.0.0.1\" --provider-states-setup-url \"http://127.0.0.1/states/\" --pact-broker-base-url \"https://broker:9292/test\" --provider \"Provider Name\" --broker-username \"username\" --broker-password \"password\"";
            Assert.Equal(expectedArguments, config.Arguments);
        }

        [Fact]
        public void Ctor_WhenCalledWithATokenAuthenticatedBrokerConfig_SetsTheCorrectArgs()
        {
            var config = GetSubject(baseUri: new Uri("http://127.0.0.1"), 
                brokerConfig: new PactBrokerConfig("Provider Name", "https://broker:9292/test", false, null, null, null, null),
                pactBrokerUriOptions: new PactUriOptions().SetBearerAuthentication("token"), 
                providerStateSetupUri: new Uri("http://127.0.0.1/states/"));

            var expectedArguments = "--provider-base-url \"http://127.0.0.1\" --provider-states-setup-url \"http://127.0.0.1/states/\" --pact-broker-base-url \"https://broker:9292/test\" --provider \"Provider Name\" --broker-token \"token\"";
            Assert.Equal(expectedArguments, config.Arguments);
        }

        [Fact]
        public void Ctor_WhenCalledWithTagsInTheBrokerConfig_SetsTheCorrectArgs()
        {
            var config = GetSubject(baseUri: new Uri("http://127.0.0.1"), 
                brokerConfig: new PactBrokerConfig("Provider Name", "https://broker:9292/test", false, new List<string> { "ctag1", "ctag2" }, new List<string> { "ptag1", "ptag2" }, null, null),
                pactBrokerUriOptions: new PactUriOptions().SetBearerAuthentication("token"), 
                providerStateSetupUri: new Uri("http://127.0.0.1/states/"));

            var expectedArguments = "--provider-base-url \"http://127.0.0.1\" --provider-states-setup-url \"http://127.0.0.1/states/\" --pact-broker-base-url \"https://broker:9292/test\" --provider \"Provider Name\" --consumer-version-tag \"ctag1\" --consumer-version-tag \"ctag2\" --provider-version-tag \"ptag1\" --provider-version-tag \"ptag2\" --broker-token \"token\"";
            Assert.Equal(expectedArguments, config.Arguments);
        }
 
        [Fact]
        public void Ctor_WhenCalledWithEnablePendingAndConsumerTagSelectorsInTheBrokerConfig_SetsTheCorrectArgs()
        {
            var config = GetSubject(baseUri: new Uri("http://127.0.0.1"), 
                brokerConfig: new PactBrokerConfig("Provider Name", "https://broker:9292/test", true, null, null, new List<VersionTagSelector>
                {
                    new VersionTagSelector("ctag1", all: true),
                    new VersionTagSelector("ctag2", latest: true),
                    new VersionTagSelector("ctag3", consumer: "Consumer Name", fallbackTag: "master", latest: true),
                    new VersionTagSelector("ctag4", consumer: "Consumer Name", fallbackTag: "master", all: true)
                }, null),
                pactBrokerUriOptions: new PactUriOptions().SetBearerAuthentication("token"), 
                providerStateSetupUri: new Uri("http://127.0.0.1/states/"));

            var expectedArguments = "--provider-base-url \"http://127.0.0.1\" --provider-states-setup-url \"http://127.0.0.1/states/\" --pact-broker-base-url \"https://broker:9292/test\" --provider \"Provider Name\" --consumer-version-selector \"{\\\"tag\\\":\\\"ctag1\\\",\\\"all\\\":true}\" --consumer-version-selector \"{\\\"tag\\\":\\\"ctag2\\\",\\\"latest\\\":true}\" --consumer-version-selector \"{\\\"tag\\\":\\\"ctag3\\\",\\\"consumer\\\":\\\"Consumer Name\\\",\\\"fallbackTag\\\":\\\"master\\\",\\\"latest\\\":true}\" --consumer-version-selector \"{\\\"tag\\\":\\\"ctag4\\\",\\\"consumer\\\":\\\"Consumer Name\\\",\\\"fallbackTag\\\":\\\"master\\\",\\\"all\\\":true}\" --enable-pending --broker-token \"token\"";
            Assert.Equal(expectedArguments, config.Arguments);
        }

        [Fact]
        public void Ctor_WhenCalledWithIncludeWipPactsSinceInTheBrokerConfig_SetsTheCorrectArgs()
        {
            var config = GetSubject(baseUri: new Uri("http://127.0.0.1"),
                brokerConfig: new PactBrokerConfig("Provider Name", "https://broker:9292/test", false, null, null, null, "2020-06-22"),
                pactBrokerUriOptions: new PactUriOptions().SetBearerAuthentication("token"),
                providerStateSetupUri: new Uri("http://127.0.0.1/states/"));

            var expectedArguments = "--provider-base-url \"http://127.0.0.1\" --provider-states-setup-url \"http://127.0.0.1/states/\" --pact-broker-base-url \"https://broker:9292/test\" --provider \"Provider Name\" --include-wip-pacts-since \"2020-06-22\" --broker-token \"token\"";
            Assert.Equal(expectedArguments, config.Arguments);
        }

        [Fact]
        public void Ctor_WhenCalledWithNoProviderStateSetupUri_SetsTheCorrectArgs()
        {
            var config = GetSubject(new Uri("http://127.0.0.1"), "./tester-pact/pact-file.json");

            var expectedArguments = "\"./tester-pact/pact-file.json\" --provider-base-url \"http://127.0.0.1\"";
            Assert.Equal(expectedArguments, config.Arguments);
        }

        [Fact]
        public void Ctor_WhenCalled_SetsWaitForExitToTrue()
        {
            var config = GetSubject();
            Assert.True(config.WaitForExit);
        }

        [Fact]
        public void Ctor_WhenCalled_SetsOutputters()
        {
            var verifierConfig = new PactVerifierConfig();
            var config = GetSubject(verifierConfig: verifierConfig);
            Assert.Equal(verifierConfig.Outputters, config.Outputters);
        }

        [Fact]
        public void Ctor_WhenCalledWithPublishVerificationResults_SetsTheCorrectArgs()
        {
            var config = GetSubject(baseUri: new Uri("http://127.0.0.1"), 
                pactUri: "./tester-pact/pact-file.json", 
                providerStateSetupUri: new Uri("http://127.0.0.1/states/"), 
                verifierConfig: new PactVerifierConfig
                {
                    PublishVerificationResults = true,
                    ProviderVersion = "1.0.0"
                });

            var expectedArguments = "\"./tester-pact/pact-file.json\" --provider-base-url \"http://127.0.0.1\" --provider-states-setup-url \"http://127.0.0.1/states/\" --publish-verification-results=true --provider-app-version=\"1.0.0\"";
            Assert.Equal(expectedArguments, config.Arguments);
        }

        [Fact]
        [Obsolete]
        public void Ctor_WhenCalledWithCustomHeader_SetsTheCorrectArgs()
        {
            var config = GetSubject(baseUri: new Uri("http://127.0.0.1"), 
                pactUri: "./tester-pact/pact-file.json", 
                providerStateSetupUri: new Uri("http://127.0.0.1/states/"), 
                verifierConfig: new PactVerifierConfig
                {
#pragma warning disable CS0618 // Type or member is obsolete
                    CustomHeader = new KeyValuePair<string, string>("Authorization", "Basic VGVzdA=="),
#pragma warning restore CS0618 // Type or member is obsolete
                    ProviderVersion = "1.0.0"
                });

            var expectedArguments = "\"./tester-pact/pact-file.json\" --provider-base-url \"http://127.0.0.1\" --provider-states-setup-url \"http://127.0.0.1/states/\" --custom-provider-header \"Authorization:Basic VGVzdA==\"";
            Assert.Equal(expectedArguments, config.Arguments);
        }

        [Fact]
        public void Ctor_WhenCalledWithCustomHeaders_SetsTheCorrectArgs()
        {
            var config = GetSubject(baseUri: new Uri("http://127.0.0.1"), 
                pactUri: "./tester-pact/pact-file.json", 
                providerStateSetupUri: new Uri("http://127.0.0.1/states/"), 
                verifierConfig: new PactVerifierConfig
                {
                    CustomHeaders = new Dictionary<string, string>
                    {
                        { "Authorization", "Basic VGVzdA==" },
                        { "X-Something", "MYthing" }
                    },
                    ProviderVersion = "1.0.0"
                });

            var expectedArguments = "\"./tester-pact/pact-file.json\" --provider-base-url \"http://127.0.0.1\" --provider-states-setup-url \"http://127.0.0.1/states/\" --custom-provider-header \"Authorization:Basic VGVzdA==\" --custom-provider-header \"X-Something:MYthing\"";
            Assert.Equal(expectedArguments, config.Arguments);
        }

        [Fact]
        public void Ctor_WhenCalledWithVerboseTrue_SetsTheCorrectArgs()
        {
            var config = GetSubject(baseUri: new Uri("http://127.0.0.1"), 
                pactUri: "./tester-pact/pact-file.json", 
                providerStateSetupUri: new Uri("http://127.0.0.1/states/"), 
                verifierConfig: new PactVerifierConfig
                {
                    Verbose = true,
                    ProviderVersion = "1.0.0"
                });

            var expectedArguments = "\"./tester-pact/pact-file.json\" --provider-base-url \"http://127.0.0.1\" --provider-states-setup-url \"http://127.0.0.1/states/\" --verbose true";
            Assert.Equal(expectedArguments, config.Arguments);
        }

        [Fact]
        public void Ctor_WhenCalledWithNoEnvironment_NoAdditionalEnvironmentVariablesAreAdded()
        {
            var baseUri = new Uri("http://127.0.0.1");
            var pactUri = "./tester-pact/pact-file.json";
            var providerStateSetupUri = new Uri("http://127.0.0.1/states/");

            var verifierConfig = new PactVerifierConfig
            {
                ProviderVersion = "1.0.0"
            };

            var config = GetSubject(baseUri: baseUri, pactUri: pactUri, providerStateSetupUri: providerStateSetupUri, verifierConfig: verifierConfig);

            AssertEnvironmentIsCorrectlySet(null, config.Environment);
        }

        [Fact]
        public void Ctor_WhenCalledWithEnvironmentSet_AdditionalEnvironmentVariablesAreAdded()
        {
            var baseUri = new Uri("http://127.0.0.1");
            var pactUri = "./tester-pact/pact-file.json";
            var providerStateSetupUri = new Uri("http://127.0.0.1/states/");

            var verifierConfig = new PactVerifierConfig
            {
                ProviderVersion = "1.0.0"
            };

            var environment = new Dictionary<string, string>
            {
                { "PACT_DESCRIPTION", "Test1" },
                { "PACT_PROVIDER_STATE", "Test2" }
            };

            var config = GetSubject(baseUri: baseUri, pactUri: pactUri, providerStateSetupUri: providerStateSetupUri, verifierConfig: verifierConfig, environment: environment);

            AssertEnvironmentIsCorrectlySet(environment, config.Environment);
        }

        [Fact]
        public void Ctor_WhenVerifierConfigIsNull_SetsOutputtersToNull()
        {
            var config = GetSubject();

            Assert.Null(config.Outputters);
        }

        private void AssertEnvironmentIsCorrectlySet(IDictionary<string, string> expectedEnv, IDictionary<string, string> actualEnv)
        {
            expectedEnv = expectedEnv ?? new Dictionary<string, string>();

            Assert.Equal(expectedEnv.Count + 1, actualEnv.Count);

            foreach(var envVar in expectedEnv)
            {
                Assert.Equal(envVar.Value, actualEnv[envVar.Key]);
            }

            Assert.NotEmpty(actualEnv["PACT_INTERACTION_RERUN_COMMAND"]);
        }
    }
}