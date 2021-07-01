using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using PactNet.Native.Messaging;

namespace Provider.Tests
{
    public class TestStartup
    {
        private readonly Startup inner;

        public TestStartup(IConfiguration configuration)
        {
            inner = new Startup(configuration);
        }

        public void ConfigureServices(IServiceCollection services)
        {
            inner.ConfigureServices(services);
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app
               .UseMiddleware<ProviderStateMiddleware>()
               .UseMiddleware<AuthorizationTokenReplacementMiddleware>()
               .UseMessaging();

            inner.Configure(app, env);
        }
    }
}
