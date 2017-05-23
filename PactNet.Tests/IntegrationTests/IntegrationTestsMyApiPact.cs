﻿using System;
using System.Net.Http;
using PactNet.Mocks.MockHttpService;
using PactNet.Mocks.MockHttpService.Mappers;

namespace PactNet.Tests.IntegrationTests
{
    public class IntegrationTestsMyApiPact : IDisposable
    {
        public IPactBuilder PactBuilder { get; private set; }
        public IMockProviderService MockProviderService { get; private set; }

        public int MockServerPort { get { return 4321; } }
        public string MockProviderServiceBaseUri { get { return String.Format("http://localhost:{0}", MockServerPort); } }

        public IntegrationTestsMyApiPact()
        {
            var pactConfig = new PactConfig();

            PactBuilder = new PactBuilder((port, enableSsl, providerName, bindOnAllAdapters) =>
                    new MockProviderService(
                        baseUri => HttpHostFactory.Create(baseUri, pactConfig),
                        port, enableSsl,
                        baseUri => new HttpClient { BaseAddress = new Uri(baseUri) },
                        new HttpMethodMapper()))
                .ServiceConsumer("IntegrationTests")
                .HasPactWith("MyApi");

            MockProviderService = PactBuilder.MockService(MockServerPort);
        }

        public void Dispose()
        {
            PactBuilder.Build();
        }
    }
}