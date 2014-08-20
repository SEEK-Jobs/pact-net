using System.Collections.Generic;
using Nancy;
using Nancy.Bootstrapper;
using Nancy.Diagnostics;
using Nancy.TinyIoc;
using PactNet.Mocks.MockHttpService.Comparers;
using PactNet.Mocks.MockHttpService.Mappers;
using PactNet.Reporters;

namespace PactNet.Mocks.MockHttpService.Nancy
{
    public class MockProviderNancyBootstrapper : DefaultNancyBootstrapper
    {
        private readonly IMockContextService _mockContextService;

        public MockProviderNancyBootstrapper(IMockContextService mockContextService)
        {
            _mockContextService = mockContextService;
        }

        protected override IEnumerable<ModuleRegistration> Modules
        {
            get { return new List<ModuleRegistration>(); }
        }

        protected override NancyInternalConfiguration InternalConfiguration
        {
            get
            {
                return NancyInternalConfiguration.WithOverrides(c =>
                {
                    c.ContextFactory = typeof (PactAwareContextFactory);
                    c.RequestDispatcher = typeof(MockProviderNancyRequestDispatcher);
                });
            }
        }

        protected override void ApplicationStartup(TinyIoCContainer container, IPipelines pipelines)
        {
            RegisterDependenciesWithNancyContainer(container);

            DiagnosticsHook.Disable(pipelines);
        }

        protected override void ConfigureApplicationContainer(TinyIoCContainer container)
        {
        }

        private void RegisterDependenciesWithNancyContainer(TinyIoCContainer container)
        {
            container.Register(typeof(IProviderServiceRequestMapper), typeof(ProviderServiceRequestMapper));
            container.Register(typeof(INancyResponseMapper), typeof(NancyResponseMapper));
            container.Register(typeof (IMockContextService), _mockContextService);
            container.Register(typeof (IMockProviderNancyRequestHandler), typeof (MockProviderNancyRequestHandler));
        }
    }
}