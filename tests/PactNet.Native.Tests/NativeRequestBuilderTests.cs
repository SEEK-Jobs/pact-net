using System;
using System.Collections.Generic;
using System.Net.Http;

using AutoFixture;

using FluentAssertions;

using Moq;

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

using Xunit;

namespace PactNet.Native.Tests
{
    public class NativeRequestBuilderTests
    {
        private readonly NativeRequestBuilder builder;

        private readonly Mock<IHttpMockServer> mockServer;

        private readonly IFixture fixture;
        private readonly InteractionHandle handle;
        private readonly JsonSerializerSettings settings;

        public NativeRequestBuilderTests()
        {
            mockServer = new Mock<IHttpMockServer>();

            fixture = new Fixture();
            var customization = new SupportMutableValueTypesCustomization();
            customization.Customize(fixture);

            handle = fixture.Create<InteractionHandle>();
            settings = new JsonSerializerSettings();

            builder = new NativeRequestBuilder(mockServer.Object, handle, settings);
        }

        [Fact]
        public void Given_WhenCalled_AddsProviderState()
        {
            builder.Given("provider state");

            mockServer.Verify(s => s.Given(handle, "provider state"));
        }

        [Fact]
        public void Given_WithParams_AddsProviderState()
        {
            builder.Given("provider state",
                               new Dictionary<string, string>
                               {
                                   ["foo"] = "bar",
                                   ["baz"] = "bash",
                               });

            mockServer.Verify(s => s.GivenWithParam(handle, "provider state", "foo", "bar"));
            mockServer.Verify(s => s.GivenWithParam(handle, "provider state", "baz", "bash"));
        }

        [Fact]
        public void WithRequest_HttpMethod_AddsRequest()
        {
            builder.WithRequest(HttpMethod.Post, "/some/path");

            mockServer.Verify(s => s.WithRequest(handle, "POST", "/some/path"));
        }

        [Fact]
        public void WithRequest_String_AddsRequest()
        {
            builder.WithRequest("POST", "/some/path");

            mockServer.Verify(s => s.WithRequest(handle, "POST", "/some/path"));
        }

        [Fact]
        public void WithQuery_WhenCalled_AddsQueryParam()
        {
            builder.WithQuery("name", "value");

            mockServer.Verify(s => s.WithQueryParameter(handle, "name", "value", 0));
        }

        [Fact]
        public void WithQuery_RepeatedQuery_SetsIndex()
        {
            builder.WithQuery("name", "value1");
            builder.WithQuery("name", "value2");
            builder.WithQuery("other", "value");

            mockServer.Verify(s => s.WithQueryParameter(handle, "name", "value1", 0));
            mockServer.Verify(s => s.WithQueryParameter(handle, "name", "value2", 1));
            mockServer.Verify(s => s.WithQueryParameter(handle, "other", "value", 0));
        }

        [Fact]
        public void WithHeader_WhenCalled_AddsHeaderParam()
        {
            builder.WithHeader("name", "value");

            mockServer.Verify(s => s.WithRequestHeader(handle, "name", "value", 0));
        }

        [Fact]
        public void WithHeader_RepeatedHeader_SetsIndex()
        {
            builder.WithHeader("name", "value1");
            builder.WithHeader("name", "value2");
            builder.WithHeader("other", "value");

            mockServer.Verify(s => s.WithRequestHeader(handle, "name", "value1", 0));
            mockServer.Verify(s => s.WithRequestHeader(handle, "name", "value2", 1));
            mockServer.Verify(s => s.WithRequestHeader(handle, "other", "value", 0));
        }

        [Fact]
        public void WithJsonBody_WithoutCustomSettings_AddsRequestBodyWithDefaultSettings()
        {
            builder.WithJsonBody(new { Foo = 42 });

            mockServer.Verify(s => s.WithRequestBody(handle, "application/json", @"{""Foo"":42}"));
        }

        [Fact]
        public void WithJsonBody_WithCustomSettings_AddsRequestBodyWithOverriddenSettings()
        {
            builder.WithJsonBody(new { Foo = 42 },
                                      new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });

            mockServer.Verify(s => s.WithRequestBody(handle, "application/json", @"{""foo"":42}"));
        }

        [Fact]
        public void WillRespond_RequestConfigured_ReturnsResponseBuilder()
        {
            builder.WithRequest(HttpMethod.Delete, "/foo");

            var responseBuilder = builder.WillRespond();

            responseBuilder.Should().BeOfType<NativeResponseBuilder>();
        }

        [Fact]
        public void WillRespond_RequestNotConfigured_ThrowsInvalidOperationException()
        {
            Action action = () => builder.WillRespond();

            action.Should().Throw<InvalidOperationException>("because the request has not been configured");
        }
    }
}
