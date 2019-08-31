using System;
using System.IO;
using NSubstitute;
using PactNet.Core;
using Xunit;

namespace PactNet.Tests
{
    public class PactVerifierTests
    {
        private IPactCoreHost _mockVerifierCoreHost;

        private IPactVerifier GetSubject()
        {
            _mockVerifierCoreHost = Substitute.For<IPactCoreHost>();
            return new PactVerifier(hostConfig => _mockVerifierCoreHost, new PactVerifierConfig());
        }

        [Fact]
        public void PactVerifier_WhenCalledWithPublishVerificationResultsAndNoProviderVersion_ThrowsArgumentException()
        {
            var config = new PactVerifierConfig
            {
                PublishVerificationResults = true,
                ProviderVersion = string.Empty
            };

            Assert.Throws<ArgumentException>(() => new PactVerifier(config));
        }

        [Fact]
        public void ProviderState_WhenCalledWithSetupUri_SetsProviderStateSetupUri()
        {
            const string providerStateSetupUri = "http://localhost:223/states";
            var pactVerifier = (PactVerifier)GetSubject();

            pactVerifier
                .ProviderState(providerStateSetupUri);

            Assert.Equal(providerStateSetupUri, pactVerifier.ProviderStateSetupUri.OriginalString);
        }

        [Fact]
        public void ProviderState_WhenCalledWithNullSetupUri_ThrowsArgumentException()
        {
            var pactVerifier = GetSubject();

            Assert.Throws<ArgumentException>(() =>
                pactVerifier
                .ProviderState(null));
        }

        [Fact]
        public void ProviderState_WhenCalledWithEmptySetupUri_ThrowsArgumentException()
        {
            var pactVerifier = GetSubject();

            Assert.Throws<ArgumentException>(() =>
                pactVerifier
                .ProviderState(String.Empty));
        }

        [Fact]
        public void ServiceProvider_WhenCalledWithNullProviderName_ThrowsArgumentException()
        {
            var pactVerifier = GetSubject();

            Assert.Throws<ArgumentException>(() => pactVerifier.ServiceProvider(null, "http://localhost:3442"));
        }

        [Fact]
        public void ServiceProvider_WhenCalledWithEmptyProviderName_ThrowsArgumentException()
        {
            var pactVerifier = GetSubject();

            Assert.Throws<ArgumentException>(() => pactVerifier.ServiceProvider(String.Empty, "http://localhost:3442"));
        }

        [Fact]
        public void ServiceProvider_WhenCalledWithAnAlreadySetProviderName_ThrowsArgumentException()
        {
            const string providerName = "My API";
            var pactVerifier = GetSubject();

            pactVerifier.ServiceProvider(providerName, "http://localhost:3442");

            Assert.Throws<ArgumentException>(() => pactVerifier.ServiceProvider(providerName, "http://localhost:3442"));
        }

        [Fact]
        public void ServiceProvider_WhenCalledWithNullBaseUri_ThrowsArgumentException()
        {
            var pactVerifier = GetSubject();

            Assert.Throws<ArgumentException>(() => pactVerifier.ServiceProvider("Event API", null));
        }

        [Fact]
        public void ServiceProvider_WhenCalledWithProviderName_SetsProviderName()
        {
            const string providerName = "Event API";
            var pactVerifier = GetSubject();

            pactVerifier.ServiceProvider(providerName, "https://localhost:3442");

            Assert.Equal(providerName, ((PactVerifier)pactVerifier).ProviderName);
        }

        [Fact]
        public void ServiceProvider_WhenCalledWithBaseUri_HttpClientRequestSenderIsPassedIntoProviderServiceValidatorFactoryWhenVerifyIsCalled()
        {
            const string baseUri = "https://localhost:3442";
            var pactVerifier = GetSubject();

            pactVerifier.ServiceProvider("Event API", baseUri);

            Assert.Equal(baseUri, ((PactVerifier)pactVerifier).ServiceBaseUri.OriginalString);
        }

        [Fact]
        public void HonoursPactWith_WhenCalledWithNullConsumerName_ThrowsArgumentException()
        {
            var pactVerifier = GetSubject();

            Assert.Throws<ArgumentException>(() => pactVerifier.HonoursPactWith(null));
        }

        [Fact]
        public void HonoursPactWith_WhenCalledWithEmptyConsumerName_ThrowsArgumentException()
        {
            var pactVerifier = GetSubject();

            Assert.Throws<ArgumentException>(() => pactVerifier.HonoursPactWith(String.Empty));
        }

        [Fact]
        public void HonoursPactWith_WhenCalledWithAnAlreadySetConsumerName_ThrowsArgumentException()
        {
            const string consumerName = "My Consumer";
            var pactVerifier = GetSubject();

            pactVerifier.HonoursPactWith(consumerName);

            Assert.Throws<ArgumentException>(() => pactVerifier.HonoursPactWith(consumerName));
        }

        [Fact]
        public void HonoursPactWith_WhenCalledWithConsumerName_SetsConsumerName()
        {
            const string consumerName = "My Client";
            var pactVerifier = GetSubject();

            pactVerifier.HonoursPactWith(consumerName);

            Assert.Equal(consumerName, ((PactVerifier)pactVerifier).ConsumerName);
        }

        [Fact]
        public void PactUri_WhenCalledWithNullUri_ThrowsArgumentException()
        {
            var pactVerifier = GetSubject();

            Assert.Throws<ArgumentException>(() => pactVerifier.PactUri(null));
        }

        [Fact]
        public void PactUri_WhenCalledWithEmptyUri_ThrowsArgumentException()
        {
            var pactVerifier = GetSubject();

            Assert.Throws<ArgumentException>(() => pactVerifier.PactUri(String.Empty));
        }

        [Fact]
        public void PactUri_WhenCalledWithUri_SetsPactFileUri()
        {
            var pactFileUri = $"..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}Consumer.Tests{Path.DirectorySeparatorChar}pacts{Path.DirectorySeparatorChar}my_client-event_api.json";
            var pactVerifier = GetSubject();

            pactVerifier.PactUri(pactFileUri);

            Assert.Equal(pactFileUri, ((PactVerifier)pactVerifier).PactFileUri);
        }

        [Fact]
        public void PactUri_WhenCalledWithUriInADifferentFormat_SetsPactFileUri()
        {
            var pactFileUri = $"..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}Consumer.Tests{Path.DirectorySeparatorChar}pacts{Path.DirectorySeparatorChar}my_client-event_api.json";
            var pactVerifier = GetSubject();

            pactVerifier.PactUri(pactFileUri);

            Assert.Equal(pactFileUri, ((PactVerifier)pactVerifier).PactFileUri);
        }

        [Fact]
        public void Verify_WhenServiceBaseUriIsNull_ThrowsInvalidOperationException()
        {
            var pactVerifier = GetSubject();
            pactVerifier.PactUri($"..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}Consumer.Tests{Path.DirectorySeparatorChar}pacts{Path.DirectorySeparatorChar}my_client-event_api.json");

            Assert.Throws<InvalidOperationException>(() => pactVerifier.Verify());
        }

        [Fact]
        public void Verify_WhenPactFileUriIsNull_ThrowsInvalidOperationException()
        {
            var pactVerifier = GetSubject();
            pactVerifier.ServiceProvider("Event API", "http://localhost:2839");

            Assert.Throws<InvalidOperationException>(() => pactVerifier.Verify());
        }

        [Fact]
        public void Verify_WhenTheVerifierIsCorrectlySetUpWithALocalPactFile_PactVerifyCoreHostIsStarted()
        {
            var serviceProvider = "Event API";
            var serviceConsumer = "My client";
            var pactUri = $"..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}Consumer.Tests{Path.DirectorySeparatorChar}pacts{Path.DirectorySeparatorChar}my_client-event_api.json";

            var pactVerifier = GetSubject();
            pactVerifier
               .ServiceProvider(serviceProvider, "http://localhost")
               .HonoursPactWith(serviceConsumer)
               .PactUri(pactUri);

            pactVerifier.Verify();

            _mockVerifierCoreHost.Received(1).Start();
        }

        [Fact]
        public void Verify_WhenTheVerifierIsCorrectlySetUpWithAHttpPactFile_PactVerifyCoreHostIsStarted()
        {
            var serviceProvider = "Event API";
            var serviceConsumer = "My client";
            var pactUri = "http://broker/consumer/test/provider/hello/latest";

            var pactVerifier = GetSubject();
            pactVerifier
               .ServiceProvider(serviceProvider, "http://localhost")
               .HonoursPactWith(serviceConsumer)
               .PactUri(pactUri);

            pactVerifier.Verify();

            _mockVerifierCoreHost.Received(1).Start();
        }

        [Fact]
        public void Verify_WhenTheVerifierIsCorrectlySetUpWithAHttpsPactFileWithBasicAuthCredentials_PactVerifyCoreHostIsStarted()
        {
            var serviceProvider = "Event API";
            var serviceConsumer = "My client";
            var pactUri = "https://broker/consumer/test/provider/hello/latest";
            var pactUriOptions = new PactUriOptions("username", "password");

            var pactVerifier = GetSubject();
            pactVerifier
               .ServiceProvider(serviceProvider, "http://localhost")
               .HonoursPactWith(serviceConsumer)
               .PactUri(pactUri, pactUriOptions);

            pactVerifier.Verify();

            _mockVerifierCoreHost.Received(1).Start();
        }

        [Fact]
        public void Verify_WhenTheVerifierIsCorrectlySetUpWithAHttpsPactFileWitTokenAuthCredentials_PactVerifyCoreHostIsStarted()
        {
            var serviceProvider = "Event API";
            var serviceConsumer = "My client";
            var pactUri = "https://broker/consumer/test/provider/hello/latest";
            var pactUriOptions = new PactUriOptions("mytoken");

            var pactVerifier = GetSubject();
            pactVerifier
                .ServiceProvider(serviceProvider, "http://localhost")
                .HonoursPactWith(serviceConsumer)
                .PactUri(pactUri, pactUriOptions);

            pactVerifier.Verify();

            _mockVerifierCoreHost.Received(1).Start();
        }
    }
}
