using Autofac;
using Autofac.Integration.WebApi;
using Microsoft.Owin.Security.DataProtection;
using Microsoft.Owin.Security.OAuth;
using Owin;
using Provider.Api.Web.Tests.Controllers;

namespace Provider.Api.Web.Tests
{
    public class TestStartup
    {
        public void Configuration(IAppBuilder app)
        {
            var apiStartup = new Startup();
            app.Use<ProviderStateMiddleware>();
            app.Use(typeof(AuthorizationTokenReplacementMiddleware),
                app.CreateDataProtector(typeof(OAuthAuthorizationServerMiddleware).Namespace, "Access_Token", "v1"));

            //var config = new HttpConfiguration();
            //      config.Formatters.JsonFormatter.SupportedMediaTypes
            //       .Add(new MediaTypeHeaderValue("text/html"));

            //      // Web API routes
            //      config.MapHttpAttributeRoutes();
            //      app.UseWebApi(config);

            var builder = new ContainerBuilder();
            builder.RegisterApiControllers(typeof(VerificationController).Assembly);
            var container = builder.Build();

            app.UseAutofacMiddleware(container);
            apiStartup.Configuration(app);
        }
    }
}