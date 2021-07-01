using System;

using AutoFixture;

using FluentAssertions;

using Moq;

using PactNet.Infrastructure.Outputters;

using Xunit;

namespace PactNet.Native.Tests
{
    public class NativePactBuilderTests
    {
        private readonly NativePactBuilder builder;

        private readonly Mock<IHttpMockServer> mockServer;
        private readonly Mock<IOutput> mockOutput;

        private readonly IFixture fixture;
        private readonly PactHandle handle;
        private readonly Uri serverUri;
        private readonly PactConfig config;

        public NativePactBuilderTests()
        {
            mockServer = new Mock<IHttpMockServer>(MockBehavior.Strict);
            mockOutput = new Mock<IOutput>();

            fixture = new Fixture();
            var customization = new SupportMutableValueTypesCustomization();
            customization.Customize(fixture);

            handle = fixture.Create<PactHandle>();
            serverUri = fixture.Create<Uri>();
            config = new PactConfig
            {
                Outputters = new[] { mockOutput.Object }
            };

            // set some default mock setups
            mockServer.Setup(s => s.CreateMockServerForPact(handle, "127.0.0.1:0", false)).Returns(serverUri.Port);
            mockServer.Setup(s => s.NewInteraction(handle, It.IsAny<string>())).Returns(new InteractionHandle());
            mockServer.Setup(s => s.MockServerLogs(serverUri.Port)).Returns(string.Empty);
            mockServer.Setup(s => s.MockServerMismatches(serverUri.Port)).Returns(string.Empty);
            mockServer.Setup(s => s.WritePactFile(serverUri.Port, config.PactDir, false));
            mockServer.Setup(s => s.CleanupMockServer(serverUri.Port)).Returns(true);

            builder = new NativePactBuilder(mockServer.Object, handle, config);
        }

        [Fact]
        public void UponReceiving_WhenCalled_CreatesNewInteraction()
        {
            const string Description = "test description";

            builder.UponReceiving(Description);

            mockServer.Verify(s => s.NewInteraction(handle, Description));
        }

        [Fact]
        public void Verify_WhenCalled_StartsMockServer()
        {
            builder.Verify(Success);

            mockServer.Verify(s => s.CreateMockServerForPact(handle, "127.0.0.1:0", false));
        }

        [Fact]
        public void Verify_ErrorStartingMockServer_ThrowsInvalidOperationException()
        {
            mockServer
                .Setup(s => s.CreateMockServerForPact(It.IsAny<PactHandle>(), It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<InvalidOperationException>();

            Action action = () => builder.Verify(Success);

            action.Should().Throw<InvalidOperationException>("because the mock server failed to start");
        }

        [Fact(Skip = "Enable this when the released FFI library supports getting mock server logs")]
        public void Verify_NoLogs_DoesNotWriteLogs()
        {
            builder.Verify(Success);

            mockOutput.Verify(o => o.WriteLine(It.IsAny<string>()), Times.Never);
        }

        [Fact(Skip = "Enable this when the released FFI library supports getting mock server logs")]
        public void Verify_Logs_WritesLogsToOutput()
        {
            const string expected = "some logs";
            mockServer.Setup(s => s.MockServerLogs(serverUri.Port)).Returns(expected);

            builder.Verify(Success);

            mockOutput.Verify(o => o.WriteLine(expected));
        }

        [Fact]
        public void Verify_InteractionThrowsException_RethrowsInteractionException()
        {
            builder
                .Invoking(b => b.Verify(Error))
                .Should().ThrowExactly<Exception>("because the inner interaction call failed");
        }

        [Fact]
        public void Verify_NoMismatches_WritesPactFile()
        {
            builder.Verify(Success);

            mockServer.Verify(s => s.WritePactFile(serverUri.Port, config.PactDir, false));
        }

        [Fact]
        public void Verify_NoMismatches_ShutsDownMockServer()
        {
            builder.Verify(Success);

            mockServer.Verify(s => s.CleanupMockServer(serverUri.Port));
        }

        [Fact]
        public void Verify_FailedToWritePactFile_ThrowsInvalidOperationException()
        {
            mockServer
                .Setup(s => s.WritePactFile(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<InvalidOperationException>();

            Action action = () => builder.Verify(Success);

            action.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void Verify_Mismatches_DoesNotWritePactFile()
        {
            mockServer.Setup(s => s.MockServerMismatches(serverUri.Port)).Returns("some mismatches");

            try
            {
                builder.Verify(Success);
            }
            // ReSharper disable once EmptyGeneralCatchClause
            catch
            {
            }

            mockServer.Verify(s => s.WritePactFile(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<bool>()), Times.Never);
        }

        [Fact]
        public void Verify_Mismatches_WritesMismatchesToOutput()
        {
            const string expected = "some mismatches";
            mockServer.Setup(s => s.MockServerMismatches(serverUri.Port)).Returns(expected);

            try
            {
                builder.Verify(Success);
            }
            // ReSharper disable once EmptyGeneralCatchClause
            catch
            {
            }

            mockOutput.Verify(o => o.WriteLine(expected));
        }

        [Fact]
        public void Verify_Mismatches_ThrowsPactFailureException()
        {
            mockServer.Setup(s => s.MockServerMismatches(serverUri.Port)).Returns("some mismatches");

            Action action = () => builder.Verify(Success);

            action.Should().Throw<PactFailureException>();
        }

        [Fact]
        public void Verify_Mismatches_ShutsDownMockServer()
        {
            mockServer.Setup(s => s.MockServerMismatches(serverUri.Port)).Returns("some mismatches");

            try
            {
                builder.Verify(Success);
            }
            // ReSharper disable once EmptyGeneralCatchClause
            catch
            {
            }

            mockServer.Verify(s => s.CleanupMockServer(serverUri.Port));
        }

        private static void Success(IConsumerContext ctx)
        {
        }

        private static void Error(IConsumerContext ctx)
        {
            throw new Exception("Interaction failed");
        }
    }
}
