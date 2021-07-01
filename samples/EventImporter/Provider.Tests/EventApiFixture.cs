using System;

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace Provider.Tests
{
    public class EventApiFixture : IDisposable
    {
        private readonly IHost server;

        public Uri ServerUri { get; }

        public EventApiFixture()
        {
            ServerUri = new Uri("http://localhost:9333");

            server = Host.CreateDefaultBuilder()
                              .ConfigureWebHostDefaults(webBuilder =>
                              {
                                  webBuilder.UseUrls(ServerUri.ToString());
                                  webBuilder.UseStartup<TestStartup>();
                              })
                              .Build();

            server.Start();
        }

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        public void Dispose()
        {
            server.Dispose();
        }
    }
}
