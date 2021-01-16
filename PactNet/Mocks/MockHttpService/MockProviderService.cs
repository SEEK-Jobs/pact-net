using System;
using System.Collections.Generic;
using PactNet.Mocks.MockHttpService.Host;
using PactNet.Mocks.MockHttpService.Models;
using PactNet.Mocks.Models;
using PactNet.Models;
using static System.String;
using PactNet.Core;

namespace PactNet.Mocks.MockHttpService
{
    public class MockProviderService : IMockProviderService
    {
        private readonly Func<Uri, IHttpHost> _hostFactory;
        private IHttpHost _host;
        private readonly AdminHttpClient _adminHttpClient;

        private string _providerState;
        private string _description;
        private ProviderServiceRequest _request;
        private ProviderServiceResponse _response;

        public Uri BaseUri { get; }

        internal MockProviderService(
            Func<Uri, IHttpHost> hostFactory,
            int port,
            bool enableSsl,
            Func<Uri, AdminHttpClient> adminHttpClientFactory)
        {
            _hostFactory = hostFactory;
            BaseUri = new Uri($"{(enableSsl ? "https" : "http")}://localhost:{port}");
            _adminHttpClient = adminHttpClientFactory(BaseUri);
        }

        public MockProviderService(int port, bool enableSsl, string consumerName, string providerName, PactConfig config, IPAddress ipAddress)
            : this(port, enableSsl, consumerName, providerName, config, ipAddress, null, null, null)
        {
        }

        public MockProviderService(int port, bool enableSsl, string consumerName, string providerName, PactConfig config, IPAddress ipAddress, Newtonsoft.Json.JsonSerializerSettings jsonSerializerSettings)
            : this(port, enableSsl, consumerName, providerName, config, ipAddress, jsonSerializerSettings, null, null)
        {
        }

        public MockProviderService(int port, bool enableSsl, string consumerName, string providerName, PactConfig config, IPAddress ipAddress, Newtonsoft.Json.JsonSerializerSettings jsonSerializerSettings, string sslCert, string sslKey)
            : this(
                baseUri => new RubyHttpHost(baseUri, consumerName, providerName, config, ipAddress, sslCert, sslKey),
                port,
                enableSsl,
                baseUri => new AdminHttpClient(baseUri, jsonSerializerSettings))
        {
        }

        public IMockProviderService Given(string providerState)
        {
            if (IsNullOrEmpty(providerState))
            {
                throw new ArgumentException("Please supply a non null or empty providerState");
            }

            _providerState = providerState;

            return this;
        }

        public IMockProviderService UponReceiving(string description)
        {
            if (IsNullOrEmpty(description))
            {
                throw new ArgumentException("Please supply a non null or empty description");
            }

            _description = description;

            return this;
        }

        public IMockProviderService With(ProviderServiceRequest request)
        {
            if (request == null)
            {
                throw new ArgumentException("Please supply a non null request");
            }

            if (request.Method == HttpVerb.NotSet)
            {
                throw new ArgumentException("Please supply a request Method");
            }

            if (!IsContentTypeSpecifiedForBody(request))
            {
                throw new ArgumentException("Please supply a Content-Type request header");
            }

            _request = request;

            return this;
        }

        public void WillRespondWith(ProviderServiceResponse response)
        {
            if (response == null)
            {
                throw new ArgumentException("Please supply a non null response");
            }

            if (response.Status <= 0)
            {
                throw new ArgumentException("Please supply a response Status");
            }

            if (!IsContentTypeSpecifiedForBody(response))
            {
                throw new ArgumentException("Please supply a Content-Type response header");
            }

            _response = response;

            RegisterInteraction();
        }

        public void VerifyInteractions()
        {
            var testContext = BuildTestContext();
            Async.RunSync(() => _adminHttpClient.SendAdminHttpRequest(HttpVerb.Get, $"{Constants.InteractionsVerificationPath}?example_description={testContext}"));
        }

        public void SendAdminHttpRequest(HttpVerb method, string path)
        {
            Async.RunSync(() => _adminHttpClient.SendAdminHttpRequest(method, path));
        }

        public void Start()
        {
            StopRunningHost();

            _host = _hostFactory(BaseUri);
            _host.Start();
        }

        public void Stop()
        {
            ClearAllState();
            StopRunningHost();
        }

        public void ClearInteractions()
        {
            if (_host != null)
            {
                var testContext = BuildTestContext();
                Async.RunSync(() => _adminHttpClient.SendAdminHttpRequest(HttpVerb.Delete, $"{Constants.InteractionsPath}?example_description={testContext}"));
            }
        }

        private void RegisterInteraction()
        {
            if (IsNullOrEmpty(_description))
            {
                throw new InvalidOperationException("description has not been set, please supply using the UponReceiving method.");
            }

            if (_request == null)
            {
                throw new InvalidOperationException("request has not been set, please supply using the With method.");
            }

            if (_response == null)
            {
                throw new InvalidOperationException("response has not been set, please supply using the WillRespondWith method.");
            }

            var interaction = new ProviderServiceInteraction
            {
                ProviderState = _providerState,
                Description = _description,
                Request = _request,
                Response = _response
            };

            Async.RunSync(() => _adminHttpClient.SendAdminHttpRequest(HttpVerb.Post, Constants.InteractionsPath, interaction));

            ClearTrasientState();
        }

        private void StopRunningHost()
        {
            if (_host != null)
            {
                _host.Stop();
                _host = null;
            }
        }

        private void ClearAllState()
        {
            ClearTrasientState();
            ClearInteractions();
        }

        private void ClearTrasientState()
        {
            _request = null;
            _response = null;
            _providerState = null;
            _description = null;
        }

        private bool IsContentTypeSpecifiedForBody(IMessage message)
        {
            //No content-type required if there is no body
            if (message.Body == null)
            {
                return true;
            }

            IDictionary<string, object> headers = null;
            if (message.Headers != null)
            {
                headers = new Dictionary<string, object>(message.Headers, StringComparer.OrdinalIgnoreCase);
            }

            return headers != null && headers.ContainsKey("Content-Type");
        }

        private static string BuildTestContext()
        {
#if USE_NET4X
            var stack = new System.Diagnostics.StackTrace(true);
            var stackFrames = stack.GetFrames() ?? new System.Diagnostics.StackFrame[0];

            var relevantStackFrameSummaries = new List<string>();

            foreach (var stackFrame in stackFrames)
            {
                var type = stackFrame.GetMethod().ReflectedType;

                if (type == null ||
                    (type.Assembly.GetName().Name.StartsWith("PactNet", StringComparison.CurrentCultureIgnoreCase) &&
                     !type.Assembly.GetName().Name.Equals("PactNet.Tests", StringComparison.CurrentCultureIgnoreCase)))
                {
                    continue;
                }

                //Don't care about any mscorlib frames down
                if (type.Assembly.GetName().Name.Equals("mscorlib", StringComparison.CurrentCultureIgnoreCase))
                {
                    break;
                }

                relevantStackFrameSummaries.Add(Format("{0}.{1}", type.Name, stackFrame.GetMethod().Name));
            }

            return Join(" ", relevantStackFrameSummaries);

#else
            return string.Empty;
#endif
        }
    }
}