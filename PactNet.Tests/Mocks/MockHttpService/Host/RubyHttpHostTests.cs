using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using NSubstitute;
using PactNet.Core;
using PactNet.Mocks.MockHttpService;
using PactNet.Mocks.MockHttpService.Host;
using PactNet.Tests.Fakes;
using Xunit;

namespace PactNet.Tests.Mocks.MockHttpService.Host
{
    public class RubyHttpHostTests
    {
        private IPactCoreHost _mockCoreHost;
        private FakeHttpMessageHandler _fakeHttpMessageHandler;

        private IHttpHost GetSubject(Func<HttpResponseMessage> aliveCheckResponseFactory = null)
        {
            var baseUri = new Uri("http://localhost:9333");

            _mockCoreHost = Substitute.For<IPactCoreHost>();
            _fakeHttpMessageHandler = aliveCheckResponseFactory != null ?
                new FakeHttpMessageHandler(aliveCheckResponseFactory) :
                new FakeHttpMessageHandler();

            return new RubyHttpHost(
                _mockCoreHost,
                new AdminHttpClient(baseUri, _fakeHttpMessageHandler, null));
        }

        [Fact]
        public void Start_WhenCalledAndTheCoreHostStartsQuickly_ShouldStartTheCoreHostAndEnsureItIsRunning()
        {
            var host = GetSubject();

            host.Start();

            _mockCoreHost.Received(1).Start();
            Assert.Equal(1, _fakeHttpMessageHandler.RequestsReceived.Count());
            var receivedRequest = _fakeHttpMessageHandler.RequestsReceived.ElementAt(0);
            Assert.Equal(HttpMethod.Get, receivedRequest.Method);
            Assert.Equal("/", receivedRequest.RequestUri.PathAndQuery);
            Assert.Equal("true", receivedRequest.Headers.GetValues(Constants.AdministrativeRequestHeaderKey).First());
        }

        [Fact]
        public void Start_WhenCalledAndTheCoreHostStartsSlowly_ShouldStartTheCoreHostAndEnsureItIsRunning()
        {
            var count = 0;
            var host = GetSubject(() =>
            {
                count++;
                return count < 10 ?
                    new HttpResponseMessage(HttpStatusCode.InternalServerError) :
                    new HttpResponseMessage(HttpStatusCode.OK);
            });

            host.Start();

            _mockCoreHost.Received(1).Start();
            Assert.Equal(10, _fakeHttpMessageHandler.RequestsReceived.Count());
        }

        [Fact]
        public void Start_WhenCalledAndTheCoreHostDoesNotStart_ThrowsPactFailureException()
        {
            var count = 0;
            var host = GetSubject(() =>
            {
                count++;
                return count < 21 ?
                    new HttpResponseMessage(HttpStatusCode.InternalServerError) :
                    new HttpResponseMessage(HttpStatusCode.OK);
            });

            Assert.Throws<PactFailureException>(() => host.Start());
            _mockCoreHost.Received(1).Start();
            Assert.Equal(20, _fakeHttpMessageHandler.RequestsReceived.Count());
        }

        [Fact]
        public void Stop_WhenCalled_ShouldStopTheCoreHost()
        {
            var host = GetSubject();

            host.Stop();

            _mockCoreHost.Received(1).Stop();
        }
    }
}
