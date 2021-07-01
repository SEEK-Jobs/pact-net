using System;
using System.Collections.Generic;

using FluentAssertions;

using Xunit;

namespace PactNet.Native.Tests
{
    public class MessagePactVerifierTests
    {
        private readonly FakeMessageVerifier _verifier;

        public MessagePactVerifierTests()
        {
            _verifier = new FakeMessageVerifier();
        }

        [Fact]
        public void ServiceProvider_Should_Add_Base_Message_Uri()
        {
            _verifier.ServiceProvider("myProvider", new Uri("http://localhost:4444"));

            _verifier.VerifierArgs.Should().Contain("--base-path");
            _verifier.VerifierArgs.Should().Contain("/pact-messages");
        }

        [Fact]
        public void ServiceProvider_Should_Add_Base_Message_Uri_To_Relative_Path()
        {
            var relativePath = "/myservice";

            _verifier.ServiceProvider("myProvider", new Uri($"http://localhost:4444{relativePath}"));

            _verifier.VerifierArgs.Should().Contain("--base-path");
            _verifier.VerifierArgs.Should().Contain($"{relativePath}/pact-messages");
        }

        public class FakeMessageVerifier : MessagePactVerifier
        {
            public IList<string> VerifierArgs => verifierArgs;
        }
    }
}
