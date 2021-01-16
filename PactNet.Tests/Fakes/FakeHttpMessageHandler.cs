using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace PactNet.Tests.Fakes
{
    public class FakeHttpMessageHandler : DelegatingHandler
    {
        private readonly IList<HttpRequestMessage> _requestsReceived;
        public IEnumerable<HttpRequestMessage> RequestsReceived => _requestsReceived;

        private readonly IList<string> _requestContentReceived;
        public IEnumerable<string> RequestContentReceived => _requestContentReceived;

        public Func<HttpResponseMessage> ResponseFactory { get; set; }

        public FakeHttpMessageHandler(HttpResponseMessage response = null)
        {
            ResponseFactory = () => response ?? new HttpResponseMessage(HttpStatusCode.OK);
            _requestsReceived = new List<HttpRequestMessage>();
            _requestContentReceived = new List<string>();
        }

        public FakeHttpMessageHandler(Func<HttpResponseMessage> responseFactory)
        {
            ResponseFactory = responseFactory;
            _requestsReceived = new List<HttpRequestMessage>();
            _requestContentReceived = new List<string>();
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            _requestsReceived.Add(request);
            if (request.Content != null)
            {
                _requestContentReceived.Add(await request.Content.ReadAsStringAsync());
            }

            return ResponseFactory();
        }
    }
}