﻿using Newtonsoft.Json;
using PactNet.Mocks.MockHttpService;
using PactNet.Mocks.MockHttpService.Models;
using PactNet.Models;
using System;
using System.IO;

namespace PactNet
{
    public class PactBuilder : IPactBuilder
    {
        public string ConsumerName { get; private set; }
        public string ProviderName { get; private set; }

        private readonly string _pactDir;

        private readonly
            Func<int, bool, string, string, IPAddress, JsonSerializerSettings, string, string, IMockProviderService>
            _mockProviderServiceFactory;

        private readonly
            Func<int, bool, string, string, IPAddress, JsonSerializerSettings, string, string, bool, IMockProviderService>
            _mockProviderServiceFactory2;

        private IMockProviderService _mockProviderService;

        internal PactBuilder(
            Func<int, bool, string, string, IPAddress, JsonSerializerSettings, string, string, IMockProviderService>
                mockProviderServiceFactory)
        {
            _mockProviderServiceFactory = mockProviderServiceFactory;
        }

        internal PactBuilder(
            Func<int, bool, string, string, IPAddress, JsonSerializerSettings, string, string, bool, IMockProviderService>
                mockProviderServiceFactory2)
        {
            _mockProviderServiceFactory2 = mockProviderServiceFactory2;
            _mockProviderServiceFactory = (port, enableSsl, consumerName, providerName, host, jsonSerializerSettings, sslCert, sslKey) => _mockProviderServiceFactory2(port, enableSsl, consumerName, providerName, host, jsonSerializerSettings, sslCert, sslKey, false);
        }

        public PactBuilder()
            : this(new PactConfig())
        {
        }

        public PactBuilder(PactConfig config)
            : this((port, enableSsl, consumerName, providerName, host, jsonSerializerSettings, sslCert, sslKey) =>
                new MockProviderService(port, enableSsl, consumerName, providerName, config, host,
                    jsonSerializerSettings, sslCert, sslKey))
        {
            _pactDir = config.PactDir;
        }

        public IPactBuilder ServiceConsumer(string consumerName)
        {
            if (string.IsNullOrEmpty(consumerName))
            {
                throw new ArgumentException("Please supply a non null or empty consumerName");
            }

            ConsumerName = consumerName;

            return this;
        }

        public IPactBuilder HasPactWith(string providerName)
        {
            if (string.IsNullOrEmpty(providerName))
            {
                throw new ArgumentException("Please supply a non null or empty providerName");
            }

            ProviderName = providerName;

            return this;
        }

        public IMockProviderService MockService(
            int port, 
            bool enableSsl = false, 
            IPAddress host = IPAddress.Loopback, 
            string sslCert = null, 
            string sslKey = null,
            bool useRemoteMockService = false)
        {
            if (_mockProviderServiceFactory2 != null)
            {
                return MockService(port, null, enableSsl: enableSsl, host: host, sslCert: sslCert, sslKey: sslKey, useRemoteMockService: useRemoteMockService, useIpv4LoopbackOnly: false);
            }
            return MockService(port, null, enableSsl: enableSsl, host: host, sslCert: sslCert, sslKey: sslKey, useRemoteMockService: useRemoteMockService);
        }

        public IMockProviderService MockService(
            int port,
            JsonSerializerSettings jsonSerializerSettings,
            bool enableSsl = false,
            IPAddress host = IPAddress.Loopback,
            string sslCert = null,
            string sslKey = null,
            bool useRemoteMockService = false)
        {
            if (string.IsNullOrEmpty(ConsumerName))
            {
                throw new InvalidOperationException(
                    "ConsumerName has not been set, please supply a consumer name using the ServiceConsumer method.");
            }

            if (string.IsNullOrEmpty(ProviderName))
            {
                throw new InvalidOperationException(
                    "ProviderName has not been set, please supply a provider name using the HasPactWith method.");
            }

            if (_mockProviderService != null && useRemoteMockService == false)
            {
                _mockProviderService.Stop();
            }

            _mockProviderService = _mockProviderServiceFactory(port, enableSsl, ConsumerName, ProviderName, host,
                jsonSerializerSettings, sslCert, sslKey);

            _mockProviderService.UseRemoteMockService = useRemoteMockService;

            _mockProviderService.Start();

            return _mockProviderService;
        }

        public IMockProviderService MockService(
            int port, 
            JsonSerializerSettings jsonSerializerSettings, 
            bool enableSsl = false, 
            IPAddress host = IPAddress.Loopback, 
            string sslCert = null, 
            string sslKey = null,
            bool useRemoteMockService = false,
            bool useIpv4LoopbackOnly = false)
        {
            if (string.IsNullOrEmpty(ConsumerName))
            {
                throw new InvalidOperationException(
                    "ConsumerName has not been set, please supply a consumer name using the ServiceConsumer method.");
            }

            if (string.IsNullOrEmpty(ProviderName))
            {
                throw new InvalidOperationException(
                    "ProviderName has not been set, please supply a provider name using the HasPactWith method.");
            }

            if (_mockProviderService != null && useRemoteMockService == false )
            {
                _mockProviderService.Stop();
            }

            _mockProviderService = _mockProviderServiceFactory2(port, enableSsl, ConsumerName, ProviderName, host,
                jsonSerializerSettings, sslCert, sslKey, useIpv4LoopbackOnly);

            _mockProviderService.UseRemoteMockService = useRemoteMockService;

            _mockProviderService.Start();

            return _mockProviderService;
        }

        public void Build()
        {
            if (_mockProviderService == null)
            {
                throw new InvalidOperationException(
                    $"The Pact file could not be saved because the mock provider service is not initialized. Please initialize by calling the {nameof(MockService)}() method.");
            }

            PersistPactFile();
            _mockProviderService.Stop();
        }

        private void PersistPactFile()
        {
            var responsePact = _mockProviderService.SendAdminHttpRequest(HttpVerb.Post, Constants.PactPath);

            if (_mockProviderService.UseRemoteMockService)
            {
                string fileName = ConsumerName.ToLower() + ProviderName.ToLower() + ".json";
                File.WriteAllText(Path.Combine(_pactDir, fileName), responsePact);
            }
        }
    }
}
