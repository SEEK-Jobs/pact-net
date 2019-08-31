using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Owin;
using Newtonsoft.Json;
using static System.String;

namespace Provider.Api.Web.Tests
{
    public class ProviderStateMiddleware
    {
        private const string ConsumerName = "Event API Consumer";
        private readonly Func<IDictionary<string, object>, Task> m_next;
        private readonly IDictionary<string, Action> _providerStates;

        public ProviderStateMiddleware(Func<IDictionary<string, object>, Task> next)
        {
            m_next = next;
            _providerStates = new Dictionary<string, Action>
            {
                {
                    "there are events with ids '45D80D13-D5A2-48D7-8353-CBB4C0EAABF5', '83F9262F-28F1-4703-AB1A-8CFD9E8249C9' and '3E83A96B-2A0C-49B1-9959-26DF23F83AEB'",
                    InsertEventsIntoDatabase
                },
                {
                    "there is an event with id '83f9262f-28f1-4703-ab1a-8cfd9e8249c9'",
                    InsertEventIntoDatabase
                },
                {
                    "there is one event with type 'DetailsView'",
                    EnsureOneDetailsViewEventExists
                }
            };
        }

        private void InsertEventsIntoDatabase()
        {

        }

        private void InsertEventIntoDatabase()
        {

        }

        private void EnsureOneDetailsViewEventExists()
        {

        }

        public async Task Invoke(IDictionary<string, object> environment)
        {
            IOwinContext context = new OwinContext(environment);

            if (context.Request.Path.Value == "/provider-states")
            {
                context.Response.StatusCode = (int)HttpStatusCode.OK;

                if (context.Request.Method == HttpMethod.Post.ToString() &&
                    context.Request.Body != null)
                {
                    string jsonRequestBody;
                    using (var reader = new StreamReader(context.Request.Body, Encoding.UTF8))
                    {
                        jsonRequestBody = reader.ReadToEnd();
                    }

                    var providerState = JsonConvert.DeserializeObject<ProviderState>(jsonRequestBody);

                    //A null or empty provider state key must be handled
                    if (providerState != null &&
                        !IsNullOrEmpty(providerState.State) &&
                        providerState.Consumer == ConsumerName)
                    {
                        _providerStates[providerState.State].Invoke();
                    }

                    await context.Response.WriteAsync(Empty);
                }
            }
            else
            {
                await m_next.Invoke(environment);
            }
        }
    }
}