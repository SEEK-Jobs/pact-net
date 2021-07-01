using System;
using System.Collections.Generic;

using AutoFixture;

using FluentAssertions;

using Moq;

using Newtonsoft.Json;

using PactNet.Native.Exceptions;
using PactNet.Native.Models;

using Xunit;

namespace PactNet.Native.Tests
{
    public class NativePactMessageBuilderTests
    {
        private readonly IPactMessageBuilderV3 _builder;
        private readonly Mock<IMessageMockServer> _mockedServer;
        private readonly MessageHandle _handle;
        private readonly string _pactDir;
        private readonly MessagePactHandle _pactHandle;

        public NativePactMessageBuilderTests()
        {
            var fixture = new Fixture();
            var customization = new SupportMutableValueTypesCustomization();
            customization.Customize(fixture);

            _pactDir = "C:/";
            _pactHandle = new MessagePactHandle();
            _handle = fixture.Create<MessageHandle>();
            _mockedServer = new Mock<IMessageMockServer>();

            _mockedServer
                .Setup(x => x.NewMessage(It.IsAny<MessagePactHandle>(), It.IsAny<string>()))
                .Returns(_handle);

            var config = new PactConfig { PactDir = _pactDir };
            _builder = new NativePactMessageBuilder(_mockedServer.Object, _pactHandle, config);
        }

        [Fact]
        public void Ctor_Throws_Exception_If_Server_Not_Set()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new NativePactMessageBuilder(null, new MessagePactHandle(), new PactConfig()));
        }

        [Fact]
        public void Ctor_Throws_Exception_If_Config_Not_Set()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new NativePactMessageBuilder(_mockedServer.Object, new MessagePactHandle(), null));
        }

        [Fact]
        public void ExpectsTeReceive_WhenCalled_AddsDescription()
        {
            _builder.ExpectsToReceive("provider state");

            _mockedServer.Verify(s => s.MessageExpectsToReceive(_handle, "provider state"));
        }

        [Fact]
        public void Given_WhenCalled_AddsProviderState()
        {
            _builder.Given("provider state");

            _mockedServer.Verify(s => s.MessageGiven(_handle, "provider state"));
        }

        [Fact]
        public void Given_WithParams_AddsProviderState()
        {
            _builder.Given("provider state",
                new Dictionary<string, string>
                {
                    ["foo"] = "bar",
                    ["baz"] = "bash",
                });

            _mockedServer.Verify(s => s.MessageGivenWithParam(_handle, "provider state", "foo", "bar"));
            _mockedServer.Verify(s => s.MessageGivenWithParam(_handle, "provider state", "baz", "bash"));
        }

        [Fact]
        public void WithMetadata_WhenCalled_AddsMetadata()
        {
            var expectedKey = "poolId";
            var expectedValue = "1234";

            _builder.WithMetadata(expectedKey, expectedValue);

            _mockedServer.Verify(s => s.MessageWithMetadata(_handle, expectedKey, expectedValue));
        }

        [Fact]
        public void WithContent_WhenCalled_AddsContent()
        {
            var content = new { id = 1, desc = "description" };
            _builder.WithContent(content);

            _mockedServer.Verify(s => s.MessageWithContents(_handle, "application/json", JsonConvert.SerializeObject(content), 100));
        }

        [Fact]
        public void Build_WhenCalled_WritesPactFile()
        {
            _builder.Build();

            _mockedServer.Verify(s => s.WriteMessagePactFile(_pactHandle, _pactDir, true));
        }

        [Fact]
        public void Verify_Should_Fail_If_Type_Is_Not_The_Same_As_The_Message()
        {
            //Arrange
            SetServerReifyMessage("{ \"param1\": \"value1\" }");

            //Act
            _builder
                .Invoking(x => x.Verify<MessageModel>(_ => { }))
                .Should().Throw<PactMessageConsumerVerificationException>();
        }

        [Fact]
        public void Verify_Should_Fail_If_Verification_By_The_Consumer_Handler_Throws_Exception()
        {
            //Arrange
            var testMessage = new MessageModel(1, string.Empty).ToNativeMessage();

            SetServerReifyMessage(JsonConvert.SerializeObject(testMessage));

            //Act
            _builder
                .Invoking(x => x.Verify<MessageModel>(_ => throw new Exception("exception test")))
                .Should().Throw<PactMessageConsumerVerificationException>();
        }

        [Fact]
        public void Verify_Checks_Consumer_Handler_Completes_Successfully_With_Message()
        {
            //Arrange
            var testMessage = new MessageModel(1, "this message is a test").ToNativeMessage();

            SetServerReifyMessage(JsonConvert.SerializeObject(testMessage));

            //Act
            _builder
                .Invoking(x => x.Verify<MessageModel>(_ => { }))
                .Should().Throw<PactMessageConsumerVerificationException>();
        }

        private void SetServerReifyMessage(string message)
        {
            _mockedServer
                .Setup(x => x.MessageReify(It.IsAny<MessageHandle>()))
                .Returns(message);
        }

        private class MessageModel
        {
            private readonly int _id;
            private readonly string _description;

            internal MessageModel(int id, string description)
            {
                _id = id;
                _description = description;
            }

            internal NativeMessage ToNativeMessage()
            {
                return new NativeMessage
                {
                    Contents = this
                };
            }
        }
    }
}
