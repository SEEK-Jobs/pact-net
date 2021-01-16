using System;
using System.Threading;
using PactNet.Core;
using PactNet.Mocks.MockHttpService.Models;
using PactNet.Models;

namespace PactNet.Mocks.MockHttpService.Host
{
    internal class RubyHttpHost : IHttpHost
    {
        private readonly IPactCoreHost _coreHost;
        private readonly AdminHttpClient _adminHttpClient;

        internal RubyHttpHost(
            IPactCoreHost coreHost,
            AdminHttpClient adminHttpClient)
        {
            _coreHost = coreHost;
            _adminHttpClient = adminHttpClient;
        }

        public RubyHttpHost(
            Uri baseUri,
            string consumerName, string providerName, PactConfig config, IPAddress host = IPAddress.Loopback,
            string sslCert = null, string sslKey = null) :
            this(new PactCoreHost<MockProviderHostConfig>(
                    new MockProviderHostConfig(baseUri.Port,
                        baseUri.Scheme.Equals("HTTPS", StringComparison.OrdinalIgnoreCase),
                        consumerName,
                        providerName,
                        config,
                        host,
                        sslCert,
                        sslKey)),
                new AdminHttpClient(baseUri))
        {
        }

        private Tuple<bool, Exception> IsMockProviderServiceRunning()
        {
            try
            {
                Async.RunSync(() => _adminHttpClient.SendAdminHttpRequest(HttpVerb.Get, "/"));
                return new Tuple<bool, Exception>(true, null);
            }
            catch (Exception ex)
            {
                return new Tuple<bool, Exception>(false, ex);
            }
        }

        public void Start()
        {
            _coreHost.Start();

            var aliveChecks = 1;

            var lastOutput = IsMockProviderServiceRunning();
            while (!lastOutput.Item1)
            {
                if (aliveChecks >= 20)
                {
                    throw lastOutput.Item2;
                }

                Thread.Sleep(1000);
                lastOutput = IsMockProviderServiceRunning();
                aliveChecks++;
            }
        }

        public void Stop()
        {
            _coreHost.Stop();
        }
    }
}